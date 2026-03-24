using Microsoft.AspNetCore.Mvc;

namespace WebApplicationAPP.Controllers
{
    public class PagosController : Controller
    {
        // Lista simulada
        static List<dynamic> pagos = new List<dynamic>();

        // LISTA DE PAGOS
        public IActionResult Index()
        {
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
            // 🔴 Validación (HU)
            if (string.IsNullOrEmpty(cliente) || monto <= 0 || string.IsNullOrEmpty(metodo))
            {
                ViewBag.Error = "Debe completar todos los datos correctamente";
                return View();
            }

            pagos.Add(new
            {
                id = pagos.Count + 1,
                cliente,
                monto,
                metodo,
                fecha = DateTime.Now
            });

            return RedirectToAction("Index");
        }

        // CIERRE DE CAJA
        public IActionResult Cierre()
        {
            decimal total = pagos.Sum(p => (decimal)p.monto);
            ViewBag.Total = total;

            return View();
        }
    }
}