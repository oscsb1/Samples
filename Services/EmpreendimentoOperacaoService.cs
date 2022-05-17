using Microsoft.EntityFrameworkCore;
using InCorpApp.Data;
using InCorpApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Security;
using Blazored.SessionStorage;
using AutoMapper;
using Microsoft.Data.SqlClient;
using InCorpApp.Constantes;
using InCorpApp.Services.Exceptions;
using InCorpApp.Utils;
using System.Threading;
using InCorpApp.Interfaces;
using System.Data.Common;
using System.Data;
using System.IO;
using Microsoft.AspNetCore.Hosting;


namespace InCorpApp.Services
{
    public class EmpreendimentoOperacaoService : ServiceBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ConfigService _configService;
        private readonly RegrasRateioService _regrasRateioService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public EmpreendimentoOperacaoService(ApplicationDbContext context,
                    ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper, RegrasRateioService rrService, ConfigService configService, IWebHostEnvironment webHostEnvironment) : base(sessionStorageService, sessionService, mapper)
        {
            _context = context;
            _regrasRateioService = rrService;
            _configService = configService;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<List<SocioRetirada>> FindAllRetiradasByEmpSocioIdAsync(int empsocioid)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.SocioRetirada.Where(x => x.EmpreendimentoSocioId == empsocioid && x.TenantId == si.TenantId).AsNoTracking().ToListAsync();
        }
        public async Task<List<SocioRetiradaLancto>> FindAllRetiradasLanctosBySocioRetiradaIdAsync(int socioretiradaid, bool track = false)
        {
            SessionInfo si = await GetSessionAsync();
            if (track)
            {
                return await _context.SocioRetiradaLancto.Where(x => x.SocioRetiradaId == socioretiradaid && x.TenantId == si.TenantId).ToListAsync();
            }
            else
            {
                return await _context.SocioRetiradaLancto.Where(x => x.SocioRetiradaId == socioretiradaid && x.TenantId == si.TenantId).AsNoTracking().ToListAsync();
            }
        }
        public async Task<List<SocioDebitoLancto>> FindAllDebitosLanctosBySocioRetiradaIdAsync(int socioretiradaid, bool track = false)
        {
            SessionInfo si = await GetSessionAsync();
            List<SocioDebitoLancto> r = new();
            if (track)
            {

                var q = await (from SocioRetiradaDebitoLancto in _context.SocioRetiradaDebitoLancto
                               join SocioDebitoLancto in _context.SocioDebitoLancto on SocioRetiradaDebitoLancto.SocioDebitoLanctoId equals SocioDebitoLancto.Id
                               join SocioDebito in _context.SocioDebito on SocioDebitoLancto.SocioDebitoId equals SocioDebito.Id
                               select new { SocioDebitoLancto, SocioRetiradaDebitoLancto, SocioDebito }
                    ).Where(x => x.SocioRetiradaDebitoLancto.SocioRetiradaId == socioretiradaid && x.SocioRetiradaDebitoLancto.TenantId == si.TenantId).ToListAsync();
                foreach (var it in q)
                {
                    it.SocioDebitoLancto.SocioDebito = it.SocioDebito;
                    r.Add(it.SocioDebitoLancto);
                }
            }
            else
            {
                var q = await (from SocioRetiradaDebitoLancto in _context.SocioRetiradaDebitoLancto
                               join SocioDebitoLancto in _context.SocioDebitoLancto on SocioRetiradaDebitoLancto.SocioDebitoLanctoId equals SocioDebitoLancto.Id
                               join SocioDebito in _context.SocioDebito on SocioDebitoLancto.SocioDebitoId equals SocioDebito.Id
                               select new { SocioDebitoLancto, SocioRetiradaDebitoLancto, SocioDebito }
                        ).Where(x => x.SocioRetiradaDebitoLancto.SocioRetiradaId == socioretiradaid && x.SocioRetiradaDebitoLancto.TenantId == si.TenantId).ToListAsync();
                foreach (var it in q)
                {
                    it.SocioDebitoLancto.SocioDebito = it.SocioDebito;
                    r.Add(it.SocioDebitoLancto);
                }
            }
            return r;
        }
        public async Task<List<SocioAporte>> FindAllAportesByEmpSocioIdAsync(int empsocioid, bool track = false)
        {
            SessionInfo si = await GetSessionAsync();
            if (track)
            {
                return await _context.SocioAporte.Where(x => x.EmpreendimentoSocioId == empsocioid && x.TenantId == si.TenantId).ToListAsync();
            }
            else
            {
                return await _context.SocioAporte.Where(x => x.EmpreendimentoSocioId == empsocioid && x.TenantId == si.TenantId).AsNoTracking().ToListAsync();
            }
        }
        public async Task<List<SocioAporteDeposito>> FindAllAportesDespositosBySocioAporteIdAsync(int socioaporteid, bool track = false)
        {
            SessionInfo si = await GetSessionAsync();
            if (track)
            {
                return await _context.SocioAporteDeposito.Where(x => x.SocioAporteId == socioaporteid && x.TenantId == si.TenantId).ToListAsync();
            }
            else
            {
                return await _context.SocioAporteDeposito.Where(x => x.SocioAporteId == socioaporteid && x.TenantId == si.TenantId).AsNoTracking().ToListAsync();
            }
        }
        private async Task<Resultado> ValidarAporte(SocioAporte a)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            var emp = await _context.EmpreendimentoSocio.FirstOrDefaultAsync(x => x.Id == a.EmpreendimentoSocioId && x.TenantId == si.TenantId);
            if (emp == null)
            {
                r.Ok = false;
                r.ErrMsg = "Sócio não informado";
                return r;
            }
            var ex = await _context.Empreendimento.FirstOrDefaultAsync(x => x.Id == emp.EmpreendimentoId && x.TenantId == si.TenantId);
            if (ex == null)
            {
                r.Ok = false;
                r.ErrMsg = "Empreendimento não informado";
                return r;
            }
            if (a.DataLimiteDeposito < ex.DataInicioOperacao)
            {
                r.Ok = false;
                r.ErrMsg = "Data menor que a data de início de operação do empreendimento";
                return r;
            }

            /* temporario
            var p = await FindPeriodoByDataAsync(emp.EmpreendimentoId, a.DataLimiteDeposito);
            if (p != null && p.Status != StatusPeriodo.EntradaDeDados)
            {
                r.Ok = false;
                r.ErrMsg = "Período " + p.Nome + " está " + p.NomeStatus + " não permite alterações";
                return r;
            }
            */

            if (a.Valor <= 0)
            {
                r.Ok = false;
                r.ErrMsg = "Valor inválido";
                return r;
            }

            r.Ok = true;
            return r;
        }
        public async Task<Resultado> UpdateSocioAporteAsync(SocioAporte a)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = await ValidarAporte(a);
            if (!r.Ok)
            {
                return r;
            }
            a.TenantId = si.TenantId;
            try
            {
                _context.SocioAporte.Update(a);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;

        }
        public async Task<Resultado> DeleteSocioAporte(SocioAporte a)
        {
            Resultado r = new();
            SessionInfo si = await GetSessionAsync();
            if (await _context.SocioAporteDeposito.AnyAsync(x => x.SocioAporteId == a.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Aporte com depósitos não pode ser excluído.";
                return r;
            }
            try
            {
                _context.SocioAporte.Remove(a);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;

        }
        public async Task<Resultado> ImportarSocioAporteAsync(List<SocioAporteView> lsv)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            await _context.Database.BeginTransactionAsync();
            try
            {
                List<SocioAporte> ls = new();
                foreach (var i in lsv)
                {
                    ls.Add(i.SocioAporte);
                    i.SocioAporte.TenantId = si.TenantId;
                    i.SocioAporte.Assinado = true;
                    i.SocioAporte.DataAssinatura = i.SocioAporte.DataSolicitacao;
                    i.SocioAporte.GUID = Guid.NewGuid().ToString();
                }
                await _context.SocioAporte.AddRangeAsync(ls);
                await _context.SaveChangesAsync();
                List<SocioAporteDeposito> ld = new();
                foreach (var a in ls)
                {
                    SocioAporteDeposito d = new()
                    {
                        TenantId = si.TenantId,
                        EmpreendimentoSocioId = a.EmpreendimentoSocioId,
                        SocioAporteId = a.Id,
                        Valor = a.Valor,
                        Assinado = true,
                        DataAssinatura = a.DataAssinatura,
                        GUID = Guid.NewGuid().ToString(),
                        DataCriacao = a.DataCriacao,
                        DataDeposito = a.DataSolicitacao
                    };
                    ld.Add(d);
                }
                await _context.SocioAporteDeposito.AddRangeAsync(ld);
                await _context.SaveChangesAsync();
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
        public async Task<Resultado> ImportarSocioRetiradaAsync(List<SocioRetiradaView> lsv)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            await _context.Database.BeginTransactionAsync();
            try
            {
                List<SocioRetirada> ls = new();
                foreach (var i in lsv)
                {
                    ls.Add(i.SocioRetirada);
                    i.SocioRetirada.TenantId = si.TenantId;
                    i.SocioRetirada.Assinado = true;
                    i.SocioRetirada.DataAssinatura = i.SocioRetirada.DataLiberacao;
                    i.SocioRetirada.GUID = Guid.NewGuid().ToString();
                }
                await _context.SocioRetirada.AddRangeAsync(ls);
                await _context.SaveChangesAsync();
                List<SocioRetiradaLancto> ld = new();
                foreach (var a in ls)
                {
                    SocioRetiradaLancto d = new()
                    {
                        TenantId = si.TenantId,
                        EmpreendimentoSocioId = a.EmpreendimentoSocioId,
                        SocioRetiradaId = a.Id,
                        Valor = a.Valor,
                        Assinado = true,
                        DataAssinatura = a.DataAssinatura,
                        GUID = Guid.NewGuid().ToString(),
                        DataCriacao = a.DataCriacao,
                        DataDeposito = a.DataPrevistaPagamento
                    };
                    ld.Add(d);
                }
                await _context.SocioRetiradaLancto.AddRangeAsync(ld);
                await _context.SaveChangesAsync();
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
        public async Task<Resultado> ImportarSocioDebitoAsync(List<SocioDebitoView> lsv)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            string GUIDImport = Guid.NewGuid().ToString();
            await _context.Database.BeginTransactionAsync();
            try
            {
                List<SocioDebito> ls = new();
                foreach (var i in lsv)
                {
                    if (i.SocioDebito.CodigoExterno == "ND")
                    {
                        foreach (var isdl in i.Lanctos)
                        {
                            isdl.GUIDLoteImportado = GUIDImport;
                        }
                        await SaveAmortizacoes(i);
                        continue;
                    }
                    ls.Add(i.SocioDebito);
                    i.SocioDebito.TenantId = si.TenantId;
                    i.SocioDebito.Assinado = true;
                    i.SocioDebito.DataAssinatura = i.SocioDebito.DataLancto;
                    i.SocioDebito.GUID = Guid.NewGuid().ToString();
                }
                if (ls.Count > 0)
                {
                    await _context.SocioDebito.AddRangeAsync(ls);
                    await _context.SaveChangesAsync();
                    List<SocioDebitoLancto> ld = new();
                    foreach (var a in lsv)
                    {
                        foreach (var l in a.Lanctos)
                        {
                            l.TenantId = si.TenantId;
                            l.SocioDebitoId = a.SocioDebito.Id;
                            l.EmpreendimentoSocioId = a.SocioDebito.EmpreendimentoSocioId;
                            l.Assinado = true;
                            l.GUID = Guid.NewGuid().ToString();
                            l.DataAssinatura = a.SocioDebito.DataAssinatura;
                            l.DataCriacao = Constante.Today;
                            l.Origem = OrigemLancto.Digitado;
                            ld.Add(l);
                        }
                    }
                    await _context.SocioDebitoLancto.AddRangeAsync(ld);
                    await _context.SaveChangesAsync();
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
        public async Task<Resultado> AddSocioAporteAsync(SocioAporte a)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = await ValidarAporte(a);
            if (!r.Ok)
            {
                return r;
            }

            a.TenantId = si.TenantId;
            try
            {
                await _context.SocioAporte.AddAsync(a);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;

        }
        public async Task<Resultado> ValidarAporteDeposito(SocioAporteDeposito d)
        {
            Resultado r = new();
            SessionInfo si = await GetSessionAsync();
            var a = await _context.SocioAporte.FirstOrDefaultAsync(x => x.Id == d.SocioAporteId && x.TenantId == si.TenantId);
            if (d.DataDeposito < a.DataLimite.AddMonths(-6) || d.DataDeposito > Constante.Today)
            {
                r.Ok = false;
                r.ErrMsg = "Data inválida";
                return r;
            }
            if (d.Valor <= 0)
            {
                r.Ok = false;
                r.ErrMsg = "Valor inválido";
                return r;
            }
            double vd = Math.Round(await _context.SocioAporteDeposito.Where(x => x.SocioAporteId == d.SocioAporteId && x.Id != d.Id).SumAsync(x => x.Valor), 2);
            if (vd + d.Valor > a.Valor)
            {
                r.Ok = false;
                r.ErrMsg = "Valor do(s) depósitos superior ao valor solicitado.";
                return r;
            }
            r.Ok = true;
            return r;
        }
        public async Task<Resultado> AddSocioAporteDepositoAsync(SocioAporteDeposito d)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = await ValidarAporteDeposito(d);
            if (!r.Ok)
            {
                return r;
            }

            d.TenantId = si.TenantId;
            try
            {
                await _context.SocioAporteDeposito.AddAsync(d);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;

        }
        public async Task<Resultado> UpdateSocioAporteDepositoAsync(SocioAporteDeposito d)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = await ValidarAporteDeposito(d);
            if (!r.Ok)
            {
                return r;
            }

            d.TenantId = si.TenantId;
            try
            {
                _context.SocioAporteDeposito.Update(d);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;

        }
        public async Task<Resultado> DeleteAporteDepositoAsync(SocioAporteDeposito d)
        {
            Resultado r = new();
            try
            {
                await _context.Database.BeginTransactionAsync();
                _context.SocioAporteDeposito.Remove(d);
                await _context.SaveChangesAsync();
                if (d.FileId > 0)
                {
                    UpdaloadService us = new(_context, _sessionStorageService, _sessionService, _mapper, null, null);
                    await us.Delete(d.FileId);
                }
                await _context.Database.CommitTransactionAsync();
                r.Ok = true;
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<List<Periodo>> FindAllPeriodoAsync(Empreendimento e)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Periodo.Where(x => x.EmpreendimentoId == e.Id && x.TenantId == si.TenantId).OrderByDescending(x => x.DataInicio).ToListAsync();
        }
        public async Task<Periodo> FindPeriodoDefaultAsync(Empreendimento e)
        {
            SessionInfo si = await GetSessionAsync();
            Periodo p = await _context.Periodo.FirstOrDefaultAsync(x => x.EmpreendimentoId == e.Id && x.TenantId == si.TenantId && x.DataInicio <= Constante.Today && x.DataFim >= Constante.Today);
            if (p == null)
            {
                p = await _context.Periodo.FirstOrDefaultAsync(x => x.EmpreendimentoId == e.Id && x.TenantId == si.TenantId && x.DataInicio <= Constante.Today.AddMonths(-1) && x.DataFim >= Constante.Today.AddMonths(-1));
                if (p == null)
                {
                    List<Periodo> lp = await _context.Periodo.Where(x => x.EmpreendimentoId == e.Id && x.TenantId == si.TenantId).OrderByDescending(x => x.DataInicio).ToListAsync();
                    if (lp.Count > 0)
                    {
                        return lp[0];
                    }
                    else
                    {
                        return await NovoPeriodoAsync(e);
                    }
                }
                else
                {
                    return p;
                }
            }
            else
            {
                return p;
            }
        }
        public async Task<Periodo> FindLastPeriodoClosedOrAuditAsync(Empreendimento e)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Periodo.Where(x => x.EmpreendimentoId == e.Id && x.TenantId == si.TenantId && (x.Status == StatusPeriodo.Fechado || x.Status == StatusPeriodo.Auditoria)).OrderByDescending(x => x.DataInicio).FirstOrDefaultAsync();
        }
        public async Task<Periodo> FindPeriodoByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Periodo.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
        }
        public async Task<Periodo> FindPeriodoByDataAsync(int empid, DateTime dt)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Periodo.AsNoTracking().FirstOrDefaultAsync(x => x.EmpreendimentoId == empid && x.DataInicio <= dt && x.DataFim >= dt && x.TenantId == si.TenantId);
        }
        public async Task<List<LanctoEmpreendimento>> FindAllLancamentosByPeriodoIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            List<LanctoEmpreendimento> r = new();
            try
            {
                var l = await (from LanctoEmpreendimento in _context.LanctoEmpreendimento
                               join Periodo in _context.Periodo on LanctoEmpreendimento.PeriodoId equals Periodo.Id
                               join PlanoConta in _context.PlanoConta on LanctoEmpreendimento.PlanoContaId equals PlanoConta.Id
                               join LanctoEmpRelacionamento in _context.LanctoEmpRelacionamento on LanctoEmpreendimento.Id equals LanctoEmpRelacionamento.LanctoEmpreendimentoId into lr
                               from lr1 in lr.DefaultIfEmpty()
                               join Relacionamento in _context.Relacionamento on lr1.RelacionamentoId equals Relacionamento.Id into rela
                               from rela1 in rela.DefaultIfEmpty()

                               join LanctoEmpUnidade in _context.LanctoEmpUnidade on LanctoEmpreendimento.Id equals LanctoEmpUnidade.LanctoEmpreendimentoId into lu
                               from lu1 in lu.DefaultIfEmpty()
                               join UnidadeEmpreendimento in _context.UnidadeEmpreendimento on lu1.UnidadeEmpreendimentoId equals UnidadeEmpreendimento.Id into un
                               from u in un.DefaultIfEmpty()

                               select new { LanctoEmpreendimento, Periodo, lr1, rela1, u, PlanoContaV = PlanoConta.Nome }).Where(x => x.Periodo.Id == id && x.LanctoEmpreendimento.TenantId == si.TenantId).ToListAsync();
                foreach (var it in l)
                {
                    it.LanctoEmpreendimento.PlanoContaV = it.PlanoContaV;
                    if (it.rela1 != null)
                    {
                        it.LanctoEmpreendimento.LanctoEmpRelacionamento = new LanctoEmpRelacionamento() { LanctoEmpreendimentoId = it.LanctoEmpreendimento.Id };
                        it.LanctoEmpreendimento.LanctoEmpRelacionamento.RelacionamentoNome = it.rela1.Nome;
                        it.LanctoEmpreendimento.LanctoEmpRelacionamento.RelacionamentoId = it.rela1.Id;
                    }
                    else
                    {
                        it.LanctoEmpreendimento.LanctoEmpRelacionamento = new LanctoEmpRelacionamento() { LanctoEmpreendimentoId = it.LanctoEmpreendimento.Id };
                        it.LanctoEmpreendimento.LanctoEmpRelacionamento.RelacionamentoNome = string.Empty;
                        it.LanctoEmpreendimento.LanctoEmpRelacionamento.RelacionamentoId = 0;
                    }
                    if (it.u != null)
                    {
                        it.LanctoEmpreendimento.LanctoEmpUnidade = new LanctoEmpUnidade() { LanctoEmpreendimentoId = it.LanctoEmpreendimento.Id };
                        it.LanctoEmpreendimento.LanctoEmpUnidade.UnidadeEmpreendimento = new UnidadeEmpreendimento() { CodigoExterno = it.u.CodigoExterno, Id = it.u.Id };
                        it.LanctoEmpreendimento.LanctoEmpUnidade.UnidadeEmpreendimentoId = it.u.Id;
                    }
                    else
                    {
                        it.LanctoEmpreendimento.LanctoEmpUnidade = new LanctoEmpUnidade() { LanctoEmpreendimentoId = it.LanctoEmpreendimento.Id };
                        it.LanctoEmpreendimento.LanctoEmpUnidade.UnidadeEmpreendimento = new UnidadeEmpreendimento() { CodigoExterno = string.Empty, Id = 0 };
                    }
                    r.Add(it.LanctoEmpreendimento);
                }
            }
            catch
            {
                throw;
            }
            return r;

        }
        public async Task<Resultado> DeletePeriodoAsync(Periodo p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new() { Ok = true };

            if (await _context.Periodo.AnyAsync(x => x.EmpreendimentoId == p.EmpreendimentoId && x.TenantId == si.TenantId && x.DataInicio > p.DataInicio))
            {
                r.Ok = false;
                r.ErrMsg = "Existe período posterior ao período selecionado";
                return r;
            }

            if (await _context.LanctoEmpreendimento.AnyAsync(x => x.PeriodoId == p.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = Constante.ItemaNaoPodeSerExcluido("Período");
                return r;
            }
            if (await _context.LanctoLoteImportacao.AnyAsync(x => x.PeriodoId == p.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = Constante.ItemaNaoPodeSerExcluido("Período");
                return r;
            }
            _context.Periodo.Remove(p);
            await _context.SaveChangesAsync();

            return r;
        }
        public async Task<Periodo> NovoPeriodoDataAsync(int empreendimentoId, DateTime dt, TipoAlerta ta = TipoAlerta.none)
        {
            SessionInfo si = await GetSessionAsync();

            Periodo r = new()
            {
                DataInicio = Utils.UtilsClass.GetInicio(dt),
                DataFim = Utils.UtilsClass.GetUltimo(dt),
                EmpreendimentoId = empreendimentoId,
                TenantId = si.TenantId,
                Status = StatusPeriodo.EntradaDeDados,
                Alerta = ta
            };
            await _context.Periodo.AddAsync(r);
            await _context.SaveChangesAsync();

            return r;
        }
        public async Task<Periodo> NovoPeriodoAsync(Empreendimento e)
        {
            SessionInfo si = await GetSessionAsync();

            Periodo r = new();
            Periodo lastP = null;

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
    "select top 1 * from Periodo  where EmpreendimentoId = @eid and TenantId = @tid and Status <> 3 order by DataInicio desc ";

            command.Parameters.Add(new SqlParameter("@eid", System.Data.SqlDbType.Int));
            command.Parameters["@eid"].Value = e.Id;
            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            await command.Connection.OpenAsync();

            try
            {
                var lu = await command.ExecuteReaderAsync();
                while (await lu.ReadAsync())
                {
                    lastP = new Periodo()
                    {
                        Id = (int)lu["Id"],
                        DataInicio = (DateTime)lu["DataInicio"],
                        DataFim = (DateTime)lu["DataFim"]
                    };
                }
                lu.Close();
            }
            catch
            {
                command.Connection.Close();
                throw;
            }
            finally
            {
                command.Connection.Close();
            }
            if (lastP == null)
            {
                r.DataInicio = Utils.UtilsClass.GetInicio(e.DataInicioOperacao);
                r.DataFim = Utils.UtilsClass.GetUltimo(e.DataInicioOperacao);
            }
            else
            {
                if (lastP.DataFim < Constante.Today)
                {
                    r.DataInicio = lastP.DataFim.AddDays(1);
                    r.DataFim = Utils.UtilsClass.GetUltimo(r.DataInicio);
                }
                else
                {
                    return null;
                }

            }

            r.EmpreendimentoId = e.Id;
            r.TenantId = si.TenantId;
            r.Status = StatusPeriodo.EntradaDeDados;

            await _context.Periodo.AddAsync(r);
            await _context.SaveChangesAsync();

            return r;
        }
        public async Task<Resultado> IncluirLancamentoEmpreendimento(int periodoid, LanctoEmpreendimento lancto)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            lancto.PeriodoId = periodoid;
            lancto.TenantId = si.TenantId;
            lancto.Rateado = false;

            await _context.Database.BeginTransactionAsync();
            try
            {
                lancto.LanctoEmpUnidade.UnidadeEmpreendimento = null;
                lancto.LanctoEmpUnidade.LanctoEmpreendimento = null;
                await _context.LanctoEmpreendimento.AddAsync(lancto);
                await _context.SaveChangesAsync();

                if (lancto.LanctoEmpRelacionamento.RelacionamentoId != 0)
                {
                    lancto.LanctoEmpRelacionamento.LanctoEmpreendimentoId = lancto.Id;
                    lancto.LanctoEmpRelacionamento.TenantId = si.TenantId;
                    lancto.LanctoEmpRelacionamento.Id = 0;
                    await _context.LanctoEmpRelacionamento.AddAsync(lancto.LanctoEmpRelacionamento);
                    await _context.SaveChangesAsync();
                }

                if (lancto.LanctoEmpUnidade.UnidadeEmpreendimentoId != 0)
                {
                    lancto.LanctoEmpUnidade.LanctoEmpreendimentoId = lancto.Id;
                    lancto.LanctoEmpUnidade.TenantId = si.TenantId;
                    lancto.LanctoEmpUnidade.UnidadeEmpreendimento = null;
                    lancto.LanctoEmpUnidade.LanctoEmpreendimento = null;
                    lancto.LanctoEmpUnidade.Id = 0;
                    await _context.LanctoEmpUnidade.AddAsync(lancto.LanctoEmpUnidade);
                    await _context.SaveChangesAsync();
                }
                _context.Database.CommitTransaction();
                r.Ok = true;
                r.ErrMsg = string.Empty;
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
            return r;
        }
        public async Task<Resultado> UpdateLancamentoEmpreendimento(LanctoEmpreendimento lancto)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (!await _context.LanctoEmpreendimento.AnyAsync(x => x.Id == lancto.Id && x.TenantId == si.TenantId))
            {
                r.ErrMsg = "lançamentos não encontrado";
                r.Ok = false;
                return r;
            }
            lancto.TenantId = si.TenantId;
            lancto.Rateado = false;
            lancto.Erro = false;
            lancto.ErroMsg = string.Empty;

            await _context.Database.BeginTransactionAsync();
            try
            {
                lancto.LanctoEmpUnidade.UnidadeEmpreendimento = null;
                lancto.LanctoEmpUnidade.LanctoEmpreendimento = null;
                _context.LanctoEmpreendimento.Update(lancto);
                await _context.SaveChangesAsync();

                var un = await _context.LanctoEmpUnidade.FirstOrDefaultAsync(x => x.LanctoEmpreendimentoId == lancto.Id);
                if (un != null)
                {
                    _context.LanctoEmpUnidade.Remove(un);
                    await _context.SaveChangesAsync();
                }
                var re = await _context.LanctoEmpRelacionamento.FirstOrDefaultAsync(x => x.LanctoEmpreendimentoId == lancto.Id);
                if (re != null)
                {
                    _context.LanctoEmpRelacionamento.Remove(re);
                    await _context.SaveChangesAsync();
                }

                if (lancto.LanctoEmpRelacionamento.RelacionamentoId != 0)
                {
                    lancto.LanctoEmpRelacionamento.LanctoEmpreendimentoId = lancto.Id;
                    lancto.LanctoEmpRelacionamento.TenantId = si.TenantId;
                    lancto.LanctoEmpRelacionamento.Id = 0;
                    await _context.LanctoEmpRelacionamento.AddAsync(lancto.LanctoEmpRelacionamento);
                    await _context.SaveChangesAsync();
                }
                if (lancto.LanctoEmpUnidade.UnidadeEmpreendimentoId != 0)
                {
                    lancto.LanctoEmpUnidade.LanctoEmpreendimentoId = lancto.Id;
                    lancto.LanctoEmpUnidade.TenantId = si.TenantId;
                    lancto.LanctoEmpUnidade.UnidadeEmpreendimento = null;
                    lancto.LanctoEmpUnidade.LanctoEmpreendimento = null;
                    lancto.LanctoEmpUnidade.Id = 0;
                    await _context.LanctoEmpUnidade.AddAsync(lancto.LanctoEmpUnidade);
                    await _context.SaveChangesAsync();
                }

                _context.Database.CommitTransaction();
                _context.ChangeTracker.AcceptAllChanges();
                r.Ok = true;
                r.ErrMsg = string.Empty;
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> ExluirLancamentoAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();

            Resultado r = new();

            LanctoEmpreendimento l = await _context.LanctoEmpreendimento.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);

            if (l == null)
            {
                r.Ok = false;
                r.ErrMsg = "Lançamento não encontrado.";
                return r;
            }
            Periodo p = await _context.Periodo.FirstOrDefaultAsync(x => x.Id == l.PeriodoId && x.TenantId == si.TenantId);
            if (p.Status != StatusPeriodo.EntradaDeDados)
            {
                r.Ok = false;
                r.ErrMsg = "Exclusão permitida somente para período aberto";
                return r;
            }
            if (await _context.LanctoEmpreendimentoLanctoImp.AnyAsync(x => x.LanctoEmpreendimentoId == id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Lançamento importado não pode ser excluído, utilizar a operãções lotes";
            }

            await _context.Database.BeginTransactionAsync();
            try
            {

                var un = await _context.LanctoEmpUnidade.FirstOrDefaultAsync(x => x.LanctoEmpreendimentoId == l.Id);
                if (un != null)
                {
                    _context.LanctoEmpUnidade.Remove(un);
                    await _context.SaveChangesAsync();
                }
                var re = await _context.LanctoEmpRelacionamento.FirstOrDefaultAsync(x => x.LanctoEmpreendimentoId == l.Id);
                if (re != null)
                {
                    _context.LanctoEmpRelacionamento.Remove(re);
                    await _context.SaveChangesAsync();
                }

                _context.LanctoEmpreendimento.Remove(l);
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();

                r.Ok = true;
                r.ErrMsg = string.Empty;

            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }

            r.Ok = true;

            return r;

        }
        public async Task<Resultado> SetPeriodoStatusAsync(Periodo p, StatusPeriodo s)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            if (p.TenantId != si.TenantId)
            {
                throw new Exception("TenantId período != tenantid sessão");
            }
            if (s == StatusPeriodo.Auditoria)
            {
                if (await _context.Periodo.AnyAsync(x => x.TenantId == si.TenantId && x.EmpreendimentoId == p.EmpreendimentoId && x.DataFim < p.DataInicio && x.Status != StatusPeriodo.Fechado))
                {
                    r.Ok = false;
                    r.ErrMsg = "Existe período anterior que não está fechado. Todos os períodos anteriores devem estar fechados.";
                    return r;
                }
            }
            bool mytrans = (_context.Database.CurrentTransaction == null);
            if (mytrans)
            {
                await _context.Database.BeginTransactionAsync();
            }
            try
            {
                if (p.Status != StatusPeriodo.EntradaDeDados)
                {
                    p.Alerta = TipoAlerta.none;
                }
                if (p.Status == StatusPeriodo.Fechado)
                {
                    var la = await _context.SocioResultadoPeriodo.Where(x => x.PeriodoId == p.Id && x.TenantId == si.TenantId).ToListAsync();
                    foreach (var a in la)
                    {
                        await _context.Database.ExecuteSqlRawAsync(" Delete SocioCorrecaoResultadoRetida where TenantId = '" + si.TenantId + "'  and SocioResultadoPeriodoId = " + a.Id.ToString());
                        if (a.Assinado)
                        {
                            if (!a.Cancelado)
                            {
                                a.Cancelado = true;
                                a.DataCancelamento = Constante.Now;
                                a.Historico += " - cancelado por reprocessamento do período";
                                _context.SocioResultadoPeriodo.Update(a);
                            }
                        }
                        else
                        {
                            _context.SocioResultadoPeriodo.Remove(a);
                        }
                    };
                }
                if ((p.Status == StatusPeriodo.Auditoria || p.Status == StatusPeriodo.Fechamento || p.Status == StatusPeriodo.Fechado) && s == StatusPeriodo.EntradaDeDados)
                {
                    var lpv = await _context.PeriodoAuditoriaVersao.Where(x => x.PeriodoId == p.Id && x.TenantId == si.TenantId && x.DataCancelamento == DateTime.MinValue).ToListAsync();
                    foreach (var pv in lpv)
                    {
                        pv.DataCancelamento = Constante.Now;
                        var lpvs = await _context.PeriodoSocioAuditoria.Where(x => x.PeriodoAuditoriaVersaoId == pv.Id && x.TenantId == si.TenantId && x.Status != StatusAuditoriaSocio.Cancelado).ToListAsync();
                        foreach (var pvs in lpvs)
                        {
                            pvs.Status = StatusAuditoriaSocio.Cancelado;
                            _context.PeriodoSocioAuditoria.Update(pvs);
                        }
                        _context.PeriodoAuditoriaVersao.Update(pv);
                    };
                }
                p.Status = s;
                if (s == StatusPeriodo.EntradaDeDados)
                {
                    await _context.Database.ExecuteSqlRawAsync(
        "delete  LanctoEmpRateadoPlanoContaSocio  " +
        " where PeriodoId = " + p.Id + " and TenantId =  '" + si.TenantId + "'");
                    await _context.Database.ExecuteSqlRawAsync(
        "update LanctoEmpreendimento  set Rateado = 0" +
        " where PeriodoId = " + p.Id + " and TenantId =  '" + si.TenantId + "'");
                }

                _context.Periodo.Update(p);
                await _context.SaveChangesAsync();
                if (mytrans)
                {
                    await _context.Database.CommitTransactionAsync();
                }
                r.Ok = true;
                return r;
            }
            catch (Exception e)
            {
                if (mytrans)
                {
                    await _context.Database.RollbackTransactionAsync();
                    r.Ok = false;
                    r.ErrMsg = e.Message;
                    return r;
                }
                else
                {
                    throw;
                }
            }
        }
        public async Task<Resultado> LiberarAcessoDadosPeriodo(Periodo p, bool enviaremail)
        {
            Resultado r = new();
            p.DadosLiberado = true;
            p.Alerta = TipoAlerta.Liberado;
            _context.Periodo.Update(p);
            await _context.SaveChangesAsync();
            if (enviaremail)
            {
                await EnviarEmailFechamentoPeriodo(p);
            }
            return r;
        }

        public async Task<Resultado> EnviarEmailFechamentoPeriodo(Periodo p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            var empreend = await _context.Empreendimento.Where(x => x.TenantId == si.TenantId && x.Id == p.EmpreendimentoId).FirstOrDefaultAsync();
            var empresa = await _context.Empresa.Where(x => x.TenantId == si.TenantId && x.Id == empreend.EmpresaId).FirstOrDefaultAsync();

            string nomeEmp = await _context.Empreendimento.Where(x => x.TenantId == si.TenantId && x.Id == p.EmpreendimentoId).Select(x => x.Nome).FirstOrDefaultAsync();

            var ls = await (from EmpreendimentoSocio in _context.EmpreendimentoSocio
                            join Socio in _context.Socio on EmpreendimentoSocio.SocioId equals Socio.Id
                            join EmpSocioUser in _context.EmpSocioUser on EmpreendimentoSocio.Id equals EmpSocioUser.EmpreendimentoSocioId into empuser
                            from cc in empuser.DefaultIfEmpty()
                            select new { EmailSocio = Socio.Email, EmpreendimentoSocio, NomeUser = cc.Nome ?? string.Empty, EmailUser = cc.Email ?? string.Empty, ReceberEmail = (FormaReceberEmail?)cc.ReceberEmail ?? FormaReceberEmail.NaoReceber }
              ).Where(x => x.EmpreendimentoSocio.EmpreendimentoId == p.EmpreendimentoId && x.EmpreendimentoSocio.TenantId == si.TenantId).ToListAsync();

            List<EmpreendimentoSocio> empsos = new();
            foreach (var se in ls)
            {
                var eso = empsos.FirstOrDefault(x => x.Id == se.EmpreendimentoSocio.Id);
                if (eso == null)
                {
                    empsos.Add(se.EmpreendimentoSocio);
                    se.EmpreendimentoSocio.EmpSocioUsers = new();
                    EmpSocioUser ess = new()
                    {
                        Email = se.EmailSocio,
                        Nome = se.EmpreendimentoSocio.Nome,
                        ReceberEmail = se.EmpreendimentoSocio.ReceberEmail
                    };
                    se.EmpreendimentoSocio.EmpSocioUsers.Add(ess);
                }
                if (se.EmailUser != string.Empty)
                {
                    EmpSocioUser esu = se.EmpreendimentoSocio.EmpSocioUsers.FirstOrDefault(x => x.Email == se.EmailUser);
                    if (esu == null)
                    {
                        esu = new()
                        {
                            Email = se.EmailUser,
                            Nome = se.NomeUser,
                            ReceberEmail = se.ReceberEmail
                        };
                        se.EmpreendimentoSocio.EmpSocioUsers.Add(esu);
                    };
                }
            }

            List<LinhaSocio> linhasSocio = await LoadDadosSocioPlanoGerencialByDate(p.EmpreendimentoId, p.DataInicio, p.DataFim);

            SegurancaService sgs = new(_context, null, null, _sessionStorageService, _sessionService, _configService, null);

            foreach (var se in empsos)
            {
                string ep = string.Empty;
                string ecc = string.Empty;
                string ecco = string.Empty;

                foreach (var su in se.EmpSocioUsers)
                {
                    if (su.ReceberEmail == FormaReceberEmail.Principal)
                    {
                        ep += su.Email + ";";
                    }
                    if (su.ReceberEmail == FormaReceberEmail.Copia)
                    {
                        ecc += su.Email + ";";
                    }
                    if (su.ReceberEmail == FormaReceberEmail.CopiaOculta)
                    {
                        ecco += su.Email + ";";
                    }
                }

                if (ep == string.Empty && ecc == string.Empty && ecco == string.Empty)
                {
                    continue;
                }
                if (ep == string.Empty)
                {
                    ep = ecc;
                    ecc = string.Empty;
                }

                List<SfAttachment> atts = new();

                List<PeriodoReport> lpr = await FindAllPeriodoReportsByPeriodoId(p.Id, se.Id);
                if (lpr.Count > 0)
                {
                   lpr = lpr.Where(x => x.EnviarComoAnexo == true).ToList();
                    if (lpr.Count > 0)
                    {
                        UpdaloadService ups = new(_context, _sessionStorageService, _sessionService, _mapper, _webHostEnvironment, _configService);
                        foreach (var pr in lpr)
                        {
                            if (pr.FileId > 0)
                            {
                                Arquivo a = await _context.Arquivo.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == pr.FileId);
                                if (a != null)
                                {
                                    SfAttachment att = new();
                                    att.FullPath = Path.Combine(_webHostEnvironment.WebRootPath, await ups.SaveToFolder(pr.FileId));
                                    att.FileName = pr.NomeReport.Replace(" ", "") + "." + Path.GetExtension(a.FileNome); ;
                                    atts.Add(att);
                                }
                            }
                        }
                    }
                }


                int socioNomePrincipalId = await GetSocioPrincipalIdBySocioEmp(p.EmpreendimentoId, se.Id);
                string nomeGrupo = string.Empty;
                if (socioNomePrincipalId > 0)
                {
                    nomeGrupo = await _context.GrupoSocio.Where(x => x.TenantId == si.TenantId && x.Id == socioNomePrincipalId).Select(x => x.Nome).FirstOrDefaultAsync();
                }
                var lso = linhasSocio.Where(x => x.EmpreendimentoSocioId == se.Id ||
                                            (x.SocioNomePrincipalId == socioNomePrincipalId && x.SocioNomePrincipalId > 0)).ToList();


                string texto =
                    "<!DOCTYPE html> " +
                    " <head> " +
                    " <style> " +
                    "table { " +
                    " text-align: left; " +
                    " font-family: arial, sans-serif; " +
                    " border-collapse: collapse; " +
                    " font-color:4e226c; " +
                    " margin-left: 20px; " +
                    " margin-top: 20px; " +
                    " border: 1px solid #4e226c; " +
                    " }" +
                    " th {   " +
                    " text-align: center; " +
                    " padding: 2px; " +
                    " margin-right: 5px; " +
                    " margin-left: 5px; " +
                    " color:#4e226c; " +
                    " } " +
                    " td { " +
                    " padding-left: 5px; " +
                    " padding-right: 5px; " +
                    " padding-top: 2px; " +
                    " padding-bottom: 2px; " +
                    " margin-right: 5px; " +
                    " margin-left: 5px; " +
                    " } " +
                    " </style> " +
                    "<meta charset = 'utf-8' /> " +
                    "<title> SmartFOX - Sistema de gestão empresarial </title >  " +
                    "</head> " +
                    "<body> " +
                    "<div style='border: 1px solid 4e226c;' >" +
                    "<p style = 'background-color: white;text-align:left;margin:10px;margin-bottom:0px;margin-top:20px;padding-left:10px;font-size:16px;font-weight:normal;font-family:verdana;'> " +
                    "Prezado(a) " + se.Nome + "  </p>" +
                    "<p style = 'background-color: white;text-align:left;margin:10px;margin-bottom:0px;margin-top:20px;padding-left:10px;font-size:16px;font-weight:normal;font-family:verdana;'> " +
                    "Segue abaixo o resumo financeiro referente ao mês de " + p.DataFim.ToString("MMMM/yyyy") + "</p> " +

                    FormataHtmlResultado(p,lso) +

                    "<p style = 'background-color: white;text-align:left;margin:10px;margin-bottom:0px;margin-top:20px;padding-left:10px;font-size:16px;font-weight:normal;font-family:verdana;'> " +
                    "Para ter acesso à prestação de contas completa, acesse o Portal Incorp no link www.sf21app.com </p>" +

                    "<p style = 'background-color: white;text-align:left;margin:10px;margin-bottom:0px;margin-top:20px;padding-left:10px;font-size:16px;font-weight:normal;font-family:verdana;'> " +
                    "Caso tenha qualquer dúvida, favor enviar e-mail para " + empresa.EmailRI + "</p>" +

                    "<p style = 'background-color: white;text-align:left;margin:10px;margin-bottom:0px;margin-top:20px;padding-left:10px;font-size:16px;font-weight:normal;font-family:verdana;'> " +
                    "Atenciosamente, </p>" +
                    "<p style = 'background-color: white;text-align:left;margin:10px;margin-bottom:0px;margin-top:20px;padding-left:10px;font-size:16px;font-weight:normal;font-family:verdana;'> " +
                    empresa.Nome +  "</p>" +
                    "</div>" +
                    "</body>" +
                    "</html> ";

                string titulo;
                if (nomeGrupo == string.Empty)
                {
                    titulo = empreend.Nome + " - " + p.DataFim.ToString("MMMM/yyyy");
                }
                else
                {
                    titulo = empreend.Nome + " - " + p.DataFim.ToString("MMMM/yyyy") + " - " + nomeGrupo;
                }
                sgs.SendEmail(titulo, texto, ep, ecc, ecco, atts, false);

            }

            return r;
        }


        protected string AddLinhaTabelaEmail(string t, double va, double vp, double vf, string style = null)
        {
            if (style == null)
            {
                style = string.Empty;

            }
            if (style != string.Empty)
            {
                return "<tr> <td + style='" + style + "'>" + t + "</td> " +
                 " <td style='" + style + ";text-align:right'>" + vp.ToString("N2", Constante.Culture) + "</td></tr> ";
            }
            else
            {
                return "<tr> <td>" + t + "</td> " +
                " <td style='text-align:right'>" + vp.ToString("N2", Constante.Culture) + "</td></tr> ";
            }
        }

        protected string FormataHtmlResultado(Periodo p, List<LinhaSocio> linhasSocio)
        {
            try
            {

                string r = string.Empty;

                for (int j = 0; j < linhasSocio.Count; j++)
                {
                    LinhaSocio so = linhasSocio[j];
                    if (linhasSocio.Count == 2 && so.EmpreendimentoSocioId == 0)
                    {
                        continue;
                    }

                    so.LinhaPeriodos.RemoveAll(x => x.PeriodoId != p.Id);
                    r += " <table> ";

                    if (linhasSocio.Count != 2)
                    {
                        r += " <tr> ";
                        r += " <th style='background-color:#e4e4e4' colspan=2 > " + so.SocioNome + " </td> ";
                        r += " </tr> ";
                    }

                    so.LinhaSaldoCapital.LinhaPeriodos.RemoveAll(x => x.PeriodoId != p.Id);
                    if (so.LinhaSaldoCapital.LinhaPeriodos.Count > 0)
                    {
                        r += AddLinhaTabelaEmail("Direitos períodos anteriores (1)", 0, so.LinhaSaldoCapital.LinhaPeriodos[0].Valor, 0);
                    }
                    else
                    {
                        r += AddLinhaTabelaEmail("Direitos períodos anteriores (1)", 0, 0, 0);
                    }

                    so.LinhaAporte.LinhaPeriodos.RemoveAll(x => x.PeriodoId != p.Id);
                    if (so.LinhaAporte.LinhaPeriodos.Count > 0)
                    {
                        r += AddLinhaTabelaEmail("Aportes de capital (2)", so.LinhaAporte.ValorAnterior, so.LinhaAporte.LinhaPeriodos[0].Valor, so.LinhaAporte.ValorTotal);
                    }
                    else
                    {
                        r += AddLinhaTabelaEmail("Aportes de capital (2)", so.LinhaAporte.ValorAnterior, 0, so.LinhaAporte.ValorTotal);
                    }

                    so.LinhaSaldoCapital.LinhaPeriodos.RemoveAll(x => x.PeriodoId != p.Id);
                    if (so.LinhaSaldoCapital.LinhaPeriodos.Count > 0)
                    {
                        r += AddLinhaTabelaEmail("Saldo de capital (3=1+2)", so.LinhaSaldoCapital.ValorAnterior, so.LinhaSaldoCapital.LinhaPeriodos[0].Valor, so.LinhaSaldoCapital.ValorTotal);
                    }
                    else
                    {
                        r += AddLinhaTabelaEmail("Saldo de capital (3=1+2)", so.LinhaSaldoCapital.ValorAnterior, 0, so.LinhaSaldoCapital.ValorTotal);
                    }

                    if (so.LinhaPeriodos.Count > 0)
                    {
                        r += AddLinhaTabelaEmail("Participação nas receitas (4)", so.ValorAnteriorReceita, so.LinhaPeriodos[0].ValorReceita, so.ValorTotalReceita);
                    }
                    else
                    {
                        r += AddLinhaTabelaEmail("Participação nas receitas (4)", so.ValorAnteriorReceita, 0, so.ValorTotalReceita);
                    }

                    if (so.LinhaPeriodos.Count > 0)
                    {
                        r += AddLinhaTabelaEmail("Participação nas despesas (5)", so.ValorAnteriorDespesa, so.LinhaPeriodos[0].ValorDespesa, so.ValorTotalDespesa);
                    }
                    else
                    {
                        r += AddLinhaTabelaEmail("Participação nas despesas (5)", so.ValorAnteriorDespesa, 0, so.ValorTotalDespesa);
                    }

                    if (so.LinhaPeriodos.Count > 0)
                    {
                        r += AddLinhaTabelaEmail("Resultado do mes (6=4-5)", so.ValorAnteriorReceita - so.ValorAnteriorDespesa, so.LinhaPeriodos[0].ValorReceita - so.LinhaPeriodos[0].ValorDespesa, so.ValorTotalReceita - so.ValorTotalDespesa, "background-color:#e4e4e4;font-weight:bold;");
                    }
                    else
                    {
                        r += AddLinhaTabelaEmail("Resultado dos mes (6=4-5)", so.ValorAnteriorReceita - so.ValorAnteriorDespesa, 0, so.ValorTotalReceita - so.ValorTotalDespesa, "background-color:#e4e4e4;font-weight:bold;");
                    }

                    so.LinhaResultado.LinhaPeriodos.RemoveAll(x => x.PeriodoId != p.Id);
                    if (so.LinhaResultado.LinhaPeriodos.Count > 0)
                    {
                        r += AddLinhaTabelaEmail("Saldo a distribuir (7=3+6)", so.LinhaResultado.ValorAnterior, so.LinhaResultado.LinhaPeriodos[0].Valor, so.LinhaResultado.ValorTotal);
                    }
                    else
                    {
                        r += AddLinhaTabelaEmail("Saldo a distribuir (7=3+6)", so.LinhaResultado.ValorAnterior, 0, so.LinhaResultado.ValorTotal);
                    }

                    so.LinhaCorrecaoDistruibuicao.LinhaPeriodos.RemoveAll(x => x.PeriodoId != p.Id);
                    if (so.LinhaCorrecaoDistruibuicao.LinhaPeriodos.Count > 0)
                    {
                        r += AddLinhaTabelaEmail("Juros e correções sobre distribuições retidas (8)", so.LinhaCorrecaoDistruibuicao.ValorAnterior, so.LinhaCorrecaoDistruibuicao.LinhaPeriodos[0].Valor, so.LinhaCorrecaoDistruibuicao.ValorTotal);
                    }
                    else
                    {
                        r += AddLinhaTabelaEmail("Juros e correções sobre distribuições retidas (8)", so.LinhaCorrecaoDistruibuicao.ValorAnterior, 0, so.LinhaCorrecaoDistruibuicao.ValorTotal);
                    }

                    so.LinhaDistribuicao.LinhaPeriodos.RemoveAll(x => x.PeriodoId != p.Id);
                    if (so.LinhaDistribuicao.LinhaPeriodos.Count > 0)
                    {
                        r += AddLinhaTabelaEmail("Distribuição autorizada (9)", so.LinhaDistribuicao.ValorAnterior, so.LinhaDistribuicao.LinhaPeriodos[0].Valor, so.LinhaDistribuicao.ValorTotal);
                    }
                    else
                    {
                        r += AddLinhaTabelaEmail("Distribuição autorizada (9)", so.LinhaDistribuicao.ValorAnterior, 0, so.LinhaDistribuicao.ValorTotal);
                    }

                    so.LinhaRetencao.LinhaPeriodos.RemoveAll(x => x.PeriodoId != p.Id);
                    if (so.LinhaRetencao.LinhaPeriodos.Count > 0)
                    {
                        r += AddLinhaTabelaEmail("Retenções / Amortizações (10)", so.LinhaRetencao.ValorAnterior, so.LinhaRetencao.LinhaPeriodos[0].Valor, so.LinhaRetencao.ValorTotal);
                    }
                    else
                    {
                        r += AddLinhaTabelaEmail("Retenções / Amortizações (10)", so.LinhaRetencao.ValorAnterior, 0, so.LinhaRetencao.ValorTotal);
                    }

                    so.LinhaDistribuicao.LinhaPeriodos.RemoveAll(x => x.PeriodoId != p.Id);
                    if (so.LinhaDistribuicao.LinhaPeriodos.Count > 0 && so.LinhaRetencao.LinhaPeriodos.Count > 0)
                    {
                        r += AddLinhaTabelaEmail("Valor líquido a receber (9-10)", so.LinhaDistribuicao.ValorAnterior, so.LinhaDistribuicao.LinhaPeriodos[0].Valor - so.LinhaRetencao.LinhaPeriodos[0].Valor, so.LinhaDistribuicao.ValorTotal, "background-color:#f7e488;font-weight:bold;");
                    }
                    else
                    {
                        if (so.LinhaDistribuicao.LinhaPeriodos.Count > 0)
                        {
                            r += AddLinhaTabelaEmail("Valor líquido a receber (9-10)", so.LinhaDistribuicao.ValorAnterior, so.LinhaDistribuicao.LinhaPeriodos[0].Valor, so.LinhaDistribuicao.ValorTotal, "background-color:#f7e488;font-weight:bold;");
                        }
                        else
                        {
                            r += AddLinhaTabelaEmail("Valor líquido a receber (9-10)", so.LinhaDistribuicao.ValorAnterior, 0, so.LinhaDistribuicao.ValorTotal, "background-color:#f7e488;font-weight:bold;");
                        }
                    }

                    so.LinhaSaldoDistribuir.LinhaPeriodos.RemoveAll(x => x.PeriodoId != p.Id);
                    if (so.LinhaSaldoDistribuir.LinhaPeriodos.Count > 0)
                    {
                        r += AddLinhaTabelaEmail("Saldo distribuições futuras (7+8-9)", so.LinhaSaldoDistribuir.ValorAnterior, so.LinhaSaldoDistribuir.LinhaPeriodos[0].Valor, so.LinhaSaldoDistribuir.ValorTotal);
                    }
                    else
                    {
                        r += AddLinhaTabelaEmail("Saldo distribuições futuras (7+8-9)", so.LinhaSaldoDistribuir.ValorAnterior, 0, so.LinhaSaldoDistribuir.ValorTotal);
                    }
                    r += "</table>";
                    r += "<p style = 'background-color: white;color:white;margin:10px;font-size:12px;font-family:verdana;'>.</p>";
                }

                return r;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        public async Task<Resultado> BloquearAcessoDadosPeriodo(Periodo p)
        {
            Resultado r = new();
            p.DadosLiberado = false;
            p.Alerta = TipoAlerta.none;
            _context.Periodo.Update(p);
            await _context.SaveChangesAsync();
            return r;
        }
        public async Task<Resultado> SetPeriodoAlertaAsync(Periodo p, TipoAlerta a)
        {
            Resultado r = new();
            p.Alerta = a;
            try
            {
                _context.Periodo.Update(p);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> CancelarListPeriodo(int periodoidinicial)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            Periodo p = await _context.Periodo.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == periodoidinicial);
            if (p == null)
            {
                r.Ok = false;
                r.ErrMsg = "Período não encontrado";
                return r;
            }
            if (p.Status == StatusPeriodo.EntradaDeDados)
            {
                r.Ok = false;
                r.ErrMsg = "Período já está na fase de entrada de dados";
                return r;
            }
            List<Periodo> lp = await _context.Periodo.Where(x => x.TenantId == si.TenantId && x.EmpreendimentoId == p.EmpreendimentoId && x.DataInicio > p.DataInicio).ToListAsync();
            lp.Add(p);
            lp = lp.OrderByDescending(x => x.DataInicio).ToList();

            await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var p1 in lp)
                {
                    if (p1.Status != StatusPeriodo.EntradaDeDados)
                    {
                        r = await SetPeriodoStatusAsync(p1, StatusPeriodo.EntradaDeDados);
                        if (!r.Ok)
                        {
                            await _context.Database.RollbackTransactionAsync();
                            return r;
                        }
                    }
                }
                await _context.Database.CommitTransactionAsync();
                r.Ok = true;
                return r;
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<Resultado> ReprocessarFechamento(int periodoidinicial)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            _context.ChangeTracker.Clear();

            Periodo p = await _context.Periodo.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == periodoidinicial);
            if (p == null)
            {
                r.Ok = false;
                r.ErrMsg = "Período não encontrado";
                return r;
            }
            if (p.Status != StatusPeriodo.EntradaDeDados)
            {
                r.Ok = false;
                r.ErrMsg = "Período deve estar na fase de entrada de dados";
                return r;
            }
            List<Periodo> lp = await _context.Periodo.Where(x => x.TenantId == si.TenantId && x.EmpreendimentoId == p.EmpreendimentoId && x.DataInicio > p.DataInicio).ToListAsync();
            lp.Add(p);
            if (lp.Any(x => x.Status != StatusPeriodo.EntradaDeDados))
            {
                r.Ok = false;
                r.ErrMsg = "Todos os períodos devem estar na fase de entrada de dados";
                r.Item = null;
                return r;
            }
            lp = lp.OrderBy(x => x.DataInicio).ToList();

            foreach (var p1 in lp)
            {
                if (p1.Status == StatusPeriodo.EntradaDeDados)
                {
                    try
                    {
                        r = await RatearLancamentosPlanoContaPeriodoAsync(p1.Id);
                        if (!r.Ok)
                        {
                            p1.ErrMsg = r.ErrMsg;
                            p1.Alerta = TipoAlerta.Erro;
                        }
                        else
                        {
                            p1.ErrMsg = string.Empty;
                            p1.Alerta = TipoAlerta.none;
                        }
                    }
                    catch (Exception e)
                    {
                        p1.ErrMsg = e.Message;
                        p1.Alerta = TipoAlerta.Erro;
                    }
                }
            }
            if (lp.Any(x => x.Alerta == TipoAlerta.Erro))
            {
                r.Ok = false;
                r.ErrMsg = "Existe(m) período(s) com erros nos lançamentos";
                r.Item = lp;
                _context.ChangeTracker.Clear();
                return r;
            }

            foreach (var p2 in lp)
            {
                r = await CreateNewAuditVersionAsync(p2);
                if (!r.Ok)
                {
                    p2.Alerta = TipoAlerta.Erro;
                    p2.ErrMsg = r.ErrMsg;
                    r.Item = lp;
                    _context.ChangeTracker.Clear();
                    return r;
                }
                List<PeridoAuditoriaVersaoView> lav = await GetPeridoAuditoriaStatusAsync(p2);
                SocioService socioService = new(_context, null, _sessionStorageService, _sessionService, _mapper);
                foreach (var av in lav)
                {
                    if (av.PeriodoSocioAuditoriaViews.Any(x => x.PeriodoSocioAuditoria.Status == StatusAuditoriaSocio.Pendente))
                    {
                        foreach (var pa in av.PeriodoSocioAuditoriaViews)
                        {
                            pa.PeriodoSocioAuditoria.Assinado = true;
                            pa.PeriodoSocioAuditoria.DataAssinatura = Constante.Now;
                            pa.PeriodoSocioAuditoria.NomeAssinado = si.UserName;
                            r = await socioService.AprovarDocumentoAsync(pa.PeriodoSocioAuditoria as ISFDocument);
                            if (!r.Ok)
                            {
                                pa.PeriodoSocioAuditoria.Assinado = false;
                                pa.PeriodoSocioAuditoria.DataAssinatura = DateTime.MinValue;
                                pa.PeriodoSocioAuditoria.NomeAssinado = string.Empty;
                            }
                        }
                    }
                    r = await SetPeriodoStatusAsync(p2, StatusPeriodo.Fechamento);
                    if (!r.Ok)
                    {
                        p2.Alerta = TipoAlerta.Erro;
                        p2.ErrMsg = r.ErrMsg;
                        r.Item = lp;
                        _context.ChangeTracker.Clear();
                        return r;
                    }
                    r = await ProcessaFechamentoPeriodoAsync(p2);
                    if (!r.Ok)
                    {
                        p2.Alerta = TipoAlerta.Erro;
                        p2.ErrMsg = r.ErrMsg;
                        r.Item = lp;
                        _context.ChangeTracker.Clear();
                        return r;
                    }
                }
            }
            r.Ok = true;
            return r;
        }
        public async Task<Resultado> UpdateLancamentoImportado(LanctoImportado lancto)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            LanctoImportado l = await _context.LanctoImportado.FirstOrDefaultAsync(x => x.Id == lancto.Id && x.TenantId == si.TenantId);
            if (l == null)
            {
                r.Ok = false;
                r.ErrMsg = "lançamentos não encontrado";
            }
            lancto.TenantId = si.TenantId;
            Mapper.Map(lancto, l);

            _context.LanctoImportado.Update(l);
            await _context.SaveChangesAsync();
            r.Ok = true;
            r.ErrMsg = string.Empty;
            return r;
        }
        public async Task<Resultado> RatearLancamentosPeriodoAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            List<LanctoEmpreendimento> lle = new();

            var ll1 = await (from LanctoEmpreendimento in _context.LanctoEmpreendimento
                             join PlanoConta in _context.PlanoConta on LanctoEmpreendimento.PlanoContaId equals PlanoConta.Id
                             select new { LanctoEmpreendimento, PlanoConta }
              ).Where(x => x.LanctoEmpreendimento.PeriodoId == id && x.LanctoEmpreendimento.TenantId == si.TenantId).ToListAsync();
            foreach (var l1 in ll1)
            {
                if (!l1.PlanoConta.AporteDistribuicao)
                {
                    lle.Add(l1.LanctoEmpreendimento);
                };
            }
            List<LancamentoEmpreendimentoRateado> llr = await RatearLancamentoAsync(lle);
            List<LanctoEmpRateadoPlanoContaSocio> lcr = new();
            foreach (var illr in llr)
            {
                foreach (var ilrs in illr.RateioSocios)
                {
                    LanctoEmpRateadoPlanoContaSocio cs = new()
                    {
                        PlanoContaId = illr.LanctoEmpreendimento.PlanoContaId,
                        EmpreendimentoSocioId = ilrs.EmpreendimentoSocioId,
                        GrupoRateioId = ilrs.GrupoRateioId,
                        GrupoSocioId = ilrs.GrupoSocioId,
                        Percentual = ilrs.Percentual,
                        PeriodoId = illr.LanctoEmpreendimento.PeriodoId,
                        ProgramacaoGrupoRateioId = ilrs.ProgramacaoGrupoRateioId,
                        TenantId = si.TenantId,
                        Valor = ilrs.Valor
                    };
                    lcr.Add(cs);
                }
            }

            List<LanctoEmpRateadoPlanoContaSocio> nlcr =
                lcr.GroupBy(x => new
                {
                    x.PeriodoId,
                    x.PlanoContaId,
                    x.EmpreendimentoSocioId,
                    x.GrupoRateioId,
                    x.GrupoSocioId,
                    x.ProgramacaoGrupoRateioId,
                    x.TenantId
                },
                (Key, Lista) => new
                {
                    Key,
                    Valor = Lista.Sum(x => x.Valor),
                    Percentual = Lista.Max(x => x.Percentual)
                }).Select(y => new LanctoEmpRateadoPlanoContaSocio()
                {
                    Id = 0,
                    PeriodoId = y.Key.PeriodoId,
                    PlanoContaId = y.Key.PlanoContaId,
                    EmpreendimentoSocioId = y.Key.EmpreendimentoSocioId,
                    GrupoRateioId = y.Key.GrupoRateioId,
                    GrupoSocioId = y.Key.GrupoSocioId,
                    ProgramacaoGrupoRateioId = y.Key.ProgramacaoGrupoRateioId,
                    TenantId = si.TenantId,
                    Valor = y.Valor,
                    Percentual = y.Percentual
                }).ToList();

            await _context.Database.BeginTransactionAsync();
            try
            {
                /*
                var lrdel =
                await (from LanctoEmpreendimentoRateadoSocio in _context.LanctoEmpreendimentoRateadoSocio
                       join LanctoEmpreendimento in _context.LanctoEmpreendimento on LanctoEmpreendimentoRateadoSocio.LanctoEmpreendimentoId equals LanctoEmpreendimento.Id
                       select new
                       {
                           LanctoEmpreendimentoRateadoSocio,
                           LanctoEmpreendimentoRateadoSocio.TenantId,
                           LanctoEmpreendimento.PeriodoId
                       }
                             ).Where(x => x.PeriodoId == id && x.TenantId == si.TenantId).ToListAsync();

                foreach (var ir in lrdel)
                {
                    _context.LanctoEmpreendimentoRateadoSocio.Remove(ir.LanctoEmpreendimentoRateadoSocio);
                }
                */
                foreach (var le in llr)
                {
                    le.LanctoEmpreendimento.LanctoEmpRelacionamento = null;
                    le.LanctoEmpreendimento.LanctoEmpUnidade = null;
                    _context.LanctoEmpreendimento.Update(le.LanctoEmpreendimento);

                    /*  Inibido a gravação do rateio por lancamento po socio
                     *  Ativar para verificar bugs
                    foreach (var lr in le.RateioSocios)
                    {
                        lr.LanctoEmpreendimentoId = le.LanctoEmpreendimento.Id;
                        lr.Id = 0;
                        _context.LanctoEmpreendimentoRateadoSocio.Add(lr);
                    }
                    */
                }

                List<LanctoEmpRateadoPlanoContaSocio> lrs1 = _context.LanctoEmpRateadoPlanoContaSocio.Where(x => x.PeriodoId == id && x.TenantId == si.TenantId).ToList();
                foreach (var ics in lrs1)
                {
                    _context.LanctoEmpRateadoPlanoContaSocio.Remove(ics);
                }
                foreach (var nics in nlcr)
                {
                    _context.LanctoEmpRateadoPlanoContaSocio.Add(nics);
                }

                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
                if (llr.Any(x => x.LanctoEmpreendimento.Rateado == false))
                {
                    r.Ok = false;
                    r.ErrMsg = "Existem lançamentos com erros e não podem ser rateados";
                }
                else
                {
                    r.Ok = true;
                }

            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }

            return r;
        }
        public async Task<Resultado> RatearLancamentosPlanoContaPeriodoAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            List<LanctoEmpreendimento> lle = new();

            int empid = await (from Periodo in _context.Periodo
                               select new { Periodo.EmpreendimentoId, Periodo.Id, Periodo.TenantId }).Where(x => x.Id == id && x.TenantId == si.TenantId).Select(x => x.EmpreendimentoId).FirstOrDefaultAsync();

            List<ContaCorrenteRegraRateio> regraCC = await _context.ContaCorrenteRegraRateio.Where(x => x.TenantId == si.TenantId && x.EmpreendimentoId == empid).ToListAsync();
            List<GrupoRateio> grupoRateio = await _context.GrupoRateio.Where(x => x.TenantId == si.TenantId && x.EmpreendimentoId == empid).ToListAsync();

            var ll1 = await (from LanctoEmpreendimento in _context.LanctoEmpreendimento
                             join PlanoConta in _context.PlanoConta on LanctoEmpreendimento.PlanoContaId equals PlanoConta.Id
                             join MovtoBanco in _context.MovtoBanco on LanctoEmpreendimento.OrigemId equals MovtoBanco.Id into movto
                             from cc in movto.DefaultIfEmpty()
                             select new { LanctoEmpreendimento, PlanoConta, ContaCorrenteId = (int?)cc.ContaCorrenteId ?? 0 }
              ).Where(x => x.LanctoEmpreendimento.PeriodoId == id && x.LanctoEmpreendimento.TenantId == si.TenantId).ToListAsync();
            foreach (var l1 in ll1)
            {
                if (!l1.PlanoConta.AporteDistribuicao)
                {
                    l1.LanctoEmpreendimento.Rateado = false;
                    l1.LanctoEmpreendimento.ErroMsg = string.Empty;
                    l1.LanctoEmpreendimento.Erro = false;
                    l1.LanctoEmpreendimento.ContaCorrenteId = (int)l1.ContaCorrenteId;
                    lle.Add(l1.LanctoEmpreendimento);
                    if (l1.ContaCorrenteId != 0)
                    {
                        var rcc = regraCC.FirstOrDefault(x => x.ContaCorrenteId == l1.ContaCorrenteId && x.PlanoContaId == l1.LanctoEmpreendimento.PlanoContaId);
                        if (rcc != null)
                        {
                            var gr = grupoRateio.FirstOrDefault(x => x.RegraRateioDefaultId == rcc.RegraRateioId);
                            if (gr != null)
                            {
                                l1.LanctoEmpreendimento.GrupoRateioId = gr.Id;
                            }
                            else
                            {
                                l1.LanctoEmpreendimento.GrupoRateioId = 0;
                            }
                        }
                        else
                        {
                            rcc = regraCC.FirstOrDefault(x => x.ContaCorrenteId == l1.ContaCorrenteId && x.PlanoGerencialId == l1.PlanoConta.PlanoGerencialId);
                            if (rcc != null)
                            {
                                var gr = grupoRateio.FirstOrDefault(x => x.RegraRateioDefaultId == rcc.RegraRateioId);
                                if (gr != null)
                                {
                                    l1.LanctoEmpreendimento.GrupoRateioId = gr.Id;
                                }
                                else
                                {
                                    l1.LanctoEmpreendimento.GrupoRateioId = 0;
                                }
                            }
                            else
                            {
                                l1.LanctoEmpreendimento.GrupoRateioId = 0;
                            }
                        }
                    }
                };
            }
            List<LancamentoEmpreendimentoPlanoContaRateado> llr = await RatearLancamentoPlanoContaAsync(lle);
            List<LanctoEmpRateadoPlanoContaSocio> lcr = new();
            foreach (var illr in llr)
            {
                foreach (var ilrs in illr.RateioPlanoContaSocios)
                {
                    LanctoEmpRateadoPlanoContaSocio cs = new()
                    {
                        PlanoContaId = illr.LanctoEmpreendimento.PlanoContaId,
                        EmpreendimentoSocioId = ilrs.EmpreendimentoSocioId,
                        GrupoRateioId = ilrs.GrupoRateioId,
                        GrupoSocioId = ilrs.GrupoSocioId,
                        Percentual = ilrs.Percentual,
                        PeriodoId = illr.LanctoEmpreendimento.PeriodoId,
                        ProgramacaoGrupoRateioId = ilrs.ProgramacaoGrupoRateioId,
                        TenantId = si.TenantId,
                        Valor = ilrs.Valor
                    };
                    lcr.Add(cs);
                }
            }

            await _context.Database.BeginTransactionAsync();
            try
            {
                List<LanctoEmpRateadoPlanoContaSocio> lrs1 = _context.LanctoEmpRateadoPlanoContaSocio.Where(x => x.PeriodoId == id && x.TenantId == si.TenantId).ToList();
                if (lrs1.Count > 0)
                {
                    _context.LanctoEmpRateadoPlanoContaSocio.RemoveRange(lrs1);
                }
                _context.LanctoEmpreendimento.UpdateRange(lle);
                if (lle.Any(x => x.Erro == true))
                {
                    r.Ok = false;
                    r.ErrMsg = "Existem lançamentos com erros e não podem ser rateados";
                }
                else
                {
                    if (lcr.Count > 0)
                    {
                        await _context.LanctoEmpRateadoPlanoContaSocio.AddRangeAsync(lcr);
                    }
                    r.Ok = true;
                    r.ErrMsg = string.Empty;
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                return r;
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<List<LancamentoEmpreendimentoRateado>> RatearLancamentoAsync(List<LanctoEmpreendimento> ll)
        {
            SessionInfo si = await GetSessionAsync();

            List<LancamentoEmpreendimentoRateado> r = new();
            List<PlanoContaRegraRateio> lpc = new();
            List<ProgramacaoGrupoRegraTemp> lps = new();

            if (ll.Count == 0)
            {
                return r;
            }
            Periodo p = await _context.Periodo.FirstOrDefaultAsync(x => x.Id == ll[0].PeriodoId && x.TenantId == si.TenantId);
            if (p == null)
            {
                throw new Exception("Lançamento sem período");
            }
            List<PlanoConta> lcontas = await _context.PlanoConta.Where(x => x.TenantId == si.TenantId && x.EmpreendimentoId == p.EmpreendimentoId).ToListAsync();



            foreach (LanctoEmpreendimento l in ll)
            {
                var conta = lcontas.FirstOrDefault(x => x.Id == l.PlanoContaId);
                if (conta == null)
                {
                    l.Erro = true;
                    l.ErroMsg = "Conta não cadastrada";
                    l.Rateado = false;
                }
                l.Erro = false;
                l.ErroMsg = string.Empty;
                l.Rateado = false;
                if (conta.AporteDistribuicao || !conta.Ratear)
                {
                    continue;
                }
                PlanoContaRegraRateio pc = lpc.FirstOrDefault(x => x.PlanoContaId == l.PlanoContaId && x.DataInicio <= l.DataCompetencia && x.DataFim >= l.DataCompetencia);
                if (pc == null)
                {
                    pc = await _regrasRateioService.GetProgramacaoGrupoRateioByPlanoContaIdAsync(conta, l.DataCompetencia);
                    if (pc != null)
                    {
                        lpc.Add(pc);
                    }
                }
                if (pc == null)
                {
                    l.Rateado = false;
                    l.Erro = true;
                    l.ErroMsg = "Regra para rateio não encontrada para esse PlanoConta! Verificar a validade da regra.";
                    LancamentoEmpreendimentoRateado rl1 = new()
                    {
                        LanctoEmpreendimento = l
                    };
                    r.Add(rl1);
                    continue;
                }

                ProgramacaoGrupoRegraTemp ps = lps.FirstOrDefault(x => x.ProgramacaoGrupoRateioId == pc.ProgramacaoGrupoRateioId && x.DataInicio <= l.DataCompetencia && x.DataFim >= l.DataCompetencia);
                if (ps == null)
                {
                    ps = await _regrasRateioService.GetProgramacaoGrupoRateioSociosByIdAsync(pc.ProgramacaoGrupoRateioId, l.DataCompetencia);
                    if (ps != null)
                    {
                        lps.Add(ps);
                    }
                }
                if (ps == null)
                {
                    l.Rateado = false;
                    l.Erro = true;
                    l.ErroMsg = "Regra para rateio não encontradao para está PlanoConta , Verificar a validade da regra, programação.!";

                    LancamentoEmpreendimentoRateado rl2 = new()
                    {
                        LanctoEmpreendimento = l
                    };
                    r.Add(rl2);
                    continue;
                }

                lps.Add(ps);

                LancamentoEmpreendimentoRateado rl = new()
                {
                    LanctoEmpreendimento = l
                };

                string delerro = string.Empty;

                foreach (ProgramacaoGrupoRateioSociosDetalhe psd in ps.Detalhes)
                {
                    LanctoEmpreendimentoRateadoSocio er = new()
                    {
                        TenantId = si.TenantId,
                        LanctoEmpreendimentoId = l.Id,
                        GrupoRateioId = psd.GrupoRateioId,
                        GrupoSocioId = psd.GrupoSocioId,
                        ProgramacaoGrupoRateioId = ps.ProgramacaoGrupoRateioId,
                        EmpreendimentoSocioId = psd.EmpreendimentoSocioId,
                        Percentual = psd.PercentualRateio,
                        Valor = Math.Round(l.Valor * psd.PercentualRateio, 2)
                    };
                    if (delerro == string.Empty)
                    {
                        delerro = "Regra: " + psd.NomeRegra + ". Sócios: ";
                    }
                    delerro += psd.NomeSocio + " = " + (psd.PercentualRateio * 100).ToString("N6", Constante.Culture) + "%  ";
                    if ((l.Tipo == TipoLancto.Despesa && conta.Tipo == TipoPlanoConta.Receita) ||
                        (l.Tipo == TipoLancto.Receita && conta.Tipo == TipoPlanoConta.Despesa))
                    {
                        er.Valor *= -1;
                    }
                    rl.RateioSocios.Add(er);
                }
                if (Math.Abs(rl.RateioSocios.Sum(x => x.Percentual) - 1) > 0.001 ||
                    Math.Abs(rl.RateioSocios.Sum(x => Math.Abs(x.Valor)) - rl.LanctoEmpreendimento.Valor) > 0.1)
                {
                    l.Erro = true;
                    l.ErroMsg = "Regra de rateio com erro, rateio parcial. " + delerro;
                    l.Rateado = false;
                }
                else
                {
                    l.Rateado = (rl.RateioSocios.Count > 0);

                    if (rl.RateioSocios.Count > 0)
                    {
                        double vt = rl.RateioSocios.Sum(x => Math.Abs(x.Valor));
                        double vt1 = l.Valor - vt;
                        rl.RateioSocios[0].Valor += vt1;
                    }
                }
                r.Add(rl);
            }
            return r;
        }
        public async Task<List<LancamentoEmpreendimentoPlanoContaRateado>> RatearLancamentoPlanoContaAsync(List<LanctoEmpreendimento> ll)
        {
            SessionInfo si = await GetSessionAsync();

            List<LancamentoEmpreendimentoPlanoContaRateado> r = new();
            List<PlanoContaRegraRateio> lpc = new();
            List<ProgramacaoGrupoRegraTemp> lps = new();

            if (ll.Count == 0)
            {
                return r;
            }
            Periodo p = await _context.Periodo.FirstOrDefaultAsync(x => x.Id == ll[0].PeriodoId && x.TenantId == si.TenantId);
            if (p == null)
            {
                throw new Exception("Lançamento sem período");
            }
            List<PlanoConta> lcontas = await _context.PlanoConta.Where(x => x.TenantId == si.TenantId && x.EmpreendimentoId == p.EmpreendimentoId).ToListAsync();

            List<LanctoEmpreendimento> llPcota = new();
            foreach (LanctoEmpreendimento lo in ll)
            {
                var conta = lcontas.FirstOrDefault(x => x.Id == lo.PlanoContaId);
                if (conta == null)
                {
                    lo.Erro = true;
                    lo.ErroMsg = "Conta não cadastrada";
                    lo.Rateado = false;
                    continue;
                }
                lo.Erro = false;
                lo.ErroMsg = string.Empty;
                lo.Rateado = false;
                if (conta.AporteDistribuicao || !conta.Ratear || (conta.Tipo != TipoPlanoConta.Receita && conta.Tipo != TipoPlanoConta.Despesa))
                {
                    continue;
                }
                LanctoEmpreendimento nl = llPcota.FirstOrDefault(x => x.PlanoContaId == lo.PlanoContaId && x.GrupoRateioId == lo.GrupoRateioId);
                if (nl == null)
                {
                    nl = new()
                    {
                        PeriodoId = lo.PeriodoId,
                        PlanoContaId = lo.PlanoContaId,
                        DataCompetencia = p.DataInicio,
                        GrupoRateioId = lo.GrupoRateioId
                    };
                    llPcota.Add(nl);
                }
                if ((lo.Tipo == TipoLancto.Despesa && conta.Tipo == TipoPlanoConta.Receita) ||
                    (lo.Tipo == TipoLancto.Receita && conta.Tipo == TipoPlanoConta.Despesa))
                {
                    nl.Valor += (lo.Valor * -1);
                }
                else
                {
                    nl.Valor += lo.Valor;
                }
            }

            foreach (LanctoEmpreendimento l in llPcota)
            {
                var conta = lcontas.FirstOrDefault(x => x.Id == l.PlanoContaId);
                PlanoContaRegraRateio pc = lpc.FirstOrDefault(x => x.PlanoContaId == l.PlanoContaId && x.DataInicio <= l.DataCompetencia && x.DataFim >= l.DataCompetencia && x.GrupoRateioId == l.GrupoRateioId);
                if (pc == null)
                {
                    pc = await _regrasRateioService.GetProgramacaoGrupoRateioByPlanoContaIdAsync(conta, l.DataCompetencia, l.GrupoRateioId);
                    if (pc != null)
                    {
                        lpc.Add(pc);
                    }
                }
                if (pc == null)
                {
                    l.Rateado = false;
                    l.Erro = true;
                    l.ErroMsg = "Regra para rateio não encontrada para esse PlanoConta! Verificar a validade da regra.";
                    continue;
                }
                ProgramacaoGrupoRegraTemp ps = lps.FirstOrDefault(x => x.ProgramacaoGrupoRateioId == pc.ProgramacaoGrupoRateioId && x.DataInicio <= l.DataCompetencia && x.DataFim >= l.DataCompetencia);
                if (ps == null)
                {
                    ps = await _regrasRateioService.GetProgramacaoGrupoRateioSociosByIdAsync(pc.ProgramacaoGrupoRateioId, l.DataCompetencia);
                    if (ps != null)
                    {
                        lps.Add(ps);
                    }
                }
                if (ps == null)
                {
                    l.Rateado = false;
                    l.Erro = true;
                    l.ErroMsg = "Regra para rateio não encontradao para este plano de conta , Verificar a validade da regra, programação.!";
                    continue;
                }
                lps.Add(ps);

                LancamentoEmpreendimentoPlanoContaRateado rl = new()
                {
                    LanctoEmpreendimento = l
                };

                string delerro = string.Empty;

                foreach (ProgramacaoGrupoRateioSociosDetalhe psd in ps.Detalhes)
                {
                    LanctoEmpRateadoPlanoContaSocio er = new()
                    {
                        TenantId = si.TenantId,
                        PeriodoId = l.PeriodoId,
                        PlanoContaId = l.PlanoContaId,
                        GrupoRateioId = psd.GrupoRateioId,
                        GrupoSocioId = psd.GrupoSocioId,
                        ProgramacaoGrupoRateioId = ps.ProgramacaoGrupoRateioId,
                        EmpreendimentoSocioId = psd.EmpreendimentoSocioId,
                        Percentual = psd.PercentualRateio,
                        Valor = Math.Round(l.Valor * psd.PercentualRateio, 2)
                    };
                    if (delerro == string.Empty)
                    {
                        delerro = "Regra: " + psd.NomeRegra + ". Sócios: ";
                    }
                    delerro += psd.NomeSocio + " = " + (psd.PercentualRateio * 100).ToString("N6", Constante.Culture) + "%  ";
                    if ((l.Tipo == TipoLancto.Despesa && conta.Tipo == TipoPlanoConta.Receita) ||
                        (l.Tipo == TipoLancto.Receita && conta.Tipo == TipoPlanoConta.Despesa))
                    {
                        er.Valor *= -1;
                    }
                    rl.RateioPlanoContaSocios.Add(er);
                }
                if (Math.Abs(rl.RateioPlanoContaSocios.Sum(x => x.Percentual) - 1) > 0.001 ||
                    Math.Abs(rl.RateioPlanoContaSocios.Sum(x => x.Valor) - rl.LanctoEmpreendimento.Valor) > 0.1)
                {
                    l.Erro = true;
                    l.ErroMsg = "Regra de rateio com erro, rateio parcial. " + delerro;
                    l.Rateado = false;
                }
                else
                {
                    l.Rateado = (rl.RateioPlanoContaSocios.Count > 0);
                    if (rl.RateioPlanoContaSocios.Count > 0)
                    {
                        double vt = rl.RateioPlanoContaSocios.Sum(x => x.Valor);
                        double vt1 = l.Valor - vt;
                        vt1 /= rl.RateioPlanoContaSocios.Count;
                        foreach (var xx in rl.RateioPlanoContaSocios)
                        {
                            xx.Valor += vt1;
                            xx.Valor = Math.Round(xx.Valor, 2);
                        }
                    }
                    if (rl.RateioPlanoContaSocios.Count > 0)
                    {
                        double vt = rl.RateioPlanoContaSocios.Sum(x => x.Valor);
                        double vt1 = l.Valor - vt;
                        if (Math.Abs(vt1) > 0.015)
                        {
                            double vt2 = vt1 / 2;
                            rl.RateioPlanoContaSocios[0].Valor += vt2;
                            rl.RateioPlanoContaSocios[1].Valor += vt2;
                            rl.RateioPlanoContaSocios[0].Valor = Math.Round(rl.RateioPlanoContaSocios[0].Valor, 2);
                            rl.RateioPlanoContaSocios[1].Valor = Math.Round(rl.RateioPlanoContaSocios[1].Valor, 2);
                        }
                        else
                        {
                            rl.RateioPlanoContaSocios[0].Valor += vt1;
                            rl.RateioPlanoContaSocios[0].Valor = Math.Round(rl.RateioPlanoContaSocios[0].Valor, 2);
                        }
                        vt = rl.RateioPlanoContaSocios.Sum(x => x.Valor);
                        vt1 = l.Valor - vt;
                        rl.RateioPlanoContaSocios[0].Valor += vt1;
                        rl.RateioPlanoContaSocios[0].Valor = Math.Round(rl.RateioPlanoContaSocios[0].Valor, 2);
                    }
                }
                r.Add(rl);
            }
            foreach (var xplano in llPcota)
            {
                ll.Where(x => x.PlanoContaId == xplano.PlanoContaId).ToList().ForEach(x =>
                   {
                       x.Erro = xplano.Erro;
                       x.Rateado = xplano.Rateado;
                       x.ErroMsg = xplano.ErroMsg;
                   });
            }
            return r;
        }
        public async Task<List<LanctoLoteImportacao>> IncluirLancamentoInmportadoAsync(Empreendimento emp, string nomearquivo, List<LanctoImportado> lu, Func<int, int> callback, bool savelacnto = true)
        {
            SessionInfo si = await GetSessionAsync();
            List<Periodo> lp = new();
            List<LanctoLoteImportacao> ll = new();
            List<PlanoConta> lpc = await _context.PlanoConta.Where(x => x.EmpreendimentoId == emp.Id && x.TenantId == si.TenantId).OrderBy(x => x.Nome).ToListAsync();
            List<PlanoGerencial> lpg = await _context.PlanoGerencial.Where(x => x.TenantId == si.TenantId).OrderBy(x => x.TipoPlanoConta).ThenBy(x => x.Nome).ToListAsync();
            RelacionamentoService rs = new(_context, null, null, _sessionStorageService, _sessionService, _mapper);

            _context.ChangeTracker.Clear();

            string masterLoteGuid = Guid.NewGuid().ToString();
            try
            {
                await _context.Database.BeginTransactionAsync();
                int i = 0;
                int j = -1;
                foreach (LanctoImportado l in lu)
                {
                    l.TenantId = si.TenantId;
                    Periodo pn = lp.FirstOrDefault(x => x.DataInicio <= l.DataMovto && x.DataFim >= l.DataMovto && x.TenantId == si.TenantId);
                    if (pn == null)
                    {
                        pn = await FindPeriodoByDataAsync(emp.Id, l.DataMovto);
                        if (pn == null)
                        {
                            pn = await NovoPeriodoDataAsync(emp.Id, l.DataMovto);
                            lp.Add(pn);
                        }
                        else
                        {
                            lp.Add(pn);
                        }
                    }
                    else
                    {
                        pn.Alerta = TipoAlerta.Alterado;
                        _context.Periodo.Update(pn);
                    }
                    if (pn.Status != StatusPeriodo.EntradaDeDados)
                    {
                        throw new Exception("Período " + pn.Nome + " deve estar na fase de entrada de dados para inclusão de lançamentos");

                    }
                    LanctoLoteImportacao nlote = ll.FirstOrDefault(x => x.PeriodoId == pn.Id && x.IdExterno == nomearquivo && x.TenantId == si.TenantId);
                    if (nlote == null)
                    {
                        nlote = await _context.LanctoLoteImportacao.FirstOrDefaultAsync(x => x.PeriodoId == pn.Id && x.Status == StatusLote.Pendente && x.IdExterno == nomearquivo && x.TenantId == si.TenantId);
                        if (nlote != null)
                        {
                            ll.Add(nlote);
                        }
                    }
                    if (nlote == null)
                    {
                        nlote = new()
                        {
                            DataImportacao = Constante.Now,
                            IdExterno = nomearquivo,
                            Origem = nomearquivo,
                            PeriodoId = pn.Id,
                            TenantId = si.TenantId,
                            Status = StatusLote.Processado,
                            DataProcessamento = Constante.Now,
                            MasterLoteGuid = masterLoteGuid
                        };
                        await _context.LanctoLoteImportacao.AddAsync(nlote);
                        await _context.SaveChangesAsync();
                        ll.Add(nlote);
                    }
                    l.LanctoLoteImportacaoId = nlote.Id;
                    l.PeriodoId = nlote.PeriodoId;

                    if (l.RelacionamentoId == 0 && l.RelacionamentoV != string.Empty)
                    {
                        Relacionamento rela = await _context.Relacionamento.FirstOrDefaultAsync(x => x.Nome == l.RelacionamentoV && x.TenantId == si.TenantId);
                        if (rela == null)
                        {
                            rela = new Relacionamento()
                            {
                                Nome = l.RelacionamentoV,
                                CPFCNPJ = string.Empty,
                                CodigoExternoCliente = l.RelacionamentoCodigoExternoV,
                                CodigoExternoFornecedor = l.RelacionamentoCodigoExternoV
                            };
                            rela = await rs.InsertIncompletoAsync(rela, new List<Contato>());
                        }
                        l.RelacionamentoId = rela.Id;
                    }
                    PlanoGerencial pg = null;
                    if (l.PlanoGerencialId == 0)
                    {
                        pg = lpg.FirstOrDefault(x => x.CodigoExterno.ToLower() == l.PlanoGerencialCodigoExternoV.ToLower() && x.CodigoExterno != string.Empty);
                        if (pg == null)
                        {
                            pg = lpg.FirstOrDefault(x => x.Nome.ToLower() == l.PlanoGerencialV.ToLower());
                        }
                        if (pg == null)
                        {
                            pg = new PlanoGerencial()
                            {
                                TenantId = si.TenantId,
                                AporteDistribuicao = false,
                                Nome = l.PlanoGerencialV,
                                CodigoExterno = l.PlanoGerencialCodigoExternoV,
                                NomeGrupoContaAuxiliar = string.Empty
                            };
                            if (l.Tipo == TipoLancto.Receita)
                            {
                                pg.TipoPlanoConta = TipoPlanoConta.Receita;
                            }
                            else
                            {
                                if (l.Tipo == TipoLancto.Despesa)
                                {
                                    pg.TipoPlanoConta = TipoPlanoConta.Despesa;
                                }
                                else
                                {
                                    throw new Exception("Não implementado tipo lançamento dif receita e despesa");
                                }
                            }
                            await _context.PlanoGerencial.AddAsync(pg);
                            await _context.SaveChangesAsync();
                            lpg.Add(pg);
                        }
                    }
                    else
                    {
                        pg = lpg.FirstOrDefault(x => x.Id == l.PlanoGerencialId);
                    }
                    PlanoConta pc = null;
                    if (l.PlanoContaId == 0)
                    {
                        pc = lpc.FirstOrDefault(x => x.CodigoExterno.ToLower() == l.PlanoContaCodigoExternoV.ToLower() && x.CodigoExterno != string.Empty);
                        if (pc == null)
                        {
                            pc = new PlanoConta()
                            {
                                TenantId = si.TenantId,
                                AporteDistribuicao = false,
                                Nome = l.PlanoContaV,
                                EmpreendimentoId = emp.Id,
                                Ratear = true,
                                CodigoExterno = l.PlanoContaCodigoExternoV
                            };
                            if (l.Tipo == TipoLancto.Receita)
                            {
                                pc.Tipo = TipoPlanoConta.Receita;
                            }
                            else
                            {
                                if (l.Tipo == TipoLancto.Despesa)
                                {
                                    pc.Tipo = TipoPlanoConta.Despesa;
                                }
                                else
                                {
                                    throw new Exception("Não implementado tipo lançamento dif receita e despesa");
                                }
                            }
                            if (pg == null)
                            {
                                throw new Exception(" Plano de conta sem conta gerencial " + l.PlanoContaV);
                            }
                            pc.PlanoGerencialId = pg.Id;
                            if (pc.NomeCurto == null || pc.NomeCurto == string.Empty)
                            {
                                pc.NomeCurto = pc.Nome;
                            }
                            await _context.PlanoConta.AddAsync(pc);
                            await _context.SaveChangesAsync();
                            lpc.Add(pc);
                        };
                        l.PlanoContaId = pc.Id;
                    }

                    if (savelacnto)
                    {
                        await _context.LanctoImportado.AddAsync(l);
                    }

                    if (callback != null)
                    {
                        i++;
                        double v0 = i;
                        double v00 = lu.Count;
                        double v1 = v0 / v00 * 100;
                        int v2 = Convert.ToInt32(Math.Round(v1, 0));
                        if (v2 > j)
                        {
                            j = v2 + 45;
                            callback(v2);
                        }
                    }

                }
                callback(888);

                await _context.SaveChangesAsync();


                callback(999);
                i = 0;
                j = -1;

                if (savelacnto)
                {

                    foreach (LanctoLoteImportacao loteimp in ll)
                    {
                        var r = await ProcessarLote(loteimp);
                        if (!r.Ok)
                        {
                            loteimp.Status = StatusLote.Erro;
                            _context.LanctoLoteImportacao.Update(loteimp);
                            await _context.SaveChangesAsync();
                        }
                        i++;
                        double v0 = i;
                        double v00 = ll.Count;
                        double v1 = v0 / v00 * 100;
                        int v2 = Convert.ToInt32(Math.Round(v1, 0));
                        if (v2 > j)
                        {
                            j = v2 + 5;
                            callback(v2);
                        }
                    }
                }
                else
                {
                    await ProcessarLanctoImportados(ll, lu, callback);
                }
                await _context.Database.CommitTransactionAsync();
                return ll;
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
        public async Task<List<LanctoLoteImportacao>> FindAllLotesByPeriodoIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();

            var l = await (from LanctoLoteImportacao in _context.LanctoLoteImportacao
                           join Periodo in _context.Periodo on LanctoLoteImportacao.PeriodoId equals Periodo.Id
                           select new { LanctoLoteImportacao, Periodo }).Where(x => x.LanctoLoteImportacao.PeriodoId == id && x.LanctoLoteImportacao.TenantId == si.TenantId).ToListAsync();
            List<LanctoLoteImportacao> r = new();
            foreach (var t in l)
            {
                t.LanctoLoteImportacao.Periodo = t.Periodo;
                r.Add(t.LanctoLoteImportacao);
            }

            return r.OrderByDescending(x => x.DataImportacao).ToList();
        }
        public async Task<Resultado> ExluirLoteAsync(int id, bool todos = false)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            await _context.Database.BeginTransactionAsync();
            LanctoLoteImportacao ldel = await _context.LanctoLoteImportacao.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
            try
            {
                if (ldel == null)
                {
                    r.Ok = false;
                    r.ErrMsg = "Lote não encontrado.";
                    await _context.Database.RollbackTransactionAsync();
                    return r;
                }
                List<LanctoLoteImportacao> llotes = new();
                if (todos && ldel.MasterLoteGuid != null && ldel.MasterLoteGuid != string.Empty)
                {
                    llotes = await _context.LanctoLoteImportacao.Where(x => x.Id != ldel.Id && x.TenantId == si.TenantId &&
                                                     x.MasterLoteGuid == ldel.MasterLoteGuid && x.MasterLoteGuid != string.Empty && x.MasterLoteGuid != null).ToListAsync();
                }
                llotes.Add(ldel);

                foreach (var l in llotes)
                {
                    Periodo p = await _context.Periodo.FirstOrDefaultAsync(x => x.Id == l.PeriodoId && x.TenantId == si.TenantId);
                    if (p.Status != StatusPeriodo.EntradaDeDados)
                    {
                        r.Ok = false;
                        r.ErrMsg = "Exclusão permitida somente para período na fase entrada de dados. Período " + p.Nome + " -> " + p.NomeStatus;
                        await _context.Database.RollbackTransactionAsync();
                        return r;
                    }
                    var li = await _context.LanctoImportado.Where(x => x.LanctoLoteImportacaoId == id && x.TenantId == si.TenantId).ToListAsync();
                    if (li.Count > 0)
                    {
                        foreach (var item in li)
                        {
                            var leli = await _context.LanctoEmpreendimentoLanctoImp.FirstOrDefaultAsync(x => x.LanctoImportadoId == item.Id);
                            if (leli != null)
                            {
                                var lanc = await _context.LanctoEmpreendimento.FirstOrDefaultAsync(x => x.Id == leli.LanctoEmpreendimentoId && x.TenantId == si.TenantId);
                                _context.LanctoEmpreendimentoLanctoImp.Remove(leli);
                                await _context.SaveChangesAsync();
                                if (lanc != null)
                                {
                                    _context.LanctoEmpreendimento.Remove(lanc);
                                }
                            }
                            _context.LanctoImportado.Remove(item);
                        }
                    }
                    var lle = await _context.LanctoEmpreendimento.Where(x => x.OrigemId == l.Id && x.Origem == OrigemLancto.ImportadoLote && x.TenantId == si.TenantId).ToListAsync();
                    if (lle.Count > 0)
                    {
                        p.Alerta = TipoAlerta.Alterado;
                        _context.Periodo.Update(p);
                    }
                    var lleid = lle.Select(x => x.Id).ToArray();
                    var ller = await _context.LanctoEmpRelacionamento.Where(x => lleid.Contains(x.LanctoEmpreendimentoId)).ToListAsync();
                    _context.LanctoEmpRelacionamento.RemoveRange(ller);
                    await _context.SaveChangesAsync();
                    _context.LanctoEmpreendimento.RemoveRange(lle);
                    await _context.SaveChangesAsync();
                }
                _context.LanctoLoteImportacao.RemoveRange(llotes);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                r.Ok = true;
                _context.ChangeTracker.Clear();
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<List<LanctoImportado>> FindAllLanctoIMportadoByLoteIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.LanctoImportado.Where(x => x.LanctoLoteImportacaoId == id && x.TenantId == si.TenantId).OrderBy(x => x.DataMovto).AsNoTracking().ToListAsync();
        }
        public async Task<Resultado> ProcessarLoteByLoteId(int id)
        {
            SessionInfo si = await GetSessionAsync();
            LanctoLoteImportacao lote = await _context.LanctoLoteImportacao.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
            return await ProcessarLote(lote);

        }
        public async Task<Resultado> ProcessarLanctoImportados(List<LanctoLoteImportacao> lotes, List<LanctoImportado> lanctos, Func<int, int> callback)
        {
            SessionInfo si = await GetSessionAsync();

            Resultado r = new();

            bool mytrans = (_context.Database.CurrentTransaction == null);

            if (lanctos.Any(x => x.Erro))
            {
                r.Ok = false;
                r.ErrMsg = "Exitem lançamentos com erros, lote não foi processado";
                if (!mytrans)
                {
                    throw new Exception("Exitem lançamentos com erros, lote não foi processado");
                }
                return r;
            }

            if (lanctos.Any(x => x.PlanoContaId == 0))
            {
                r.Ok = false;
                r.ErrMsg = "Exitem lançamentos com erros, lote não foi processado";
                if (!mytrans)
                {
                    throw new Exception("Exitem lançamentos plano de conta não informado;");
                }
                return r;
            }

            if (mytrans)
            {
                await _context.Database.BeginTransactionAsync();
            }
            try
            {
                LanctoLoteImportacao lti = null;
                int j = -1;
                int i = 0;
                List<LanctoEmpreendimento> lle = new();
                foreach (var item in lanctos)
                {
                    int loteid = 0;
                    if (lti == null || lti.Id != item.PeriodoId)
                    {
                        lti = lotes.FirstOrDefault(x => x.PeriodoId == item.PeriodoId);
                    }
                    if (lti != null)
                    {
                        loteid = lti.Id;
                    }
                    LanctoEmpreendimento le = new()
                    {
                        PlanoContaId = item.PlanoContaId,
                        CodigoExterno = item.CodigoExterno,
                        DataCompetencia = item.DataCompetencia,
                        DataMovto = item.DataMovto,
                        Descricao = item.Descricao,
                        Documento = item.Documento,
                        Origem = OrigemLancto.ImportadoLote,
                        PeriodoId = item.PeriodoId,
                        Rateado = false,
                        RelacionamentoId = item.RelacionamentoId,
                        TenantId = si.TenantId,
                        Tipo = item.Tipo,
                        Valor = item.Valor,
                        UnidadeEmpreendimentoId = item.UnidadeEmpreendimentoId,
                        OrigemId = loteid
                    };
                    lle.Add(le);
                    i++;
                    double v0 = i;
                    double v00 = lanctos.Count;
                    double v1 = v0 / v00 * 100;
                    int v2 = Convert.ToInt32(Math.Round(v1, 0));
                    if (v2 > j)
                    {
                        j = v2 + 5;
                        callback(v2);
                    }
                }
                await _context.LanctoEmpreendimento.AddRangeAsync(lle);
                await _context.SaveChangesAsync();
                List<LanctoEmpRelacionamento> llr = new();
                List<LanctoEmpUnidade> llu = new();
                foreach (var nl in lle)
                {
                    if (nl.RelacionamentoId != 0)
                    {
                        LanctoEmpRelacionamento lr = new()
                        {
                            LanctoEmpreendimentoId = nl.Id,
                            RelacionamentoId = nl.RelacionamentoId,
                            TenantId = si.TenantId
                        };
                        llr.Add(lr);
                    }
                    if (nl.UnidadeEmpreendimentoId != 0)
                    {
                        LanctoEmpUnidade lun = new()
                        {
                            LanctoEmpreendimentoId = nl.Id,
                            UnidadeEmpreendimentoId = nl.UnidadeEmpreendimentoId,
                            TenantId = si.TenantId
                        };
                        llu.Add(lun);
                    }
                }

                if (llr.Count > 0)
                {
                    await _context.LanctoEmpRelacionamento.AddRangeAsync(llr);
                    await _context.SaveChangesAsync();
                }

                if (llu.Count > 0)
                {
                    await _context.LanctoEmpUnidade.AddRangeAsync(llu);
                    await _context.SaveChangesAsync();
                }

                if (mytrans)
                {
                    await _context.Database.CommitTransactionAsync();
                }
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
                return r;
            }

            r.Ok = true;
            return r;


        }
        public async Task<Resultado> ProcessarLote(LanctoLoteImportacao lote)
        {
            SessionInfo si = await GetSessionAsync();

            Resultado r = new();

            if (lote == null)
            {
                r.Ok = false;
                r.ErrMsg = "Lote não encontrado";
                return r;
            }

            if (lote.Status == StatusLote.Processado)
            {
                r.Ok = false;
                r.ErrMsg = "Lote já processado";
                return r;
            }


            var lanc = await _context.LanctoImportado.Where(x => x.LanctoLoteImportacaoId == lote.Id && x.TenantId == si.TenantId).OrderBy(x => x.DataMovto).ToListAsync();

            if (lanc.Any(x => x.Erro))
            {
                r.Ok = false;
                r.ErrMsg = "Exitem lançamentos com erros, lote não foi processado";
                return r;
            }

            foreach (var item in lanc)
            {
                if (item.PlanoContaId == 0 || item.RelacionamentoId == 0)
                {
                    r.Ok = false;
                    r.ErrMsg = "Exitem lançamentos com erros, lote não foi processado";
                    return r;
                }
            }


            bool mytrans = (_context.Database.CurrentTransaction == null);
            if (mytrans)
            {
                await _context.Database.BeginTransactionAsync();
            }
            try
            {
                foreach (var item in lanc)
                {
                    LanctoEmpreendimento le = new()
                    {
                        PlanoContaId = item.PlanoContaId,
                        CodigoExterno = item.CodigoExterno,
                        DataCompetencia = item.DataCompetencia,
                        DataMovto = item.DataMovto,
                        Descricao = item.Descricao,
                        Documento = item.Documento,
                        Origem = OrigemLancto.Importado,
                        PeriodoId = lote.PeriodoId,
                        Rateado = false,
                        RelacionamentoId = item.RelacionamentoId,
                        TenantId = si.TenantId,
                        Tipo = item.Tipo,
                        Valor = item.Valor,
                        UnidadeEmpreendimentoId = item.UnidadeEmpreendimentoId
                    };
                    await _context.LanctoEmpreendimento.AddAsync(le);
                    await _context.SaveChangesAsync();
                    LanctoEmpreendimentoLanctoImp leli = new()
                    {
                        LanctoImportadoId = item.Id,
                        TenantId = si.TenantId,
                        LanctoEmpreendimentoId = le.Id
                    };
                    await _context.LanctoEmpreendimentoLanctoImp.AddAsync(leli);
                }

                lote.Status = StatusLote.Processado;
                lote.DataProcessamento = Constante.Now;
                var px = lote.Periodo;
                _context.LanctoLoteImportacao.Update(lote);
                await _context.SaveChangesAsync();
                lote.Periodo = px;
                if (mytrans)
                {
                    await _context.Database.CommitTransactionAsync();
                }
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
                return r;
            }

            r.Ok = true;
            return r;


        }
        public async Task<Resultado> CancelarProcessamentoLote(LanctoLoteImportacao lote)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            await _context.Database.BeginTransactionAsync();
            try
            {
                if (lote == null)
                {
                    r.Ok = false;
                    r.ErrMsg = "Lote não encontrado.";
                    _context.Database.RollbackTransaction();
                    return r;
                }

                if (lote.Status != StatusLote.Processado)
                {
                    r.Ok = false;
                    r.ErrMsg = "Lote não está processado.";
                    _context.Database.RollbackTransaction();
                    return r;
                }
                Periodo p;
                if (lote.Periodo == null)
                {
                    p = await _context.Periodo.AsNoTracking().FirstOrDefaultAsync(x => x.Id == lote.PeriodoId && x.TenantId == si.TenantId);
                }
                else
                {
                    p = lote.Periodo;
                }
                if (p.Status != StatusPeriodo.EntradaDeDados)
                {
                    r.Ok = false;
                    r.ErrMsg = "Operação permitida somente para período aberto";
                    _context.Database.RollbackTransaction();
                    return r;
                }

                var li = await _context.LanctoImportado.AsNoTracking().Where(x => x.LanctoLoteImportacaoId == lote.Id && x.TenantId == si.TenantId).ToListAsync();
                List<LanctoEmpreendimento> lle = new();
                List<LanctoEmpreendimentoLanctoImp> lleli = new();
                foreach (var item in li)
                {
                    var leli = await _context.LanctoEmpreendimentoLanctoImp.FirstOrDefaultAsync(x => x.LanctoImportadoId == item.Id && x.TenantId == si.TenantId);
                    if (leli != null)
                    {
                        lleli.Add(leli);
                        var lanc = await _context.LanctoEmpreendimento.FirstOrDefaultAsync(x => x.Id == leli.LanctoEmpreendimentoId && x.TenantId == si.TenantId);
                        if (lanc != null)
                        {
                            lle.Add(lanc);
                        }
                    }
                }
                _context.LanctoEmpreendimentoLanctoImp.RemoveRange(lleli);
                await _context.SaveChangesAsync();
                _context.LanctoEmpreendimento.RemoveRange(lle);
                await _context.SaveChangesAsync();
                lote.Status = StatusLote.Pendente;
                _context.LanctoLoteImportacao.Update(lote);
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
                r.Ok = true;
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }

            return r;


        }
        public async Task<Resultado> CreateNewAuditVersionAsync(Periodo p)
        {
            SessionInfo si = await GetSessionAsync();

            Resultado r = new();

            PeriodoAuditoriaVersao v = new()
            {
                PeriodoId = p.Id,
                DataCriacao = Constante.Now,
                TenantId = si.TenantId,
                DataCancelamento = DateTime.MinValue
            };

            await _context.Database.BeginTransactionAsync();
            try
            {

                List<PeriodoAuditoriaVersao> lv = await _context.PeriodoAuditoriaVersao.Where(x => x.PeriodoId == p.Id && x.TenantId == si.TenantId).OrderByDescending(x => x.DataCriacao).ToListAsync();
                if (lv.Count > 0)
                {
                    lv[0].DataCancelamento = Constante.Now;
                    _context.PeriodoAuditoriaVersao.Update(lv[0]);
                    var ls = await _context.PeriodoSocioAuditoria.Where(x => x.PeriodoAuditoriaVersaoId == lv[0].Id && x.TenantId == si.TenantId).ToListAsync();
                    foreach (var it in ls)
                    {
                        if (it.Status == StatusAuditoriaSocio.Pendente)
                        {
                            it.Status = StatusAuditoriaSocio.Cancelado;
                            _context.PeriodoSocioAuditoria.Update(it);
                        }
                    }
                }

                await _context.PeriodoAuditoriaVersao.AddAsync(v);
                await _context.SaveChangesAsync();

                var sa = await _context.EmpreendimentoSocio.Where(x => x.EmpreendimentoId == p.EmpreendimentoId && x.TenantId == si.TenantId).ToListAsync();
                foreach (EmpreendimentoSocio s in sa)
                {
                    var lsp = await _context.EmpreendimentoSocioParticipacao.Where(x => x.EmpreendimentoSocioId == s.Id && x.TenantId == si.TenantId).ToListAsync();

                    foreach (var sp in lsp)
                    {
                        if (sp.DataInicioDist <= p.DataInicio && sp.DataFimDist >= p.DataFim)
                        {
                            PeriodoSocioAuditoria pa = new()
                            {
                                PeriodoAuditoriaVersaoId = v.Id,
                                EmpreendimentoSocioId = s.Id,
                                TenantId = si.TenantId,
                                DataCriacao = Constante.Now,
                                Status = StatusAuditoriaSocio.Pendente,
                                Descricao = "As informações referente ao período " + p.Nome + " estão corretas."
                            };
                            await _context.PeriodoSocioAuditoria.AddAsync(pa);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                await _context.Database.CommitTransactionAsync();

                r.Ok = true;
                return r;
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }

        }
        public async Task<List<PeridoAuditoriaVersaoView>> GetPeridoAuditoriaStatusAsync(Periodo p)
        {
            SessionInfo si = await GetSessionAsync();

            List<PeridoAuditoriaVersaoView> r = new();

            var l = await (from PeriodoAuditoriaVersao in _context.PeriodoAuditoriaVersao
                           join PeriodoSocioAuditoria in _context.PeriodoSocioAuditoria on PeriodoAuditoriaVersao.Id equals PeriodoSocioAuditoria.PeriodoAuditoriaVersaoId
                           join EmpreendimentoSocio in _context.EmpreendimentoSocio on PeriodoSocioAuditoria.EmpreendimentoSocioId equals EmpreendimentoSocio.Id
                           join Socio in _context.Socio on EmpreendimentoSocio.SocioId equals Socio.Id

                           select new
                           {
                               PeriodoAuditoriaVersao,
                               PeriodoSocioAuditoria,
                               SocioNome = EmpreendimentoSocio.Nome
                           }
                            ).AsNoTracking().Where(x => x.PeriodoAuditoriaVersao.PeriodoId == p.Id && x.PeriodoAuditoriaVersao.TenantId == si.TenantId).OrderByDescending(x => x.PeriodoAuditoriaVersao.DataCriacao).ToListAsync();


            foreach (var it in l)
            {
                PeridoAuditoriaVersaoView pv = r.FirstOrDefault(x => x.PeriodoAuditoriaVersaoId == it.PeriodoAuditoriaVersao.Id);
                if (pv == null)
                {
                    pv = new()
                    {
                        PeriodoAuditoriaVersaoId = it.PeriodoAuditoriaVersao.Id,
                        PeriodoAuditoriaVersao = it.PeriodoAuditoriaVersao,
                        PeriodoSocioAuditoriaViews = new()
                    };
                    r.Add(pv);
                }
                PeriodoSocioAuditoriaView sa = new()
                {
                    Nome = it.SocioNome,
                    PeriodoSocioAuditoria = it.PeriodoSocioAuditoria
                };
                pv.PeriodoSocioAuditoriaViews.Add(sa);
            }
            return r;
        }
        public async Task<bool> AuditoriaFinalizadaAsync(Periodo p)
        {
            SessionInfo si = await GetSessionAsync();
            try
            {
                var l = await _context.PeriodoAuditoriaVersao.AsNoTracking().Where(x => x.PeriodoId == p.Id && x.TenantId == si.TenantId).OrderByDescending(x => x.Id).ToListAsync();
                if (l == null || l.Count == 0)
                {
                    return false;
                }
                List<PeriodoSocioAuditoria> lps = await _context.PeriodoSocioAuditoria.AsNoTracking().Where(x => x.PeriodoAuditoriaVersaoId == l[0].Id && x.TenantId == si.TenantId).ToListAsync();
                if (lps.Count == 0)
                {
                    return false;
                }
                return !lps.Any(x => x.Status == StatusAuditoriaSocio.Pendente);
            }
            catch (Exception e)
            {
                if (e.Message == string.Empty)
                    return true;
            }

            return false;


        }
        public async Task<Resultado> ProcessaFechamentoPeriodoAsync(Periodo p)
        {
            SessionInfo si = await GetSessionAsync();

            Resultado r = new();

            if (p.Status != StatusPeriodo.Fechamento && p.Status != StatusPeriodo.Rateio)
            {
                r.Ok = false;
                r.ErrMsg = "Período precisa estar na fase de fechamento";
                return r;
            }

            /*
            if (!await AuditoriaFinalizadaAsync(p))
            {
                r.Ok = false;
                r.ErrMsg = "Existem sócios com auditoria pendente.";
                return r;
            }
            */
            var lrs = await LoadDadosSocioPlanoGerencial(p, true);

            await _context.Database.BeginTransactionAsync();
            try
            {
                var la = await _context.SocioResultadoPeriodo.Where(x => x.PeriodoId == p.Id && x.TenantId == si.TenantId).ToListAsync();
                foreach (var a in la)
                {
                    if (a.Assinado)
                    {
                        if (!a.Cancelado)
                        {
                            a.Cancelado = true;
                            a.DataCancelamento = Constante.Now;
                            a.Historico += " - cancelado por reprocessamento do período";
                            _context.SocioResultadoPeriodo.Update(a);
                        }
                    }
                    else
                    {
                        _context.SocioResultadoPeriodo.Remove(a);
                    }
                };
                foreach (var s in lrs)
                {
                    LinhaPeriodo ps = s.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == p.Id);
                    if (ps != null)
                    {
                        SocioResultadoPeriodo sl = new()
                        {
                            PeriodoId = p.Id,
                            TenantId = si.TenantId,
                            EmpreendimentoSocioId = s.EmpreendimentoSocioId,
                            GUID = Guid.NewGuid().ToString(),
                            Valor = ps.ValorResultado,
                            Historico = "Resultado do período " + p.Nome,
                            DataMovto = p.DataFim
                        };
                        await _context.SocioResultadoPeriodo.AddAsync(sl);
                    }
                }
                p.Status = StatusPeriodo.Fechado;
                _context.Periodo.Update(p);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                r.Ok = true;
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<List<LinhaSocio>> GetResultadoPeriodoAsync(Periodo p)
        {
            return await LoadDadosSocioPlanoGerencial(p);
        }
        public async Task<List<SocioDebito>> FindAllDebitosByEmpSocioIdAsync(int empsocioid, bool track = false)
        {
            SessionInfo si = await GetSessionAsync();
            if (track)
            {
                return await _context.SocioDebito.Where(x => x.EmpreendimentoSocioId == empsocioid && x.TenantId == si.TenantId).ToListAsync();
            }
            else
            {
                return await _context.SocioDebito.Where(x => x.EmpreendimentoSocioId == empsocioid && x.TenantId == si.TenantId).AsNoTracking().ToListAsync();
            }
        }
        public async Task<List<SocioDebitoLancto>> FindAllDebitosLanctosBySocioDebitoIdAsync(int socioDebitoid, bool track = false)
        {
            SessionInfo si = await GetSessionAsync();
            if (track)
            {
                return await _context.SocioDebitoLancto.Where(x => x.SocioDebitoId == socioDebitoid && x.TenantId == si.TenantId).OrderBy(x => x.DataLancto).ToListAsync();
            }
            else
            {
                return await _context.SocioDebitoLancto.Where(x => x.SocioDebitoId == socioDebitoid && x.TenantId == si.TenantId).OrderBy(x => x.DataLancto).AsNoTracking().ToListAsync();
            }
        }
        public async Task<List<SocioDebitoView>> FindAllDebitosViewByEmpSocioIdAsync(int empsocioid)
        {
            SessionInfo si = await GetSessionAsync();

            var ld = await (from SocioDebito in _context.SocioDebito
                            join EmpreendimentoSocio in _context.EmpreendimentoSocio on SocioDebito.EmpreendimentoSocioId equals EmpreendimentoSocio.Id
                            join Socio in _context.Socio on EmpreendimentoSocio.SocioId equals Socio.Id
                            join SocioDebitoLancto in _context.SocioDebitoLancto on SocioDebito.Id equals SocioDebitoLancto.SocioDebitoId into dl
                            from dl1 in dl.DefaultIfEmpty()
                            select new
                            {
                                SocioDebito,
                                NomeSocio = EmpreendimentoSocio.Nome,
                                SocioDebitoLancto = dl1
                            }).Where(x => x.SocioDebito.EmpreendimentoSocioId == empsocioid && x.SocioDebito.TenantId == si.TenantId).ToListAsync();

            List<SocioDebitoView> r = new();
            foreach (var i in ld)
            {
                SocioDebitoView d = r.FirstOrDefault(x => x.SocioDebito.Id == i.SocioDebito.Id);
                if (d == null)
                {
                    d = new()
                    {
                        SocioDebito = i.SocioDebito,
                        NomeSocio = i.NomeSocio,
                        Lanctos = new()
                    };
                    r.Add(d);
                }
                if (i.SocioDebitoLancto != null)
                {
                    d.Lanctos.Add(i.SocioDebitoLancto);
                }
            }
            return r;
        }
        private async Task<Resultado> ValidarDebito(SocioDebito a)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            var emp = await _context.EmpreendimentoSocio.FirstOrDefaultAsync(x => x.Id == a.EmpreendimentoSocioId && x.TenantId == si.TenantId);
            if (emp == null)
            {
                r.Ok = false;
                r.ErrMsg = "Sócio não informado";
                return r;
            }
            var ex = await _context.Empreendimento.FirstOrDefaultAsync(x => x.Id == emp.EmpreendimentoId && x.TenantId == si.TenantId);
            if (ex == null)
            {
                r.Ok = false;
                r.ErrMsg = "Empreendimento não informado";
                return r;
            }
            if (a.DataLancto < ex.DataInicioOperacao)
            {
                r.Ok = false;
                r.ErrMsg = "Data inválida";
                return r;
            }

            if (a.DataVencto != DateTime.MinValue && a.DataVencto < ex.DataInicioOperacao)
            {
                r.Ok = false;
                r.ErrMsg = "Data de vencimento inválida";
                return r;
            }

            if (a.DataInicioCorrecao < a.DataLancto)
            {
                a.DataInicioCorrecao = a.DataLancto;
            }

            if (a.Valor <= 0)
            {
                r.Ok = false;
                r.ErrMsg = "Valor inválido";
                return r;
            }

            r.Ok = true;
            return r;
        }
        public async Task<Resultado> UpdateSocioDebitoAsync(SocioDebito a)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = await ValidarDebito(a);
            if (!r.Ok)
            {
                return r;
            }
            a.TenantId = si.TenantId;
            try
            {
                _context.SocioDebito.Update(a);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;

        }
        public async Task<Resultado> DeleteSocioDebito(SocioDebito a)
        {
            Resultado r = new();
            SessionInfo si = await GetSessionAsync();
            if (await _context.SocioDebitoLancto.AnyAsync(x => x.SocioDebitoId == a.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Debito com lançamentos não pode ser excluído.";
                return r;
            }

            try
            {
                _context.SocioDebito.Remove(a);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;

        }
        public async Task<Resultado> AddSocioDebitoAsync(SocioDebito a)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = await ValidarDebito(a);
            if (!r.Ok)
            {
                return r;
            }

            a.TenantId = si.TenantId;
            a.GUID = Guid.NewGuid().ToString();


            try
            {
                await _context.SocioDebito.AddAsync(a);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;

        }
        public async Task<Resultado> ValidarDebitoLancto(SocioDebitoLancto d)
        {
            Resultado r = new();
            SessionInfo si = await GetSessionAsync();
            var a = await _context.SocioDebito.FirstOrDefaultAsync(x => x.Id == d.SocioDebitoId && x.TenantId == si.TenantId);
            if (d.DataLancto < a.DataLancto)
            {
                r.Ok = false;
                r.ErrMsg = "Data inválida";
                return r;
            }
            if (d.Valor <= 0)
            {
                r.Ok = false;
                r.ErrMsg = "Valor inválido";
                return r;
            }

            r.Ok = true;
            return r;
        }
        public async Task<Resultado> AddSocioDebitoLanctoAsync(SocioDebitoLancto d, SocioDebito sd)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = await ValidarDebitoLancto(d);
            if (!r.Ok)
            {
                return r;
            }

            d.TenantId = si.TenantId;
            d.GUID = Guid.NewGuid().ToString();

            await _context.Database.BeginTransactionAsync();
            try
            {

                await _context.SocioDebitoLancto.AddAsync(d);
                await _context.SaveChangesAsync();

                double j = Math.Round(await _context.SocioDebitoLancto.Where(x => x.TenantId == si.TenantId && x.SocioDebitoId == sd.Id && x.TipoLanctoDebito == TipoLanctoDebito.Juros).SumAsync(x => x.Valor), 2);
                double a = Math.Round(await _context.SocioDebitoLancto.Where(x => x.TenantId == si.TenantId && x.SocioDebitoId == sd.Id && x.TipoLanctoDebito == TipoLanctoDebito.Amortizacao).SumAsync(x => x.Valor), 2);

                sd.Quitado = (sd.Valor + j - a <= 0);
                _context.SocioDebito.Update(sd);
                await _context.SaveChangesAsync();

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
        public async Task<Resultado> UpdateSocioDebitoLanctoAsync(SocioDebitoLancto d, SocioDebito sd)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = await ValidarDebitoLancto(d);
            if (!r.Ok)
            {
                return r;
            }

            d.TenantId = si.TenantId;
            await _context.Database.BeginTransactionAsync();
            try
            {

                _context.SocioDebitoLancto.Update(d);
                await _context.SaveChangesAsync();

                double j = Math.Round(await _context.SocioDebitoLancto.Where(x => x.TenantId == si.TenantId && x.SocioDebitoId == sd.Id && x.TipoLanctoDebito == TipoLanctoDebito.Juros).SumAsync(x => x.Valor), 2);
                double a = Math.Round(await _context.SocioDebitoLancto.Where(x => x.TenantId == si.TenantId && x.SocioDebitoId == sd.Id && x.TipoLanctoDebito == TipoLanctoDebito.Amortizacao).SumAsync(x => x.Valor), 2);

                sd.Quitado = (sd.Valor + j - a <= 0);
                _context.SocioDebito.Update(sd);
                await _context.SaveChangesAsync();

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
        public async Task<Resultado> DeleteDebitoLanctoAsync(SocioDebitoLancto d, SocioDebito sdp)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            _context.ChangeTracker.Clear();
            if (await _context.SocioRetiradaDebitoLancto.AnyAsync(x => x.SocioDebitoLanctoId == d.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Lançamento gerado pela liberação de distribuição, somente pode ser excluído com o cancelamento da liberação de distribuição.";
                return r;
            }
            List<SocioDebitoLancto> ld = null;
            if (d.GUIDLoteImportado != null && d.GUIDLoteImportado != string.Empty)
            {
                ld = await _context.SocioDebitoLancto.Where(x => x.GUIDLoteImportado == d.GUIDLoteImportado && x.TenantId == si.TenantId && x.Id != d.Id).ToListAsync();
            }
            ld.Add(d);
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.SocioDebitoLancto.RemoveRange(ld);
                await _context.SaveChangesAsync();
                SocioDebito sd = null;
                foreach (var dels in ld)
                {
                    if (sdp.Id == dels.SocioDebitoId)
                    {
                        sd = sdp;
                    }
                    else
                    {
                        sd = await _context.SocioDebito.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == dels.SocioDebitoId);
                    }
                    double j = Math.Round(await _context.SocioDebitoLancto.Where(x => x.TenantId == si.TenantId && x.SocioDebitoId == sd.Id && x.TipoLanctoDebito == TipoLanctoDebito.Juros).SumAsync(x => x.Valor), 2);
                    double a = Math.Round(await _context.SocioDebitoLancto.Where(x => x.TenantId == si.TenantId && x.SocioDebitoId == sd.Id && x.TipoLanctoDebito == TipoLanctoDebito.Amortizacao).SumAsync(x => x.Valor), 2);
                    sd.Quitado = (sd.Valor + j - a <= 0);
                    _context.SocioDebito.Update(sd);
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                r.Ok = true;
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<SocioRetiradaAutoView> GetSocioRetiradaAutoViewBySocioEmpIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            SocioRetiradaAutoView r = new()
            {
                EmpreendimentoSocioId = id
            };
            r.ValorResultadoAcumulado = Math.Round(await _context.SocioResultadoPeriodo.Where(x => x.EmpreendimentoSocioId == id && x.Cancelado == false && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);

            r.ValorAportesAcumulado = Math.Round(await (from SocioAporteDeposito in _context.SocioAporteDeposito
                                                        join SocioAporte in _context.SocioAporte on SocioAporteDeposito.SocioAporteId equals SocioAporte.Id
                                                        select new { SocioAporteDeposito, SocioAporte }
                                       ).Where(x => x.SocioAporte.EmpreendimentoSocioId == id && x.SocioAporteDeposito.TenantId == si.TenantId).SumAsync(x => x.SocioAporteDeposito.Valor), 2);

            r.ValorDebitoAcumulado = Math.Round(await _context.SocioDebito.Where(x => x.EmpreendimentoSocioId == id && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
            r.ValorDebitoAcumulado += Math.Round(await (from SocioDebitoLancto in _context.SocioDebitoLancto
                                                        join SocioDebito in _context.SocioDebito on SocioDebitoLancto.SocioDebitoId equals SocioDebito.Id
                                                        select new { SocioDebitoLancto, SocioDebito }).Where(x => x.SocioDebitoLancto.TipoLanctoDebito == TipoLanctoDebito.Juros && x.SocioDebito.EmpreendimentoSocioId == id && x.SocioDebito.TenantId == si.TenantId).SumAsync(x => x.SocioDebitoLancto.Valor), 2);

            r.ValorRetiradaAcumulado = Math.Round(await _context.SocioRetirada.Where(x => x.EmpreendimentoSocioId == id && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
            r.ValorRetiradaDepositado = Math.Round(await (from SocioRetiradaLancto in _context.SocioRetiradaLancto
                                                          join SocioRetirada in _context.SocioRetirada on SocioRetiradaLancto.SocioRetiradaId equals SocioRetirada.Id
                                                          select new { SocioRetiradaLancto, SocioRetirada }
                                       ).Where(x => x.SocioRetirada.EmpreendimentoSocioId == id && x.SocioRetiradaLancto.TenantId == si.TenantId).SumAsync(x => x.SocioRetiradaLancto.Valor), 2);

            r.ValorDebitoAmortizado = Math.Round(await (from SocioDebitoLancto in _context.SocioDebitoLancto
                                                        join SocioDebito in _context.SocioDebito on SocioDebitoLancto.SocioDebitoId equals SocioDebito.Id
                                                        select new { SocioDebitoLancto, SocioDebito }).Where(x => x.SocioDebitoLancto.TipoLanctoDebito == TipoLanctoDebito.Amortizacao && x.SocioDebito.EmpreendimentoSocioId == id && x.SocioDebito.TenantId == si.TenantId).SumAsync(x => x.SocioDebitoLancto.Valor), 2);

            return r;

        }
        public async Task<Resultado> CancelarFechamentoPeriodo(Periodo p)
        {
            SessionInfo si = await GetSessionAsync();
            if (await _context.Periodo.AnyAsync(x => x.TenantId == si.TenantId && x.DataInicio > p.DataFim && x.Status == StatusPeriodo.Fechado && x.EmpreendimentoId == p.EmpreendimentoId))
            {
                Resultado r = new()
                {
                    Ok = false,
                    ErrMsg = "Não é permitido cancelar um período com período posterior fechado."
                };
                return r;
            }
            return await SetPeriodoStatusAsync(p, StatusPeriodo.EntradaDeDados);
        }
        public async Task<Resultado> SaveAmortizacoes(SocioDebitoView sl)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            bool mytrans = (_context.Database.CurrentTransaction == null);
            if (mytrans)
            {
                await _context.Database.BeginTransactionAsync();
            }
            try
            {
                foreach (var sdl in sl.Lanctos)
                {
                    if (sdl.Valor > 0)
                    {
                        var tlav = await FindAllDebitosViewByEmpSocioIdAsync(sl.SocioDebito.EmpreendimentoSocioId);
                        tlav = tlav.Where(x => x.SocioDebito.Quitado == false).OrderBy(x => x.SocioDebito.DataLancto).ToList();
                        double va = sdl.Valor;
                        foreach (var d in tlav)
                        {
                            if (d.ValorSaldo > 0 && va > 0)
                            {
                                SocioDebitoLancto sd = new()
                                {
                                    GUID = Guid.NewGuid().ToString(),
                                    DataLancto = sdl.DataLancto,
                                    TenantId = si.TenantId,
                                    TipoLanctoDebito = TipoLanctoDebito.Amortizacao,
                                    SocioDebitoId = d.SocioDebito.Id,
                                    Origem = OrigemLancto.ImportadoND,
                                    Historico = "Amortização importada em " + Constante.Now.ToString("dd/MM/yyyy HH:mm"),
                                    EmpreendimentoSocioId = sl.SocioDebito.EmpreendimentoSocioId,
                                    GUIDLoteImportado = sdl.GUIDLoteImportado
                                };
                                if (d.ValorSaldo > va)
                                {
                                    sd.Valor = va;
                                    va = 0;
                                }
                                else
                                {
                                    sd.Valor = d.ValorSaldo;
                                    va -= d.ValorSaldo;
                                    d.SocioDebito.Quitado = true;
                                    _context.SocioDebito.Update(d.SocioDebito);
                                }
                                await _context.SocioDebitoLancto.AddAsync(sd);
                                await _context.SaveChangesAsync();
                                d.Lanctos.Add(sd);
                                await _context.SaveChangesAsync();
                            }
                        }
                        if (va > 0)
                        {
                            throw new("Valor da amortização maior que os valores dos emprestimos/adiantamentos. data " + sdl.DataLancto.ToString("dd/MM/yyyy") + " valor " + sdl.Valor.ToString("C2", Constante.Culture));
                        }
                    }
                }
                if (mytrans)
                {
                    _context.Database.CommitTransaction();
                }
                r.Ok = true;
                r.ErrMsg = "Lançamento de liberação de retiradas gravados com sucesso.";
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                if (mytrans)
                {
                    _context.Database.RollbackTransaction();
                }
                else
                {
                    throw;
                }
            }
            return r;
        }
        public async Task<Resultado> ProcessarLiberacaoRetirada(List<SocioRetiradaAutoView> lsl)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();


            if (lsl.Sum(x => x.ValorRetiradaAutorizado) == 0)
            {
                r.Ok = false;
                r.ErrMsg = "Valor autorizado para retirada não informado";
                return r;
            }

            if (lsl.Any(x => x.ValorRetiradaAutorizado < 0))
            {
                r.Ok = false;
                r.ErrMsg = "Valor autorizado para retirada inválido";
                return r;
            }

            if (lsl.Any(x => x.DataPagamento < Constante.Today.AddYears(-4)))
            {
                r.Ok = false;
                r.ErrMsg = "Data para pagamento inválida";
                return r;
            }
            if (lsl.Any(x => x.DataPagamento > Constante.Today.AddYears(1)))
            {
                r.Ok = false;
                r.ErrMsg = "Data para pagamento inválida";
                return r;
            }

            _context.Database.BeginTransaction();
            try
            {
                foreach (var sl in lsl.Where(x => x.ValorRetiradaAutorizado > 0).ToList())
                {
                    SocioRetirada sr = new()
                    {
                        EmpreendimentoSocioId = sl.EmpreendimentoSocioId,
                        DataLiberacao = sl.DataLiberacao,
                        DataPrevistaPagamento = sl.DataPagamento,
                        GUID = Guid.NewGuid().ToString(),
                        Historico = "liberação de resultado",
                        TenantId = si.TenantId,
                        Valor = sl.ValorRetiradaAutorizado
                    };
                    await _context.SocioRetirada.AddAsync(sr);
                    await _context.SaveChangesAsync();
                    if (sl.ValorAmortizarDebito > 0)
                    {
                        var tlav = await FindAllDebitosViewByEmpSocioIdAsync(sl.EmpreendimentoSocioId);
                        tlav = tlav.Where(x => x.SocioDebito.Quitado == false).OrderBy(x => x.SocioDebito.DataLancto).ToList();
                        double va = sl.ValorAmortizarDebito;
                        foreach (var d in tlav)
                        {
                            if (d.ValorSaldo > 0 && va > 0)
                            {
                                SocioDebitoLancto sd = new()
                                {
                                    GUID = Guid.NewGuid().ToString(),
                                    DataLancto = sl.DataLiberacao,
                                    TenantId = si.TenantId,
                                    TipoLanctoDebito = TipoLanctoDebito.Amortizacao,
                                    SocioDebitoId = d.SocioDebito.Id,
                                    Origem = OrigemLancto.Sistema,
                                    Historico = "Amortização por distribuição",
                                    EmpreendimentoSocioId = sl.EmpreendimentoSocioId
                                };
                                if (d.ValorSaldo > va)
                                {
                                    sd.Valor = va;
                                    va = 0;
                                }
                                else
                                {
                                    sd.Valor = d.ValorSaldo;
                                    va -= d.ValorSaldo;
                                    d.SocioDebito.Quitado = true;
                                    _context.SocioDebito.Update(d.SocioDebito);
                                }
                                await _context.SocioDebitoLancto.AddAsync(sd);
                                await _context.SaveChangesAsync();
                                d.Lanctos.Add(sd);
                                SocioRetiradaDebitoLancto srd = new()
                                {
                                    SocioDebitoLanctoId = sd.Id,
                                    SocioRetiradaId = sr.Id,
                                    TenantId = si.TenantId
                                };
                                await _context.SocioRetiradaDebitoLancto.AddAsync(srd);
                                await _context.SaveChangesAsync();

                            }
                        }
                    }

                }
                _context.Database.CommitTransaction();
                r.Ok = true;
                r.ErrMsg = "Lançamento de liberação de retiradas gravados com sucesso.";
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                _context.Database.RollbackTransaction();
            }

            return r;
        }
        public async Task<Resultado> ValidarRetiradaLancto(SocioRetiradaLancto d)
        {
            Resultado r = new();
            SessionInfo si = await GetSessionAsync();
            var a = await _context.SocioRetirada.FirstOrDefaultAsync(x => x.Id == d.SocioRetiradaId && x.TenantId == si.TenantId);
            if (d.DataDeposito < a.DataPrevistaPagamento.AddDays(-10))
            {
                r.Ok = false;
                r.ErrMsg = "Data inválida";
                return r;
            }
            if (d.Valor <= 0)
            {
                r.Ok = false;
                r.ErrMsg = "Valor inválido";
                return r;
            }

            double vd = Math.Round(await _context.SocioRetiradaLancto.Where(x => x.SocioRetiradaId == d.SocioRetiradaId && x.Id != d.Id).SumAsync(x => x.Valor), 2) +
                        Math.Round(await (from SocioRetiradaDebitoLancto in _context.SocioRetiradaDebitoLancto
                                          join SocioDebitoLancto in _context.SocioDebitoLancto on SocioRetiradaDebitoLancto.SocioDebitoLanctoId equals SocioDebitoLancto.Id
                                          select new { SocioDebitoLancto.Valor, SocioRetiradaDebitoLancto.SocioRetiradaId, SocioDebitoLancto.TenantId }
                             ).Where(x => x.SocioRetiradaId == d.SocioRetiradaId && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);

            if (vd + d.Valor > a.Valor)
            {
                r.Ok = false;
                r.ErrMsg = "Valor do(s) depósitos superior ao valor da retirada.";
                return r;
            }

            r.Ok = true;
            return r;
        }
        public async Task<Resultado> AddSocioRetiradaLanctoAsync(SocioRetiradaLancto d)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = await ValidarRetiradaLancto(d);
            if (!r.Ok)
            {
                return r;
            }
            d.TenantId = si.TenantId;
            d.GUID = Guid.NewGuid().ToString();
            try
            {
                await _context.SocioRetiradaLancto.AddAsync(d);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> UpdateSocioRetiradaLanctoAsync(SocioRetiradaLancto d)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = await ValidarRetiradaLancto(d);
            if (!r.Ok)
            {
                return r;
            }

            d.TenantId = si.TenantId;
            try
            {
                _context.SocioRetiradaLancto.Update(d);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> DeleteRetiradaLanctoAsync(SocioRetiradaLancto d)
        {
            Resultado r = new();
            try
            {
                _context.SocioRetiradaLancto.Remove(d);
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
        public async Task<Resultado> DeleteSocioRetirada(SocioRetirada sr)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (await _context.SocioRetiradaLancto.AnyAsync(x => x.SocioRetiradaId == sr.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Distribuição com lançamentos não pode ser excluída";
                return r;
            }
            List<SocioRetiradaDebitoLancto> lsd = await _context.SocioRetiradaDebitoLancto.Where(x => x.SocioRetiradaId == sr.Id && x.TenantId == si.TenantId).ToListAsync();
            _context.Database.BeginTransaction();
            try
            {
                foreach (var it in lsd)
                {
                    SocioDebitoLancto sd = await _context.SocioDebitoLancto.FirstOrDefaultAsync(x => x.Id == it.SocioDebitoLanctoId && x.TenantId == si.TenantId);
                    if (sd != null)
                    {
                        SocioDebito sdeb = await _context.SocioDebito.FirstOrDefaultAsync(x => x.Id == sd.SocioDebitoId && x.TenantId == si.TenantId);
                        sdeb.Quitado = false;
                    }
                    _context.SocioRetiradaDebitoLancto.Remove(it);
                    await _context.SaveChangesAsync();
                    if (sd != null)
                    {
                        _context.SocioDebitoLancto.Remove(sd);
                        await _context.SaveChangesAsync();
                    }
                }
                _context.SocioRetirada.Remove(sr);
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
                return r;
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<LanctoLoteImportacao> GetLanctoLoteImportacaoByLanctoEmpreedimentoId(int id)
        {
            SessionInfo si = await GetSessionAsync();

            var l = await (from LanctoLoteImportacao in _context.LanctoLoteImportacao
                           join LanctoImportado in _context.LanctoImportado on LanctoLoteImportacao.Id equals LanctoImportado.LanctoLoteImportacaoId
                           join LanctoEmpreendimentoLanctoImp in _context.LanctoEmpreendimentoLanctoImp on LanctoImportado.Id equals LanctoEmpreendimentoLanctoImp.LanctoImportadoId
                           join LanctoEmpreendimento in _context.LanctoEmpreendimento on LanctoEmpreendimentoLanctoImp.LanctoEmpreendimentoId equals LanctoEmpreendimento.Id

                           select new { LanctoLoteImportacao, LanctoEmpreendimentoId = LanctoEmpreendimento.Id }).Where(x => x.LanctoEmpreendimentoId == id && x.LanctoLoteImportacao.TenantId == si.TenantId).FirstOrDefaultAsync();

            if (l != null)
            {
                return l.LanctoLoteImportacao;
            }
            else
            {
                return null;
            }
        }
        public async Task<LanctoEmpreendimentoLanctoImp> GetLanctoEmpreendimentoLanctoImpByLanctoEmpreedimentoId(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.LanctoEmpreendimentoLanctoImp.Where(x => x.LanctoEmpreendimentoId == id && x.TenantId == si.TenantId).FirstOrDefaultAsync();
        }
        public async Task<int> GetSocioPrincipalIdBySocioEmp(int empid, int empsocioid)
        {
            SessionInfo si = await GetSessionAsync();
            var gpdid = await (from GrupoSocio in _context.GrupoSocio
                               join GrupoSocioEmpreedSocio in _context.GrupoSocioEmpreedSocio on GrupoSocio.Id equals GrupoSocioEmpreedSocio.GrupoSocioId
                               select new { GrupoSocio, GrupoSocioEmpreedSocio.EmpreendimentoSocioId }
                          ).AsNoTracking().Where(x => x.GrupoSocio.EmpreendimentoId == empid && x.GrupoSocio.UtilizarGerencial == true
                                   && x.GrupoSocio.TenantId == si.TenantId && x.EmpreendimentoSocioId == empsocioid
                                    ).FirstOrDefaultAsync();
            if (gpdid == null)
            {
                return 0;
            }
            else
            {
                return gpdid.GrupoSocio.Id;
            }
        }
        public async Task<Resultado> ExportarMovtoBancoToLanctoEmp(int contaid, DateTime dti, DateTime dtf, Func<int, int> callback, string lotegruid = null)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            FinanceService fi = new(_context, _sessionStorageService, _sessionService, _mapper);
            List<MovtoBancoView> lm1 = null;
            List<MovtoBancoView> lm = null;
            ContaCorrenteEmpreendimento ce = null;

            if (lotegruid == null || lotegruid == string.Empty)
            {
                ce = await _context.ContaCorrenteEmpreendimento.FirstOrDefaultAsync(x => x.ContaCorrenteId == contaid && x.TenantId == si.TenantId);
                lm1 = await fi.FindAllMovtoBancoByContaId(contaid, dti, dtf);
                lm = lm1.Where(x => x.Exportado == false && x.Transferencia == false).ToList();
                if (lm.Count == 0 && lm1.Count > 0)
                {
                    {
                        r.Ok = false;
                        r.ErrMsg = "Todos os lançamentos já foram exportados";
                        return r;
                    }
                }
            }
            else
            {
                ce = await _context.ContaCorrenteEmpreendimento.FirstOrDefaultAsync(x => x.ContaCorrenteId == contaid && x.TenantId == si.TenantId);
                lm1 = await fi.FindAllMovtoBancoByLoteGUID(lotegruid);
                lm = lm1.Where(x => x.Exportado == false && x.Transferencia == false).ToList();
                if (lm.Count == 0 && lm1.Count > 0)
                {
                    {
                        r.Ok = false;
                        r.ErrMsg = "Todos os lançamentos já foram exportados";
                        return r;
                    }
                }
            }

            List<Periodo> lp = await _context.Periodo.Where(x => x.DataInicio >= UtilsClass.GetInicio(dti) && x.DataFim <= UtilsClass.GetUltimo(dtf) && x.TenantId == si.TenantId && x.EmpreendimentoId == ce.EmpreendimentoId).ToListAsync();

            foreach (var p in lp)
            {
                if (p.Status != StatusPeriodo.EntradaDeDados)
                {
                    r.Ok = false;
                    r.ErrMsg = "Situação do período " + p.Nome + " não permite integrar movimento bancário";
                    return r;
                }
            }

            foreach (var l in lm)
            {
                if (l.PlanoContaId == 0 || l.RelacionamentoId == 0)
                {
                    r.Ok = false;
                    r.ErrMsg = "Exite(m) lançamento(s) com plano de contas ou favorecido não informado";
                    return r;
                }
            }
            try
            {
                await _context.Database.BeginTransactionAsync();
                int i = 0;
                int j = -1;
                List<LanctoEmpreendimento> lle = new();
                foreach (var l in lm)
                {
                    if (l.Transferencia)
                    {
                        continue;
                    }
                    Periodo p = lp.FirstOrDefault(x => x.DataInicio <= l.DataMovto && x.DataFim >= l.DataMovto);
                    if (p == null)
                    {
                        p = await NovoPeriodoDataAsync(ce.EmpreendimentoId, l.DataMovto, TipoAlerta.Novo);
                        lp.Add(p);
                    }
                    else
                    {
                        if (p.Alerta != TipoAlerta.Alterado)
                        {
                            p.Alerta = TipoAlerta.Alterado;
                            _context.Periodo.Update(p);
                        }
                    }
                    LanctoEmpreendimento le = fi.ConvertMovtoBancoToLanctoEmp(l, p.Id);
                    le.TenantId = si.TenantId;
                    lle.Add(le);
                    if (callback != null)
                    {
                        i++;
                        double v0 = i;
                        double v00 = lm.Count;
                        double v1 = v0 / v00 * 100;
                        int v2 = Convert.ToInt32(Math.Round(v1, 0));
                        if (v2 > j)
                        {
                            j = v2 + 5;
                            callback(v2);
                        }
                    }
                }
                await _context.LanctoEmpreendimento.AddRangeAsync(lle);
                await _context.SaveChangesAsync();
                foreach (var xle in lle)
                {
                    if (xle.RelacionamentoId != 0)
                    {
                        xle.LanctoEmpRelacionamento.LanctoEmpreendimentoId = xle.Id;
                        xle.LanctoEmpRelacionamento.TenantId = si.TenantId;
                        await _context.LanctoEmpRelacionamento.AddAsync(xle.LanctoEmpRelacionamento);
                    }
                }
                await _context.SaveChangesAsync();
                lle.Clear();
                lle = null;
                await _context.Database.CommitTransactionAsync();
                r.Ok = true;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                await _context.Database.RollbackTransactionAsync();
            }
            return r;
        }
        public async Task<List<LinhaSocio>> LoadDadosSocioPlanoGerencial(Periodo p, bool fechamento = false)
        {
            return await LoadDadosSocioPlanoGerencialByDate(p.EmpreendimentoId, p.DataInicio, p.DataFim, fechamento);
        }
        public async Task<List<LinhaPlanoGerencial>> LoadDadosPlanoGerencialByDate(int empid, DateTime dtini, DateTime dtfim, int empsocioid = 0)
        {
            SessionInfo si = await GetSessionAsync();
            List<LinhaPlanoGerencial> r = new();
            string sql1 = string.Empty;
            string sql2 = string.Empty;
            string sqlcc2 = string.Empty;
            string sqlcc3 = string.Empty;
            if (empsocioid != 0)
            {
                EmpreendimentoSocio es = await _context.EmpreendimentoSocio.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.EmpreendimentoId == empid && x.Id == empsocioid);
                if (es == null)
                {
                    return r;
                }
                if (!es.VisualizarDemonstrativo)
                {
                    /* duplicar o lancamento porque tem mais de 1 socio rateado para o mesmo lançamento.
                    var gpdid = await (from GrupoSocio in _context.GrupoSocio
                                     join GrupoSocioEmpreedSocio in _context.GrupoSocioEmpreedSocio on GrupoSocio.Id equals GrupoSocioEmpreedSocio.GrupoSocioId
                                     select new { GrupoSocio, GrupoSocioEmpreedSocio.EmpreendimentoSocioId }
                                  ).AsNoTracking().Where(x => x.GrupoSocio.EmpreendimentoId == empid && x.GrupoSocio.UtilizarGerencial == true
                                           && x.GrupoSocio.TenantId == si.TenantId && x.EmpreendimentoSocioId == empsocioid
                                            ).FirstOrDefaultAsync();
                    if (gpdid != null)
                    {
                        var loutrossoc = await _context.GrupoSocioEmpreedSocio.Where(x => x.TenantId == si.TenantId && x.GrupoSocioId == gpdid.GrupoSocio.Id).ToListAsync();
                        string incla = "(";
                        for (int i = 0; i < loutrossoc.Count; i++)
                        {
                            if (i == 0)
                            {
                                incla = incla + loutrossoc[i].EmpreendimentoSocioId.ToString();
                            }
                            else
                            {
                                incla = incla + "," + loutrossoc[i].EmpreendimentoSocioId.ToString();
                            }
                        }
                        incla = incla + ")";
                        sql1 = " inner join LanctoEmpRateadoPlanoContaSocio on LanctoEmpRateadoPlanoContaSocio.PlanoContaId = PlanoConta.Id ";
                        sql2 = " and LanctoEmpRateadoPlanoContaSocio.EmpreendimentoSocioId in  " + incla + " and  LanctoEmpRateadoPlanoContaSocio.PeriodoId = Periodo.Id ";
                    }
                    else
                    */
                    {
                        sql1 = " inner join LanctoEmpRateadoPlanoContaSocio on LanctoEmpRateadoPlanoContaSocio.PlanoContaId = PlanoConta.Id ";
                        sql2 = " and LanctoEmpRateadoPlanoContaSocio.EmpreendimentoSocioId =  " + empsocioid.ToString() + " and  LanctoEmpRateadoPlanoContaSocio.PeriodoId = Periodo.Id ";
                    }
                }


                string sqlcc = string.Empty;
                List<ContaCorrenteEmpreendimento> contas = await _context.ContaCorrenteEmpreendimento.Where(x => x.EmpreendimentoId == empid).ToListAsync();
                if (contas != null && contas.Count > 0)
                {
                    var lcs = await _context.EmpSocioContaCorrente.AsNoTracking().Where(x => x.TenantId == si.TenantId && x.EmpreendimentoSocioId == empsocioid && x.Permitir == false).ToListAsync();
                    if (lcs.Count > 0)
                    {
                        sqlcc = " and MovtoBanco.ContaCorrenteId in ( ";
                        int i = 0;
                        foreach (var cr in contas)
                        {
                            if (!lcs.Any(x => x.ContaCorrenteId == cr.ContaCorrenteId))
                            {
                                if (i == 0)
                                {
                                    sqlcc += cr.ContaCorrenteId.ToString();
                                    i++;

                                }
                                else
                                {
                                    sqlcc += "," + cr.ContaCorrenteId.ToString();
                                }
                            }
                        }
                        if (i == 0)
                        {
                            return null;
                        }
                        else
                        {
                            sqlcc += " ) ";
                        }
                    }

                    sqlcc2 =
        "   union all         select " +
        "Periodo.Id PeriodoId, " +
        "Max(Periodo.DataInicio) DataInicio, " +
        "PlanoConta.Tipo TipoConta, " +
        "PlanoConta.Id PlanoContaId, " +
        "Isnull(PlanoConta.NomeCurto,PlanoConta.Nome) PlanoContaNome, " +
        "PlanoGerencial.Id PlanoGerencialId, " +
        "PlanoGerencial.Nome PlanoGerencialNome, " +
        "Max(Isnull(PlanoGerencial.NomeGrupoContaAuxiliar,'Conta Auxiliar')) NomeGrupoContaAuxiliar, " +
        "max( " +
        "case when ISNULL(PeriodoAtu.Id, 0) = LanctoEmpreendimento.PeriodoId then " +
        "LanctoEmpreendimento.PeriodoId " +
        "else " +
        "      0   end )  PeriodoAtu, " +

        "sum( " +
        "case when ISNULL(PeriodoAtu.Id, 0) = LanctoEmpreendimento.PeriodoId then " +
        "0.0 " +
        "else " +
        " case when (LanctoEmpreendimento.tipo =3 and PlanoConta.Tipo = 4) or  (LanctoEmpreendimento.tipo =4 and PlanoConta.Tipo = 3) or (LanctoEmpreendimento.tipo = 4 and PlanoConta.Tipo = 6) then  " +
        " LanctoEmpreendimento.Valor * -1  " +
        " else  " +
        " LanctoEmpreendimento.Valor end " +
        "                 end )  ValorAnterior, " +
        "sum( " +
        "case when ISNULL(PeriodoAtu.Id, 0) = LanctoEmpreendimento.PeriodoId then " +
        " case when (LanctoEmpreendimento.tipo =3 and PlanoConta.Tipo = 4) or  (LanctoEmpreendimento.tipo =4 and PlanoConta.Tipo = 3) or (LanctoEmpreendimento.tipo = 4 and PlanoConta.Tipo = 6)  then  " +
        " LanctoEmpreendimento.Valor * -1  " +
        " else  " +
        " LanctoEmpreendimento.Valor end " +
        "else " +
        "                0 end )  ValorPeriodo " +
        "                from LanctoEmpreendimento " +
        "                inner join PlanoConta on PlanoConta.Id = LanctoEmpreendimento.PlanoContaId " +
        "inner join PlanoGerencial on PlanoGerencial.Id = PlanoConta.PlanoGerencialId " +
        "inner join MovtoBanco on MovtoBanco.Id = LanctoEmpreendimento.OrigemId " +

        "inner join Periodo on Periodo.Id = LanctoEmpreendimento.PeriodoId and Periodo.EmpreendimentoId = @empid " +
        "left join Periodo PeriodoAtu on PeriodoAtu.DataInicio >= @dtini and PeriodoAtu.DataFim <= @dtfim  and LanctoEmpreendimento.PeriodoId = PeriodoAtu.Id and PeriodoAtu.EmpreendimentoId = @empid2 " +
        "where LanctoEmpreendimento.DataMovto <=  Convert(date, @dtfim2, 112)  and LanctoEmpreendimento.TenantId = @tid " +
        "and LanctoEmpreendimento.Origem = 4 " +
        "and PlanoConta.Tipo = 6 " +
        sqlcc +
        " group by " +
        "Periodo.Id , " +
        "PlanoConta.Tipo , " +
        "PlanoConta.Id , " +
        "Isnull(PlanoConta.NomeCurto,PlanoConta.Nome) , " +
        "PlanoGerencial.Id , " +
        "PlanoGerencial.Nome ";
                    sqlcc3 =
        "and PlanoConta.Tipo != 6 ";
                }



            }

            dtini = UtilsClass.GetInicio(dtini);
            dtfim = UtilsClass.GetUltimo(dtfim);
            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
        "            select " +
        "Periodo.Id PeriodoId, " +
        "Max(Periodo.DataInicio) DataInicio, " +
        "PlanoConta.Tipo TipoConta, " +
        "PlanoConta.Id PlanoContaId, " +
        "Isnull(PlanoConta.NomeCurto,PlanoConta.Nome) PlanoContaNome, " +
        "PlanoGerencial.Id PlanoGerencialId, " +
        "PlanoGerencial.Nome PlanoGerencialNome, " +
        "Max(Isnull(PlanoGerencial.NomeGrupoContaAuxiliar,'Conta Auxiliar')) NomeGrupoContaAuxiliar, " +
        "max( " +
        "case when ISNULL(PeriodoAtu.Id, 0) = LanctoEmpreendimento.PeriodoId then " +
        "LanctoEmpreendimento.PeriodoId " +
        "else " +
        "      0   end )  PeriodoAtu, " +

        "sum( " +
        "case when ISNULL(PeriodoAtu.Id, 0) = LanctoEmpreendimento.PeriodoId then " +
        "0.0 " +
        "else " +
        " case when (LanctoEmpreendimento.tipo =3 and PlanoConta.Tipo = 4) or  (LanctoEmpreendimento.tipo =4 and PlanoConta.Tipo = 3) or (LanctoEmpreendimento.tipo = 4 and PlanoConta.Tipo = 6) then  " +
        " LanctoEmpreendimento.Valor * -1  " +
        " else  " +
        " LanctoEmpreendimento.Valor end " +
        "                 end )  ValorAnterior, " +
        "sum( " +
        "case when ISNULL(PeriodoAtu.Id, 0) = LanctoEmpreendimento.PeriodoId then " +
        " case when (LanctoEmpreendimento.tipo =3 and PlanoConta.Tipo = 4) or  (LanctoEmpreendimento.tipo =4 and PlanoConta.Tipo = 3) or (LanctoEmpreendimento.tipo = 4 and PlanoConta.Tipo = 6)  then  " +
        " LanctoEmpreendimento.Valor * -1  " +
        " else  " +
        " LanctoEmpreendimento.Valor end " +
        "else " +
        "                0 end )  ValorPeriodo " +
        "                from LanctoEmpreendimento " +
        "                inner join PlanoConta on PlanoConta.Id = LanctoEmpreendimento.PlanoContaId " +
        "inner join PlanoGerencial on PlanoGerencial.Id = PlanoConta.PlanoGerencialId " +
        "inner join Periodo on Periodo.Id = LanctoEmpreendimento.PeriodoId and Periodo.EmpreendimentoId = @empid " +
        sql1 +
        "left join Periodo PeriodoAtu on PeriodoAtu.DataInicio >= @dtini and PeriodoAtu.DataFim <= @dtfim  and LanctoEmpreendimento.PeriodoId = PeriodoAtu.Id and PeriodoAtu.EmpreendimentoId = @empid2 " +
        "where LanctoEmpreendimento.DataMovto <=  Convert(date, @dtfim2, 112)  and LanctoEmpreendimento.TenantId = @tid " +
        sql2 +
        sqlcc3 +
        " group by " +
        "Periodo.Id , " +
        "PlanoConta.Tipo , " +
        "PlanoConta.Id , " +
        "Isnull(PlanoConta.NomeCurto,PlanoConta.Nome) , " +
        "PlanoGerencial.Id , " +
        "PlanoGerencial.Nome " +
        sqlcc2 +
        "order by Periodo.Id ,  PlanoConta.Tipo, PlanoGerencial.Nome,Isnull(PlanoConta.NomeCurto,PlanoConta.Nome) ";


            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = empid;

            command.Parameters.Add(new SqlParameter("@empid2", System.Data.SqlDbType.Int));
            command.Parameters["@empid2"].Value = empid;

            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@dtini", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim"].Value = dtfim.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim2"].Value = dtfim.ToString("yyyyMMdd");
            await command.Connection.OpenAsync();
            var lu = await command.ExecuteReaderAsync();
            try
            {
                while (await lu.ReadAsync())
                {
                    LinhaPlanoGerencial ig = r.FirstOrDefault(x => x.PlanoGerencialId == (int)lu["PlanoGerencialId"]);
                    if (ig == null)
                    {
                        ig = new()
                        {
                            PlanoGerencialId = (int)lu["PlanoGerencialId"],
                            PlanoGerencialNome = lu["PlanoGerencialNome"].ToString(),
                            PlanoContas = new(),
                            TipoPlanoConta = (TipoPlanoConta)lu["TipoConta"],
                            Expand = false
                        };
                        if (ig.TipoPlanoConta == TipoPlanoConta.Auxiliar)
                        {
                            ig.NomeGrupoContaAuxiliar = lu["NomeGrupoContaAuxiliar"].ToString();
                        }
                        else
                        {
                            ig.NomeGrupoContaAuxiliar = string.Empty;
                        }
                        r.Add(ig);
                    }
                    if ((int)lu["PeriodoAtu"] != 0)
                    {
                        LinhaPeriodo pg = ig.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                        if (pg == null)
                        {
                            pg = new()
                            {
                                PeriodoId = (int)lu["PeriodoId"],
                                DataInicio = (DateTime)lu["DataInicio"],
                            };
                            ig.LinhaPeriodos.Add(pg);
                        }
                        pg.Valor += (double)lu["ValorPeriodo"];
                    }
                    ig.ValorAnterior += (double)lu["ValorAnterior"];
                    ig.ValorPeriodo += (double)lu["ValorPeriodo"];
                    ig.ValorTotal = ig.ValorAnterior + ig.ValorPeriodo;
                    LinhaPlanoConta ipc = ig.PlanoContas.FirstOrDefault(x => x.PlanoContaId == (int)lu["PlanoContaId"]);
                    if (ipc == null)
                    {
                        ipc = new()
                        {
                            PlanoContaId = (int)lu["PlanoContaId"],
                            TipoPlanoConta = (TipoPlanoConta)lu["TipoConta"],
                            PlanoContaNome = lu["PlanoContaNome"].ToString(),
                            Expand = false,
                        };
                        ig.PlanoContas.Add(ipc);
                    };
                    if ((int)lu["PeriodoAtu"] != 0)
                    {
                        LinhaPeriodo pc = ipc.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                        if (pc == null)
                        {
                            pc = new()
                            {
                                PeriodoId = (int)lu["PeriodoId"],
                                DataInicio = (DateTime)lu["DataInicio"],
                            };
                            ipc.LinhaPeriodos.Add(pc);
                        }
                        pc.Valor += (double)lu["ValorPeriodo"];
                    }
                    ipc.ValorAnterior += (double)lu["ValorAnterior"];
                    ipc.ValorPeriodo += (double)lu["ValorPeriodo"];
                    ipc.ValorTotal = ipc.ValorAnterior + ipc.ValorPeriodo;
                }
                await lu.CloseAsync();
                LinhaPlanoGerencial t1 = new()
                {
                    PlanoGerencialId = 0,
                    PlanoGerencialNome = "Receitas",
                    PlanoContas = new List<LinhaPlanoConta>(),
                    TipoPlanoConta = TipoPlanoConta.Receita,
                    Expand = false,
                    ValorAnterior = r.Where(x => x.TipoPlanoConta == TipoPlanoConta.Receita).Sum(x => x.ValorAnterior)
                };
                var lgr = r.Where(x => x.TipoPlanoConta == TipoPlanoConta.Receita);
                foreach (var g in lgr)
                {
                    foreach (var pg in g.LinhaPeriodos)
                    {
                        LinhaPeriodo pt = t1.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == pg.PeriodoId);
                        if (pt == null)
                        {
                            pt = new()
                            {
                                PeriodoId = pg.PeriodoId,
                                DataInicio = pg.DataInicio
                            };
                            t1.LinhaPeriodos.Add(pt);
                        }
                        pt.Valor += pg.Valor;
                    }
                }
                t1.ValorPeriodo = t1.LinhaPeriodos.Sum(x => x.Valor);
                t1.ValorTotal = t1.ValorAnterior + t1.ValorPeriodo;
                LinhaPlanoGerencial t2 = new()
                {
                    PlanoGerencialId = 0,
                    PlanoGerencialNome = "Despesas",
                    PlanoContas = new(),
                    TipoPlanoConta = TipoPlanoConta.Despesa,
                    Expand = false,
                    ValorAnterior = r.Where(x => x.TipoPlanoConta == TipoPlanoConta.Despesa).Sum(x => x.ValorAnterior)
                };
                var lgd = r.Where(x => x.TipoPlanoConta == TipoPlanoConta.Despesa);
                foreach (var g in lgd)
                {
                    foreach (var pg in g.LinhaPeriodos)
                    {
                        LinhaPeriodo pt = t2.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == pg.PeriodoId);
                        if (pt == null)
                        {
                            pt = new()
                            {
                                PeriodoId = pg.PeriodoId,
                                DataInicio = pg.DataInicio
                            };
                            t2.LinhaPeriodos.Add(pt);
                        }
                        pt.Valor += pg.Valor;
                    }
                }
                t2.ValorPeriodo = t2.LinhaPeriodos.Sum(x => x.Valor);
                t2.ValorTotal = t2.ValorAnterior + t2.ValorPeriodo;
                foreach (var pt2 in t2.LinhaPeriodos)
                {
                    LinhaPeriodo pt1 = t1.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == pt2.PeriodoId);
                    if (pt1 == null)
                    {
                        pt1 = new()
                        {
                            PeriodoId = pt2.PeriodoId,
                            DataInicio = pt2.DataInicio
                        };
                        t1.LinhaPeriodos.Add(pt1);
                    }
                }
                LinhaPlanoGerencial t3 = new()
                {
                    PlanoGerencialId = 99999999,
                    PlanoGerencialNome = "Resultado no período",
                    PlanoContas = new(),
                    TipoPlanoConta = TipoPlanoConta.Resultado,
                    Expand = false,
                    ValorAnterior = r.Where(x => x.TipoPlanoConta == TipoPlanoConta.Receita).Sum(x => x.ValorAnterior) - r.Where(x => x.TipoPlanoConta == TipoPlanoConta.Despesa).Sum(x => x.ValorAnterior)
                };

                foreach (var tg in t1.LinhaPeriodos)
                {
                    LinhaPeriodo pt = t3.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == tg.PeriodoId);
                    if (pt == null)
                    {
                        pt = new()
                        {
                            PeriodoId = tg.PeriodoId,
                            DataInicio = tg.DataInicio
                        };
                        t3.LinhaPeriodos.Add(pt);
                    }
                    pt.Valor += tg.Valor;
                }
                foreach (var tg in t2.LinhaPeriodos)
                {
                    LinhaPeriodo pt = t3.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == tg.PeriodoId);
                    if (pt == null)
                    {
                        pt = new()
                        {
                            PeriodoId = tg.PeriodoId,
                            DataInicio = tg.DataInicio
                        };
                        t3.LinhaPeriodos.Add(pt);
                    }
                    pt.Valor -= tg.Valor;
                }

                t3.ValorPeriodo = t3.LinhaPeriodos.Sum(x => x.Valor);
                t3.ValorTotal = t3.ValorAnterior + t3.ValorPeriodo;

                r.Add(t1);
                r.Add(t2);
                r.Add(t3);

                var laux = r.Where(x => x.TipoPlanoConta == TipoPlanoConta.Auxiliar).ToList();
                List<LinhaPlanoGerencial> lgfinal = new();
                foreach (var lgaux in laux)
                {
                    var gfinal = lgfinal.FirstOrDefault(x => x.NomeGrupoContaAuxiliar == lgaux.NomeGrupoContaAuxiliar);
                    if (gfinal == null)
                    {
                        gfinal = new()
                        {
                            PlanoGerencialId = 0,
                            NomeGrupoContaAuxiliar = lgaux.NomeGrupoContaAuxiliar,
                            PlanoGerencialNome = lgaux.NomeGrupoContaAuxiliar,
                            TipoPlanoConta = TipoPlanoConta.Auxiliar,
                            PlanoContas = new(),
                            Expand = false,
                            LinhaPeriodos = new()
                        };
                        lgfinal.Add(gfinal);
                    }
                    gfinal.ValorAnterior += lgaux.ValorAnterior;
                    gfinal.ValorPeriodo += lgaux.ValorPeriodo;
                    gfinal.ValorTotal += lgaux.ValorTotal;
                    foreach (var p in lgaux.LinhaPeriodos)
                    {
                        LinhaPeriodo fp = gfinal.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == p.PeriodoId);
                        if (fp == null)
                        {
                            fp = new()
                            {
                                PeriodoId = p.PeriodoId,
                                DataInicio = p.DataInicio,
                                Expand = false
                            };
                            gfinal.LinhaPeriodos.Add(fp);
                        }
                        fp.Valor += p.Valor;
                    }
                }
                if (lgfinal.Count > 0)
                {
                    r.AddRange(lgfinal);
                }

                LinhaPlanoGerencial tcaixa = new()
                {
                    PlanoGerencialId = 0,
                    PlanoGerencialNome = "Saldo em conta",
                    PlanoContas = new List<LinhaPlanoConta>(),
                    TipoPlanoConta = TipoPlanoConta.Caixa,
                    Expand = false
                };
                /*

                                var lcx = r.Where(x => x.TipoPlanoConta == TipoPlanoConta.Auxiliar && x.PlanoGerencialId == 0);
                                tcaixa.ValorAnterior = t3.ValorAnterior + lcx.Sum(x => x.ValorAnterior);
                                foreach (var pg in t3.LinhaPeriodos)
                                {
                                    LinhaPeriodo pt = tcaixa.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == pg.PeriodoId);
                                    if (pt == null)
                                    {
                                        pt = new()
                                        {
                                            PeriodoId = pg.PeriodoId,
                                            DataInicio = pg.DataInicio
                                        };
                                        tcaixa.LinhaPeriodos.Add(pt);
                                    }
                                    pt.Valor = pg.Valor;
                                }
                                foreach (var g in lcx)
                                {
                                    foreach (var pg in g.LinhaPeriodos)
                                    {
                                        LinhaPeriodo pt = tcaixa.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == pg.PeriodoId);
                                        if (pt == null)
                                        {
                                            pt = new()
                                            {
                                                PeriodoId = pg.PeriodoId,
                                                DataInicio = pg.DataInicio
                                            };
                                            tcaixa.LinhaPeriodos.Add(pt);
                                        }
                                        pt.Valor += pg.Valor;
                                    }
                                }
                                tcaixa.LinhaPeriodos = tcaixa.LinhaPeriodos.OrderBy(x => x.DataInicio).ToList();
                                double vac = tcaixa.ValorAnterior;
                                tcaixa.ValorTotal = tcaixa.ValorAnterior;
                                foreach (var pg in tcaixa.LinhaPeriodos)
                                {
                                    pg.Valor += vac;
                                    vac = pg.Valor;
                                    tcaixa.ValorTotal = pg.Valor;
                                }
                */
                tcaixa.ContasCorrentes = await LoadDadosContaCorrenteByDate(empid, dtini, dtfim, empsocioid);
                if (tcaixa.ContasCorrentes != null)
                {
                    r.Add(tcaixa);
                }
                r = r.OrderBy(x => x.TipoPlanoConta).ThenBy(x => x.NomeGrupoContaAuxiliar).ThenBy(x => x.PlanoGerencialId).ThenBy(x => x.PlanoGerencialNome).ToList();
            }
            finally
            {
                await command.Connection.CloseAsync();
            }
            return r;
        }
        public async Task<List<LinhaSocio>> LoadDadosSocioPlanoGerencialByDate(int empid, DateTime dtini, DateTime dtfim, bool fechamento = false)
        {
            SessionInfo si = await GetSessionAsync();
            List<LinhaSocio> r = new();


            var lsoc = await (from Socio in _context.Socio
                              join EmpreendimentoSocio in _context.EmpreendimentoSocio on Socio.Id equals EmpreendimentoSocio.SocioId
                              select new { Socio, EmpreendimentoSocio }
                          ).Where(x => x.EmpreendimentoSocio.EmpreendimentoId == empid && x.EmpreendimentoSocio.TenantId == si.TenantId).ToListAsync();

            foreach (var soc in lsoc)
            {
                LinhaSocio rsoc = new()
                {
                    EmpreendimentoSocioId = soc.EmpreendimentoSocio.Id,
                    SocioNome = soc.EmpreendimentoSocio.Nome,
                    LinhasGerencial = new(),
                    TipoSocio = soc.EmpreendimentoSocio.TipoSocio,
                    Expand = false,
                    ShowMe = true,
                    ShowMeDemonstrativo = true
                };
                r.Add(rsoc);
            }

            if (!fechamento)
            {
                var lgs = await (from GrupoSocio in _context.GrupoSocio
                                 join GrupoSocioEmpreedSocio in _context.GrupoSocioEmpreedSocio on GrupoSocio.Id equals GrupoSocioEmpreedSocio.GrupoSocioId
                                 select new { GrupoSocio, GrupoSocioEmpreedSocio.EmpreendimentoSocioId }
                          ).AsNoTracking().Where(x => x.GrupoSocio.EmpreendimentoId == empid && x.GrupoSocio.UtilizarGerencial == true
                                   && x.GrupoSocio.TenantId == si.TenantId
                                    ).ToListAsync();

                if (lgs.Count > 0)
                {
                    foreach (var so in r)
                    {
                        var g = lgs.FirstOrDefault(x => x.EmpreendimentoSocioId == so.EmpreendimentoSocioId);
                        if (g != null)
                        {
                            so.SocioNomePrincipal = g.GrupoSocio.Nome;
                            so.SocioNomePrincipalId = g.GrupoSocio.Id;
                            so.ShowMe = false;
                            so.ShowMeDemonstrativo = false;
                        }
                        else
                        {
                            so.ShowMe = true;
                            so.ShowMeDemonstrativo = true;
                        }
                    }
                    foreach (var gs in lgs)
                    {
                        if (!r.Any(x => x.SocioNomePrincipalId == gs.GrupoSocio.Id && x.EmpreendimentoSocioId == 0))
                        {
                            LinhaSocio rsoc = new()
                            {
                                EmpreendimentoSocioId = 0,
                                SocioNomePrincipalId = gs.GrupoSocio.Id,
                                SocioNome = " " + gs.GrupoSocio.Nome,
                                SocioNomePrincipal = gs.GrupoSocio.Nome,
                                LinhasGerencial = new(),
                                TipoSocio = TipoSocio.none,
                                Expand = false,
                                ExpandResult = false,
                                ExpandDemonstrativo = false,
                                ShowMeDemonstrativo = true,
                                ShowMe = true
                            };
                            r.Add(rsoc);
                        }
                    }
                }
            }

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
        "            select " +
        "Periodo.Id PeriodoId, " +
        "Max(Periodo.DataInicio) DataInicio, " +
        "EmpreendimentoSocio.Id EmpreendimentoSocioId, " +
        "max (EmpreendimentoSocio.TipoSocio) TipoSocio, " +
        "EmpreendimentoSocio.Nome SocioNome, " +
        "PlanoGerencial.AporteDistribuicao, " +
        "PlanoConta.Tipo TipoConta, " +
        "PlanoConta.Id PlanoContaId, " +
        "Isnull(PlanoConta.NomeCurto,PlanoConta.Nome) PlanoContaNome, " +
        "PlanoGerencial.Id PlanoGerencialId, " +
        "PlanoGerencial.Nome PlanoGerencialNome, " +
        "max( " +
        "case when ISNULL(PeriodoAtu.Id, 0) = Periodo.Id then " +
        "Periodo.Id " +
        "else " +
        "      0   end )  PeriodoAtu, " +
        "sum( " +
        "case when ISNULL(PeriodoAtu.Id, 0) = LanctoEmpRateadoPlanoContaSocio.PeriodoId then " +
        "0.0 " +
        "else " +
        "                LanctoEmpRateadoPlanoContaSocio.Valor end )  ValorAnterior, " +
        "sum( " +
        "case when ISNULL(PeriodoAtu.Id, 0) = LanctoEmpRateadoPlanoContaSocio.PeriodoId then " +
        "LanctoEmpRateadoPlanoContaSocio.Valor " +
        "else " +
        "                0 end )  ValorPeriodo " +
        "                from LanctoEmpRateadoPlanoContaSocio " +
        "                inner join PlanoConta on PlanoConta.Id = LanctoEmpRateadoPlanoContaSocio.PlanoContaId " +
        "inner join PlanoGerencial on PlanoGerencial.Id = PlanoConta.PlanoGerencialId " +
        "inner join Periodo on Periodo.Id = LanctoEmpRateadoPlanoContaSocio.PeriodoId and Periodo.EmpreendimentoId = @empid " +
        "inner join EmpreendimentoSocio on EmpreendimentoSocio.Id = LanctoEmpRateadoPlanoContaSocio.EmpreendimentoSocioId " +
        "inner join Socio on Socio.Id = EmpreendimentoSocio.SocioId " +
        "left join Periodo PeriodoAtu on PeriodoAtu.DataInicio >= @dtini and PeriodoAtu.DataFim <= @dtfim  and LanctoEmpRateadoPlanoContaSocio.PeriodoId = PeriodoAtu.Id " +
        "where Periodo.DataFim <=  Convert(date, @dtfim2, 112) and PlanoGerencial.AporteDistribuicao = @ad  and LanctoEmpRateadoPlanoContaSocio.TenantId = @tid" +
        " group by " +
        "Periodo.Id , " +
        "EmpreendimentoSocio.Id,  " +
        "EmpreendimentoSocio.Nome , " +
        "PlanoConta.Tipo , " +
        "PlanoConta.Id , " +
        "Isnull(PlanoConta.NomeCurto,PlanoConta.Nome) , " +
        "PlanoGerencial.Id , " +
        "PlanoGerencial.Nome, " +
        "PlanoGerencial.AporteDistribuicao " +
        "order by Periodo.Id , EmpreendimentoSocio.Nome,  PlanoConta.Tipo, PlanoGerencial.Nome,Isnull(PlanoConta.NomeCurto,PlanoConta.Nome) ";


            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = empid;
            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@ad", System.Data.SqlDbType.Bit));
            command.Parameters["@ad"].Value = false;
            command.Parameters.Add(new SqlParameter("@dtini", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim"].Value = dtfim.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim2"].Value = dtfim.ToString("yyyyMMdd");


            await command.Connection.OpenAsync();
            var lu = await command.ExecuteReaderAsync();
            try
            {
                while (await lu.ReadAsync())
                {
                    LinhaSocio iso = r.FirstOrDefault(x => x.EmpreendimentoSocioId == (int)lu["EmpreendimentoSocioId"]);
                    if (iso == null)
                    {
                        iso = new()
                        {
                            EmpreendimentoSocioId = (int)lu["EmpreendimentoSocioId"],
                            SocioNome = lu["SocioNome"].ToString(),
                            LinhasGerencial = new(),
                            TipoSocio = (TipoSocio)lu["TipoSocio"],
                            Expand = false
                        };
                        r.Add(iso);
                    }
                    if ((int)lu["PeriodoAtu"] != 0)
                    {
                        LinhaPeriodo ps = iso.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                        if (ps == null)
                        {
                            ps = new()
                            {
                                PeriodoId = (int)lu["PeriodoId"],
                                DataInicio = (DateTime)lu["DataInicio"],
                            };
                            iso.LinhaPeriodos.Add(ps);
                        }
                        if ((TipoPlanoConta)lu["TipoConta"] == TipoPlanoConta.Receita)
                        {
                            ps.ValorReceita += (double)lu["ValorPeriodo"];
                            iso.ValorAnteriorReceita += (double)lu["ValorAnterior"];
                        }
                        else
                        {
                            if ((TipoPlanoConta)lu["TipoConta"] == TipoPlanoConta.Despesa)
                            {
                                ps.ValorDespesa += (double)lu["ValorPeriodo"];
                                iso.ValorAnteriorDespesa += (double)lu["ValorAnterior"];
                            }
                        }
                        ps.ValorResultado = ps.ValorReceita - ps.ValorDespesa;

                        if (iso.SocioNomePrincipalId != 0)
                        {
                            LinhaSocio tiso = r.FirstOrDefault(x => x.EmpreendimentoSocioId == 0 && x.SocioNomePrincipalId == iso.SocioNomePrincipalId);
                            LinhaPeriodo tps = tiso.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                            if (tps == null)
                            {
                                tps = new()
                                {
                                    PeriodoId = (int)lu["PeriodoId"],
                                    DataInicio = (DateTime)lu["DataInicio"],
                                };
                                tiso.LinhaPeriodos.Add(tps);
                            }
                            if ((TipoPlanoConta)lu["TipoConta"] == TipoPlanoConta.Receita)
                            {
                                tps.ValorReceita += (double)lu["ValorPeriodo"];
                                tiso.ValorAnteriorReceita += (double)lu["ValorAnterior"];
                            }
                            else
                            {
                                if ((TipoPlanoConta)lu["TipoConta"] == TipoPlanoConta.Despesa)
                                {
                                    tps.ValorDespesa += (double)lu["ValorPeriodo"];
                                    tiso.ValorAnteriorDespesa += (double)lu["ValorAnterior"];
                                }
                            }
                            tps.ValorResultado = tps.ValorReceita - tps.ValorDespesa;
                        }

                    }
                    if ((TipoPlanoConta)lu["TipoConta"] == TipoPlanoConta.Receita)
                    {
                        iso.ValorAnteriorReceita += (double)lu["ValorAnterior"];
                    }
                    else
                    {
                        if ((TipoPlanoConta)lu["TipoConta"] == TipoPlanoConta.Despesa)
                        {
                            iso.ValorAnteriorDespesa += (double)lu["ValorAnterior"];
                        }
                    }
                    iso.ValorAnteriorResultado = iso.ValorAnteriorReceita - iso.ValorAnteriorDespesa;
                    iso.ValorTotalReceita = iso.ValorAnteriorReceita + iso.LinhaPeriodos.Sum(x => x.ValorReceita);
                    iso.ValorTotalDespesa = iso.ValorAnteriorDespesa + iso.LinhaPeriodos.Sum(x => x.ValorDespesa);
                    iso.ValorTotalResultado = iso.ValorTotalReceita - iso.ValorTotalDespesa;

                    if (iso.SocioNomePrincipalId != 0)
                    {
                        LinhaSocio tiso = r.FirstOrDefault(x => x.EmpreendimentoSocioId == 0 && x.SocioNomePrincipalId == iso.SocioNomePrincipalId);
                        if ((TipoPlanoConta)lu["TipoConta"] == TipoPlanoConta.Receita)
                        {
                            tiso.ValorAnteriorReceita += (double)lu["ValorAnterior"];
                        }
                        else
                        {
                            if ((TipoPlanoConta)lu["TipoConta"] == TipoPlanoConta.Despesa)
                            {
                                tiso.ValorAnteriorDespesa += (double)lu["ValorAnterior"];
                            }
                        }
                        tiso.ValorAnteriorResultado = tiso.ValorAnteriorReceita - tiso.ValorAnteriorDespesa;
                        tiso.ValorTotalReceita = tiso.ValorAnteriorReceita + tiso.LinhaPeriodos.Sum(x => x.ValorReceita);
                        tiso.ValorTotalDespesa = tiso.ValorAnteriorDespesa + tiso.LinhaPeriodos.Sum(x => x.ValorDespesa);
                        tiso.ValorTotalResultado = tiso.ValorTotalReceita - tiso.ValorTotalDespesa;
                    }

                    LinhaPlanoGerencial ig = iso.LinhasGerencial.FirstOrDefault(x => x.PlanoGerencialId == (int)lu["PlanoGerencialId"]);
                    if (ig == null)
                    {
                        ig = new()
                        {
                            PlanoGerencialId = (int)lu["PlanoGerencialId"],
                            PlanoGerencialNome = lu["PlanoGerencialNome"].ToString(),
                            PlanoContas = new(),
                            TipoPlanoConta = (TipoPlanoConta)lu["TipoConta"],
                            Expand = false
                        };
                        iso.LinhasGerencial.Add(ig);
                    }
                    if ((int)lu["PeriodoAtu"] != 0)
                    {
                        LinhaPeriodo pg = ig.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                        if (pg == null)
                        {
                            pg = new()
                            {
                                PeriodoId = (int)lu["PeriodoId"],
                                DataInicio = (DateTime)lu["DataInicio"],
                            };
                            ig.LinhaPeriodos.Add(pg);
                        }
                        pg.Valor += (double)lu["ValorPeriodo"];
                    }
                    ig.ValorPeriodo += (double)lu["ValorPeriodo"];
                    ig.ValorAnterior += (double)lu["ValorAnterior"];
                    ig.ValorTotal = ig.ValorAnterior + ig.ValorPeriodo;
                    LinhaPlanoConta ipc = ig.PlanoContas.FirstOrDefault(x => x.PlanoContaId == (int)lu["PlanoContaId"]);
                    if (ipc == null)
                    {
                        ipc = new()
                        {
                            PlanoContaId = (int)lu["PlanoContaId"],
                            TipoPlanoConta = (TipoPlanoConta)lu["TipoConta"],
                            PlanoContaNome = lu["PlanoContaNome"].ToString(),
                            Expand = false,
                            ValorAnterior = (double)lu["ValorAnterior"],
                            ValorPeriodo = 0,
                            ValorTotal = (double)lu["ValorAnterior"] + (double)lu["ValorPeriodo"]
                        };
                        ig.PlanoContas.Add(ipc);
                    };
                    if ((int)lu["PeriodoAtu"] != 0)
                    {
                        LinhaPeriodo pc = ipc.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                        if (pc == null)
                        {
                            pc = new()
                            {
                                PeriodoId = (int)lu["PeriodoId"],
                                DataInicio = (DateTime)lu["DataInicio"],
                            };
                            ipc.LinhaPeriodos.Add(pc);
                        }
                        pc.Valor += (double)lu["ValorPeriodo"];
                    }
                    ipc.ValorPeriodo += (double)lu["ValorPeriodo"];
                    ipc.ValorAnterior += (double)lu["ValorAnterior"];
                    ipc.ValorTotal = ipc.ValorAnterior + ipc.ValorPeriodo;
                }
                foreach (var so in r)
                {
                    so.LinhasReceitas = so.LinhasGerencial.Where(x => x.TipoPlanoConta == TipoPlanoConta.Receita).ToList();
                    so.LinhasDespesas = so.LinhasGerencial.Where(x => x.TipoPlanoConta == TipoPlanoConta.Despesa).ToList();
                }

            }
            finally
            {
                await command.Connection.CloseAsync();
            }

            foreach (var so in r)
            {
                so.LinhaPeriodos = so.LinhaPeriodos.OrderBy(x => x.DataInicio).ToList();
            }

            r = await LoadDadosSocioDistribuicaoByDate(empid, dtini, dtfim, r);
            r = await LoadDadosSocioCorrecaoDistribuicaoByDate(empid, dtini, dtfim, r);
            r = await LoadDadosSocioRetencaoByDate(empid, dtini, dtfim, r);
            r = await LoadDadosSocioAporteByDate(empid, dtini, dtfim, r);
            r = await LoadDadosSocioEmprestimoByDate(empid, dtini, dtfim, r);
            r = CalcularSaldoDistribuir(r);
            r = r.OrderBy(x => x.SocioNomePrincipal).ThenBy(x => x.TipoSocio).ThenBy(x => x.SocioNome).ToList();
            return r;
        }
        public List<LinhaSocio> CalcularSaldoDistribuir(List<LinhaSocio> lso)
        {
            foreach (var so in lso)
            {

                so.LinhaSaldoDistribuir.ValorAnterior = Math.Round(so.LinhaAporte.ValorAnterior + so.ValorAnteriorReceita - so.ValorAnteriorDespesa - so.LinhaDistribuicao.ValorAnterior, 2)
                    + so.LinhaCorrecaoDistruibuicao.ValorAnterior;
                so.LinhaSaldoDistribuir.ValorTotal = so.LinhaSaldoDistribuir.ValorAnterior;
                foreach (var ps in so.LinhaPeriodos)
                {
                    LinhaPeriodo sdp = new()
                    {
                        PeriodoId = ps.PeriodoId,
                        DataInicio = ps.DataInicio
                    };
                    so.LinhaSaldoDistribuir.LinhaPeriodos.Add(sdp);
                    //    var rp = so.LinhaRetencao.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == ps.PeriodoId);
                    var dp = so.LinhaDistribuicao.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == ps.PeriodoId);
                    var cp = so.LinhaCorrecaoDistruibuicao.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == ps.PeriodoId);
                    var ap = so.LinhaAporte.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == ps.PeriodoId);

                    sdp.Valor = so.LinhaSaldoDistribuir.ValorTotal + ps.ValorResultado;
                    if (ap != null)
                    {
                        sdp.Valor += ap.Valor;
                    }
                    /*
                                        if (rp != null)
                                        {
                                            sdp.Valor -= rp.Valor;
                                        }
                      */
                    if (dp != null)
                    {
                        sdp.Valor -= dp.Valor;
                    }
                    if (cp != null)
                    {
                        sdp.Valor += cp.Valor;
                    }

                    sdp.Valor = Math.Round(sdp.Valor, 2);
                    so.LinhaSaldoDistribuir.ValorTotal = sdp.Valor;
                }
                //                so.LinhaSaldoCapital.ValorAnterior = so.LinhaAporte.ValorAnterior + so.LinhaSaldoDistribuir.ValorAnterior;
                so.LinhaSaldoCapital.ValorAnterior = so.LinhaAporte.ValorAnterior;
                for (int i = 0; i < so.LinhaPeriodos.Count; i++)
                {
                    var ps = so.LinhaPeriodos[i];
                    LinhaPeriodo sdc = new()
                    {
                        PeriodoId = ps.PeriodoId,
                        DataInicio = ps.DataInicio
                    };
                    so.LinhaSaldoCapital.LinhaPeriodos.Add(sdc);
                    if (i == 0)
                    {
                        sdc.Valor = so.LinhaSaldoDistribuir.ValorAnterior;
                        sdc.ValorResultado = so.LinhaSaldoDistribuir.ValorAnterior; ;
                    }
                    else
                    {
                        var sd = so.LinhaSaldoDistribuir.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == so.LinhaPeriodos[i - 1].PeriodoId);
                        if (sd != null)
                        {
                            sdc.Valor = sd.Valor;
                            sdc.ValorResultado = sd.Valor;
                        }
                    }
                    var rp = so.LinhaAporte.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == ps.PeriodoId);
                    if (rp != null)
                    {
                        sdc.Valor += rp.Valor;
                    }
                }
                // so.LinhaSaldoDistribuir.ValorAnterior é sempre zero  , 
                //        so.LinhaSaldoDistribuir.ValorAnterior = 0;
                //        so.LinhaSaldoCapital.ValorTotal = so.LinhaAporte.ValorTotal + so.LinhaSaldoDistribuir.ValorAnterior;
                so.LinhaSaldoCapital.ValorTotal = so.LinhaAporte.ValorTotal;

                so.LinhaResultado.ValorAnterior = so.LinhaSaldoCapital.ValorAnterior + so.ValorAnteriorReceita - so.ValorAnteriorDespesa;
                so.LinhaResultado.ValorTotal = so.LinhaSaldoCapital.ValorTotal + so.ValorTotalReceita - so.ValorTotalDespesa;
                foreach (var ps in so.LinhaPeriodos)
                {
                    LinhaPeriodo sdp = new()
                    {
                        PeriodoId = ps.PeriodoId,
                        DataInicio = ps.DataInicio
                    };
                    so.LinhaResultado.LinhaPeriodos.Add(sdp);
                    var sdc = so.LinhaSaldoCapital.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == ps.PeriodoId);
                    sdp.Valor = ps.ValorReceita - ps.ValorDespesa;
                    if (sdc != null)
                    {
                        sdp.Valor += sdc.Valor;
                    }
                }

            }
            return lso;
        }
        public async Task<List<LanctoEmpreendimento>> LoadPlanoContaLanctoByData(LinhaPlanoConta plano, int empid, DateTime dtini, DateTime dtfim)
        {
            SessionInfo si = await GetSessionAsync();
            List<LanctoEmpreendimento> r = new();


            var l = await (from LanctoEmpreendimento in _context.LanctoEmpreendimento
                           join Periodo in _context.Periodo on LanctoEmpreendimento.PeriodoId equals Periodo.Id
                           join PlanoConta in _context.PlanoConta on LanctoEmpreendimento.PlanoContaId equals PlanoConta.Id
                           join LanctoEmpRelacionamento in _context.LanctoEmpRelacionamento on LanctoEmpreendimento.Id equals LanctoEmpRelacionamento.LanctoEmpreendimentoId into lr
                           from lr1 in lr.DefaultIfEmpty()
                           join Relacionamento in _context.Relacionamento on lr1.RelacionamentoId equals Relacionamento.Id into rela
                           from rela1 in rela.DefaultIfEmpty()
                           join LanctoEmpUnidade in _context.LanctoEmpUnidade on LanctoEmpreendimento.Id equals LanctoEmpUnidade.LanctoEmpreendimentoId into lu
                           from lu1 in lu.DefaultIfEmpty()
                           join UnidadeEmpreendimento in _context.UnidadeEmpreendimento on lu1.UnidadeEmpreendimentoId equals UnidadeEmpreendimento.Id into un
                           from u in un.DefaultIfEmpty()
                           select new { LanctoEmpreendimento, Periodo, lr1, rela1, u, TipoPlanoConta = PlanoConta.Tipo }).AsNoTracking().Where(x => x.LanctoEmpreendimento.DataMovto >= dtini
                            && x.LanctoEmpreendimento.DataMovto <= dtfim && x.LanctoEmpreendimento.PlanoContaId == plano.PlanoContaId
                            && x.Periodo.EmpreendimentoId == empid && x.LanctoEmpreendimento.TenantId == si.TenantId).ToListAsync();
            foreach (var it in l)
            {
                if (it.rela1 != null)
                {
                    it.LanctoEmpreendimento.LanctoEmpRelacionamento = new() { LanctoEmpreendimentoId = it.LanctoEmpreendimento.Id };
                    it.LanctoEmpreendimento.LanctoEmpRelacionamento.RelacionamentoNome = it.rela1.Nome;
                    it.LanctoEmpreendimento.LanctoEmpRelacionamento.RelacionamentoId = it.rela1.Id;
                }
                else
                {
                    it.LanctoEmpreendimento.LanctoEmpRelacionamento = new() { LanctoEmpreendimentoId = it.LanctoEmpreendimento.Id };
                    it.LanctoEmpreendimento.LanctoEmpRelacionamento.RelacionamentoNome = string.Empty;
                    it.LanctoEmpreendimento.LanctoEmpRelacionamento.RelacionamentoId = 0;
                }
                if (it.u != null)
                {
                    it.LanctoEmpreendimento.LanctoEmpUnidade = new() { LanctoEmpreendimentoId = it.LanctoEmpreendimento.Id };
                    it.LanctoEmpreendimento.LanctoEmpUnidade.UnidadeEmpreendimento = new() { CodigoExterno = it.u.CodigoExterno, Id = it.u.Id };
                    it.LanctoEmpreendimento.LanctoEmpUnidade.UnidadeEmpreendimentoId = it.u.Id;
                }
                else
                {
                    it.LanctoEmpreendimento.LanctoEmpUnidade = new() { LanctoEmpreendimentoId = it.LanctoEmpreendimento.Id };
                    it.LanctoEmpreendimento.LanctoEmpUnidade.UnidadeEmpreendimento = new() { CodigoExterno = string.Empty, Id = 0 };
                }
                if ((it.LanctoEmpreendimento.Tipo == TipoLancto.Despesa && it.TipoPlanoConta == TipoPlanoConta.Receita) ||
                    (it.LanctoEmpreendimento.Tipo == TipoLancto.Receita && it.TipoPlanoConta == TipoPlanoConta.Despesa) ||
                     (it.LanctoEmpreendimento.Tipo == TipoLancto.Despesa && it.TipoPlanoConta == TipoPlanoConta.Auxiliar))
                    it.LanctoEmpreendimento.Valor *= -1;
                r.Add(it.LanctoEmpreendimento);
            }
            return r;
        }
        public async Task<List<LinhaSocio>> LoadPlanoContaRateioByDate(LinhaPlanoConta plano, int empid, DateTime dtini, DateTime dtfim)
        {
            SessionInfo si = await GetSessionAsync();
            List<LinhaSocio> r = new();
            var l = await (from LanctoEmpRateadoPlanoContaSocio in _context.LanctoEmpRateadoPlanoContaSocio
                           join Periodo in _context.Periodo on LanctoEmpRateadoPlanoContaSocio.PeriodoId equals Periodo.Id
                           join GrupoRateio in _context.GrupoRateio on LanctoEmpRateadoPlanoContaSocio.GrupoRateioId equals GrupoRateio.Id
                           join EmpreendimentoSocio in _context.EmpreendimentoSocio on LanctoEmpRateadoPlanoContaSocio.EmpreendimentoSocioId equals EmpreendimentoSocio.Id
                           join Socio in _context.Socio on EmpreendimentoSocio.SocioId equals Socio.Id
                           select new { LanctoEmpRateadoPlanoContaSocio, GrupoRateio, Socio, EmpreendimentoSocio, Periodo }
                          ).AsNoTracking().Where(x => x.Periodo.EmpreendimentoId == empid && x.LanctoEmpRateadoPlanoContaSocio.PlanoContaId == plano.PlanoContaId
                                    && x.Periodo.DataFim >= dtini && x.Periodo.DataFim <= dtfim
                                    && x.LanctoEmpRateadoPlanoContaSocio.TenantId == si.TenantId
                                    ).ToListAsync();

            foreach (var it in l)
            {
                LinhaSocio so = r.FirstOrDefault(x => x.EmpreendimentoSocioId == it.LanctoEmpRateadoPlanoContaSocio.EmpreendimentoSocioId && x.ProgramacaoGrupoRateioId == it.LanctoEmpRateadoPlanoContaSocio.ProgramacaoGrupoRateioId);
                if (so == null)
                {
                    so = new LinhaSocio()
                    {
                        EmpreendimentoSocioId = it.LanctoEmpRateadoPlanoContaSocio.EmpreendimentoSocioId,
                        Expand = false,
                        SocioNome = it.EmpreendimentoSocio.Nome,
                        ProgramacaoGrupoRateioId = it.LanctoEmpRateadoPlanoContaSocio.ProgramacaoGrupoRateioId
                    };
                    so.SocioNomePrincipalId = await GetSocioPrincipalIdBySocioEmp(empid, so.EmpreendimentoSocioId);
                    r.Add(so);
                }
                LinhaPeriodo sop = so.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == it.Periodo.Id);
                if (sop == null)
                {
                    sop = new LinhaPeriodo()
                    {
                        PeriodoId = it.Periodo.Id
                    };
                    so.LinhaPeriodos.Add(sop);
                }
                sop.Valor += it.LanctoEmpRateadoPlanoContaSocio.Valor;
                sop.Percentual = it.LanctoEmpRateadoPlanoContaSocio.Percentual;
            }

            return r.OrderBy(x => x.SocioNome).ToList();
        }
        public async Task<List<LinhaSocio>> LoadDadosSocioDistribuicaoByDate(int empid, DateTime dtini, DateTime dtfim, List<LinhaSocio> ls = null)
        {
            SessionInfo si = await GetSessionAsync();
            List<LinhaSocio> r = null;
            if (ls != null)
            {
                r = ls;
            }
            else
            {
                r = new List<LinhaSocio>();
            }


            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
        "            select " +
        "Periodo.Id PeriodoId, " +
        "Periodo.DataInicio DataInicio, " +
        "EmpreendimentoSocio.Id EmpreendimentoSocioId, " +
        "EmpreendimentoSocio.Nome SocioNome, " +
        "( " +
        "case when SocioRetirada.DataLiberacao >= @dtini1 then " +
        "0.0 " +
        "else " +
        "                SocioRetirada.Valor end )  ValorAnterior, " +
        "( " +
        "case when  SocioRetirada.DataLiberacao >= @dtini2 then " +
        "SocioRetirada.Valor " +
        "else " +
        "                0 end )  ValorPeriodo " +
        "                from SocioRetirada " +
        "inner join EmpreendimentoSocio on EmpreendimentoSocio.Id = SocioRetirada.EmpreendimentoSocioId " +
        "inner join Empreendimento on Empreendimento.Id = EmpreendimentoSocio.EmpreendimentoId and Empreendimento.Id = @empid  " +
        "inner join Periodo on Periodo.DataInicio <= SocioRetirada.DataLiberacao and Periodo.DataFim >= SocioRetirada.DataLiberacao  and Periodo.EmpreendimentoId = Empreendimento.Id   " +
        "inner join Socio on Socio.Id = EmpreendimentoSocio.SocioId  " +
        "where SocioRetirada.DataLiberacao <=  Convert(date, @dtfim, 112) and SocioRetirada.TenantId = @tid  " +
        "order by   EmpreendimentoSocio.Nome  ";


            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = empid;
            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@dtini1", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini1"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtini2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini2"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim"].Value = dtfim.ToString("yyyyMMdd");

            await command.Connection.OpenAsync();
            var lu = await command.ExecuteReaderAsync();
            try
            {
                while (await lu.ReadAsync())
                {
                    LinhaSocio iso = r.FirstOrDefault(x => x.EmpreendimentoSocioId == (int)lu["EmpreendimentoSocioId"]);
                    if (iso == null)
                    {
                        iso = new LinhaSocio()
                        {
                            EmpreendimentoSocioId = (int)lu["EmpreendimentoSocioId"],
                            SocioNome = lu["SocioNome"].ToString(),
                            LinhaDistribuicao = new LinhaDistribuicao(),
                            Expand = false
                        };
                        r.Add(iso);
                    }
                    else
                    {
                        if (iso.LinhaDistribuicao == null)
                        {
                            iso.LinhaDistribuicao = new LinhaDistribuicao();
                        }
                    }
                    LinhaPeriodo ps = iso.LinhaDistribuicao.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                    if (ps == null)
                    {
                        ps = new LinhaPeriodo()
                        {
                            PeriodoId = (int)lu["PeriodoId"],
                            DataInicio = (DateTime)lu["DataInicio"]
                        };
                        iso.LinhaDistribuicao.LinhaPeriodos.Add(ps);
                    }

                    ps.Valor += (double)lu["ValorPeriodo"];
                    iso.LinhaDistribuicao.ValorPeriodo += (double)lu["ValorPeriodo"];
                    iso.LinhaDistribuicao.ValorAnterior += (double)lu["ValorAnterior"];
                    iso.LinhaDistribuicao.ValorTotal = iso.LinhaDistribuicao.ValorAnterior + iso.LinhaDistribuicao.ValorPeriodo;

                    if (iso.SocioNomePrincipalId != 0)
                    {
                        LinhaSocio tiso = r.FirstOrDefault(x => x.EmpreendimentoSocioId == 0 && x.SocioNomePrincipalId == iso.SocioNomePrincipalId);
                        if (tiso.LinhaDistribuicao == null)
                        {
                            tiso.LinhaDistribuicao = new();
                            tiso.LinhaDistribuicao.LinhaPeriodos = new();
                        }
                        LinhaPeriodo tps = tiso.LinhaDistribuicao.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                        if (tps == null)
                        {
                            tps = new LinhaPeriodo()
                            {
                                PeriodoId = (int)lu["PeriodoId"],
                                DataInicio = (DateTime)lu["DataInicio"]
                            };
                            tiso.LinhaDistribuicao.LinhaPeriodos.Add(tps);
                        }

                        tps.Valor += (double)lu["ValorPeriodo"];
                        tiso.LinhaDistribuicao.ValorPeriodo += (double)lu["ValorPeriodo"];
                        tiso.LinhaDistribuicao.ValorAnterior += (double)lu["ValorAnterior"];
                        tiso.LinhaDistribuicao.ValorTotal = tiso.LinhaDistribuicao.ValorAnterior + tiso.LinhaDistribuicao.ValorPeriodo;

                    }
                }
            }
            finally
            {
                await command.Connection.CloseAsync();
            }
            return r;
        }
        public async Task<List<LinhaSocio>> LoadDadosSocioCorrecaoDistribuicaoByDate(int empid, DateTime dtini, DateTime dtfim, List<LinhaSocio> ls = null)
        {
            SessionInfo si = await GetSessionAsync();
            List<LinhaSocio> r = null;
            if (ls != null)
            {
                r = ls;
            }
            else
            {
                r = new List<LinhaSocio>();
            }


            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
        "            select " +
        "Periodo.Id PeriodoId, " +
        "Periodo.DataInicio DataInicio, " +
        "EmpreendimentoSocio.Id EmpreendimentoSocioId, " +
        "EmpreendimentoSocio.Nome SocioNome, " +
        "( " +
        "case when SocioCorrecaoResultadoRetida.DataRef >= @dtini1 then " +
        "0.0 " +
        "else " +
        "                SocioCorrecaoResultadoRetida.Valor end )  ValorAnterior, " +
        "( " +
        "case when  SocioCorrecaoResultadoRetida.DataRef >= @dtini2 then " +
        "SocioCorrecaoResultadoRetida.Valor " +
        "else " +
        "                0 end )  ValorPeriodo " +
        "                from SocioCorrecaoResultadoRetida " +
        "inner join SocioResultadoPeriodo on SocioResultadoPeriodo.Id = SocioCorrecaoResultadoRetida.SocioResultadoPeriodoId " +
        "inner join EmpreendimentoSocio on EmpreendimentoSocio.Id = SocioResultadoPeriodo.EmpreendimentoSocioId " +
        "inner join Empreendimento on Empreendimento.Id = EmpreendimentoSocio.EmpreendimentoId and Empreendimento.Id = @empid  " +
        "inner join Periodo on Periodo.DataInicio <= SocioCorrecaoResultadoRetida.DataRef and Periodo.DataFim >= SocioCorrecaoResultadoRetida.DataRef  and Periodo.EmpreendimentoId = Empreendimento.Id   " +
        "inner join Socio on Socio.Id = EmpreendimentoSocio.SocioId  " +
        "where SocioCorrecaoResultadoRetida.DataRef <=  Convert(date, @dtfim, 112) and SocioCorrecaoResultadoRetida.TenantId = @tid  " +
        "order by   EmpreendimentoSocio.Nome  ";


            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = empid;
            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@dtini1", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini1"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtini2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini2"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim"].Value = dtfim.ToString("yyyyMMdd");

            await command.Connection.OpenAsync();
            var lu = await command.ExecuteReaderAsync();
            try
            {
                while (await lu.ReadAsync())
                {
                    LinhaSocio iso = r.FirstOrDefault(x => x.EmpreendimentoSocioId == (int)lu["EmpreendimentoSocioId"]);
                    if (iso == null)
                    {
                        iso = new LinhaSocio()
                        {
                            EmpreendimentoSocioId = (int)lu["EmpreendimentoSocioId"],
                            SocioNome = lu["SocioNome"].ToString(),
                            LinhaCorrecaoDistruibuicao = new LinhaCorrecaoDistruibuicao(),
                            Expand = false
                        };
                        r.Add(iso);
                    }
                    else
                    {
                        if (iso.LinhaCorrecaoDistruibuicao == null)
                        {
                            iso.LinhaCorrecaoDistruibuicao = new LinhaCorrecaoDistruibuicao();
                        }
                    }
                    LinhaPeriodo ps = iso.LinhaCorrecaoDistruibuicao.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                    if (ps == null)
                    {
                        ps = new LinhaPeriodo()
                        {
                            PeriodoId = (int)lu["PeriodoId"],
                            DataInicio = (DateTime)lu["DataInicio"]
                        };
                        iso.LinhaCorrecaoDistruibuicao.LinhaPeriodos.Add(ps);
                    }

                    ps.Valor += (double)lu["ValorPeriodo"];
                    iso.LinhaCorrecaoDistruibuicao.ValorPeriodo += (double)lu["ValorPeriodo"];
                    iso.LinhaCorrecaoDistruibuicao.ValorAnterior += (double)lu["ValorAnterior"];
                    iso.LinhaCorrecaoDistruibuicao.ValorTotal = iso.LinhaCorrecaoDistruibuicao.ValorAnterior + iso.LinhaCorrecaoDistruibuicao.ValorPeriodo;

                    if (iso.SocioNomePrincipalId != 0)
                    {
                        LinhaSocio tiso = r.FirstOrDefault(x => x.EmpreendimentoSocioId == 0 && x.SocioNomePrincipalId == iso.SocioNomePrincipalId);
                        if (tiso.LinhaCorrecaoDistruibuicao == null)
                        {
                            tiso.LinhaCorrecaoDistruibuicao = new();
                            tiso.LinhaCorrecaoDistruibuicao.LinhaPeriodos = new();
                        }
                        LinhaPeriodo tps = tiso.LinhaCorrecaoDistruibuicao.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                        if (tps == null)
                        {
                            tps = new LinhaPeriodo()
                            {
                                PeriodoId = (int)lu["PeriodoId"],
                                DataInicio = (DateTime)lu["DataInicio"]
                            };
                            tiso.LinhaCorrecaoDistruibuicao.LinhaPeriodos.Add(tps);
                        }

                        tps.Valor += (double)lu["ValorPeriodo"];
                        tiso.LinhaCorrecaoDistruibuicao.ValorPeriodo += (double)lu["ValorPeriodo"];
                        tiso.LinhaCorrecaoDistruibuicao.ValorAnterior += (double)lu["ValorAnterior"];
                        tiso.LinhaCorrecaoDistruibuicao.ValorTotal = tiso.LinhaCorrecaoDistruibuicao.ValorAnterior + tiso.LinhaCorrecaoDistruibuicao.ValorPeriodo;

                    }
                }
            }
            finally
            {
                await command.Connection.CloseAsync();
            }
            return r;
        }
        public async Task<List<LinhaSocio>> LoadDadosSocioRetencaoByDate(int empid, DateTime dtini, DateTime dtfim, List<LinhaSocio> ls = null)
        {
            SessionInfo si = await GetSessionAsync();
            List<LinhaSocio> r = null;
            if (ls != null)
            {
                r = ls;
            }
            else
            {
                r = new List<LinhaSocio>();
            }


            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
        "            select " +
        "Periodo.Id PeriodoId, " +
        "EmpreendimentoSocio.Id EmpreendimentoSocioId, " +
        "EmpreendimentoSocio.Nome SocioNome, " +
        "( " +
        "case when SocioRetirada.DataLiberacao >= @dtini1 then " +
        "0.0 " +
        "else " +
        "                SocioDebitoLancto.Valor end )  ValorAnterior, " +
        "( " +
        "case when  SocioRetirada.DataLiberacao >= @dtini2 then " +
        "SocioDebitoLancto.Valor " +
        "else " +
        "                0 end )  ValorPeriodo " +
        "                from SocioDebitoLancto " +
        "inner join SocioRetiradaDebitoLancto on SocioRetiradaDebitoLancto.SocioDebitoLanctoId = SocioDebitoLancto.Id  " +
        "inner join SocioRetirada on SocioRetirada.Id = SocioRetiradaDebitoLancto.SocioRetiradaId  " +
        "inner join EmpreendimentoSocio on EmpreendimentoSocio.Id = SocioRetirada.EmpreendimentoSocioId " +
        "inner join Empreendimento on Empreendimento.Id = EmpreendimentoSocio.EmpreendimentoId and Empreendimento.Id = @empid  " +
        "inner join Periodo on Periodo.DataInicio <= SocioRetirada.DataLiberacao and Periodo.DataFim >= SocioRetirada.DataLiberacao  and Periodo.EmpreendimentoId = Empreendimento.Id   " +
        "inner join Socio on Socio.Id = EmpreendimentoSocio.SocioId  " +
        "where SocioRetirada.DataLiberacao <=  Convert(date, @dtfim, 112) and SocioDebitoLancto.TenantId = @tid  " +
        "order by   EmpreendimentoSocio.Nome  ";


            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = empid;
            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@dtini1", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini1"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtini2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini2"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim"].Value = dtfim.ToString("yyyyMMdd");

            await command.Connection.OpenAsync();
            var lu = await command.ExecuteReaderAsync();
            try
            {
                while (await lu.ReadAsync())
                {
                    LinhaSocio iso = r.FirstOrDefault(x => x.EmpreendimentoSocioId == (int)lu["EmpreendimentoSocioId"]);
                    if (iso == null)
                    {
                        iso = new LinhaSocio()
                        {
                            EmpreendimentoSocioId = (int)lu["EmpreendimentoSocioId"],
                            SocioNome = lu["SocioNome"].ToString(),
                            LinhaRetencao = new LinhaRetencao(),
                            Expand = false
                        };
                        r.Add(iso);
                    }
                    else
                    {
                        if (iso.LinhaRetencao == null)
                        {
                            iso.LinhaRetencao = new LinhaRetencao();
                        }
                    }
                    LinhaPeriodo ps = iso.LinhaRetencao.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                    if (ps == null)
                    {
                        ps = new LinhaPeriodo()
                        {
                            PeriodoId = (int)lu["PeriodoId"]
                        };
                        iso.LinhaRetencao.LinhaPeriodos.Add(ps);
                    }

                    ps.Valor += (double)lu["ValorPeriodo"];
                    iso.LinhaRetencao.ValorPeriodo += (double)lu["ValorPeriodo"];
                    iso.LinhaRetencao.ValorAnterior += (double)lu["ValorAnterior"];
                    iso.LinhaRetencao.ValorTotal = iso.LinhaRetencao.ValorAnterior + iso.LinhaRetencao.ValorPeriodo;


                    if (iso.SocioNomePrincipalId != 0)
                    {
                        LinhaSocio tiso = r.FirstOrDefault(x => x.EmpreendimentoSocioId == 0 && x.SocioNomePrincipalId == iso.SocioNomePrincipalId);
                        if (tiso.LinhaRetencao == null)
                        {
                            tiso.LinhaRetencao = new();
                            tiso.LinhaRetencao.LinhaPeriodos = new();
                        }
                        LinhaPeriodo tps = tiso.LinhaRetencao.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                        if (tps == null)
                        {
                            tps = new LinhaPeriodo()
                            {
                                PeriodoId = (int)lu["PeriodoId"]
                            };
                            tiso.LinhaRetencao.LinhaPeriodos.Add(tps);
                        }

                        tps.Valor += (double)lu["ValorPeriodo"];
                        tiso.LinhaRetencao.ValorPeriodo += (double)lu["ValorPeriodo"];
                        tiso.LinhaRetencao.ValorAnterior += (double)lu["ValorAnterior"];
                        tiso.LinhaRetencao.ValorTotal = tiso.LinhaRetencao.ValorAnterior + tiso.LinhaRetencao.ValorPeriodo;
                    }
                }
            }
            finally
            {
                await command.Connection.CloseAsync();
            }
            return r;
        }
        public async Task<List<LinhaSocio>> LoadDadosSocioEmprestimoByDate(int empid, DateTime dtini, DateTime dtfim, List<LinhaSocio> ls = null)
        {
            SessionInfo si = await GetSessionAsync();
            List<LinhaSocio> r = null;
            if (ls != null)
            {
                r = ls;
            }
            else
            {
                r = new List<LinhaSocio>();
            }


            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
        "            select " +
        "(Periodo.Id) PeriodoId, " +
        "(Periodo.DataInicio) DataInicio, " +
        "(EmpreendimentoSocio.Id) EmpreendimentoSocioId, " +
        "EmpreendimentoSocio.Nome SocioNome, " +
        "( " +
        "(case when  Isnull(SocioDebito.DataLancto,'19100101')  >= @dtini1 then " +
        "0.0 " +
        "else " +
        "                Isnull(SocioDebito.Valor,0.0) end ))  ValorAnterior, " +
        "( " +
        "(case when   Isnull(SocioDebito.DataLancto,'19100101')  >= @dtini2 then " +
        "Isnull(SocioDebito.Valor,0.0) " +
        "else " +
        "                0 end ))  ValorPeriodo, " +

        "0.0  ValorAnteriorJuro, " +
        "0.0 ValorPeriodoJuro, " +
        "0.0 ValorAnteriorAmort, " +
        "0.0 ValorPeriodoAmort " +

        /*
                "                from SocioDebito " +
                "inner join EmpreendimentoSocio on EmpreendimentoSocio.Id = SocioDebito.EmpreendimentoSocioId " +
                "inner join Empreendimento on Empreendimento.Id = EmpreendimentoSocio.EmpreendimentoId and Empreendimento.Id = @empid  " +
                "inner join Periodo on Periodo.DataInicio <= SocioDebito.DataLancto and Periodo.DataFim >= SocioDebito.DataLancto  and Periodo.EmpreendimentoId = Empreendimento.Id   " +
                "inner join Socio on Socio.Id = EmpreendimentoSocio.SocioId  " +
                "where SocioDebito.DataLancto <=  Convert(date, @dtfim, 112) and SocioDebito.TenantId = @tid  " +
        */
        "                        from Periodo " +
        "        inner join Empreendimento on Empreendimento.Id = Periodo.EmpreendimentoId" +
        "        inner  join EmpreendimentoSocio on EmpreendimentoSocio.EmpreendimentoId = Empreendimento.Id " +
        "        inner  join Socio on Socio.Id = EmpreendimentoSocio.SocioId " +
        "        left   join SocioDebito on SocioDebito.EmpreendimentoSocioId = EmpreendimentoSocio.Id  and  SocioDebito.DataLancto >= Periodo.DataInicio and SocioDebito.DataLancto <= Periodo.DataFim " +
        "        where Periodo.DataFim <= Convert(date, @dtfim, 112) and Periodo.TenantId = @tid " +
        "        and Empreendimento.Id = @empid " +

        "union all " +
        "            select " +
        "(Periodo.Id) PeriodoId, " +
        "(Periodo.DataInicio) DataInicio, " +
        "(EmpreendimentoSocio.Id) EmpreendimentoSocioId, " +
        "EmpreendimentoSocio.Nome SocioNome, " +
        "0.0 ValorAnterior, " +
        "0.0 ValorPeriodo, " +
        "( " +
        "(case when Isnull(SocioDebitoLancto.DataLancto,getdate()-10000) >= @dtini1 then " +
        "0.0 " +
        "else " +
        " case when Isnull(SocioDebitoLancto.Valor,0) = 0 then 0.0 " +
        "       when Isnull(SocioDebitoLancto.TipoLanctoDebito,0) = 1 then Isnull(SocioDebitoLancto.Valor,0) " +
        "       when Isnull(SocioDebitoLancto.TipoLanctoDebito,0) = 2 then 0.0 end  " +
        "             end ))  ValorAnteriorJuro, " +
        "( " +
        "(case when Isnull(SocioDebitoLancto.DataLancto,getdate()-10000) < @dtini1 then " +
        "0.0 " +
        "else " +
        " case when Isnull(SocioDebitoLancto.Valor,0) = 0 then 0.0 " +
        "       when Isnull(SocioDebitoLancto.TipoLanctoDebito,0) = 1 then Isnull(SocioDebitoLancto.Valor,0) " +
        "       when Isnull(SocioDebitoLancto.TipoLanctoDebito,0) = 2 then 0.0 end  " +
        "             end ))  ValorPeriodoJuro, " +

        "( " +
        "(case when Isnull(SocioDebitoLancto.DataLancto,getdate()-10000) >= @dtini1 then " +
        "0.0 " +
        "else " +
        " case when Isnull(SocioDebitoLancto.Valor,0) = 0 then 0.0 " +
        "       when Isnull(SocioDebitoLancto.TipoLanctoDebito,0) = 2 then Isnull(SocioDebitoLancto.Valor,0) " +
        "       when Isnull(SocioDebitoLancto.TipoLanctoDebito,0) = 1 then 0.0 end  " +
        "             end ))  ValorAnteriorAmort, " +
        "( " +
        "(case when Isnull(SocioDebitoLancto.DataLancto,getdate()-10000) < @dtini1 then " +
        "0.0 " +
        "else " +
        " case when Isnull(SocioDebitoLancto.Valor,0) = 0 then 0.0 " +
        "       when Isnull(SocioDebitoLancto.TipoLanctoDebito,0) = 2 then Isnull(SocioDebitoLancto.Valor,0) " +
        "       when Isnull(SocioDebitoLancto.TipoLanctoDebito,0) = 1 then 0.0 end  " +
        "             end ))  ValorPeriodoAmort " +

        "                from SocioDebitoLancto " +
        "inner join SocioDebito on SocioDebitoLancto.SocioDebitoId = SocioDebito.Id  " +
        "inner join EmpreendimentoSocio on EmpreendimentoSocio.Id = SocioDebito.EmpreendimentoSocioId " +
        "inner join Empreendimento on Empreendimento.Id = EmpreendimentoSocio.EmpreendimentoId and Empreendimento.Id = @empid  " +
        "inner join Periodo on Periodo.DataInicio <= SocioDebitoLancto.DataLancto and Periodo.DataFim >= SocioDebitoLancto.DataLancto  and Periodo.EmpreendimentoId = Empreendimento.Id   " +
        "inner join Socio on Socio.Id = EmpreendimentoSocio.SocioId  " +
        "where SocioDebitoLancto.DataLancto <=  Convert(date, @dtfim, 112) and SocioDebitoLancto.TenantId = @tid  " +

        "order by EmpreendimentoSocio.Nome  ";


            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = empid;
            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@dtini1", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini1"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtini2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini2"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim"].Value = dtfim.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim1", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim1"].Value = dtfim.ToString("yyyyMMdd");

            await command.Connection.OpenAsync();
            try
            {
                var lu = await command.ExecuteReaderAsync();

                while (await lu.ReadAsync())
                {
                    LinhaSocio iso = r.FirstOrDefault(x => x.EmpreendimentoSocioId == (int)lu["EmpreendimentoSocioId"]);
                    if (iso == null)
                    {
                        iso = new LinhaSocio()
                        {
                            EmpreendimentoSocioId = (int)lu["EmpreendimentoSocioId"],
                            SocioNome = lu["SocioNome"].ToString(),
                            LinhaEmprestimo = new(),
                            Expand = false
                        };
                        r.Add(iso);
                    }
                    else
                    {
                        if (iso.LinhaEmprestimo == null)
                        {
                            iso.LinhaEmprestimo = new();
                        }
                    }
                    LinhaPeriodo ps = iso.LinhaEmprestimo.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                    if (ps == null)
                    {
                        ps = new LinhaPeriodo()
                        {
                            PeriodoId = (int)lu["PeriodoId"],
                            DataInicio = (DateTime)lu["DataInicio"]
                        };
                        iso.LinhaEmprestimo.LinhaPeriodos.Add(ps);
                    }

                    ps.Valor += (double)lu["ValorPeriodo"];
                    ps.ValorDespesa += (double)lu["ValorPeriodoJuro"];
                    ps.ValorReceita += (double)lu["ValorPeriodoAmort"];
                    ps.ValorResultado = 0;
                    iso.LinhaEmprestimo.ValorAnterior += (double)lu["ValorAnterior"];
                    iso.LinhaEmprestimo.ValorAnteriorJuro += (double)lu["ValorAnteriorJuro"];
                    iso.LinhaEmprestimo.ValorAnteriorAmort += (double)lu["ValorAnteriorAmort"];
                    iso.LinhaEmprestimo.ValorTotalTotal = 0;

                    if (iso.SocioNomePrincipalId != 0)
                    {
                        LinhaSocio tiso = r.FirstOrDefault(x => x.EmpreendimentoSocioId == 0 && x.SocioNomePrincipalId == iso.SocioNomePrincipalId);
                        if (tiso.LinhaEmprestimo == null)
                        {
                            tiso.LinhaEmprestimo = new();
                            tiso.LinhaEmprestimo.LinhaPeriodos = new();
                        }
                        LinhaPeriodo tps = tiso.LinhaEmprestimo.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                        if (tps == null)
                        {
                            tps = new LinhaPeriodo()
                            {
                                PeriodoId = (int)lu["PeriodoId"],
                                DataInicio = (DateTime)lu["DataInicio"]
                            };
                            tiso.LinhaEmprestimo.LinhaPeriodos.Add(tps);
                        }

                        tps.Valor += (double)lu["ValorPeriodo"];
                        tps.ValorDespesa += (double)lu["ValorPeriodoJuro"];
                        tps.ValorReceita += (double)lu["ValorPeriodoAmort"];
                        tps.ValorResultado = 0;
                        tiso.LinhaEmprestimo.ValorAnterior += (double)lu["ValorAnterior"];
                        tiso.LinhaEmprestimo.ValorAnteriorJuro += (double)lu["ValorAnteriorJuro"];
                        tiso.LinhaEmprestimo.ValorAnteriorAmort += (double)lu["ValorAnteriorAmort"];
                        tiso.LinhaEmprestimo.ValorTotalTotal = 0;
                    }

                }
                foreach (var i in r)
                {
                    i.LinhaEmprestimo.LinhaPeriodos = i.LinhaEmprestimo.LinhaPeriodos.OrderBy(x => x.DataInicio).ToList();
                }
                foreach (var i in r)
                {
                    double vt = i.LinhaEmprestimo.ValorAnteriorTotal;
                    i.LinhaEmprestimo.ValorTotalAmort = i.LinhaEmprestimo.ValorAnteriorAmort;
                    i.LinhaEmprestimo.ValorTotalJuro = i.LinhaEmprestimo.ValorAnteriorJuro;
                    i.LinhaEmprestimo.ValorTotal = i.LinhaEmprestimo.ValorAnterior;
                    i.LinhaEmprestimo.ValorTotalTotal = i.LinhaEmprestimo.ValorTotal + i.LinhaEmprestimo.ValorTotalJuro - i.LinhaEmprestimo.ValorTotalAmort;
                    i.LinhaEmprestimo.TemEmprestimo = i.LinhaEmprestimo.ValorAnterior != 0 || i.LinhaEmprestimo.ValorTotalTotal != 0;
                    foreach (var p in i.LinhaEmprestimo.LinhaPeriodos)
                    {
                        p.ValorResultadoAnterior = i.LinhaEmprestimo.ValorTotalTotal;
                        p.ValorResultado = vt + p.Valor - p.ValorReceita + p.ValorDespesa;
                        vt = p.ValorResultado;
                        i.LinhaEmprestimo.ValorTotalAmort += p.ValorReceita;
                        i.LinhaEmprestimo.ValorTotalJuro += p.ValorDespesa;
                        i.LinhaEmprestimo.ValorTotal += p.Valor;
                        i.LinhaEmprestimo.ValorTotalTotal = i.LinhaEmprestimo.ValorTotal + i.LinhaEmprestimo.ValorTotalJuro - i.LinhaEmprestimo.ValorTotalAmort;
                        if (p.ValorResultado != 0)
                        {
                            i.LinhaEmprestimo.TemEmprestimo = true;
                        }
                    }
                }

            }
            catch
            {
                //debug            si.Email = e.Message;
                throw;
            }
            finally
            {
                await command.Connection.CloseAsync();
            }
            return r;
        }
        public async Task<LinhaEmprestimo> LoadEmprestimoByDate(int empidsocioid, DateTime dtini, DateTime dtfim)
        {
            SessionInfo si = await GetSessionAsync();
            LinhaEmprestimo r = new();
            var l = await (from SocioDebitoLancto in _context.SocioDebitoLancto
                           join SocioDebito in _context.SocioDebito on SocioDebitoLancto.SocioDebitoId equals SocioDebito.Id
                           select new { SocioDebitoLancto, SocioDebito.EmpreendimentoSocioId, DataDebito = SocioDebito.DataLancto }).AsNoTracking().Where(x => x.SocioDebitoLancto.TenantId == si.TenantId
                                      && x.SocioDebitoLancto.DataLancto >= dtini
                                      && x.SocioDebitoLancto.DataLancto <= dtfim
                                      && x.EmpreendimentoSocioId == empidsocioid).OrderBy(x => x.SocioDebitoLancto.DataLancto).ThenBy(x => x.DataDebito).ToListAsync();
            foreach (var i in l)
            {
                if (i.SocioDebitoLancto.Historico == null || i.SocioDebitoLancto.Historico == string.Empty)
                {
                    if (i.SocioDebitoLancto.TipoLanctoDebito == TipoLanctoDebito.Juros)
                    {
                        i.SocioDebitoLancto.Historico = "Juros";
                    }
                    else
                    if (i.SocioDebitoLancto.TipoLanctoDebito == TipoLanctoDebito.Amortizacao)
                    {
                        i.SocioDebitoLancto.Historico = "Amortização";
                    }
                }
                r.EmprestimosLanctos.Add(i.SocioDebitoLancto);
            }
            r.Emprestimos = await _context.SocioDebito.AsNoTracking().Where(x => x.TenantId == si.TenantId
                                      && x.DataLancto >= dtini && x.DataLancto <= dtfim && x.EmpreendimentoSocioId == empidsocioid).ToListAsync();
            foreach (var e in r.Emprestimos)
            {
                if (e.Acordo == null || e.Acordo == string.Empty)
                {
                    e.Acordo = "Empréstimo/Adiantamento";
                }
            }
            if (r.Emprestimos == null)
            {
                r.Emprestimos = new();
            }
            return r;
        }
        public async Task<List<LinhaSocio>> LoadDadosSocioAporteByDate(int empid, DateTime dtini, DateTime dtfim, List<LinhaSocio> ls = null)
        {
            SessionInfo si = await GetSessionAsync();
            List<LinhaSocio> r = null;
            if (ls != null)
            {
                r = ls;
            }
            else
            {
                r = new();
            }
            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
        "            select " +
        "Periodo.Id PeriodoId, " +
        "Periodo.DataInicio DataInicio, " +
        "EmpreendimentoSocio.Id EmpreendimentoSocioId, " +
        "EmpreendimentoSocio.Nome SocioNome, " +
        "( " +
        "case when SocioAporteDeposito.DataDeposito >= @dtini1 then " +
        "0.0 " +
        "else " +
        "                SocioAporteDeposito.Valor end )  ValorAnterior, " +
        "( " +
        "case when  SocioAporteDeposito.DataDeposito >= @dtini2 then " +
        "SocioAporteDeposito.Valor " +
        "else " +
        "                0 end )  ValorPeriodo " +
        "                from SocioAporteDeposito " +
        "inner join EmpreendimentoSocio on EmpreendimentoSocio.Id = SocioAporteDeposito.EmpreendimentoSocioId " +
        "inner join Empreendimento on Empreendimento.Id = EmpreendimentoSocio.EmpreendimentoId and Empreendimento.Id = @empid  " +
        "inner join Periodo on Periodo.DataInicio <= SocioAporteDeposito.DataDeposito and Periodo.DataFim >= SocioAporteDeposito.DataDeposito  and Periodo.EmpreendimentoId = Empreendimento.Id   " +
        "inner join Socio on Socio.Id = EmpreendimentoSocio.SocioId  " +
        "where SocioAporteDeposito.DataDeposito <=  Convert(date, @dtfim, 112) and SocioAporteDeposito.TenantId = @tid  " +
        "order by   EmpreendimentoSocio.Nome  ";

            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = empid;
            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@dtini1", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini1"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtini2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini2"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim"].Value = dtfim.ToString("yyyyMMdd");

            await command.Connection.OpenAsync();
            var lu = await command.ExecuteReaderAsync();
            try
            {
                while (await lu.ReadAsync())
                {
                    LinhaSocio iso = r.FirstOrDefault(x => x.EmpreendimentoSocioId == (int)lu["EmpreendimentoSocioId"]);
                    if (iso == null)
                    {
                        iso = new()
                        {
                            EmpreendimentoSocioId = (int)lu["EmpreendimentoSocioId"],
                            SocioNome = lu["SocioNome"].ToString(),
                            LinhaAporte = new(),
                            Expand = false
                        };
                        r.Add(iso);
                    }
                    else
                    {
                        if (iso.LinhaAporte == null)
                        {
                            iso.LinhaAporte = new();
                        }
                    }
                    LinhaPeriodo ps = iso.LinhaAporte.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                    if (ps == null)
                    {
                        ps = new()
                        {
                            PeriodoId = (int)lu["PeriodoId"],
                            DataInicio = (DateTime)lu["DataInicio"],
                        };
                        iso.LinhaAporte.LinhaPeriodos.Add(ps);
                    }
                    ps.Valor += (double)lu["ValorPeriodo"];
                    iso.LinhaAporte.ValorPeriodo += (double)lu["ValorPeriodo"];
                    iso.LinhaAporte.ValorAnterior += (double)lu["ValorAnterior"];
                    iso.LinhaAporte.ValorTotal = iso.LinhaAporte.ValorAnterior + iso.LinhaAporte.ValorPeriodo;

                    if (iso.SocioNomePrincipalId != 0)
                    {
                        LinhaSocio tiso = r.FirstOrDefault(x => x.EmpreendimentoSocioId == 0 && x.SocioNomePrincipalId == iso.SocioNomePrincipalId);
                        if (tiso.LinhaAporte == null)
                        {
                            tiso.LinhaAporte = new();
                            tiso.LinhaAporte.LinhaPeriodos = new();
                        }
                        LinhaPeriodo tps = tiso.LinhaAporte.LinhaPeriodos.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                        if (tps == null)
                        {
                            tps = new()
                            {
                                PeriodoId = (int)lu["PeriodoId"],
                                DataInicio = (DateTime)lu["DataInicio"]
                            };
                            tiso.LinhaAporte.LinhaPeriodos.Add(tps);
                        }
                        tps.Valor += (double)lu["ValorPeriodo"];
                        tiso.LinhaAporte.ValorPeriodo += (double)lu["ValorPeriodo"];
                        tiso.LinhaAporte.ValorAnterior += (double)lu["ValorAnterior"];
                        tiso.LinhaAporte.ValorTotal = tiso.LinhaAporte.ValorAnterior + tiso.LinhaAporte.ValorPeriodo;
                    }
                }
            }
            finally
            {
                await command.Connection.CloseAsync();
            }
            return r;
        }
        public async Task<List<LinhaContaCorrente>> LoadDadosContaCorrenteByDate(int empid, DateTime dtini, DateTime dtfim, int empsocioid = 0)
        {
            SessionInfo si = await GetSessionAsync();
            List<LinhaContaCorrente> r = new();
            string sql1 = string.Empty;

            if (empsocioid != 0)
            {
                List<ContaCorrenteEmpreendimento> contas = await _context.ContaCorrenteEmpreendimento.Where(x => x.EmpreendimentoId == empid).ToListAsync();
                if (contas != null && contas.Count > 0)
                {
                    var lcs = await _context.EmpSocioContaCorrente.AsNoTracking().Where(x => x.TenantId == si.TenantId && x.EmpreendimentoSocioId == empsocioid && x.Permitir == false).ToListAsync();
                    if (lcs.Count > 0)
                    {
                        sql1 = " and ContaCorrente.Id in ( ";
                        int i = 0;
                        foreach (var cr in contas)
                        {
                            if (!lcs.Any(x => x.ContaCorrenteId == cr.ContaCorrenteId))
                            {
                                if (i == 0)
                                {
                                    sql1 += cr.ContaCorrenteId.ToString();
                                    i++;

                                }
                                else
                                {
                                    sql1 += "," + cr.ContaCorrenteId.ToString();
                                }
                            }
                        }
                        if (i == 0)
                        {
                            return null;
                        }
                        else
                        {
                            sql1 += " ) ";
                        }
                    }
                }
            }

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
        "            select " +
        "Periodo.Id PeriodoId, " +
        "Max(Periodo.DataInicio) DataInicio, " +
        "ContaCorrente.Id ContaCorrenteId, " +
        "max(ContaCorrente.Nome) ContaCorrenteNome, " +
         "Sum(case when MovtoBanco.DataMovto >= @dtini1 then " +
        "0.0 " +
        "else " +
        "     case when MovtoBanco.TipoMovtoBanco = 2 then  MovtoBanco.Valor * -1 else MovtoBanco.Valor end end )  SaldoAnterior, " +


        "sum( " +
        "case when  MovtoBanco.DataMovto < @dtini2 and MovtoBanco.TipoMovtoBanco = 1 and MovtoBanco.Transferencia = 0 then " +
        " MovtoBanco.Valor " +
        "else " +
        "  0.0 end )  EntradasAnterior, " +
        "sum(case when  MovtoBanco.DataMovto < @dtini3 and MovtoBanco.TipoMovtoBanco = 2 and MovtoBanco.Transferencia = 0 then " +
        "MovtoBanco.Valor * -1 " +
        "else " +
        "                0.0 end )  SaidasAnterior, " +
        "Sum(case when  MovtoBanco.DataMovto < @dtini4 and Transferencia = 1 then " +
        "   case when MovtoBanco.TipoMovtoBanco = 2 then  MovtoBanco.Valor * -1 else MovtoBanco.Valor end " +
        "else " +
        "                0.0 end )  TransferenciasAnterior, " +

        "sum( " +
        "case when  MovtoBanco.DataMovto <= @dtfim and MovtoBanco.TipoMovtoBanco = 1 and MovtoBanco.Transferencia = 0 then " +
        " MovtoBanco.Valor " +
        "else " +
        "  0.0 end )  EntradasFinal, " +
        "sum(case when  MovtoBanco.DataMovto <= @dtfim and MovtoBanco.TipoMovtoBanco = 2 and MovtoBanco.Transferencia = 0 then " +
        "MovtoBanco.Valor * -1 " +
        "else " +
        "                0.0 end )  SaidasFinal, " +
        "Sum(case when  MovtoBanco.DataMovto <= @dtfim and Transferencia = 1 then " +
        "   case when MovtoBanco.TipoMovtoBanco = 2 then  MovtoBanco.Valor * -1 else MovtoBanco.Valor end " +
        "else " +
        "                0.0 end )  TransferenciasFinal, " +

        "sum( " +
        "case when  MovtoBanco.DataMovto >= @dtini2 and MovtoBanco.TipoMovtoBanco = 1 and MovtoBanco.Transferencia = 0 then " +
        "MovtoBanco.Valor " +
        "else " +
        "                0.0 end )  Entradas, " +
        "sum(case when  MovtoBanco.DataMovto >= @dtini3 and MovtoBanco.TipoMovtoBanco = 2 and MovtoBanco.Transferencia = 0 then " +
        "MovtoBanco.Valor * -1 " +
        "else " +
        "                0.0 end )  Saidas, " +
        "Sum(case when  MovtoBanco.DataMovto >= @dtini4 and Transferencia = 1 then " +
        "   case when MovtoBanco.TipoMovtoBanco = 2 then  MovtoBanco.Valor * -1 else MovtoBanco.Valor end " +
        "else " +
        "                0.0 end )  Transferencias, " +
        "Max(Isnull(ContaCorrenteExtrato.FileId,0)) FileId " +
        "                from ContaCorrente " +
        "inner join MovtoBanco on MovtoBanco.ContaCorrenteId = ContaCorrente.Id " +
        "inner join ContaCorrenteEmpreendimento on ContaCorrenteEmpreendimento.ContaCorrenteId =  ContaCorrente.Id " +
        "inner join Empreendimento on Empreendimento.Id = ContaCorrenteEmpreendimento.EmpreendimentoId and Empreendimento.Id = @empid  " +
        "inner join Periodo on Periodo.DataInicio <= MovtoBanco.DataMovto and Periodo.DataFim >= MovtoBanco.DataMovto  and Periodo.EmpreendimentoId = Empreendimento.Id   " +

        "left join ContaCorrenteExtrato on ContaCorrenteExtrato.DataInicio = Periodo.DataInicio and ContaCorrenteExtrato.ContaCorrenteId = ContaCorrente.Id " +


        "where MovtoBanco.DataMovto <=  Convert(date, @dtfim, 112) and ContaCorrente.TenantId = @tid  " +
        sql1 +
        "group by  Periodo.Id, ContaCorrente.Id  ";

            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = empid;
            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@dtini1", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini1"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtini2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini2"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtini3", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini3"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtini4", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini4"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim"].Value = dtfim.ToString("yyyyMMdd");

            bool mycon = command.Connection.State != System.Data.ConnectionState.Open;
            if (mycon)
            {
                await command.Connection.OpenAsync();
            }
            var lu = await command.ExecuteReaderAsync();
            try
            {
                LinhaContaCorrente geral = new();
                geral.ContaCorrenteId = 0;
                geral.Nome = " Saldo em conta";
                geral.LinhasMovto = new();

                while (await lu.ReadAsync())
                {
                    LinhaContaCorrente ict = r.FirstOrDefault(x => x.ContaCorrenteId == (int)lu["ContaCorrenteId"]);
                    if (ict == null)
                    {
                        ict = new()
                        {
                            ContaCorrenteId = (int)lu["ContaCorrenteId"],
                            Nome = lu["ContaCorrenteNome"].ToString(),
                            LinhasMovto = new()
                        };
                        r.Add(ict);
                    }
                    LinhaMovtoContaCorrente ps = ict.LinhasMovto.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                    if (ps == null)
                    {
                        ps = new()
                        {
                            PeriodoId = (int)lu["PeriodoId"],
                            DataInicio = (DateTime)lu["DataInicio"],
                            FileId = (int)lu["FileId"]
                        };
                        if (ps.FileId > 0)
                        {
                            ict.TemExtrato = true;
                        }
                        ict.LinhasMovto.Add(ps);
                    }
                    ict.SaldoAnterior += (double)lu["SaldoAnterior"];
                    ps.Entradas += (double)lu["Entradas"];
                    ps.Saidas += (double)lu["Saidas"];
                    ps.Transferencias += (double)lu["Transferencias"];

                    ict.EntradasAnterior += (double)lu["EntradasAnterior"];
                    ict.SaidasAnterior += (double)lu["SaidasAnterior"];
                    ict.TransferenciasAnterior += (double)lu["TransferenciasAnterior"];

                    ict.EntradasFinal += (double)lu["EntradasFinal"];
                    ict.SaidasFinal += (double)lu["SaidasFinal"];
                    ict.TransferenciasFinal += (double)lu["TransferenciasFinal"];

                    LinhaMovtoContaCorrente psg = geral.LinhasMovto.FirstOrDefault(x => x.PeriodoId == (int)lu["PeriodoId"]);
                    if (psg == null)
                    {
                        psg = new()
                        {
                            PeriodoId = (int)lu["PeriodoId"],
                            DataInicio = (DateTime)lu["DataInicio"]
                        };
                        geral.LinhasMovto.Add(psg);
                    }
                    geral.SaldoAnterior += (double)lu["SaldoAnterior"];
                    psg.Entradas += (double)lu["Entradas"];
                    psg.Saidas += (double)lu["Saidas"];
                    psg.Transferencias += (double)lu["Transferencias"];

                    geral.EntradasAnterior += (double)lu["EntradasAnterior"];
                    geral.SaidasAnterior += (double)lu["SaidasAnterior"];
                    geral.TransferenciasAnterior += (double)lu["TransferenciasAnterior"];

                    geral.EntradasFinal += (double)lu["EntradasFinal"];
                    geral.SaidasFinal += (double)lu["SaidasFinal"];
                    geral.TransferenciasFinal += (double)lu["TransferenciasFinal"];

                }
                geral.SaldoFinal = geral.SaldoAnterior;
                foreach (var ct in r)
                {
                    ct.LinhasMovto = ct.LinhasMovto.OrderBy(x => x.DataInicio).ToList();
                    double vac = ct.SaldoAnterior;
                    ct.SaldoFinal = ct.SaldoAnterior;
                    foreach (var pg in ct.LinhasMovto)
                    {
                        pg.SaldoAnterior = ct.SaldoFinal;
                        pg.SaldoFinal += vac + pg.Entradas + pg.Saidas + pg.Transferencias;
                        vac = pg.SaldoFinal;
                        ct.SaldoFinal = pg.SaldoFinal;
                    }
                }

                geral.LinhasMovto = geral.LinhasMovto.OrderBy(x => x.DataInicio).ToList();
                double vacg = geral.SaldoAnterior;
                geral.SaldoFinal = geral.SaldoAnterior;
                foreach (var pg in geral.LinhasMovto)
                {
                    pg.SaldoAnterior = geral.SaldoFinal;
                    pg.SaldoFinal += vacg + pg.Entradas + pg.Saidas + pg.Transferencias;
                    vacg = pg.SaldoFinal;
                    geral.SaldoFinal = pg.SaldoFinal;
                }
                r.Add(geral);
                r = r.OrderBy(x => x.Nome).ToList();
                geral.Nome = "Saldo em conta";
            }
            finally
            {
                if (mycon)
                {
                    await command.Connection.CloseAsync();
                }
            }
            return r;
        }
        public async Task<List<PeriodoNota>> FindAllNotasByPeriodoId(int periodoid, int empscioid = 0)
        {
            SessionInfo si = await GetSessionAsync();
            var l = await _context.PeriodoNota.Where(x => x.TenantId == si.TenantId && x.PeriodoId == periodoid).ToListAsync();
            if (empscioid != 0)
            {
                var es = await _context.EmpreendimentoSocio.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == empscioid);
                if (es != null)
                {
                    if (es.TipoSocio == TipoSocio.Investidor)
                    {
                        return l.Where(x => x.TenantId == si.TenantId && x.PeriodoId == periodoid && (x.AcessoGeral == true || x.AcessoInvestidor == true)).ToList();
                    }
                    if (es.TipoSocio == TipoSocio.Parceiro)
                    {
                        return l.Where(x => x.TenantId == si.TenantId && x.PeriodoId == periodoid && (x.AcessoGeral == true || x.AcessoParceiro == true)).ToList();
                    }
                    if (es.TipoSocio == TipoSocio.SocioAdministrador)
                    {
                        return l.Where(x => x.TenantId == si.TenantId && x.PeriodoId == periodoid && (x.AcessoGeral == true || x.AcessoSocioAdministrador == true)).ToList();
                    }
                }
            }
            return l;
        }
        public async Task<Resultado> AddPeriodoNota(PeriodoNota p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            p.TenantId = si.TenantId;
            try
            {
                await _context.PeriodoNota.AddAsync(p);
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
        public async Task<Resultado> UpdatePeriodoNota(PeriodoNota p)
        {
            Resultado r = new();
            try
            {
                _context.PeriodoNota.Update(p);
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
        public async Task<Resultado> DeletePeriodoNota(PeriodoNota p)
        {
            Resultado r = new();
            try
            {
                _context.PeriodoNota.Remove(p);
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
        public async Task<List<PeriodoReport>> FindAllPeriodoReportsByPeriodoId(int periodoid, int empsocioid = 0)
        {
            SessionInfo si = await GetSessionAsync();
            var l = await _context.PeriodoReport.Where(x => x.TenantId == si.TenantId && x.PeriodoId == periodoid).OrderBy(x => x.NomeReport).ToListAsync();
            if (empsocioid == 0)
            {
                return l;
            }
            List<PeriodoReport> r = new();
            foreach (var a in l)
            {
                if (await _context.PeriodoReportEmpSocio.AnyAsync(x => x.TenantId == si.TenantId && x.PeriodoReportId == a.Id))
                {
                    if (await _context.PeriodoReportEmpSocio.AnyAsync(x => x.TenantId == si.TenantId && x.PeriodoReportId == a.Id && x.EmpreendimentoSocioId == empsocioid))
                    {
                        r.Add(a);
                    }
                }
                else
                {
                    r.Add(a);
                }
            }
            return r;
        }
        public async Task<List<PeriodoReportEmpSocio>> FindAllPeriodoReportEmpSocioByEmpId(int empid, int reportid)
        {
            SessionInfo si = await GetSessionAsync();
            _context.ChangeTracker.Clear();

            List<PeriodoReportEmpSocio> r = new();

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
        "            select " +
        " EmpreendimentoSocio.Id EmpreendimentoSocioId, " +
        " EmpreendimentoSocio.Nome Nome, " +
        " EmpreendimentoSocio.TenantId TenantId, " +
        " Isnull(PeriodoReportEmpSocio.Id,0) Id, " +
        " Isnull(PeriodoReportEmpSocio.PeriodoReportId,0) PeriodoReportId " +
        " from EmpreendimentoSocio " +
        " left join PeriodoReportEmpSocio on PeriodoReportEmpSocio.EmpreendimentoSocioId = EmpreendimentoSocio.Id and PeriodoReportEmpSocio.PeriodoReportId = @reportid " +
        " where EmpreendimentoSocio.EmpreendimentoId = @empid and EmpreendimentoSocio.TenantId = @tid ";


            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = empid;

            command.Parameters.Add(new SqlParameter("@reportid", System.Data.SqlDbType.Int));
            command.Parameters["@reportid"].Value = reportid;

            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            await command.Connection.OpenAsync();
            var lu = await command.ExecuteReaderAsync();

            while (await lu.ReadAsync())
            {
                PeriodoReportEmpSocio ps = new()
                {
                    EmpreendimentoSocioId = (int)lu["EmpreendimentoSocioId"],
                    Id = (int)lu["Id"],
                    PeriodoReportId = (int)lu["PeriodoReportId"],
                    NomeSocio = lu["Nome"].ToString(),
                    TenantId = lu["TenantId"].ToString(),
                    EmpreendimentoId = empid
                };
                r.Add(ps);
            }
            await lu.CloseAsync();
            await command.Connection.CloseAsync();

            return r;
        }
        public async Task<Resultado> AddPeriodoReport(PeriodoReport p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            p.TenantId = si.TenantId;
            try
            {
                await _context.Database.BeginTransactionAsync();
                await _context.PeriodoReport.AddAsync(p);
                await _context.SaveChangesAsync();
                foreach (var a in p.EmpSocios.Where(x => x.PeriodoReportId > 0).ToList())
                {
                    PeriodoReportEmpSocio pr = new()
                    {
                        PeriodoReportId = p.Id,
                        EmpreendimentoSocioId = a.EmpreendimentoSocioId,
                        TenantId = si.TenantId
                    };
                    pr.NomeSocio = await _context.EmpreendimentoSocio.Where(x => x.TenantId == si.TenantId && x.Id == a.EmpreendimentoSocioId).Select(x => x.Nome).FirstOrDefaultAsync();
                    a.PeriodoReportId = p.Id;
                    await _context.PeriodoReportEmpSocio.AddAsync(pr);
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                r.Ok = true;
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> UpdatePeriodoReport(PeriodoReport p)
        {
            Resultado r = new();
            try
            {
                _context.PeriodoReport.Update(p);
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
        public async Task<Resultado> AddPeriodoReportEmpSocio(PeriodoReportEmpSocio p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            p.TenantId = si.TenantId;
            try
            {
                if (p.Id == 0)
                {
                    await _context.PeriodoReportEmpSocio.AddAsync(p);
                }
                else
                {
                    _context.PeriodoReportEmpSocio.Update(p);
                }
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
        public async Task<Resultado> DeletePeriodoReportEmpSocio(PeriodoReportEmpSocio p)
        {
            Resultado r = new();
            try
            {
                _context.PeriodoReportEmpSocio.Remove(p);
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
        public async Task<Resultado> CopyReport(int empid, int periododestino, DateTime dt)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            int periodoorigem = await _context.Periodo.Where(x => x.EmpreendimentoId == empid && x.DataFim < dt).OrderByDescending(x => x.DataFim).Select(x => x.Id).FirstOrDefaultAsync();

            await _context.Database.BeginTransactionAsync();
            try
            {
                var lra = await _context.PeriodoReport.Where(x => x.PeriodoId == periodoorigem && x.TenantId == si.TenantId).ToListAsync();
                foreach (var ra in lra)
                {
                    PeriodoReport nr = new()
                    {
                        PeriodoId = periododestino,
                        TenantId = si.TenantId,
                        NomeReport = ra.NomeReport
                    };
                    await _context.PeriodoReport.AddAsync(nr);
                    await _context.SaveChangesAsync();
                    var lsa = await _context.PeriodoReportEmpSocio.Where(x => x.PeriodoReportId == ra.Id && x.TenantId == si.TenantId).ToListAsync();
                    foreach (var sa in lsa)
                    {
                        PeriodoReportEmpSocio ns = new()
                        {
                            TenantId = si.TenantId,
                            EmpreendimentoSocioId = sa.EmpreendimentoSocioId,
                            PeriodoReportId = nr.Id
                        };
                        await _context.PeriodoReportEmpSocio.AddAsync(ns);
                    }
                    await _context.SaveChangesAsync();
                }
                await _context.Database.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                await _context.Database.RollbackTransactionAsync();
            }
            return r;
        }
        public async Task<Resultado> DeletePeriodoReport(PeriodoReport p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            try
            {
                await _context.Database.BeginTransactionAsync();
                await _context.Database.ExecuteSqlRawAsync(" Delete PeriodoReportEmpSocio where TenantId = '" + si.TenantId + "'  and PeriodoReportId = " + p.Id.ToString());
                _context.PeriodoReport.Remove(p);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                r.Ok = true;
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> AddOrEditMovtoUnidade(UnidadeLoteImportacao ul, UnidadeEmpreendimentoView uv, List<UnidadeEmpreendimentoView> items = null)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            _context.ChangeTracker.Clear();
            await _context.Database.BeginTransactionAsync();
            try
            {
                if (ul.Id == 0)
                {
                    var nul = await _context.UnidadeLoteImportacao.FirstOrDefaultAsync(x => x.DataMovto == ul.DataMovto && x.EmpreendimentoId == ul.EmpreendimentoId && x.TenantId == si.TenantId);
                    if (nul == null)
                    {
                        ul.TenantId = si.TenantId;
                        ul.DataInclusao = Constante.Now;
                        ul.TipoLoteImpUnidade = TipoLoteImpUnidade.Digitado;
                        ul.FileName = "Digitado em: " + Constante.Now.ToString("dd/MM/yyyy HH:mm");
                        await _context.UnidadeLoteImportacao.AddAsync(ul);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        ul = nul;
                        items = null;
                    }
                }
                var m = await _context.UnidadeMovto.Where(x => x.UnidadeEmpreendimentoId == uv.Unidade.Id && x.TenantId == si.TenantId && x.UnidadeLoteImportacaoId == ul.Id).ToListAsync();
                if (m.Count > 0)
                {
                    _context.UnidadeMovto.RemoveRange(m);
                    await _context.SaveChangesAsync();
                }
                uv.Movto.Id = 0;
                uv.Movto.UnidadeLoteImportacaoId = ul.Id;
                uv.Movto.UnidadeEmpreendimentoId = uv.Unidade.Id;
                uv.Movto.TenantId = si.TenantId;
                await _context.UnidadeMovto.AddAsync(uv.Movto);
                await _context.SaveChangesAsync();

                if (items != null)
                {
                    List<UnidadeMovto> lma = new();
                    foreach (var ma in items)
                    {
                        if (ma.Unidade.Id == uv.Unidade.Id)
                        {
                            continue;
                        }
                        ma.Movto.Id = 0;
                        ma.Movto.UnidadeLoteImportacaoId = ul.Id;
                        ma.Movto.UnidadeEmpreendimentoId = ma.Unidade.Id;
                        ma.Movto.TenantId = si.TenantId;
                        lma.Add(ma.Movto);
                    }
                    if (lma.Count > 0)
                    {
                        await _context.UnidadeMovto.AddRangeAsync(lma);
                        await _context.SaveChangesAsync();
                    }
                }

                await _context.Database.CommitTransactionAsync();
                r.SetDefault();
                r.Item = ul;
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> AddMovtoUnidade(UnidadeEmpreendimentoView uv)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            bool mytrans = (_context.Database.CurrentTransaction == null);
            if (mytrans)
            {
                await _context.Database.BeginTransactionAsync();
            }
            try
            {
                var unidade = await _context.UnidadeEmpreendimento.FirstOrDefaultAsync(x => x.CodigoExterno == uv.Unidade.CodigoExterno
                                                                                     && x.EmpreendimentoId == uv.Unidade.EmpreendimentoId
                                                                                     && x.TenantId == si.TenantId);
                if (unidade == null)
                {
                    unidade = new();
                    _mapper.Map(uv.Unidade, unidade);
                    unidade.TenantId = si.TenantId;
                    await _context.UnidadeEmpreendimento.AddAsync(unidade);
                    await _context.SaveChangesAsync();
                }
                uv.Movto.UnidadeEmpreendimentoId = unidade.Id;
                uv.Movto.TenantId = si.TenantId;
                await _context.UnidadeMovto.AddAsync(uv.Movto);
                await _context.SaveChangesAsync();
                if (mytrans)
                {
                    await _context.Database.CommitTransactionAsync();
                }
                r.SetDefault();
            }
            catch (Exception e)
            {
                if (mytrans)
                {
                    await _context.Database.RollbackTransactionAsync();
                    r.Ok = false;
                    r.ErrMsg = e.Message;
                }
                else
                {
                    throw;
                }
            }
            return r;
        }
        public async Task<Resultado> SaveUnidadeMovtoFromFile(int empId, string filename, List<UnidadeEmpreendimentoView> lu)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            await _context.Database.BeginTransactionAsync();
            try
            {
                DateTime dtref = DateTime.MinValue;
                lu = lu.OrderBy(x => x.DataMovto).ThenBy(x => x.Unidade.CodigoExterno).ToList();
                UnidadeLoteImportacao ul = null;
                foreach (var uv in lu)
                {
                    if (dtref != uv.DataMovto)
                    {
                        dtref = uv.DataMovto;
                        ul = await _context.UnidadeLoteImportacao.FirstOrDefaultAsync(x => x.DataMovto == dtref && x.EmpreendimentoId == empId && x.TenantId == si.TenantId);
                        if (ul == null)
                        {
                            ul = new()
                            {
                                EmpreendimentoId = empId,
                                DataMovto = uv.DataMovto,
                                FileName = filename,
                                TipoLoteImpUnidade = TipoLoteImpUnidade.Upload,
                                DataInclusao = Constante.Now,
                                TenantId = si.TenantId
                            };
                            await _context.UnidadeLoteImportacao.AddAsync(ul);
                            await _context.SaveChangesAsync();
                        }
                    }

                    SqlParameter[] params1 =
                    {
                        new SqlParameter("@codext", System.Data.SqlDbType.NVarChar, 100),
                        new SqlParameter("@loteid", System.Data.SqlDbType.Int),
                        new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100)
                    };
                    params1[0].Value = uv.Unidade.CodigoExterno;
                    params1[1].Value = ul.Id;
                    params1[2].Value = si.TenantId;

                    await _context.Database.ExecuteSqlRawAsync(
        " delete UnidadeMovto from UnidadeEmpreendimento " +
        " where UnidadeMovto.UnidadeEmpreendimentoId = UnidadeEmpreendimento.Id " +
        " and UnidadeEmpreendimento.CodigoExterno =  @codext  " +
        " and UnidadeMovto.UnidadeLoteImportacaoId = @loteid  " +
        " and  UnidadeEmpreendimento.TenantId = @tid   ", params1);

                    uv.Movto.UnidadeLoteImportacaoId = ul.Id;
                    uv.Unidade.EmpreendimentoId = empId;
                    r = await AddMovtoUnidade(uv);
                    if (!r.Ok)
                    {
                        await _context.Database.RollbackTransactionAsync();
                        return r;
                    }
                }
                await _context.Database.CommitTransactionAsync();
                r.SetDefault();
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<List<UnidadeEmpreendimentoView>> GetEspelhoVenda(int empid, DateTime dtref)
        {
            SessionInfo si = await GetSessionAsync();
            List<UnidadeEmpreendimentoView> r = new();

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
          "  Select UnidadeEmpreendimento.TipoEmp  " +
          "  ,UnidadeEmpreendimento.Andar " +
          "  ,UnidadeEmpreendimento.Nro " +
          "  ,UnidadeEmpreendimento.Torre " +
          "  ,UnidadeEmpreendimento.Area " +
          "  ,UnidadeEmpreendimento.Quadra " +
          "  ,UnidadeEmpreendimento.OffsetX " +
          "  ,UnidadeEmpreendimento.OffsetY " +
          "  ,UnidadeEmpreendimento.CodigoExterno " +
          "  ,UnidadeEmpreendimento.Id UnidadeEmpreendimentoId " +
          "  ,Isnull(UnidadeLoteImportacao.Id, 0) UnidadeLoteImportacaoId " +
          "  ,Isnull(UnidadeMovto.Id,0) UnidadeMovtoId  " +
          "  ,UnidadeLoteImportacao.DataMovto  " +
          "  ,UnidadeMovto.StatusUnidade  " +
          "  ,UnidadeMovto.StatusFinanceiroUnidade  " +
          "  ,UnidadeMovto.ValorAtualVenda  " +
          "  ,UnidadeMovto.ValorAtualAluguel  " +
          "  ,UnidadeMovto.DataContrato  " +
          "  ,UnidadeMovto.ValorContrato  " +
          "  ,UnidadeMovto.DataDistrato  " +
          "  ,UnidadeMovto.ValorReembolso  " +
          "  ,UnidadeMovto.ValorAte30  " +
          "  ,UnidadeMovto.ValorAte60  " +
          "  ,UnidadeMovto.ValorAte90  " +
          "  ,UnidadeMovto.ValorAte120  " +
          "  ,UnidadeMovto.ValorApos120  " +
          "  from UnidadeEmpreendimento  " +
        " left join  " +
        " (Select  UnidadeMovto.UnidadeEmpreendimentoId, max(UnidadeLoteImportacao.DataMovto) DataMovto  from UnidadeMovto  " +
        " join UnidadeLoteImportacao on UnidadeLoteImportacao.Id = UnidadeMovto.UnidadeLoteImportacaoId  " +
        " where UnidadeLoteImportacao.DataMovto <= @dtref and UnidadeLoteImportacao.EmpreendimentoId = @empid2  " +
        " group by UnidadeMovto.UnidadeEmpreendimentoId  " +
        " ) LastMovto on LastMovto.UnidadeEmpreendimentoId = UnidadeEmpreendimento.Id  " +
        " left join UnidadeLoteImportacao on UnidadeLoteImportacao.EmpreendimentoId = UnidadeEmpreendimento.EmpreendimentoId  " +
        " and UnidadeLoteImportacao.DataMovto = LastMovto.DataMovto  " +
        " left join UnidadeMovto on UnidadeMovto.UnidadeLoteImportacaoId = UnidadeLoteImportacao.Id  " +
        "      and UnidadeMovto.UnidadeEmpreendimentoId = LastMovto.UnidadeEmpreendimentoId  " +
        "  where UnidadeEmpreendimento.EmpreendimentoId = @empid  " +
        "  and UnidadeEmpreendimento.TenantId = @tid  ";

            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = empid;
            command.Parameters.Add(new SqlParameter("@empid2", System.Data.SqlDbType.Int));
            command.Parameters["@empid2"].Value = empid;
            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@dtref", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtref"].Value = dtref.ToString("yyyyMMdd"); ;
            await command.Connection.OpenAsync();
            try
            {
                var lu = await command.ExecuteReaderAsync();
                while (await lu.ReadAsync())
                {
                    UnidadeEmpreendimentoView uv = new();
                    uv.Unidade.Id = (int)lu["UnidadeEmpreendimentoId"];
                    uv.Id = (int)lu["UnidadeEmpreendimentoId"];
                    uv.Unidade.TipoEmp = (TipoUE)lu["TipoEmp"];
                    uv.Unidade.Andar = lu["Andar"].ToString();
                    uv.Unidade.Nro = lu["Nro"].ToString();
                    uv.Unidade.Torre = lu["Torre"].ToString();
                    uv.Unidade.Quadra = lu["Quadra"].ToString();
                    uv.Unidade.Area = (double)lu["Area"];
                    uv.Unidade.OffsetX = (double)lu["OffsetX"];
                    uv.Unidade.OffsetY = (double)lu["OffsetY"];
                    uv.Unidade.CodigoExterno = lu["CodigoExterno"].ToString();
                    uv.UnidadeLoteImportacaoId = (int)lu["UnidadeLoteImportacaoId"];
                    if (uv.UnidadeLoteImportacaoId == 0)
                    {
                        uv.DataMovto = DateTime.MinValue;
                    }
                    else
                    {
                        uv.DataMovto = (DateTime)lu["DataMovto"];
                    }
                    uv.Movto.Id = (int)lu["UnidadeMovtoId"];
                    uv.Movto.UnidadeEmpreendimentoId = (int)lu["UnidadeEmpreendimentoId"];
                    uv.Movto.UnidadeLoteImportacaoId = (int)lu["UnidadeLoteImportacaoId"];
                    if (uv.Movto.Id == 0)
                    {
                        r.Add(uv);
                        continue;
                    }
                    uv.Movto.StatusUnidade = (StatusUnidade)lu["StatusUnidade"];
                    uv.Movto.StatusFinanceiroUnidade = (StatusFinanceiroUnidade)lu["StatusFinanceiroUnidade"];
                    uv.Movto.ValorAtualVenda = (double)lu["ValorAtualVenda"];
                    uv.Movto.ValorAtualAluguel = (double)lu["ValorAtualAluguel"];
                    uv.Movto.DataContrato = (DateTime)lu["DataContrato"];
                    uv.Movto.ValorContrato = (double)lu["ValorContrato"];
                    uv.Movto.DataDistrato = (DateTime)lu["DataDistrato"];
                    uv.Movto.ValorReembolso = (double)lu["ValorReembolso"];
                    uv.Movto.ValorAte30 = (double)lu["ValorAte30"];
                    uv.Movto.ValorAte60 = (double)lu["ValorAte60"];
                    uv.Movto.ValorAte90 = (double)lu["ValorAte90"];
                    uv.Movto.ValorAte120 = (double)lu["ValorAte120"];
                    uv.Movto.ValorApos120 = (double)lu["ValorApos120"];
                    r.Add(uv);
                }
                lu.Close();
            }
            catch
            {
                command.Connection.Close();
                throw;
            }
            finally
            {
                command.Connection.Close();
            }
            return r;
        }
        public async Task<UnidadeLoteImportacao> GetPreviusUnidadeLoteImportacao(int empid, DateTime dtref)
        {
            SessionInfo si = await GetSessionAsync();
            if (await _context.UnidadeLoteImportacao.AnyAsync(x => x.TenantId == si.TenantId && x.EmpreendimentoId == empid && x.DataMovto <= dtref))
            {
                DateTime lastdt = await _context.UnidadeLoteImportacao.Where(x => x.TenantId == si.TenantId && x.EmpreendimentoId == empid && x.DataMovto <= dtref).MaxAsync(x => x.DataMovto);
                return await _context.UnidadeLoteImportacao.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.EmpreendimentoId == empid && x.DataMovto == lastdt);
            }
            else
            {
                return null;
            }
        }
        public async Task<UnidadeLoteImportacao> GetNextUnidadeLoteImportacao(int empid, DateTime dtref)
        {
            SessionInfo si = await GetSessionAsync();
            if (await _context.UnidadeLoteImportacao.AnyAsync(x => x.TenantId == si.TenantId && x.EmpreendimentoId == empid && x.DataMovto > dtref))
            {
                DateTime lastdt = await _context.UnidadeLoteImportacao.Where(x => x.TenantId == si.TenantId && x.EmpreendimentoId == empid && x.DataMovto > dtref).MinAsync(x => x.DataMovto);
                return await _context.UnidadeLoteImportacao.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.EmpreendimentoId == empid && x.DataMovto == lastdt);
            }
            else
            {
                return null;
            }
        }
        public async Task<Resultado> DeleteUnidadeLoteImportacao(UnidadeLoteImportacao ul)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            try
            {
                await _context.Database.BeginTransactionAsync();
                var lm = await _context.UnidadeMovto.Where(x => x.TenantId == si.TenantId && x.UnidadeLoteImportacaoId == ul.Id).ToListAsync();
                _context.UnidadeMovto.RemoveRange(lm);
                await _context.SaveChangesAsync();
                _context.UnidadeLoteImportacao.Remove(ul);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                r.SetDefault();
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<List<VendasMesView>> GetVendas(int empid, DateTime dtini, DateTime dtfim)
        {
            SessionInfo si = await GetSessionAsync();
            List<VendasMesView> r = new();

            DateTime dt = UtilsClass.GetInicio(dtini);
            while (dt < dtfim)
            {
                VendasMesView v = new()
                {
                    DtIni = dt
                };
                r.Add(v);
                dt = UtilsClass.GetInicio(dt.AddDays(35));
            }
            r = r.OrderBy(x => x.DtIni).ToList();

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
        "           select sum(qtde) qtdevenda, DataContrato " +
        " from " +
        " ( " +
        " select Max(DataContrato) DataContrato, 1 qtde from UnidadeMovto " +
        " inner join UnidadeLoteImportacao on UnidadeLoteImportacao.Id = UnidadeMovto.UnidadeLoteImportacaoId " +
        " where DataContrato >= @dtini and  DataContrato <= @dtfim and UnidadeMovto.TenantId = @tid " +
        " and UnidadeLoteImportacao.EmpreendimentoId = @empid " +
        " group by Substring(Convert(varchar(10), DataContrato, 112), 1, 6), UnidadeEmpreendimentoId " +
        ") as t " +
        " group by t.DataContrato ";

            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = empid;

            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@dtini", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim"].Value = dtfim.ToString("yyyyMMdd");

            await command.Connection.OpenAsync();

            try
            {
                var lu = await command.ExecuteReaderAsync();
                while (await lu.ReadAsync())
                {
                    VendasMesView vv = r.FirstOrDefault(x => x.MesAno == ((DateTime)lu["DataContrato"]).ToString("MM/yyyy"));
                    if (vv != null)
                    {
                        vv.QtdVendida += (int)lu["qtdevenda"];
                    }
                }
                lu.Close();


                command.CommandText =
        "           select sum(qtde) qtdedistrato, DataDistrato " +
        " from " +
        " ( " +
        " select Max(DataDistrato) DataDistrato, 1 qtde from UnidadeMovto " +
        " inner join UnidadeLoteImportacao on UnidadeLoteImportacao.Id = UnidadeMovto.UnidadeLoteImportacaoId " +
        " where DataDistrato >= @dtini and  DataDistrato <= @dtfim and UnidadeMovto.TenantId = @tid " +
        " and UnidadeLoteImportacao.EmpreendimentoId = @empid " +
        " group by Substring(Convert(varchar(10), DataDistrato, 112), 1, 6), UnidadeEmpreendimentoId " +
        ") as t " +
        " group by t.DataDistrato ";

                command.Parameters["@empid"].Value = empid;
                command.Parameters["@tid"].Value = si.TenantId;
                command.Parameters["@dtini"].Value = dtini.ToString("yyyyMMdd");
                command.Parameters["@dtfim"].Value = dtfim.ToString("yyyyMMdd");

                lu = await command.ExecuteReaderAsync();
                while (await lu.ReadAsync())
                {
                    VendasMesView vv = r.FirstOrDefault(x => x.MesAno == ((DateTime)lu["DataDistrato"]).ToString("MM/yyyy"));
                    if (vv != null)
                    {
                        vv.QtdDistrato += (int)lu["qtdedistrato"];
                    }
                }
                lu.Close();

            }
            catch
            {
                command.Connection.Close();
                throw;
            }
            finally
            {
                command.Connection.Close();
            }

            r = r.Where(x => x.QtdDistrato > 0 || x.QtdVendida > 0).ToList();

            return r;
        }
        public async Task<List<VendasMesView>> GetAging(int empid, DateTime dtini, DateTime dtfim)
        {
            SessionInfo si = await GetSessionAsync();
            List<VendasMesView> r = new();

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
        "select UnidadeLoteImportacao.DataMovto,  " +
        "Sum(ValorAte30) Ate30,   " +
        "Sum(ValorAte60) Ate60,   " +
        "Sum(ValorAte90) Ate90 ,   " +
        "Sum(ValorAte120) Ate120,   " +
        "Sum(ValorApos120) Apos120,  " +
        "Sum(case when(ValorAte30 > 0) then 1 else 0 end) Qtd30,  " +
        "Sum(case when(ValorAte60 > 0) then 1 else 0 end) Qtd60,  " +
        "Sum(case when(ValorAte90 > 0) then 1 else 0 end) Qtd90,  " +
        "Sum(case when(ValorAte120 > 0) then 1 else 0 end) Qtd120,  " +
        "Sum(case when(ValorApos120 > 0) then 1 else 0 end) QtdApos120  " +
        "from UnidadeMovto  " +
        "inner join UnidadeLoteImportacao on UnidadeLoteImportacao.Id = UnidadeMovto.UnidadeLoteImportacaoId  " +
        " where UnidadeLoteImportacao.DataMovto >= @dtini and  UnidadeLoteImportacao.DataMovto <= @dtfim and UnidadeMovto.TenantId = @tid " +
        "  and UnidadeLoteImportacao.EmpreendimentoId = @empid " +
        " group by UnidadeLoteImportacao.DataMovto ";

            command.Parameters.Add(new SqlParameter("@empid", System.Data.SqlDbType.Int));
            command.Parameters["@empid"].Value = empid;

            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@dtini", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim"].Value = dtfim.ToString("yyyyMMdd");

            await command.Connection.OpenAsync();

            try
            {
                var lu = await command.ExecuteReaderAsync();
                while (await lu.ReadAsync())
                {
                    VendasMesView v = new()
                    {
                        Ate30 = Math.Round((double)lu["Ate30"], 2),
                        Ate60 = Math.Round((double)lu["Ate60"], 2),
                        Ate90 = Math.Round((double)lu["Ate90"], 2),
                        Ate120 = Math.Round((double)lu["Ate120"], 2),
                        Apos120 = Math.Round((double)lu["Apos120"], 2),
                        Qtd30 = (int)lu["Qtd30"],
                        Qtd60 = (int)lu["Qtd60"],
                        Qtd90 = (int)lu["Qtd90"],
                        Qtd120 = (int)lu["Qtd120"],
                        QtdApos120 = (int)lu["QtdApos120"],
                        DtMovto = (DateTime)lu["DataMovto"]
                    };
                    r.Add(v);
                }
                lu.Close();
            }
            catch
            {
                command.Connection.Close();
                throw;
            }
            finally
            {
                command.Connection.Close();
            }
            return r;
        }

    }
}
