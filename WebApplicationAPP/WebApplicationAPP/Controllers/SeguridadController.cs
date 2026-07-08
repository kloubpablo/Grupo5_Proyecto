using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace WebApplicationAPP.Controllers
{
    public class SeguridadController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public SeguridadController(YampiBarbershopContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Recuperar()
        {
            return View();
        }
        // REGISTRO CLIENTE
        public IActionResult Registrar()
        {
            return View();
          
        }

        [HttpPost]
        public IActionResult Registrar(
     string nombre,
     string username,
     string correo,
     string password)
        {
            if (string.IsNullOrEmpty(nombre) ||
       string.IsNullOrEmpty(username) ||
       string.IsNullOrEmpty(correo) ||
       string.IsNullOrEmpty(password))
            {
                ViewBag.Error =
                    "Debe completar todos los campos";

                return View();
            }

            bool existeUsuario = _context.Usuarios
                .Any(u => u.Username == username);

            if (existeUsuario)
            {
                ViewBag.Error =
                    "El usuario ya existe";

                return View();
            }

            bool existeCorreo = _context.Usuarios
                .Any(u => u.CorreoElectronico == correo);

            if (existeCorreo)
            {
                ViewBag.Error =
                    "El correo ya está registrado";

                return View();
            }

            // CREAR USUARIO
            var usuario = new Usuario
            {
                Nombre = nombre,
                Username = username,
                PasswordHash = password,
                CorreoElectronico = correo,
                IdRol = 4, 
                ContraTemp = false
            };

            _context.Usuarios.Add(usuario);

            // CREAR CLIENTE
            var cliente = new Cliente
            {
                Nombre = nombre,
                Telefono = "00000000",
                Email = correo,
                FechaRegistro = DateTime.Now
            };

            _context.Clientes.Add(cliente);

            _context.SaveChanges();

            TempData["Mensaje"] =
                "Cuenta creada correctamente";

            return RedirectToAction("Index");
        }

        public IActionResult CambiarContrasena()
        {
            return View();
        }

        // 🔥 LOGIN
        [HttpPost]
        public IActionResult Index(string usuario, string password)
        {
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Debe completar todos los campos";
                return View();
            }

            var user = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefault(u =>
                    u.Username == usuario &&
                    u.PasswordHash == password);

            if (user == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

            // 🔥 SESIÓN BÁSICA
            HttpContext.Session.SetInt32("IdUsuario", user.IdUsuario);
            HttpContext.Session.SetString("Rol", user.IdRolNavigation.Nombre);
            HttpContext.Session.SetString("NombreUsuario", user.Nombre);

            // ⚠️ IMPORTANTE:
            // Ya NO dependes de permisos en sesión
            // porque cambian dinámicamente en BD

            if (user.ContraTemp)
                return RedirectToAction("CambiarContrasena");

            return RedirectToAction("Index", "Dashboard");
        }

        // 🔥 LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
            HttpContext.Session.Remove("Rol");
        }

        // 🔥 CAMBIO CONTRASEÑA
        [HttpPost]
        public IActionResult CambiarContrasena(string nuevaContrasena, string confirmarContrasena)
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
                return RedirectToAction("Index");

            if (nuevaContrasena != confirmarContrasena)
            {
                ViewBag.Error = "Las contraseñas no coinciden";
                return View();
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == idUsuario);

            if (usuario == null)
                return RedirectToAction("Index");

            usuario.PasswordHash = nuevaContrasena;
            usuario.ContraTemp = false;

            _context.SaveChanges();

            return RedirectToAction("Index", "Dashboard");
        }

        // 🔥 RECUPERAR
        [HttpPost]
        public IActionResult Recuperar(string correo)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.CorreoElectronico == correo);

            if (usuario == null)
            {
                ViewBag.Error = "El correo ingresado no existe";
                return View();
            }

            string temporal = Guid.NewGuid().ToString().Substring(0, 8);

            usuario.PasswordHash = temporal;
            usuario.ContraTemp = true;

            _context.SaveChanges();

            var mensaje = new MimeMessage();

            mensaje.From.Add(new MailboxAddress(
                "Yampi Barbershop",
                "poveda1390@gmail.com"));

            mensaje.To.Add(MailboxAddress.Parse(correo));

            mensaje.Subject = "Recuperación de contraseña";

            mensaje.Body = new TextPart("plain")
            {
                Text = $"Su contraseña temporal es: {temporal}"
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("poveda1390@gmail.com", "vubf zvno qeay ualq");
                client.Send(mensaje);
                client.Disconnect(true);
            }

            ViewBag.Mensaje = "Se envió una contraseña temporal a su correo";
            return View();
        }
    }
}