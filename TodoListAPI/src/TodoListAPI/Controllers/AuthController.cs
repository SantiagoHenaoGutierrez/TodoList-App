using Microsoft.AspNetCore.Mvc;
using TodoListAPI.Models.DTOs;
using TodoListAPI.Services.Interfaces;

namespace TodoListAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Autentica un usuario y devuelve un token JWT
    /// </summary>
    /// <param name="loginRequest">Credenciales del usuario</param>
    /// <returns>Token JWT y datos del usuario</returns>
    /// <response code="200">Login exitoso</response>
    /// <response code="400">Datos inválidos</response>
    /// <response code="401">Credenciales incorrectas</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(loginRequest);

        if (result == null)
        {
            _logger.LogWarning("Intento de login fallido para el email: {Email}", loginRequest.Email);
            return Unauthorized(new { message = "Email o contraseña incorrectos" });
        }

        _logger.LogInformation("Login exitoso para el usuario: {Email}", loginRequest.Email);
        return Ok(result);
    }
}