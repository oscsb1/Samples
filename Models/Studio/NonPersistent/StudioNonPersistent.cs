using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Models;

namespace InCorpApp.Models
{
    public class ProfessorAgendasDia
    {
        public Professor Professor { get; set; }
        public List<ProfessorAgendaDia> ProfessorAgendaDias { get; set; } = new List<ProfessorAgendaDia>();
    }

    public class ProfessorProgramacaoAulas
    {
        public Professor Professor { get; set; }
        public bool TemAgenda { get; set; } = false;
        public List<ProgramacaoAula> ProfessorAulas { get; set; } = new List<ProgramacaoAula>();
    }

    public class ResumoFinanceiroPlano
    {
        public double AlunoPlanoId { get; set; }
        public double ValorTotalParcelas { get; set; }
        public double ValorTotalParcelasPagas { get; set; }
        public double ValorParcelaPendente => ValorTotalParcelas - ValorTotalParcelasPagas;
        public double ValorTotalAulas { get; set; }
        public double ValorTotalAulasPagas { get; set; }
        public double ValorAulasPendente => ValorTotalAulas - ValorTotalAulasPagas;
        public double ValorTotalAulasExecutadas { get; set; }
        public bool Erro => Math.Abs(ValorTotalParcelas - ValorTotalAulas) > 0.1;
    }


    public class TabelaPreco
    {
        public List<Aula> Aulas { get; set; } = new();
        public List<Plano> Planos { get; set; } = new();
        public List<PlanoAula> PlanoAulas { get; set; } = new();
        public PlanoTabelaPreco PlanoTabelaPreco { get; set; }
        public List<PlanoTabelaPreco> TabelaPrecos { get; set; }
        public List<PlanoAulaPreco> PlanoAulaPrecos { get; set; } = new();
    }
}
