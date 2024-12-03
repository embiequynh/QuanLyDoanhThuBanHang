using Microsoft.EntityFrameworkCore;
using SalesManagementApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext với SQL Server
builder.Services.AddDbContext<SalesContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đảm bảo không sử dụng Browser Link và Razor Runtime Compilation
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Cấu hình cho Static files và routing
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
