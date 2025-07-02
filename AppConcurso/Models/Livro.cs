using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGB_Project.Models
{
    [Table("Livros")]
    public class Livro
    {
        [Key]
        public int IdLivro { get; set; }

        [Required]
        public string Titulo { get; set; }

        [Required]
        public string Autor { get; set; }

        public int Estoque { get; set; }
    }
}