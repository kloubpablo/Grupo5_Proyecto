using WebApplicationAPP.Models;
using WebApplicationAPP.Repositories;

namespace WebApplicationAPP.Business
{
    public class UsuarioBusiness
    {
        private readonly IUsuarioRepository _repo;

        public UsuarioBusiness(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        public List<Usuario> Listar()
        {
            return _repo.Listar();
        }

        public Usuario Obtener(int id)
        {
            return _repo.Obtener(id);
        }

        public string Insertar(Usuario u)
        {
            // VALIDAR USERNAME
            if (_repo.ExisteUsername(u.Username))
                return "El username ya existe";

            // VALIDAR CORREO
            if (_repo.ExisteCorreo(u.CorreoElectronico))
                return "El correo ya existe";

            _repo.Insertar(u);

            return "OK";
        }

        public string Actualizar(Usuario u)
        {
            var existente = _repo.Obtener(u.IdUsuario);

            if (existente == null)
                return "No existe";

            existente.Nombre = u.Nombre;
            existente.Username = u.Username;
            existente.CorreoElectronico = u.CorreoElectronico;
            existente.PasswordHash = u.PasswordHash;
            existente.IdRol = u.IdRol;

            _repo.Actualizar(existente);

            return "OK";
        }
    }
}