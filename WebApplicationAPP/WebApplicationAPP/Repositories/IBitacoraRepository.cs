using System.Collections.Generic;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Repositories
{
    public interface IBitacoraRepository
    {
        void Registrar(BitacoraEvento evento);
        List<BitacoraEvento> ObtenerTodos();
    }
}
