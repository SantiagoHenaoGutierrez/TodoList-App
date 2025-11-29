using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListAPI.Models.DTOs;
using TodoListAPI.Services.Interfaces;

namespace TodoListAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    /// <summary>
    /// Obtiene todas las tareas del usuario autenticado
    /// </summary>
    /// <param name="filter">Filtro opcional: 'completed', 'pending' o vacío para todas</param>
    /// <returns>Lista de tareas</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTasks([FromQuery] string? filter = null)
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetAllTasksAsync(userId, filter);
        return Ok(tasks);
    }

    /// <summary>
    /// Obtiene una tarea específica por ID
    /// </summary>
    /// <param name="id">ID de la tarea</param>
    /// <returns>Datos de la tarea</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(int id)
    {
        var userId = GetUserId();
        var task = await _taskService.GetTaskByIdAsync(id, userId);

        if (task == null)
            return NotFound(new { message = "Tarea no encontrada" });

        return Ok(task);
    }

    /// <summary>
    /// Crea una nueva tarea
    /// </summary>
    /// <param name="createTaskDto">Datos de la nueva tarea</param>
    /// <returns>Tarea creada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        var task = await _taskService.CreateTaskAsync(createTaskDto, userId);

        _logger.LogInformation("Tarea creada: {TaskId} por usuario: {UserId}", task.Id, userId);

        return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
    }

    /// <summary>
    /// Actualiza una tarea existente
    /// </summary>
    /// <param name="id">ID de la tarea</param>
    /// <param name="updateTaskDto">Datos actualizados</param>
    /// <returns>Tarea actualizada</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetUserId();
        var task = await _taskService.UpdateTaskAsync(id, updateTaskDto, userId);

        if (task == null)
            return NotFound(new { message = "Tarea no encontrada" });

        _logger.LogInformation("Tarea actualizada: {TaskId}", id);

        return Ok(task);
    }

    /// <summary>
    /// Elimina una tarea
    /// </summary>
    /// <param name="id">ID de la tarea</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var userId = GetUserId();
        var result = await _taskService.DeleteTaskAsync(id, userId);

        if (!result)
            return NotFound(new { message = "Tarea no encontrada" });

        _logger.LogInformation("Tarea eliminada: {TaskId}", id);

        return NoContent();
    }

    /// <summary>
    /// Alterna el estado de completado de una tarea
    /// </summary>
    /// <param name="id">ID de la tarea</param>
    /// <returns>Tarea actualizada</returns>
    [HttpPatch("{id}/toggle")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleTaskStatus(int id)
    {
        var userId = GetUserId();
        var task = await _taskService.ToggleTaskStatusAsync(id, userId);

        if (task == null)
            return NotFound(new { message = "Tarea no encontrada" });

        _logger.LogInformation("Estado de tarea alternado: {TaskId}", id);

        return Ok(task);
    }

    /// <summary>
    /// Obtiene estadísticas de las tareas del usuario
    /// </summary>
    /// <returns>Estadísticas (total, completadas, pendientes)</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics()
    {
        var userId = GetUserId();
        var stats = await _taskService.GetTaskStatisticsAsync(userId);
        return Ok(stats);
    }
}