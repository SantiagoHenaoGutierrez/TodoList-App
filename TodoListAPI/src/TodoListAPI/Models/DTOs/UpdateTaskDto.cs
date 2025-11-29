using System.ComponentModel.DataAnnotations;

namespace TodoListAPI.Models.DTOs
{
    public class UpdateTaskDto
    {
        [Required(ErrorMessage = "El título es requerido")]
        [MaxLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Description { get; set; }

        public bool IsCompleted { get; set; }
    }
}
