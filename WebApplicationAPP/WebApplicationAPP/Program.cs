using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;

var builder = WebApplication.CreateBuilder(args);

// Conexión SQL Server
builder.Services.AddDbContext<YampiBarbershopContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SqlConnection")));

// Servicios MVC
builder.Services.AddControllersWithViews()
.AddRazorRuntimeCompilation();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Manejo de errores
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Seguridad}/{action=Index}/{id?}");

app.Run();