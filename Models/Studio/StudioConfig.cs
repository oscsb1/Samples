using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class StudioConfig :BaseModel
    {
        public string HdrContratoAproval { get; set; }
        public string MsgPacoteHoras { get; set; }
        public string FooterContratoAproval { get; set; }
        public string StatusAulaAgendada { get; set; }
        public string StatusAulaCancelada { get; set; }
        public string StatusAulaExecutada { get; set; }
        public string StatusAulaNaoProgramada { get; set; }
        public string StatusAulaProgramada { get; set; }
        public string StatusAulaReAgendamento { get; set; }
        public string StatusAulaFaltaSemReagendamento { get; set; }
        public string StatusAulaReserva { get; set; }
        public string CorAlunoHorarioFixo { get; set; }
        public string CorAgendaProfessor { get; set; }
        public int PlanoContaIdProfessor { get; set; }
        public int PlanoContaIdAluno { get; set; }
        public int PlanoContaDefaultDespesaId { get; set; }
        public int PlanoContaDefaultReceitaId { get; set; }
        public int RelacionamentoDefaultId { get; set; }
        public int PlanoContaDefaultAporteId { get; set; }
        public int PlanoContaDefaultDividendoId { get; set; }
        public int ModeloAgenda { get; set; } = 1;

    }
}
