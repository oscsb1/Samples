using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public enum StudioTipoContaAulaView : int
    {
        Professor = 4,
        Aula = 5,
        TipoPlano =6,
        Aluno = 7,
        Studio = 8
    }

    public class StudioContaBaseView
    {
        public int ContaId { get; set; }
        public string ContaNome { get; set; }
        public bool Expand { get; set; } = false;
    }
    public class StudioPeriodoBaseView
    {
        public DateTime DataRef { get; set; }
        public bool TemMovto { get; set; } = false;
        public string MesAno => DataRef.ToString("MM/yyyy");
    }

    public class StudioPeriodoContaView: StudioPeriodoBaseView
    {
        public double Valor { get; set; }
    }

    public class StudioPeriodoAulaView : StudioPeriodoBaseView
    {
        public int Qtde { get; set; }
    }
    public class StudioPeriodoResultadoView : StudioPeriodoBaseView
    {
        public double Resultado => ReceitaTotalMemosAportes - DespesaTotalMemosDividendo;
        public double ResultadoFinal => ReceitaTotal - DespesaTotal + Aportes - Dividendos;
        public double ReceitaTotal { get; set; }
        public double ReceitaTotalMemosAportes => ReceitaTotal - Aportes;
        public double DespesaTotal { get; set; }
        public double DespesaTotalMemosDividendo => DespesaTotal - Dividendos;
        public double Aportes { get; set; }
        public double Dividendos { get; set; }
    }

    public class StudioPeriodoMovtoBancoView:StudioPeriodoBaseView
    {

        public double SaldoAnterior { get; set; }
        public double Entradas { get; set; }
        public double Saidas { get; set; }
        public double Transferencias { get; set; }
        public double SaldoFinal => SaldoAnterior + Entradas + Saidas + Transferencias;
    }
    public class StudioContaView : StudioContaBaseView
    {
        public TipoPlanoConta Tipo { get; set; }
        public List<StudioPeriodoContaView> Periodos { get; set; } = new();
        public List<StudioContaView> Contas { get; set; } = new();
    }
    public class StudioContaAulaView: StudioContaBaseView
    {
        public StudioTipoContaAulaView TipoConta { get; set; }
        public List<StudioPeriodoAulaView> Periodos { get; set; } = new();
        public List<StudioContaAulaView> Contas { get; set; } = new();
        public List<StudioContaAulaView> Professores { get; set; } = new();
        public List<StudioContaAulaView> Aulas { get; set; } = new();
        public List<StudioContaAulaView> TipoPlano { get; set; } = new();
        public List<StudioContaAulaView> Alunos { get; set; } = new();

    }

    public class StudioResultadoView
    {
        public int StudioId { get; set; }
        public string StudioNome { get; set; }
        public DateTime DtIni { get; set; }
        public DateTime DtFim { get; set; }
        public List<StudioContaView> Contas { get; set; } = new ();
        public List<StudioPeriodoResultadoView> Periodos { get; set; } = new ();
        public List<StudioContaView> Receitas { get; set; }
        public List<StudioContaView> Despesas { get; set; }
        public List<StudioContaView> Auxliares { get; set; }
    }

    public class StudioMovtoBancoContaView: StudioContaBaseView
    {
        public List<StudioPeriodoMovtoBancoView> Periodos { get; set; } = new();
        public List<StudioMovtoBancoContaView> Contas { get; set; } = new();
        public double SaldoAnterior { get; set; }
    }

    public class StudioResultadoMovtoBancoView
    {
        public DateTime DtIni { get; set; }
        public DateTime DtFim { get; set; }
        public List<StudioPeriodoMovtoBancoView> Periodos { get; set; } = new();
        public List<StudioMovtoBancoContaView> Contas { get; set; } = new();
    }

    public class StudioResultadoAulaView
    {
        public DateTime DtIni { get; set; }
        public DateTime DtFim { get; set; }
        public List<StudioPeriodoAulaView> Periodos { get; set; } = new();
        public List<StudioContaAulaView> Contas { get; set; } = new();
    }
}
