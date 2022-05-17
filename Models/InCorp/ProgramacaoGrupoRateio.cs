using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class ProgramacaoGrupoRateio : BaseModel
    {

        public int GrupoRateioId { get; set; }
        public GrupoRateio GrupoRateio { get; set; }


        [DisplayName("Data de início")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DataInicio { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]

        [DisplayName("Data de término")]
        public DateTime DataFim { get; set; }
        [DisplayName("Ativo")]
        public bool Ativo { get; set; }

    }
}
