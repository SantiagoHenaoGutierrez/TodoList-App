using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TodoListAPI.Controllers;
using TodoListAPI.Models.DTOs;
using TodoListAPI.Services.Interfaces;
using Xunit;

namespace TodoListAPI.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<ILogger<TasksController>> _loggerMock;
    private readonly TasksController _controller;
    private const int TestUserId = 1;

    public TasksControllerTests()
    {
        _taskServiceMock = new Mock<ITaskService>();
        _loggerMock = new Mock<ILogger<TasksController>>();
        _controller = new TasksController(_taskServiceMock.Object, _loggerMock.Object);

        // Configurar el contexto del usuario autenticado
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString()),
            new Claim(ClaimTypes.Email, "test@test.com")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetAllTasks_ShouldReturnOk_WithListOfTasks()
    {
        // Arrange
        var tasks = new List<TaskDto>
        {
            new() { Id = 1, Title = "Tarea 1", IsCompleted = false },
            new() { Id = 2, Title = "Tarea 2", IsCompleted = true }
        };

        _taskServiceMock
            .Setup(x => x.GetAllTasksAsync(TestUserId, null))
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetAllTasks(null);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskDto>>().Subject;
        returnedTasks.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllTasks_ShouldApplyFilter_WhenFilterProvided()
    {
        // Arrange
        var completedTasks = new List<TaskDto>
        {
            new() { Id = 1, Title = "Tarea 1", IsCompleted = true }
        };

        _taskServiceMock
            .Setup(x => x.GetAllTasksAsync(TestUserId, "completed"))
            .ReturnsAsync(completedTasks);

        // Act
        var result = await _controller.GetAllTasks("completed");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskDto>>().Subject;
        returnedTasks.Should().HaveCount(1);
        returnedTasks.First().IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetTaskById_ShouldReturnOk_WhenTaskExists()
    {
        // Arrange
        var task = new TaskDto { Id = 1, Title = "Tarea de prueba" };

        _taskServiceMock
            .Setup(x => x.GetTaskByIdAsync(1, TestUserId))
            .ReturnsAsync(task);

        // Act
        var result = await _controller.GetTaskById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTask = okResult.Value.Should().BeOfType<TaskDto>().Subject;
        returnedTask.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetTaskById_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        _taskServiceMock
            .Setup(x => x.GetTaskByIdAsync(999, TestUserId))
            .ReturnsAsync((TaskDto?)null);

        // Act
        var result = await _controller.GetTaskById(999);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateTask_ShouldReturnCreated_WhenTaskIsValid()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "Nueva tarea",
            Description = "Descripción"
        };

        var createdTask = new TaskDto
        {
            Id = 1,
            Title = createDto.Title,
            Description = createDto.Description
        };

        _taskServiceMock
            .Setup(x => x.CreateTaskAsync(It.IsAny<CreateTaskDto>(), TestUserId))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _controller.CreateTask(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var returnedTask = createdResult.Value.Should().BeOfType<TaskDto>().Subject;
        returnedTask.Title.Should().Be("Nueva tarea");
    }

    [Fact]
    public async Task CreateTask_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var createDto = new CreateTaskDto { Title = "" }; // Título vacío

        _controller.ModelState.AddModelError("Title", "El título es requerido");

        // Act
        var result = await _controller.CreateTask(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateTask_ShouldReturnOk_WhenTaskIsUpdated()
    {
        // Arrange
        var updateDto = new UpdateTaskDto
        {
            Title = "Tarea actualizada",
            IsCompleted = true
        };

        var updatedTask = new TaskDto
        {
            Id = 1,
            Title = updateDto.Title,
            IsCompleted = updateDto.IsCompleted
        };

        _taskServiceMock
            .Setup(x => x.UpdateTaskAsync(1, It.IsAny<UpdateTaskDto>(), TestUserId))
            .ReturnsAsync(updatedTask);

        // Act
        var result = await _controller.UpdateTask(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTask = okResult.Value.Should().BeOfType<TaskDto>().Subject;
        returnedTask.Title.Should().Be("Tarea actualizada");
    }

    [Fact]
    public async Task UpdateTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateTaskDto { Title = "Tarea", IsCompleted = true };

        _taskServiceMock
            .Setup(x => x.UpdateTaskAsync(999, It.IsAny<UpdateTaskDto>(), TestUserId))
            .ReturnsAsync((TaskDto?)null);

        // Act
        var result = await _controller.UpdateTask(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteTask_ShouldReturnNoContent_WhenTaskIsDeleted()
    {
        // Arrange
        _taskServiceMock
            .Setup(x => x.DeleteTaskAsync(1, TestUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTask(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        _taskServiceMock
            .Setup(x => x.DeleteTaskAsync(999, TestUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTask(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ToggleTaskStatus_ShouldReturnOk_WhenTaskIsToggled()
    {
        // Arrange
        var toggledTask = new TaskDto
        {
            Id = 1,
            Title = "Tarea",
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow
        };

        _taskServiceMock
            .Setup(x => x.ToggleTaskStatusAsync(1, TestUserId))
            .ReturnsAsync(toggledTask);

        // Act
        var result = await _controller.ToggleTaskStatus(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTask = okResult.Value.Should().BeOfType<TaskDto>().Subject;
        returnedTask.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatistics_ShouldReturnOk_WithStatistics()
    {
        // Arrange
        var statistics = new Dictionary<string, int>
        {
            { "total", 10 },
            { "completed", 6 },
            { "pending", 4 }
        };

        _taskServiceMock
            .Setup(x => x.GetTaskStatisticsAsync(TestUserId))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var stats = okResult.Value.Should().BeAssignableTo<Dictionary<string, int>>().Subject;
        stats["total"].Should().Be(10);
        stats["completed"].Should().Be(6);
        stats["pending"].Should().Be(4);
    }
}