using System;
using System.ComponentModel.DataAnnotations.Schema;
using InCorpApp.Constantes;
using Microsoft.AspNetCore.Identity;

namespace InCorpApp.Models
{
    public class Professor : BaseModel
    {

        public int RelacionamentoId { get; set; }
        public Relacionamento Relacionamento { get; set; } = new Relacionamento();
        public bool AcessoLiberado { get; set; }
        [NotMapped]
        public string Nome { get => Relacionamento.Nome; }
        [NotMapped]
        public string Email { get => Relacionamento.Email; }
        [NotMapped]
        public string Telefone { get => Relacionamento.Telefone; }

    }
    public class ProfessorAula : BaseModel
    {
        public int ProfessorId { get; set; }
        public Professor Professor { get; set; }
        public int AulaId { get; set; }
        public Aula Aula { get; set; }
        [NotMapped]
        public bool Vinculado { get; set; } = false;
    }
    public class ProfessorAgenda : BaseModel
    {
        public int ProfessorId { get; set; }
        public Professor Professor { get; set; }
        public DateTime DataInicio { get; set; } = Constante.Today;
        public int StudioId { get; set; }
        public Studio Studio { get; set; }
        [NotMapped]
        public string DataInicioV => DataInicio.ToString("dd/MM/yyyy");
        [NotMapped]
        public string StudioNome
        {
            get
            {
                if (Studio != null) { return Studio.Nome; } else { return string.Empty; }
            }
        }
    }
    public class ProfessorAgendaDia : BaseModel
    {
        public int ProfessorAgendaId { get; set; }
        public ProfessorAgenda ProfessorAgenda { get; set; }
        public DayOfWeek Dia { get; set; }
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
        public int DiaN { get { return (int)Dia; } set { Dia = (DayOfWeek)value; } }
        [NotMapped]
        public string DiaV
        {
            get
            {
                return Constante.DiaSemana[(int)Dia];
            }
            set { }
        }

    }
    public class ProfessorAgendaDiaEspecial : BaseModel
    {
        public int ProfessorId { get; set; }
        public Professor Professor { get; set; }
        public int StudioId { get; set; }
        public Studio Studio { get; set; }
        public DateTime Data { get; set; } = Constante.Today;
        public bool TemExpediente { get; set; }
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
        public string TemExpedienteV
        {
            get { if (TemExpediente) { return "sim"; } else { return "não"; } }
            set {; }
        }

        [NotMapped]
        public string DataV
        {
            get => Data.ToString("dd/MM/yyyy");
        }

    }
    public class ProfessorSalario : BaseModel
    {
        public int ProfessorId { get; set; }
        public Professor Professor { get; set; }
        public DateTime DataInicio { get; set; }
    }
    public class ProfessorSalarioAula : BaseModel
    {
        public int ProfessorSalarioId { get; set; }
        public ProfessorSalario ProfessorSalario { get; set; }
        public int AulaId { get; set; }
        public Aula Aula { get; set; }
        public bool PercentualExecutada { get; set; }
        public double ValorExecutada { get; set; }
        public bool PercentualReserva { get; set; }
        public double ValorReserva { get; set; }
        public bool PercentualFalta { get; set; }
        public double ValorFalta { get; set; }
        [NotMapped]
        public DateTime DataInicio { get; set; }
        [NotMapped]
        public int ProfessorId { get; set; }
        [NotMapped]
        public string NomeAula { get; set; }
        [NotMapped]
        public string ValorExecutadaV
        {
            get
            {
                if (PercentualExecutada)
                {
                    return ValorExecutada.ToString("N2", Constante.Culture) + "%";
                }
                else
                {
                    return ValorExecutada.ToString("C2", Constante.Culture);

                }
            }
        }
        [NotMapped]
        public string ValorFaltaV
        {
            get
            {
                if (PercentualFalta)
                {
                    return ValorFalta.ToString("N2", Constante.Culture) + "%";
                }
                else
                {
                    return ValorFalta.ToString("C2", Constante.Culture);

                }
            }
        }
        [NotMapped]
        public string ValorReservaV
        {
            get
            {
                if (PercentualReserva)
                {
                    return ValorReserva.ToString("N2", Constante.Culture) + "%";
                }
                else
                {
                    return ValorReserva.ToString("C2", Constante.Culture);

                }
            }
        }
    }
    public class ProfessorLotePagto : BaseModel
    {
        public int ProfessorId { get; set; }
        public Professor Professor { get; set; }
        public StatusProfessorLotePagto Status { get; set; }
        public DateTime DataPagto { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int TotalAulas { get; set; }
        public double ValorTotal { get; set; }
        public double ValorTotalPagar { get; set; }
        public double ValorTotalAulas { get; set; }
        public double Margem { get; set; }
        public double MargemPercentual { get; set; }
        public string BonusObs { get; set; }
        public double Bonus { get; set; }
        public string DescontoObs { get; set; }
        public double Desconto { get; set; }
        public bool Assinado { get; set; }
        public DateTime DataAssinatura { get; set; }

    }
    public class ProfessorUsuario : BaseModel
    {
        public Professor Professor { get; set; }
        public int ProfessorId { get; set; }
        public IdentityUser IdentityUser { get; set; }
        public string IdentityUserId { get; set; }
    }

}
