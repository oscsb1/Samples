using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using InCorpApp.Interfaces;
using InCorpApp.Models;

namespace InCorpApp.Models
{
    public class PeriodoSocioAuditoria : BaseDocument, ISFDocument
    {

        public int PeriodoAuditoriaVersaoId { get; set; }
        public PeriodoAuditoriaVersao PeriodoAuditoriaVersao { get; set; }

        public StatusAuditoriaSocio Status { get; set; }
        [DisplayName("Data conclusão")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataConclusao { get; set; }
        public string Descricao { get; set; }
        public OrigemDocto OrigemDocto { get => OrigemDocto.Auditoria; }
        public double Valor { get => 0; }
        public DateTime DataLimite { get => DataCriacao.AddDays(10); }

        public string TextoAprovacao
        {
            get  { if (Descricao == null || Descricao == string.Empty) { return "As informações estão corretas."; } else { return Descricao; } }
        }


    }
}
