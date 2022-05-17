using InCorpApp.Interfaces;
using System;
using System.Globalization;
using InCorpApp.Models;
using System.ComponentModel.DataAnnotations;
using InCorpApp.Constantes;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCorpApp.Models
{
    public class SocioAporteDeposito : BaseDocument, ISFDocument
    {
        public int SocioAporteId { get; set; }
        public SocioAporte SocioAporte { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]

        public DateTime DataDeposito { get; set; }
        public double Valor { get; set; }
        public string Historico { get; set; }
        public string Descricao { get => Historico; }
        public DateTime DataLimite { get => DataDeposito; }
        public OrigemDocto OrigemDocto { get => OrigemDocto.AporteDeposito; }
        public string TextoAprovacao
        {
            get => "Aporte depositado em " + DataDeposito.ToString("dd/MM/yyyy") + " no valor de " + Valor.ToString("C2", Constante.Culture);
        }

        public int FileId { get; set; }

    }
}
