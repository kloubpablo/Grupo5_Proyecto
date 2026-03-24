using Microsoft.AspNetCore.Mvc;
using WebApplicationAPP.Bussines;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class ComercioController : Controller
    {
        private readonly ComercioBussines _comercioBussines;

        public ComercioController(ComercioBussines bussines)
        {
            _comercioBussines = bussines;
        }

        public IActionResult Index()
        {
            var lista = _comercioBussines.Listar();
            return View(lista);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Comercio comercio)
        {
            if (!ModelState.IsValid)
                return View(comercio);

            _comercioBussines.Registrar(comercio);

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var comercio = _comercioBussines.Obtener(id);
            return View(comercio);
        }

        [HttpPost]
        public IActionResult Edit(Comercio comercio)
        {
            _comercioBussines.Editar(comercio);
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            var comercio = _comercioBussines.Obtener(id);
            return View(comercio);
        }
    }
}
