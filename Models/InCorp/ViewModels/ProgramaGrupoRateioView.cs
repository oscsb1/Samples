using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class ProgramaGrupoRateioView
    {
        public ProgramacaoGrupoRateio ProgramacaoGrupoRateio { get; set; } = new ProgramacaoGrupoRateio();
        public List<ProgramacaoGrupoRateioSociosView> ProgramacaoGrupoRateioSociosViews { get; set; } = new List<ProgramacaoGrupoRateioSociosView>();
    }
}
