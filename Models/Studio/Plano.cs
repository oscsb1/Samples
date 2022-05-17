using InCorpApp.Constantes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;

namespace InCorpApp.Models
{
    public class Plano : BaseModel
    {
        public string Nome { get; set; }
        public TipoPlano TipoPlano { get; set; }
        public int DuracaoPlano { get; set; }
        public int DiaVencto { get; set; }
        public int QtdeParcelas { get; set; }
        public TipoCalculoValorPlano TipoCalculoValorPlano { get; set; }
        public double ValorFixoMensal { get; set; }
        public bool Ativo { get; set; }
        [NotMapped]
        public List<PlanoAula> Aulas { get; set; } = new List<PlanoAula>();
        /*
                [NotMapped]
                public string ValorTotalV
                {
                    get
                    {
                        if (Aulas == null) { return string.Empty; }
                        else
                        {
                            if (TipoPlano == TipoPlano.PacoteQtdeAula)
                            {
                                return (Aulas.Sum(x => x.QtdeAulas * x.ValorAula)).ToString("C2", Constante.Culture);
                            }
                            else
                            {
                                return (Aulas.Sum(x => x.QtdeAulasSemana * x.ValorAula * 4)).ToString("C2", Constante.Culture);
                            }
                        };
                    }
                    set {; }
                }
                [NotMapped]
                public string ValorParcelaV
                {
                    get
                    {
                        if (Aulas == null || QtdeParcelas == 0) { return string.Empty; }
                        else
                        {
                            if (TipoPlano == TipoPlano.PacoteQtdeAula)
                            {
                                return (Aulas.Sum(x => x.QtdeAulas * x.ValorAula)/QtdeParcelas).ToString("C2", Constante.Culture);
                            }
                            else
                            {
                                return (Aulas.Sum(x => x.QtdeAulasSemana * x.ValorAula * 4)/QtdeParcelas).ToString("C2", Constante.Culture);
                            }
                        };
                    }
                    set {; }
                }
        */
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
    }
    public class PlanoAula : BaseModel
    {
        public int PlanoId { get; set; }
        public Plano Plano { get; set; }
        public int AulaId { get; set; }
        public Aula Aula { get; set; }
        public int QtdeAulas { get; set; }
        public int QtdeAulasSemana { get; set; }
        public bool HorarioFixo { get; set; }
        public double ValorAula { get; set; }
        [NotMapped]
        public string NomeAula { get => Aula.Nome; }
        [NotMapped]
        public int DuracaoAula { get => Aula.Duracao; }
        [NotMapped]
        public int QtdeMaxAlunos { get => Aula.QtdeMaximaAluno; }
        [NotMapped]
        public string HorarioFixoV
        {
            get { if (HorarioFixo) { return "sim"; } else { return "não"; }; }
        }
    }

    public class PlanoTabelaPreco : BaseModel
    {
        public DateTime DataInicio { get; set; }
        public string Nome { get; set; } = "Nova tabela";
        public bool Ativo { get; set; } = true;
        [NotMapped]
        public string LblAtivo
        {
            get
            {
                if (Ativo)
                { return string.Empty; }
                else
                {
                    return "Inativa";
                }
            }
        }
    }
    public class PlanoAulaPreco: BaseModel
    {
        public int PlanoTabelaPrecoId { get; set; }
        public PlanoTabelaPreco PlanoTabelaPreco { get; set; }
        public int PlanoAulaId { get; set; }
        public PlanoAula PlanoAula { get; set; }
        public double ValorAula { get; set; }
        [NotMapped]
        public string TabelaPrecoNome { get; set; }
    }

    public class StudioPlano : BaseModel
    {
        public int StudioId { get; set; }
        public Studio Studio { get; set; }
        public int PlanoId { get; set; }
        public Plano Plano { get; set; }
        public string NomePlano { get => Plano.Nome; }
    }
}

