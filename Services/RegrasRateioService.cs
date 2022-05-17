using Microsoft.EntityFrameworkCore;
using InCorpApp.Data;
using InCorpApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Services.Exceptions;
using Microsoft.Data.SqlClient;
using Blazored.SessionStorage;
using InCorpApp.Security;
using AutoMapper;

using System.Data;


namespace InCorpApp.Services
{
    public class RegrasRateioService : ServiceBase
    {

        private readonly ApplicationDbContext _context;
        private readonly SocioService _socioService;

        public RegrasRateioService(ApplicationDbContext context,
            SocioService socioService,
                         ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper) : base(sessionStorageService, sessionService, mapper)
        {
            _context = context;
            _socioService = socioService;
        }

        public async Task<List<GrupoSocio>> FindAllGrupoSocioAsync(int empId)
        {
            SessionInfo si = await GetSessionAsync();

            List<GrupoSocio> r = new();

            var command = _context.Database.GetDbConnection().CreateCommand();

            command.CommandText =

            "Select GrupoSocio.* from GrupoSocio " +
            "where GrupoSocio.EmpreendimentoId = @id and TenantId = @tid";


            command.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));
            command.Parameters["@id"].Value = empId;

            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;


            await command.Connection.OpenAsync();
            try
            {
                var lu = await command.ExecuteReaderAsync();



                while (await lu.ReadAsync())
                {
                    var s = new GrupoSocio()
                    {
                        Id = (int)lu["id"],
                        EmpreendimentoId = (int)lu["EmpreendimentoId"],
                        Nome = lu["Nome"].ToString(),
                        TenantId = lu["TenantId"].ToString()
                    };
                    r.Add(s);
                };

                lu.Close();
            }
            finally
            {
                command.Connection.Close();
            }
            return r;
        }
        public async Task<GrupoSocio> FindGrupoSocioByIdAsync(int id)
        {
            return await _context.GrupoSocio.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<GrupoSocioView> GetNewGrupoSocioView(int empid)
        {

            GrupoSocioView r = new()
            {
                GrupoSocio = new GrupoSocio() { EmpreendimentoId = empid },
                SociosGrupoView = new List<SocioGrupoView>()
            };

            var lsocios = await FindAllSocioEmppreendimentoAsync(empid);

            foreach (var socioemp in lsocios)
            {
                SocioGrupoView sgv = new();
                r.SociosGrupoView.Add(sgv);
                sgv.SocioEmpreendimentoView = socioemp;
                sgv.Selecionado = false;
                sgv.GrupoSocioEmpreedSocio = new GrupoSocioEmpreedSocio()
                {
                    GrupoSocioId = r.GrupoSocio.Id,
                    EmpreendimentoSocioId = socioemp.EmpreendimentoSocioId
                };
                sgv.Selecionado = false;

            }

            return r;
        }
        public async Task<GrupoSocioView> FindGrupoSocioViewByIdAsync(int id)
        {


            SessionInfo si = await GetSessionAsync();


            GrupoSocioView r = new()
            {
                GrupoSocio = await _context.GrupoSocio.FirstOrDefaultAsync(x => x.Id == id)
            };

            var lsocios = await FindAllSocioEmppreendimentoAsync(r.GrupoSocio.EmpreendimentoId);


            foreach (var socioemp in lsocios)
            {
                SocioGrupoView sgv = new();
                r.SociosGrupoView.Add(sgv);
                sgv.SocioEmpreendimentoView = socioemp;
                sgv.GrupoSocioEmpreedSocio = await _context.GrupoSocioEmpreedSocio.AsNoTracking().FirstOrDefaultAsync(x => x.EmpreendimentoSocioId == socioemp.EmpreendimentoSocio.Id && x.GrupoSocioId == id && x.TenantId == si.TenantId);
                if (sgv.GrupoSocioEmpreedSocio == null)
                {
                    sgv.GrupoSocioEmpreedSocio = new GrupoSocioEmpreedSocio()
                    {
                        GrupoSocioId = r.GrupoSocio.Id,
                        EmpreendimentoSocioId = socioemp.EmpreendimentoSocioId
                    };
                    sgv.Selecionado = false;
                }
                else
                {
                    sgv.Selecionado = true;
                }
            }

            return r;



        }
        public async Task<List<SocioEmpreendimentoView>> FindAllSocioEmppreendimentoAsync(int empId)
        {
            return await _socioService.FindAllSociosByEmpIdAsync(empId);
        }
        public async Task AddGrupoSocioAsync(GrupoSocioView gv)
        {
            SessionInfo si = await GetSessionAsync();
            gv.GrupoSocio.TenantId = si.TenantId;
            _context.GrupoSocio.Add(gv.GrupoSocio);
            _context.Database.BeginTransaction();
            try
            {
                _context.SaveChanges();
                foreach (SocioGrupoView sv in gv.SociosGrupoView)
                {
                    if (sv.Selecionado)
                    {
                        sv.GrupoSocioEmpreedSocio.TenantId = si.TenantId;
                        sv.GrupoSocioEmpreedSocio.GrupoSocioId = gv.GrupoSocio.Id;
                        sv.GrupoSocioEmpreedSocio.EmpreendimentoSocioId = sv.SocioEmpreendimentoView.EmpreendimentoSocio.Id;
                        _context.GrupoSocioEmpreedSocio.Add(sv.GrupoSocioEmpreedSocio);
                    }
                }
                _context.SaveChanges();
                _context.Database.CommitTransaction();
            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;
            }
        }
        public async Task UpdateGrupoSocioAsync(GrupoSocioView gv)
        {

            SessionInfo si = await GetSessionAsync();

            gv.GrupoSocio.TenantId = si.TenantId;

            _context.Database.BeginTransaction();

            try
            {
                _context.GrupoSocio.Update(gv.GrupoSocio);

                List<GrupoSocioEmpreedSocio> lgs = _context.GrupoSocioEmpreedSocio.Where(x => x.GrupoSocioId == gv.GrupoSocio.Id && x.TenantId == si.TenantId).ToList();
                lgs.ForEach(x => _context.Remove(x));
                _context.SaveChanges();

                foreach (SocioGrupoView sv in gv.SociosGrupoView)
                {
                    if (sv.Selecionado)
                    {
                        GrupoSocioEmpreedSocio gs = new()
                        {
                            TenantId = si.TenantId,
                            GrupoSocioId = gv.GrupoSocio.Id,
                            EmpreendimentoSocioId = sv.SocioEmpreendimentoView.EmpreendimentoSocio.Id,
                            Percentual = sv.GrupoSocioEmpreedSocio.Percentual
                        };
                        _context.GrupoSocioEmpreedSocio.Add(gs);
                    }
                }
                _context.SaveChanges();
                _context.Database.CommitTransaction();
            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;
            }

        }
        public async Task ExcluirGrupoSocioAsync(int id)
        {

            SessionInfo si = await GetSessionAsync();

            ProgramacaoGrupoRateioSocios rr = await _context.ProgramacaoGrupoRateioSocios.FirstOrDefaultAsync(x => x.GrupoSocioId == id && x.TenantId == si.TenantId);

            if (rr != null)
            {
                throw new NaoPodeExcluirException("Grupo não pode ser excluído. Pertence ao grupo de rateio ");
            }



            var gs = _context.GrupoSocio.FirstOrDefault(x => x.Id == id);
            if (gs == null)
            {
                throw new NotFoundException("Grupo de socio não cadastrado.");
            }

            _context.Database.BeginTransaction();
            try
            {
                List<GrupoSocioEmpreedSocio> lgs = _context.GrupoSocioEmpreedSocio.Where(x => x.GrupoSocioId == gs.Id && x.TenantId == si.TenantId).ToList();
                lgs.ForEach(x => _context.Remove(x));

                _context.Remove(gs);

                _context.SaveChanges();
                _context.Database.CommitTransaction();

            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;

            }

        }
        public async Task<List<GrupoRateio>> FindAllGrupoRateioAsync(int empId)
        {

            SessionInfo si = await GetSessionAsync();

            List<GrupoRateio> r = new();

            var command = _context.Database.GetDbConnection().CreateCommand();

            command.CommandText =

            "Select GrupoRateio.* from GrupoRateio " +
            "where GrupoRateio.EmpreendimentoId = @id and TenantId = @tid ";


            command.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));
            command.Parameters["@id"].Value = empId;

            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;


            await command.Connection.OpenAsync();

            try
            {
                var lu = await command.ExecuteReaderAsync();
                while (await lu.ReadAsync())
                {
                    var s = new GrupoRateio()
                    {
                        Id = (int)lu["id"],
                        EmpreendimentoId = (int)lu["EmpreendimentoId"],
                        Nome = lu["Nome"].ToString(),
                        TenantId = lu["TenantId"].ToString()
                    };
                    r.Add(s);
                };
                lu.Close();
            }
            finally
            {
                command.Connection.Close();
            }
            return r;

        }
        public async Task<GrupoRateio> FindGrupoRateioByIdAsync(int id)
        {
            return await _context.GrupoRateio.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task AddGrupoRateioAsync(GrupoRateioView gr)
        {
            SessionInfo si = await GetSessionAsync();

            gr.GrupoRateio.Ativo = true;
            gr.GrupoRateio.TenantId = si.TenantId;

            if (gr.GrupoRateio.RegraRateioDefaultId != 0)
            {
                if (await _context.GrupoRateio.AnyAsync(x => x.TenantId == si.TenantId && x.RegraRateioDefaultId == gr.GrupoRateio.RegraRateioDefaultId
                                                         && x.EmpreendimentoId == gr.GrupoRateio.EmpreendimentoId && x.Id != gr.GrupoRateio.Id))
                {
                    throw new Exception("Regra padrão já está associada a outra regra desse empreendimento.");
                }
            }

            _context.GrupoRateio.Add(gr.GrupoRateio);
            _context.Database.BeginTransaction();
            try
            {
                _context.SaveChanges();
                gr.ProgramacaoGrupoRateio.GrupoRateioId = gr.GrupoRateio.Id;
                gr.ProgramacaoGrupoRateio.TenantId = gr.GrupoRateio.TenantId;
                _context.ProgramacaoGrupoRateio.Add(gr.ProgramacaoGrupoRateio);
                _context.SaveChanges();
                _context.Database.CommitTransaction();
            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;
            }
        }
        public async Task UpdateGrupoRateioAsync(GrupoRateioView gr)
        {
            SessionInfo si = await GetSessionAsync();
            if (gr.GrupoRateio.RegraRateioDefaultId != 0)
            {
                if (await _context.GrupoRateio.AnyAsync(x => x.TenantId == si.TenantId && x.RegraRateioDefaultId == gr.GrupoRateio.RegraRateioDefaultId
                                                         && x.EmpreendimentoId == gr.GrupoRateio.EmpreendimentoId && x.Id != gr.GrupoRateio.Id))
                {
                    throw new Exception("Regra padrão já está associada a outra regra desse empreendimento.");
                }
            }
            _context.GrupoRateio.Update(gr.GrupoRateio);
            await _context.SaveChangesAsync();
        }
        public async Task ExcluirGrupoRateioAsync(int id)
        {

            SessionInfo si = await GetSessionAsync();

            ProgramacaoGrupoRateio pg = await _context.ProgramacaoGrupoRateio.FirstOrDefaultAsync(x => x.GrupoRateioId == id && x.TenantId == si.TenantId);

            if (await _context.ProgramacaoGrupoRateioSocios.AnyAsync(x => x.ProgramacaoGrupoRateioId == pg.Id))
            {
                throw new NaoPodeExcluirException("Regra com sócios vinculados não pode ser excluída");
            }

            if (await _context.PlanoConta.AnyAsync(x => x.GrupoRateioId == pg.Id && x.TenantId == si.TenantId))
            {
                throw new NaoPodeExcluirException("Regra com Conta vinculada não pode ser excluída");
            }





            try
            {
                _context.Database.BeginTransaction();

                List<ProgramacaoGrupoRateio> lpr = _context.ProgramacaoGrupoRateio.Where(x => x.GrupoRateioId == id && x.TenantId == si.TenantId).ToList();
                lpr.ForEach(x => _context.Remove(x));

                var gs = _context.GrupoRateio.FirstOrDefault(x => x.Id == id);
                if (gs == null)
                {
                    throw new NotFoundException("Grupo não cadastrado");
                }


                _context.Remove(gs);

                _context.SaveChanges();

                _context.Database.CommitTransaction();

            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;
            }
        }
        public async Task<List<PlanoConta>> FindAllPlanoContaByEmpreendimentoId(int empid)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.PlanoConta.Where(x => x.EmpreendimentoId == empid && x.TenantId == si.TenantId).OrderBy(x => x.Nome).ToListAsync();
        }
        public async Task<List<PlanoContaView>> FindAllPlanoContaViewAsync(int empId)
        {

            SessionInfo si = await GetSessionAsync();

            var command = _context.Database.GetDbConnection().CreateCommand();

            var lr = await _context.GrupoRateio.Where(x => x.TenantId == si.TenantId && x.EmpreendimentoId == empId).ToListAsync();


            command.CommandText = "SELECT PlanoConta.PlanoGerencialId, PlanoConta.EmpreendimentoId, Isnull(PlanoGerencialRGEmpreendimento.RegraRateioId, Isnull( PlanoGerencial.RegraRateioDefaultId,0)) RegraRateioDefaultId, "
                       + "  PlanoConta.Ratear,  PlanoConta.TenantId,  PlanoConta.Id, PlanoConta.Nome, PlanoConta.CodigoExterno,PlanoConta.ContaContabilId, ContaContabil.Nome NomeContaContabil, GrupoRateio.Nome Regrarateio, "
                       + "  PlanoConta.GrupoRateioId, PlanoConta.Tipo, PlanoGerencial.Nome NomePlanoGerencial "
                        + "from PlanoConta PlanoConta "
                        + " inner join PlanoGerencial on PlanoGerencial.Id = PlanoConta.PlanoGerencialId "
                        + " left join PlanoGerencialRGEmpreendimento on PlanoGerencialRGEmpreendimento.PlanoGerencialId = PlanoConta.PlanoGerencialId "
                        + "       and  PlanoGerencialRGEmpreendimento.EmpreendimentoId = PlanoConta.EmpreendimentoId"
                        + " left join ContaContabil on ContaContabil.Id = PlanoConta.ContaContabilId "
                        + " left join GrupoRateio   on GrupoRateio.Id = PlanoConta.GrupoRateioId "
                        + " where PlanoConta.EmpreendimentoId = @e  and PlanoConta.TenantId = @tid   order by PlanoConta.Tipo, PlanoConta.Nome";


            command.Parameters.Add(new SqlParameter("@e", SqlDbType.Int));
            command.Parameters["@e"].Value = empId;

            command.Parameters.Add(new SqlParameter("@tid", SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;


            await command.Connection.OpenAsync();
            List<PlanoContaView> r = new();
            try
            {
                var lu = await command.ExecuteReaderAsync();


                while (await lu.ReadAsync())
                {
                    var u = new PlanoContaView()
                    {
                        Id = (int)lu["Id"],
                        Tipo = (TipoPlanoConta)lu["Tipo"],
                        EmpreendimentoId = (int)lu["EmpreendimentoId"],
                        TenantId = lu["TenantId"].ToString(),
                        ContaContabilId = (int)lu["ContaContabilId"],
                        Ratear = (bool)lu["Ratear"],
                        Nome = lu["Nome"].ToString(),
                        CodigoExterno = lu["CodigoExterno"].ToString(),
                        NomeContaContabil = lu["NomeContaContabil"].ToString(),
                        GrupoRateioId = (int)lu["GrupoRateioId"],
                        RegraRateioDefaultId = (int)lu["RegraRateioDefaultId"],
                        RegraRateio = lu["RegraRateio"].ToString(),
                        PlanoGerencialId = (int)lu["PlanoGerencialId"],
                        NomePlanoGerencial = lu["NomePlanoGerencial"].ToString()
                    };
                    u.RegraRateioDefault = string.Empty;
                    if (u.GrupoRateioId == 0)
                    {
                        if (u.RegraRateioDefaultId != 0)
                        {
                            var rd = lr.FirstOrDefault(x => x.RegraRateioDefaultId == u.RegraRateioDefaultId);
                            if (rd != null)
                            {
                                u.RegraRateioDefault = rd.Nome;
                            }
                        }
                    }
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
        public async Task<PlanoConta> FindPlanoContaByCodigoExternoAsync(int empid, string c)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.PlanoConta.FirstOrDefaultAsync(x => x.CodigoExterno == c && x.EmpreendimentoId == empid && x.TenantId == si.TenantId);
        }
        public async Task<PlanoConta> FindPlanoContaByNomeAsync(int empid, string n)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.PlanoConta.FirstOrDefaultAsync(x => x.Nome == n && x.EmpreendimentoId == empid && x.TenantId == si.TenantId);

        }
        public async Task<Resultado> AddPlanosContasAsync(List<PlanoConta> lp)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var p in lp)
                {
                    p.TenantId = si.TenantId;
                    r = await AddPlanoContaAsync(p);
                    if (!r.Ok)
                    {
                        throw new Exception(r.ErrMsg);
                    }
                }
                await _context.Database.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;

        }
        public async Task<Resultado> AddPlanoContaAsync(PlanoConta c)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (await _context.PlanoConta.AnyAsync(x => x.Nome.ToLower() == c.Nome.ToLower() &&
                                                        x.TenantId == si.TenantId &&
                                                        x.EmpreendimentoId == c.EmpreendimentoId &&
                                                        x.CodigoExterno == c.CodigoExterno
                                                        ))
            {
                r.Ok = false;
                r.ErrMsg = "Nome já cadastrado: " + c.Nome + " código externo: " + c.CodigoExterno;
                return r;
            }

            bool mytrans = (_context.Database.CurrentTransaction == null);
            if (mytrans)
            {
                await _context.Database.BeginTransactionAsync();
            }
        ; try
            {
                c.TenantId = si.TenantId;
                if (c.PlanoGerencial != null && c.PlanoGerencial.Id == 0)
                {
                    c.PlanoGerencial.TenantId = si.TenantId;
                    var pg = await _context.PlanoGerencial.Where(x => x.Nome.ToLower() == c.PlanoGerencial.Nome.ToLower() && x.TenantId == si.TenantId).FirstOrDefaultAsync();
                    if (pg != null)
                    {
                        c.PlanoGerencialId = pg.Id;
                    }
                    else
                    {
                        await _context.PlanoGerencial.AddAsync(c.PlanoGerencial);
                        await _context.SaveChangesAsync();
                        c.PlanoGerencialId = c.PlanoGerencial.Id;
                    }
                }
                if (c.NomeCurto == null || c.NomeCurto == string.Empty)
                {
                    c.NomeCurto = c.Nome;
                }
                c.GrupoRateioDefault = false;
                await _context.PlanoConta.AddAsync(c);
                await _context.SaveChangesAsync();
                if (mytrans)
                {
                    await _context.Database.CommitTransactionAsync();
                }
                r.Ok = true;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                if (mytrans)
                {
                    await _context.Database.RollbackTransactionAsync();
                }
                else
                {
                    throw;
                }
            }
            return r;
        }
        public async Task<Resultado> UpdatePlanoContaAsync(PlanoConta c)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (await _context.PlanoConta.AnyAsync(x => x.Nome.ToLower() == c.Nome.ToLower() &&
                                                        x.CodigoExterno == c.CodigoExterno &&
                                                        x.TenantId == si.TenantId && x.Id != c.Id && x.EmpreendimentoId == c.EmpreendimentoId))
            {
                r.Ok = false;
                r.ErrMsg = "Nome já cadastrado";
                return r;
            }

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =

        "Update PlanoConta set  " +
        "            Tipo = @Tipo, " +
        "            Nome = @Nome, " +
        "            ContaContabilId = @ContaContabilId,  " +
        "            CodigoExterno = @CodigoExterno , " +
        "            GrupoRateioId = @GrupoRateioId,  " +
        "            PlanoGerencialId = @PlanoGerencialId, " +
        "            AporteDistribuicao = @AporteDistribuicao,  " +
        "            Ratear = @Ratear, " +
        "            GrupoRateioDefault = @GrupoRateioDefault, " +
        "            NomeCurto = @NomeCurto " +
        " where Id = @Id  and TenantId = @tid";

            if (c.CodigoExterno == null)
            {
                c.CodigoExterno = string.Empty;
            }
            if (c.NomeCurto == null || c.NomeCurto == string.Empty)
            {
                c.NomeCurto = c.Nome;
            }

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int));
            command.Parameters["@id"].Value = c.Id;

            command.Parameters.Add(new SqlParameter("@tid", SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@Nome", SqlDbType.NVarChar, 100));
            command.Parameters["@Nome"].Value = c.Nome;

            command.Parameters.Add(new SqlParameter("@NomeCurto", SqlDbType.NVarChar, 100));
            command.Parameters["@NomeCurto"].Value = c.NomeCurto;

            command.Parameters.Add(new SqlParameter("@ContaContabilId", SqlDbType.Int));
            command.Parameters["@ContaContabilId"].Value = c.ContaContabilId;

            command.Parameters.Add(new SqlParameter("@CodigoExterno", SqlDbType.NVarChar, 100));
            command.Parameters["@CodigoExterno"].Value = c.CodigoExterno;

            command.Parameters.Add(new SqlParameter("@GrupoRateioId", SqlDbType.NVarChar, 100));
            command.Parameters["@GrupoRateioId"].Value = c.GrupoRateioId;

            command.Parameters.Add(new SqlParameter("@AporteDistribuicao", SqlDbType.Bit));
            command.Parameters["@AporteDistribuicao"].Value = c.AporteDistribuicao;

            command.Parameters.Add(new SqlParameter("@PlanoGerencialId", SqlDbType.NVarChar, 100));
            command.Parameters["@PlanoGerencialId"].Value = c.PlanoGerencialId;


            command.Parameters.Add(new SqlParameter("@Tipo", SqlDbType.Int));
            command.Parameters["@Tipo"].Value = (int)c.Tipo;

            command.Parameters.Add(new SqlParameter("@Ratear", SqlDbType.Bit));
            command.Parameters["@Ratear"].Value = c.Ratear;

            command.Parameters.Add(new SqlParameter("@GrupoRateioDefault", SqlDbType.Bit));
            command.Parameters["@GrupoRateioDefault"].Value = false;

            await command.Connection.OpenAsync();
            command.Transaction = await command.Connection.BeginTransactionAsync();
            try
            {
                await command.ExecuteNonQueryAsync();

                await command.Transaction.CommitAsync();
                r.Ok = true;
            }
            catch (Exception e)
            {
                await command.Transaction.RollbackAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            finally
            {
                await command.Connection.CloseAsync();
            }
            return r;

        }
        public async Task<List<PlanoGerencial>> FindAllPlanoGerencialAsync()
        {
            SessionInfo si = await GetSessionAsync();
            List<PlanoGerencial> r =  await _context.PlanoGerencial.Where(x => x.TenantId == si.TenantId).OrderBy(x => x.TipoPlanoConta).ThenBy(x => x.Nome).ToListAsync();
            var lrp = await _context.RegraRateio.Where(x => x.TenantId == si.TenantId).ToListAsync();
            foreach (var rp in lrp)
            {
                r.Where(x=> x.RegraRateioDefaultId == rp.Id).ToList().ForEach(x => x.NomeRegraRateioDefault = rp.Nome);
            }
            return r;
        }
        public async Task<PlanoGerencial> FindPlanoGerencialByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.PlanoGerencial.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
        }
        public async Task<Resultado> AddPlanoGerencialAsync(PlanoGerencial c)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            if (await _context.PlanoGerencial.AnyAsync(x => x.Nome.ToLower() == c.Nome.ToLower() && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Nome já cadastrado";
                return r;
            }
            if (c.NomeGrupoContaAuxiliar == null && c.TipoPlanoConta == TipoPlanoConta.Auxiliar)
            {
                c.NomeGrupoContaAuxiliar = "Conta auxiliar";
            }
            try
            {
                c.TenantId = si.TenantId;
                await _context.PlanoGerencial.AddAsync(c);
                await _context.SaveChangesAsync();
                r.Ok = true;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;

        }
        public async Task<Resultado> UpdatePlanoGerencialAsync(PlanoGerencial cv)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            if (await _context.PlanoGerencial.AnyAsync(x => x.Nome.ToLower() == cv.Nome.ToLower() && x.TenantId == si.TenantId && x.Id != cv.Id))
            {
                r.Ok = false;
                r.ErrMsg = "Nome já cadastrado";
                return r;
            }
            if (cv.NomeGrupoContaAuxiliar == null && cv.TipoPlanoConta == TipoPlanoConta.Auxiliar)
            {
                cv.NomeGrupoContaAuxiliar = "Conta auxiliar";
            }
            try
            {
                if (!await _context.PlanoGerencial.AnyAsync(x => x.Id == cv.Id && x.TenantId == si.TenantId))
                {
                    throw new NotFoundException("Conta não encontrada.");

                }
                cv.TenantId = si.TenantId;
                await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.PlanoGerencial.Update(cv);
                    var lc = await _context.PlanoConta.Where(x => x.PlanoGerencialId == cv.Id && x.TenantId == si.TenantId && x.Tipo != cv.TipoPlanoConta).ToListAsync();
                    if (lc.Count > 0)
                    {
                        foreach (var it in lc)
                        {
                            it.Tipo = cv.TipoPlanoConta;
                        }
                        _context.PlanoConta.UpdateRange(lc);
                    }
                    _context.SaveChanges();
                    await _context.Database.CommitTransactionAsync();
                    r.Ok = true;
                }
                catch (Exception e)
                {
                    await _context.Database.RollbackTransactionAsync();
                    r.Ok = false;
                    r.ErrMsg = e.Message;
                    return r;
                }
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> DeletePlanoGerencialAsync(PlanoGerencial c)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            try
            {
                if (await _context.PlanoConta.AnyAsync(x => x.PlanoGerencialId == c.Id && x.TenantId == si.TenantId))
                {
                    throw new NaoPodeExcluirException("Conta gerencial com plano de conta vinculado, não pode ser excluída!");
                }
                _context.PlanoGerencial.Remove(c);
                await _context.SaveChangesAsync();
                r.Ok = true;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<PlanoConta> FindPlanoContaByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.PlanoConta.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
        }
        public async Task<int> FindProgramaGrupoRateioIdByCatId(int id)
        {

            var command = _context.Database.GetDbConnection().CreateCommand();

            command.CommandText = "SELECT  ProgramacaoGrupoRateio.Id  from ProgramacaoGrupoRateio "
                        + " inner join ProgramacaoGrupoRateioPlanoConta on ProgramacaoGrupoRateioPlanoConta.ProgramacaoGrupoRateioId = ProgramacaoGrupoRateio.Id "
                        + " where ProgramacaoGrupoRateio.DataInicio <= GETDATE() and ProgramacaoGrupoRateio.DataFim >= getdate() "
                        + " and ProgramacaoGrupoRateioPlanoConta.PlanoContaId = @id ";


            command.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));
            command.Parameters["@id"].Value = id;



            await command.Connection.OpenAsync();
            var lu = await command.ExecuteReaderAsync();

            int r = 0;
            try
            {
                while (await lu.ReadAsync())
                {
                    r = (int)lu["Id"];
                };
            }
            finally
            {
                lu.Close();
                command.Connection.Close();
            }
            return r;




        }
        public async Task ExcluirPlanoContaAsync(PlanoConta c)
        {
            if (await _context.LanctoEmpreendimento.AnyAsync(x => x.PlanoContaId == c.Id))
            {
                throw new NaoPodeExcluirException("Conta com lançamentos não pode ser excluída!");
            }
            if (await _context.MovtoBanco.AnyAsync(x => x.PlanoContaId == c.Id))
            {
                throw new NaoPodeExcluirException("Conta com lançamentos não pode ser excluída!");
            }
            if (await _context.Titulo.AnyAsync(x => x.PlanoContaId == c.Id))
            {
                throw new NaoPodeExcluirException("Conta com lançamentos não pode ser excluída!");
            }
            _context.PlanoConta.Remove(c);
            await _context.SaveChangesAsync();

        }
        public async Task<List<PlanoConta>> FindAllPlanoContaAsync(int empid)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.PlanoConta.AsNoTracking().Where(x => x.EmpreendimentoId == empid && x.TenantId == si.TenantId).OrderBy(z => z.Tipo).OrderBy(z => z.Nome).ToListAsync();
        }
        public async Task<GrupoRateio> FindGrupoRateioByProgramacaoIdAsync(int id)
        {
            ProgramacaoGrupoRateio gr = await _context.ProgramacaoGrupoRateio.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (gr != null)
            {
                return await _context.GrupoRateio.AsNoTracking().FirstOrDefaultAsync(x => x.Id == gr.GrupoRateioId);
            }
            return null;

        }
        public async Task<List<ProgramacaoGrupoRateio>> FindAllProgramacaoByGrupoRateioIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();

            return await _context.ProgramacaoGrupoRateio.Where(x => x.GrupoRateioId == id).OrderByDescending(z => z.DataInicio).ToListAsync();
        }
        public async Task<ProgramacaoGrupoRateio> FindProgramacaoGrupoRateioByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();

            return await _context.ProgramacaoGrupoRateio.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
        }
        public async Task<PlanoContaRegraRateio> GetProgramacaoGrupoRateioByPlanoContaIdAsync(PlanoConta pc, DateTime dtref, int grupoRateioId = 0)
        {
            SessionInfo si = await GetSessionAsync();
            int regradefaultid = 0;
            GrupoRateio gr = null;
            if (grupoRateioId != 0)
            {
                gr = await _context.GrupoRateio.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == grupoRateioId);
            }
            else
            {
                if (pc.GrupoRateioId == 0)
                {
                    RegraRateio rr = null;
                    var pg = await _context.PlanoGerencial.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == pc.PlanoGerencialId);
                    if (pg != null)
                    {
                        var pge = await _context.PlanoGerencialRGEmpreendimento.FirstOrDefaultAsync(x => x.TenantId == si.TenantId
                                                                                                && x.EmpreendimentoId == pc.EmpreendimentoId
                                                                                                && x.PlanoGerencialId == pc.PlanoGerencialId);
                        if (pge != null)
                        {
                            regradefaultid = pge.RegraRateioId;
                        }
                        else
                        {
                            regradefaultid = pg.RegraRateioDefaultId;
                        }
                        if (regradefaultid != 0)
                        {
                            rr = await _context.RegraRateio.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == regradefaultid);
                            if (rr != null)
                            {
                                gr = await _context.GrupoRateio.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.RegraRateioDefaultId == rr.Id && x.EmpreendimentoId == pc.EmpreendimentoId);
                                if (gr != null)
                                {
                                    bool mytrans = (_context.Database.CurrentTransaction == null);
                                    if (mytrans)
                                    {
                                        await _context.Database.BeginTransactionAsync();
                                    }
                                    try
                                    {
                                        pc.GrupoRateioDefault = true;
                                        _context.PlanoConta.Update(pc);
                                        await _context.SaveChangesAsync();
                                        if (mytrans)
                                        {
                                            await _context.Database.CommitTransactionAsync();
                                        }
                                    }
                                    catch
                                    {
                                        if (mytrans)
                                        {
                                            await _context.Database.RollbackTransactionAsync();
                                            throw;
                                        }
                                        else
                                        {
                                            throw;
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }

            PlanoContaRegraRateio r = null;

            var command = _context.Database.GetDbConnection().CreateCommand();

            if (gr == null)
            {
                command.CommandText =
            "            SELECT  " +
            "          GrupoRateio.Id GrupoRateioId, " +
            "          ProgramacaoGrupoRateio.Id ProgramacaoGrupoRateioId, " +
            "          ProgramacaoGrupoRateio.DataInicio," +
            "		   ProgramacaoGrupoRateio.DataFim " +
            "          from PlanoConta " +
            "          inner join GrupoRateio on GrupoRateio.Id = PlanoConta.GrupoRateioId " +
            "          inner join ProgramacaoGrupoRateio on ProgramacaoGrupoRateio.GrupoRateioId = GrupoRateio.Id " +
            "          where PlanoConta.Id = @catid " +
            "            and ProgramacaoGrupoRateio.DataInicio <= Convert(date, @dt1,112)  and ProgramacaoGrupoRateio.DataFim >= Convert(date, @dt2,112)  " +
            "      and PlanoConta.TenantId = @tid and ProgramacaoGrupoRateio.TenantId = @tid1 ";
                command.Parameters.Add(new SqlParameter("@catid", System.Data.SqlDbType.Int));
                command.Parameters["@catid"].Value = pc.Id;
            }
            else
            {
                command.CommandText =
                "          SELECT  " +
                "          GrupoRateio.Id GrupoRateioId, " +
                "          ProgramacaoGrupoRateio.Id ProgramacaoGrupoRateioId, " +
                "          ProgramacaoGrupoRateio.DataInicio," +
                "		   ProgramacaoGrupoRateio.DataFim " +
                "          from GrupoRateio " +
                "          inner join ProgramacaoGrupoRateio on ProgramacaoGrupoRateio.GrupoRateioId = GrupoRateio.Id " +
                "          where GrupoRateio.Id = @regradefaultid " +
                "            and ProgramacaoGrupoRateio.DataInicio <= Convert(date, @dt1,112)  and ProgramacaoGrupoRateio.DataFim >= Convert(date, @dt2,112)  " +
                "      and GrupoRateio.TenantId = @tid and ProgramacaoGrupoRateio.TenantId = @tid1 ";
                command.Parameters.Add(new SqlParameter("@regradefaultid", System.Data.SqlDbType.Int));
                command.Parameters["@regradefaultid"].Value = gr.Id;
            }

            command.Parameters.Add(new SqlParameter("@dt1", System.Data.SqlDbType.VarChar, 8));
            command.Parameters["@dt1"].Value = dtref.ToString("yyyyMMdd");
            command.Parameters.Add(new SqlParameter("@dt2", System.Data.SqlDbType.VarChar, 8));
            command.Parameters["@dt2"].Value = dtref.ToString("yyyyMMdd");
            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@tid1", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid1"].Value = si.TenantId;

            await command.Connection.OpenAsync();
            try
            {
                var lu = await command.ExecuteReaderAsync();
                while (await lu.ReadAsync())
                {
                    r = new PlanoContaRegraRateio()
                    {
                        PlanoContaId = pc.Id,
                        ProgramacaoGrupoRateioId = (int)lu["ProgramacaoGrupoRateioId"],
                        DataInicio = (DateTime)lu["DataInicio"],
                        DataFim = (DateTime)lu["DataFim"],
                        GrupoRateioId = (int)lu["GrupoRateioId"]
                    };
                }

                command.Connection.Close();
            }
            catch
            {
                command.Connection.Close();
                throw;
            }

            return r;

        }
        public async Task<ProgramacaoGrupoRegraTemp> GetProgramacaoGrupoRateioSociosByIdAsync(int progid, DateTime dt)
        {

            SessionInfo si = await GetSessionAsync();

            ProgramacaoGrupoRegraTemp r = new();

            var command = _context.Database.GetDbConnection().CreateCommand();

            command.CommandText =



        "  SELECT  " +
        "         ProgramacaoGrupoRateio.Id ProgramacaoGrupoRateioId, " +
        "         ProgramacaoGrupoRateio.DataInicio, " +
        "         ProgramacaoGrupoRateio.DataFim, " +
        "         GrupoRateio.Id GrupoRateioId, " +
        "         GrupoRateio.Nome NomeRegra, " +
        "         GrupoSocio.Id GrupoSocioId, " +
        "         GrupoSocio.Nome NomeGrupoSocio, " +
        "         ProgramacaoGrupoRateioSocios.Percentual GrupoSocioPercentual, " +
        "         GrupoSocio.UtilzarPercentualContrato,  " +
        "         GrupoSocioEmpreedSocio.Percentual GrupoSocioPercentual, " +
        "         EmpreendimentoSocioParticipacao.Cotas CotasSocio, " +
        "         EmpreendimentoTotalCota.TotalCotas,  " +
        "         GrupoSocioTotalCota.GrupoSocioTotalCotas GrupoSocioTotalCotas, " +
        "         EmpreendimentoSocioParticipacao.Cotas * 100.0 / (case when EmpreendimentoTotalCota.TotalCotas = 0 then 1.0 else EmpreendimentoTotalCota.TotalCotas end)  EmpreendimentoSocioPercentual,  " +
        "         EmpreendimentoSocio.Nome NomeSocio, " +
        "         EmpreendimentoSocioParticipacao.EmpreendimentoSocioId,  " +
        "         ProgramacaoGrupoRateioSocios.Percentual / 100. * " +
        "         ( " +
        "         Case " +
        "            when GrupoSocio.UtilzarPercentualContrato = 1 then " +
        "            EmpreendimentoSocioParticipacao.Cotas / ( case when GrupoSocioTotalCota.GrupoSocioTotalCotas = 0 then 1 else GrupoSocioTotalCota.GrupoSocioTotalCotas end ) " +
        "            else " +
        "                GrupoSocioEmpreedSocio.Percentual / 100 " +
        "            end " +
        "         )  " +
        "          PercentualRateio " +
        "         from ProgramacaoGrupoRateio " +
        "         inner join ProgramacaoGrupoRateioSocios on ProgramacaoGrupoRateioSocios.ProgramacaoGrupoRateioId = ProgramacaoGrupoRateio.Id " +
        "         inner join GrupoRateio on GrupoRateio.Id = ProgramacaoGrupoRateio.GrupoRateioId " +
        "         inner join Empreendimento on Empreendimento.Id = GrupoRateio.EmpreendimentoId and Empreendimento.TenantId = @tid1 " +
        "         inner join GrupoSocio on GrupoSocio.Id = ProgramacaoGrupoRateioSocios.GrupoSocioId " +
        "         inner join GrupoSocioEmpreedSocio on GrupoSocioEmpreedSocio.GrupoSocioId = GrupoSocio.Id and GrupoSocioEmpreedSocio.TenantId = @tid2 " +
        "         inner join EmpreendimentoSocio on EmpreendimentoSocio.Id = GrupoSocioEmpreedSocio.EmpreendimentoSocioId " +
        " " +
        "         inner join EmpreendimentoSocioParticipacao on EmpreendimentoSocioParticipacao.EmpreendimentoSocioId = EmpreendimentoSocio.Id " +
        "         inner join Socio on Socio.Id = EmpreendimentoSocio.SocioId " +
        "         inner join(Select Empreendimento.Id EmpreendimentoId, Sum(EmpreendimentoSocioParticipacao.Cotas) TotalCotas from Empreendimento " +
        "                       inner join EmpreendimentoSocio on EmpreendimentoSocio.EmpreendimentoId = Empreendimento.Id " +
        " " +
        "                       inner join EmpreendimentoSocioParticipacao on EmpreendimentoSocioParticipacao.EmpreendimentoSocioId = EmpreendimentoSocio.Id " +
        "                     and EmpreendimentoSocioParticipacao.DataInicioDist <= Convert(date, @dti, 112) " +
        "                     and EmpreendimentoSocioParticipacao.DataFimDist >= Convert(date, @dtf, 112) " +
        " " +
        "                      group by Empreendimento.Id  ) " +
        " " +
        "                     EmpreendimentoTotalCota on  EmpreendimentoTotalCota.EmpreendimentoId = EmpreendimentoSocio.EmpreendimentoId " +
        " " +
        "         inner join(Select Empreendimento.Id EmpreendimentoId, GrupoSocio.Id GrupoSocioId, Sum(EmpreendimentoSocioParticipacao.Cotas) GrupoSocioTotalCotas from Empreendimento " +
        " " +
        "                      inner join GrupoSocio on GrupoSocio.EmpreendimentoId = Empreendimento.Id " +
        "                      inner join GrupoSocioEmpreedSocio on GrupoSocioEmpreedSocio.GrupoSocioId = GrupoSocio.Id " +
        " " +
        "                      inner join EmpreendimentoSocio on EmpreendimentoSocio.Id = GrupoSocioEmpreedSocio.EmpreendimentoSocioId " +
        " " +
        "                      inner join EmpreendimentoSocioParticipacao on EmpreendimentoSocioParticipacao.EmpreendimentoSocioId = EmpreendimentoSocio.Id " +
        "                     and EmpreendimentoSocioParticipacao.DataInicioDist <= Convert(date, @dti, 112) " +
        "                     and EmpreendimentoSocioParticipacao.DataFimDist >= Convert(date, @dtf, 112) " +
        "                       group by Empreendimento.Id , GrupoSocio.Id ) " +
        " " +
        "                    GrupoSocioTotalCota on  GrupoSocioTotalCota.EmpreendimentoId = Empreendimento.Id and GrupoSocioTotalCota.GrupoSocioId = GrupoSocio.Id " +
        "         			where ProgramacaoGrupoRateio.Id = @progid  " +
        "     and EmpreendimentoSocioParticipacao.DataInicioDist <= Convert(date, @dti,112) and EmpreendimentoSocioParticipacao.DataFimDist >= Convert(date, @dtf,112) and EmpreendimentoSocioParticipacao.TenantId = @tid3  " +
        "     and ProgramacaoGrupoRateio.TenantId = @tid ";

            command.Parameters.Add(new SqlParameter("@progid", System.Data.SqlDbType.Int));
            command.Parameters["@progid"].Value = progid;
            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@tid1", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid1"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@tid2", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid2"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@tid3", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid3"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@dti", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dti"].Value = dt.ToString("yyyyMMdd");
            command.Parameters.Add(new SqlParameter("@dtf", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtf"].Value = dt.ToString("yyyyMMdd");
            await command.Connection.OpenAsync();
            try
            {
                var lu = await command.ExecuteReaderAsync();

                while (await lu.ReadAsync())
                {

                    ProgramacaoGrupoRateioSociosDetalhe d = new();
                    r.ProgramacaoGrupoRateioId = (int)lu["ProgramacaoGrupoRateioId"];
                    r.DataInicio = (DateTime)lu["DataInicio"];
                    r.DataFim = (DateTime)lu["DataFim"];


                    d.GrupoRateioId = (int)lu["GrupoRateioId"];
                    d.NomeRegra = lu["NomeRegra"].ToString();
                    d.GrupoSocioId = (int)lu["GrupoSocioId"];
                    d.NomeGrupoSocio = lu["NomeGrupoSocio"].ToString();
                    d.UtilzarPercentualContrato = (bool)lu["UtilzarPercentualContrato"];
                    d.GrupoSocioPercentual = (double)lu["GrupoSocioPercentual"];
                    d.CotasSocio = (double)lu["CotasSocio"];
                    d.TotalCotas = (double)lu["TotalCotas"];
                    d.GrupoSocioTotalCotas = (double)lu["GrupoSocioTotalCotas"];
                    d.EmpreendimentoSocioPercentual = (double)lu["EmpreendimentoSocioPercentual"];
                    d.NomeSocio = lu["NomeSocio"].ToString();
                    d.EmpreendimentoSocioId = (int)lu["EmpreendimentoSocioId"];
                    d.PercentualRateio = (double)lu["PercentualRateio"];
                    r.Detalhes.Add(d);

                };
                lu.Close();
                command.Connection.Close();
            }
            catch
            {
                command.Connection.Close();
                throw;
            }
            finally
            {

            }
            return r;
        }
        public async Task AddProgramacaoGrupoRateioAsync(ProgramacaoGrupoRateioView p, GrupoRateio es)
        {
            p.ProgramacaoGrupoRateio.DataInicio = Utils.UtilsClass.GetInicio(p.ProgramacaoGrupoRateio.DataInicio);
            p.ProgramacaoGrupoRateio.DataFim = Utils.UtilsClass.GetUltimo(p.ProgramacaoGrupoRateio.DataFim);

            if (p.ProgramacaoGrupoRateio.DataFim <= p.ProgramacaoGrupoRateio.DataInicio)
            {
                throw new Exception("Data de inicio inválida!");
            }


            SessionInfo si = await GetSessionAsync();

            if (await _context.ProgramacaoGrupoRateio.AnyAsync(x => x.GrupoRateioId == es.Id && x.TenantId == si.TenantId && ((x.DataInicio <= p.ProgramacaoGrupoRateio.DataInicio && x.DataFim >= p.ProgramacaoGrupoRateio.DataInicio) ||
            (p.ProgramacaoGrupoRateio.DataInicio < x.DataFim))))
            {
                throw new Exception("Data de inicio inválida!");
            }

            p.ProgramacaoGrupoRateio.TenantId = si.TenantId;
            p.ProgramacaoGrupoRateio.GrupoRateioId = es.Id;

            await _context.ProgramacaoGrupoRateio.AddAsync(p.ProgramacaoGrupoRateio);

            await _context.SaveChangesAsync();



            foreach (var ps in p.ProgramacaoGrupoRateioSociosViews)
            {
                if (ps.Selecionado)
                {
                    ProgramacaoGrupoRateioSocios nps = new()
                    {
                        TenantId = si.TenantId,
                        GrupoSocioId = ps.GrupoSocioId,
                        ProgramacaoGrupoRateioId = p.ProgramacaoGrupoRateio.Id,
                        Percentual = ps.ProgramacaoGrupoRateioSocios.Percentual
                    };
                    _context.ProgramacaoGrupoRateioSocios.Add(nps);
                }

            }

        }
        public async Task UpdateProgramacaoGrupoRateioAsync(ProgramacaoGrupoRateioView p)
        {
            SessionInfo si = await GetSessionAsync();

            p.ProgramacaoGrupoRateio.DataInicio = Utils.UtilsClass.GetInicio(p.ProgramacaoGrupoRateio.DataInicio);
            p.ProgramacaoGrupoRateio.DataFim = Utils.UtilsClass.GetUltimo(p.ProgramacaoGrupoRateio.DataFim);

            if (p.ProgramacaoGrupoRateio.DataFim <= p.ProgramacaoGrupoRateio.DataInicio)
            {
                throw new Exception("Data de inicio inválida!");
            }

            if (await _context.ProgramacaoGrupoRateio.AnyAsync(x => x.GrupoRateioId == p.ProgramacaoGrupoRateio.GrupoRateioId && x.TenantId == si.TenantId
              && x.Id != p.ProgramacaoGrupoRateio.Id &&
            ((x.DataInicio <= p.ProgramacaoGrupoRateio.DataInicio && x.DataFim >= p.ProgramacaoGrupoRateio.DataInicio) ||
            (p.ProgramacaoGrupoRateio.DataInicio < x.DataFim))))
            {
                throw new Exception("Data de inicio inválida!");
            }

            p.ProgramacaoGrupoRateio.TenantId = si.TenantId;

            var pu = await FindProgramacaoGrupoRateioByIdAsync(p.ProgramacaoGrupoRateio.Id);
            if (pu == null)
            {
                throw new NotFoundException("Programação não cadastrada");
            }

            pu.DataInicio = p.ProgramacaoGrupoRateio.DataInicio;
            pu.DataFim = p.ProgramacaoGrupoRateio.DataFim;

            _context.ProgramacaoGrupoRateio.Update(pu);

            var lps = await _context.ProgramacaoGrupoRateioSocios.Where(x => x.ProgramacaoGrupoRateioId == pu.Id && x.TenantId == si.TenantId).ToListAsync();
            foreach (var ps in lps)
            {
                _context.ProgramacaoGrupoRateioSocios.Remove(ps);
            }

            foreach (var ps in p.ProgramacaoGrupoRateioSociosViews)
            {
                if (ps.Selecionado)
                {
                    ProgramacaoGrupoRateioSocios nps = new()
                    {
                        TenantId = si.TenantId,
                        GrupoSocioId = ps.GrupoSocioId,
                        ProgramacaoGrupoRateioId = pu.Id,
                        Percentual = ps.ProgramacaoGrupoRateioSocios.Percentual
                    };
                    _context.ProgramacaoGrupoRateioSocios.Add(nps);
                }

                await _context.SaveChangesAsync();

            }

        }
        public async Task DeleteProgramacaoGrupoRateioAsync(ProgramacaoGrupoRateio p)
        {
            _context.ProgramacaoGrupoRateio.Remove(p);
            await _context.SaveChangesAsync();
        }
        public async Task<List<ProgramacaoGrupoRateioSociosView>> FindAllProgramacaoGrupoRateioSociosViewById(int empid, int id)
        {
            List<ProgramacaoGrupoRateioSociosView> r = new();

            SessionInfo si = await GetSessionAsync();


            var command = _context.Database.GetDbConnection().CreateCommand();

            command.CommandText =


        "select GrupoSocio.*, " +
        "Isnull(ProgramacaoGrupoRateioSocios.ProgramacaoGrupoRateioId, 0) ProgramacaoGrupoRateioId , " +
        "Isnull(ProgramacaoGrupoRateioSocios.Percentual, 0.0) Percentual " +
        "from GrupoSocio " +
        "left join ProgramacaoGrupoRateioSocios on ProgramacaoGrupoRateioSocios.GrupoSocioId = GrupoSocio.Id " +
        "and ProgramacaoGrupoRateioSocios.ProgramacaoGrupoRateioId = @id and ProgramacaoGrupoRateioSocios.TenantId = @tid2 " +
        "where GrupoSocio.EmpreendimentoId = @empid  and GrupoSocio.TenantId = @tid ";

            command.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));
            command.Parameters["@id"].Value = id;


            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = empid;

            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@tid2", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid2"].Value = si.TenantId;

            await command.Connection.OpenAsync();
            try
            {
                var lu = await command.ExecuteReaderAsync();
                while (await lu.ReadAsync())
                {
                    var s = new ProgramacaoGrupoRateioSociosView()
                    {
                        GrupoSocioId = (int)lu["Id"],
                        ProgramacaoGrupoRateioId = (int)lu["ProgramacaoGrupoRateioId"]
                    };
                    s.Selecionado = !(s.ProgramacaoGrupoRateioId == 0);
                    s.GrupoSocio.Id = s.GrupoSocioId;
                    s.GrupoSocio.Nome = lu["Nome"].ToString();
                    s.ProgramacaoGrupoRateioSocios.Percentual = (double)lu["Percentual"];
                    s.ProgramacaoGrupoRateioSocios.ProgramacaoGrupoRateioId = (int)lu["ProgramacaoGrupoRateioId"];

                    r.Add(s);
                };
                lu.Close();
            }
            finally
            {
                command.Connection.Close();
            }
            return r;

        }
        public async Task<Socio> FindSocioByEmpSocioIdAsync(int empsocioid)
        {
            return await _socioService.FindSocioByEmpSocioIdAsync(empsocioid);
        }
        public async Task<List<RegraRateio>> FindAllRegraRateioAsync()
        {
            SessionInfo si = await GetSessionAsync();
            _context.ChangeTracker.Clear();
            return await _context.RegraRateio.Where(x => x.TenantId == si.TenantId).OrderBy(x => x.Nome).ToListAsync();
        }
        public async Task<RegraRateio> FindRegraRateioByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.RegraRateio.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
        }
        public async Task<Resultado> AddRegraRateioAsync(RegraRateio rr)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            rr.TenantId = si.TenantId;
            try
            {
                await _context.RegraRateio.AddAsync(rr);
                await _context.SaveChangesAsync();
                r.Ok = true;
                return r;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<Resultado> UpdateRegraRateioAsync(RegraRateio rr)
        {
            Resultado r = new();
            try
            {
                _context.RegraRateio.Update(rr);
                await _context.SaveChangesAsync();
                r.Ok = true;
                return r;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<Resultado> ExcluirRegraRateioAsync(RegraRateio rr)
        {

            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            var l = await _context.GrupoRateio.Where(x => x.RegraRateioDefaultId == rr.Id && x.TenantId == si.TenantId).ToListAsync();
            if (l.Count > 0)
            {
                string s = string.Empty;
                foreach (var g in l)
                {
                    s += "  " + g.Nome;
                }
                r.Ok = false;
                r.ErrMsg = "Regra padrão está vinculada a(s) regras em empreendimentos => " + s;
                return r;
            }

            var lg = await _context.PlanoGerencial.Where(x => x.RegraRateioDefaultId == rr.Id && x.TenantId == si.TenantId).ToListAsync();
            if (lg.Count > 0)
            {
                string s = string.Empty;
                foreach (var g in l)
                {
                    s += "  " + g.Nome;
                }
                r.Ok = false;
                r.ErrMsg = "Regra padrão está vinculada a(s) contas gerenciais => " + s;
                return r;
            }
            try
            {
                _context.RegraRateio.Remove(rr);
                await _context.SaveChangesAsync();
                r.Ok = true;
                return r;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }

    }
}