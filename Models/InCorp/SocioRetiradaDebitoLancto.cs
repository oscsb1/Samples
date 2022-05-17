using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class SocioRetiradaDebitoLancto : BaseModel
    {
        public int SocioDebitoLanctoId { get; set; }
        public SocioDebitoLancto SocioDebitoLancto { get; set; }
        public int SocioRetiradaId { get; set; }
        public SocioRetirada SocioRetirada {get;set;}
    }
}
