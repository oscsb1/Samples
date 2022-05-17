using AutoMapper;
using InCorpApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class IncorpProfile : Profile
    {

        public IncorpProfile()
        {
            CreateMap<Socio, Socio>();
            CreateMap<UnidadeEmpreendimento, UnidadeEmpreendimento>();
            CreateMap<EmpreendimentoSocioParticipacao, EmpreendimentoSocioParticipacao>();
            CreateMap<ProgramacaoGrupoRateioView, ProgramacaoGrupoRateio>();
            CreateMap<ProgramacaoGrupoRateioSociosView, ProgramacaoGrupoRateioSocios>();
            CreateMap<SocioView, EmpreendimentoSocioParticipacao>();
   //         CreateMap<SocioView, Socio>();
  //         CreateMap<Socio, SocioView>();
            CreateMap<LanctoDetailView, LanctoEmpreendimento>();
            CreateMap<LanctoEmpreendimento, LanctoDetailView>();
            CreateMap<LanctoDetailView, LanctoImportado>();
            CreateMap<LanctoImportado, LanctoDetailView>();
            CreateMap<LanctoEmpreendimento, LanctoEmpreendimento>();
            CreateMap<LanctoImportado, LanctoImportado>();
            CreateMap<LanctoEmpreendimento, LanctoEmpreendimento>();



        }
    }
}
