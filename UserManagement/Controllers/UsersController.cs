using UM.BLL.Interfaces;
using UM.BLL.Models;
using Microsoft.AspNetCore.Mvc;
using UM.BLL.Authorization;

namespace UserManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Add([FromBody] UserModel userModel)
        {
            await _userService.AddAsync(userModel);
            return Ok(userModel);
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest authRequestModel)
        {
            var authResponse = await _userService.Authenticate(authRequestModel, ipAddress());
            setTokenCookie(authResponse.RefreshToken);
            return Ok(authResponse);
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<UserModel>> GetById(Guid id)
        {
            return await _userService.GetByIdAsync(id);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = _userService.RefreshToken(refreshToken, ipAddress());
            setTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        [HttpPost("revoke-token")]
        public IActionResult RevokeToken(RevokeTokenRequest model)
        {
            // accept refresh token in request body or cookie
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            _userService.RevokeToken(token, ipAddress());
            return Ok(new { message = "Token revoked" });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetAll()
        {
            return Ok(await _userService.GetAllAsync());
        }


        [HttpGet("{id}/refresh-tokens")]
        public async Task<IActionResult> GetRefreshTokens(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user.RefreshTokens);
        }

        #region Helper methods

        private void setTokenCookie(string token)
        {
            // append cookie with refresh token to the http response
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string ipAddress()
        {
            // get source ip address for the current request
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }

        #endregion
    }
}