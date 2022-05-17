using System;
using System.ComponentModel.DataAnnotations;

namespace InCorpApp.Models
{
    public enum OrigemImnportacaoLanctoEmp : int
    {
        Ambos = 0,
        MovtoBanco = 1,
        MovtoEmpreendimento = 2
    }
    public enum StatusUnidade : int
    {
        None = 0,
        Disponivel = 1,
        Vendida = 2,
        EmDistrato = 3,
        Alugado = 4,
        Bloqueado = 5,
        Reservado = 6
    }
    public enum StatusFinanceiroUnidade : int
    {
        NA = 0,
        Adimplente = 1,
        Atraso = 2,
        Inadimplente = 3
    }
    public enum StatusConciliacao : int
    {
        Pendente = 0,
        Parcial = 1,
        Total = 2
    }
    public enum TituloFrequencia : int
    {
        Unico = 1,
        Mensal = 2
    }
    public enum TipoContrato : int
    {
        Mensal = 1
    }
    public enum ClasseTituloBase : int
    {
        ParcelaStudio = 1,
        ProgramacaoAula = 2,
        TituloPagar = 3,
        TituloReceber = 4,
        LotePagtoProfessor = 5
    }
    public enum TipoTitulo : int
    {
        none = 0,
        ContasReceber = 3,
        ContasPagar = 4
    }
    public enum TituloOrigem : int
    {
        Manual = 1
    }
    public enum TipoMovtoParcela : int
    {
        Baixa = 1,
        Juros = 2,
        Multa = 3,
        Desconta = 4
    }

    public enum TipoSocio : int
    {
        none = 0,
        Parceiro = 1,
        Investidor = 2,
        SocioAdministrador = 3
    }
    public enum TipoAlerta : int
    {
        none = 0,
        Novo = 1,
        Alterado = 2,
        Erro = 3,
        Liberado = 4
    }
    public enum TipoRelacionamento : int
    {
        [Display(Name = "Professor")]
        Professor = 1,
        [Display(Name = "Aluno")]
        Aluno = 2,
        [Display(Name = "Outro")]
        Outra = 3
    }


    public enum TenantApp : int
    {
        Zero = 0,
        InCorp = 1,
        Studio = 2
    }
    public enum TipoPessoa : int
    {
        [Display(Name = "Física")]
        Fisica = 1,
        [Display(Name = "Jurídica")]
        Juridica = 2,
        [Display(Name = "Outras")]
        Outra = 3
    }


    public enum TipoUE : int
    {
        // terreno
        // unidade horizontal
        // unidade vertical

        // tipo comercial
        // tipo residencial
        none = 0,
        [Display(Name = "Casa")]
        Casa = 1,
        [Display(Name = "Apartamento")]
        Apto = 2,
        [Display(Name = "Lote")]
        Lote = 3,
        [Display(Name = "Outro")]
        Comercial = 4,
        Outro = 5
    }


    public enum StatusPeriodo : int
    {
        EntradaDeDados = 1,
        Rateio = 2,
        Auditoria = 3,
        Fechamento = 4,
        Fechado = 5
    }

    public enum StatusAuditoriaSocio : int
    {
        Pendente = 1,
        Concluido = 2,
        Cancelado = 4
    }


    public enum OrigemStudioParcela : int
    {
        ProgramacaoAula = 1,
        PlanoParcela = 2,
        LotePagtoProfessor = 3
    }
    public enum TipoLancto : int
    {
        [Display(Name = "Crédito")]
        Receita = 3,
        [Display(Name = "Débito")]
        Despesa = 4,
        [Display(Name = "Transferência")]
        Transferencia = 5,
        [Display(Name = "Resultado")]
        Resultado = 6
    }


    public enum TipoMovtoBanco : int
    {
        [Display(Name = "Crédito")]
        Credito = 1,
        [Display(Name = "Débito")]
        Debito = 2
    }

    public enum TipoLanctoSocio : int
    {
        [Display(Name = "Aportes")]
        Aporte = 3,
        [Display(Name = "Liberação de proventos")]
        Provento = 6,
        [Display(Name = "Resultado negativo do período")]
        Prejuizo = 8,
        [Display(Name = "Empréstimo/Adiantamento contratual")]
        DebitoContratual = 10,
        [Display(Name = "Juros sobre empréstimo/adiantamento contratual")]
        JurosDebitoContratual = 12

    }

    public enum OrigemLancto : int
    {
        [Display(Name = "Digitado")]
        Digitado = 1,
        [Display(Name = "Importado")]
        Importado = 2,
        [Display(Name = "Sistema")]
        Sistema = 3,
        [Display(Name = "Importado conta corremte")]
        ImportadoCC = 4,
        SistemaJuros = 5,
        ImportadoLote = 6,
        ImportadoND = 7
    }

    public enum StatusLote : int
    {
        [Display(Name = "Erro")]
        Erro = 1,
        [Display(Name = "Pendente ")]
        Pendente = 2,
        [Display(Name = "Processado")]
        Processado = 3
    }


    public enum TipoPlanoConta : int
    {
        [Display(Name = "Receita")]
        Receita = 3,
        [Display(Name = "Despesa")]
        Despesa = 4,
        Auxiliar = 6,
        Resultado = 5,
        Caixa = 8
    }

    public enum TipoCC : int
    {
        [Display(Name = "Ativo")]
        Ativo = 1,
        [Display(Name = "Passivo")]
        Passivo = 2,
        [Display(Name = "Receita")]
        Receita = 3,
        [Display(Name = "Despesa")]
        Despesa = 4,
        [Display(Name = "Outra")]
        Outra = 5
    }



    public enum TipoOperacao : int
    {
        [Display(Name = "Inclusão")]
        Inclusao = 1,
        [Display(Name = "Edição")]
        Edicao = 2,
        [Display(Name = "Exclusão")]
        Exclusao = 3,
        [Display(Name = "Consulta")]
        Consulta = 4

    }


    public enum TipoCCorrente : int
    {
        [Display(Name = "Caixa")]
        Carteira = 1,
        [Display(Name = "Conta corrente")]
        CtaCorrente = 2,
        [Display(Name = "Cartão de crédito")]
        CartaoCred = 3,
        [Display(Name = "Investimento")]
        Investimento = 4,
        [Display(Name = "Conta corrente sócios")]
        CtaSocio = 5,
        [Display(Name = "Outra")]
        Outra = 6
    }

    public enum StatusEmpreendimento : int
    {
        Inativo = 0,
        Ativo = 1,
        Bloqueado = 2,
        Encerrado = 3
    }


    public enum TipoIndice : int
    {
        SemCorrecao = 0,
        IPCA = 1,
        IGPM = 2,
        Fixo = 4,
        INCC = 3,
        Especifico = 5,
        CDI = 6
    }
    public enum TipoPeriodoIndice : int
    {
        Diario = 1,
        Mensal = 2
    }
    public enum OrigemIndice : int
    {
        Geral = 1,
        Especifico = 2
    }


    public enum TipoLanctoDebito : int
    {
        None = 0,
        Juros = 1,
        Amortizacao = 2
    }


    public enum OrigemDocto : int
    {
        none = 0,
        Aporte = 1,
        AporteDeposito = 2,
        Debito = 3,
        DebitoJuro = 4,
        DebitoAmortizacao = 5,
        Distribuicao = 6,
        DistribuicaoDeposito = 7,
        ResultadoPeriodo = 8,
        Auditoria = 9
    }


    public enum OrigemLinha : int
    {
        None = 0,
        Aporte = 1,
        Distribuicao = 2,
        Lancamento = 3,
        LancamentoRateado = 4
    }

    public enum StatusProfessorLotePagto : int
    {
        Aberto = 1,
        ConferenciaProfessor = 2,
        Liberado = 3,
        Pago = 4,
        Conciliado = 5,
        ConciliadoParcial = 6
    }
    public enum StatusAula : int
    {
        none = 0,
        Programada = 1,
        Agendada = 2,
        Executada = 3,
        Cancelada = 4,
        NaoProgramada = 5,
        ReAgendamento = 6,
        FaltaSemReagendamento = 7,
        Reserva = 8,
        Excluir = 9
    }
    public enum TipoSaveMode : int
    {
        OnlyDB = 0,
        OnlyFolder = 1
    }
    public enum OrigemAnexo : int
    {
        DepositoAporte = 1,
        DepositoRetirada = 2,
        ExtratoContaCorrente = 3,
        LogoEmpreendimento = 4,
        LogoEmpresa = 5,
        ReportFechamanto = 6,
        FichaAluno = 7,
        Planta =8
    }
    public enum TipoPlano : int
    {
        none = 0,
        PacoteQtdeAula = 1,
        PeriodoValorFixo = 2,
        PeriodoValorMensal = 3
    }
    public enum TipoCalculoValorPlano : int
    {
        none = 0,
        BaseQtdeAula = 1,
        ValorFixoMensal = 2,
    }
    public enum OrigemProgramacao : int
    {
        none = 0,
        Sistema = 1,
        Manual = 2
    }
    public enum StatusPlanoAluno : int
    {
        NaoProgramado = 1,
        PendenteConfirmacao = 2,
        Ativo = 3,
        Cancelado = 4,
        Encerrado = 5
    }
    public enum StatusParcela : int
    {
        none = 0,
        Aberto = 1,
        Pago = 2,
        Conciliado = 3,
        Cancelada = 4,
        Inadimplente = 5,
        Agendado = 6,
        ConciliadoParcial = 7
    }

    public enum TipoAula : int
    {
        Plano = 1,
        Pacote = 2,
        Avulsa = 3,
        Teste = 4,
        EmGrupo
    }


    public enum OrigemToken : int
    {
        Invalid = 0,
        Seguranca = 1,
        Studio = 2
    }

    public enum OperacaoToken : int
    {
        Invalid = 0,
        TrocarSenha = 1,
        ConfirmarEmail = 2,
        AprovarPlano = 3,
        ExtratoPlano = 4
    }
    public enum TipoParametro : int
    {
        none = 0,
        Inteiro = 1,
        Data = 2,
        Boleano = 3
    }

    public enum TipoUsuario : int
    {
        Usuario = 1,
        Socio = 2,
        Professor = 3
    }

}
