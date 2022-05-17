using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class EmpreendimentoSocioParticipacao:BaseModel
    {
        public int EmpreendimentoSocioId { get; set; }

        public EmpreendimentoSocio EmpreendimentoSocio { get; set; }

        [DisplayFormat(DataFormatString = "{0:F4}")]
        public double Cotas { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataInicio { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataFim { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataInicioDist { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataFimDist { get; set; }

    }
}
