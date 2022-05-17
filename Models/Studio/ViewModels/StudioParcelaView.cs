using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Constantes;
using InCorpApp.Models;

namespace InCorpApp.Models
{
    public class StudioParcelaView
    {
        public int Id { get; set; }
        public ProgramacaoAula ProgramacaoAula { get; set; }
        public AlunoPlanoParcela AlunoPlanoParcela { get; set; }
        public TipoAula TipoAula { get; set; }
        public string TipoAulaV
        {
            get
            {
                string s = string.Empty;
                switch (TipoAula)
                {
                    case TipoAula.Avulsa:
                        s = "avulsa";
                        break;
                    case TipoAula.Plano:
                        s = "plano";
                        break;
                    case TipoAula.Pacote:
                        s = "pacote";
                        break;
                }
                return s;
            }
        }
        public string NomeAluno { get; set; } = string.Empty;
        public int AlunoId { get; set; }
        public int RelacionamentoId { get; set; }
        public int AlunoPlanoId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataVencto { get; set; }
        public DateTime DataPagto { get; set; }
        public double Valor { get; set; }
        public double ValorPago { get; set; }
        public double ValorConciliado { get; set; }
        public StatusParcela Status { get; set; }
        public string StatusNome
        {
            get
            {
                return Constante.GetNomeStatusParcela(Status);
            }
            set { }
        }
        public string OBS { get; set; } = string.Empty;
        public string ValorV => Valor.ToString("C2", Constante.Culture);
        public string ValorPagoV
        {
            get
            {
                if (ValorPago == 0)
                { return string.Empty; }
                else
                { return ValorPago.ToString("C2", Constante.Culture); }
            }
        }
        public string DataVenctoV => DataVencto.ToString("dd/MM/yyyy");
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
        public string FaturadoV
        {
            get
            {
                if (Faturado)
                { return "sim"; }
                else
                { return string.Empty; }
            }
        }
        public string NotaFiscal { get; set; }
    }
}
