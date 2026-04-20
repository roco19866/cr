using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CertificateSystem.Models
{
    public class CertificateRequest
    {
        [Required]
        public IFormFile TemplateFile { get; set; }

        public IFormFile ExcelFile { get; set; }

        public string SelectedEmployeeIds { get; set; }

        public string AdditionalText { get; set; }
        
        public string DateText { get; set; }

        [Required]
        public string ExportType { get; set; } // png, jpg, pdf
        
        // Coordinates for drawing (X, Y)
        public int NameX { get; set; } = 400;
        public int NameY { get; set; } = 300;
        
        public int CivilIdX { get; set; } = 400;
        public int CivilIdY { get; set; } = 400;

        public int DateX { get; set; } = 400;
        public int DateY { get; set; } = 500;

        public int AdditionalTextX { get; set; } = 400;
        public int AdditionalTextY { get; set; } = 200;

        // Typography Config - Name
        public string NameFontFamily { get; set; } = "Arial";
        public int NameFontSize { get; set; } = 48;
        public string NameFontColor { get; set; } = "#000000";
        public string NameTextAlign { get; set; } = "Center";

        // Typography Config - Civil ID
        public string CivilIdFontFamily { get; set; } = "Arial";
        public int CivilIdFontSize { get; set; } = 32;
        public string CivilIdFontColor { get; set; } = "#000000";
        public string CivilIdTextAlign { get; set; } = "Center";

        // Typography Config - Date
        public string DateFontFamily { get; set; } = "Arial";
        public int DateFontSize { get; set; } = 32;
        public string DateFontColor { get; set; } = "#000000";
        public string DateTextAlign { get; set; } = "Center";

        // Typography Config - Additional Text
        public string AdditionalTextFontFamily { get; set; } = "Arial";
        public int AdditionalTextFontSize { get; set; } = 32;
        public string AdditionalTextFontColor { get; set; } = "#000000";
        public string AdditionalTextAlign { get; set; } = "Center";
    }
}
