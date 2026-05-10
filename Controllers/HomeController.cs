using CertificateSystem.Models;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.IO.Compression;
using CertificateSystem.Services;

namespace CertificateSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;
        private readonly IWhatsAppService _whatsAppService;

        public HomeController(IWebHostEnvironment env, IEmailService emailService, IWhatsAppService whatsAppService)
        {
            _env = env;
            _emailService = emailService;
            _whatsAppService = whatsAppService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees(string search, string gender, string role, string department, [FromServices] AppDbContext db)
        {
            var query = db.Employees.AsQueryable();
            if (!string.IsNullOrEmpty(search)) query = query.Where(e => e.Name.Contains(search));
            if (!string.IsNullOrEmpty(gender)) query = query.Where(e => e.Gender == gender);
            if (!string.IsNullOrEmpty(role)) query = query.Where(e => e.Role == role);
            if (!string.IsNullOrEmpty(department)) query = query.Where(e => e.Department == department);
            
            var results = await query.ToListAsync();
            return Json(results);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateCertificates(CertificateRequest request, [FromServices] AppDbContext db)
        {
            if (request.TemplateFile == null || request.TemplateFile.Length == 0)
            {
                ModelState.AddModelError("", "الرجاء رفع قالب الشهادة");
                return View("Index");
            }

            if ((request.ExcelFile == null || request.ExcelFile.Length == 0) && string.IsNullOrEmpty(request.SelectedEmployeeIds))
            {
                ModelState.AddModelError("", "الرجاء رفع ملف إكسيل أو تحديد موظفين من القائمة");
                return View("Index");
            }

            var certificates = new List<(string FileName, byte[] Data)>();

            // 1. Read the Students
            var students = new List<(string Name, string CivilId, string Email, string Phone)>();
            
            if (request.ExcelFile != null && request.ExcelFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await request.ExcelFile.CopyToAsync(memoryStream);
                    using (var workbook = new XLWorkbook(memoryStream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header

                        foreach (var row in rows)
                        {
                            var name = row.Cell(1).Value.ToString();
                            var civilId = row.Cell(2).Value.ToString();
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                students.Add((name, civilId, row.Cell(3).Value.ToString(), row.Cell(4).Value.ToString()));
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(request.SelectedEmployeeIds))
            {
                var idStrings = request.SelectedEmployeeIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var ids = idStrings.Select(id => int.TryParse(id.Trim(), out int val) ? val : 0).Where(v => v > 0).ToList();
                var dbEmployees = await db.Employees.Where(e => ids.Contains(e.Id)).ToListAsync();
                foreach(var emp in dbEmployees) 
                {
                     students.Add((emp.Name, emp.CivilId ?? "", emp.Email ?? "", emp.Phone ?? ""));
                }
            }

            // Process fonts
            FontCollection collection = new FontCollection();
            string fontPath = Path.Combine(_env.WebRootPath, "fonts", "Cairo-Bold.ttf");
            FontFamily? cairoFamily = null;
            if (System.IO.File.Exists(fontPath))
            {
                try { cairoFamily = collection.Add(fontPath); } catch { }
            }
            
            // Helper to get font
            Font GetFont(string familyName, int baseSize, FontStyle style = FontStyle.Bold)
            {
                FontFamily f;
                bool found = false;
                if (!string.IsNullOrEmpty(familyName)) found = SystemFonts.TryGet(familyName, out f);
                else f = default;

                if (!found && !string.IsNullOrEmpty(familyName)) found = collection.TryGet(familyName, out f);
                
                if (!found)
                {
                    if (!SystemFonts.TryGet("Arial", out f) && !SystemFonts.TryGet("Segoe UI", out f))
                    {
                        if (cairoFamily.HasValue) f = cairoFamily.Value;
                        else f = SystemFonts.Families.FirstOrDefault();
                    }
                }
                return f.CreateFont(baseSize > 0 ? baseSize : 32, style);
            }

            // Helper to get color
            SixLabors.ImageSharp.Color GetColor(string hexColor)
            {
                try { return SixLabors.ImageSharp.Color.ParseHex(hexColor); } 
                catch { return SixLabors.ImageSharp.Color.Black; }
            }

            // Helper to get text alignment
            TextAlignment GetTextAlignment(string align)
            {
                return align switch
                {
                    "Right" => TextAlignment.Start,  // Start is Right in RTL
                    "Left" => TextAlignment.End,     // End is Left in RTL
                    _ => TextAlignment.Center
                };
            }

            // Read template to memory once
            using var templateMemoryStream = new MemoryStream();
            await request.TemplateFile.CopyToAsync(templateMemoryStream);
            var templateBytes = templateMemoryStream.ToArray();

            // 3. Generate Certificates
            int counter = 1;
            foreach (var student in students)
            {
                using (var image = SixLabors.ImageSharp.Image.Load(templateBytes))
                {
                    // Draw name
                    var fontName = GetFont(request.NameFontFamily, request.NameFontSize, FontStyle.Bold);
                    var colName = GetColor(request.NameFontColor);
                    var optName = new RichTextOptions(fontName) { HorizontalAlignment = HorizontalAlignment.Center, TextAlignment = GetTextAlignment(request.NameTextAlign), TextDirection = TextDirection.RightToLeft, VerticalAlignment = VerticalAlignment.Center, Origin = new PointF(request.NameX, request.NameY) };
                    image.Mutate(x => x.DrawText(optName, student.Name, colName));

                    // Draw Civil ID
                    if (!string.IsNullOrEmpty(student.CivilId))
                    {
                        var fontCivil = GetFont(request.CivilIdFontFamily, request.CivilIdFontSize, FontStyle.Regular);
                        var colCivil = GetColor(request.CivilIdFontColor);
                        var optCivil = new RichTextOptions(fontCivil) { HorizontalAlignment = HorizontalAlignment.Center, TextAlignment = GetTextAlignment(request.CivilIdTextAlign), TextDirection = TextDirection.RightToLeft, VerticalAlignment = VerticalAlignment.Center, Origin = new PointF(request.CivilIdX, request.CivilIdY) };
                        image.Mutate(x => x.DrawText(optCivil, student.CivilId, colCivil));
                    }

                    // Draw Additional Text
                    if (!string.IsNullOrEmpty(request.AdditionalText))
                    {
                        var fontAdd = GetFont(request.AdditionalTextFontFamily, request.AdditionalTextFontSize, FontStyle.Regular);
                        var colAdd = GetColor(request.AdditionalTextFontColor);
                        var optAdd = new RichTextOptions(fontAdd) { HorizontalAlignment = HorizontalAlignment.Center, TextAlignment = GetTextAlignment(request.AdditionalTextAlign), TextDirection = TextDirection.RightToLeft, VerticalAlignment = VerticalAlignment.Center, Origin = new PointF(request.AdditionalTextX, request.AdditionalTextY) };
                        image.Mutate(x => x.DrawText(optAdd, request.AdditionalText, colAdd));
                    }

                    // Draw Date
                    if (!string.IsNullOrEmpty(request.DateText))
                    {
                        var fontDate = GetFont(request.DateFontFamily, request.DateFontSize, FontStyle.Regular);
                        var colDate = GetColor(request.DateFontColor);
                        var optDate = new RichTextOptions(fontDate) { HorizontalAlignment = HorizontalAlignment.Center, TextAlignment = GetTextAlignment(request.DateTextAlign), TextDirection = TextDirection.RightToLeft, VerticalAlignment = VerticalAlignment.Center, Origin = new PointF(request.DateX, request.DateY) };
                        image.Mutate(x => x.DrawText(optDate, request.DateText, colDate));
                    }

                    // Export Logic
                    string fileName = $"Certificate_{counter}_{student.Name.Replace(" ", "_")}";
                    byte[] finalData = null;

                    if (request.ExportType == "pdf")
                    {
                        // Convert Image to PDF
                        using (var imgStream = new MemoryStream())
                        {
                            image.SaveAsPng(imgStream);
                            imgStream.Position = 0;

                            using (PdfDocument pdf = new PdfDocument())
                            {
                                PdfPage page = pdf.AddPage();
                                // Create XImage from stream
                                using (XImage ximg = XImage.FromStream(() => new MemoryStream(imgStream.ToArray())))
                                {
                                    // Adjust page size to match image aspect ratio if needed, for simplicity we use A4 or Image size
                                    page.Width = ximg.PointWidth;
                                    page.Height = ximg.PointHeight;

                                    using (XGraphics gfx = XGraphics.FromPdfPage(page))
                                    {
                                        gfx.DrawImage(ximg, 0, 0, page.Width, page.Height);
                                    }
                                }
                                
                                using (var pdfStream = new MemoryStream())
                                {
                                    pdf.Save(pdfStream, false);
                                    finalData = pdfStream.ToArray();
                                }
                            }
                        }
                        fileName += ".pdf";
                    }
                    else if (request.ExportType == "png")
                    {
                        using (var ms = new MemoryStream())
                        {
                            image.SaveAsPng(ms);
                            finalData = ms.ToArray();
                        }
                        fileName += ".png";
                    }
                    else // jpg
                    {
                        using (var ms = new MemoryStream())
                        {
                            image.SaveAsJpeg(ms);
                            finalData = ms.ToArray();
                        }
                        fileName += ".jpg";
                    }

                    certificates.Add((fileName, finalData));

                    // 4. Send via Email
                    if (request.SendEmail && !string.IsNullOrEmpty(student.Email))
                    {
                        try
                        {
                            await _emailService.SendEmailWithAttachmentAsync(
                                student.Email,
                                "شهادة إتمام - نظام تراؤف",
                                $"السلام عليكم {student.Name}، نرفق لكم شهادتكم.",
                                finalData,
                                fileName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending email to {student.Email}: {ex.Message}");
                        }
                    }

                    // 5. Send via WhatsApp Simulation / Link Generation
                    if (request.SendWhatsApp && !string.IsNullOrEmpty(student.Phone))
                    {
                        var waLink = _whatsAppService.GenerateWhatsAppLink(student.Phone, $"شهادتك جاهزة يا {student.Name}");
                        // In a real scenario, you'd send the file via API, here we log the link
                        Console.WriteLine($"WhatsApp link for {student.Name}: {waLink}");
                    }
                }
                counter++;
            }

            // 4. Create ZIP archive
            using (var zipStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var cert in certificates)
                    {
                        var zipEntry = archive.CreateEntry(cert.FileName, CompressionLevel.Fastest);
                        using (var entryStream = zipEntry.Open())
                        {
                            entryStream.Write(cert.Data, 0, cert.Data.Length);
                        }
                    }
                }
                zipStream.Position = 0;
                return File(zipStream.ToArray(), "application/zip", "Certificates.zip");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmployee(int id, string name, string email, string phone, [FromServices] AppDbContext db)
        {
            var emp = await db.Employees.FindAsync(id);
            if (emp != null)
            {
                emp.Name = name;
                emp.Email = email;
                emp.Phone = phone;
                await db.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
