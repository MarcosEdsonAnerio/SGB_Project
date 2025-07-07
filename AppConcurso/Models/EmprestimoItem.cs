using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGB_Project.Models
{
    [Table("EmprestimoItens")]
    public class EmprestimoItem
    {
        // Chaves que formam a chave primária composta
        public int IdEmprestimo { get; set; }
        public int IdLivro { get; set; }

        // Propriedades de Navegação
        [ForeignKey("IdEmprestimo")]
        public virtual Emprestimo? Emprestimo { get; set; }

        [ForeignKey("IdLivro")]
        public virtual Livro? Livro { get; set; }
    }
}