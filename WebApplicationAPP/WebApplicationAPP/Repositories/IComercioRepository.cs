using WebApplicationAPP.Models;

namespace WebApplicationAPP.Repositories
{
    public interface IComercioRepository
    {
        List<Comercio> ObtenerTodos();
        Comercio ObtenerPorId(int id);
        void Agregar(Comercio comercio);
        void Actualizar(Comercio comercio);
    }
}
