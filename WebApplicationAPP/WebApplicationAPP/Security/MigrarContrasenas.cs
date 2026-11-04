using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Security;

public static class MigrarContrasenas
{
    // Ejecutar una sola vez con: dotnet run -- --migrate-passwords
    // Con respaldo previo, aplicacion detenida y sin otros escritores activos.
    public static async Task EjecutarAsync(IServiceProvider servicios)
    {
        using var scope = servicios.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<YampiBarbershopContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
        await using var tx = await db.Database.BeginTransactionAsync();

        var usuarios = await db.Usuarios.OrderBy(u => u.IdUsuario).ToListAsync();
        int convertidos = 0;
        int existentes = 0;

        foreach (var usuario in usuarios)
        {
            if (string.IsNullOrEmpty(usuario.PasswordHash))
                throw new InvalidOperationException(
                    $"La cuenta con ID {usuario.IdUsuario} no tiene contrasena. " +
                    "No se ha confirmado ninguna conversion.");

            if (PasswordHashUtil.EsHashIdentity(usuario.PasswordHash))
            {
                existentes++;
                continue;
            }

            usuario.PasswordHash = hasher.HashPassword(usuario, usuario.PasswordHash);
            convertidos++;
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();
        Console.WriteLine($"Migracion completada. Convertidos: {convertidos}. Ya protegidos: {existentes}.");
        Console.WriteLine("No se imprimieron contrasenas ni hashes.");
    }
}
