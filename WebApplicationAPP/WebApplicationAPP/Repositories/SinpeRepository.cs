using WebApplicationAPP.Data;
using WebApplicationAPP.Models;


namespace WebApplicationAPP.Repositories
{
    public class SinpeRepository : ISinpeRepository
    {
        private readonly AppDbContext _context;

        public SinpeRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Create(Sinpe sinpe)
        {
            _context.Sinpe.Add(sinpe);
            _context.SaveChanges();
        }

        public List<Sinpe> ObtenerTodos()
        {
            return _context.Sinpe.ToList();
        }



    }
}
