using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using InCorpApp.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCorpApp.Models
{
    public class PlanoConta : BaseModel
    {
        public int PlanoGerencialId { get; set; }
        public PlanoGerencial PlanoGerencial { get; set; }
        public TipoPlanoConta Tipo { get; set; }
        [NotMapped]
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
        public bool AporteDistribuicao { get; set; }
        public bool Ratear { get; set; }
        public int EmpreendimentoId { get; set; }
        public Empreendimento Empreendimento { get; set; }
        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "{0} tamanho deve ser entre {2} and {1}")]
        public string Nome { get; set; }
        public string NomeCurto { get; set; }
        [Display(Name = "Conta contabil vinculada")]
        public int ContaContabilId { get; set; }
        [Display(Name = "Codigo para integração")]
        public string CodigoExterno { get; set; }
        [Display(Name = "Regra rateio vinculada")]
        public int GrupoRateioId { get; set; }
        public bool GrupoRateioDefault { get; set; } = false;
        [NotMapped]
        public bool Erro { get; set; } = false;
        [NotMapped]
        public string ErroMsg { get; set; } = string.Empty;
        [NotMapped]
        public String PlanoContaGerencialNome { get; set; } = string.Empty;
        public static string FieldsName()
        {
            return "Tipo R-Receita/D-Debito;Data;Documento;Histórico;Valor;Código plano conta;Plano de conta;Codigo favorecido;Favorecido;Código plano gerencial;Plano de Gerencial;Codigo externo;Código complementar plano de conta";
        }
        public static string FieldsCaptions()
        {
            return "Tipo R-Receita/D-Debito;Data;Documento;Histórico;Valor;Código plano conta;Plano de conta;Codigo favorecido;Favorecido;Código plano gerencial;Plano de Gerencial;Codigo externo;Código complementar plano de conta";
        }
    }
}

/*
Tipo R-Receita/D-Debito;
Data;
Documento;Histórico;
Valor;
Código plano conta;
Plano de conta;
Codigo favorecido;
Favorecido;
Código plano gerencial;
Plano de Gerencial;
Código complementar plano de conta";
 */ 