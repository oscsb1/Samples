using Microsoft.EntityFrameworkCore;
using InCorpApp.Data;
using InCorpApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Services.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Blazored.SessionStorage;
using InCorpApp.Security;
using AutoMapper;

namespace InCorpApp.Services
{
    public class SocioOperacoesService:ServiceBase
    {

        private readonly ApplicationDbContext _context;


        public SocioOperacoesService(ApplicationDbContext context, ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper) : base(sessionStorageService, sessionService, mapper)
        {
            _context = context;

        }

        public async Task<List<Empreendimento>> FindAllEmpreendimentoBySocioId(int id)
        {
            var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText =
                  "select Empreendimento.* "
                + " from Empreendimento "
                + " inner join EmpreendimentoSocio on EmpreendimentoSocio.EmpreendimentoId = Empreendimento.Id "
                + "  where EmpreendimentoSocio.SocioId = @id "
                + " order by  Empreendimento.Nome  ";

            command.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));
            command.Parameters["@id"].Value =id;

            await command.Connection.OpenAsync();
            List<Empreendimento> r = new ();
            try
            {
                var lu = await command.ExecuteReaderAsync();
                while (await lu.ReadAsync())
                {
                    Empreendimento u = new()
                    {
                        Id = (int)lu["Id"],
                        Nome = lu["Nome"].ToString(),

                    };
                    r.Add(u);
                };

                lu.Close();
            }
            finally
            {
                command.Connection.Close();
            }

            return r;


        }
    }
}
