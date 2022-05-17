using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Models;
using InCorpApp.Constantes;
using System.Diagnostics.CodeAnalysis;

namespace InCorpApp.Models
{

    public class ProgramacaoAula : BaseModel
    {
        public int StudioId { get; set; }
        public Studio Studio { get; set; } = new Studio() { Nome = "Não definido" };
        public int AlunoId { get; set; }
        public Aluno Aluno { get; set; } = new Aluno() { Relacionamento = new Relacionamento() { Nome = "Não definido" } };
        public string AlunoNome
        {
            get
            {
                if (Aluno != null) { return Aluno.Nome; } else { return "Não definido"; }
            }
        }
        public int AlunoPlanoId { get; set; }
        [NotMapped]
        public int AlunoPlanoAulaId { get; set; }
        public AlunoPlano AlunoPlano { get; set; }
        public int AulaId { get; set; }
        public Aula Aula { get; set; } = new Aula() { Duracao = 0, Nome = "Não definido" };
        public int ProfessorId { get; set; }
        public Professor Professor { get; set; } = new Professor() { Relacionamento = new Relacionamento() { Nome = "Não definido" } };
        public TipoAula TipoAula { get; set; }
        [NotMapped]
        public string NomeTipoAula
        {
            get
            {
                string s = string.Empty;
                switch (TipoAula)
                {
                    case TipoAula.Avulsa:
                        s = "Avulsa";
                        break;
                    case TipoAula.Pacote:
                        s = "Pacote";
                        break;
                    case TipoAula.Teste:
                        s = "Experimental";
                        break;
                    case TipoAula.Plano:
                        s = "Plano";
                        break;
                    case TipoAula.EmGrupo:
                        s = "Grupo";
                        break;
                }
                return s;
            }
            set { }
        }
        public DateTime DataProgramada { get; set; }
        [NotMapped]
        public string DataProgramadaV => DataProgramada.ToString("dd/MM/yyyy");
        [NotMapped]
        public string DataProgramadaAlunoV => DataProgramada.ToString("dd/MM") + ' ' + HoraInicio.ToString("HH:mm");
        [NotMapped]
        public string AnoMes => DataProgramada.ToString("yyyyMM");
        [NotMapped]
        public string MesAno => DataProgramada.ToString("MM/yyyy");
        public OrigemProgramacao Origem { get; set; }
        public double Inicio { get; set; }
        public double Fim { get; set; }
        public double Valor { get; set; }
        [NotMapped]
        public string ValorV
        {
            get
            {
                return Valor.ToString("C2", Constante.Culture);
            }
            set { }
        }
        public double ValorPago { get; set; }
        public double ValorConciliado { get; set; }
        [NotMapped]
        public string ValorPagoV
        {
            get
            {
                return ValorPago.ToString("C2", Constante.Culture);
            }
            set { }
        }
        public StatusAula Status { get; set; }
        [NotMapped]
        public string StatusNome
        {
            get
            {
                string s = string.Empty;
                switch (Status)
                {
                    case StatusAula.Agendada:
                        s = "Agendada";
                        break;
                    case StatusAula.Cancelada:
                        s = "Cancelada";
                        break;
                    case StatusAula.Executada:
                        s = "Executada";
                        break;
                    case StatusAula.NaoProgramada:
                        s = "Não programada";
                        break;
                    case StatusAula.Programada:
                        s = "Programada - pendente confirmação";
                        break;
                    case StatusAula.ReAgendamento:
                        s = "Reagendamento pendente";
                        break;
                    case StatusAula.FaltaSemReagendamento:
                        s = "Falta - sem reagendamento";
                        break;
                    case StatusAula.Reserva:
                        s = "Reserva de horário";
                        break;
                }
                return s;
            }
            set { }
        }
        [NotMapped]
        public StudioConfig StudioConfig { get; set; }
        [NotMapped]
        public string StatusCor
        {
            get
            {
                string s = string.Empty;
                switch (Status)
                {
                    case StatusAula.Agendada:
                        s = StudioConfig.StatusAulaAgendada;
                        break;
                    case StatusAula.Cancelada:
                        s = StudioConfig.StatusAulaCancelada;
                        break;
                    case StatusAula.Executada:
                        s = StudioConfig.StatusAulaExecutada;
                        break;
                    case StatusAula.NaoProgramada:
                        s = StudioConfig.StatusAulaNaoProgramada;
                        break;
                    case StatusAula.Programada:
                        s = StudioConfig.StatusAulaProgramada;
                        break;
                    case StatusAula.ReAgendamento:
                        s = StudioConfig.StatusAulaReAgendamento;
                        break;
                    case StatusAula.FaltaSemReagendamento:
                        s = StudioConfig.StatusAulaFaltaSemReagendamento;
                        break;
                    case StatusAula.Reserva:
                        s = StudioConfig.StatusAulaReserva;
                        break;
                }
                return s;
            }
            set { }
        }
        public string OBS { get; set; } = string.Empty;
        public string NotaFiscal { get; set; } = string.Empty;
        public StatusParcela StatusFinanceiro { get; set; }
        [NotMapped]
        public string StatusFinanceiroNome
        {
            get
            {
                return Constante.GetNomeStatusParcela(StatusFinanceiro);
            }
            set { }
        }
        public bool Faturado { get; set; }
        public DateTime DataPagto { get; set; }
        public double ValorProfessor { get; set; }
        public int ProfessorLotePagtoId { get; set; }
        [NotMapped]
        public bool Pago
        {
            get
            {
                return (StatusFinanceiro == StatusParcela.Conciliado || Faturado || StatusFinanceiro == StatusParcela.Pago);
            }
            set
            {
                if (value)
                {
                    StatusFinanceiro = StatusParcela.Pago;
                    DataPagto = Constante.Today;
                }
                else
                {
                    DataPagto = DateTime.MinValue;
                    StatusFinanceiro = StatusParcela.Aberto;
                }
            }
        }
        public int ProfessorRealId { get; set; }
        public Professor ProfessorReal { get; set; }
        public int AulaAgendaId { get; set; }
        [NotMapped]
        public int DiaN => (int)DataProgramada.DayOfWeek;
        [NotMapped]
        public DayOfWeek Dia => DataProgramada.DayOfWeek;
        [NotMapped]
        public DateTime HoraInicio
        {
            get => Constante.Today.AddMinutes(Inicio);
            set => Inicio = value.Hour * 60 + value.Minute;
        }
        [NotMapped]
        public DateTime HoraFim
        {
            get
            {
                if (Aula != null)
                {
                    if (Fim == 0 || Fim == Inicio)
                    { Fim = Inicio + Aula.Duracao; }
                    return Constante.Today.AddMinutes(Inicio + Aula.Duracao);
                }
                else
                {
                    if (Fim != 0)
                    {
                        return Constante.Today.AddMinutes(Fim);
                    }
                    else
                    {
                        return Constante.Today.AddMinutes(Inicio);
                    }
                }
            }
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
        public string Diadd => DataProgramada.ToString("dd");
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
        public string DiaVShort
        {
            get
            {
                string s = string.Empty;
                switch (Dia)
                {
                    case DayOfWeek.Sunday:
                        s = "Dom";
                        break;
                    case DayOfWeek.Monday:
                        s = "Seg";
                        break;
                    case DayOfWeek.Tuesday:
                        s = "Ter";
                        break;
                    case DayOfWeek.Wednesday:
                        s = "Qua";
                        break;
                    case DayOfWeek.Thursday:
                        s = "Qui";
                        break;
                    case DayOfWeek.Friday:
                        s = "Sex";
                        break;
                    case DayOfWeek.Saturday:
                        s = "Sab";
                        break;
                }
                return s;
            }
            set { }
        }
        [NotMapped]
        public string StudioNome
        {
            get
            {
                if (Studio != null) { return Studio.Nome; } else { return "Não definido"; }
            }
        }
        [NotMapped]
        public string AulaNome
        {
            get
            {
                if (Aula != null) { return Aula.Nome; } else { return "Não definido"; }
            }
        }
        [NotMapped]
        private string _professornome = string.Empty;
        [NotMapped]
        public string ProfessorNome
        {
            get
            {
                if (Professor != null && Professor.Relacionamento != null && Professor.Relacionamento.Id > 0) { return Professor.Nome; } else { return _professornome; }
            }
            set { _professornome = value; }
        }
        [NotMapped]
        public string ProfessorRealNome
        {
            get
            {
                if (Status != StatusAula.Executada)
                {
                    return string.Empty;
                }
                if (ProfessorReal != null) { return ProfessorReal.Nome; } else { return "Não definido"; }
            }
            set { }
        }
        [NotMapped]
        public DateTime DataProgramadaCompleta => new(DataProgramada.Year, DataProgramada.Month, DataProgramada.Day, HoraInicio.Hour, HoraInicio.Minute, 0);
        [NotMapped]
        public string NomeAgendaDia
        {
            get
            {
                if (Aula.Codigo != null && Aula.Codigo != string.Empty)
                {
                    return Aluno.Nome + " ( " + Aula.Codigo + " )";
                }
                else
                {
                    return Aluno.Nome;
                }
            }
        }
        [NotMapped]
        public string DataCompleta
        {
            get
            {
                return DataProgramadaV + " " + HoraInicioV + " - " + HoraFimV;
            }
            set { }
        }
        [NotMapped]
        public string ProntuarioAula { get; set; } = string.Empty;
        [NotMapped]
        public int SeqOrder
        {
            get
            {
                switch (Status)
                {
                    case StatusAula.Agendada:
                        return 90;
                    case StatusAula.Cancelada:
                        return 20;
                    case StatusAula.Executada:
                        return 70;
                    case StatusAula.NaoProgramada:
                        return 50;
                    case StatusAula.Programada:
                        return 80;
                    case StatusAula.ReAgendamento:
                        return 40;
                    case StatusAula.FaltaSemReagendamento:
                        return 60;
                    case StatusAula.Reserva:
                        return 30;
                }
                return 0;
            }
        }
    }

    public class ProgramacaoAulaHist : BaseModel
    {
        public int ProgramacaoAulaId { get; set; }
        public ProgramacaoAula ProgramacaoAula { get; set; }
        public StatusAula StatusAnt { get; set; }
        [NotMapped]
        public string StatusAntNome
        {
            get
            {
                string s = string.Empty;
                switch (StatusAnt)
                {
                    case StatusAula.Agendada:
                        s = "Confirmada";
                        break;
                    case StatusAula.Cancelada:
                        s = "Cancelada";
                        break;
                    case StatusAula.Executada:
                        s = "Executada";
                        break;
                    case StatusAula.NaoProgramada:
                        s = "Agendamento pendente";
                        break;
                    case StatusAula.Programada:
                        s = "Programada - pendente confirmação do aluno";
                        break;
                    case StatusAula.ReAgendamento:
                        s = "Reagendamento pendente";
                        break;
                    case StatusAula.FaltaSemReagendamento:
                        s = "Falta - sem reagendamento";
                        break;
                    case StatusAula.Reserva:
                        s = "Reserva de horário";
                        break;
                }
                return s;
            }
            set { }
        }
        public StatusAula StatusDep { get; set; }
        [NotMapped]
        public string StatusDepNome
        {
            get
            {
                string s = string.Empty;
                switch (StatusDep)
                {
                    case StatusAula.Agendada:
                        s = "Confirmada";
                        break;
                    case StatusAula.Cancelada:
                        s = "Cancelada";
                        break;
                    case StatusAula.Executada:
                        s = "Executada";
                        break;
                    case StatusAula.NaoProgramada:
                        s = "Agendamento pendente";
                        break;
                    case StatusAula.Programada:
                        s = "Programada - pendente confirmação do aluno";
                        break;
                    case StatusAula.ReAgendamento:
                        s = "Reagendamento pendente";
                        break;
                    case StatusAula.FaltaSemReagendamento:
                        s = "Falta - sem reagendamento";
                        break;
                    case StatusAula.Reserva:
                        s = "Reserva de horário";
                        break;
                }
                return s;
            }
            set { }
        }
        public string ProfessorAnt { get; set; }
        public string ProfessorDep { get; set; }
        public DateTime DataProgramadaAnt { get; set; }
        [NotMapped]
        public string DataProgramadaAntV => DataProgramadaAnt.ToString("dd/MM/yyyy");

        public DateTime DataProgramadaDep { get; set; }
        [NotMapped]
        public string DataProgramadaDepV => DataProgramadaDep.ToString("dd/MM/yyyy");
        public double InicioAnt { get; set; }
        public double InicioDep { get; set; }
        public double FimAnt { get; set; }
        public double FimDep { get; set; }

        public double ValorAnt { get; set; }
        public double ValorDep { get; set; }
        public DateTime DataAlt { get; set; }
        [NotMapped]
        public string DataAltV => DataAlt.ToString("dd/MM HH:mm");
        public string UserName { get; set; }
        public string Obs { get; set; }
        [NotMapped]
        public DateTime HoraInicioAnt
        {
            get => Constante.Today.AddMinutes(InicioAnt);
            set => InicioAnt = value.Hour * 60 + value.Minute;
        }
        [NotMapped]
        public DateTime HoraInicioDep
        {
            get => Constante.Today.AddMinutes(InicioDep);
            set => InicioDep = value.Hour * 60 + value.Minute;
        }
        [NotMapped]
        public DateTime HoraFimAnt
        {
            get => Constante.Today.AddMinutes(FimAnt);
            set => FimAnt = value.Hour * 60 + value.Minute;
        }
        [NotMapped]
        public DateTime HoraFimDep
        {
            get => Constante.Today.AddMinutes(FimDep);
            set => FimDep = value.Hour * 60 + value.Minute;
        }
        public string ValorAntV
        {
            get
            {
                return ValorAnt.ToString("C2", Constante.Culture);
            }
            set { }
        }
        public string ValorDepV
        {
            get
            {
                return ValorDep.ToString("C2", Constante.Culture);
            }
            set { }
        }

    }


}
