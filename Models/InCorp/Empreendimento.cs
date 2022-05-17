using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using InCorpApp.Models;

namespace InCorpApp.Models
{


    public class Empreendimento : BaseModel
    {

        public Empresa Empresa { get; set; }
        public int EmpresaId { get; set; }

        [Required(ErrorMessage = "{0} obrigatório" )]
        [Display(Name = "Nome")]
        [StringLength(80, MinimumLength = 5, ErrorMessage = "{0} tamanho deve ser entre {2} and {1}")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "Razão Social")]
        [StringLength(80, MinimumLength = 5, ErrorMessage = "{0} tamanho deve ser entre {2} and {1}")]
        public string RazaoSocial { get; set; }

        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "CNPJ")]
        [CNPJ(ErrorMessage = "CNPJ inválido")]
        public string CNPJ { get; set; }

        [Display(Name = "Codigo para integração")]
        public string CodigoExterno { get; set; } = string.Empty;

        [Display(Name = "Situação")]
        public StatusEmpreendimento Status { get; set; }
        [DisplayName("Data de inicio de operação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataInicioOperacao { get; set; }
        [DisplayName("Data término deoperação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [AllowNull]
        public DateTime DataFimOperacao { get; set; }
        public bool ExibirAging { get; set; }
        public bool ExibirEspelhoVendas { get; set; }
        public bool Casa { get; set; }
        public bool Apto { get; set; }
        public bool Lote { get; set; }
        public int LogoFileId { get; set; }
        public int PlantaFontSize { get; set; } = 11;
        public int PlantaCirculoSize { get; set; } = 10; 

    }
}
