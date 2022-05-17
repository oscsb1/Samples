using InCorpApp.Constantes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCorpApp.Models
{
    public class Periodo : BaseModel
    {
        public TipoAlerta Alerta { get; set; }
        public int EmpreendimentoId { get; set; }
        public Empreendimento Empreendimento { get; set; }
        public StatusPeriodo Status { get; set; }
        public bool DadosLiberado { get; set; }
        [NotMapped]
        public string Nome => DataInicio.ToString("MM/yyyy");
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        [NotMapped]
        public string NomeStatus
        {
            get
            {
                string s = string.Empty;
                switch (Status)
                {
                    case StatusPeriodo.EntradaDeDados:
                        s = "Entrada de dados";
                        break;
                    case StatusPeriodo.Rateio:
                        s = "Validação do rateio";
                        break;
                    case StatusPeriodo.Auditoria:
                        s = "Auditoria";
                        break;
                    case StatusPeriodo.Fechamento:
                        s = "Fechamento";
                        break;

                    case StatusPeriodo.Fechado:
                        s = "Fechado";
                        break;

                }
                return s;
            }
            set { }
        }
        [NotMapped]
        public string ErrMsg { get; set; } = string.Empty;
    }
    public class PeriodoNota : BaseModel
    {
        public int PeriodoId { get; set; }
        public Periodo Periodo { get; set; }
        [NotMapped]
        public int Seq { get; set; }
        public string Nota { get; set; }
        public bool AcessoGeral { get; set; }
        public bool AcessoParceiro { get; set; }
        public bool AcessoInvestidor { get; set; }
        public bool AcessoSocioAdministrador { get; set; }
        [NotMapped]
        public string AcessoGeralV => Constante.GetTextoBool(AcessoGeral);
        [NotMapped]
        public string AcessoParceiroV => Constante.GetTextoBool(AcessoParceiro);
        [NotMapped]
        public string AcessoInvestidorV => Constante.GetTextoBool(AcessoInvestidor);
        [NotMapped]
        public string AcessoSocioAdministradorV => Constante.GetTextoBool(AcessoSocioAdministrador);
    }
}
