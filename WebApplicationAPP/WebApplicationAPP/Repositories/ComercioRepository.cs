using WebApplicationAPP.Data;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Repositories
{
    public class ComercioRepository : IComercioRepository
    {
        private readonly AppDbContext _context;

        public ComercioRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Comercio> ObtenerTodos()
        {
            return _context.Comercio.ToList();
        }

        public Comercio ObtenerPorId(int id)
        {
            return _context.Comercio.Find(id);
        }

        public void Agregar(Comercio comercio)
        {
            _context.Comercio.Add(comercio);
            _context.SaveChanges();
        }

        public void Actualizar(Comercio comercio)
        {
            _context.Comercio.Update(comercio);
            _context.SaveChanges();
        }
    }
}
