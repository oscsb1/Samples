using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class PlanoContaRegraRateio
    {
        public int PlanoContaId { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int GrupoRateioId { get; set; } = 0;
        public int ProgramacaoGrupoRateioId { get; set; }


    }

    public class ProgramacaoGrupoRegraTemp
    {
        public int ProgramacaoGrupoRateioId { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public List<ProgramacaoGrupoRateioSociosDetalhe> Detalhes { get; set; } = new ();
    }
}
