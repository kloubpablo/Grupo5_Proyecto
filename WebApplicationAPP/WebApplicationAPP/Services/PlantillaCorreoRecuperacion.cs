using System;
using System.Net;

namespace WebApplicationAPP.Services;

/// <summary>
/// Plantilla de correo independiente de los estilos CSS del sitio.
/// No requiere imagenes ni fuentes externas para mostrarse correctamente.
/// </summary>
public static class PlantillaCorreoRecuperacion
{
    public static string Crear(string nombre, string enlace)
    {
        // Nunca interpolar datos de usuarios directamente dentro del HTML.
        string nombreSeguro = WebUtility.HtmlEncode(nombre);
        string enlaceSeguro = WebUtility.HtmlEncode(enlace);

        return $$"""
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Recuperar contraseña - Yampi BarberShop</title>
</head>
<body style="margin:0;padding:0;background-color:#09090a;font-family:'Segoe UI',Arial,sans-serif;">
  <div style="display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;">
    Recupera el acceso a Yampi BarberShop. Tu enlace vence en 30 minutos.
  </div>

  <table role="presentation" cellpadding="0" cellspacing="0" border="0"
         width="100%" bgcolor="#09090a" style="width:100%;background-color:#09090a;">
    <tr><td align="center" style="padding:36px 14px;">
      <table role="presentation" cellpadding="0" cellspacing="0" border="0"
             width="100%" style="max-width:560px;width:100%;background-color:#171719;border:1px solid #35353a;border-radius:14px;">
        <tr><td align="center" style="padding:34px 28px 22px;">
          <div style="font-size:32px;line-height:1.1;font-weight:800;letter-spacing:4px;color:#d4af37;">YAMPI</div>
          <div style="font-size:12px;letter-spacing:5px;color:#f5f5f5;margin-top:9px;">BARBER SHOP</div>
          <div style="margin-top:20px;height:2px;background-color:#d4af37;width:64px;"></div>
        </td></tr>

        <tr><td style="padding:12px 30px 32px;">
          <h1 style="margin:0 0 20px;font-size:24px;line-height:1.3;color:#f5f5f5;font-weight:700;">Recupera tu contraseña</h1>
          <p style="margin:0 0 14px;font-size:15px;line-height:1.7;color:#f5f5f5;">Hola, {{nombreSeguro}}:</p>
          <p style="margin:0 0 22px;font-size:15px;line-height:1.7;color:#dadadd;">
            Recibimos una solicitud para restablecer la contraseña de tu cuenta de Yampi BarberShop.
          </p>
          <p style="margin:0 0 25px;font-size:15px;line-height:1.7;color:#dadadd;">
            Utiliza el siguiente botón para crear una contraseña nueva.
          </p>

          <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%">
            <tr><td align="center" bgcolor="#d4af37" style="background-color:#d4af37;border-radius:7px;">
              <a href="{{enlaceSeguro}}" style="display:block;padding:16px 20px;color:#09090a;font-family:'Segoe UI',Arial,sans-serif;font-size:16px;font-weight:700;text-align:center;text-decoration:none;">
                Restablecer contraseña
              </a>
            </td></tr>
          </table>

          <p style="margin:26px 0 12px;font-size:13px;line-height:1.7;color:#e4c65a;">
            Este enlace vence en 30 minutos y solo puede usarse una vez.
          </p>
          <p style="margin:0 0 16px;font-size:13px;line-height:1.7;color:#a5a5aa;">
            Si no solicitaste este cambio, puedes ignorar el mensaje. Tu contraseña actual no cambiará.
          </p>
          <p style="margin:0 0 8px;font-size:12px;line-height:1.5;color:#a5a5aa;">
            Si el botón no funciona, copia este enlace en tu navegador:
          </p>
          <p style="margin:0;font-size:12px;line-height:1.6;word-break:break-all;overflow-wrap:anywhere;">
            <a href="{{enlaceSeguro}}" style="color:#e4c65a;text-decoration:underline;">{{enlaceSeguro}}</a>
          </p>
        </td></tr>

        <tr><td align="center" style="padding:22px 25px;border-top:1px solid #35353a;">
          <p style="margin:0 0 8px;font-size:11px;letter-spacing:1.5px;font-weight:700;color:#d4af37;">TU ESTILO, NUESTRA PASIÓN</p>
          <p style="margin:0;font-size:12px;line-height:1.6;color:#a5a5aa;">
            © {{DateTime.UtcNow.Year}} Yampi BarberShop · Costa Rica
          </p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>
""";
    }
}
