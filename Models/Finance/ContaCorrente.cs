using InCorpApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{



    public class Banco
    {
        public int Id { get; set; }

        [Display(Name = "Código")]
        [Required(ErrorMessage = "{0} obrigatório")]
        public string Codigo { get; set; }

        [Display(Name = "Nome")]
        [Required(ErrorMessage = "{0} obrigatório")]
        public string Nome { get; set; }

        public string NomeCodigo { get; set; }



    }

    public class ContaCorrente : BaseModel
    {

        [Display(Name = "Nome")]
        [Required(ErrorMessage = "{0} obrigatório")]
        public string Nome { get; set; }
        [Display(Name = "Tipo de conta")]
        [Required(ErrorMessage = "{0} obrigatório")]
        public TipoCCorrente Tipo { get; set; }
        public int BancoId { get; set; }
        [Display(Name = "Agencia")]
        public string Agencia { get; set; } = string.Empty;
        [Display(Name = "Digito agencia")]
        [AllowNull]
        public string DigitoAgencia { get; set; }
        [Display(Name = "Número da conta")]
        public string Numero { get; set; } = string.Empty;
        [Display(Name = "Digito da conta")]
        [AllowNull]
        public string DigitoNumero { get; set; }
        [AllowNull]
        public string CodigoExterno { get; set; }
        [NotMapped]
        public int EmpreendimentoId { get; set; }
        [NotMapped]
        public int StudioId { get; set; }
        [NotMapped]
        public string Banco { get; set; }
        [NotMapped]
        public string TipoCC
        {
            get
            {
                return Tipo switch
                {
                    TipoCCorrente.Carteira => "Caixa",
                    TipoCCorrente.CartaoCred => "Cartão de crédito",
                    TipoCCorrente.CtaCorrente =>"Conta corrente",
                    TipoCCorrente.CtaSocio => "Conta caixa sócio",
                    TipoCCorrente.Investimento => "Conta investimento",
                    TipoCCorrente.Outra => "Outra",
                    _ => "plano invalido"
                };
                ;
            }
            set {; }
        }
        [NotMapped]
        public double Creditos { get; set; }
        [NotMapped]
        public double Debitos { get; set; }
        [NotMapped]
        public double SaldoAtual { get; set; }
        [NotMapped]
        public string StudioNome { get; set; }


    }
    public class ContaCorrenteEmpreendimento : BaseModel
    {
        public int ContaCorrenteId { get; set; }
        public ContaCorrente ContaCorrente { get; set; }
        public int EmpreendimentoId { get; set; }
        public Empreendimento Empreendimento { get; set; }

    }
    public class ContaCorrenteStudio : BaseModel
    {
        public int ContaCorrenteId { get; set; }
        public ContaCorrente ContaCorrente { get; set; }
        public int StudioId { get; set; }
        public Studio Studio { get; set; }

    }
}
