var builder = WebApplication.CreateBuilder(args);

// Servicios básicos MVC
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

// 👉 Ruta inicial (IMPORTANTE)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Seguridad}/{action=Index}/{id?}");

app.Run();