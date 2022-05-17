using InCorpApp.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCorpApp.Models
{
    public class PlanoGerencial : BaseModel
    {
        public string Nome { get; set; } = string.Empty;
        public string CodigoExterno { get; set; } = string.Empty;
        public TipoPlanoConta TipoPlanoConta { get; set; }
        public bool AporteDistribuicao { get; set; }
        public string NomeGrupoContaAuxiliar { get; set; } = string.Empty;
        public int RegraRateioDefaultId { get; set; }
        [NotMapped]
        public string TipoPlanoContaV
        {
            get
            {
                string s = string.Empty;
                switch (TipoPlanoConta)
                {
                    case TipoPlanoConta.Despesa:
                        s = "Despesa";
                        break;
                    case TipoPlanoConta.Receita:
                        s = "Receita";
                        break;
                    case TipoPlanoConta.Auxiliar:
                        s = "Auxiliar";
                        break;
                }
                return s;
            }

        }
        [NotMapped]
        public string NomeRegraRateioDefault { get; set; } = string.Empty;
    }
}
