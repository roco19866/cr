using System.ComponentModel.DataAnnotations;

namespace CertificateSystem.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        
        [MaxLength(50)]
        public string CivilId { get; set; }
        
        [MaxLength(20)]
        public string Gender { get; set; } // "ذكر", "أنثى"
        
        [MaxLength(50)]
        public string Role { get; set; } // "مدير", "رئيس قسم", "موظف", "سائق"

        [MaxLength(100)]
        public string Department { get; set; } // "الادارة/القسم"
    }
}
