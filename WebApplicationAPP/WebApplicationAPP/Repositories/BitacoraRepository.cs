using WebApplicationAPP.Data;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Repositories
{
    public class BitacoraRepository : IBitacoraRepository
    {
        private readonly AppDbContext _context;

        public BitacoraRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Registrar(BitacoraEvento evento)
        {
            _context.BitacoraEvento.Add(evento);
            _context.SaveChanges();
        }

        public List<BitacoraEvento> ObtenerTodos()
        {
            return _context.BitacoraEvento
                .OrderByDescending(x => x.IdEvento)
                .ToList();
        }
    }
}
