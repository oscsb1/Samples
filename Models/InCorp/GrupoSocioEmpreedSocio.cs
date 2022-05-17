using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class GrupoSocioEmpreedSocio : BaseModel
    {

        public int EmpreendimentoSocioId { get; set; }
        public EmpreendimentoSocio EmpreendimentoSocio { get; set; }
        public int GrupoSocioId { get; set; }
        public GrupoSocio GrupoSocio { get; set; }
        [DisplayFormat(DataFormatString = "{0:F4}")]
        public float Percentual { get; set; }


    }

}
