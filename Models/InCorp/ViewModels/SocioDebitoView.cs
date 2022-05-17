using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using InCorpApp.Constantes;
using InCorpApp.Models;

namespace InCorpApp.Models

{
    public class SocioDebitoView
    {
        public SocioDebito SocioDebito { get; set; }
        public List<SocioDebitoLancto> Lanctos { get; set; } = new List<SocioDebitoLancto>();
        public string NomeSocio { get; set; }
        public DateTime DataLancto => SocioDebito.DataLancto;
        public DateTime DataVencto => SocioDebito.DataVencto;
        public DateTime DataUltimaCorrecao => SocioDebito.DataUltimaCorrecao;
        public double Valor => SocioDebito.Valor;
        public double Juros => Lanctos.Where(x => x.TipoLanctoDebito == TipoLanctoDebito.Juros).Sum(x => x.Valor);
        public double ValorPago => Lanctos.Where(x => x.TipoLanctoDebito == TipoLanctoDebito.Amortizacao).Sum(x => x.Valor);
        public double ValorSaldo => (SocioDebito.Valor + Lanctos.Where(x => x.TipoLanctoDebito == TipoLanctoDebito.Juros).Sum(x => x.Valor)
            - Lanctos.Where(x => x.TipoLanctoDebito == TipoLanctoDebito.Amortizacao).Sum(x => x.Valor)
            );

        public double GetValorSaldoByData(DateTime dt)
        {
            return SocioDebito.Valor
             + Lanctos.Where(x => x.TipoLanctoDebito == TipoLanctoDebito.Juros && x.DataLancto <= dt).Sum(x => x.Valor)
             - Lanctos.Where(x => x.TipoLanctoDebito == TipoLanctoDebito.Amortizacao && x.DataLancto <= dt).Sum(x => x.Valor);
        }
        public string ValorSaldoV
        {
            get
            {
                return (SocioDebito.Valor + Lanctos.Where(x => x.TipoLanctoDebito == TipoLanctoDebito.Juros).Sum(x => x.Valor)
          - Lanctos.Where(x => x.TipoLanctoDebito == TipoLanctoDebito.Amortizacao).Sum(x => x.Valor)
          ).ToString("C2", Constante.Culture);
            }
            set
            {
                ;
            }
        }
        
        public int SocioDebitoId
        {
            get
            {
                if (SocioDebito == null)
                {
                    SocioDebito = new();
                }
                return SocioDebito.Id;
            }
            set
            {
                if (SocioDebito == null)
                {
                    SocioDebito = new();
                }
                SocioDebito.Id = value;
            }
        }

        public bool Erro { get; set; } = false;
        public string ErroMsg { get; set; } = string.Empty;
    }
}


