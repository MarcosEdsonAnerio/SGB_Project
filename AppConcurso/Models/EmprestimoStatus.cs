using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGB_Project.Models
{
    [Table("EmprestimoStatus")]
    public class EmprestimoStatus
    {
        [Key]
        public int IdEmprestimo { get; set; }
        
        public DateTime DataDevolucao { get; set; }
        
        // Propriedade de Navegação para o Emprestimo
        [ForeignKey("IdEmprestimo")]
        public virtual Emprestimo? Emprestimo { get; set; }
    }
}
