using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class LanctoEmpreendimentoLanctoImp : BaseModel
    {

        public int LanctoEmpreendimentoId { get; set; }
        public LanctoEmpreendimento LanctoEmpreendimento { get; set; }
        public int LanctoImportadoId { get; set; }
        public LanctoImportado LanctoImportado { get; set; }

    }

}
