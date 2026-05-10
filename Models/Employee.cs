using System.ComponentModel.DataAnnotations;

namespace CertificateSystem.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? CivilId { get; set; }
        
        [MaxLength(20)]
        public string? Gender { get; set; } // "ذكر", "أنثى"
        
        [MaxLength(50)]
        public string? Role { get; set; } // "مدير", "رئيس قسم", "موظف", "سائق"

        [MaxLength(100)]
        public string? Department { get; set; } // "الادارة/القسم"

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }
    }
}
