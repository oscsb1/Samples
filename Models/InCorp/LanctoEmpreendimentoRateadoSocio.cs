using InCorpApp.Constantes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class LanctoEmpreendimentoRateadoSocio : BaseModel
    {
        public int LanctoEmpreendimentoId { get; set; }
        public LanctoEmpreendimento LanctoEmpreendimento { get; set; }
        public int GrupoRateioId { get; set; }
        public GrupoRateio GrupoRateio { get; set; }
        public int GrupoSocioId { get; set; }
        public GrupoSocio GrupoSocio { get; set; }
        public int ProgramacaoGrupoRateioId { get; set; }
        public ProgramacaoGrupoRateio ProgramacaoGrupoRateio { get; set; }
        public int EmpreendimentoSocioId { get; set; }
        public EmpreendimentoSocio EmpreendimentoSocio { get; set; }
        [DisplayFormat(DataFormatString = "{0:F4}")]
        public double Percentual { get; set; }
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Valor { get; set; }
        [NotMapped]
        public Socio Socio { get; set; }
    }

    public class LanctoEmpRateadoPlanoContaSocio : BaseModel
    {
        public int PeriodoId { get; set; }
        public Periodo Periodo { get; set; }
        public int PlanoContaId { get; set; }
        public PlanoConta PlanoConta { get; set; }
        public int GrupoRateioId { get; set; }
        public GrupoRateio GrupoRateio { get; set; }
        public int GrupoSocioId { get; set; }
        public GrupoSocio GrupoSocio { get; set; }
        public int ProgramacaoGrupoRateioId { get; set; }
        public ProgramacaoGrupoRateio ProgramacaoGrupoRateio { get; set; }
        public int EmpreendimentoSocioId { get; set; }
        public EmpreendimentoSocio EmpreendimentoSocio { get; set; }
        [DisplayFormat(DataFormatString = "{0:F4}")]
        public double Percentual { get; set; }
        [NotMapped]
        public string PercentualV => (Percentual * 100).ToString("N4", Constante.Culture) + "%";
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Valor { get; set; }
        [NotMapped]
        public Socio Socio { get; set; }

        [NotMapped]
        public List<LanctoEmpreendimento> LanctosEmpreendimento { get;set;}

    }



}
