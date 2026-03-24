using Microsoft.AspNetCore.Mvc;

namespace WebApplicationAPP.Controllers
{
    public class CitasController : Controller
    {
        // Lista simulada
        static List<dynamic> citas = new List<dynamic>();

        // LISTA
        public IActionResult Index()
        {
            return View(citas);
        }

        // CREAR (GET)
        public IActionResult Crear()
        {
            return View();
        }

        // CREAR (POST)
        [HttpPost]
        public IActionResult Crear(string cliente, string fecha, string hora, string barbero)
        {
            // 🔴 Validar campos vacíos
            if (string.IsNullOrEmpty(cliente) || string.IsNullOrEmpty(fecha) || string.IsNullOrEmpty(hora))
            {
                ViewBag.Error = "Debe completar todos los campos";
                return View();
            }

            // 🔴 Validar horario ocupado
            if (citas.Any(c => c.fecha == fecha && c.hora == hora))
            {
                ViewBag.Error = "Horario no disponible";
                return View();
            }

            citas.Add(new
            {
                id = citas.Count + 1,
                cliente,
                fecha,
                hora,
                barbero,
                estado = "Activa"
            });

            return RedirectToAction("Index");
        }

        // CANCELAR
        public IActionResult Cancelar(int id)
        {
            var cita = citas.FirstOrDefault(c => c.id == id);

            if (cita != null)
            {
                citas.Remove(cita);
            }

            return RedirectToAction("Index");
        }
    }
}