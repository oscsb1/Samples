using System;
using System.ComponentModel.DataAnnotations.Schema;
using InCorpApp.Models;
using InCorpApp.Constantes;


namespace InCorpApp.Models
{
    public class Agenda : BaseModel
    {
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
    public class AgendaDia : BaseModel
    {
        public int AgendaId { get; set; }
        public Agenda Agenda { get; set; }
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
        [NotMapped]
        public bool Expediente { get; set; } = true;

    }

    public class AgendaDiaEspecial : BaseModel
    {
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
}
