using Microsoft.AspNetCore.Mvc;
using WebApplicationAPP.Repositories;

namespace WebApplicationAPP.Controllers
{
    public class BitacoraController : Controller
    {
        private readonly IBitacoraRepository _repo;

        public BitacoraController(IBitacoraRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            var lista = _repo.ObtenerTodos();
            return View(lista);
        }
    }
}
