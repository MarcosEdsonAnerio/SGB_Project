using System.ComponentModel.DataAnnotations.Schema;

namespace SGB_Project.Models
{
    [NotMapped]
    public class ConsultaAcervoModel
    {
        public int IdLivro { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public int Estoque { get; set; }

        public bool Disponivel => Estoque > 0;
    }
}
