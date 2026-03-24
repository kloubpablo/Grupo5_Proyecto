using Microsoft.AspNetCore.Mvc;

namespace WebApplicationAPP.Controllers
{
    public class ClientesController : Controller
    {
        // Lista simulada
        static List<dynamic> clientes = new List<dynamic>();

        // LISTA
        public IActionResult Index(string buscar)
        {
            var lista = clientes;

            // 🔍 Buscar cliente
            if (!string.IsNullOrEmpty(buscar))
            {
                lista = clientes.Where(c => c.nombre.Contains(buscar)).ToList();
            }

            return View(lista);
        }

        // CREAR (GET)
        public IActionResult Crear()
        {
            return View();
        }

        // CREAR (POST)
        [HttpPost]
        public IActionResult Crear(string nombre, string telefono)
        {
            // 🔴 Validación
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(telefono))
            {
                ViewBag.Error = "Debe completar todos los campos";
                return View();
            }

            // 🔴 Duplicado
            if (clientes.Any(c => c.nombre == nombre))
            {
                ViewBag.Error = "El cliente ya existe";
                return View();
            }

            clientes.Add(new
            {
                id = clientes.Count + 1,
                nombre,
                telefono
            });

            return RedirectToAction("Index");
        }

        // EDITAR (GET)
        public IActionResult Editar(int id)
        {
            var cliente = clientes.FirstOrDefault(c => c.id == id);
            return View(cliente);
        }

        // EDITAR (POST)
        [HttpPost]
        public IActionResult Editar(int id, string nombre, string telefono)
        {
            var cliente = clientes.FirstOrDefault(c => c.id == id);

            if (cliente != null)
            {
                clientes.Remove(cliente);

                clientes.Add(new
                {
                    id,
                    nombre,
                    telefono
                });
            }

            return RedirectToAction("Index");
        }
    }
}