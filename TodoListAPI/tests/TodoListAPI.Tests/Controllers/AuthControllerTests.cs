using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TodoListAPI.Controllers;
using TodoListAPI.Models.DTOs;
using TodoListAPI.Services.Interfaces;
using Xunit;

namespace TodoListAPI.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_authServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@test.com",
            Password = "Password123"
        };

        var loginResponse = new LoginResponseDto
        {
            Token = "fake-jwt-token",
            Email = "test@test.com",
            FullName = "Test User",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequestDto>()))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LoginResponseDto>().Subject;
        response.Token.Should().Be("fake-jwt-token");
        response.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@test.com",
            Password = "WrongPassword"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequestDto>()))
            .ReturnsAsync((LoginResponseDto?)null);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "invalid-email",
            Password = "123" // Muy corta
        };

        _controller.ModelState.AddModelError("Email", "El formato del email no es válido");
        _controller.ModelState.AddModelError("Password", "La contraseña debe tener al menos 6 caracteres");

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_ShouldCallAuthService_ExactlyOnce()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "test@test.com",
            Password = "Password123"
        };

        var loginResponse = new LoginResponseDto
        {
            Token = "fake-jwt-token",
            Email = "test@test.com",
            FullName = "Test User",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequestDto>()))
            .ReturnsAsync(loginResponse);

        // Act
        await _controller.Login(loginRequest);

        // Assert
        _authServiceMock.Verify(x => x.LoginAsync(It.IsAny<LoginRequestDto>()), Times.Once);
    }
}