using UserManagement.Authorization;
using UserManagement.Helpers;
using UserManagement.Interfaces;
using Microsoft.Extensions.Options;

namespace UserManagement.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context, IUserService userService, IJwtUtils jwtUtils)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtUtils.ValidateJwtToken(token!);
            if (userId != null)
            {
                context.Items["User"] = await userService.GetByIdAsync(userId.Value);
            }

            await _next(context);
        }
    }
}
