using CertificateSystem.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Fast database creation without migrations for prototyping
    _db.Database.EnsureCreated();

    if (!_db.Employees.Any())
    {
        _db.Employees.AddRange(
            new Employee { Name = "أحمد خالد", CivilId = "1000000001", Gender = "ذكر", Role = "مدير", Department = "الإدارة العليا" },
            new Employee { Name = "سارة محمد", CivilId = "1000000002", Gender = "أنثى", Role = "مدير", Department = "تقنية المعلومات" },
            new Employee { Name = "فهد عبدالله", CivilId = "1000000003", Gender = "ذكر", Role = "رئيس قسم", Department = "الموارد البشرية" },
            new Employee { Name = "نورة يوسف", CivilId = "1000000004", Gender = "أنثى", Role = "رئيس قسم", Department = "التسويق" },
            new Employee { Name = "بدر صالح", CivilId = "1000000005", Gender = "ذكر", Role = "موظف", Department = "الموارد البشرية" },
            new Employee { Name = "مها علي", CivilId = "1000000006", Gender = "أنثى", Role = "موظف", Department = "تقنية المعلومات" },
            new Employee { Name = "حسن سعيد", CivilId = "1000000007", Gender = "ذكر", Role = "سائق", Department = "العمليات" },
            new Employee { Name = "ريم سعود", CivilId = "1000000008", Gender = "أنثى", Role = "موظف", Department = "التسويق" },
            new Employee { Name = "زياد طارق", CivilId = "1000000009", Gender = "ذكر", Role = "موظف", Department = "العمليات" }
        );
        _db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Disabled for Render compatibility (handled by their proxy)
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
