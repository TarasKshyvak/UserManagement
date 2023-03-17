using UM.DAL.Entities;

namespace UM.BLL.Authorization
{
    public interface IJwtUtils
    {
        public string GenerateJwtToken(User user);
        /// <summary>
        /// Returns user id from JWT token if validation successful, otherwise null
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Guid? ValidateJwtToken(string token);
        public RefreshToken GenerateRefreshToken(string ipAddress);
    }
}
