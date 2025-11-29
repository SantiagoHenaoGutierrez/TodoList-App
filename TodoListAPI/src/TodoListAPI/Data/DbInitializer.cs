using TodoListAPI.Models.Entities;

namespace TodoListAPI.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        // Verificar si ya hay usuarios
        if (context.Users.Any())
        {
            return; // La BD ya fue inicializada
        }

        // Crear usuario de prueba
        var testUser = new User
        {
            Email = "admin@todolist.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
            FullName = "Administrador",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(testUser);
        context.SaveChanges();

        // Crear tareas de ejemplo
        var tasks = new List<TodoTask>
        {
            new TodoTask
            {
                Title = "Completar el backend de la API",
                Description = "Implementar todos los endpoints necesarios",
                IsCompleted = true,
                UserId = testUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                CompletedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TodoTask
            {
                Title = "Desarrollar el frontend en Angular",
                Description = "Crear componentes y servicios",
                IsCompleted = false,
                UserId = testUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new TodoTask
            {
                Title = "Escribir pruebas unitarias",
                Description = "Asegurar cobertura de código",
                IsCompleted = false,
                UserId = testUser.Id,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Tasks.AddRange(tasks);
        context.SaveChanges();
    }
}