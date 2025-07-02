using Microsoft.EntityFrameworkCore;
using SGB_Project.Contexto;
using SGB_Project.Models;

namespace SGB_Project.Controllers
{
    public class LeitorService
    {
        private readonly SGB_ProjectContext _context;

        public LeitorService(SGB_ProjectContext context)
        {
            _context = context;
        }

        public async Task<List<Leitor>> ListarAsync()
        {
            return await _context.Leitores.ToListAsync();
        }

        public async Task AdicionarAsync(Leitor leitor)
        {
            _context.Leitores.Add(leitor);
            await _context.SaveChangesAsync();
        }

        public async Task<Leitor?> BuscarPorIdAsync(int id)
        {
            return await _context.Leitores.FindAsync(id);
        }

        public async Task AtualizarAsync(Leitor leitor)
        {
            _context.Update(leitor);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(int id)
        {
            var leitor = await _context.Leitores.FindAsync(id);
            if (leitor != null)
            {
                _context.Leitores.Remove(leitor);
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