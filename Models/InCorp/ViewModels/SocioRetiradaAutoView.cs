using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Constantes;
using InCorpApp.Utils;

namespace InCorpApp.Models
{
    public class SocioRetiradaAutoView
    {
        public int EmpreendimentoSocioId { get; set; }
        public string Nome { get; set; }
        public double ValorAportesAcumulado { get; set; }
        public double ValorResultadoAcumulado { get; set; }
        public double ValorRetiradaAcumulado { get; set; }
        public double ValorRetiradaDepositado { get; set; }
        public double ValorDebitoAcumulado { get; set; }
        public double ValorDebitoAmortizado { get; set; }
        public double ValorRetiradaAutorizado { get; set; }
        public double ValorAmortizarDebito { get; set; }
        public DateTime DataPagamento { get; set; } = Constante.Today;
        public DateTime DataLiberacao { get; set; } = UtilsClass.GetInicio(Constante.Today).AddDays(-1);
        public double ValorRetiradaDisponivel { get => Math.Round( ValorAportesAcumulado + ValorResultadoAcumulado - ValorRetiradaAcumulado,2); }
        public double ValorDebitoPendente { get => Math.Round( ValorDebitoAcumulado - ValorDebitoAmortizado, 2); }
        public bool Erro { get; set; }
        public string ErrMsg { get; set; }
    }
}
