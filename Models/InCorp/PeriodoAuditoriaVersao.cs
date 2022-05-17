using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class PeriodoAuditoriaVersao : BaseModel
    {
        public int PeriodoId {get;set;}
        public Periodo Periodo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime DataCancelamento { get; set; }
    }
}
