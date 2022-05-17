using InCorpApp.Interfaces;
using System;
using System.Globalization;
using InCorpApp.Models;
using System.ComponentModel.DataAnnotations.Schema;
using InCorpApp.Constantes;

namespace InCorpApp.Models
{
    public class SocioDebitoLancto : BaseDocument, ISFDocument
    {
        public TipoLanctoDebito TipoLanctoDebito { get; set; }
        public int SocioDebitoId { get; set; }
        public SocioDebito SocioDebito { get; set; }
        public double Valor { get; set; }
        public DateTime DataLancto { get; set; }
        public string Historico { get; set; }
        public int IndiceEconomicoMovotId { get; set; }
        public OrigemLancto Origem { get; set; }
        public string TipoLanctoV
        {
            get
            {
                return TipoLanctoDebito switch
                {
                    TipoLanctoDebito.Juros => "Juros",
                    TipoLanctoDebito.Amortizacao => "Amortização",
                    TipoLanctoDebito.None => "",
                    _ => throw new NotImplementedException()
                };
            }

        }
        public string AssinadoV
        {
            get
            {
                if (Assinado)
                { return "sim"; }
                else
                { return string.Empty; }
            }
        }
        public DateTime DataLimite { get => DataLancto.AddDays(3); }
        public OrigemDocto OrigemDocto
        {
            get
            {
                if (TipoLanctoDebito == TipoLanctoDebito.Juros) { return OrigemDocto.DebitoJuro; } else { return OrigemDocto.DebitoAmortizacao; }
            }
        }
        public string Descricao { get => Historico; }
        public string GUIDLoteImportado { get; set; }
        public string TextoAprovacao
        {
            get => Historico + " " + " valor : " + Valor.ToString("C2", Constante.Culture);
        }
        [NotMapped]
        public string DataDebito
        {
            get { if (SocioDebito != null) { return SocioDebito.DataLancto.ToString("dd/MM/yyyy"); } else { return string.Empty; } }
        }
        [NotMapped]
        public string ValorDebito
        {
            get { if (SocioDebito != null) { return SocioDebito.Valor.ToString("C2", Constante.Culture); } else { return string.Empty; } }
        }
    }
}


