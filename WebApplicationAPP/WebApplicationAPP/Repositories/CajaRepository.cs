using WebApplicationAPP.Data;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Repositories
{
    public class CajaRepository : ICajaRepository
    {
        private readonly AppDbContext _context;
        public CajaRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Cajas> GetAllCajas() { 
            return _context.Cajas.ToList();
        }
        public Cajas GetCajaById(int id) { 
            return _context.Cajas.Find(id);
        }

        public void AddCaja(Cajas caja) { 
            _context.Cajas.Add(caja);
            _context.SaveChanges();
        }

        public void UpdateCaja()
        {
            _context.SaveChanges();
        }

        public void DeleteCaja(int id) { 
            var caja = GetCajaById(id);
            if (caja != null)
            {
                _context.Cajas.Remove(caja);
                _context.SaveChanges();
            }
        }

        public bool ExisteNombreCaja(string nombre, int idComercio, int? idExcluir = null)
        {
            return _context.Cajas.Any(c =>
                c.Nombre == nombre &&
                c.IdComercio == idComercio &&
                (!idExcluir.HasValue || c.Id != idExcluir.Value));
        }

        public bool ExisteTelefonoActivo(string telefonoSINPE, int idComercio, int? idCajaExcluir = null)
        {
            return _context.Cajas.Any(c =>
                c.TelefonoSINPE == telefonoSINPE &&
                c.IdComercio == idComercio &&
                c.Estado == true &&
                (!idCajaExcluir.HasValue || c.Id != idCajaExcluir.Value));
        }

    }
}
