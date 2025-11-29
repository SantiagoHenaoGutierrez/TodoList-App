using TodoListAPI.Models.DTOs;

namespace TodoListAPI.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest);
}