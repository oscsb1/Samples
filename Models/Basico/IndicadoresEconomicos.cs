using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCorpApp.Models
{
    public class IndiceEconomico
    {
        public int Id { get; set; }
        public TipoIndice TipoIndice { get;set;}
        public string Nome { get; set; }
        public OrigemIndice OrigemIndice { get; set; }
        public string TenantId { get; set; }
        public TipoPeriodoIndice TipoPeriodoIndice { get; set; }
    }
    public class IndiceEconomicoMovto
    {
        public int Id { get; set; }
        public int IndiceEconomicoId { get; set; }
        public IndiceEconomico IndiceEconomico { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public double Valor { get; set; }
        [NotMapped]
        public bool Erro { get; set; }
        [NotMapped]
        public string ErroMsg { get; set; }
        public static string FieldsName()
        {
            return "Data;Valor";
        }
        public static string FieldsCaptions()
        {
            return "Data;Valor";
        }


    }

}
