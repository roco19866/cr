using Microsoft.EntityFrameworkCore;

namespace CertificateSystem.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<CertificateHistory> CertificateHistories { get; set; }
        public DbSet<DesignTemplate> DesignTemplates { get; set; }
    }
}
