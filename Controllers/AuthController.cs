using System.Threading.Tasks;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers
{
    [ApiController]
    [Route("api/ems/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var response = await _authService.Login(dto);
            if (response.Status == 0)
            {
                return BadRequest(new ApiResponse<LoginResponseDto>(null, response.Message));
            }
            return Ok(new ApiResponse<LoginResponseDto>(response, "Login successful."));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            string token = string.Empty;
            string authHeader = Request.Headers["Authorization"]!;
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring(7);
            }

            var success = await _authService.Logout(token);
            return Ok(new ApiResponse<bool>(success, "Logout successful."));
        }
    }
}
