using Microsoft.EntityFrameworkCore;
using SGB_Project.Contexto;
using SGB_Project.Models;

namespace SGB_Project.Controllers
{
    public class LivroService
    {
        private readonly SGB_ProjectContext _context;

        public LivroService(SGB_ProjectContext context)
        {
            _context = context;
        }

        public async Task<List<Livro>> ListarAsync()
        {
            return await _context.Livros.ToListAsync();
        }

        public async Task AdicionarAsync(Livro livro)
        {
            _context.Livros.Add(livro);
            await _context.SaveChangesAsync();
        }

        public async Task<Livro?> BuscarPorIdAsync(int id)
        {
            return await _context.Livros.FindAsync(id);
        }

        public async Task AtualizarAsync(Livro livro)
        {
            // O Entity Framework rastreia a entidade e sabe que ela foi modificada
            _context.Update(livro);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(int id)
        {
            var livro = await _context.Livros.FindAsync(id);
            if (livro != null)
            {
                _context.Livros.Remove(livro);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<string, int>> ContarLivrosPorTopAutoresAsync(int topN = 5)
        {
            var livrosPorAutor = await _context.Livros
                .GroupBy(l => l.Autor)
                .Select(g => new
                {
                    Autor = g.Key,
                    Contagem = g.Count()
                })
                .OrderByDescending(x => x.Contagem)
                .Take(topN)
                .ToDictionaryAsync(x => x.Autor, x => x.Contagem);

            return livrosPorAutor;
        }
    }
}