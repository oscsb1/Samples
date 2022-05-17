

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCorpApp.Models
{
    public class PeriodoReport:BaseModel
    {
        public int PeriodoId { get; set; }
        public Periodo Periodo { get; set; }
        public string NomeReport { get; set; }
        public int FileId { get; set; }
        public bool EnviarComoAnexo { get; set; }
        [NotMapped]
        public bool ShowReport { get; set; } = false;
        [NotMapped]
        public List<PeriodoReportEmpSocio> EmpSocios { get; set; } = new();
    }

    public class PeriodoReportEmpSocio :BaseModel
    {
        public int PeriodoReportId { get; set; }
        public PeriodoReport PeriodoReport { get; set; }
        public int EmpreendimentoSocioId { get; set; }
        public EmpreendimentoSocio EmpreendimentoSocio { get; set; }
        [NotMapped]
        public string NomeSocio { get; set; }
        [NotMapped]
        public bool Saving { get; set; } = false;
        [NotMapped]
        public int EmpreendimentoId { get; set; }
    }
}
