
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using InCorpApp.Constantes;

namespace InCorpApp.Models
{

    public class EmpSocioContaCorrenteView
    {
        public int ContaCorrenteId { get; set; }
        public string NomeConta { get; set; }
        public bool Vissualizar { get; set; }
        public bool Processando { get; set; } = false;
    }
    public class SocioView
    {
        public int Id { get; set; }
        public string Email { get; set; }
        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "{0} tamanho deve ser entre {2} and {1}")]
        public string Nome
        {
            get => EmpSocio.Nome; set { EmpSocio.Nome = value; }
        }
        [Display(Name = "Razão Social")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "{0} tamanho deve ser entre {2} and {1}")]
        public string RazaoSocial
        {
            get => EmpSocio.RazaoSocial; set { EmpSocio.RazaoSocial = value; }
        }
        [Display(Name = "Tipo")]
        [Required(ErrorMessage = "{0} obrigatório")]
        public int Tipo
        {
            get => EmpSocio.Tipo; set { EmpSocio.Tipo = value; } 
        }
        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "CPF/CNPJ")]
        [CNPJ(ErrorMessage = "CPF/CNPJ inválido")]
        public string CPFCNPJ
        {
            get => EmpSocio.CPFCNPJ; set { EmpSocio.CPFCNPJ = value; }
        }
        public bool AplicarCorrecaoDistRetida { get => EmpSocio.AplicarCorrecaoDistRetida; set { EmpSocio.AplicarCorrecaoDistRetida = value; } }
        public bool CorrecaoAutomatica { get => EmpSocio.CorrecaoAutomatica; set { EmpSocio.CorrecaoAutomatica = value; } }
        public TipoIndice TipoIndexador { get => EmpSocio.TipoIndexador; set { EmpSocio.TipoIndexador = value; } }
        public int IndiceEconomicoId { get => EmpSocio.IndiceEconomicoId; set { EmpSocio.IndiceEconomicoId = value; } }
        public double Taxa { get => EmpSocio.Taxa; set { EmpSocio.Taxa = value; } }
        public FormaReceberEmail ReceberEmail { get => EmpSocio.ReceberEmail; set { EmpSocio.ReceberEmail = value; } }
        public Empreendimento Emp { get; set; }
        public EmpreendimentoSocio EmpSocio { get; set; } = new();
        public List<EmpreendimentoSocioParticipacao> Participacoes { get; set; }
        [DisplayFormat(DataFormatString = "{0:F4}")]
        public double Cotas { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataInicio { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataFim { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataInicioDist { get; set; } = Constante.Today;
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataFimDist { get; set; }
        public List<EmpSocioContaCorrenteView> EmpSocioContaCorrentes { get; set; } = new();
        public List<EmpSocioUser> EmpSocioUsers { get; set; } = new();
    }
}
