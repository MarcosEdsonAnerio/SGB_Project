using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGB_Project.Models
{
    [Table("Emprestimos")]
    public class Emprestimo
    {
        [Key]
        public int IdEmprestimo { get; set; }

        public int IdLeitor { get; set; } // Chave estrangeira

        public DateTime DataEmprestimo { get; set; }
        
        [NotMapped] // This tells EF Core to ignore this property when interacting with the database
        public DateTime? DataDevolucao { get; set; } // Data de devolução (null = pendente)

        // Propriedade de Navegação para o Leitor
        [ForeignKey("IdLeitor")]
        public virtual Leitor? Leitor { get; set; }

        // Propriedade de Navegação para os vários itens do empréstimo
        public virtual ICollection<EmprestimoItem> EmprestimoItens { get; set; } = new List<EmprestimoItem>();
    }
}
