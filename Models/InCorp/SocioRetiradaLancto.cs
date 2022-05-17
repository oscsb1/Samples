using InCorpApp.Interfaces;
using System;
using System.Globalization;
using InCorpApp.Models;
using InCorpApp.Constantes;

namespace InCorpApp.Models
{
    public class SocioRetiradaLancto : BaseDocument, ISFDocument
    {
        public int SocioRetiradaId { get; set; }
        public SocioRetirada SocioRetirada { get; set; }
        public DateTime DataDeposito { get; set; }
        public double Valor { get; set; }
        public int FileId { get; set; }
        public string Descricao { get => "Deposito da distribuição"; }
        public DateTime DataLimite { get => DataDeposito.AddDays(3); }
        public OrigemDocto OrigemDocto { get => OrigemDocto.DistribuicaoDeposito; }
        public string TextoAprovacao
        {
            get => " Deposito efetuado em " + DataDeposito.ToString("dd/MM/yyyy") + " no valor de " + Valor.ToString("C2", Constante.Culture);
        }
    }
}
