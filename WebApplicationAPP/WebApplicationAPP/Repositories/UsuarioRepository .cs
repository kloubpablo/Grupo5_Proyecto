using WebApplicationAPP.Data;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly YampiBarbershopContext _context;

        public UsuarioRepository(YampiBarbershopContext context)
        {
            _context = context;
        }

        public List<Usuario> Listar()
        {
            return _context.Usuarios.ToList();
        }

        public Usuario Obtener(int id)
        {
            return _context.Usuarios
                .FirstOrDefault(x => x.IdUsuario == id);
        }

        public void Insertar(Usuario u)
        {
            _context.Usuarios.Add(u);

            _context.SaveChanges();
        }

        public void Actualizar(Usuario u)
        {
            _context.Usuarios.Update(u);

            _context.SaveChanges();
        }

        public bool ExisteUsername(string username)
        {
            return _context.Usuarios
                .Any(x => x.Username == username);
        }

        public bool ExisteCorreo(string correo)
        {
            return _context.Usuarios
                .Any(x => x.CorreoElectronico == correo);
        }
    }
}