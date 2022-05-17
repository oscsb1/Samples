using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using InCorpApp.Models;

namespace InCorpApp.Models
{


    public class PlanoContaView 
    {
        public int Id { get; set; }
        public string TenantId { get; set; }
        public int PlanoGerencialId { get; set; }
        public PlanoGerencial PlanoGerencial { get; set; }
        public TipoPlanoConta Tipo { get; set; }
        public int TipoDespesa { get; set; }
        public bool Ratear { get; set; }
        public int EmpreendimentoId { get; set; }
        public Empreendimento Empreendimento { get; set; }
        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "{0} tamanho deve ser entre {2} and {1}")]
        public string Nome { get; set; }
        [Display(Name = "Conta contabil vinculada")]
        public int ContaContabilId { get; set; }
        [Display(Name = "Codigo para integração")]
        public string CodigoExterno { get; set; }
        [Display(Name = "Regra rateio vinculada")]
        public int GrupoRateioId { get; set; }
        public int RegraRateioDefaultId { get; set; }
        public string RegraRateioDefault { get; set; }

        [Display(Name = "Conta gerencial")]
        public string NomePlanoGerencial { get; set; }

        [Display(Name = "Conta contabil vinculada")]
        public string NomeContaContabil { get; set; }

        [Display(Name = "Regra de rateio vinculada")]
        public string RegraRateio { get; set; }

        public string TipoV
        {
            get
            {
                return Tipo switch
                {
                    TipoPlanoConta.Despesa => "Despesa",
                    TipoPlanoConta.Receita => "Receita",
                    TipoPlanoConta.Auxiliar => "Auxiliar",
                    _ => "erro"
                };
            }
        }

        public string TipoDespesaV
        {
            get
            {
                return TipoDespesa switch
                {
                    0 => "",
                    1 => "Impostos sobre vendas",
                    2 => "Comissões sobre vendas",
                    3 => "Taxa de administração",
                    4 => "Outras",
                    _ => throw new NotImplementedException()
                };
            }
        }
    }


    public class PlanoContasView
    {
        public TipoPlanoConta Tipo { get; set; }

        public List<PlanoContaView> PlanoContas = new ();
    }
}
