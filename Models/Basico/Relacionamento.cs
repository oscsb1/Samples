using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Constantes;

namespace InCorpApp.Models
{
    public class Relacionamento : BaseModel
    {


        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;


        [Display(Name = "Razão Social")]
        public string RazaoSocial { get; set; } = string.Empty;

        [Display(Name = "Tipo")]
        public int Tipo { get; set; }

        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "CPF/CNPJ")]
        [CNPJ(ErrorMessage = "CPF/CNPJ inválido")]
        public string CPFCNPJ { get; set; } = string.Empty;
        [EmailAddress(ErrorMessage = "{0} inválido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Telefone")]
        public string Telefone { get; set; } = string.Empty;

        public int Sexo { get; set; }
        public bool Ativo { get; set; }

        public DateTime DataNascimento { get; set; }
        public bool EhCliente { get; set; }
        public bool EhFornecedor { get; set; }

        [Display(Name = "Codigo para integraçao cliente")]
        public string CodigoExternoCliente { get; set; } = string.Empty;

        [Display(Name = "Codigo para integraçao fornecedor")]
        public string CodigoExternoFornecedor { get; set; } = string.Empty;
        public string CFA
        {
            get
            {
                if (EhFornecedor && EhCliente)
                { return "Ambos"; }
                else
                if (EhCliente)
                { return "Cliente"; }
                else
                { return "Fornecedor"; };
            }
        }

        public int PlanoContaId { get; set; }
        public static string FieldsName()
        {
            return "C-Cliente/F-Fornecedor/A-Ambos;Nome;F-Fisica/J-Juridica;Razão Social;CPF/CNPJ;Telefone;Email;Cod.Integração cli;Cod.Integração for";
        }
        public static string FieldsCaptions()
        {
            return "C-Cliente/F-Fornecedor/A-Ambos;Nome;F-Fisica/J-Juridica;Razão Social;CPF/CNPJ;Telefone;Email;Cod.Integração cli;Cod.Integração for";
        }

        [NotMapped]
        public double Idade { get => Constante.Today.Subtract(DataNascimento).TotalDays / 365; }

        [NotMapped]
        public int RelacionamentoFaturaId { get; set; }
        public bool Sistema { get; set; } = false;

    }

    public class Cliente : Relacionamento
    {

    }

    public class Fornecedor : Relacionamento
    {

    }



}