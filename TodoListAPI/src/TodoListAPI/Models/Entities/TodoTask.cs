namespace TodoListAPI.Models.Entities
{
    public class TodoTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Relación con usuario
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
