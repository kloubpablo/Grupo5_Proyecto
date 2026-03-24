namespace WebApplicationAPP.Services
{
    public interface IBitacoraService
    {
        void RegistrarEvento(
            string tabla,
            string tipoEvento,
            string descripcion,
            string stackTrace,
            object? datosAnteriores = null,
            object? datosPosteriores = null);
    }
}
