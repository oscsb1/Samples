using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Constantes;
using InCorpApp.Models;

namespace InCorpApp.Models
{
    public class Aula : BaseModel
    {
        public string Nome { get; set; }
        public string Codigo { get; set; }
        public int Duracao { get; set; }
        public int QtdeMaximaAluno { get; set; } = 1;
        public double ValorAvulso { get; set; }
        public bool SemCusto { get; set; }
        public bool AulaGrupo { get; set; }
        public string Cor { get; set; }
        [NotMapped]
        public string ValorAvulsoV => ValorAvulso.ToString("C2", Constante.Culture);
        [NotMapped]
        public List<AulaAgenda> Agendas { get; set; }

    }

    public class AulaAgenda :BaseModel
    {
        public int AulaId { get; set; }
        public Aula Aula { get; set; }
        public int StudioSalaId { get; set; }
        public StudioSala StudioSala { get; set; }
        public int ProfessorId { get; set; }
        public Professor Professor { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int Dia { get; set; }
        public double Inicio { get; set; }
        public double Fim { get; set; }
        [NotMapped]
        public string StudioSalaNome { get; set; }
        [NotMapped]
        public string StudioNome { get; set; }
        [NotMapped]
        public string AulaNome { get; set; }
        [NotMapped]
        public string ProfessorNome { get; set; }
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
        public string Cor { get; set; }
        [NotMapped]
        public bool Saving { get; set; }
        [NotMapped]
        public bool Erro = false;
        [NotMapped]
        public string ErrMsg = string.Empty;
        [NotMapped]
        public string _Valor = string.Empty;
        [NotMapped]
        public string Valor
        {
            get
            {
                if (_Valor == string.Empty)
                { _Valor = HoraInicio.ToString("HH:mm"); };
                return _Valor;
            }
            set
            {
                if (value != null && value != string.Empty && value.Length > 2 && int.TryParse(value.Replace(":", ""), out _))
                {
                    string s = value.Replace(":", "");
                    if (int.TryParse(s.Substring(s.Length - 2, 2), out int mm))
                    {
                        if (int.TryParse(s[0..^2], out int hh) && mm < 60)
                        {
                            if (hh < 24)
                            {
                                Inicio = hh * 60 + mm;
                                if (Aula != null)
                                {
                                    Fim = Inicio + Aula.Duracao;
                                }
                                return;
                            }
                        }
                    }
                }
                else
                {
                    Erro = true;
                    ErrMsg = "Hora de inicio inválida";
                }
            }
        }
        [NotMapped]
        public bool Registro { get; set; } = false;
    }

    public class AulaAgendaAlunoPlanoAula :BaseModel
    {
        public int AulaAgendaId { get; set; }
        public AulaAgenda AulaAgenda { get; set; }
        public int AlunoPlanoAulaId { get; set; }
        public AlunoPlanoAula AlunoPlanoAula { get; set; }
    }

    public class AulaAgendaProfessor :BaseModel
    {
        public int AulaAgendaId { get; set; }
        public int ProfessorId { get; set; }
        public DateTime Dia { get; set; }
        public int StudioSalaId { get; set; }
        public bool Registro { get; set; } = false;
    }

}
