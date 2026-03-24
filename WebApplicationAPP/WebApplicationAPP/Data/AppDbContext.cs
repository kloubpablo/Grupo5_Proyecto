using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Cajas> Cajas { get; set; }

        public DbSet<Sinpe> Sinpe { get; set; }

        public DbSet<Comercio> Comercio { get; set; }

        public DbSet<BitacoraEvento> BitacoraEvento { get; set; }
        //public DbSet<WebApplicationAPP.Models.Producto> Productos { get; set; }
    }
}
