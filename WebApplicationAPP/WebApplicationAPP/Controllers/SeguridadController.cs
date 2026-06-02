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

        // LOGIN
        public IActionResult Index()
        {
            return View();
        }

        // RECUPERAR
        public IActionResult Recuperar()
        {
            return View();
        }
        // REGISTRO CLIENTE
        public IActionResult Registrar()
        {
            return View();
            ViewBag.Error = "Entró al POST";
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

        // CAMBIAR CONTRASEÑA
        public IActionResult CambiarContrasena()
        {
            return View();
        }

        // LOGIN POST
        [HttpPost]
        public IActionResult Index(string usuario, string password)
        {
            // VALIDAR CAMPOS
            if (string.IsNullOrEmpty(usuario) ||
                string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Debe completar todos los campos";

                return View();
            }

            // BUSCAR USUARIO
            var user = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefault(u =>
                    u.Username == usuario &&
                    u.PasswordHash == password);

            // VALIDAR LOGIN
            if (user != null)
            {
                // SESIONES
                HttpContext.Session.SetInt32("IdUsuario", user.IdUsuario);

                HttpContext.Session.SetString(
                    "Rol",
                    user.IdRolNavigation.Nombre);

                HttpContext.Session.SetString(
                    "NombreUsuario",
                    user.Nombre);

                // VALIDAR CONTRASEÑA TEMPORAL
                if (user.ContraTemp)
                {
                    return RedirectToAction("CambiarContrasena");
                }

                if (user.IdRolNavigation.Nombre == "Cliente")
                {
                    return RedirectToAction(
                        "MisCitas",
                        "Citas");
                }

                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            // ERROR
            ViewBag.Error = "Usuario o contraseña incorrectos";

            return View();
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index");
        }

        // CAMBIAR CONTRASEÑA POST
        [HttpPost]
        public IActionResult CambiarContrasena(
            string nuevaContrasena,
            string confirmarContrasena)
        {
            int? idUsuario =
                HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Index");
            }

            // VALIDAR CONTRASEÑAS
            if (nuevaContrasena != confirmarContrasena)
            {
                ViewBag.Error =
                    "Las contraseñas no coinciden";

                return View();
            }

            // BUSCAR USUARIO
            var usuario = _context.Usuarios
                .FirstOrDefault(u =>
                    u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                return RedirectToAction("Index");
            }

            // ACTUALIZAR CONTRASEÑA
            usuario.PasswordHash = nuevaContrasena;

            usuario.ContraTemp = false;

            _context.SaveChanges();

            return RedirectToAction(
                "Index",
                "Dashboard");
        }

        // RECUPERAR CONTRASEÑA
        [HttpPost]
        public IActionResult Recuperar(string correo)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u =>
                    u.CorreoElectronico == correo);

            // VALIDAR CORREO
            if (usuario == null)
            {
                ViewBag.Error =
                    "El correo ingresado no existe";

                return View();
            }

            // GENERAR CONTRASEÑA TEMPORAL
            string temporal =
                Guid.NewGuid()
                .ToString()
                .Substring(0, 8);

            usuario.PasswordHash = temporal;

            usuario.ContraTemp = true;

            _context.SaveChanges();

            // CREAR CORREO
            var mensaje = new MimeMessage();

            mensaje.From.Add(
                new MailboxAddress(
                    "Yampi Barbershop",
                    "poveda1390@gmail.com"));

            mensaje.To.Add(
                MailboxAddress.Parse(correo));

            mensaje.Subject =
                "Recuperación de contraseña";

            mensaje.Body = new TextPart("plain")
            {
                Text =
                $"Su contraseña temporal es: {temporal}"
            };

            // ENVIAR CORREO
            using (var client = new SmtpClient())
            {
                client.Connect(
                    "smtp.gmail.com",
                    587,
                    false);

                client.Authenticate(
                    "poveda1390@gmail.com",
                    "vubf zvno qeay ualq");

                client.Send(mensaje);

                client.Disconnect(true);
            }

            ViewBag.Mensaje =
                "Se envió una contraseña temporal a su correo";

            return View();
        }
    }
}