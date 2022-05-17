using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public enum TipoLanctoSocioResultado : int
    {
        Correcao = 1,
        Pagamento = 2
    }
    public class SocioResultadoPeriodoLancto
    {
        public int Id { get; set; }
        public TipoLanctoSocioResultado TipoLancto { get; set; }
        public int SocioResultadoPeriodoId { get; set; }
        public int PeriodoId { get; set; }
        public DateTime DataRef { get; set; }
        public double Valor { get; set; }
        public OrigemLancto Origem { get; set; }
        public string Historico { get; set; }
    }
    public class SocioResultadoExtratoPeriodo
    {
        public int SocioResultadoPeriodoId { get; set; }
        public int PeriodoId { get; set; }
        public DateTime DataRef { get; set; }
        public DateTime DataUltimaCorrecao { get; set; }
        public double Taxa { get; set; }
        public double Valor { get; set; }
        public double PrejuizoAcumulado { get; set; } = 0;
        public bool ExpandLancto { get; set; } = false;
        public List<SocioResultadoPeriodoLancto> Lanctos { get; set; } = new();
        public double GetValorByDate(DateTime dt)
        {
            double r = Valor - PrejuizoAcumulado;
            if (Lanctos.Any(x => x.TipoLancto == TipoLanctoSocioResultado.Correcao && x.DataRef <= dt))
            {
                r += Lanctos.Where(x => x.TipoLancto == TipoLanctoSocioResultado.Correcao && x.DataRef <= dt).Sum(x => x.Valor);
            }
            if (Lanctos.Any(x => x.TipoLancto == TipoLanctoSocioResultado.Pagamento && x.DataRef <= dt))
            {
                r -= Lanctos.Where(x => x.TipoLancto == TipoLanctoSocioResultado.Pagamento && x.DataRef <= dt).Sum(x => x.Valor);
            }
            return r;
        }
        public double GetValorDistribuido()
        {
            if (Lanctos.Any(x=> x.TipoLancto == TipoLanctoSocioResultado.Pagamento))
            {
                return Lanctos.Where(x => x.TipoLancto == TipoLanctoSocioResultado.Pagamento).Sum(x => x.Valor);
            }
            else
            {
                return 0;
            }
        }
        public double GetValorJuros()
        {
            if (Lanctos.Any(x => x.TipoLancto == TipoLanctoSocioResultado.Correcao))
            {
                return Lanctos.Where(x => x.TipoLancto == TipoLanctoSocioResultado.Correcao).Sum(x => x.Valor);
            }
            else
            {
                return 0;
            }
        }
    }
    public class SocioResultadoExtrato
    {
        public EmpreendimentoSocio EmpreendimentoSocio { get; set; }
        public List<SocioRetiradaLancto> Distribuicoes { get; set; } = new();
        public List<SocioResultadoExtratoPeriodo> SocioResultadoExtratoPeriodos { get; set; } = new();
    }

    public class SocioResultado
    {
        public EmpreendimentoSocio EmpreendimentoSocio { get; set; }
        public SocioResultadoPeriodo SocioResultadoPeriodo { get; set; }
    }
}
