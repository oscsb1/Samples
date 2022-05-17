using InCorpApp.Constantes;
using System;
using System.Collections.Generic;

namespace InCorpApp.Models
{

    public class LinhaPeriodoBase
    {
        public int PeriodoId { get; set; }
        public StatusPeriodo Status { get; set; }
        public TipoAlerta Alerta { get; set; } = TipoAlerta.none;
        public DateTime DataInicio { get; set; }
        public string MesAno => DataInicio.ToString("MM/yyyy");
    }

    public class LinhaPeriodo : LinhaPeriodoBase
    {
        public double Percentual { get; set; }
        public string PercentualV => (Percentual * 100).ToString("N4", Constante.Culture) + "%";
        public double Valor { get; set; }
        public double ValorReceita { get; set; }
        public double ValorDespesa { get; set; }
        public double ValorResultado { get; set; }
        public double ValorResultadoAnterior { get; set; }
        public bool Expand { get; set; }
    }
    public class LinhaDistribuicao
    {
        public double ValorAnterior { get; set; }
        public double ValorPeriodo { get; set; }
        public double ValorTotal { get; set; }
        public List<LinhaPeriodo> LinhaPeriodos { get; set; } = new();
        public List<SocioRetiradaDebitoLancto> LinhaDepositos { get; set; } = new();
    }
    public class LinhaRetencao
    {
        public double ValorAnterior { get; set; }
        public double ValorPeriodo { get; set; }
        public double ValorTotal { get; set; }
        public List<LinhaPeriodo> LinhaPeriodos { get; set; } = new();
        public List<SocioDebitoLancto> LinhaDepositos { get; set; } = new();
    }

    public class LinhaCorrecaoDistruibuicao
    {
        protected int _temvalor = 0;
        public double ValorAnterior { get; set; }
        public double ValorPeriodo { get; set; }
        public double ValorTotal { get; set; }
        public List<LinhaPeriodo> LinhaPeriodos { get; set; } = new();
        public List<SocioCorrecaoResultadoRetida> LinhaLanctos { get; set; } = new();
        public bool TemValor
        {
            get
            {
                if (_temvalor == 0)
                {
                    if (ValorAnterior != 0 || ValorPeriodo != 0 || ValorTotal != 0)
                    {
                        _temvalor = 1;
                    }
                    else
                    {
                        _temvalor = 2;
                    }
                }
                return _temvalor == 1;
            }
        }
    }

    public class LinhaEmprestimo
    {
        public bool TemEmprestimo { get; set; } = false;
        public bool Expand { get; set; } = false;
        public double ValorAnteriorTotal => ValorAnterior + ValorAnteriorJuro - ValorAnteriorAmort;
        public double ValorAnterior { get; set; }
        public double ValorAnteriorJuro { get; set; }
        public double ValorAnteriorAmort { get; set; }
        public double ValorTotalTotal { get; set; }
        public double ValorTotal { get; set; }
        public double ValorTotalJuro { get; set; }
        public double ValorTotalAmort { get; set; }
        public List<LinhaPeriodo> LinhaPeriodos { get; set; } = new();
        public List<SocioDebito> Emprestimos { get; set; } = new();
        public List<SocioDebitoLancto> EmprestimosLanctos { get; set; } = new();
    }

    public class LinhaBaseRaiz
    {
        public List<LinhaPeriodoBase> LinhaPeriodos { get; set; } = new();
    }

    public class LinhaBase
    {
        public List<LinhaPeriodo> LinhaPeriodos { get; set; } = new();
    }

    public class LinhaSaldoBase : LinhaBase
    {
        public double ValorAnterior { get; set; }
        public double ValorTotal { get; set; }
    }
    public class LinhaSaldoDistribuir : LinhaSaldoBase
    {
    }
    public class LinhaSaldoCapital : LinhaSaldoBase
    {
    }
    public class LinhaResultado : LinhaSaldoBase
    {
    }

    public class LinhaPlanoConta
    {
        public int PlanoContaId { get; set; }
        public string PlanoContaNome { get; set; }
        public double ValorAnterior { get; set; }
        public double ValorPeriodo { get; set; }
        public double ValorTotal { get; set; }
        public TipoPlanoConta TipoPlanoConta { get; set; }
        public bool Expand { get; set; }
        public List<LinhaPeriodo> LinhaPeriodos { get; set; } = new();
        public List<LanctoEmpreendimento> Lanctos { get; set; }
        public List<LinhaSocio> LanctosRateados { get; set; }
        public List<LanctoEmpRateadoPlanoContaSocio> LanctosRateadosAnalise { get; set; }
    }
    public class LinhaPlanoGerencial
    {
        public int PlanoGerencialId { get; set; }
        public string PlanoGerencialNome { get; set; }
        public string NomeGrupoContaAuxiliar { get; set; }
        public double ValorAnterior { get; set; }
        public double ValorPeriodo { get; set; }
        public double ValorTotal { get; set; }
        public TipoPlanoConta TipoPlanoConta { get; set; }
        public bool AporteDistribuicao { get; set; }
        public bool Expand { get; set; }
        public bool ExpandResult { get; set; }
        public bool ShowMe { get; set; }
        public bool ShowMeResult { get; set; }
        public List<LinhaPeriodo> LinhaPeriodos { get; set; } = new();
        public List<LinhaPlanoConta> PlanoContas { get; set; }
        public List<LinhaContaCorrente> ContasCorrentes { get; set; }
    }
    public class LinhaSocio
    {
        public string SocioNomePrincipal { get; set; } = "Sócios";
        public int SocioNomePrincipalId { get; set; } = 0;
        public int ProgramacaoGrupoRateioId { get; set; } = 0;
        public int EmpreendimentoSocioId { get; set; }
        public string SocioNome { get; set; }
        public TipoSocio TipoSocio { get; set; }
        public double ValorAnteriorReceita { get; set; }
        public double ValorAnteriorDespesa { get; set; }
        public double ValorAnteriorResultado { get; set; }
        public double ValorTotalReceita { get; set; }
        public double ValorTotalDespesa { get; set; }
        public double ValorTotalResultado { get; set; }
        public List<LinhaPeriodo> LinhaPeriodos { get; set; } = new();
        public bool Expand { get; set; }
        public bool ExpandResult { get; set; }
        public bool ShowMe { get; set; } = false;
        public bool ShowMeDemonstrativo { get; set; } = false;
        public bool ExpandDemonstrativo { get; set; } = false;

        public List<LinhaPlanoGerencial> LinhasReceitas { get; set; } = new();
        public List<LinhaPlanoGerencial> LinhasDespesas { get; set; } = new();
        public List<LinhaPlanoGerencial> LinhasGerencial { get; set; } = new();

        public LinhaDistribuicao LinhaDistribuicao { get; set; } = new();
        public LinhaAporte LinhaAporte { get; set; } = new();
        public LinhaRetencao LinhaRetencao { get; set; } = new();
        public LinhaCorrecaoDistruibuicao LinhaCorrecaoDistruibuicao { get; set; } = new();
        public LinhaSaldoDistribuir LinhaSaldoDistribuir { get; set; } = new();
        public LinhaSaldoCapital LinhaSaldoCapital { get; set; } = new();
        public LinhaResultado LinhaResultado { get; set; } = new();
        public LinhaEmprestimo LinhaEmprestimo { get; set; } = new();

    }
    public class LinhaAporte
    {
        public double ValorAnterior { get; set; }
        public double ValorPeriodo { get; set; }
        public double ValorTotal { get; set; }
        public List<LinhaPeriodo> LinhaPeriodos { get; set; } = new();
    }
    public class LinhaRetirada
    {
        public int Seq { get; set; } = 10;
        public int EmpreendimentoSocioId { get; set; }
        public string SocioNome { get; set; }
        public double ValorAnterior { get; set; }
        public double ValorPeriodo { get; set; }
        public double ValorTotal { get; set; }
        public bool Expand { get; set; }
        public List<SocioRetiradaLancto> Depositos { get; set; } = new();
    }

    public class LinhaContaCorrente
    {
        public int ContaCorrenteId { get; set; }
        public string Nome { get; set; }
        public double SaldoAnterior { get; set; }
        public double EntradasAnterior { get; set; }
        public double SaidasAnterior { get; set; }
        public double TransferenciasAnterior { get; set; }
        public double SaldoAnterioFinal => EntradasAnterior + SaidasAnterior + TransferenciasAnterior;
        public double SaldoFinal { get; set; }
        public double EntradasFinal { get; set; }
        public double SaidasFinal { get; set; }
        public double TransferenciasFinal { get; set; }
        public bool TemExtrato { get; set; }
        public double SaldoFinalFinal => EntradasFinal + SaidasFinal + TransferenciasFinal;
        public List<LinhaMovtoContaCorrente> LinhasMovto { get; set; }

    }

    public class LinhaMovtoContaCorrente
    {
        public int PeriodoId { get; set; }
        public DateTime DataInicio { get; set; }
        public double Entradas { get; set; }
        public double Saidas { get; set; }
        public double Transferencias { get; set; }
        public double SaldoAnterior { get; set; }
        public double SaldoFinal { get; set; }
        public int FileId { get; set; }
        public bool Expand { get; set; }
    }
}
