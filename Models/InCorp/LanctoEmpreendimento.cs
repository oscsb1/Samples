using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InCorpApp.Models;

namespace InCorpApp.Models
{
    public class LanctoEmpreendimento : BaseModel
    {
        public int PeriodoId { get; set; }
        public Periodo Periodo { get; set; }
        public int PlanoContaId { get; set; }
        public PlanoConta PlanoConta { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataCompetencia { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataMovto { get; set; }
        public TipoLancto Tipo { get; set; }
        public OrigemLancto Origem { get; set; }
        public int OrigemId { get; set; }
        public bool Rateado { get; set; }
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Valor { get; set; }
        public string Descricao { get; set; }
        public string Documento { get; set; }
        public string CodigoExterno { get; set; }
        public bool Erro { get; set; }
        public string ErroMsg { get; set; }
        [NotMapped]
        public string TipoV
        {
            get
            {
                return Tipo switch
                {
                    TipoLancto.Receita => "Crédito",
                    TipoLancto.Despesa => "Débito",
                    _ => throw new NotImplementedException()
                };
            }
        }
        [NotMapped]
        public string DataCompetenciaV => DataCompetencia.ToString("dd/MM/yyyy");
        [NotMapped]
        public string DataMovtoV => DataMovto.ToString("dd/MM/yyyy");
        [NotMapped]
        public string RateadoV => Rateado switch
        {
            false => "não",
            true => "sim"
        };
        [NotMapped]
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
        [NotMapped]
        public string OrigemV => Origem switch
        {
            OrigemLancto.Digitado => "manual",
            OrigemLancto.Importado => "importado",
            OrigemLancto.Sistema => "sistema",
            OrigemLancto.ImportadoCC => "Importado conta corremte",
            OrigemLancto.ImportadoLote => "Importação em lote",
            _ => throw new NotImplementedException()
        };
        [NotMapped]
        public string PlanoContaV { get; set; }
        [NotMapped]
        public int RelacionamentoId
        {
            get { return LanctoEmpRelacionamento.RelacionamentoId; }
            set
            {
                LanctoEmpRelacionamento.RelacionamentoId = value;
            }
        }
        [NotMapped]
        public string RelacionamentoNome
        {
            get { return LanctoEmpRelacionamento.RelacionamentoNome; }
            set
            {
                LanctoEmpRelacionamento.RelacionamentoNome = value;
            }
        }
        [NotMapped]
        public LanctoEmpRelacionamento LanctoEmpRelacionamento { get; set; } = new LanctoEmpRelacionamento();
        [NotMapped]
        public int UnidadeEmpreendimentoId
        {
            get { return LanctoEmpUnidade.UnidadeEmpreendimentoId; }
            set
            {
                LanctoEmpUnidade.UnidadeEmpreendimento.Id = value;
                LanctoEmpUnidade.UnidadeEmpreendimentoId = value;
            }
        }
        [NotMapped]
        public string CodigoUnidade
        {
            get
            {
                if (LanctoEmpUnidade == null)
                {
                    LanctoEmpUnidade = new();
                    LanctoEmpUnidade.UnidadeEmpreendimento.CodigoExterno = string.Empty;
                }
                return LanctoEmpUnidade.UnidadeEmpreendimento.CodigoExterno;
            }
            set
            {
                if (value == null)
                {
                    LanctoEmpUnidade.UnidadeEmpreendimento.CodigoExterno = string.Empty;
                }
                else
                {
                    LanctoEmpUnidade.UnidadeEmpreendimento.CodigoExterno = value;
                }
            }
        }
        [NotMapped]
        public LanctoEmpUnidade LanctoEmpUnidade { get; set; } = new LanctoEmpUnidade();
        [NotMapped]
        public int ContaCorrenteId { get; set; } = 0;
        [NotMapped]
        public int GrupoRateioId { get; set; } = 0;
    }


    public class LanctoEmpUnidade : BaseModel
    {
        public int LanctoEmpreendimentoId { get; set; }
        public LanctoEmpreendimento LanctoEmpreendimento { get; set; }
        public int UnidadeEmpreendimentoId { get; set; }
        public UnidadeEmpreendimento UnidadeEmpreendimento { get; set; } = new UnidadeEmpreendimento();
    }
    public class LanctoEmpRelacionamento : BaseModel
    {
        public int LanctoEmpreendimentoId { get; set; }
        public int RelacionamentoId { get; set; }
        [NotMapped]
        public string RelacionamentoNome { get; set; } = string.Empty;
    }
}
