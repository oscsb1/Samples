using InCorpApp.Constantes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class SocioRetiradaView
    {
        public SocioRetirada SocioRetirada { get; set; }
        public List<SocioRetiradaLancto> Lanctos { get; set; } = new List<SocioRetiradaLancto>();
        public List<SocioDebitoLancto> AmortizarDebitos { get; set; } = new List<SocioDebitoLancto>();
        public string NomeSocio { get; set; }
        public DateTime DataLiberacao => SocioRetirada.DataLiberacao;
        public DateTime DataPrevistaPagamento => SocioRetirada.DataPrevistaPagamento;
        public double Valor { get => SocioRetirada.Valor; }
        public string ValorV { get { return SocioRetirada.Valor.ToString("C2", Constante.Culture); }  set {; } }
        public double ValorAmortizarDebito => AmortizarDebitos.Sum(x => x.Valor);
        public double ValorDepositado => Lanctos.Sum(x => x.Valor);
        public double ValorSaldo { get => (SocioRetirada.Valor - Lanctos.Sum(x => x.Valor) - AmortizarDebitos.Sum(x => x.Valor)); }
        public string ValorSaldoV { get { return ValorSaldo.ToString("C2", Constante.Culture); } set {; } }

        public bool Erro { get; set; } = false;
        public string ErroMsg { get; set; } = string.Empty;
    }

}