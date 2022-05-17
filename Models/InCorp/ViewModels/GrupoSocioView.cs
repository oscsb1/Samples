using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{

    public class GrupoSocioView
    {
        public GrupoSocio GrupoSocio { get; set; }
        public List<SocioGrupoView> SociosGrupoView { get; set; } = new List<SocioGrupoView>();
    }



    public class SocioGrupoView

    {
        public GrupoSocioEmpreedSocio GrupoSocioEmpreedSocio { get; set; }
        public SocioEmpreendimentoView SocioEmpreendimentoView { get; set; }
        
        public bool Selecionado { get; set; }
 
    }

    public class SocioEmpreendimentoView
    {
        public int SocioId { get; set; }
        public Socio Socio { get; set; } = new Socio();
        public int EmpreendimentoSocioId { get; set; }

        public EmpreendimentoSocio EmpreendimentoSocio { get; set; } = new EmpreendimentoSocio();

        public string NomeV  => EmpreendimentoSocio.Nome;
        public string CPFCNPJV => EmpreendimentoSocio.CPFCNPJ;

    }






    public class SocioRateioView : ProgramacaoGrupoRateioSocios
    {
        public string Nome { get; set; }
        public bool Selecionado { get; set; }

        public void CopyTo(ProgramacaoGrupoRateioSocios g)
        {
            g.Id = Id;
            g.Percentual = Percentual;
            g.ProgramacaoGrupoRateioId = ProgramacaoGrupoRateioId;
            g.GrupoSocioId = GrupoSocioId;
        }
        public void CopyFrom(ProgramacaoGrupoRateioSocios g)
        {
            Id = g.Id;
            Percentual = g.Percentual;
            ProgramacaoGrupoRateioId = g.ProgramacaoGrupoRateioId;
            GrupoSocioId = g.GrupoSocioId;
        }

    }




    public class GrupoRateioView 
    {


        public GrupoRateio GrupoRateio { get; set; } = new GrupoRateio();

        public List<ProgramacaoGrupoRateio> ProgramacaoGrupoRateios { get; set; } = new List<ProgramacaoGrupoRateio>();

        public ProgramacaoGrupoRateio ProgramacaoGrupoRateio { get; set; } = new ProgramacaoGrupoRateio();

 

    }








}
