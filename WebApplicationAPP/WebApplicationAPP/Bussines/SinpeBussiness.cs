using WebApplicationAPP.Data;
using WebApplicationAPP.Models;
using WebApplicationAPP.Repositories;
using WebApplicationAPP.Services;

namespace WebApplicationAPP.Business
{
    public class SinpeBusiness
    {
        private readonly ISinpeRepository _sinpeRepo;
        private readonly ICajaRepository _cajasRepo;
        private readonly IBitacoraService _bitacora;
        private IBitacoraService? bitacora;

public SinpeBusiness(ISinpeRepository sinpeRepo,
                     ICajaRepository cajasRepo,
                     IBitacoraService bitacora)
{
    _sinpeRepo = sinpeRepo;
    _cajasRepo = cajasRepo;
    _bitacora = bitacora;
}

        public void Create(Sinpe sinpe)
        {
            // Validar que la caja exista
            var caja = _cajasRepo.GetAllCajas()
                .FirstOrDefault(c => c.TelefonoSINPE == sinpe.TelefonoDestinatario);

            if (caja == null)
                throw new Exception("El teléfono destinatario no está registrado.");

            // Validar que esté activa
            if (!caja.Estado)
                throw new Exception("No se puede pagar a una caja inactiva.");

            // Datos automáticos
            sinpe.FechaDeRegistro = DateTime.Now;
            sinpe.Estado = false; // No sincronizado

            _sinpeRepo.Create(sinpe);
            _bitacora.RegistrarEvento(
          "SINPE_G4",
          "Registrar",
          "Pago SINPE registrado",
          "",
          sinpe);
        
        }
    }
}
