using AutoMapper;
using Blazored.SessionStorage;
using InCorpApp.Data;
using InCorpApp.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Models;
using Microsoft.EntityFrameworkCore;
using InCorpApp.Constantes;

namespace InCorpApp.Services
{
    public class TituloFilter
    {
        public int CompanyId { get; set; }
        public TipoTitulo TipoTitulo { get; set; }
        public DateTime DtIni { get; set; }
        public DateTime DtFim { get; set; }
        public int RelacionamentoId { get; set; }
        public List<StatusParcela> StatusParcelas { get; set; } = new();
        public string Documento { get; set; } = string.Empty;
    }
    public class TituloService : ServiceBase
    {
        private readonly ApplicationDbContext _context;
        public TituloService(ApplicationDbContext context,
                    ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper ) : base(sessionStorageService, sessionService, mapper)
        {
            _context = context;
        }
        public async Task<Resultado> AddTitulos(List<Titulo> titulos)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var t in titulos)
                {
                    t.TenantId = si.TenantId;
                    t.DataInclusao = Constante.Today;
                    await _context.Titulo.AddAsync(t);
                    await _context.SaveChangesAsync();
                    int ct = 0;

                    if (t.Frequencia == TituloFrequencia.Mensal)
                    {
                        ct++;
                        t.Parcelas[0].TenantId = si.TenantId;
                        t.Parcelas[0].TituloId = t.Id;
                        t.Parcelas[0].Status = StatusParcela.Aberto;
                        DateTime dv = t.Parcelas[0].DataVencto;
                        int d = int.Parse(t.Parcelas[0].DataVencto.ToString("dd"));
                        int m = int.Parse(t.Parcelas[0].DataVencto.ToString("MM"));
                        int a = int.Parse(t.Parcelas[0].DataVencto.ToString("yyyy"));

                        int totp = 1;
                        while (dv < t.GerarParcelasAte)
                        {
                            ct++;
                            totp++;
                            int diav = d;
                            if (m == 12)
                            {
                                m = 1;
                                a++;
                            }
                            else
                            {
                                m++;
                            }
                            if (d > 28 && m == 2)
                            {
                                diav = 28;
                            }
                            if (d == 31 && (m == 4 || m == 6 || m == 9 || m == 11))
                            {
                                diav = 30;
                            }
                            dv = new DateTime(a, m, diav);
                        }
                        t.Parcelas[0].Numero = t.Id.ToString() + " 1/" + totp.ToString();
                        await _context.TituloParcela.AddAsync(t.Parcelas[0]);

                        dv = t.Parcelas[0].DataVencto;
                        d = int.Parse(t.Parcelas[0].DataVencto.ToString("dd"));
                        m = int.Parse(t.Parcelas[0].DataVencto.ToString("MM"));
                        a = int.Parse(t.Parcelas[0].DataVencto.ToString("yyyy"));
                        ct = 1;
                        while (dv < t.GerarParcelasAte)
                        {
                            ct++;
                            int diav = d;
                            if (m == 12)
                            {
                                m = 1;
                                a++;
                            }
                            else
                            {
                                m++;
                            }
                            if (d > 28 && m == 2)
                            {
                                diav = 28;
                            }
                            if (d == 31 && (m == 4 || m == 6 || m == 9 || m == 11))
                            {
                                diav = 30;
                            }
                            dv = new DateTime(a, m, diav);
                            TituloParcela np = new()
                            {
                                DataInclusao = Constante.Today,
                                Status = StatusParcela.Aberto,
                                TenantId = si.TenantId,
                                DataVencto = dv,
                                TituloId = t.Id,
                                Numero = t.Id.ToString() + " " + ct.ToString() + "/" + totp.ToString(),
                                Valor = t.Parcelas[0].Valor,
                                ValorEstimado = t.Parcelas[0].ValorEstimado
                            };
                            await _context.TituloParcela.AddAsync(np);
                        }
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        foreach (var p in t.Parcelas)
                        {
                            ct++;
                            p.TenantId = si.TenantId;
                            p.TituloId = t.Id;
                            p.DataInclusao = Constante.Today;
                            p.Numero = t.Id.ToString() + " " + ct.ToString() + "/" + t.Parcelas.Count.ToString();
                            await _context.TituloParcela.AddAsync(p);
                            await _context.SaveChangesAsync();
                            foreach (var m in p.Movtos)
                            {
                                m.TenantId = si.TenantId;
                                m.TituloParcelaId = p.Id;
                                await _context.TituloParcela.AddAsync(p);
                                await _context.SaveChangesAsync();
                            }
                        }
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
        public async Task<Resultado> UpdadeTitulo(Titulo t)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            await _context.Database.BeginTransactionAsync();
            try
            {
                t.TenantId = si.TenantId;
                _context.Titulo.Update(t);
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
        public async Task<Resultado> AddTituloParcela(int tituloid, TituloParcela p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            try
            {
                p.TenantId = si.TenantId;
                p.TituloId = tituloid;
                await _context.TituloParcela.AddAsync(p);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> UpdateTituloParcela(TituloParcela p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            try
            {
                p.TenantId = si.TenantId;
                double maxv = Math.Round(await _context.MovtoBancoTitulo.Where(x => x.TituloParcelaId == p.Id && x.TenantId == si.TenantId).SumAsync(x => x.Valor),2);
                if (p.Status != StatusParcela.Conciliado && p.Status != StatusParcela.ConciliadoParcial)
                {
                    if (maxv > 0)
                    {
                        r.Ok = false;
                        r.ErrMsg = "Situação da parcela inválida quando conciliada com movimento bancário";
                        return r;
                    }
                }
                if (p.Valor == maxv && maxv > 0)
                {
                    p.Status = StatusParcela.Conciliado;
                    p.ValorPago = maxv;

                }
                else
                {
                    if (maxv >0)
                    {
                        p.Status = StatusParcela.ConciliadoParcial;
                        p.ValorPago = maxv;
                    }
                }
                _context.TituloParcela.Update(p);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> DeleteTituloParcela(TituloParcela p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            try
            {
                if (await _context.TituloParcelaMovto.AnyAsync(x => x.TenantId == si.TenantId && x.TituloParcelaId == p.Id))
                {
                    r.Ok = false;
                    r.ErrMsg = "Parcela do título com movimento não pode ser excluída.";
                    return r;
                }
                var lmp = await _context.MovtoBancoTitulo.Where(x => x.TituloParcelaId == p.Id && x.TenantId == si.TenantId).ToListAsync();
                _context.MovtoBancoTitulo.RemoveRange(lmp);
                await _context.SaveChangesAsync();
                foreach (var mp in lmp )
                {
                    var mb = await _context.MovtoBanco.FirstOrDefaultAsync(x => x.Id == mp.MovtoBancoId);
                    if (mb != null)
                    {
                        mb.StatusConciliacao = StatusConciliacao.Pendente;
                        _context.MovtoBanco.Update(mb);
                    }
                }
                _context.TituloParcela.Remove(p);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> AddTituloParcelaMovto(int parcelaid, TituloParcelaMovto m)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            try
            {
                m.TenantId = si.TenantId;
                m.TituloParcelaId = parcelaid;
                await _context.TituloParcelaMovto.AddAsync(m);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> UpdateTituloParcelaMovto(TituloParcelaMovto m)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            try
            {
                m.TenantId = si.TenantId;
                _context.TituloParcelaMovto.Update(m);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> DeleteTituloParcelaMovto(TituloParcelaMovto m)
        {
            Resultado r = new();
            try
            {
                _context.TituloParcelaMovto.Remove(m);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<List<TituloView>> GetTitulos(TituloFilter f)
        {
            SessionInfo si = await GetSessionAsync();
            List<TituloView> r = new();

            var q = await (from Titulo in _context.Titulo
                           join PlanoConta in _context.PlanoConta on Titulo.PlanoContaId equals PlanoConta.Id
                           join Relacionamento in _context.Relacionamento on Titulo.RelacionamentoId equals Relacionamento.Id
                           join TituloParcela in _context.TituloParcela on Titulo.Id equals TituloParcela.TituloId
                           select new { Titulo, TituloParcela, NomeConta = PlanoConta.Nome, NomeRel = Relacionamento.Nome }
                ).AsNoTracking().Where(x => x.Titulo.TenantId == si.TenantId
                     && x.TituloParcela.DataVencto >= f.DtIni
                     && x.Titulo.TipoTitulo == f.TipoTitulo
                     && x.TituloParcela.DataVencto <= f.DtFim).ToListAsync();

            List<Studio> ls = null;
            List<Empreendimento> le = null;
            foreach (var i in q)
            {
                TituloView tv = new () { Titulo = i.Titulo, TituloParcela = i.TituloParcela, PlanoContaV = i.NomeConta, RelacionamentoV = i.NomeRel };
                r.Add(tv);
                if (si.TenantApp == TenantApp.Studio)
                {
                    if (ls == null)
                    {
                        ls = await _context.Studio.Where(x => x.TenantId == si.TenantId).ToListAsync();
                    }
                    var s = ls.FirstOrDefault(x => x.Id == tv.Titulo.CompanyId);
                    tv.Titulo.CompanyName = s.Nome;
                }
                else
                {
                    if (si.TenantApp == TenantApp.InCorp)
                    {
                        if (le == null)
                        {
                            le = await _context.Empreendimento.Where(x => x.TenantId == si.TenantId).ToListAsync();
                        }
                        var e = le.FirstOrDefault(x => x.Id == tv.Titulo.CompanyId);
                        tv.Titulo.CompanyName = e.Nome;
                    }

                }
            }
            if (f.CompanyId > 0)
            {
                r = r.Where(x => x.Titulo.CompanyId == f.CompanyId).ToList();
            }
            if (f.Documento != string.Empty)
            {
                r = r.Where(x => x.Titulo.Documento.Contains(f.Documento)).ToList();
            }
            if (f.RelacionamentoId != 0)
            {
                r = r.Where(x => x.Titulo.RelacionamentoId == f.RelacionamentoId).ToList();
            }
            if (f.StatusParcelas.Count > 0)
            {
                r = r.Where(x => f.StatusParcelas.Contains(x.TituloParcela.Status)).ToList();
            }
            return r;
        }
        public async Task<List<TituloParcelaConciliacaoView>> GetConcilicacaoParcela (int id)
        {
            SessionInfo si = await GetSessionAsync();
            List<TituloView> r = new();

            return await (from MovtoBancoTitulo in _context.MovtoBancoTitulo
                           join MovtoBanco in _context.MovtoBanco on MovtoBancoTitulo.MovtoBancoId equals MovtoBanco.Id
                           join ContaCorrente in _context.ContaCorrente on MovtoBanco.ContaCorrenteId equals ContaCorrente.Id
                           select new TituloParcelaConciliacaoView()
                           {
                               ContaCorrenteId = ContaCorrente.Id,
                               ContaCorrenteNome = ContaCorrente.Nome,
                               DataMovto = MovtoBanco.DataMovto,
                               MovtoBancoId = MovtoBanco.Id,
                               Valor = MovtoBanco.Valor,
                               TenantId = MovtoBancoTitulo.TenantId,
                               TituloParcelaId = MovtoBancoTitulo.TituloParcelaId
                           }
                ).AsNoTracking().Where(x => x.TenantId == si.TenantId
                     && x.TituloParcelaId == id).ToListAsync();
        }
        public async Task<TituloView> GetTituloViewByTituloParcelaId(int id)
        {
            SessionInfo si = await GetSessionAsync();

            var q = await (from Titulo in _context.Titulo
                           join PlanoConta in _context.PlanoConta on Titulo.PlanoContaId equals PlanoConta.Id
                           join Relacionamento in _context.Relacionamento on Titulo.RelacionamentoId equals Relacionamento.Id
                           join TituloParcela in _context.TituloParcela on Titulo.Id equals TituloParcela.TituloId
                           select new { Titulo, TituloParcela, NomeConta = PlanoConta.Nome, NomeRel = Relacionamento.Nome }
                ).AsNoTracking().Where(x => x.Titulo.TenantId == si.TenantId
                     && x.TituloParcela.Id >= id).ToListAsync();

            List<Studio> ls = null;
            List<Empreendimento> le = null;
            foreach (var i in q)
            {
                TituloView tv = new() { Titulo = i.Titulo, TituloParcela = i.TituloParcela, PlanoContaV = i.NomeConta, RelacionamentoV = i.NomeRel };
                if (si.TenantApp == TenantApp.Studio)
                {
                    if (ls == null)
                    {
                        ls = await _context.Studio.Where(x => x.TenantId == si.TenantId).ToListAsync();
                    }
                    var s = ls.FirstOrDefault(x => x.Id == tv.Titulo.CompanyId);
                    tv.Titulo.CompanyName = s.Nome;
                }
                else
                {
                    if (si.TenantApp == TenantApp.InCorp)
                    {
                        if (le == null)
                        {
                            le = await _context.Empreendimento.Where(x => x.TenantId == si.TenantId).ToListAsync();
                        }
                        var e = le.FirstOrDefault(x => x.Id == tv.Titulo.CompanyId);
                        tv.Titulo.CompanyName = e.Nome;
                    }

                }
                return tv;
            }
            return null;
        }
        public async Task<Titulo> GetTituloById(int id)
        {
            SessionInfo si = await GetSessionAsync();
            var q = await (from Titulo in _context.Titulo
                           join TituloParcela in _context.TituloParcela on Titulo.Id equals TituloParcela.TituloId
                           select new { Titulo, TituloParcela }
                          ).Where(x => x.Titulo.Id == id && x.Titulo.TenantId == si.TenantId).OrderBy(x=> x.TituloParcela.DataVencto).ToListAsync();
            Titulo r = null;
            foreach (var t in q)
            {
                if (r == null)
                {
                    r = t.Titulo;
                }
                r.Parcelas.Add(t.TituloParcela);
                var pm = await _context.TituloParcelaMovto.Where(x => x.TituloParcelaId == t.TituloParcela.Id && x.TenantId == si.TenantId).OrderBy(x => x.DataMovto).ToListAsync();
                if (pm.Count > 0)
                {
                    foreach (var m in pm)
                    {
                        t.TituloParcela.Movtos.Add(m);
                    }
                }
            }
            return r;
        }
        public async Task<List<TituloParcela>> GetParcelasTitulo(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.TituloParcela.Where(x => x.TituloId == id && x.TenantId == si.TenantId).OrderBy(x => x.DataVencto).ToListAsync();
        }
        public async Task<Resultado> DeleteTituloById(int id)
        {
            Resultado r = new();
            SessionInfo si = await GetSessionAsync();
            var t = await GetTituloById(id);
            if (t == null)
            {
                r.Ok = false;
                r.ErrMsg = "Título não encontratado";
                return r;
            }
            if (t.Origem == TituloOrigem.Manual)
            {
                await _context.Database.BeginTransactionAsync();
                try
                {
                    foreach (var p in t.Parcelas)
                    {
                        if (p.Movtos.Count > 0)
                        {
                            _context.TituloParcelaMovto.RemoveRange(p.Movtos);
                            await _context.SaveChangesAsync();
                        }

                        var lmp = await _context.MovtoBancoTitulo.Where(x => x.TituloParcelaId == p.Id && x.TenantId == si.TenantId).ToListAsync();
                        _context.MovtoBancoTitulo.RemoveRange(lmp);
                        await _context.SaveChangesAsync();
                        foreach (var mp in lmp)
                        {
                            var mb = await _context.MovtoBanco.FirstOrDefaultAsync(x => x.Id == mp.MovtoBancoId);
                            if (mb != null)
                            {
                                mb.StatusConciliacao = StatusConciliacao.Pendente;
                                _context.MovtoBanco.Update(mb);
                            }
                        }
                    }
                    _context.TituloParcela.RemoveRange(t.Parcelas);
                    await _context.SaveChangesAsync();
                    _context.Titulo.Remove(t);
                    await _context.SaveChangesAsync();
                    r.Ok = true;
                    await _context.Database.CommitTransactionAsync();
                }
                catch (Exception e)
                {
                    await _context.Database.RollbackTransactionAsync();
                    r.Ok = false;
                    r.ErrMsg = e.Message;
                }
            }
            else
            {
                r.Ok = false;
                r.ErrMsg = "Somente títulos incluídos manualmente podem ser excluídos";
            }
            return r;

        }
        public async Task<Resultado> AddTitulosReceber(List<Titulo> tituloReceber)
        {
            List<Titulo> lt = new();
            foreach (var t in tituloReceber)
            {
                lt.Add(t as Titulo);
            }
            return await AddTitulos(lt);
        }
        public async Task<Resultado> UpdadeTituloReceber(Titulo t)
        {
            return await UpdadeTitulo(t);
        }
        public async Task<Resultado> AddTituloReceberParcela(int TituloReceberid, TituloParcela p)
        {
            return await AddTituloParcela(TituloReceberid, p);
        }
        public async Task<Resultado> UpdateTituloReceberParcela(TituloParcela p)
        {
            return await UpdateTituloParcela(p);
        }
        public async Task<Resultado> DeleteTituloReceberParcela(TituloParcela p)
        {
            return await DeleteTituloParcela(p);
        }
        public async Task<Resultado> AddTituloReceberParcelaMovto(int parcelaid, TituloParcelaMovto m)
        {
            return await AddTituloParcelaMovto(parcelaid, m);
        }
        public async Task<Resultado> UpdateTituloReceberParcelaMovto(TituloParcelaMovto m)
        {
            return await UpdateTituloParcelaMovto(m);
        }
        public async Task<Resultado> DeleteTituloReceberParcelaMovto(TituloParcelaMovto m)
        {
            return await DeleteTituloParcelaMovto(m);
        }
        public async Task<List<TituloView>> GetTitulosReceber(TituloFilter f)
        {
            f.TipoTitulo = TipoTitulo.ContasReceber;
            return await GetTitulos(f);
        }
        public async Task<TituloView> GetTituloReceberViewByTituloReceberParcelaId(int id)
        {
            return await GetTituloViewByTituloParcelaId(id) as TituloView;
        }
        public async Task<Titulo> GetTituloReceberById(int id)
        {
            return await GetTituloById(id);
        }
        public async Task<List<TituloParcela>> GetParcelasTituloReceber(int id)
        {
            var lp = await GetParcelasTitulo(id);
            List<TituloParcela> r = new();
            foreach (var p in lp)
            {
                r.Add(p as TituloParcela);
            }
            return r;
        }
        public async Task<Resultado> DeleteTituloReceberById(int id)
        {
            return await DeleteTituloById(id);
        }

    }
}
