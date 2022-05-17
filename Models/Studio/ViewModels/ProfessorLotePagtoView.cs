using InCorpApp.Constantes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class ProgramacaoAulaPagtoView
    {
        public int ProfessorLotePagtoId { get; set; }
        public int ProgramacaoAulaId { get; set; }
        public int AulaId { get; set; }
        public string NomeAula { get; set; }
        public int AlunoId { get; set; }
        public string NomeAluno { get; set; }
        public DateTime DataRef { get; set; }
        public StatusAula Status { get; set; }
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
                        s = "Exec";
                        break;
                    case StatusAula.NaoProgramada:
                        s = "Não programada";
                        break;
                    case StatusAula.Programada:
                        s = "Programada - pendente confirmação do aluno";
                        break;
                    case StatusAula.ReAgendamento:
                        s = "Reagendamento pendente";
                        break;
                    case StatusAula.FaltaSemReagendamento:
                        s = "Falta";
                        break;
                    case StatusAula.Reserva:
                        s = "Reserva";
                        break;
                }
                return s;
            }
            set { }
        }
        public double ValorAula { get; set; }
        public string ValorAulaV => ValorAula.ToString("N2", Constante.Culture);
        public TipoAula TipoAula { get; set; }
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
                        s = "Pct";
                        break;
                    case TipoAula.Teste:
                        s = "Exp";
                        break;
                    case TipoAula.Plano:
                        s = "Plano";
                        break;
                }
                return s;
            }
            set { }
        }

        public StatusParcela StatusFinanceiro { get; set; }
        public double ValorProfessor { get; set; }
        public string ValorProfessorV => ValorProfessor.ToString("N2", Constante.Culture);
        public string NomeStudio { get; set; }
        public string Erro { get; set; } = string.Empty;
    }
    public class ProfessorLotePagtoView: BaseModel
    {
        public int ProfessorId { get; set; }
        public int RelacionamentoId { get; set; }
        public bool Expand { get; set; } = false;
        public StatusProfessorLotePagto Status { get; set; } = StatusProfessorLotePagto.Aberto;
        [NotMapped]
        public string StatusV
        {
            get
            {
                string s = string.Empty;
                switch (Status)
                {
                    case StatusProfessorLotePagto.Aberto:
                        s = "Aberto";
                        break;
                    case StatusProfessorLotePagto.Pago:
                        s = "Pago";
                        break;
                    case StatusProfessorLotePagto.Liberado:
                        s = "Liberado para pagamento";
                        break;
                    case StatusProfessorLotePagto.ConferenciaProfessor:
                        s = "Conferência do professor";
                        break;
                    case StatusProfessorLotePagto.Conciliado:
                        s = "Conciliado";
                        break;
                    case StatusProfessorLotePagto.ConciliadoParcial:
                        s = "Conciliação parcial";
                        break;
                }
                return s;
            }
            set { }
        }
        public DateTime DataPagto { get; set; }
        public string NomeProfessor { get; set; }
        public DateTime DataInicio { get; set; }
        public string DataInicioV { get {return DataInicio.ToString("dd/MM/yyyy"); }  set {; } }
        public DateTime DataFim { get; set; }
        public string DataFimV { get { return DataFim.ToString("dd/MM/yyyy"); } set {; } }
        public int TotalAulas { get; set; }
        public string TotalAulasV { get { return TotalAulas.ToString(); } set {; } }
        public double ValorTotal { get; set; }
        public string ValorTotalV { get { return ValorTotal.ToString("N2", Constante.Culture); } set {; } }
        public double ValorTotalPagar { get; set; }
        public string ValorTotalPagarV => ValorTotalPagar.ToString("N2", Constante.Culture);
        public double ValorTotalAulas { get; set; }
        [NotMapped]
        public string ValorTotalAulasV { get { return ValorTotalAulas.ToString("N2", Constante.Culture); } set {; } }
        public double Margem { get; set; }
        public string MargemV => Margem.ToString("N2", Constante.Culture);
        public double MargemPercentual { get; set; }
        public string MargemPercentualV => MargemPercentual.ToString("N2", Constante.Culture) + " %";
        public string BonusObs { get; set; } = string.Empty;
        public double Bonus { get; set; }
        public string BonusV => Bonus.ToString("N2", Constante.Culture);
        public string DescontoObs { get; set; } = string.Empty;
        public double Desconto { get; set; }
        public string DescontoV => Desconto.ToString("N2", Constante.Culture);
        public bool ReadOnly { get; set; } = false;
        public bool Assinado { get; set; } = false;
        public DateTime DataAssinatura { get; set; }
        public List<ProgramacaoAulaPagtoView> ProgramacaoAulaPagtos { get; set; } = new();
    }
}
