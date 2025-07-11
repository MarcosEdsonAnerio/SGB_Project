using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using SGB_Project.Contexto;
using SGB_Project.Models;

namespace SGB_Project.Controllers
{
    public class UsuarioService
    {
        private readonly SGB_ProjectContext _context;

        public UsuarioService(SGB_ProjectContext context)
        {
            _context = context;
        }

        // Método para cadastrar um novo usuário
        public async Task<bool> CadastrarAsync(Usuario usuario)
        {
            // Verificar se já existe um usuário com o mesmo email
            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == usuario.Email);

            if (usuarioExistente != null)
            {
                return false; // Usuário já existe
            }

            // Criptografar a senha
            usuario.Senha = HashSenha(usuario.Senha);
            usuario.DataCadastro = DateTime.Now;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        // Método para realizar login
        public async Task<Usuario?> LoginAsync(string email, string senha)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.Ativo);

            if (usuario == null)
            {
                return null; // Usuário não encontrado ou inativo
            }

            // Verificar a senha
            if (VerificarSenha(senha, usuario.Senha))
            {
                return usuario; // Login bem-sucedido
            }

            return null; // Senha incorreta
        }

        // Método para obter todos os usuários
        public async Task<List<Usuario>> ListarAsync()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // Método para obter um usuário por ID
        public async Task<Usuario?> ObterPorIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        // Método para atualizar um usuário
        public async Task<bool> AtualizarAsync(Usuario usuario)
        {
            var usuarioExistente = await _context.Usuarios.FindAsync(usuario.IdUsuario);
            if (usuarioExistente == null)
            {
                return false;
            }

            // Atualizar propriedades
            usuarioExistente.Nome = usuario.Nome;
            usuarioExistente.Email = usuario.Email;
            usuarioExistente.TipoUsuario = usuario.TipoUsuario;
            usuarioExistente.Ativo = usuario.Ativo;
            
            // Se uma nova senha foi fornecida, atualize-a
            if (!string.IsNullOrEmpty(usuario.Senha) && usuario.Senha != usuarioExistente.Senha)
            {
                usuarioExistente.Senha = HashSenha(usuario.Senha);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Método para excluir um usuário
        public async Task<bool> ExcluirAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return false;
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        // Método para verificar se o primeiro usuário já foi criado
        public async Task<bool> ExisteUsuarioAsync()
        {
            return await _context.Usuarios.AnyAsync();
        }

        // Método para criar o primeiro usuário administrador
        public async Task CriarPrimeiroUsuarioAsync()
        {
            if (!await ExisteUsuarioAsync())
            {
                var administrador = new Usuario
                {
                    Nome = "Administrador",
                    Email = "admin@biblioteca.com",
                    Senha = HashSenha("admin123"),
                    TipoUsuario = 1, // Administrador
                    Ativo = true,
                    DataCadastro = DateTime.Now
                };

                _context.Usuarios.Add(administrador);
                await _context.SaveChangesAsync();
            }
        }

        // Método para criptografar a senha
        private string HashSenha(string senha)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(senha);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // Método para verificar a senha
        private bool VerificarSenha(string senha, string senhaHash)
        {
            string hashDaSenhaFornecida = HashSenha(senha);
            return hashDaSenhaFornecida == senhaHash;
        }
    }
}
