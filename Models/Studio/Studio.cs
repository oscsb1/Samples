

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCorpApp.Models
{
    public class Studio : BaseModel
    {
        public string Nome { get; set; }
        public string RazaoSocial { get; set; }
        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "CNPJ")]
        [CNPJ(ErrorMessage = "CNPJ inválido")]
        public string CNPJ { get; set; }
        public int AlunoIdDefault { get; set; }

        [NotMapped]
        public List<StudioPlano> StudioPlanos { get; set; } = new();
        [NotMapped]
        public List<ContaCorrente> ContaCorrentes { get; set; } = new();
        [NotMapped]
        public List<StudioSala> Salas { get; set; } = new();
    }

    public class StudioSala : BaseModel
    {
        public int StudioId { get; set; }
        public Studio Studio { get; set; }
        public string Nome { get; set; }
        [NotMapped]
        public string StudioNome { get; set; }

        [NotMapped]
        public string StudioESalaNome => StudioNome + " - " + Nome;
        [NotMapped]
        public bool Saving { get; set; }
        [NotMapped]
        public bool Erro = false;
        [NotMapped]
        public string ErrMsg = string.Empty;
    }
}
