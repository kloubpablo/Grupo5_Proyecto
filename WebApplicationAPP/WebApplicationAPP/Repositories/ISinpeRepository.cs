using WebApplicationAPP.Models;

namespace WebApplicationAPP.Repositories
{
    public interface ISinpeRepository
    {
        void Create(Sinpe sinpe);
        List<Sinpe> ObtenerTodos();
    }
}
