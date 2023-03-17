using UM.BLL.Models;
using UM.DAL.Entities;

namespace UM.BLL.Interfaces
{
    public interface IUserService
    {
        Task AddAsync(UserModel userModel);
        Task<AuthenticateResponse> Authenticate(AuthenticateRequest authRequestModel, string ipAddress);
        Task<IEnumerable<UserModel>> GetAllAsync();
        Task<UserModel> GetByIdAsync(Guid id);
        Task<User> GetUserByIdAsync(Guid id);
        AuthenticateResponse RefreshToken(string token, string ipAddress);
        void RevokeToken(string token, string ipAddress);
    }
}
