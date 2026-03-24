using System;
using System.Text.Json;
using WebApplicationAPP.Models;
using WebApplicationAPP.Repositories;

namespace WebApplicationAPP.Services
{
    public class BitacoraService : IBitacoraService
    {
        private readonly IBitacoraRepository _repo;

        public BitacoraService(IBitacoraRepository repo)
        {
            _repo = repo;
        }

        public void RegistrarEvento(
            string tabla,
            string tipoEvento,
            string descripcion,
            string stackTrace,
            object? datosAnteriores = null,
            object? datosPosteriores = null)
        {
            var evento = new BitacoraEvento
            {
                TablaDeEvento = tabla,
                TipoDeEvento = tipoEvento,
                FechaDeEvento = DateTime.Now,
                DescripcionDeEvento = descripcion,
                StackTrace = stackTrace ?? "",
                DatosAnteriores = datosAnteriores != null
                    ? JsonSerializer.Serialize(datosAnteriores)
                    : null,
                DatosPosteriores = datosPosteriores != null
                    ? JsonSerializer.Serialize(datosPosteriores)
                    : null
            };

            _repo.Registrar(evento);
        }
    }
}
