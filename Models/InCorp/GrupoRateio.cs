using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class GrupoRateio : BaseModel
    {
        public int EmpreendimentoId { get; set; }
        public Empreendimento Empreendimento { get; set; }

        [DisplayName("Nome do Grupo")]
        public string Nome { get; set; }
        [DisplayName("Aplicar para  PlanoContas do tipo receita")]
        public bool RateioReceita { get; set; }
        [DisplayName("Aplicar para  PlanoContas do tipo despesa")]
        public bool RateioDespesa { get; set; }
        [DisplayName("Ativo")]
        public bool Ativo { get; set; }
        public int RegraRateioDefaultId { get; set; }
    }
    public class RegraRateio : BaseModel
    {
        public string Nome { get; set; }
    }
    public class PlanoGerencialRGEmpreendimento : BaseModel
    {
        public int EmpreendimentoId { get; set; }
        public Empreendimento Empreendimento { get; set; }
        public int PlanoGerencialId { get; set; }
        public PlanoGerencial PlanoGerencial { get; set; }
        [NotMapped]
        public string PlanoGerencialNome { get; set; }
        public int RegraRateioId { get; set; }
        public RegraRateio RegraRateio { get; set; }
        [NotMapped]
        public string RegraRateioNome { get; set; }
        [NotMapped]
        public int RegraRateioDefaultId { get; set; }
        [NotMapped] 
        public string RegraRateioDefaultNome { get; set; }
        [NotMapped]
        public bool Saving { get; set; } = false;

    }

    public class ContaCorrenteRegraRateio : BaseModel
    {
        public int EmpreendimentoId { get; set; }
        public int ContaCorrenteId { get; set; }
        public ContaCorrente ContaCorrente { get; set; }
        public int PlanoGerencialId { get; set; }
        public PlanoGerencial PlanoGerencial { get; set; }
        public int PlanoContaId { get; set; }
        public int RegraRateioId { get; set; }
        public RegraRateio RegraRateio { get; set; }
        [NotMapped]
        public bool Saving { get; set; }
    }
}
