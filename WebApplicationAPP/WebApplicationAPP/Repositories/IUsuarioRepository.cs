using WebApplicationAPP.Models;

namespace WebApplicationAPP.Repositories
{
    public interface IUsuarioRepository
    {
        List<Usuario> Listar();

        Usuario Obtener(int id);

        void Insertar(Usuario u);

        void Actualizar(Usuario u);

        bool ExisteUsername(string username);

        bool ExisteCorreo(string correo);
    }
}
