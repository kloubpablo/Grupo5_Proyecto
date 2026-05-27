using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class PagosController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public PagosController(YampiBarbershopContext context)
        {
            _context = context;
        }

        private bool EsRecepcionista()
        {
            return HttpContext.Session
                .GetString("Rol") == "Recepcionista";
        }

        public IActionResult Index()
        {
            if (!EsRecepcionista())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var pagos = _context.Pagos
                .Include(p => p.IdClienteNavigation)
                .ToList();

            return View(pagos);
        }

        public IActionResult Registrar()
        {
            if (!EsRecepcionista())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Registrar(
            string cliente,
            decimal monto,
            string metodo)
        {
            if (!EsRecepcionista())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            if (string.IsNullOrEmpty(cliente) ||
                monto <= 0 ||
                string.IsNullOrEmpty(metodo))
            {
                ViewBag.Error =
                    "Debe completar todos los datos correctamente";

                return View();
            }

            var clienteExistente = _context.Clientes
                .FirstOrDefault(c =>
                    c.Nombre == cliente);

            if (clienteExistente == null)
            {
                clienteExistente = new Cliente
                {
                    Nombre = cliente,
                    Telefono = "00000000",
                    FechaRegistro = DateTime.Now
                };

                _context.Clientes.Add(clienteExistente);

                _context.SaveChanges();
            }

            var pago = new Pago
            {
                IdCliente = clienteExistente.IdCliente,
                Monto = monto,
                Metodo = metodo,
                Fecha = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Pagos.Add(pago);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Cierre()
        {
            if (!EsRecepcionista())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            decimal total = _context.Pagos
                .Where(p =>
                    p.Fecha ==
                    DateOnly.FromDateTime(DateTime.Now))
                .Sum(p => (decimal?)p.Monto) ?? 0;

            ViewBag.Total = total;

            return View();
        }
    }
}