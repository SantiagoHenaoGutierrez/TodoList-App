using Microsoft.EntityFrameworkCore;
using TodoListAPI.Data;
using TodoListAPI.Models.DTOs;
using TodoListAPI.Models.Entities;
using TodoListAPI.Services.Interfaces;

namespace TodoListAPI.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;

    public TaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync(int userId, string? filter = null)
    {
        var query = _context.Tasks.Where(t => t.UserId == userId);

        // Filtrar por estado
        if (!string.IsNullOrEmpty(filter))
        {
            query = filter.ToLower() switch
            {
                "completed" => query.Where(t => t.IsCompleted),
                "pending" => query.Where(t => !t.IsCompleted),
                _ => query
            };
        }

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<TaskDto?> GetTaskByIdAsync(int taskId, int userId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

        return task == null ? null : MapToDto(task);
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, int userId)
    {
        var task = new TodoTask
        {
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            UserId = userId,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task<TaskDto?> UpdateTaskAsync(int taskId, UpdateTaskDto updateTaskDto, int userId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

        if (task == null)
            return null;

        task.Title = updateTaskDto.Title;
        task.Description = updateTaskDto.Description;

        // Si se marca como completada
        if (updateTaskDto.IsCompleted && !task.IsCompleted)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        // Si se desmarca como completada
        else if (!updateTaskDto.IsCompleted && task.IsCompleted)
        {
            task.CompletedAt = null;
        }

        task.IsCompleted = updateTaskDto.IsCompleted;

        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task<bool> DeleteTaskAsync(int taskId, int userId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

        if (task == null)
            return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<TaskDto?> ToggleTaskStatusAsync(int taskId, int userId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

        if (task == null)
            return null;

        task.IsCompleted = !task.IsCompleted;
        task.CompletedAt = task.IsCompleted ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task<Dictionary<string, int>> GetTaskStatisticsAsync(int userId)
    {
        var tasks = await _context.Tasks
            .Where(t => t.UserId == userId)
            .ToListAsync();

        return new Dictionary<string, int>
        {
            { "total", tasks.Count },
            { "completed", tasks.Count(t => t.IsCompleted) },
            { "pending", tasks.Count(t => !t.IsCompleted) }
        };
    }

    private static TaskDto MapToDto(TodoTask task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            CompletedAt = task.CompletedAt
        };
    }
}