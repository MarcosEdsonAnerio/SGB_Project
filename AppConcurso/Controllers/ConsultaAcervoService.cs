using Microsoft.EntityFrameworkCore;
using SGB_Project.Contexto;
using SGB_Project.Models;

namespace SGB_Project.Controllers
{
    public class ConsultaAcervoService
    {
        private readonly SGB_ProjectContext _context;

        public ConsultaAcervoService(SGB_ProjectContext context)
        {
            _context = context;
        }

        public async Task<List<ConsultaAcervoModel>> ConsultarAsync(string? titulo = null, string? autor = null, bool? apenasDisponiveis = null)
        {
            var query = _context.Livros.AsQueryable();

            if (!string.IsNullOrWhiteSpace(titulo))
                query = query.Where(l => l.Titulo.Contains(titulo));

            if (!string.IsNullOrWhiteSpace(autor))
                query = query.Where(l => l.Autor.Contains(autor));

            if (apenasDisponiveis == true)
                query = query.Where(l => l.Estoque > 0);

            var resultado = await query
                .Select(l => new ConsultaAcervoModel
                {
                    IdLivro = l.IdLivro,
                    Titulo = l.Titulo,
                    Autor = l.Autor,
                    Estoque = l.Estoque
                }).ToListAsync();

            return resultado;
        }
    }
}
