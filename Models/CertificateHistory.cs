using System;
using System.ComponentModel.DataAnnotations;

namespace CertificateSystem.Models
{
    public class CertificateHistory
    {
        [Key]
        public int Id { get; set; }
        
        public string EmployeeName { get; set; }
        
        public string EmployeeEmail { get; set; }
        
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        
        public bool EmailSent { get; set; }
        
        public bool WhatsAppSent { get; set; }
        
        public string ExportType { get; set; } // png, jpg, pdf
    }
}
