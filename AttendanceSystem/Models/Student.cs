using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string SchoolId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string Grade { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<AttendanceLog> AttendanceLogs { get; set; } = new List<AttendanceLog>();
    }
}
