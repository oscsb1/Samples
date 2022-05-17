using InCorpApp.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using InCorpApp.Models;
using InCorpApp.Constantes;

namespace InCorpApp.Models
{
    public class SocioResultadoPeriodo : BaseDocument, ISFDocument
    {
        public bool Cancelado { get; set; }
        public new int PeriodoId { get; set; }
        public Periodo Periodo { get; set; }
        public DateTime DataMovto { get; set; }
        public DateTime DataUltimaCorrecao { get; set; }
        public double Valor { get; set; }
        public string Historico { get; set; }
        public DateTime DataCancelamento { get; set; }
        public DateTime DataLimite { get => DataMovto.AddDays(3); }
        [NotMapped]
        public OrigemDocto OrigemDocto { get => OrigemDocto.ResultadoPeriodo;  } 
        public string Descricao { get => Historico; }
        public string TextoAprovacao
        {
            get => Historico + " " + " valor : " + Valor.ToString("C2", Constante.Culture);
        }

    }

    public class SocioCorrecaoResultadoRetida : BaseModel
    {
        public OrigemLancto Origem { get; set; }
        public int SocioResultadoPeriodoId { get; set; }
        public SocioResultadoPeriodo SocioResultadoPeriodo { get; set; }
        public DateTime DataRef { get; set; }
        public double Valor { get; set; }
        public string Historico { get; set; }
    }
}


