using Microsoft.EntityFrameworkCore;
using SGB_Project.Contexto;
using SGB_Project.Models;

namespace SGB_Project.Controllers
{
    public class EmprestimoItemService
    {
        private readonly SGB_ProjectContext _context;

        public EmprestimoItemService(SGB_ProjectContext context)
        {
            _context = context;
        }

        public async Task<List<EmprestimoItem>> ListarAsync()
        {
            return await _context.EmprestimoItens.ToListAsync();
        }

        public async Task AdicionarAsync(EmprestimoItem emprestimoItem)
        {
            _context.EmprestimoItens.Add(emprestimoItem);
            await _context.SaveChangesAsync();
        }

        // A busca precisa da chave composta (IdEmprestimo e IdLivro)
        public async Task<EmprestimoItem?> BuscarPorIdAsync(int idEmprestimo, int idLivro)
        {
            return await _context.EmprestimoItens.FindAsync(idEmprestimo, idLivro);
        }

        public async Task AtualizarAsync(EmprestimoItem emprestimoItem)
        {
            _context.Update(emprestimoItem);
            await _context.SaveChangesAsync();
        }

        // A remoção também precisa da chave composta
        public async Task RemoverAsync(int idEmprestimo, int idLivro)
        {
            var emprestimoItem = await _context.EmprestimoItens.FindAsync(idEmprestimo, idLivro);
            if (emprestimoItem != null)
            {
                _context.EmprestimoItens.Remove(emprestimoItem);
                await _context.SaveChangesAsync();
            }
        }
    }
}