using InCorpApp.Constantes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class TituloBaseView : BaseModel
    {
        public ClasseTituloBase Classe { get; set; }
        public TipoTitulo TipoTitulo { get; set; }
        public int RelacionamentoId { get; set; }
        public string RelacionamentoNome { get; set; } = string.Empty;
        public int PlanoContaId { get; set; }
        public string PlanoContaNome { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Historico { get; set; } = string.Empty;
        public DateTime DataVencto { get; set; }
        public DateTime DataPagto { get; set; }
        public double Valor { get; set; }
        public double ValorConciliado { get; set; }
        public StatusParcela Status { get; set; }
        public string StatusV
        {
            get
            {
                return Constante.GetNomeStatusParcela(Status);
            }
            set
            { }
        }
    }
}
