using InCorpApp.Interfaces;
using System;
using System.Globalization;
using InCorpApp.Models;
using InCorpApp.Constantes;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCorpApp.Models
{
    public class SocioRetirada : BaseDocument, ISFDocument
    {
        public DateTime DataLiberacao { get; set; }
        public double Valor { get; set; }
        public string Historico { get; set; }
        public DateTime DataPrevistaPagamento { get; set; }

        public string Descricao { get => Historico; }
        public DateTime DataLimite { get => DataLiberacao.AddDays(3); }
        public OrigemDocto OrigemDocto { get => OrigemDocto.Distribuicao; }
        public string TextoAprovacao
        {
            get
            {
                if (ValorAmortizado > 0)
                {
                    return " Distibuição autorizada em " + DataLiberacao.ToString("dd/MM/yyyy") + " no valor de " + Valor.ToString("C2", Constante.Culture) +
                        ". Valor retido para amortizar empréstimos/adiantamentos: " + ValorAmortizado.ToString("C2", Constante.Culture) + ". Valor a ser creditado: " +
                        (Valor - ValorAmortizado).ToString("C2", Constante.Culture) + " em " + DataPrevistaPagamento.ToString("dd/MM/yyyy.");
                }
                else
                {
                    return " Distibuição autoriazada em " + DataLiberacao.ToString("dd/MM/yyyy") + " no valor de " + Valor.ToString("C2", Constante.Culture);
                }
            }
        }
        [NotMapped]
        public double ValorAmortizado { get; set; }
        public static string FieldsName()
        {
            return "Código do empreendimento;Código do sócio;Data da distribuição;Valor";
        }
        public static string FieldsCaptions()
        {
            return "Código do empreendimento;Código do sócio;Data da distribuição;Valor";
        }

    }
}