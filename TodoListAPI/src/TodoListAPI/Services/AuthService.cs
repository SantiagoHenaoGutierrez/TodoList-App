using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TodoListAPI.Configuration;
using TodoListAPI.Data;
using TodoListAPI.Models.DTOs;
using TodoListAPI.Services.Interfaces;

namespace TodoListAPI.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(ApplicationDbContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest)
    {
        // Buscar usuario por email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

        if (user == null)
            return null;

        // Verificar contraseña
        if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            return null;

        // Generar token JWT
        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        return new LoginResponseDto
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            ExpiresAt = expiresAt
        };
    }

    private string GenerateJwtToken(Models.Entities.User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}