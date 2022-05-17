using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class ProgramacaoGrupoRateioSociosView 
    {

        public ProgramacaoGrupoRateioSocios ProgramacaoGrupoRateioSocios { get; set; } = new ProgramacaoGrupoRateioSocios();
        public int ProgramacaoGrupoRateioId { get; set; }

        public bool Selecionado { get; set; }
        public GrupoSocio GrupoSocio { get; set; } = new GrupoSocio();
        public int GrupoSocioId { get; set; }
    }
}
