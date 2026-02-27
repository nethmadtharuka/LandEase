using LandEase.Application.DTOs.Auth;

namespace LandEase.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);

    Task<UserProfileDto> GetProfileAsync(int userId);
}