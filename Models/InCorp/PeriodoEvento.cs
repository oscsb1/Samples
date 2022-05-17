using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{


    public class PeriodoEvento: BaseModel
    {

        public int PeriodoId { get; set; }
        public Periodo Periodo { get; set; }

        public int EventoLogId { get; set; }
        public EventoLog EventoLog { get; set; }

    }

}
