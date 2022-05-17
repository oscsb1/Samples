using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InCorpApp.Models;

namespace InCorpApp.Models
{


    public class LanctoImportado : BaseModel
    {

        public int LanctoLoteImportacaoId { get; set; }
        public LanctoLoteImportacao LanctoLoteImportacao { get; set; }
        [NotMapped]
        public int PeriodoId { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataCompetencia { get; set; }
        public string DataCompetenciaV => DataCompetencia.ToString("dd/MM/yyyy");
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataMovto { get; set; }
        [NotMapped]
        public string DataMovtoV => DataMovto.ToString("dd/MM/yyyy");
        public TipoLancto Tipo { get; set; }
        [NotMapped]
        public string TipoV => Tipo switch
        {
            TipoLancto.Despesa => "Despesa",
            TipoLancto.Receita => "Receita",
            _ => "",
        };
        public int PlanoContaId { get; set; }
        public string PlanoContaV { get; set; } = string.Empty;
        public string PlanoContaCodigoExternoV { get; set; } = string.Empty;
        public int PlanoGerencialId { get; set; }
        public string PlanoGerencialV { get; set; } = string.Empty;
        public string PlanoGerencialCodigoExternoV { get; set; } = string.Empty;
        public string CodigoUnidade { get; set; } = string.Empty;
        public int UnidadeEmpreendimentoId { get; set; } 
        public int RelacionamentoId { get; set; }
        public string RelacionamentoV { get; set; } = string.Empty;
        public string RelacionamentoCodigoExternoV { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Valor { get; set; }

        public string Descricao { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string CodigoExterno { get; set; } = string.Empty;

        public bool Erro { get; set; }
        public string ErroV
        {
            get
            {
                return Erro switch
                {
                    false => "não",
                    true => "sim"
                };
            }
        }

        public string ErroMsg { get; set; } = string.Empty;


        public static string FieldsName()
        {
            return "Tipo R-receita/D-Despesa;Data;Documento;Histórico;Valor;Código plano conta;Plano de conta;Codigo favorecido;Favorecido;Código plano gerencial;Plano de Gerencial;Codigo Integração;Complemento Plano Conta";
        }
        public static string FieldsCaptions()
        {
            return "Tipo R-receita/D-Despesa;Data;Documento;Histórico;Valor;Código plano conta;Plano de conta;Codigo favorecido;Favorecido;Código plano gerencial;Plano de Gerencial;Codigo Integração;Complemento Plano Conta";
        }


    }

}
