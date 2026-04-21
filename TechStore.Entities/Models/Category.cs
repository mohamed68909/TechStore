using System.ComponentModel.DataAnnotations;

namespace TechStore.Entities.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
    }
}
