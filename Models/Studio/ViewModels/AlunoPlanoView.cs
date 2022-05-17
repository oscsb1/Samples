using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class AlunoPlanoView
    {
        public AlunoPlano AlunoPlano { get; set; }
        public string IdV => AlunoPlano.Id.ToString();
        public DateTime DataInicio => AlunoPlano.DataInicio;
        public DateTime DataFim => AlunoPlano.DataFim;
        public string NomeStatus => AlunoPlano.NomeStatus;
        public string NomeAluno => AlunoPlano.NomeAluno;

        public string NomePlano => AlunoPlano.NomePlano;
        public string TipoPlanoV => AlunoPlano.TipoPlanoV;
    }
}
