using Microsoft.AspNetCore.Mvc;

namespace WebApplicationAPP.Controllers
{
    public class AtencionController : Controller
    {
        // Lista de espera
        static List<dynamic> listaEspera = new List<dynamic>();

        // Vista principal
        public IActionResult Index()
        {
            return View(listaEspera);
        }

        // AGREGAR A LISTA (cliente sin cita)
        public IActionResult Agregar(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                return RedirectToAction("Index");
            }

            listaEspera.Add(new
            {
                id = listaEspera.Count + 1,
                nombre = nombre,
                estado = "En espera"
            });

            return RedirectToAction("Index");
        }

        // INICIAR SERVICIO
        public IActionResult Iniciar(int id)
        {
            var cliente = listaEspera.FirstOrDefault(c => c.id == id);

            if (cliente != null)
            {
                listaEspera.Remove(cliente);

                listaEspera.Add(new
                {
                    id = cliente.id,
                    nombre = cliente.nombre,
                    estado = "En servicio"
                });
            }

            return RedirectToAction("Index");
        }

        // FINALIZAR SERVICIO
        public IActionResult Finalizar(int id)
        {
            var cliente = listaEspera.FirstOrDefault(c => c.id == id);

            if (cliente != null)
            {
                listaEspera.Remove(cliente);
            }

            return RedirectToAction("Index");
        }
    }
}