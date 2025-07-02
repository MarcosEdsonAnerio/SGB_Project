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

        // Propriedade de Navegação para o Leitor
        [ForeignKey("IdLeitor")]
        public Leitor Leitor { get; set; }

        // Propriedade de Navegação para os vários itens do empréstimo
        public ICollection<EmprestimoItem> EmprestimoItens { get; set; }
    }
}