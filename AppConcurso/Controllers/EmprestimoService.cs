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
            try
            {
                // Since DataDevolucao is NotMapped, EF Core won't try to query it from the database
                var emprestimos = await _context.Emprestimos
                                  .Include(e => e.Leitor)
                                  .Include(e => e.EmprestimoItens)
                                  .ThenInclude(ei => ei.Livro)
                                  .ToListAsync();
                
                try
                {
                    // Now get the status information from EmprestimoStatus table
                    var statusList = await _context.EmprestimoStatus.ToListAsync();
                    var statusDict = statusList.ToDictionary(s => s.IdEmprestimo);
                    
                    // Update the in-memory DataDevolucao property
                    foreach (var emprestimo in emprestimos)
                    {
                        if (statusDict.TryGetValue(emprestimo.IdEmprestimo, out var status))
                        {
                            emprestimo.DataDevolucao = status.DataDevolucao;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If the EmprestimoStatus table doesn't exist or there's another error,
                    // just continue without setting DataDevolucao
                    Console.WriteLine($"Error loading EmprestimoStatus: {ex.Message}");
                }
                
                return emprestimos;
            }
            catch
            {
                // If that fails, try a simpler query without includes
                try
                {
                    var emprestimosSimples = await _context.Emprestimos.ToListAsync();
                    
                    // Load the navigation properties manually
                    foreach (var emprestimo in emprestimosSimples)
                    {
                        try
                        {
                            await _context.Entry(emprestimo).Reference(e => e.Leitor).LoadAsync();
                            await _context.Entry(emprestimo).Collection(e => e.EmprestimoItens).LoadAsync();
                            
                            foreach (var item in emprestimo.EmprestimoItens)
                            {
                                await _context.Entry(item).Reference(ei => ei.Livro).LoadAsync();
                            }
                        }
                        catch
                        {
                            // If loading navigation properties fails, continue with what we have
                        }
                    }
                    
                    // Get status information
                    var statusList = await _context.EmprestimoStatus.ToListAsync();
                    var statusDict = statusList.ToDictionary(s => s.IdEmprestimo);
                    
                    // Update the in-memory DataDevolucao property
                    foreach (var emprestimo in emprestimosSimples)
                    {
                        if (statusDict.TryGetValue(emprestimo.IdEmprestimo, out var status))
                        {
                            emprestimo.DataDevolucao = status.DataDevolucao;
                        }
                    }
                    
                    return emprestimosSimples;
                }
                catch
                {
                    // If all fails, return an empty list rather than crashing
                    return new List<Emprestimo>();
                }
            }
        }

        public async Task AdicionarAsync(Emprestimo emprestimo)
        {
            _context.Emprestimos.Add(emprestimo);
            await _context.SaveChangesAsync();
        }

        public async Task<Emprestimo?> BuscarPorIdAsync(int id)
        {
            var emprestimo = await _context.Emprestimos
                                 .Include(e => e.Leitor)
                                 .Include(e => e.EmprestimoItens)
                                 .ThenInclude(ei => ei.Livro)
                                 .FirstOrDefaultAsync(e => e.IdEmprestimo == id);
                                 
            if (emprestimo != null)
            {
                try
                {
                    var status = await _context.EmprestimoStatus
                        .FirstOrDefaultAsync(s => s.IdEmprestimo == id);
                        
                    if (status != null)
                    {
                        emprestimo.DataDevolucao = status.DataDevolucao;
                    }
                }
                catch (Exception ex)
                {
                    // If the EmprestimoStatus table doesn't exist or there's another error,
                    // just continue without setting DataDevolucao
                    Console.WriteLine($"Error loading EmprestimoStatus: {ex.Message}");
                }
            }
            
            return emprestimo;
        }

        public async Task AtualizarAsync(Emprestimo emprestimo)
        {
            _context.Update(emprestimo);
            await _context.SaveChangesAsync();
            
            // If DataDevolucao is set, update the status table
            if (emprestimo.DataDevolucao.HasValue)
            {
                try
                {
                    // First, let's check if the table exists
                    var tableExists = true;
                    try
                    {
                        // Try to get one row to check if table exists
                        await _context.EmprestimoStatus.FirstOrDefaultAsync();
                    }
                    catch
                    {
                        tableExists = false;
                    }

                    if (tableExists)
                    {
                        var status = await _context.EmprestimoStatus
                            .FirstOrDefaultAsync(s => s.IdEmprestimo == emprestimo.IdEmprestimo);
                            
                        if (status != null)
                        {
                            status.DataDevolucao = emprestimo.DataDevolucao.Value;
                            _context.Update(status);
                        }
                        else
                        {
                            status = new EmprestimoStatus
                            {
                                IdEmprestimo = emprestimo.IdEmprestimo,
                                DataDevolucao = emprestimo.DataDevolucao.Value
                            };
                            _context.Add(status);
                        }
                        
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // If table doesn't exist, try to create it
                        try
                        {
                            // Using raw SQL to create the table
                            await _context.Database.ExecuteSqlRawAsync(@"
                                CREATE TABLE IF NOT EXISTS EmprestimoStatus (
                                    IdEmprestimo INT PRIMARY KEY,
                                    DataDevolucao DATETIME NOT NULL,
                                    FOREIGN KEY (IdEmprestimo) REFERENCES Emprestimos(IdEmprestimo) ON DELETE CASCADE
                                );");
                            
                            // Now try to insert the status
                            var status = new EmprestimoStatus
                            {
                                IdEmprestimo = emprestimo.IdEmprestimo,
                                DataDevolucao = emprestimo.DataDevolucao.Value
                            };
                            _context.Add(status);
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error creating EmprestimoStatus table: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating EmprestimoStatus: {ex.Message}");
                }
            }
        }

        public async Task RemoverAsync(int id)
        {
            try
            {
                // First try to remove any status entry
                try
                {
                    var status = await _context.EmprestimoStatus.FirstOrDefaultAsync(s => s.IdEmprestimo == id);
                    if (status != null)
                    {
                        _context.EmprestimoStatus.Remove(status);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    // If the EmprestimoStatus table doesn't exist or another error occurs,
                    // just continue with removing the emprestimo
                    Console.WriteLine($"Error removing EmprestimoStatus: {ex.Message}");
                }
            
                // Then remove the emprestimo
                var emprestimo = await _context.Emprestimos.FindAsync(id);
                if (emprestimo != null)
                {
                    _context.Emprestimos.Remove(emprestimo);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error removing emprestimo: {ex.Message}");
                throw; // Rethrow to let the caller handle it
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
                    Mes = new DateTime(g.Key.Year, g.Key.Month, 1, 0, 0, 0, DateTimeKind.Local),
                    Contagem = g.Count()
                })
                .OrderBy(x => x.Mes)
                .ToDictionary(x => x.Mes.ToString("MMM/yy"), x => x.Contagem);

            return emprestimosPorMes;
        }

        public async Task<List<EmprestimoPendente>> BuscarEmprestimosPendentesAsync()
        {
            try
            {
                var hoje = DateTime.Now;
                
                // Get all emprestimos
                var emprestimos = await _context.Emprestimos
                    .Include(e => e.Leitor)
                    .Include(e => e.EmprestimoItens)
                    .ThenInclude(ei => ei.Livro)
                    .ToListAsync();
                
                try
                {
                    // Get the status information
                    var statusList = await _context.EmprestimoStatus.ToListAsync();
                    var statusDict = statusList.ToDictionary(s => s.IdEmprestimo);
                    
                    // Update the in-memory DataDevolucao property
                    foreach (var emprestimo in emprestimos)
                    {
                        if (statusDict.TryGetValue(emprestimo.IdEmprestimo, out var status))
                        {
                            emprestimo.DataDevolucao = status.DataDevolucao;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If the EmprestimoStatus table doesn't exist or there's another error,
                    // just continue without setting DataDevolucao
                    Console.WriteLine($"Error loading EmprestimoStatus: {ex.Message}");
                }
                
                // Now filter for pending (not returned) emprestimos
                var emprestimosPendentes = emprestimos
                    .Where(e => e.DataDevolucao == null) // Check the updated property
                    .Select(e => new EmprestimoPendente
                    {
                        IdEmprestimo = e.IdEmprestimo,
                        NomeLeitor = e.Leitor?.Nome ?? "Desconhecido",
                        DataEmprestimo = e.DataEmprestimo,
                        DiasAtraso = (int)(hoje - e.DataEmprestimo.AddDays(15)).TotalDays, // Considera 15 dias de prazo
                        Livros = e.EmprestimoItens.Select(ei => new LivroEmprestado
                        {
                            IdLivro = ei.IdLivro,
                            Titulo = ei.Livro?.Titulo ?? "Desconhecido",
                            Autor = ei.Livro?.Autor ?? "Desconhecido"
                        }).ToList()
                    })
                    .OrderByDescending(e => e.DiasAtraso) // Mais atrasados primeiro
                    .ToList();

                return emprestimosPendentes;
            }
            catch
            {
                // If all fails, return an empty list
                return new List<EmprestimoPendente>();
            }
        }

        public async Task MarcarComoDevolvido(int idEmprestimo)
        {
            try
            {
                var emprestimo = await _context.Emprestimos.FindAsync(idEmprestimo);
                if (emprestimo != null)
                {
                    // Set the in-memory property for UI updates
                    emprestimo.DataDevolucao = DateTime.Now;
                    
                    try
                    {
                        // First, let's check if the table exists
                        var tableExists = true;
                        try
                        {
                            // Try to get one row to check if table exists
                            await _context.EmprestimoStatus.FirstOrDefaultAsync();
                        }
                        catch
                        {
                            tableExists = false;
                        }

                        if (tableExists)
                        {
                            // Look for existing status
                            var status = await _context.EmprestimoStatus
                                .FirstOrDefaultAsync(s => s.IdEmprestimo == idEmprestimo);
                                
                            if (status != null)
                            {
                                // Update existing status
                                status.DataDevolucao = emprestimo.DataDevolucao.Value;
                                _context.Update(status);
                            }
                            else
                            {
                                // Create new status
                                status = new EmprestimoStatus
                                {
                                    IdEmprestimo = idEmprestimo,
                                    DataDevolucao = emprestimo.DataDevolucao.Value
                                };
                                _context.Add(status);
                            }
                            
                            // Save the status change to the database
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            // If table doesn't exist, try to create it
                            try
                            {
                                // Using raw SQL to create the table
                                await _context.Database.ExecuteSqlRawAsync(@"
                                    CREATE TABLE IF NOT EXISTS EmprestimoStatus (
                                        IdEmprestimo INT PRIMARY KEY,
                                        DataDevolucao DATETIME NOT NULL,
                                        FOREIGN KEY (IdEmprestimo) REFERENCES Emprestimos(IdEmprestimo) ON DELETE CASCADE
                                    );");
                                
                                // Now try to insert the status
                                var status = new EmprestimoStatus
                                {
                                    IdEmprestimo = idEmprestimo,
                                    DataDevolucao = emprestimo.DataDevolucao.Value
                                };
                                _context.Add(status);
                                await _context.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error creating EmprestimoStatus table: {ex.Message}");
                                // If we can't create the table, at least update the UI
                                _context.Entry(emprestimo).State = EntityState.Modified;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating EmprestimoStatus: {ex.Message}");
                        // If all fails, at least update the UI
                        _context.Entry(emprestimo).State = EntityState.Modified;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error marking emprestimo as returned: {ex.Message}");
                throw; // Rethrow to let the caller handle it
            }
        }
    }

    public class EmprestimoPendente
    {
        public int IdEmprestimo { get; set; }
        public string NomeLeitor { get; set; } = string.Empty;
        public DateTime DataEmprestimo { get; set; }
        public int DiasAtraso { get; set; }
        public List<LivroEmprestado> Livros { get; set; } = new();
    }

    public class LivroEmprestado
    {
        public int IdLivro { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
    }
}