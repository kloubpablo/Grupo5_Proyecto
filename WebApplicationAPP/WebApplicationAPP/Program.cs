using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;

var builder = WebApplication.CreateBuilder(args);


// MVC
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// SESIONES
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDbContext<YampiBarbershopContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySqlConnection"),
        new MariaDbServerVersion(new Version(11, 4, 12))));

var app = builder.Build();

// ERRORES
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
    pattern: "{controller=YampiBarberShop}/{action=Index}/{id?}");

app.Run();