using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Bussines;
using WebApplicationAPP.Data;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class CajaController : Controller
    {
        private readonly CajaBussiness _cajaBussiness;
        private readonly AppDbContext _context;

        public CajaController(CajaBussiness cajaBussiness, AppDbContext context)
        {
            _cajaBussiness = cajaBussiness;
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_cajaBussiness.GetAllCajas());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cajas caja)
        {
            caja.FechaDeRegistro = DateTime.Now;
            caja.Estado = true;

            _cajaBussiness.AddCaja(caja);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {

            return View(_cajaBussiness.GetCajaById(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Cajas caja)
        {


            caja.FechaDeModificacion = DateTime.Now;

            _cajaBussiness.UpdateCaja(caja);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var caja = _cajaBussiness.GetCajaById(id);
            if (caja == null)
                return NotFound();

            return View(caja);
        }

        public IActionResult Delete(int id)
        {
            var caja = _cajaBussiness.GetCajaById(id);
            if (caja == null)
                return NotFound();

            return View(caja);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmar(int id)
        {
            _cajaBussiness.DeleteCaja(id);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult VerSINPE(int id)
        {
            var caja = _cajaBussiness.GetCajaById(id);
            if (caja == null)
                return NotFound();

            ViewBag.NombreCaja = caja.Nombre;
            ViewBag.TelefonoCaja = caja.TelefonoSINPE;

            var listaSinpe = _context.Sinpe
                .Where(s => s.TelefonoDestinatario == caja.TelefonoSINPE)
                .OrderByDescending(s => s.FechaDeRegistro)
                .ToList();

            return View(listaSinpe);
        }
    }
}