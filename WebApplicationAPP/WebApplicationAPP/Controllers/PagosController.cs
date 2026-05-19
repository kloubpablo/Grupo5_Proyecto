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

        // LISTA DE PAGOS
        public IActionResult Index()
        {
            var pagos = _context.Pagos
                .Include(p => p.IdClienteNavigation)
                .ToList();

            return View(pagos);
        }

        // REGISTRAR PAGO (GET)
        public IActionResult Registrar()
        {
            return View();
        }

        // REGISTRAR PAGO (POST)
        [HttpPost]
        public IActionResult Registrar(string cliente, decimal monto, string metodo)
        {
            // VALIDAR DATOS
            if (string.IsNullOrEmpty(cliente) ||
                monto <= 0 ||
                string.IsNullOrEmpty(metodo))
            {
                ViewBag.Error = "Debe completar todos los datos correctamente";
                return View();
            }

            // BUSCAR CLIENTE
            var clienteExistente = _context.Clientes
                .FirstOrDefault(c => c.Nombre == cliente);

            // CREAR CLIENTE SI NO EXISTE
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

            // CREAR PAGO
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

        // CIERRE DE CAJA
        public IActionResult Cierre()
        {
            decimal total = _context.Pagos
                .Where(p => p.Fecha == DateOnly.FromDateTime(DateTime.Now))
                .Sum(p => (decimal?)p.Monto) ?? 0;

            ViewBag.Total = total;

            return View();
        }
    }
}