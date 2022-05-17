using InCorpApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class UnidadeEmpreendimento : BaseModel
    {
        public int EmpreendimentoId { get; set; }
        public Empreendimento Empreendimento { get; set; }
        public TipoUE TipoEmp { get; set; }
        [NotMapped]
        public string TipoEmpV => TipoEmp switch
        {
            TipoUE.Casa => "Casa",
            TipoUE.Apto => "Apartamento",
            TipoUE.Lote => "Lote",
            TipoUE.Comercial => "Comercial",
            TipoUE.Outro => "Outro",
            _ => ""
        };
        public string CodigoExterno { get; set; }
        public double Area { get; set; }
        public double AreaConstruida { get; set; }
        public double Frente { get; set; }
        public double LadoE { get; set; }
        public double LadoD { get; set; }
        public double Fundo { get; set; }
        public string Andar { get; set; }
        public string Nro { get; set; }
        public int Vagas { get; set; }
        public string Torre { get; set; }
        public string Quadra { get; set; }
        public double AreaPrivativa { get; set; }
        public double AreaUtil { get; set; }
        public double AreaComum { get; set; }
        public double AreaTotal { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public static string FieldsName()
        {
            return "Tipo;Área;Área costruída;Frente;Lado Esquerdo;Lado Direito;Fundo;Andar;Número;Vagas;Torre;Quadra;Área privativa;Área útil;Área comum;Área total";
        }
        public static string FieldsCaptions()
        {
            return "Tipo;Área;Área costruída;Frente;Lado Esquerdo;Lado Direito;Fundo;Andar;Número;Vagas;Torre;Quadra;Área privativa;Área útil;Área comum;Área total";
        }
    }

    public class UnidadeMovto : BaseModel
    {
        public int UnidadeEmpreendimentoId { get; set; }
        public UnidadeEmpreendimento UnidadeEmpreendimento { get; set; }
        public int UnidadeLoteImportacaoId { get; set; }
        public UnidadeLoteImportacao UnidadeLoteImportacao { get; set; }
        public StatusUnidade StatusUnidade { get; set; }
        [NotMapped]
        public string StatusUnidadeV => StatusUnidade switch
        {
            StatusUnidade.None => "Não informado",
            StatusUnidade.Alugado => "Alugado",
            StatusUnidade.Bloqueado => "Bloqueado",
            StatusUnidade.Disponivel => "Disponível",
            StatusUnidade.EmDistrato => "EmDistrato",
            StatusUnidade.Vendida => "Vendido",
            StatusUnidade.Reservado => "Reservado",
            _ => ""
        };
        public StatusFinanceiroUnidade StatusFinanceiroUnidade { get; set; }
        public string StatusFinanceiroUnidadeV => StatusFinanceiroUnidade switch
        {
            StatusFinanceiroUnidade.NA => string.Empty,
            StatusFinanceiroUnidade.Adimplente => "Adimplente",
            StatusFinanceiroUnidade.Inadimplente => "Inadimplente",
            StatusFinanceiroUnidade.Atraso => "Atraso",
            _ => ""
        };

        public double ValorAtualVenda { get; set; }
        public double ValorAtualAluguel { get; set; }
        public DateTime DataContrato { get; set; }
        public double ValorContrato { get; set; }
        public DateTime DataDistrato { get; set; }
        public double ValorReembolso { get; set; }
        public double ValorAte30 { get; set; }
        public double ValorAte60 { get; set; }
        public double ValorAte90 { get; set; }
        public double ValorAte120 { get; set; }
        public double ValorApos120 { get; set; }
    }
    public enum TipoLoteImpUnidade : int
    {
        Digitado = 1,
        Upload = 2
    }

    public class UnidadeMovtoResumo : BaseModel
    {
        public int UnidadeLoteImportacaoId { get; set; }
        public UnidadeLoteImportacao UnidadeLoteImportacao { get; set; }
        public int QtdAlugado { get; set; }
        public int QtdBloqueado { get; set; }
        public int QtdDisponivel { get; set; }
        public int QtdDistrato { get; set; }
        public int QtdVendida { get; set; }
        public int QtdAdimplente { get; set; }
        public int QtdInadimplente { get; set; }
        public int QtdAtraso { get; set; }
        public double ValorlVenda { get; set; }
        public double ValorAluguel { get; set; }
        public double ValorReembolso { get; set; }
        public double ValorAte30 { get; set; }
        public double ValorAte60 { get; set; }
        public double ValorAte90 { get; set; }
        public double ValorAte120 { get; set; }
        public double ValorApos120 { get; set; }
    }
    public class UnidadeLoteImportacao : BaseModel
    {
        public int EmpreendimentoId { get; set; }
        public Empreendimento Empreendimento { get; set; }
        public DateTime DataMovto { get; set; }
        public DateTime DataInclusao { get; set; }
        public TipoLoteImpUnidade TipoLoteImpUnidade { get; set; }
        public string FileName { get; set; }
    }



}
