using WebApplicationAPP.Models;

namespace WebApplicationAPP.Repositories
{
    public interface ICajaRepository
    {
        List<Cajas> GetAllCajas();
        Cajas GetCajaById(int id);
        void AddCaja(Cajas Caja);
        void UpdateCaja();
        void DeleteCaja(int id);

        bool ExisteNombreCaja(string nombre, int idComercio, int? idCajaExcluir = null);
        bool ExisteTelefonoActivo(string telefonoSINPE, int idComercio, int? idCajaExcluir = null);

    }
}
