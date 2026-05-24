using Microsoft.AspNetCore.Mvc;
using WebApplicationAPP.Models;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using AspNetCoreGeneratedDocument;
using WebApplicationAPP.Repositories;

namespace WebApplicationAPP.Controllers
{
    public class SeguridadController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public SeguridadController(YampiBarbershopContext context)
        {
            _context = context;
        }

        // GET
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Recuperar()
        {
            return View();
        }

        public IActionResult CambiarContrasena()
        {
            return View();
        }

        // POST
        [HttpPost]
        public IActionResult Index(string usuario, string password)
        {
            // Validar campos vacíos
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Debe completar todos los campos";
                return View();
            }

            // Buscar usuario en la BD
            var user = _context.Usuarios
                .FirstOrDefault(u =>
                    u.Username == usuario &&
                    u.PasswordHash == password);


            if (user != null)
            {   
                //Guarda la sesion
                HttpContext.Session.SetInt32("IdUsuario", user.IdUsuario);
           
            //Valida si el usuario debe  cambiar la contraseña
            if (user.ContraTemp)
            {
                return RedirectToAction("CambiarContrasena");
            }
            return RedirectToAction("Index", "Dashboard");
            }
            // Error login
            ViewBag.Error = "Usuario o contraseña incorrectos";
            return View();
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Seguridad");
        }

        [HttpPost]
        public IActionResult CambiarContrasena(
        string nuevaContrasena,
        string confirmarContrasena)
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");
            if (idUsuario == null)
            {
                return RedirectToAction("Index");
            }
            if(nuevaContrasena != confirmarContrasena)
            {
                ViewBag.Error = "Las contraseñas no coinciden intente nuevamente";
                return View();
            }
            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == idUsuario);
            if(usuario == null)
            {
                return RedirectToAction("Index");
            }
            usuario.PasswordHash = nuevaContrasena;
            usuario.ContraTemp = false;
            _context.SaveChanges();
            return RedirectToAction(
                "Index", "Dashboard");
        }

        [HttpPost]
        public IActionResult Recuperar (string correo)
        {
            var usuario = _context.Usuarios.FirstOrDefault(U => U.CorreoElectronico == correo);
            if (usuario == null)
            {
                ViewBag.Error = "El correo ingresado no existe";
                return View();
            }
            //Genera un identificador lo covierte en texto y tomo solo los primero 8 digitos para generar la contraseña temporal
            string temporal = Guid.NewGuid().ToString().Substring(0, 8);

            usuario.PasswordHash = temporal;
            usuario.ContraTemp = true;

            _context.SaveChanges();

            //Crea el correo
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("Yampi Barbershop", "poveda1390@gmail.com"));

            mensaje.To.Add(MailboxAddress.Parse(correo));

            mensaje.Subject = "Recuperacion de contraseña";

            mensaje.Body = new TextPart("plain")
            {
                Text = $"Por su solicitud de recuperacion de contraseña se le asigno una contraseña temporal" +
                $"La cual es :{temporal}"
            };
            //Envia el mensaje
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("poveda1390@gmail.com", "vubf zvno qeay ualq");//Correo electronico que envia el mesanje y contraseña de ´plicacion que genera google
                client.Send(mensaje);
                client.Disconnect(true);
            }
            ViewBag.mensaje = "Se envio un correo a tu correo registrado con una contraseña temporal";
            return View();
        }
    }
}