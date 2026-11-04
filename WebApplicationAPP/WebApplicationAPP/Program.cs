using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;
using Microsoft.AspNetCore.Identity;
using WebApplicationAPP.Security;
using WebApplicationAPP.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<GmailApiEmailSender>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(25);
});

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
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

if (args.Contains("--migrate-passwords", StringComparer.OrdinalIgnoreCase))
{
    await MigrarContrasenas.EjecutarAsync(app.Services);
    return;
}

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