using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class GrupoRateioPlanoConta : BaseModel
    {

        public int GrupoRateioId { get; set; }
        public GrupoRateio GrupoRateio { get; set; }
        public int PlanoContaId { get; set; }
        public PlanoConta PlanoConta { get; set; }

    }
}
