using System;
using System.Threading.Tasks;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;

namespace EMS.Application.Services
{
    public class AuthService : IAuthService
    {
        public Task<LoginResponseDto> Login(LoginRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email_Id) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return Task.FromResult(new LoginResponseDto
                {
                    Status = 0,
                    Message = "Email and password are required."
                });
            }

            // Simple developer mock login: accepts admin credentials or dummy
            bool isValid = (dto.Email_Id.Equals("admin@ems.local", StringComparison.OrdinalIgnoreCase) && dto.Password == "admin123")
                           || dto.Password == "password123";

            if (!isValid)
            {
                return Task.FromResult(new LoginResponseDto
                {
                    Status = 0,
                    Message = "Invalid credentials. Use admin@ems.local / admin123 or password123."
                });
            }

            // Create a dummy token for testing (jwt implementation can be added in API layer easily)
            string dummyToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiJjM2EyYjEyMy0xMjM0LTU2NzgtOTAxMi0zNGU1NmY3OGE5MGIiLCJlbWFpbCI6ImFkbWluQGVtcy5sb2NhbCIsImV4cCI6MTkwOTgwMDAwMH0.mockSignatureHere";

            return Task.FromResult(new LoginResponseDto
            {
                Token = dummyToken,
                Status = 1,
                Message = "Login successful.",
                UserId = Guid.Parse("c3a2b123-1234-5678-9012-34e56f78a90b"),
                Email_Id = dto.Email_Id,
                Expiry = DateTime.UtcNow.AddHours(2)
            });
        }

        public Task<bool> Logout(string token)
        {
            // Revocation list can be added later
            return Task.FromResult(true);
        }
    }
}
