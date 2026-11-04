
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using WebApplicationAPP.Models;
using WebApplicationAPP.Services;

namespace WebApplicationAPP.Controllers
{
    public class SeguridadController : Controller
    {
        private readonly YampiBarbershopContext _context;
        private readonly IPasswordHasher<Usuario> _hasher;
        private readonly IConfiguration _config;
        private readonly ILogger<SeguridadController> _logger;
        private readonly GmailApiEmailSender _gmailApi;

        public SeguridadController(
            YampiBarbershopContext context,
            IPasswordHasher<Usuario> hasher,
            IConfiguration config,
            ILogger<SeguridadController> logger,
            GmailApiEmailSender gmailApi)
        {
            _context = context;
            _hasher = hasher;
            _config = config;
            _logger = logger;
            _gmailApi = gmailApi;
        }

        // ==========================================
        // LOGIN - GET
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ==========================================
        // RECUPERAR - GET
        // ==========================================

        [HttpGet]
        public IActionResult Recuperar()
        {
            return View();
        }

        // ==========================================
        // REGISTRO - GET
        // ==========================================

        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        // ==========================================
        // REGISTRO - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(
            string nombre,
            string username,
            string correo,
            string password,
            string confirmPassword)
        {
            nombre = nombre?.Trim() ?? "";
            username = username?.Trim() ?? "";
            correo = correo?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "Debe completar todos los campos";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden";
                return View();
            }

            if (_context.Usuarios.Any(u => u.Username == username))
            {
                ViewBag.Error = "El usuario ya existe";
                return View();
            }

            if (_context.Usuarios.Any(u => u.CorreoElectronico == correo))
            {
                ViewBag.Error = "El correo ya está registrado";
                return View();
            }

            var rolCliente = _context.Roles
                .FirstOrDefault(r =>
                    r.IdRol == 7 &&
                    r.Nombre == "Cliente" &&
                    r.Estado);

            if (rolCliente == null)
            {
                ViewBag.Error =
                    "No se encontró el rol Cliente. Contacta al administrador.";

                return View();
            }

            var usuario = new Usuario
            {
                Nombre = nombre,
                Username = username,
                CorreoElectronico = correo,
                IdRol = rolCliente.IdRol,
                ContraTemp = false,
                Estado = true
            };

            // Guardar contraseña con hashing seguro
            usuario.PasswordHash =
                _hasher.HashPassword(usuario, password);

            _context.Usuarios.Add(usuario);

            _context.Clientes.Add(new Cliente
            {
                Nombre = nombre,
                Telefono = "00000000",
                Email = correo,
                FechaRegistro = DateTime.Now
            });

            _context.SaveChanges();

            TempData["Mensaje"] = "Cuenta creada correctamente";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // CAMBIAR CONTRASEÑA - GET
        // ==========================================

        [HttpGet]
        public IActionResult CambiarContrasena()
        {
            if (HttpContext.Session.GetInt32("IdUsuario") == null)
                return RedirectToAction(nameof(Index));

            return View();
        }

        // ==========================================
        // LOGIN - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string usuario, string password)
        {
            if (string.IsNullOrEmpty(usuario) ||
                string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Debe completar todos los campos";
                return View();
            }

            var user = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefault(u =>
                    u.Username == usuario &&
                    u.Estado);

            if (user == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

            // Verificar contraseña mediante PasswordHasher
            var verificacion = _hasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password
            );

            if (verificacion == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

            // Actualizar hash si el algoritmo lo requiere
            if (verificacion ==
                PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash =
                    _hasher.HashPassword(user, password);

                _context.SaveChanges();
            }

            // Crear sesión
            HttpContext.Session.Clear();

            HttpContext.Session.SetInt32(
                "IdUsuario",
                user.IdUsuario
            );

            HttpContext.Session.SetString(
                "Rol",
                user.IdRolNavigation.Nombre
            );

            HttpContext.Session.SetString(
                "NombreUsuario",
                user.Nombre
            );

            if (user.ContraTemp)
                return RedirectToAction(nameof(CambiarContrasena));

            return IrSegunRol(user.IdRolNavigation.Nombre);
        }

        // ==========================================
        // CERRAR SESIÓN
        // ==========================================

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // CAMBIAR CONTRASEÑA - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarContrasena(
            string nuevaContrasena,
            string confirmarContrasena)
        {
            int? idUsuario =
                HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
                return RedirectToAction(nameof(Index));

            if (string.IsNullOrWhiteSpace(nuevaContrasena) ||
                nuevaContrasena != confirmarContrasena)
            {
                ViewBag.Error =
                    "Las contraseñas no coinciden o están vacías";

                return View();
            }

            var user = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefault(u =>
                    u.IdUsuario == idUsuario &&
                    u.Estado);

            if (user == null)
                return RedirectToAction(nameof(Index));

            // Guardar contraseña con hash
            user.PasswordHash =
                _hasher.HashPassword(user, nuevaContrasena);

            user.ContraTemp = false;

            _context.SaveChanges();

            return IrSegunRol(user.IdRolNavigation.Nombre);
        }

        // ==========================================
        // RECUPERACIÓN POR ENLACE - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recuperar(string correo)
        {
            const string respuesta =
                "Si el correo está registrado, recibirás un enlace para recuperar tu contraseña.";

            correo = correo?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(correo))
            {
                ViewBag.Error = "Ingresa tu correo electrónico";
                return View();
            }

            // Buscar usuario activo
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.CorreoElectronico == correo &&
                    u.Estado);

            if (usuario == null)
            {
                ViewBag.Mensaje = respuesta;
                return View();
            }

            DateTime ahora = DateTime.UtcNow;

            // Evitar solicitudes repetidas durante 2 minutos
            bool solicitudReciente =
                await _context.RecuperacionesContrasena
                    .AnyAsync(r =>
                        r.IdUsuario == usuario.IdUsuario &&
                        r.FechaCreacion > ahora.AddMinutes(-2));

            if (solicitudReciente)
            {
                ViewBag.Mensaje = respuesta;
                return View();
            }

            // ==========================================
            // VALIDAR URL LOCAL O DE PRODUCCIÓN
            // ==========================================

            string? baseUrl = _config["App:BaseUrl"];

            bool urlValida =
                Uri.TryCreate(
                    baseUrl,
                    UriKind.Absolute,
                    out var uri
                ) &&
                (
                    // HTTPS permitido en cualquier entorno
                    uri.Scheme == Uri.UriSchemeHttps ||

                    // HTTP permitido solo en localhost y Development
                    (
                        HttpContext.RequestServices
                            .GetRequiredService<IWebHostEnvironment>()
                            .IsDevelopment() &&

                        uri.Scheme == Uri.UriSchemeHttp &&

                        uri.IsLoopback
                    )
                );

            if (!urlValida)
            {
                _logger.LogError(
                    "App:BaseUrl no tiene una URL válida."
                );

                ViewBag.Error =
                    "No se pudo procesar la recuperación.";

                return View();
            }

            // Reutilizar la URL ya validada
            Uri baseUri = uri!;

            // ==========================================
            // GENERAR TOKEN SEGURO
            // ==========================================

            string token = Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32)
            );

            string enlace =
                baseUri.ToString().TrimEnd('/') +
                "/Seguridad/Restablecer?token=" +
                Uri.EscapeDataString(token);

            var recuperacion = new RecuperacionContrasena
            {
                IdUsuario = usuario.IdUsuario,
                TokenHash = ObtenerHashToken(token),
                FechaCreacion = ahora,
                FechaExpiracion = ahora.AddMinutes(30)
            };

            _context.RecuperacionesContrasena.Add(recuperacion);

            await _context.SaveChangesAsync();

            // ==========================================
            // ENVIAR CORREO PERSONALIZADO CON GMAIL API
            // ==========================================

            try
            {
                // Versión de texto plano
                string contenido =
                    $"Hola {usuario.Nombre},\n\n" +
                    "Recibimos una solicitud para restablecer tu contraseña de Yampi BarberShop.\n\n" +
                    "Para crear una nueva contraseña, abre este enlace:\n\n" +
                    $"{enlace}\n\n" +
                    "Este enlace vence en 30 minutos y solo puede utilizarse una vez.\n" +
                    "Si no solicitaste este cambio, ignora el mensaje. Tu contraseña no cambiará.\n\n" +
                    "Yampi BarberShop";

                // Versión HTML negra y dorada
                string contenidoHtml =
                    PlantillaCorreoRecuperacion.Crear(
                        usuario.Nombre,
                        enlace
                    );

                // Enviar mediante Gmail API por HTTPS
                await _gmailApi.EnviarAsync(
                    destinatario: usuario.CorreoElectronico,
                    asunto: "Recuperación de contraseña - Yampi BarberShop",
                    contenido: contenido,
                    cancellationToken: HttpContext.RequestAborted,
                    contenidoHtml: contenidoHtml
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error enviando recuperación del usuario {UsuarioId}.",
                    usuario.IdUsuario
                );

                // Eliminar solicitud si no se pudo enviar
                _context.RecuperacionesContrasena.Remove(recuperacion);

                await _context.SaveChangesAsync();
            }

            ViewBag.Mensaje = respuesta;

            return View();
        }

        // ==========================================
        // RESTABLECER CONTRASEÑA - GET
        // ==========================================

        [HttpGet]
        [ResponseCache(
            NoStore = true,
            Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Restablecer(string token)
        {
            Response.Headers["Referrer-Policy"] = "no-referrer";

            if (!EsTokenValido(token))
            {
                ViewBag.Error =
                    "El enlace es inválido o ha vencido.";

                return View();
            }

            string hashToken = ObtenerHashToken(token);

            DateTime ahora = DateTime.UtcNow;

            bool existe =
                await _context.RecuperacionesContrasena
                    .AnyAsync(r =>
                        r.TokenHash == hashToken &&
                        r.FechaUso == null &&
                        r.FechaExpiracion > ahora);

            if (!existe)
            {
                ViewBag.Error =
                    "El enlace es inválido, ha vencido o ya fue utilizado.";

                return View();
            }

            ViewBag.Token = token;

            return View();
        }

        // ==========================================
        // RESTABLECER CONTRASEÑA - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(
            NoStore = true,
            Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Restablecer(
            string token,
            string nuevaContrasena,
            string confirmarContrasena)
        {
            Response.Headers["Referrer-Policy"] = "no-referrer";

            if (!EsTokenValido(token))
            {
                ViewBag.Error = "Enlace de recuperación inválido.";
                return View();
            }

            ViewBag.Token = token;

            if (string.IsNullOrWhiteSpace(nuevaContrasena) ||
                nuevaContrasena.Length < 8)
            {
                ViewBag.Error =
                    "La contraseña debe tener al menos 8 caracteres.";

                return View();
            }

            if (nuevaContrasena != confirmarContrasena)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View();
            }

            string hashToken = ObtenerHashToken(token);

            DateTime ahora = DateTime.UtcNow;

            // Iniciar transacción
            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            // Consumir token de forma atómica
            int actualizados =
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE recuperaciones_contrasena SET fecha_uso = {ahora} WHERE token_hash = {hashToken} AND fecha_uso IS NULL AND fecha_expiracion > {ahora}"
                );

            if (actualizados != 1)
            {
                await transaccion.RollbackAsync();

                ViewBag.Error =
                    "El enlace es inválido, ha vencido o ya fue utilizado.";

                return View();
            }

            var recuperacion =
                await _context.RecuperacionesContrasena
                    .AsNoTracking()
                    .FirstAsync(r => r.TokenHash == hashToken);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == recuperacion.IdUsuario &&
                    u.Estado);

            if (usuario == null)
            {
                await transaccion.RollbackAsync();

                ViewBag.Error = "No se pudo recuperar la cuenta.";
                return View();
            }

            // Guardar contraseña con hashing seguro
            usuario.PasswordHash =
                _hasher.HashPassword(
                    usuario,
                    nuevaContrasena
                );

            usuario.ContraTemp = false;

            await _context.SaveChangesAsync();

            // Invalidar otros enlaces pendientes
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE recuperaciones_contrasena SET fecha_uso = {ahora} WHERE id_usuario = {usuario.IdUsuario} AND fecha_uso IS NULL"
            );

            await transaccion.CommitAsync();

            // Limpiar sesión
            HttpContext.Session.Clear();

            TempData["Mensaje"] =
                "Contraseña actualizada correctamente. Ya puedes iniciar sesión.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // VALIDAR FORMATO DEL TOKEN
        // ==========================================

        private static bool EsTokenValido(string? token)
        {
            return token != null &&
                   token.Length == 64 &&
                   token.All(Uri.IsHexDigit);
        }

        // ==========================================
        // GENERAR HASH DEL TOKEN
        // ==========================================

        private static string ObtenerHashToken(string token)
        {
            return Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(token)
                )
            );
        }

        // ==========================================
        // REDIRECCIÓN SEGÚN ROL
        // ==========================================

        private IActionResult IrSegunRol(string rol)
        {
            if (rol == "Administrador")
                return RedirectToAction("Index", "Dashboard");

            if (rol == "Cliente")
                return RedirectToAction("MisCitas", "Citas");

            if (rol == "Barbero")
                return RedirectToAction("Index", "Atencion");

            return RedirectToAction(nameof(Index));
        }
    }
}
