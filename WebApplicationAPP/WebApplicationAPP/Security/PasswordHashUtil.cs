using System;
using System.Buffers.Binary;

namespace WebApplicationAPP.Security;

// Solo se utiliza para identificar hashes de ASP.NET Identity ya migrados.
// No compara contrasenas: esa tarea corresponde a PasswordHasher<Usuario>.
public static class PasswordHashUtil
{
    public static bool EsHashIdentity(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return false;
        byte[] bytes;
        try { bytes = Convert.FromBase64String(valor); }
        catch (FormatException) { return false; }

        // Formato Identity v3: [0x01][PRF 4][Iteraciones 4][TamSal 4][Sal][Subclave].
        if (bytes.Length < 45 || bytes[0] != 0x01) return false;
        uint prf = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(1, 4));
        uint iteraciones = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(5, 4));
        uint tamSal = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(9, 4));
        return prf <= 2 && iteraciones >= 1000 && iteraciones <= 10000000
            && tamSal >= 16 && tamSal <= 128
            && (long)bytes.Length - 13 - tamSal >= 16;
    }
}
