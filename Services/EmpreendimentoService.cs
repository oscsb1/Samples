using Microsoft.EntityFrameworkCore;
using InCorpApp.Data;
using InCorpApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Services.Exceptions;
using Blazored.SessionStorage;
using InCorpApp.Security;
using InCorpApp.Constantes;
using AutoMapper;
using Microsoft.Data.SqlClient;
using System;

namespace InCorpApp.Services
{
    public class EmpreendimentoService : ServiceBase
    {
        private readonly ApplicationDbContext _context;
        public EmpreendimentoService(ApplicationDbContext context,
 ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper) : base(sessionStorageService, sessionService, mapper)
        {
            _context = context;

        }
        public async Task<List<Empreendimento>> FindAllAsync()
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Empreendimento.Where(x => x.TenantId == si.TenantId).OrderBy(x => x.Nome).ToListAsync();
        }
        public async Task<Resultado> InsertAsync(Empreendimento obj)
        {
            Resultado r = new() { Ok = false };

            SessionInfo si = await GetSessionAsync();

            obj.CNPJ = obj.CNPJ.Trim();
            obj.CNPJ = obj.CNPJ.Replace(".", "").Replace("-", "");
            obj.TenantId = si.TenantId;
            obj.DataInicioOperacao = Utils.UtilsClass.GetInicio(obj.DataInicioOperacao);
            obj.DataFimOperacao = Utils.UtilsClass.GetUltimo(obj.DataInicioOperacao.AddYears(30));

            await _context.AddAsync(obj);
            await _context.SaveChangesAsync();

            r.Ok = true;
            return r;
        }
        public async Task<Empreendimento> FindByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Empreendimento.FirstOrDefaultAsync(obj => obj.Id == id && obj.TenantId == si.TenantId);
        }
        public async Task<bool> AnyAsync(int id)
        {

            SessionInfo si = await GetSessionAsync();
            return await _context.Empreendimento.AnyAsync(obj => obj.Id == id && obj.TenantId == si.TenantId);
        }
        public async Task<Resultado> UpdateAsync(Empreendimento obj)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            bool hasAny = await _context.Empreendimento.AnyAsync(x => x.Id == obj.Id && obj.TenantId == si.TenantId);
            if (!hasAny)
            {
                throw new NotFoundException("Codigo não encontrado.");
            }

            obj.CNPJ = obj.CNPJ.Trim();
            obj.CNPJ = obj.CNPJ.Replace(".", "").Replace("-", "");
            obj.TenantId = si.TenantId;

            obj.DataInicioOperacao = Utils.UtilsClass.GetInicio(obj.DataInicioOperacao);
            obj.DataFimOperacao = Utils.UtilsClass.GetUltimo(obj.DataFimOperacao);


            _context.Update(obj);
            await _context.SaveChangesAsync();
            r.Ok = true;
            return r;

        }
        public async Task<Resultado> DeleteAsync(Empreendimento obj)
        {
            SessionInfo si = await GetSessionAsync();

            Resultado r = new();

            bool hasAny = await _context.Empreendimento.AnyAsync(x => x.Id == obj.Id && obj.TenantId == si.TenantId);
            if (!hasAny)
            {
                r.Ok = false;
                r.ErrMsg = Constante.ItemaNaoCadastrado("Empreendimento");
                return r;
            }

            hasAny = await _context.EmpreendimentoSocio.AnyAsync(x => x.EmpreendimentoId == obj.Id && obj.TenantId == si.TenantId);
            if (hasAny)
            {
                r.Ok = false;
                r.ErrMsg = Constante.ItemaNaoPodeSerExcluido("Empreendimento");
                return r;
            }

            hasAny = await _context.UnidadeEmpreendimento.AnyAsync(x => x.EmpreendimentoId == obj.Id && obj.TenantId == si.TenantId);
            if (hasAny)
            {
                r.Ok = false;
                r.ErrMsg = Constante.ItemaNaoPodeSerExcluido("Empreendimento");
                return r;
            }

            hasAny = await _context.ContaCorrente.AnyAsync(x => x.EmpreendimentoId == obj.Id && obj.TenantId == si.TenantId);
            if (hasAny)
            {
                r.Ok = false;
                r.ErrMsg = Constante.ItemaNaoPodeSerExcluido("Empreendimento");
                return r;
            }

            hasAny = await _context.Periodo.AnyAsync(x => x.EmpreendimentoId == obj.Id && obj.TenantId == si.TenantId);
            if (hasAny)
            {
                r.Ok = false;
                r.ErrMsg = Constante.ItemaNaoPodeSerExcluido("Empreendimento");
                return r;
            }


            _context.Empreendimento.Remove(obj);
            _context.SaveChanges();

            r.Ok = true;
            return r;

        }
        public async Task<List<UnidadeEmpreendimento>> FindAllUnidadesAsync(int empid)
        {

            SessionInfo si = await GetSessionAsync();
            return await _context.UnidadeEmpreendimento.Where(x => x.EmpreendimentoId == empid && x.TenantId == si.TenantId).ToListAsync();
        }
        public async Task ExluirUnidadeAsync(int empid, int id)
        {
            SessionInfo si = await GetSessionAsync();

            var u = await _context.UnidadeEmpreendimento.FirstAsync(x => x.Id == id && x.TenantId == si.TenantId && x.EmpreendimentoId == empid);
            if (u == null)
            {
                return;
            }

            _context.UnidadeEmpreendimento.Remove(u);
            await _context.SaveChangesAsync();
        }
        public async Task<UnidadeEmpreendimento> FindUnidadeByCodigoExternoAsync(int empid, string ce)
        {
            SessionInfo si = await GetSessionAsync();
            var u = await _context.UnidadeEmpreendimento.FirstOrDefaultAsync(x => x.CodigoExterno == ce && x.TenantId == si.TenantId && x.EmpreendimentoId == empid);
            return u;
        }
        public async Task<Resultado> IncluirUnidadeAsync(UnidadeEmpreendimento u, int empid)
        {
            SessionInfo si = await GetSessionAsync();

            Resultado r = new() { Ok = true };
            if (await _context.UnidadeEmpreendimento.AnyAsync(x => x.EmpreendimentoId == empid && x.Nro == u.Nro && x.Quadra == u.Quadra && x.Torre == u.Torre))
            {
                r.ErrMsg = "Código da unidade duplicado";
                r.Ok = false;
                return r;
            }

            u.EmpreendimentoId = empid;
            u.TenantId = si.TenantId;
            if (u.CodigoExterno == null)
            {
                u.CodigoExterno = string.Empty;
            }
            if (u.Andar == null)
            {
                u.Andar = string.Empty;
            }
            if (u.Nro == null)
            {
                u.Nro = string.Empty;
            }
            if (u.Quadra == null)
            {
                u.Quadra = string.Empty;
            }
            if (u.Torre == null)
            {
                u.Torre = string.Empty;
            }
            if (u.Nro != string.Empty && u.CodigoExterno == string.Empty)
            {
                u.CodigoExterno = u.Nro;
            }
            _context.UnidadeEmpreendimento.Add(u);
            await _context.SaveChangesAsync();

            return r;
        }
        public async Task<Resultado> AtualizarUnidadesAsync(UnidadeEmpreendimento u)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new() { Ok = true };
            u.TenantId = si.TenantId;
            if (await _context.UnidadeEmpreendimento.AnyAsync(x => x.EmpreendimentoId == u.EmpreendimentoId && x.Nro == u.Nro && x.Id != u.Id && x.Quadra == u.Quadra))
            {
                r.ErrMsg = "Código da unidade duplicado";
                r.Ok = false;
                return r;
            }
            if (u.CodigoExterno == null)
            {
                u.CodigoExterno = string.Empty;
            }
            if (u.Andar == null)
            {
                u.Andar = string.Empty;
            }
            if (u.Nro == null)
            {
                u.Nro = string.Empty;
            }
            if (u.Quadra == null)
            {
                u.Quadra = string.Empty;
            }
            if (u.Torre == null)
            {
                u.Torre = string.Empty;
            }
            if (u.Nro != string.Empty && u.CodigoExterno == string.Empty)
            {
                u.CodigoExterno = u.Nro;
            }
            _context.UnidadeEmpreendimento.Update(u);
            await _context.SaveChangesAsync();
            return r;
        }
        public void Map(UnidadeEmpreendimento o, UnidadeEmpreendimento d)
        {
            _mapper.Map(o, d);
        }
        public async Task<List<ContaCorrente>> FindAllCCViewByEmpIdAsync(int empid)
        {
            FinanceService bs = new(_context, _sessionStorageService, _sessionService, _mapper);
            return await bs.FindAllCCViewByEmpIdAsync(empid);
        }
        public async Task<ContaCorrente> FindCCByIdAsync(int id)
        {
            FinanceService bs = new(_context, _sessionStorageService, _sessionService, _mapper);
            return await bs.FindCCByIdAsync(id);
        }
        public async Task UpdateCCAsync(ContaCorrente cc)
        {
            FinanceService bs = new(_context, _sessionStorageService, _sessionService, _mapper);
            await bs.UpdateCCAsync(cc);
        }
        public async Task AddCCAsync(ContaCorrente cc)
        {
            FinanceService bs = new(_context, _sessionStorageService, _sessionService, _mapper);
            await bs.AddCCAsync(cc);
        }
        public async Task<List<Banco>> FindAllBancosAsync()
        {
            FinanceService bs = new(_context, _sessionStorageService, _sessionService, _mapper);
            return await bs.FindAllBancosAsync();
        }
        public async Task DeleteCCAsync(ContaCorrente cc)
        {
            FinanceService bs = new(_context, _sessionStorageService, _sessionService, _mapper);
            await bs.DeleteCCAsync(cc);
        }
        public async Task<bool> ReabrirAsync(Empreendimento o)
        {
            o.Status = StatusEmpreendimento.Ativo;
            _context.Empreendimento.Update(o);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> MudarStatusAsync(Empreendimento o, StatusEmpreendimento ns)
        {

            if (ns == StatusEmpreendimento.Bloqueado)
            {
                if (o.Status != StatusEmpreendimento.Ativo)
                {
                    return false;
                }
            }

            if (ns == StatusEmpreendimento.Ativo)
            {
                if (o.Status != StatusEmpreendimento.Bloqueado)
                {
                    return false;
                }
            }

            o.Status = ns;

            _context.Empreendimento.Update(o);
            await _context.SaveChangesAsync();
            return true;


        }
        public async Task<bool> AtivarAsync(Empreendimento o)
        {

            o.Status = StatusEmpreendimento.Ativo;
            _context.Empreendimento.Update(o);
            await _context.SaveChangesAsync();
            return true;

        }
        public async Task<List<PlanoGerencialRGEmpreendimento>> FindAllPlanoGerencialRGEmpreendimento(int empid)
        {
            SessionInfo si = await GetSessionAsync();
            List<PlanoGerencialRGEmpreendimento> r = new();

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
"select planoGerencial.Id PlanoGerencialId , planoGerencial.Nome PlanoGerencialNome, " +
" Isnull( PlanoGerencialRGEmpreendimento.Id, 0) Id, " +
" Isnull( planoGerencial.RegraRateioDefaultId, 0) RegraRateioDefaultId, " +
" Isnull(RegraRateio.Nome, '') RegraRateioDefaultNome, " +
"  Isnull(PlanoGerencialRGEmpreendimento.RegraRateioId, 0) RegraRateioId, " +
" Isnull(RGEmp.Nome, '') RegraRateioNome, " +
"  Isnull(PlanoGerencialRGEmpreendimento.EmpreendimentoId, 0) EmpreendimentoId " +
"from planoGerencial " +
"inner join Tenant on planoGerencial.TenantId = Tenant.Id " +
"left join Regrarateio on Regrarateio.Id = planoGerencial.RegraRateioDefaultId " +
"left join PlanoGerencialRGEmpreendimento on PlanoGerencialRGEmpreendimento.PlanoGerencialId = PlanoGerencial.Id  and PlanoGerencialRGEmpreendimento.EmpreendimentoId = @empid " +
"left join Empreendimento on Empreendimento.Id = PlanoGerencialRGEmpreendimento.EmpreendimentoId " +
"left join Regrarateio RGEmp on RGEmp.Id = PlanoGerencialRGEmpreendimento.RegrarateioId " +
" where Tenant.Id = @tid " +
" order by PlanoGerencial.Nome ";

            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = empid;
            await command.Connection.OpenAsync();

            try
            {
                var lu = await command.ExecuteReaderAsync();

                while (await lu.ReadAsync())
                {
                    PlanoGerencialRGEmpreendimento np = new()
                    {
                        Id = (int)lu["Id"],
                        PlanoGerencialId = (int)lu["PlanoGerencialId"],
                        PlanoGerencialNome = lu["PlanoGerencialNome"].ToString(),
                        RegraRateioDefaultId = (int)lu["RegraRateioDefaultId"],
                        RegraRateioDefaultNome = lu["RegraRateioDefaultNome"].ToString(),
                        RegraRateioId = (int)lu["RegraRateioId"],
                        RegraRateioNome = lu["RegraRateioNome"].ToString()
                    };
                    r.Add(np);
                }
                return r;
            }
            finally
            {
                await command.Connection.CloseAsync();
            }
        }
        public async Task<List<ContaCorrenteRegraRateio>> FindAllPlanoGerencialRGContaCorrente(int empid)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ContaCorrenteRegraRateio.Where(x => x.TenantId == si.TenantId && x.EmpreendimentoId == empid).ToListAsync();
        }
        public async Task<Resultado> UpdadePlanoGerencialRGEmpreendimento(int empid, PlanoGerencialRGEmpreendimento ct)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            try
            {
                ct.EmpreendimentoId = empid;

                await _context.Database.BeginTransactionAsync();

                await _context.Database.ExecuteSqlRawAsync(
                "Update  PlanoGerencial " +
                " set RegraRateioDefaultId = " + ct.RegraRateioDefaultId.ToString() +
                " where Id = " + ct.PlanoGerencialId.ToString() + " and TenantId =  '" + si.TenantId + "'");

                if (ct.Id == 0)
                {
                    if (ct.RegraRateioId != 0)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                        "Insert  PlanoGerencialRGEmpreendimento (TenantId,EmpreendimentoId, PlanoGerencialId, RegraRateioId) " +
                        " Values ( '" + si.TenantId + "'," + empid.ToString() + "," + ct.PlanoGerencialId.ToString() + "," + ct.RegraRateioId.ToString() + ")");
                        var nct = await _context.PlanoGerencialRGEmpreendimento.FirstOrDefaultAsync(x => x.TenantId == si.TenantId
                                                                                            && x.EmpreendimentoId == empid
                                                                                            && x.RegraRateioId == ct.RegraRateioId
                                                                                            && x.PlanoGerencialId == ct.PlanoGerencialId);
                        if (nct != null)
                        {
                            ct.Id = nct.Id;
                        }
                    }
                }
                else
                {
                    if (ct.RegraRateioId == 0)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "Delete  PlanoGerencialRGEmpreendimento  " +
                            " where Id = " + ct.Id.ToString() + " and TenantId =  '" + si.TenantId + "'");
                        ct.Id = 0;
                    }
                    else
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "Update  PlanoGerencialRGEmpreendimento " +
                            " set RegraRateioId = " + ct.RegraRateioId.ToString() +
                            " where Id = " + ct.Id.ToString() + " and TenantId =  '" + si.TenantId + "'");
                    }
                }
                await _context.Database.CommitTransactionAsync();
                r.Ok = true;
                r.Item = ct;

            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;

            }
            return r;
        }
        public async Task<Resultado> UpdadePlanoGerencialRGContaCorrente(int empid, ContaCorrenteRegraRateio ct)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (ct.PlanoGerencialId == 0)
            {
                r.Ok = false;
                r.ErrMsg = "Plano gerencial não selecionado.";
                return r;
            }
            if (ct.ContaCorrenteId == 0)
            {
                r.Ok = false;
                r.ErrMsg = "Conta corrente não selecionada.";
                return r;
            }
            if (ct.RegraRateioId == 0)
            {
                r.Ok = false;
                r.ErrMsg = "Regra de rateio não selecionada.";
                return r;
            }

            if (await _context.ContaCorrenteRegraRateio.AnyAsync(x => x.TenantId == si.TenantId
                                                                    && x.EmpreendimentoId == empid
                                                                    && x.RegraRateioId == ct.RegraRateioId
                                                                    && x.ContaCorrenteId == ct.ContaCorrenteId
                                                                    && x.PlanoContaId == ct.PlanoContaId
                                                                    && x.PlanoGerencialId == ct.PlanoGerencialId
                                                                    && x.Id != ct.Id))
            {
                r.Ok = false;
                r.ErrMsg = "Regra já cadastrada.";
                return r;
            }

            try
            {
                ct.EmpreendimentoId = empid;

                await _context.Database.BeginTransactionAsync();

                if (ct.Id != 0)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                    "Update  ContaCorrenteRegraRateio " +
                    " set RegraRateioId = " + ct.RegraRateioId.ToString() +
                    " ,ContaCorrenteId = " + ct.ContaCorrenteId.ToString() +
                    " ,PlanoGerencialId = " + ct.PlanoGerencialId.ToString() +
                    " ,PlanoContaId = " + ct.PlanoContaId.ToString() +
                    " where Id = " + ct.Id.ToString() + " and TenantId =  '" + si.TenantId + "'");
                }
                else
                {
                    await _context.Database.ExecuteSqlRawAsync(
                    "Insert  ContaCorrenteRegraRateio (TenantId, EmpreendimentoId, PlanoGerencialId, PlanoContaId, ContaCorrenteId, RegraRateioId) " +
                    " Values ( '" + si.TenantId + "'," + empid.ToString() + "," + ct.PlanoGerencialId.ToString() +
                      "," + ct.PlanoContaId.ToString() +
                      "," + ct.ContaCorrenteId.ToString() +
                      "," + ct.RegraRateioId.ToString() + ")");
                    var nct = await _context.ContaCorrenteRegraRateio.FirstOrDefaultAsync(x => x.TenantId == si.TenantId
                                                                                        && x.EmpreendimentoId == empid
                                                                                        && x.RegraRateioId == ct.RegraRateioId
                                                                                        && x.ContaCorrenteId == ct.ContaCorrenteId
                                                                                        && x.PlanoContaId == ct.PlanoContaId
                                                                                        && x.PlanoGerencialId == ct.PlanoGerencialId);
                    if (nct != null)
                    {
                        ct.Id = nct.Id;
                    }

                }
                await _context.Database.CommitTransactionAsync();
                r.Ok = true;
                r.Item = ct;

            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;

            }
            return r;
        }
        public async Task<Resultado> DeletePlanoGerencialRGContaCorrente(ContaCorrenteRegraRateio ct)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            try
            {
                await _context.Database.BeginTransactionAsync();

                await _context.Database.ExecuteSqlRawAsync(
                    "Delete  ContaCorrenteRegraRateio  " +
                    " where Id = " + ct.Id.ToString() + " and TenantId =  '" + si.TenantId + "'");

                await _context.Database.CommitTransactionAsync();
                r.Ok = true;
                r.Item = ct;

            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;

            }
            return r;
        }
    }
}

