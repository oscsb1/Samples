
using System.Collections.Generic;



namespace InCorpApp.Models
{

     public class EmpreendimentoView
    {
        public Empreendimento Emp { get; set; }
        public List<UnidadeEmpreendimento> Unidades { get; set; } = new List<UnidadeEmpreendimento>();

        public List<SocioEmpreendimentoView> Socios { get; set; } = new List<SocioEmpreendimentoView>();

        public List<GrupoSocio> GrupoSocios { get; set; } = new List<GrupoSocio>();

        public List<GrupoRateio> GrupoRateios { get; set; } = new List<GrupoRateio>();

        public List<ContaCorrente> ContaCorrentes { get; set; } = new List<ContaCorrente>();
        public List<PlanoContaView> PlanoContas { get; set; } = new List<PlanoContaView>();


    }
}
