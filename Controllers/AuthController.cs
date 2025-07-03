using System.Security.Claims;
using LoyaltyRewardsApi.DTOs;
using LoyaltyRewardsApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyRewardsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                var user = await _userService.Authenticate(loginRequest.Email, loginRequest.Password);
                var token = await _tokenService.GenerateJwtToken(user);
                
                var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
                var expiryMinutes = configuration?["JwtSettings:ExpiryMinutes"] ?? "1440";
                
                var loginResponse = new LoginResponseDto
                {
                    Token = token,
                    User = user,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(expiryMinutes))
                };

                return Ok(loginResponse);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid email or password.");
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequest)
        {
            try
            {
                var user = await _userService.Register(registerRequest);
                return Ok(new RegisterResponseDto { Message = "User registered successfully.", User = user });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordRequest)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdClaim);

            try
            {
                await _userService.ChangePassword(userId, changePasswordRequest.CurrentPassword, changePasswordRequest.NewPassword);
                return Ok("Password changed successfully.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdClaim);

            await _tokenService.RevokeToken(token, userId);
            return Ok("Logged out successfully.");
        }

        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var userId = int.Parse(userIdClaim);

            await _tokenService.RevokeAllUserTokens(userId);
            return Ok("Logged out from all sessions successfully.");
        }
    }
}
