using InCorpApp.Interfaces;
using System;
using System.Globalization;
using InCorpApp.Models;
using InCorpApp.Constantes;

namespace InCorpApp.Models
{
    public class SocioAporte :BaseDocument, ISFDocument
    {
        public DateTime DataSolicitacao { get; set; }
        public DateTime DataLimiteDeposito { get; set; }
        public DateTime DataLimite { get => DataLimiteDeposito; }
        public double Valor { get; set; }
        public string Descricao { get => "Solicitação de aporte"; }
        public OrigemDocto OrigemDocto { get => OrigemDocto.Aporte; }
        public string TextoAprovacao
        {
            get => "Aporte solicitado em " + DataSolicitacao.ToString("dd/MM/yyyy") + " com data limite para deposito em " + DataLimiteDeposito.ToString("dd/MM/yyyy") + " no valor de " + Valor.ToString("C2", Constante.Culture);
        }

        public static string FieldsName()
        {
            return "Código do empreendimento;Código do sócio;Data do aporte;Valor";
        }
        public static string FieldsCaptions()
        {
            return "Código do empreendimento;Código do sócio;Data do aporte;Valor";
        }

    }

}
