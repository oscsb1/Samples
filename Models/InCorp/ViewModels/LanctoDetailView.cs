using InCorpApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class LanctoDetailView
    {
        public int Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public int PeriodoId { get; set; }
        public int PlanoContaId { get; set; }
        public string PlanoContaV { get; set; } = string.Empty;
        public int RelacionamentoId { get; set; }
        public string RelacionamentoV { get; set; } = string.Empty;
        [Required(ErrorMessage = "{0} obrigatório")]
        public DateTime DataCompetencia { get; set; }
        [Required(ErrorMessage = "{0} obrigatório")]
        public DateTime DataMovto { get; set; }
        [Required(ErrorMessage = "{0} obrigatório")]
        public TipoLancto Tipo { get; set; }
        [Required(ErrorMessage = "{0} obrigatório")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Valor { get; set; }
        [Required(ErrorMessage = "{0} obrigatório")]
        public string Descricao { get; set; } = string.Empty;
        [Required(ErrorMessage = "{0} obrigatório")]
        public string Documento { get; set; } = string.Empty;
        public string CodigoExterno { get; set; } = string.Empty;
        public int UnidadeEmpreendimentoId { get; set; }

        public bool Erro { get; set; }
        public string ErroMsg { get; set; }
        public int LanctoLoteImportacaoId { get; set; }
        public string CodigoUnidade { get; set; } = string.Empty;
        public OrigemLancto Origem { get; set; }
        public string OrigemV => Origem switch
        {
            OrigemLancto.Digitado => "manual",
            OrigemLancto.Importado => "importado",
            OrigemLancto.Sistema => "sistema",
            OrigemLancto.ImportadoCC => "Importado conta corremte",
            _ => throw new NotImplementedException()
        };

    }
}
