// Local: /Contexto/SGB_ProjectContext.cs
using Microsoft.EntityFrameworkCore;
using SGB_Project.Models;

namespace SGB_Project.Contexto
{
    public class SGB_ProjectContext : DbContext
    {
        public SGB_ProjectContext(DbContextOptions<SGB_ProjectContext> options) : base(options)
        {
        }

        // Mapeia cada modelo para uma tabela no banco de dados
        public DbSet<Leitor> Leitores { get; set; }
        public DbSet<Livro> Livros { get; set; }
        public DbSet<Emprestimo> Emprestimos { get; set; }
        public DbSet<EmprestimoItem> EmprestimoItens { get; set; }

        // Adição necessária para o nosso projeto
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configura a chave primária composta da tabela associativa "EmprestimoItens"
            modelBuilder.Entity<EmprestimoItem>()
                .HasKey(ei => new { ei.IdEmprestimo, ei.IdLivro });
        }
    }
}