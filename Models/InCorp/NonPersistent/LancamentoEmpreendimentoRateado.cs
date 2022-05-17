using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{

    public class LancamentoEmpreendimentoRateado 
    {
        public LanctoEmpreendimento LanctoEmpreendimento { get; set; }
        public List<LanctoEmpreendimentoRateadoSocio> RateioSocios { get; set; } = new List<LanctoEmpreendimentoRateadoSocio>();
    }

    public class LancamentoEmpreendimentoPlanoContaRateado
    {
        public LanctoEmpreendimento LanctoEmpreendimento { get; set; }
        public List<LanctoEmpRateadoPlanoContaSocio> RateioPlanoContaSocios { get; set; } = new List<LanctoEmpRateadoPlanoContaSocio>();
    }

}
