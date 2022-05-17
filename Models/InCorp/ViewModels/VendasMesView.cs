using InCorpApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class VendasMesView
    {
        public DateTime DtIni { get; set; }
        public DateTime DtMovto { get { return DtIni; } set { DtIni = value; } }
        public DateTime DtFim => UtilsClass.GetUltimo(DtIni);
        public string MesAno => DtIni.ToString("MM/yyyy");
        public string DiaMesAno => DtMovto.ToString("dd/MM/yyyy");
        public int QtdVendida { get; set; }
        public double ValorVenda { get; set; }
        public int QtdDistrato { get; set; }
        public double ValorReembolso { get; set; }
        public int QtdAtraso { get; set; }
        public int QtdAdimplente { get; set; }
        public int QtdInadimplente { get; set; }
        public double Ate30 { get; set; }
        public double Ate60 { get; set; }
        public double Ate90 { get; set; }
        public double Ate120 { get; set; }
        public double Apos120 { get; set; }
        public int Qtd30 { get; set; }
        public int Qtd60 { get; set; }
        public int Qtd90 { get; set; }
        public int Qtd120 { get; set; }
        public int QtdApos120 { get; set; }
    }
}
