using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Constantes;

namespace InCorpApp.Models
{
    public class BaseDocument : BaseModel
    {
        public int EmpreendimentoSocioId { get; set; }
        public EmpreendimentoSocio EmpreendimentoSocio { get; set; }
        [NotMapped]
        public string NomeSocio { get;set;}
        [NotMapped]
        public int EmpreendimentoId { get; set; }
        [NotMapped]
        public string NomeEmpreendimento { get; set; }
        [NotMapped]
        public int PeriodoId { get; set; }
        public bool Assinado { get; set; } = false;
        public DateTime DataCriacao { get; set; } = Constante.Now;
        public DateTime DataAssinatura { get; set; } = DateTime.MinValue;
        public string GUID { get; set; } = Guid.NewGuid().ToString();
        public string NomeAssinado { get; set; }
        [NotMapped]
        public bool ShowGerencial { get; set; } = false;
    }
}
