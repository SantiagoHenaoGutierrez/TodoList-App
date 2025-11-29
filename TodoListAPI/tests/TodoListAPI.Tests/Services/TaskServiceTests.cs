using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TodoListAPI.Data;
using TodoListAPI.Models.DTOs;
using TodoListAPI.Models.Entities;
using TodoListAPI.Services;
using Xunit;

namespace TodoListAPI.Tests.Services;

public class TaskServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TaskService _taskService;
    private readonly User _testUser;

    public TaskServiceTests()
    {
        // Configurar DbContext en memoria
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Crear usuario de prueba
        _testUser = new User
        {
            Id = 1,
            Email = "test@test.com",
            PasswordHash = "hash",
            FullName = "Test User"
        };

        _context.Users.Add(_testUser);
        _context.SaveChanges();

        _taskService = new TaskService(_context);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldCreateTask_WhenValidData()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "Nueva tarea de prueba",
            Description = "Descripción de prueba"
        };

        // Act
        var result = await _taskService.CreateTaskAsync(createDto, _testUser.Id);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(createDto.Title);
        result.Description.Should().Be(createDto.Description);
        result.IsCompleted.Should().BeFalse();

        var taskInDb = await _context.Tasks.FindAsync(result.Id);
        taskInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllTasksAsync_ShouldReturnUserTasks_WhenFilterIsNull()
    {
        // Arrange
        var tasks = new List<TodoTask>
        {
            new() { Title = "Tarea 1", UserId = _testUser.Id, IsCompleted = false },
            new() { Title = "Tarea 2", UserId = _testUser.Id, IsCompleted = true },
            new() { Title = "Tarea 3", UserId = _testUser.Id, IsCompleted = false }
        };

        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetAllTasksAsync(_testUser.Id);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllTasksAsync_ShouldReturnCompletedTasks_WhenFilterIsCompleted()
    {
        // Arrange
        var tasks = new List<TodoTask>
        {
            new() { Title = "Tarea 1", UserId = _testUser.Id, IsCompleted = false },
            new() { Title = "Tarea 2", UserId = _testUser.Id, IsCompleted = true },
            new() { Title = "Tarea 3", UserId = _testUser.Id, IsCompleted = true }
        };

        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetAllTasksAsync(_testUser.Id, "completed");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.IsCompleted);
    }

    [Fact]
    public async Task GetAllTasksAsync_ShouldReturnPendingTasks_WhenFilterIsPending()
    {
        // Arrange
        var tasks = new List<TodoTask>
        {
            new() { Title = "Tarea 1", UserId = _testUser.Id, IsCompleted = false },
            new() { Title = "Tarea 2", UserId = _testUser.Id, IsCompleted = true },
            new() { Title = "Tarea 3", UserId = _testUser.Id, IsCompleted = false }
        };

        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetAllTasksAsync(_testUser.Id, "pending");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => !t.IsCompleted);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ShouldReturnTask_WhenTaskExists()
    {
        // Arrange
        var task = new TodoTask
        {
            Title = "Tarea de prueba",
            UserId = _testUser.Id
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetTaskByIdAsync(task.Id, _testUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
        result.Title.Should().Be(task.Title);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ShouldReturnNull_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _taskService.GetTaskByIdAsync(999, _testUser.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTaskByIdAsync_ShouldReturnNull_WhenTaskBelongsToAnotherUser()
    {
        // Arrange
        var anotherUser = new User
        {
            Email = "otro@test.com",
            PasswordHash = "hash",
            FullName = "Otro Usuario"
        };
        _context.Users.Add(anotherUser);

        var task = new TodoTask
        {
            Title = "Tarea de otro usuario",
            UserId = anotherUser.Id
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetTaskByIdAsync(task.Id, _testUser.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateTaskAsync_ShouldUpdateTask_WhenTaskExists()
    {
        // Arrange
        var task = new TodoTask
        {
            Title = "Tarea original",
            Description = "Descripción original",
            UserId = _testUser.Id,
            IsCompleted = false
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateTaskDto
        {
            Title = "Tarea actualizada",
            Description = "Descripción actualizada",
            IsCompleted = true
        };

        // Act
        var result = await _taskService.UpdateTaskAsync(task.Id, updateDto, _testUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be(updateDto.Title);
        result.Description.Should().Be(updateDto.Description);
        result.IsCompleted.Should().BeTrue();
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateTaskAsync_ShouldReturnNull_WhenTaskDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateTaskDto
        {
            Title = "Tarea actualizada",
            IsCompleted = true
        };

        // Act
        var result = await _taskService.UpdateTaskAsync(999, updateDto, _testUser.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldReturnTrue_WhenTaskExists()
    {
        // Arrange
        var task = new TodoTask
        {
            Title = "Tarea a eliminar",
            UserId = _testUser.Id
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.DeleteTaskAsync(task.Id, _testUser.Id);

        // Assert
        result.Should().BeTrue();
        var deletedTask = await _context.Tasks.FindAsync(task.Id);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldReturnFalse_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _taskService.DeleteTaskAsync(999, _testUser.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetTaskStatisticsAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        var tasks = new List<TodoTask>
        {
            new() { Title = "Tarea 1", UserId = _testUser.Id, IsCompleted = false },
            new() { Title = "Tarea 2", UserId = _testUser.Id, IsCompleted = true },
            new() { Title = "Tarea 3", UserId = _testUser.Id, IsCompleted = true },
            new() { Title = "Tarea 4", UserId = _testUser.Id, IsCompleted = false }
        };

        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _taskService.GetTaskStatisticsAsync(_testUser.Id);

        // Assert
        result["total"].Should().Be(4);
        result["completed"].Should().Be(2);
        result["pending"].Should().Be(2);
    }

    [Fact]
    public async Task GetTaskStatisticsAsync_ShouldReturnZeros_WhenNoTasksExist()
    {
        // Act
        var result = await _taskService.GetTaskStatisticsAsync(_testUser.Id);

        // Assert
        result["total"].Should().Be(0);
        result["completed"].Should().Be(0);
        result["pending"].Should().Be(0);
    }

    [Fact]
    public async Task ToggleTaskStatusAsync_ShouldToggleStatus_WhenTaskExists()
    {
        // Arrange
        var task = new TodoTask
        {
            Title = "Tarea",
            UserId = _testUser.Id,
            IsCompleted = false
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act - Primera vez: marcar como completada
        var result = await _taskService.ToggleTaskStatusAsync(task.Id, _testUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.IsCompleted.Should().BeTrue();
        result.CompletedAt.Should().NotBeNull();

        // Act - Segunda vez: desmarcar como completada
        var result2 = await _taskService.ToggleTaskStatusAsync(task.Id, _testUser.Id);

        // Assert
        result2!.IsCompleted.Should().BeFalse();
        result2.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task ToggleTaskStatusAsync_ShouldReturnNull_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _taskService.ToggleTaskStatusAsync(999, _testUser.Id);

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}