using System;
using System.Collections.Generic;
using InCorpApp.Models;

namespace InCorpApp.Models
{



    public class PeriodoSocioAuditoriaView
    {
        public PeriodoSocioAuditoria PeriodoSocioAuditoria { get; set; }
        public string Nome { get; set; }
        public string StatusV => PeriodoSocioAuditoria.Status switch
        {
            StatusAuditoriaSocio.Cancelado => "Cancelado",
            StatusAuditoriaSocio.Concluido => "Aprovado",
            StatusAuditoriaSocio.Pendente => "Pendente",
            _ => throw new NotImplementedException()
        };
    }
    public class PeridoAuditoriaVersaoView
    {
        public int PeriodoAuditoriaVersaoId { get; set; }
        public PeriodoAuditoriaVersao PeriodoAuditoriaVersao { get; set; } 
        public List<PeriodoSocioAuditoriaView> PeriodoSocioAuditoriaViews { get; set; }
    }
}
