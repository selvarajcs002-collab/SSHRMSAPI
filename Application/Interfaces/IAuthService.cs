using System.Threading.Tasks;
using EMS.Application.DTOs;

namespace EMS.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> Login(LoginRequestDto dto);
        Task<bool> Logout(string token);
    }
}
