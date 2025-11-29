namespace TodoListAPI.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relación con tareas
        public ICollection<TodoTask> Tasks { get; set; } = new List<TodoTask>();
    }
}
