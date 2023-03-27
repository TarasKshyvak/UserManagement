using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserManagement.Authorization;
using UserManagement.Exceptions;
using UserManagement.Helpers;
using UserManagement.Interfaces;
using UserManagement.Models;
using UserManagement.Entities;

namespace UserManagement.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IJwtUtils _jwtUtils;
        private readonly AppSettings _appSettings;

        public UserService(AppDbContext context, IMapper mapper, IJwtUtils jwtUtils, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _mapper = mapper;
            _jwtUtils = jwtUtils;
            _appSettings = appSettings.Value;
        }

        public async Task AddAsync(UserModel userModel)
        {
            ArgumentNullException.ThrowIfNull(userModel);

            var checkUser = await GetByUsernameAsync(userModel.Username);

            if (checkUser != null)
                throw new AppException($"Username '{checkUser.Username}' is not available");

            var roleId = (await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User"))!.Id;

            userModel.RoleId = roleId;

            var user = _mapper.Map<User>(userModel);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userModel.Password);

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest authRequestModel, string ipAddress)
        {
            var user = await GetByUsernameAsync(authRequestModel.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(authRequestModel.Password, user.PasswordHash))
                throw new AppException("Username or password is incorrect");

            var jwtToken = _jwtUtils.GenerateJwtToken(user);
            var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            user.RefreshTokens.Add(refreshToken);

            removeOldRefreshTokens(user);

            _context.Update(user);
            await _context.SaveChangesAsync();

            return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
        }

        public AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            var user = getUserByRefreshToken(token);
            RefreshToken refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (refreshToken.IsRevoked)
            {
                // revoke all descendant tokens in case this token has been compromised
                revokeDescendantRefreshTokens(refreshToken, user, ipAddress, 
                    $"Attempted reuse of revoked ancestor token: {token}");
                _context.Update(user);
                _context.SaveChanges();
            }

            if (!refreshToken.IsActive)
                throw new AppException("Invalid token");

            // replace old refresh token with a new one (rotate token)
            var newRefreshToken = rotateRefreshToken(refreshToken, ipAddress);
            user.RefreshTokens.Add(newRefreshToken);

            removeOldRefreshTokens(user);

            _context.Update(user);
            _context.SaveChanges();

            var jwtToken = _jwtUtils.GenerateJwtToken(user);

            return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
        }

        public void RevokeToken(string token, string ipAddress)
        {
            var user = getUserByRefreshToken(token);
            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive)
                throw new AppException("Invalid token");

            revokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
            _context.Update(user);
            _context.SaveChanges();
        }

        public async Task<IEnumerable<UserModel>> GetAllAsync()
        {
            var users = await _context.Users.ToListAsync();
            return _mapper.Map<IEnumerable<UserModel>>(users);
        }

        public async Task<UserModel> GetByIdAsync(Guid id)
        {
            var user = await GetUserByIdAsync(id);

            return _mapper.Map<UserModel>(user);
        }

        #region Helper methods

        private User getUserByRefreshToken(string token)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
                throw new AppException("Invalid token");

            return user;
        }

        private RefreshToken rotateRefreshToken(RefreshToken refreshToken, string ipAddress)
        {
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            revokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
            return newRefreshToken;
        }

        private void removeOldRefreshTokens(User user)
        {
            // remove old inactive refresh tokens from user based on TTL in app settings
            user.RefreshTokens.RemoveAll(x =>
                !x.IsActive &&
                x.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
        }

        private void revokeDescendantRefreshTokens(RefreshToken refreshToken, User user, string ipAddress, string reason)
        {
            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
                if (childToken.IsActive)
                    revokeRefreshToken(childToken, ipAddress, reason);
                else
                    revokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
            }
        }

        private void revokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
            token.ReplacedByToken = replacedByToken;
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Gender)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new NotFoundException(id);

            return user;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Gender)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        #endregion
    }
}
