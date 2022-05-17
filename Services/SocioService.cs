using Microsoft.EntityFrameworkCore;
using InCorpApp.Data;
using InCorpApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Services.Exceptions;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using Blazored.SessionStorage;
using InCorpApp.Security;
using AutoMapper;
using InCorpApp.Interfaces;
using InCorpApp.Constantes;

namespace InCorpApp.Services
{
    public class SocioService : ServiceBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserService _userService;
        public SocioService(ApplicationDbContext context,

            UserService userService, ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper) : base(sessionStorageService, sessionService, mapper)
        {
            _context = context;
            _userService = userService;
        }
        public Socio FindById(int id)
        {
            return _context.Socio.FirstOrDefault(obj => obj.Id == id);
        }

        public async Task<bool> PodeAlterarCPNJCPFAsync(int id, int empresaid)
        {

            var command = _context.Database.GetDbConnection().CreateCommand();

            command.CommandText =
            " Select Count(*) Total from Socio "
            + " inner join EmpreendimentoSocio on EmpreendimentoSocio.SocioId = Socio.Id "
            + " inner join Empreendimento on Empreendimento.Id = EmpreendimentoSocio.EmpreendimentoId "
            + "           where Empreendimento.EmpresaId <> @e  and Socio.Id = @id";

            command.Parameters.Add(new SqlParameter("@e", System.Data.SqlDbType.Int));
            command.Parameters["@e"].Value = empresaid;

            command.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));
            command.Parameters["@id"].Value = id;

            await command.Connection.OpenAsync();
            int i = 0;

            try
            {
                var lu = await command.ExecuteReaderAsync();
                while (await lu.ReadAsync())
                {
                    i = (int)lu["Total"];
                };
                lu.Close();
            }
            finally
            {
                command.Connection.Close();
            }

            return i == 0;

        }
        public async Task<Socio> FindByEmailAsync(string email)
        {

            return await _context.Socio.AsNoTracking().FirstOrDefaultAsync(obj => obj.Email == email);
        }
        public async Task<Socio> FindSocioByEmpSocioIdAsync(int empsocioid)
        {
            SessionInfo si = await GetSessionAsync();
            var es = await _context.EmpreendimentoSocio.FirstOrDefaultAsync(x => x.Id == empsocioid);
            if (es != null)
            {
                return await _context.Socio.AsNoTracking().FirstOrDefaultAsync(obj => obj.Id == es.SocioId);
            }
            else
                return null;
        }
        public async Task<SocioView> FindByEmpIdSocioIdAsync(int empid, int socioid)
        {
            SocioView sv = new();
            SessionInfo si = await GetSessionAsync();

            sv.Emp = await _context.Empreendimento.FirstOrDefaultAsync(x => x.Id == empid && x.TenantId == si.TenantId);

            if (sv.Emp != null)
            {
                if (socioid == 0)
                {
                    sv.EmpSocio = new EmpreendimentoSocio()
                    {
                        TenantId = si.TenantId,
                        EmpreendimentoId = empid
                    };
                    sv.Participacoes = new List<EmpreendimentoSocioParticipacao>();
                }
                else
                {
                    Socio s = await _context.Socio.FirstOrDefaultAsync(x => x.Id == socioid);

                    if (s == null)
                    {
                        throw new NotFoundException("Socio não cadastrado!");
                    }

                    sv.Email = s.Email;
                    sv.Id = s.Id;

                    sv.EmpSocio = await _context.EmpreendimentoSocio.FirstOrDefaultAsync(x => x.EmpreendimentoId == empid && x.SocioId == socioid && x.TenantId == si.TenantId);
                    if (sv.EmpSocio == null)
                    {
                        sv.EmpSocio = new EmpreendimentoSocio()
                        {
                            TenantId = si.TenantId,
                            EmpreendimentoId = empid
                        };
                        sv.Participacoes = new List<EmpreendimentoSocioParticipacao>();
                    }
                    else
                    {
                        sv.Participacoes = await FindAllParticipacaoAsync(sv.EmpSocio.Id);
                    }

                }

            }
            else
            {
                throw new NotFoundException("Empreendimento não cadastrado!");
            }
            sv.EmpSocioContaCorrentes = await FindEmpSocioContaCorrenteByEmpIdEmpSocioId(empid, sv.EmpSocio.Id);
            sv.EmpSocioUsers = await FindAllEmpSocioUser(sv.EmpSocio.Id);
            return sv;
        }
        public async Task<List<EmpSocioContaCorrenteView>> FindEmpSocioContaCorrenteByEmpIdEmpSocioId(int empid, int empsocioid)
        {
            SessionInfo si = await GetSessionAsync();
            List<EmpSocioContaCorrenteView> r = new();
            var lc = await (from ContaCorrente in _context.ContaCorrente
                            join ContaCorrenteEmpreendimento in _context.ContaCorrenteEmpreendimento on ContaCorrente.Id equals ContaCorrenteEmpreendimento.ContaCorrenteId
                            select new { ContaCorrente, ContaCorrenteEmpreendimento.EmpreendimentoId }
                            ).AsNoTracking().Where(x => x.ContaCorrente.TenantId == si.TenantId && x.EmpreendimentoId == empid).ToListAsync();
            var lsc = await _context.EmpSocioContaCorrente.AsNoTracking().Where(x => x.TenantId == si.TenantId && x.EmpreendimentoSocioId == empsocioid).ToListAsync();
            foreach (var c in lc)
            {
                EmpSocioContaCorrenteView sc = new()
                {
                    NomeConta = c.ContaCorrente.Nome,
                    ContaCorrenteId = c.ContaCorrente.Id
                };
                EmpSocioContaCorrente src = lsc.FirstOrDefault(x => x.EmpreendimentoSocioId == empsocioid && x.ContaCorrenteId == c.ContaCorrente.Id);
                if (src != null)
                {
                    sc.Vissualizar = src.Permitir;
                }
                else
                {
                    sc.Vissualizar = true;
                }
                r.Add(sc);
            }
            return r;
        }
        public async Task<Resultado> UpdateEmpSocioContaCorrente(EmpSocioContaCorrente sc)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            try
            {
                EmpSocioContaCorrente tsc = await _context.EmpSocioContaCorrente.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.EmpreendimentoSocioId == sc.EmpreendimentoSocioId && x.ContaCorrenteId == sc.ContaCorrenteId);
                if (tsc == null)
                {
                    tsc = new()
                    {
                        TenantId = si.TenantId,
                        EmpreendimentoSocioId = sc.EmpreendimentoSocioId,
                        ContaCorrenteId = sc.ContaCorrenteId,
                        Permitir = sc.Permitir
                    };
                    await _context.EmpSocioContaCorrente.AddAsync(tsc);
                    await _context.SaveChangesAsync();
                    r.Ok = true;
                    return r;
                }
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }

            var c = _context.Database.GetDbConnection();
            DbCommand cmd = c.CreateCommand();

            cmd.CommandText = "Update EmpSocioContaCorrente set Permitir = @p  where EmpreendimentoSocioId = @eid and ContaCorrenteId = @cid";
            cmd.Parameters.Add(new SqlParameter("@eid", System.Data.SqlDbType.Int));
            cmd.Parameters["@eid"].Value = sc.EmpreendimentoSocioId;
            cmd.Parameters.Add(new SqlParameter("@cid", System.Data.SqlDbType.Int));
            cmd.Parameters["@cid"].Value = sc.ContaCorrenteId;
            cmd.Parameters.Add(new SqlParameter("@p", System.Data.SqlDbType.Bit));
            cmd.Parameters["@p"].Value = sc.Permitir;
            try
            {
                await c.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                r.Ok = true;
                r.ErrMsg = string.Empty;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            finally
            {
                await c.CloseAsync();
            }
            return r;
        }
        public async Task AddParticipacaoAsync(EmpreendimentoSocioParticipacao p, EmpreendimentoSocio es)
        {

            if (p.DataFimDist <= p.DataInicioDist)
            {
                throw new Exception("Data de inicio inválida!");
            }


            SessionInfo si = await GetSessionAsync();

            if (await _context.EmpreendimentoSocioParticipacao.AnyAsync(x => x.EmpreendimentoSocioId == es.Id && x.TenantId == si.TenantId && ((x.DataInicioDist <= p.DataInicioDist && x.DataFimDist >= p.DataInicioDist) ||
            (p.DataInicioDist < x.DataFimDist))))
            {
                throw new Exception("Data de inicio inválida!");
            }

            p.TenantId = si.TenantId;
            p.EmpreendimentoSocioId = es.Id;
            p.DataInicioDist = Utils.UtilsClass.GetInicio(p.DataInicioDist);
            p.DataFimDist = Utils.UtilsClass.GetUltimo(p.DataFimDist);
            p.DataInicio = p.DataInicioDist;
            p.DataFim = p.DataFimDist;

            await _context.EmpreendimentoSocioParticipacao.AddAsync(p);
            await _context.SaveChangesAsync();

        }
        public async Task UpdateParticipacaoAsync(EmpreendimentoSocioParticipacao p)
        {
            SessionInfo si = await GetSessionAsync();


            if (p.DataFimDist <= p.DataInicioDist)
            {
                throw new Exception("Data de inicio inválida!");
            }

            if (await _context.EmpreendimentoSocioParticipacao.AnyAsync(x => x.EmpreendimentoSocioId == p.EmpreendimentoSocioId && x.TenantId == si.TenantId
              && x.Id != p.Id &&
            ((x.DataInicioDist <= p.DataInicioDist && x.DataFimDist >= p.DataInicioDist) ||
            (p.DataInicioDist < x.DataFimDist))))
            {
                throw new Exception("Data de inicio inválida!");
            }

            p.TenantId = si.TenantId;
            p.DataInicioDist = Utils.UtilsClass.GetInicio(p.DataInicioDist);
            p.DataFimDist = Utils.UtilsClass.GetUltimo(p.DataFimDist);
            p.DataInicio = p.DataInicioDist;
            p.DataFim = p.DataFimDist;

            _context.EmpreendimentoSocioParticipacao.Update(p);
            await _context.SaveChangesAsync();

        }
        public async Task DeleteParticipacaoAsync(EmpreendimentoSocioParticipacao p)
        {
            _context.EmpreendimentoSocioParticipacao.Remove(p);
            await _context.SaveChangesAsync();
        }
        public async Task<List<EmpreendimentoSocioParticipacao>> FindAllParticipacaoAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();

            return await _context.EmpreendimentoSocioParticipacao.Where(x => x.EmpreendimentoSocioId == id && x.TenantId == si.TenantId).OrderByDescending(x => x.DataInicioDist).ToListAsync();
        }
        public async Task<List<SocioEmpreendimentoView>> FindAllSociosByEmpIdAsync(int id)
        {

            var command = _context.Database.GetDbConnection().CreateCommand();

            SessionInfo si = await GetSessionAsync();


            command.CommandText =
            "Select Socio.Id, Socio.Email, EmpreendimentoSocio.Id EmpreendimentoSocioId,EmpreendimentoSocio.Nome, EmpreendimentoSocio.RazaoSocial, EmpreendimentoSocio.Tipo, EmpreendimentoSocio.CPFCNPJ  from Socio " +
                                            " inner join EmpreendimentoSocio on EmpreendimentoSocio.SocioId = Socio.Id " +
                                            " and EmpreendimentoSocio.EmpreendimentoId = @empid and EmpreendimentoSocio.TenantId = @tenantid ";

            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = id;
            command.Parameters.Add(new SqlParameter("@tenantid", System.Data.SqlDbType.NVarChar));
            command.Parameters["@tenantid"].Value = si.TenantId;

            await command.Connection.OpenAsync();
            List<SocioEmpreendimentoView> r = new();
            try
            {
                var lu = await command.ExecuteReaderAsync();

                while (lu.Read())
                {
                    SocioEmpreendimentoView sv = new();
                    sv.Socio.Email = lu["email"].ToString();
                    sv.EmpreendimentoSocio.Nome = lu["Nome"].ToString();
                    sv.EmpreendimentoSocio.CPFCNPJ = lu["CPFCNPJ"].ToString();
                    sv.Socio.Id = Convert.ToInt32(lu["Id"].ToString());
                    sv.SocioId = sv.Socio.Id;
                    sv.EmpreendimentoSocio.Id = (int)lu["EmpreendimentoSocioId"];
                    sv.EmpreendimentoSocioId = sv.EmpreendimentoSocio.Id;
                    sv.EmpreendimentoSocio.EmpreendimentoId = id;
                    r.Add(sv);
                };
                lu.Close();
            }
            finally
            {
                command.Connection.Close();
            }
            return r;
        }
        public async Task AddAsync(SocioView sv)
        {

            sv.Email = sv.Email.ToLower();

            SessionInfo si = await GetSessionAsync();

            var r = await ValidarSocio(sv);
            if (!r.Ok)
            {
                throw new Exception(r.ErrMsg);
            }

            Socio st = await _context.Socio.AsNoTracking().FirstOrDefaultAsync(x => x.Email == sv.Email);

            await _context.Database.BeginTransactionAsync();

            Socio s = new();
            try
            {
                if (st == null)
                {
                    if (sv.Tipo == 1 || sv.Tipo == 3)
                    {
                        sv.RazaoSocial = sv.Nome;
                    }
                    s.Email = sv.Email;
                    await _context.Socio.AddAsync(s);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    s.Email = st.Email;
                    s.Id = st.Id;
                }

                EmpreendimentoSocio es = new()
                {
                    EmpreendimentoId = sv.Emp.Id,
                    SocioId = s.Id,
                    Nome = sv.Nome,
                    CPFCNPJ = sv.CPFCNPJ,
                    Tipo = sv.Tipo,
                    RazaoSocial = sv.RazaoSocial,
                    TenantId = si.TenantId
                };
                await _context.EmpreendimentoSocio.AddAsync(es);
                await _context.SaveChangesAsync();

                EmpreendimentoSocioParticipacao ep = new();
                _mapper.Map(sv, ep);
                ep.Id = 0;
                ep.TenantId = si.TenantId;
                ep.EmpreendimentoSocioId = es.Id;
                ep.DataFim = Constante.Today.AddMonths(96);
                ep.DataInicio = Utils.UtilsClass.GetInicio(ep.DataInicioDist);
                ep.DataFimDist = Utils.UtilsClass.GetUltimo(ep.DataInicioDist.AddYears(30));
                await _context.EmpreendimentoSocioParticipacao.AddAsync(ep);
                await _context.SaveChangesAsync();
                await _userService.CriarUsuarioSocio(s);

                var ue = sv.EmpSocioUsers.FirstOrDefault(x => x.Email == sv.Email);
                if (ue != null)
                {
                    _context.EmpSocioUser.Remove(ue);
                    await _context.SaveChangesAsync();
                }
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }

        }
        public async Task<Resultado> ValidarSocio(SocioView sv)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            r.Ok = true;

            CNPJValidador cnpj = new(sv.CPFCNPJ);
            if (!cnpj.Valido)
            {
                r.Ok = false;
                if (sv.Tipo == 1)
                {
                    r.ErrMsg = "CPF inválido";
                }
                else
                {
                    r.ErrMsg = "CNPF inválido";
                }
                return r;
            }

            if (await _context.EmpreendimentoSocio.AnyAsync(x => x.EmpreendimentoId == sv.EmpSocio.EmpreendimentoId && x.TenantId == si.TenantId && x.Id != sv.EmpSocio.Id && x.CPFCNPJ == sv.CPFCNPJ))
            {
                r.Ok = false;
                if (sv.Tipo == 1)
                {
                    r.ErrMsg = "Já existe um sócio com esse CPF";
                }
                else
                {
                    r.ErrMsg = "Já existe um sócio com ess CNPF";
                }
                return r;
            }

            return r;
        }
        public async Task UpdateAsync(SocioView sv)
        {
            SessionInfo si = await GetSessionAsync();
            sv.Email = sv.Email.ToLower();

            var r = await ValidarSocio(sv);
            if (!r.Ok)
            {
                throw new Exception(r.ErrMsg);
            }

            if (sv.Tipo == 1 || sv.Tipo == 3)
            {
                sv.RazaoSocial = sv.Nome;
            }

            Socio st = await _context.Socio.AsNoTracking().FirstOrDefaultAsync(x => x.Email == sv.Email && x.Id != sv.Id);
            await _context.Database.BeginTransactionAsync();
            try
            {
                if (st != null)
                {
                    sv.EmpSocio.SocioId = st.Id;
                    _context.EmpreendimentoSocio.Update(sv.EmpSocio);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    Socio s = await _context.Socio.FirstOrDefaultAsync(x => x.Id == sv.Id);
                    if (s.Email != sv.Email)
                    {
                        s.Email = sv.Email;
                        _context.Socio.Update(s);
                    }
                    EmpreendimentoSocio es = _context.EmpreendimentoSocio.FirstOrDefault(x => x.EmpreendimentoId == sv.Emp.Id && x.SocioId == s.Id);
                    if (es == null)
                    {
                        es = new EmpreendimentoSocio()
                        {
                            EmpreendimentoId = sv.Emp.Id,
                            SocioId = s.Id,
                            TenantId = si.TenantId,
                            Nome = sv.Nome,
                            RazaoSocial = sv.RazaoSocial,
                            Tipo = sv.Tipo,
                            CPFCNPJ = sv.CPFCNPJ,
                            Id = 0
                        };
                    }
                    if (es.Id == 0)
                    {
                        await _context.EmpreendimentoSocio.AddAsync(es);
                    }
                    else
                    {
                        es.TenantId = si.TenantId;
                        es.Nome = sv.Nome;
                        es.RazaoSocial = sv.RazaoSocial;
                        es.Tipo = sv.Tipo;
                        es.CPFCNPJ = sv.CPFCNPJ;
                        _context.EmpreendimentoSocio.Update(es);
                    }

                    if (!es.RestringirAcessoCC)
                    {
                        var lrs = await _context.EmpSocioContaCorrente.Where(x => x.TenantId == si.TenantId && x.EmpreendimentoSocioId == es.Id).ToListAsync();
                        if (lrs.Count > 0)
                        {
                            _context.EmpSocioContaCorrente.RemoveRange(lrs);
                            await _context.SaveChangesAsync();
                        }
                    }
                    await _context.SaveChangesAsync();
                    await _userService.CriarUsuarioSocio(s);
                }

                var ue = sv.EmpSocioUsers.FirstOrDefault(x => x.Email == sv.Email);
                if (ue != null)
                {
                    _context.EmpSocioUser.Remove(ue);
                    await _context.SaveChangesAsync();
                }

                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
        public async Task DeleteAsync(Socio s, int empid)
        {
            SessionInfo si = await GetSessionAsync();
            EmpreendimentoSocio es = await _context.EmpreendimentoSocio.FirstOrDefaultAsync(x => x.EmpreendimentoId == empid && x.SocioId == s.Id && x.TenantId == si.TenantId);
            if (es == null)
            {
                throw new NotFoundException("Socio não vinculado ao empreendimento! ");
            }
            if (await _context.LanctoEmpRateadoPlanoContaSocio.AsNoTracking().AnyAsync(x => x.EmpreendimentoSocioId == es.Id && x.TenantId == si.TenantId))
            {
                throw new NotFoundException("Socio com movimentação não pode ser excluído! ");
            }
            if (await _context.GrupoSocioEmpreedSocio.AsNoTracking().AnyAsync(x => x.EmpreendimentoSocioId == es.Id && x.TenantId == si.TenantId))
            {
                throw new NotFoundException("Socio vinculado a grupo de socio para rateio não pode ser excluído!");
            }
            es.Empreendimento = null;
            var lp = await FindAllParticipacaoAsync(es.Id);
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.EmpreendimentoSocioParticipacao.RemoveRange(lp);
                await _context.SaveChangesAsync();
                List<EmpSocioUser> leu = await _context.EmpSocioUser.Where(x => x.EmpreendimentoSocioId == es.Id && x.TenantId == si.TenantId).ToListAsync();
                if (leu != null && leu.Count > 0)
                {
                    foreach (EmpSocioUser eu in leu)
                    {
                        await _userService.RemoverUsuarioEmpreendimento(eu);
                    }
                }
                _context.EmpreendimentoSocio.Remove(es);
                await _context.SaveChangesAsync();

                if (!await _context.EmpreendimentoSocio.AnyAsync(x => x.SocioId == s.Id))
                {
                    var su = _context.SocioUsuario.FirstOrDefault(x => x.SocioId == s.Id);
                    if (su != null)
                    {
                        _context.SocioUsuario.Remove(su);
                    }
                    _context.Socio.Remove(s);

                    Usuario u = new()
                    {
                        Email = s.Email
                    };
                    await _userService.DeleteAsync(u);
                }
                else
                {
                    if (!await _context.EmpreendimentoSocio.AnyAsync(x => x.SocioId == s.Id && x.TenantId == si.TenantId))
                    {
                        var user = await _userService.FindByEmail(s.Email);
                        if (user != null)
                        {
                            var tu = await _context.TenantUsuario.FirstOrDefaultAsync(x => x.UserId == user.Id);
                            _context.TenantUsuario.Remove(tu);
                        }
                    }
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
        public async Task<List<EmpreendimentoSocio>> FindEmpreendimentoSocioByEmpId(int empid)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.EmpreendimentoSocio.Where(x => x.EmpreendimentoId == empid && x.TenantId == si.TenantId).OrderBy(x=> x.Nome).ToListAsync();
        }
        public async Task<EmpreendimentoSocio> FindEmpSocioBySocioIdEmpIdAsync(int empid, int socioid)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.EmpreendimentoSocio.FirstOrDefaultAsync(x => x.EmpreendimentoId == empid && x.SocioId == socioid && x.TenantId == si.TenantId);
        }
        public async Task<EmpreendimentoSocio> FindEmpSocioByEmpreendimentoSocioIdAsync(int empsocioid)
        {
            var es = await (from EmpreendimentoSocio in _context.EmpreendimentoSocio
                            join Empreendimento in _context.Empreendimento on EmpreendimentoSocio.EmpreendimentoId equals Empreendimento.Id
                            select new { EmpreendimentoSocio, Empreendimento }
              ).Where(x => x.EmpreendimentoSocio.Id == empsocioid).FirstOrDefaultAsync();
            if (es != null)
            {
                es.EmpreendimentoSocio.Empreendimento = es.Empreendimento;
                return es.EmpreendimentoSocio;
            }
            else
            {
                return null;
            }
        }
        public async Task<EmpreendimentoSocio> FindEmpSocioByEmpIdCodigoExternoAsync(int empid, string cext)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.EmpreendimentoSocio.FirstOrDefaultAsync(x => x.EmpreendimentoId == empid && x.CodigoExterno == cext && x.TenantId == si.TenantId);
        }
        public async Task<List<EmpreendimentoSocio>> FindEmpreendimentoSocioByUserIdAsync(string userid)
        {
            var lsu = await (from EmpSocioUser in _context.EmpSocioUser
                             join EmpreendimentoSocio in _context.EmpreendimentoSocio on EmpSocioUser.EmpreendimentoSocioId equals EmpreendimentoSocio.Id
                             join Empreendimento in _context.Empreendimento on EmpreendimentoSocio.EmpreendimentoId equals Empreendimento.Id
                             join Socio in _context.Socio on EmpreendimentoSocio.SocioId equals Socio.Id
                             select new { EmpreendimentoSocio, Empreendimento, Socio, EmpSocioUser.UserId }
              ).Where(x => x.UserId == userid).ToListAsync();

            List<EmpreendimentoSocio> r = new();
            foreach (var i in lsu)
            {
                i.EmpreendimentoSocio.Empreendimento = i.Empreendimento;
                i.EmpreendimentoSocio.Socio = i.Socio;
                r.Add(i.EmpreendimentoSocio);
            }

            var lsoc = await (from SocioUsuario in _context.SocioUsuario
                              join Socio in _context.Socio on SocioUsuario.SocioId equals Socio.Id
                              join EmpreendimentoSocio in _context.EmpreendimentoSocio on Socio.Id equals EmpreendimentoSocio.SocioId
                              join Empreendimento in _context.Empreendimento on EmpreendimentoSocio.EmpreendimentoId equals Empreendimento.Id
                              select new { EmpreendimentoSocio, Empreendimento, Socio, SocioUsuario.IdentityUserId }
              ).Where(x => x.IdentityUserId == userid).ToListAsync();
            foreach (var s in lsoc)
            {
                if (!r.Any(x => x.EmpreendimentoId == s.EmpreendimentoSocio.EmpreendimentoId))
                {
                    s.EmpreendimentoSocio.Empreendimento = s.Empreendimento;
                    s.EmpreendimentoSocio.Socio = s.Socio;
                    r.Add(s.EmpreendimentoSocio);
                }
            }

            return r;
        }
        public async Task<List<EmpreendimentoSocio>> FindEmpreendimentoSocioBySocioIdAsync(List<int> lsocioid)
        {
            var lsu = await (from EmpreendimentoSocio in _context.EmpreendimentoSocio
                             join Empreendimento in _context.Empreendimento on EmpreendimentoSocio.EmpreendimentoId equals Empreendimento.Id
                             join Socio in _context.Socio on EmpreendimentoSocio.SocioId equals Socio.Id
                             select new { EmpreendimentoSocio, Empreendimento, Socio }
              ).Where(x => lsocioid.Contains(x.EmpreendimentoSocio.SocioId)).ToListAsync();
            List<EmpreendimentoSocio> r = new();
            foreach (var i in lsu)
            {
                i.EmpreendimentoSocio.Empreendimento = i.Empreendimento;
                i.EmpreendimentoSocio.Socio = i.Socio;
                r.Add(i.EmpreendimentoSocio);
            }
            return r;
        }
        public async Task<List<ISFDocument>> FindAllDocumentsBySocioIdAsync(int socioid, int empid = 0)
        {
            SessionInfo si = await GetSessionAsync();

            List<int> lsu = await (from EmpSocioUser in _context.EmpSocioUser
                                   join EmpreendimentoSocio in _context.EmpreendimentoSocio on EmpSocioUser.EmpreendimentoSocioId equals EmpreendimentoSocio.Id
                                   select new { EmpSocioUser.UserId, EmpreendimentoSocio.SocioId }
              ).Where(x => x.UserId == si.UserId).Select(x => x.SocioId).ToListAsync();

            if (lsu == null)
            {
                lsu = new() { socioid };
            }
            else
            {
                if (!lsu.Contains(socioid))
                {
                    lsu.Add(socioid);
                }
            }

            List<ISFDocument> r = new();

            List<EmpreendimentoSocio> les = await FindEmpreendimentoSocioBySocioIdAsync(lsu);
            if (empid != 0)
            {
                les = les.Where(x => x.EmpreendimentoId == empid).ToList();
            }

            foreach (var es in les)
            {

                /*
                                var la = await _context.SocioAporte.Where(x => x.EmpreendimentoSocioId == es.Id  && x.Assinado == false && x.DataLimiteDeposito >= Constante.Today.AddMonths(-3)).ToListAsync();
                                foreach (var a in la)
                                {
                                    a.NomeEmpreendimento = es.Empreendimento.Nome;
                                    a.EmpreendimentoId = es.Empreendimento.Id;
                                    a.NomeSocio = es.Socio.Nome;
                                    r.Add(a);
                                    var lad = await _context.SocioAporteDeposito.Where(x => x.SocioAporteId == a.Id).ToListAsync();
                                    foreach (var ad in lad)
                                    {
                                        ad.NomeEmpreendimento = es.Empreendimento.Nome;
                                        r.Add(ad);
                                    }
                                }
                */

                /*
                                var ld = await _context.SocioDebito.Where(x => x.EmpreendimentoSocioId == es.Id).ToListAsync();
                                foreach (var d in ld)
                                {
                                    r.Add(d);
                                    var ldl = await _context.SocioDebitoLancto.Where(x => x.SocioDebitoId == d.Id).ToListAsync();
                                    foreach (var dl in ldl)
                                    {
                                        r.Add(dl);
                                    }
                                }
                */
                var lre = await _context.SocioRetirada.Where(x => x.EmpreendimentoSocioId == es.Id && x.Assinado == false && x.DataLiberacao >= Constante.Today.AddMonths(-2)).ToListAsync();
                foreach (var re in lre)
                {
                    re.NomeEmpreendimento = es.Empreendimento.Nome;
                    re.EmpreendimentoId = es.Empreendimento.Id;
                    re.ShowGerencial = false;
                    re.NomeSocio = es.Nome;
                    Periodo pe = await _context.Periodo.Where(x => x.EmpreendimentoId == es.Empreendimento.Id && x.Status == StatusPeriodo.Fechado && x.DataFim < re.DataLiberacao).OrderByDescending(x => x.DataInicio).FirstOrDefaultAsync();
                    if (pe != null)
                    {
                        re.PeriodoId = pe.Id;
                    }

                    var lva = await (from SocioRetiradaDebitoLancto in _context.SocioRetiradaDebitoLancto
                                     join SocioDebitoLancto in _context.SocioDebitoLancto on SocioRetiradaDebitoLancto.SocioDebitoLanctoId equals SocioDebitoLancto.Id
                                     select new { SocioDebitoLancto, SocioRetiradaDebitoLancto.SocioRetiradaId }
                                  ).Where(x => x.SocioRetiradaId == re.Id).ToListAsync();
                    if (lva != null && lva.Count > 0)
                    {
                        re.ValorAmortizado = lva.Sum(x => x.SocioDebitoLancto.Valor);
                    }
                    r.Add(re);
                    var lred = await _context.SocioRetiradaLancto.Where(x => x.SocioRetiradaId == re.Id).ToListAsync();
                    foreach (var red in lred)
                    {
                        red.NomeEmpreendimento = es.Empreendimento.Nome;
                        red.NomeSocio = es.Nome;
                        red.EmpreendimentoId = es.Empreendimento.Id;
                        if (pe != null)
                        {
                            red.PeriodoId = pe.Id;
                        }
                        r.Add(red);
                    }
                }
                /*
                                var lresult = await (from SocioResultadoPeriodo in _context.SocioResultadoPeriodo
                                                     join Periodo in _context.Periodo on SocioResultadoPeriodo.PeriodoId equals Periodo.Id
                                                     select new { SocioResultadoPeriodo }
                                              ).Where(x => x.SocioResultadoPeriodo.EmpreendimentoSocioId == es.Id && x.SocioResultadoPeriodo.DataMovto >= Constante.Today.AddMonths(-2)).ToListAsync();
                                foreach (var result in lresult)
                                {
                                    result.SocioResultadoPeriodo.NomeEmpreendimento = es.Empreendimento.Nome;
                                    result.SocioResultadoPeriodo.EmpreendimentoId = es.Empreendimento.Id;
                                    result.SocioResultadoPeriodo.NomeSocio = es.Socio.Nome;
                                    r.Add(result.SocioResultadoPeriodo);
                                }
                */
                /*
                                var laud = await (from PeriodoSocioAuditoria in _context.PeriodoSocioAuditoria
                                                  join PeriodoAuditoriaVersao in _context.PeriodoAuditoriaVersao on PeriodoSocioAuditoria.PeriodoAuditoriaVersaoId equals PeriodoAuditoriaVersao.Id
                                                  join Periodo in _context.Periodo on PeriodoAuditoriaVersao.PeriodoId equals Periodo.Id
                                                  select new { PeriodoSocioAuditoria, PeriodoAuditoriaVersao , Periodo }
                                              ).Where(x => x.PeriodoSocioAuditoria.EmpreendimentoSocioId == es.Id && x.PeriodoSocioAuditoria.Assinado == false
                                                      && x.PeriodoAuditoriaVersao.DataCancelamento == DateTime.MinValue && x.Periodo.Status == StatusPeriodo.Auditoria).ToListAsync();

                                foreach (var aud in laud)
                                {
                                    aud.PeriodoSocioAuditoria.NomeEmpreendimento = es.Empreendimento.Nome;
                                    aud.PeriodoSocioAuditoria.EmpreendimentoId = es.Empreendimento.Id;
                                    aud.PeriodoSocioAuditoria.PeriodoId = aud.PeriodoAuditoriaVersao.PeriodoId;
                                    r.Add(aud.PeriodoSocioAuditoria);
                                }
                */
            }
            return r.OrderBy(x => x.DataCriacao).ThenBy(x => x.Assinado).ToList();
        }
        public async Task<Resultado> AprovarDocumentoAsync(ISFDocument doc)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            var c = _context.Database.GetDbConnection();
            DbCommand cmd = c.CreateCommand();

            if (string.IsNullOrEmpty(doc.NomeAssinado))
            {
                doc.NomeAssinado = si.UserName;
            }

            string sqladd = string.Empty;
            if (doc.OrigemDocto == OrigemDocto.Auditoria)
            {
                sqladd = " , Status = 2, DataConclusao = getdate() ";
            }

            cmd.CommandText = " Update " + doc.DBTableName + " set Assinado = 1, DataAssinatura = getDate(), NomeAssinado = @Nome " + sqladd + " where Id = @id";
            cmd.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));
            cmd.Parameters["@id"].Value = doc.Id;
            cmd.Parameters.Add(new SqlParameter("@Nome", System.Data.SqlDbType.NVarChar, 100));
            cmd.Parameters["@Nome"].Value = doc.NomeAssinado;

            try
            {
                await c.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                doc.Assinado = true;
                doc.DataAssinatura = Constante.Now;
                r.Ok = true;
                r.ErrMsg = "Documento Aprovado";
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            finally
            {
                await c.CloseAsync();
            }
            return r;
        }
        public async Task<Resultado> AcessarSitema(SocioView sv, bool permitir, bool enviaremail = true)
        {
            Resultado r = new();
            r.Ok = false;
            try
            {
                if (permitir)
                {
                    r = await ValidarSocio(sv);
                    if (!r.Ok)
                    {
                        return r;
                    }
                }
                sv.EmpSocio.AcessoLiberado = permitir;
                _context.EmpreendimentoSocio.Update(sv.EmpSocio);
                await _userService.AddUserToPerfil(sv.Email, Constante.RoleSocio());
                await _context.SaveChangesAsync();
                if (permitir && enviaremail)
                {
                    var u = await _userService.FindByEmail(sv.Email);
                    if (u != null)
                    {
                        if (!u.EmailConfirmed)
                        {
                            r = await _userService.EnviarEmailConfirmacaoEmail(u);
                        }
                    }
                    foreach (var up in sv.EmpSocioUsers)
                    {
                        u = await _userService.FindByEmail(up.Email);
                        if (u != null)
                        {
                            if (!u.EmailConfirmed)
                            {
                                r = await _userService.EnviarEmailConfirmacaoEmail(u);
                            }
                        }

                    }
                }
                else
                {
                    r.Ok = true;
                }
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> EnviarEmailUserNotConfirmed(List<EmpSocioUser> le)
        {
            Resultado r = new();
            r.SetDefault();
            try
            {
                foreach (var up in le)
                {
                    var u = await _userService.FindByEmail(up.Email);
                    if (u != null)
                    {
                        if (!u.EmailConfirmed)
                        {
                            r = await _userService.EnviarEmailConfirmacaoEmail(u);
                        }
                    }

                }
                r.Ok = true;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<List<EmpSocioUser>> FindAllEmpSocioUser(int empsocioid)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.EmpSocioUser.Where(x => x.EmpreendimentoSocioId == empsocioid && x.TenantId == si.TenantId).ToListAsync();
        }
    }
}
