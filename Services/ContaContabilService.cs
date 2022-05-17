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
    public class ContaContabilService: ServiceBase
    {
        private readonly ApplicationDbContext _context;

        public ContaContabilService(ApplicationDbContext context, ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper) : base(sessionStorageService, sessionService,mapper)
        {
            _context = context;

        }


        public async Task AtualizaTipoSubContaAsync(ContaContabil cc)
        {

            SessionInfo si = await GetSessionAsync();

            var command = _context.Database.GetDbConnection().CreateCommand();

            command.CommandText = " update ContaContabil  set Tipo = @t where CodigoNormalizado like @c and TenantId = @tid ";
            command.Parameters.Add(new SqlParameter("@c", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@c"].Value = cc.CodigoNormalizado + ".%";
            command.Parameters.Add(new SqlParameter("@t", System.Data.SqlDbType.Int));
            command.Parameters["@t"].Value = cc.Tipo;
            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;


            command.Connection.Open();
            await command.ExecuteNonQueryAsync();
            command.Connection.Close();
        }


        public async Task<int> CodigoExisteAsync(ContaContabil cc)
        {
            SessionInfo si = await GetSessionAsync();

            var command = _context.Database.GetDbConnection().CreateCommand();

            int id = 0;

            command.CommandText = " select Isnull(Id,0) Id  from  ContaContabil  where (Codigo = @c or Resumido = @r) and TenantId = @TenantId ";
            command.Parameters.Add(new SqlParameter("@c", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@c"].Value = cc.Codigo;
            command.Parameters.Add(new SqlParameter("@r", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@r"].Value = cc.Resumido;
            command.Parameters.Add(new SqlParameter("@TenantId", System.Data.SqlDbType.NVarChar,100));
            command.Parameters["@TenantId"].Value = si.TenantId;
            command.Connection.Open();
            var lu = await command.ExecuteReaderAsync();
            while (await lu.ReadAsync())
            {
                id = Convert.ToInt32(lu["Id"]);
            };

            command.Connection.Close();

            return id;
        }


        public string NormalizeCodigo(string c)
        {
            string[] sc = c.Split(".");

            string r = "";

            foreach (string s in sc)
            {
                string x = "00000" + s;
                x = x.Substring(s.Length - 1, 5);
                if (r == "")
                {
                    r = x;
                }
                else
                {
                    r = r + "." + x;
                }
            }
            return r;

        }



        public async Task<List<ContaContabil>> FindAllAsync()
        {

            SessionInfo si = await GetSessionAsync();

    /*
            if (!_context.ContaContabil.Any(x=> x.TenantId == si.TenantId))
            {

                var command = _context.Database.GetDbConnection().CreateCommand();

                command.CommandText = "SELECT * from xcc ";

                command.Connection.Open();
                //    try
                {
                    var lu = await command.ExecuteReaderAsync();


                    List<ContaContabil> lc = new List<ContaContabil>();
                    while (await lu.ReadAsync())
                    {
                        var c = new ContaContabil()
                        {
                            Codigo = lu["Cod"].ToString(),
                            Nome = lu["Nome"].ToString(),
                            Analitica = false,
                            Resumido = lu["Cod"].ToString(),
                            Tipo = TipoCC.Ativo,
                            TenantId = si.TenantId

                        };
                        lc.Add(c);

                    };
                    command.Connection.Close();

                    foreach (ContaContabil c in lc)
                    {
                        await Incluir(c, true);
                    }
                }
                //  catch
                {

                }
            }
    */
            return (await _context.ContaContabil.Where(x => x.TenantId == si.TenantId).OrderBy(x => x.CodigoNormalizado).ToListAsync());
        }


        public async Task<List<ContaContabil>> FindAllAsync(int tipo,bool soAnalitica)
        {

            SessionInfo si = await GetSessionAsync();
            if (soAnalitica)
            {
                return await _context.ContaContabil.Where(x => x.TenantId == si.TenantId && x.Tipo == tipo && x.Analitica == true).OrderBy(x => x.Nome).ToListAsync();
            }else
            {
                return await _context.ContaContabil.Where(x => x.TenantId == si.TenantId && x.Tipo == tipo ).OrderBy(x => x.Nome).ToListAsync();
            }
        }




        public async Task<ContaContabil> FindByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ContaContabil.FirstOrDefaultAsync(obj => obj.TenantId == si.TenantId && obj.Id == id);
        }

        public async Task<ContaContabil> FindByCodigoAsync(string cd)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ContaContabil.FirstOrDefaultAsync(obj => obj.TenantId == si.TenantId && obj.Codigo == cd);
        }

       public async Task<ContaContabil>GetContaPai(int id)
        {

            var c = await FindByIdAsync(id);

            if (c.CodigoNormalizado.Length <= 6)
            {
                return null;
            }

            string cd = c.CodigoNormalizado[0..^7];

            SessionInfo si = await GetSessionAsync();
            return await _context.ContaContabil.FirstOrDefaultAsync(obj => obj.TenantId == si.TenantId && obj.CodigoNormalizado == cd);

        }

        public async Task<bool> TemSubConta(ContaContabil c)
        {
            string cd = c.CodigoNormalizado + ".";
            SessionInfo si = await GetSessionAsync();
            return await _context.ContaContabil.AnyAsync(obj => obj.TenantId == si.TenantId && obj.CodigoNormalizado.StartsWith( cd));
        }


        public async Task<bool> AnyAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ContaContabil.AnyAsync(obj => obj.TenantId == si.TenantId && obj.Id == id);
        }
        public async Task Incluir(ContaContabil cc, bool seed)
        {
            SessionInfo si = await GetSessionAsync();
            cc.TenantId = si.TenantId;

            for (int i = 0; i < cc.Codigo.Length; i++)
            {
                if (
                    cc.Codigo.Substring(i, 1) != "0" &&
                    cc.Codigo.Substring(i, 1) != "1" &&
                    cc.Codigo.Substring(i, 1) != "2" &&
                    cc.Codigo.Substring(i, 1) != "3" &&
                    cc.Codigo.Substring(i, 1) != "4" &&
                    cc.Codigo.Substring(i, 1) != "5" &&
                    cc.Codigo.Substring(i, 1) != "6" &&
                    cc.Codigo.Substring(i, 1) != "7" &&
                    cc.Codigo.Substring(i, 1) != "8" &&
                    cc.Codigo.Substring(i, 1) != "9" &&
                    cc.Codigo.Substring(i, 1) != "."
                    ) { throw new NotFoundException("Codigo com caracter inválido!"); }
            }


            string[] sc = cc.Codigo.Split(".");
            int j = sc.Length - 1;
            cc.Seq = Convert.ToInt32(sc[j]);
            cc.CodigoNormalizado = "";
            cc.Nivel = sc.Length;

            foreach (string s in sc)
            {
                string x = "00000" + s;
                x = x.Substring(s.Length , 5);
                if (cc.CodigoNormalizado == "")
                {
                    cc.CodigoNormalizado = x;
                }
                else
                {
                    cc.CodigoNormalizado = cc.CodigoNormalizado + "." + x;
                }
            }

            int ns = await ObterProximaSequenciaAsync(cc);

            if (!seed)
            {
                if (cc.Analitica && ns > 1)
                {
                    throw new NotFoundException("Conta com subconta não pode ser analítica!");
                }
            }




            if (cc.Resumido == null || cc.Resumido == "")
            {
                cc.Resumido = cc.Codigo;
            }

            if (await CodigoExisteAsync(cc) > 0)
            {
                throw new NotFoundException("Código ou código resumido ja cadastrado!");
            }

            cc.TenantId = si.TenantId;

            cc.Analitica = !await TemSubConta(cc);


            _context.Database.BeginTransaction();
            try
            {
                _context.ContaContabil.Add(cc);
                await _context.SaveChangesAsync();

                var cp = await GetContaPai(cc.Id);
                if (cp != null)
                {
                    if (cp.Analitica)
                    {
                        cp.Analitica = false;
                    }
                    _context.ContaContabil.Update(cp);
                }
                await _context.SaveChangesAsync();

                _context.Database.CommitTransaction();
            }
            catch 
            {
                _context.Database.RollbackTransaction();
                throw;
            }

        }

        public async Task UpdateAsync(ContaContabil obj)
        {
            SessionInfo si = await GetSessionAsync();
            bool hasAny = await _context.ContaContabil.AnyAsync(x => x.Id == obj.Id && x.TenantId == si.TenantId);
            if (!hasAny)
            {
                throw new NotFoundException("Codigo não encontrado.");
            }

            for (int i = 0; i < obj.Codigo.Length; i++)
            {
                if (
                    obj.Codigo.Substring(i, 1) != "0" &&
                    obj.Codigo.Substring(i, 1) != "1" &&
                    obj.Codigo.Substring(i, 1) != "2" &&
                    obj.Codigo.Substring(i, 1) != "3" &&
                    obj.Codigo.Substring(i, 1) != "4" &&
                    obj.Codigo.Substring(i, 1) != "5" &&
                    obj.Codigo.Substring(i, 1) != "6" &&
                    obj.Codigo.Substring(i, 1) != "7" &&
                    obj.Codigo.Substring(i, 1) != "8" &&
                    obj.Codigo.Substring(i, 1) != "9" &&
                    obj.Codigo.Substring(i, 1) != "."
                    ) { throw new NotFoundException("Codigo com caracter inválido!"); }
            }


            string[] sc = obj.Codigo.Split(".");
            int j = sc.Length - 1;
            obj.Seq = Convert.ToInt32(sc[j]);
            obj.CodigoNormalizado = "";
            obj.Nivel = sc.Length;

            foreach (string s in sc)
            {
                string x = "00000" + s;
                x = x.Substring(s.Length , 5);
                if (obj.CodigoNormalizado == "")
                {
                    obj.CodigoNormalizado = x;
                }
                else
                {
                    obj.CodigoNormalizado = obj.CodigoNormalizado + "." + x;
                }
            }

            if (obj.Resumido == "")
            {
                obj.Resumido = obj.Codigo;
            }


            if (await CodigoExisteAsync(obj) != obj.Id)
            {
                throw new NotFoundException("Código ou código resumido ja cadastrado!");
            }

            obj.TenantId = si.TenantId;

            obj.Analitica = !await TemSubConta(obj);

            _context.Database.BeginTransaction();
            try
            {
                _context.Update(obj);

                await _context.SaveChangesAsync();

                var cp = await GetContaPai(obj.Id);
                if (cp != null)
                {
                    if (cp.Analitica)
                    {
                        cp.Analitica = false;
                    }
                    _context.ContaContabil.Update(cp);
                }
                await _context.SaveChangesAsync();

                _context.Database.CommitTransaction();
            }
            catch 
            {
                _context.Database.RollbackTransaction();
                throw;
            }


            await AtualizaTipoSubContaAsync(obj);

        }

        public async Task<int> ObterProximaSequenciaAsync(ContaContabil cn)
        {
            SessionInfo si = await GetSessionAsync();

            var command = _context.Database.GetDbConnection().CreateCommand();

            command.CommandText = "SELECT Isnull(Max(seq),0) + 1 seq from ContaContabil where CodigoNormalizado like @c and TenantId = @TenantId ";
            command.Parameters.Add(new SqlParameter("@c", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@c"].Value = cn.CodigoNormalizado + ".%";
            command.Parameters.Add(new SqlParameter("@TenantId", System.Data.SqlDbType.NVarChar,100));
            command.Parameters["@TenantId"].Value = si.TenantId;

            command.Connection.Open();
            int seq = 0;
            try
            {
                var lu = await command.ExecuteReaderAsync();


                while (await lu.ReadAsync())
                {
                    seq = (int)lu["seq"];
                }
                lu.Close();
            }
            finally
            {
                command.Connection.Close();
            }
            return seq;
        }
        public async Task IncluirSubConta(ContaContabil pai, ContaContabil filha)
        {
            filha.Codigo = pai.Codigo + "." + Convert.ToString(await ObterProximaSequenciaAsync(pai));
            filha.Tipo = pai.Tipo;
            await Incluir(filha,false);
        }
        public async Task ExcluirContaAsync(ContaContabil o)
        {


            ContaContabil cc = await FindByIdAsync(o.Id);
            if (cc != null)
            {
                 _context.ContaContabil.Remove(cc);
              await  _context.SaveChangesAsync();
            }

        }
    }
}
