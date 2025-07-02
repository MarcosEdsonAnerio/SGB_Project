using Microsoft.EntityFrameworkCore;
using SGB_Project.Contexto;
using SGB_Project.Models;

namespace SGB_Project.Controllers
{
    public class EmprestimoService
    {
        private readonly SGB_ProjectContext _context;

        public EmprestimoService(SGB_ProjectContext context)
        {
            _context = context;
        }

        // Para listar, incluímos os dados do Leitor e os Itens do Empréstimo,
        // pois geralmente são informações úteis.
        public async Task<List<Emprestimo>> ListarAsync()
        {
            return await _context.Emprestimos
                                 .Include(e => e.Leitor) // Inclui os dados do Leitor
                                 .Include(e => e.EmprestimoItens) // Inclui a lista de itens
                                 .ThenInclude(ei => ei.Livro) // Dentro dos itens, inclui os dados do Livro
                                 .ToListAsync();
        }

        public async Task AdicionarAsync(Emprestimo emprestimo)
        {
            _context.Emprestimos.Add(emprestimo);
            await _context.SaveChangesAsync();
        }

        public async Task<Emprestimo?> BuscarPorIdAsync(int id)
        {
            return await _context.Emprestimos
                                 .Include(e => e.Leitor)
                                 .Include(e => e.EmprestimoItens)
                                 .ThenInclude(ei => ei.Livro)
                                 .FirstOrDefaultAsync(e => e.IdEmprestimo == id);
        }

        public async Task AtualizarAsync(Emprestimo emprestimo)
        {
            _context.Update(emprestimo);
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(int id)
        {
            var emprestimo = await _context.Emprestimos.FindAsync(id);
            if (emprestimo != null)
            {
                _context.Emprestimos.Remove(emprestimo);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<string, int>> ContarEmprestimosUltimos6MesesAsync()
        {
            var hoje = DateTime.Now;
            var seisMesesAtras = hoje.AddMonths(-6);

            var emprestimos = await _context.Emprestimos
                .Where(e => e.DataEmprestimo >= seisMesesAtras)
                .ToListAsync();

            var emprestimosPorMes = emprestimos
                .GroupBy(e => new { e.DataEmprestimo.Year, e.DataEmprestimo.Month })
                .Select(g => new
                {
                    Mes = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Contagem = g.Count()
                })
                .OrderBy(x => x.Mes)
                .ToDictionary(x => x.Mes.ToString("MMM/yy"), x => x.Contagem);

            return emprestimosPorMes;
        }
    }
}