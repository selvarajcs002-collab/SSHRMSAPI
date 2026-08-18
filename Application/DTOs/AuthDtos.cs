using System;

namespace EMS.Application.DTOs
{
    public class LoginRequestDto
    {
        public string Email_Id { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Email_Id { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
    }
}
