using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using InCorpApp.Constantes;
using InCorpApp.Utils;

namespace InCorpApp.Models
{
    public class AlunoPlano : BaseModel
    {
        [NotMapped]
        public string IdV { get { return Id.ToString(); } set {; } }
        public DateTime DataInclusao { get; set; }
        public int AlunoId { get; set; }
        public TipoPlano TipoPlano { get; set; }
        public TipoCalculoValorPlano TipoCalculoValorPlano { get; set; }
        public double ValorFixoMensal { get; set; }
        [NotMapped]
        public string TipoPlanoV
        {
            get
            {
                return TipoPlano switch
                {
                    TipoPlano.PacoteQtdeAula => Constante.TipoPlanoPacoteQtdeAula,
                    TipoPlano.PeriodoValorFixo => Constante.TipoPlanoPeriodoValorFixo,
                    TipoPlano.PeriodoValorMensal => Constante.TipoPlanoPeriodoValorVariavel,
                    _ => "plano invalido",
                };
                ;
            }
            set {; }
        }
        public Aluno Aluno { get; set; }
        public int PlanoId { get; set; }
        public Plano Plano { get; set; }
        public int QtdeParcelas { get; set; }
        public DateTime DataInicio { get; set; } = Constante.Today;
        public DateTime DataFim { get; set; }
        public int DiaVencto { get; set; }
        public string TokenAprovacao { get; set; }
        [NotMapped]
        public string DataInicioV { get { return DataInicio.ToString("dd/MM/yyyy"); } set {; } }
        [NotMapped]
        public string DataFimV { get { return DataFim.ToString("dd/MM/yyyy"); } set {; } }
        public StatusPlanoAluno Status { get; set; }
        [NotMapped]
        public string NomeStatus
        {
            get
            {
                string s = string.Empty;
                switch (Status)
                {
                    case StatusPlanoAluno.Ativo:
                        s = "Ativo";
                        break;
                    case StatusPlanoAluno.Cancelado:
                        s = "Cancelado";
                        break;
                    case StatusPlanoAluno.NaoProgramado:
                        s = "Programação pendente";
                        break;
                    case StatusPlanoAluno.PendenteConfirmacao:
                        s = "Pendente de confirmação pelo aluno";
                        break;
                    case StatusPlanoAluno.Encerrado:
                        s = "Encerrado";
                        break;
                }
                return s;
            }
            set { }
        }
        [NotMapped]
        public List<AlunoPlanoAula> Aulas { get; set; }
        [NotMapped]
        public List<AlunoPlanoParcela> Parcelas { get; set; }
        [NotMapped]
        public string NomePlano { get => Plano.Nome; }
        [NotMapped]
        public string ValorTotalV
        {
            get
            {
                if (Aulas == null) { return string.Empty; }
                else
                {
                    return ValorTotal.ToString("C2", Constante.Culture);
                };
            }
            set {; }
        }
        [NotMapped]
        public string ValorParcelaV
        {
            get
            {
                if (Aulas == null || QtdeParcelas == 0 && TipoPlano != TipoPlano.PeriodoValorFixo) { return string.Empty; }
                else
                {
                    return ValorParcela.ToString("C2", Constante.Culture);
                };
            }
            set {; }
        }
        [NotMapped]
        public double ValorTotal
        {
            get
            {
                if (Aulas == null && TipoCalculoValorPlano != TipoCalculoValorPlano.ValorFixoMensal) { return 0; }
                else
                {
                    if (TipoPlano == TipoPlano.PacoteQtdeAula)
                    {
                        return (Aulas.Sum(x => x.QtdeAulas * x.ValorAula));
                    }
                    else
                    {
                        if (TipoPlano == TipoPlano.PeriodoValorFixo)
                        {
                            if (TipoCalculoValorPlano == TipoCalculoValorPlano.BaseQtdeAula)
                            {
                                return Math.Round(Aulas.Sum(x => x.QtdeAulasSemana * x.ValorAula * 4) * UtilsClass.DateDiff(1,DataInicio,DataFim), 2);
                            }
                            else
                            {
                                if (TipoCalculoValorPlano == TipoCalculoValorPlano.ValorFixoMensal)
                                {
                                    return Math.Round( ValorFixoMensal * UtilsClass.DateDiff(1, DataInicio, DataFim), 2) ;
                                }
                            }
                        }
                        return 0;
                    }
                };
            }
            set {; }
        }
        [NotMapped]
        public double ValorParcela
        {
            get
            {
                if (Aulas == null || (QtdeParcelas == 0 && TipoPlano != TipoPlano.PeriodoValorFixo)) { return 0; }
                else
                {
                    if (QtdeParcelas == 0)
                    {
                        if (DataFim > DataInicio)
                        {
                            QtdeParcelas = Convert.ToInt32(Math.Truncate(UtilsClass.DateDiff(1, DataInicio, DataFim)));
                            if (QtdeParcelas == 0)
                            {
                                QtdeParcelas = 1;
                            }
                        }
                        else
                        {
                            QtdeParcelas = 1;
                        }
                    }
                    return Math.Round(ValorTotal / QtdeParcelas, 2);
                };
            }
            set {; }
        }
        [NotMapped]
        public string NomeAluno { get; set; }
    }
    public class AlunoPlanoTokenHist : BaseModel
    {
        public int AlunoPlanoId { get; set; }
        public string Token { get; set; }
        public int TotalAulas { get; set; }
        public double ValorTotal { get; set; }
        public DateTime DataMovto { get; set; }
        public string Historico { get; set; }
    }
    public class AlunoPlanoAula : BaseModel
    {
        public int AlunoPlanoId { get; set; }
        public AlunoPlano AlunoPlano { get; set; }
        public int AulaId { get; set; }
        public Aula Aula { get; set; }
        public int QtdeAulas { get; set; }
        public double ValorAula { get; set; }
        public int QtdeAulasSemana { get; set; }
        public bool HorarioFixo { get; set; }
        public bool Planejado { get; set; }
        [NotMapped]
        public string NomeAula { get => Aula.Nome; }
        [NotMapped]
        public int DuracaoAula { get => Aula.Duracao; }
        [NotMapped]
        public int QtdeMaxAlunos { get => Aula.QtdeMaximaAluno; }
        [NotMapped]
        public string ValorAulaV => ValorAula.ToString("C2", Constante.Culture);
        [NotMapped]
        public string HorarioFixoV
        {
            get { if (HorarioFixo) { return "sim"; } else { return "não"; }; }
        }
        [NotMapped]
        public int TotalAulasFeitas { get; set; }
        [NotMapped]
        public string NomeAulaEsp { get; set; }
        [NotMapped]
        public int TotalAulasAtivas { get; set; }

    }
    public class AlunoPlanoParcela : BaseModel
    {
        public int AlunoPlanoId { get; set; }
        public AlunoPlano AlunoPlano { get; set; }
        public DateTime DataVencto { get; set; }
        public DateTime DataPagto { get; set; }
        public double Valor { get; set; }
        public double ValorPago { get; set; }
        public double ValorConciliado { get; set; }
        public StatusParcela Status { get; set; }
        [NotMapped]
        public string StatusNome
        {
            get
            {
                return Constante.GetNomeStatusParcela(Status);
            }
            set { }

        }
        public string OBS { get; set; }
        public string NotaFiscal { get; set; }
        [NotMapped]
        public string ValorV => Valor.ToString("C2", Constante.Culture);
        [NotMapped]
        public string ValorPagoV => ValorPago.ToString("C2", Constante.Culture);
        [NotMapped]
        public string DataVenctoV => DataVencto.ToString("dd/MM/yyyy");
        [NotMapped]
        public string DataPagtoV
        {
            get
            {
                if (DataPagto == DateTime.MinValue)
                { return string.Empty; }
                else
                { return DataPagto.ToString("dd/MM/yyyy"); }
            }
        }
        public bool Faturado { get; set; }
        public DateTime DataInclusao { get; set; } = Constante.Today;
    }
    public class AlunoAulaAgenda : BaseModel
    {
        public int AlunoId { get; set; }
        public Aluno Aluno { get; set; }
        public int AulaId { get; set; }
        public Aula Aula { get; set; }
        public int StudioId { get; set; }
        public Studio Studio { get; set; }
        public DayOfWeek Dia { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        [NotMapped]
        public int DiaN { get { return (int)Dia; } set { Dia = (DayOfWeek)value; } }
        public double Inicio { get; set; }
        public double Fim { get; set; }
        [NotMapped]
        public DateTime HoraInicio
        {
            get => Constante.Today.AddMinutes(Inicio);
            set => Inicio = value.Hour * 60 + value.Minute;
        }
        [NotMapped]
        public DateTime HoraFim
        {
            get => Constante.Today.AddMinutes(Fim);
            set => Fim = value.Hour * 60 + value.Minute;
        }
        [NotMapped]
        public string HoraInicioV
        {
            get => HoraInicio.ToString("HH:mm");
            set {; }
        }
        [NotMapped]
        public string HoraFimV
        {
            get => HoraFim.ToString("HH:mm");
            set {; }
        }
        [NotMapped]
        public AlunoAulaAgendaProfessor AlunoAulaAgendaProfessor { get; set; }
        [NotMapped]
        public string DiaV
        {
            get
            {
                return Constante.DiaSemana[(int)Dia];
            }
            set { }
        }
        [NotMapped]
        public string StudioNome
        {
            get
            {
                if (Studio != null) { return Studio.Nome; } else { return string.Empty; }
            }
        }
        [NotMapped]
        public string AulaNome
        {
            get
            {
                if (Aula != null) { return Aula.Nome; } else { return string.Empty; }
            }
        }
        [NotMapped]
        public string ProfessorNome
        {
            get
            {
                if (AlunoAulaAgendaProfessor != null && AlunoAulaAgendaProfessor.Professor != null) { return AlunoAulaAgendaProfessor.Professor.Nome; } else { return string.Empty; }
            }
        }
        [NotMapped]
        public int ProfessorId
        {
            get
            {
                if (AlunoAulaAgendaProfessor != null) { return AlunoAulaAgendaProfessor.ProfessorId; } else { return 0; }
            }
            set
            {
                if (AlunoAulaAgendaProfessor == null)
                {
                    AlunoAulaAgendaProfessor = new AlunoAulaAgendaProfessor()
                    {
                        AlunoAulaAgendaId = Id
                    };
                }
                AlunoAulaAgendaProfessor.ProfessorId = value;
            }
        }
    }
    public class AlunoAulaAgendaProfessor : BaseModel
    {
        public int AlunoAulaAgendaId { get; set; }
        public AlunoAulaAgenda AlunoAulaAgenda { get; set; }
        public int ProfessorId { get; set; }
        public Professor Professor { get; set; }
    }
    public class AlunoAusencia : BaseModel
    {
        public int AlunoId { get; set; }
        public Aluno Aluno { get; set; }
        public DateTime DataInicio { get; set; } = Constante.Today;
        public DateTime DataFinal { get; set; } = Constante.Today;
        public string DataInicioV => DataInicio.ToString("dd/MM/yyyy");
        public string DataFinalV => DataFinal.ToString("dd/MM/yyyy");
        public bool CobrarReservaHorario { get; set; } = false;
        public string CobrarReservaHorarioV
        {
            get { if (CobrarReservaHorario) { return "sim"; } else { return "não"; }; }
        }

        public double Percentual { get; set; } = 0;

        public string PercentualV => Percentual.ToString("N2", Constante.Culture) + " %";
    }
}
