using TodoListAPI.Models.DTOs;

namespace TodoListAPI.Services.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllTasksAsync(int userId, string? filter = null);
    Task<TaskDto?> GetTaskByIdAsync(int taskId, int userId);
    Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, int userId);
    Task<TaskDto?> UpdateTaskAsync(int taskId, UpdateTaskDto updateTaskDto, int userId);
    Task<bool> DeleteTaskAsync(int taskId, int userId);
    Task<TaskDto?> ToggleTaskStatusAsync(int taskId, int userId);
    Task<Dictionary<string, int>> GetTaskStatisticsAsync(int userId);
}