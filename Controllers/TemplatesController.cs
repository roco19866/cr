using CertificateSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CertificateSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TemplatesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DesignTemplate>>> GetTemplates()
        {
            return await _context.DesignTemplates.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<DesignTemplate>> SaveTemplate(DesignTemplate template)
        {
            _context.DesignTemplates.Add(template);
            await _context.SaveChangesAsync();
            return Ok(template);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var template = await _context.DesignTemplates.FindAsync(id);
            if (template == null) return NotFound();

            _context.DesignTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
