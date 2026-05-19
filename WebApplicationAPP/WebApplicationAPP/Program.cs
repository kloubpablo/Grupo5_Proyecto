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

app.UseAuthorization();

// Ruta inicial
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Seguridad}/{action=Index}/{id?}");

app.Run();