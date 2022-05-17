using InCorpApp.Interfaces;
using System;
using System.Globalization;
using InCorpApp.Models;
using InCorpApp.Constantes;

namespace InCorpApp.Models
{
    public class SocioDebito : BaseDocument, ISFDocument
    {
        public DateTime DataLancto { get; set; }
        public DateTime DataInicioCorrecao { get; set; }
        public DateTime DataUltimaCorrecao { get; set; }
        public bool CorrecaoAutomatica { get; set; }
        public DateTime DataVencto { get; set; }
        public double Valor { get; set; }
        public TipoIndice TipoIndexador { get; set; }
        public int IndiceEconomicoId { get; set; }
        public double Taxa { get; set; }
        public string Acordo { get; set; }
        public string Descricao { get => Acordo; }
        public bool Quitado { get; set; }
        public string TipoIndexadorV
        {
            get
            {
                return TipoIndexador switch
                {
                    TipoIndice.SemCorrecao => "Sem correção",
                    TipoIndice.IPCA => "IPCA",
                    TipoIndice.IGPM => "IGPM",
                    TipoIndice.Fixo => "Taxa fixa",
                    TipoIndice.INCC => "INCC",
                    TipoIndice.Especifico => "Específico",
                    TipoIndice.CDI => "CDI",
                    _ => throw new NotImplementedException()
                };
            }
            set
            {; }
        }
        public OrigemDocto OrigemDocto { get => OrigemDocto.Debito; }
        public DateTime DataLimite { get => DataLancto.AddDays(5); }
        public string TextoAprovacao
        {
            get => Acordo +   ". Valor : " + Valor.ToString("C2", Constante.Culture);
        }
        public string CodigoExterno { get; set; }
        public static string FieldsName()
        {
            return "Código do empreendimento;Código do sócio;Código de integração;Data do empréstimo/adiantamento;Data de inicio da correção;Correção automática;Data de vencimento;Índice;Taxa de juros;Valor do empréstimo/adiantamento;Descrição;" + 
                   "Data do lançamento;Tipo J-Juros/A-Amortização;Valor;Histórico lançamento";
        }
        public static string FieldsCaptions()
        {
            return "Código do empreendimento;Código do sócio;Código de integração;Data do empréstimo/adiantamento;Data de inicio da correção;Correção automática;Data de vencimento;Índice;Taxa de juros;Valor do empréstimo/adiantamento;Descrição;" +
                   "Data do lançamento;Tipo J-Juros/A-Amortização;Valor;Histórico lançamento";
        }
    }
}
