using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{

    public enum FormaReceberEmail : int
    {
        NaoReceber = 0,
        Principal = 1,
        Copia = 2,
        CopiaOculta = 3
    }
    public class EmpreendimentoSocio : BaseModel
    {
        public int SocioId { get; set; }
        public Socio Socio { get; set; }
        public int EmpreendimentoId { get; set; }
        public Empreendimento Empreendimento { get; set; } 
        public TipoSocio TipoSocio { get; set; }
        public bool AcessoLiberado { get; set; }
        public bool VisualizarDemonstrativo { get; set; }

        [NotMapped]
        public bool VisualizarDemonstrativoV { get { return !VisualizarDemonstrativo; } set { VisualizarDemonstrativo = !value; } }
        public bool RestringirAcessoCC { get; set; }
        public string CodigoExterno { get; set; }


        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "{0} tamanho deve ser entre {2} and {1}")]
        public string Nome { get; set; } = string.Empty;


        [Display(Name = "Razão Social")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "{0} tamanho deve ser entre {2} and {1}")]
        public string RazaoSocial { get; set; } = string.Empty;

        [Display(Name = "Tipo")]
        [Required(ErrorMessage = "{0} obrigatório")]
        public int Tipo { get; set; } = 1;

        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "CPF/CNPJ")]
        [CNPJ(ErrorMessage = "CPF/CNPJ inválido")]
        public string CPFCNPJ { get; set; } = string.Empty;
        public bool AplicarCorrecaoDistRetida { get; set; }
        public bool CorrecaoAutomatica { get; set; }
        public TipoIndice TipoIndexador { get; set; }
        public int IndiceEconomicoId { get; set; }
        public double Taxa { get; set; }
        public FormaReceberEmail ReceberEmail { get; set; }

        [NotMapped]
        public List<EmpSocioUser> EmpSocioUsers { get; set; }
    }
    public class EmpSocioContaCorrente :BaseModel
    {
        public int EmpreendimentoSocioId { get; set; }
        public EmpreendimentoSocio EmpreendimentoSocio { get; set; }
        public int ContaCorrenteId { get; set; }
        public ContaCorrente ContaCorrente { get; set; }
        public bool Permitir { get; set; }
    }

    public class EmpSocioUser :BaseModel
    {
        public int EmpreendimentoSocioId { get; set; }
        public EmpreendimentoSocio EmpreendimentoSocio { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        public bool Permitir { get; set; }
        public FormaReceberEmail ReceberEmail { get; set; }
    }
}
