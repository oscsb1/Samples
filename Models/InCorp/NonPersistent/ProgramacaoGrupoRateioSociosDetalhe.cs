using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class ProgramacaoGrupoRateioSociosDetalhe : BaseModel
    {
        public int GrupoRateioId { get; set; }
        public string NomeRegra { get; set; }
        public int GrupoSocioId { get; set; }
        public string NomeGrupoSocio { get; set; }
        public bool UtilzarPercentualContrato { get; set; }
        public double GrupoSocioPercentual { get; set; }
        public double CotasSocio { get; set; }
        public double TotalCotas { get; set; }
        public double GrupoSocioTotalCotas { get; set; }
        public double EmpreendimentoSocioPercentual { get; set; }

        public int EmpreendimentoSocioId { get; set; }
        public string NomeSocio { get; set; }
        public double PercentualRateio { get; set; }

    }

}
