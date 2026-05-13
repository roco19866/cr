using System.ComponentModel.DataAnnotations;

namespace CertificateSystem.Models
{
    public class DesignTemplate
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        // JSON string containing all the coordinates, fonts, and colors
        public string ConfigJson { get; set; }
        
        // Base64 or path to the template image
        public string? TemplateImage { get; set; }
    }
}
