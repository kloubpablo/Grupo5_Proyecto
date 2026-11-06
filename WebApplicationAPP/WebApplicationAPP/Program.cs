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


app.UseStatusCodePages(async context =>
{
    var http = context.HttpContext;

    if (http.Response.StatusCode != 403 ||
        !HttpMethods.IsGet(http.Request.Method))
    {
        return;
    }

    int? idUsuario = http.Session.GetInt32("IdUsuario");
    string? rol = http.Session.GetString("Rol");

    string destino;

    if (!idUsuario.HasValue)
    {
        // Usuario sin iniciar sesion
        destino = "/Home/Index";
    }
    else if (string.Equals(rol, "Cliente",
        StringComparison.OrdinalIgnoreCase))
    {
        // Cliente: sus citas
        destino = "/Citas/MisCitas";
    }
    else if (string.Equals(rol, "Barbero",
        StringComparison.OrdinalIgnoreCase) ||
        string.Equals(rol, "Administrador",
        StringComparison.OrdinalIgnoreCase))
    {
        // Barbero y administrador: Dashboard
        destino = "/Dashboard/Index";
    }
    else
    {
        destino = "/Home/Index";
    }

    // Evitar bucles de redireccion
    if (!http.Request.Path.Equals(
        destino, StringComparison.OrdinalIgnoreCase))
    {
        http.Response.Redirect(destino);
    }

    await Task.CompletedTask;
});


app.Run();