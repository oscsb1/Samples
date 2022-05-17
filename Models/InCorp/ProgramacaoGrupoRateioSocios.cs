using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class ProgramacaoGrupoRateioSocios : BaseModel
    {
        public int ProgramacaoGrupoRateioId { get; set; }
        public ProgramacaoGrupoRateio ProgramacaoGrupoRateio { get; set; }

        public int GrupoSocioId { get; set; }
        public GrupoSocio GrupoSocio { get; set; }


        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Percentual { get; set; }

    }

}
