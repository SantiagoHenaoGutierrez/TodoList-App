using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TodoListAPI.Configuration;
using TodoListAPI.Data;
using TodoListAPI.Models.DTOs;
using TodoListAPI.Models.Entities;
using TodoListAPI.Services;
using Xunit;

namespace TodoListAPI.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _authService;
    private readonly JwtSettings _jwtSettings;

    public AuthServiceTests()
    {
        // Configurar DbContext en memoria
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Configurar JWT Settings
        _jwtSettings = new JwtSettings
        {
            SecretKey = "MiSuperClaveSecretaDeAlMenos32CaracteresParaJWT2024!",
            Issuer = "TodoListAPI",
            Audience = "TodoListClient",
            ExpirationMinutes = 60
        };

        var jwtOptions = Options.Create(_jwtSettings);
        _authService = new AuthService(_context, jwtOptions);

        // Crear usuario de prueba
        var user = new User
        {
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
            FullName = "Test User"
        };

        _context.Users.Add(user);
        _context.SaveChanges();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@test.com",
            Password = "Password123"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be("test@test.com");
        result.FullName.Should().Be("Test User");
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "noexiste@test.com",
            Password = "Password123"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@test.com",
            Password = "PasswordIncorrecta"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnValidJWT_WithCorrectClaims()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@test.com",
            Password = "Password123"
        };

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();

        // Verificar que el token tiene 3 partes (header.payload.signature)
        var tokenParts = result.Token.Split('.');
        tokenParts.Should().HaveCount(3);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}