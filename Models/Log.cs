using System.ComponentModel.DataAnnotations;

namespace FougeraClub1.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required]
        public string Action { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}