using Microsoft.EntityFrameworkCore;
using InCorpApp.Data;
using InCorpApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Services.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using InCorpApp.Security;
using Blazored.SessionStorage;
using InCorpApp.Constantes;
using AutoMapper;
using System;

namespace InCorpApp.Services
{
    public class RelacionamentoService : ServiceBase
    {

        protected readonly ApplicationDbContext _context;
        protected readonly UserManager<IdentityUser> _userManager;
        protected readonly RoleManager<IdentityRole> _roleManager;
        public RelacionamentoService(ApplicationDbContext context,
         RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager, ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper) : base(sessionStorageService, sessionService, mapper)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<List<Contato>> FindAllContatosAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();

            return await _context.Contato.Where(x => x.RelacionamentoId == id && x.TenantId == si.TenantId).OrderBy(x => x.Nome).ToListAsync();
        }
        public async Task<List<Relacionamento>> FindAllAsync(int tipo = 0)
        {
            SessionInfo si = await GetSessionAsync();
            if (tipo == 1)
            {
                return await _context.Relacionamento.Where(x => x.TenantId == si.TenantId && x.EhCliente == true).OrderBy(x => x.Nome).ToListAsync();
            }
            else
            {
                if (tipo == 2)
                {
                    return await _context.Relacionamento.Where(x => x.TenantId == si.TenantId && x.EhFornecedor == true).OrderBy(x => x.Nome).ToListAsync();
                }
                else
                {
                    return await _context.Relacionamento.Where(x => x.TenantId == si.TenantId).OrderBy(x => x.Nome).ToListAsync();
                }
            }
        }
        public async Task<Relacionamento> InsertIncompletoAsync(Relacionamento obj, List<Contato> contatos)
        {
            SessionInfo si = await GetSessionAsync();
            obj.TenantId = si.TenantId;

            Relacionamento r;

            if (obj.CPFCNPJ == null)
            {
                obj.CPFCNPJ = string.Empty;
            }
            if (obj.CPFCNPJ != string.Empty)
            {
                obj.CPFCNPJ = obj.CPFCNPJ.Trim();
                obj.CPFCNPJ = obj.CPFCNPJ.Replace(".", "").Replace("-", "").Replace("/", "");

                r = await _context.Relacionamento.AsNoTracking().FirstOrDefaultAsync(x => x.CPFCNPJ == obj.CPFCNPJ && x.TenantId == obj.TenantId);
                if (r != null)
                {
                    return r;
                }

            }

            if (obj.CodigoExternoFornecedor == null)
            {
                obj.CodigoExternoFornecedor = string.Empty;
            }
            if (obj.CodigoExternoCliente == null)
            {
                obj.CodigoExternoCliente = string.Empty;
            }

            if (obj.CodigoExternoFornecedor.Trim() != string.Empty)
            {
                r = await _context.Relacionamento.AsNoTracking().FirstOrDefaultAsync(x => x.CodigoExternoFornecedor == obj.CodigoExternoFornecedor && x.TenantId == obj.TenantId);
                if (r != null)
                {
                    return r;
                }
            }

            if (obj.CodigoExternoCliente.Trim() != string.Empty)
            {
                r = await _context.Relacionamento.AsNoTracking().FirstOrDefaultAsync(x => x.CodigoExternoCliente == obj.CodigoExternoCliente && x.TenantId == obj.TenantId);
                if (r != null)
                {
                    return r;
                }
            }


            r = await _context.Relacionamento.AsNoTracking().FirstOrDefaultAsync(x => x.Nome == obj.Nome && x.TenantId == obj.TenantId);
            if (r != null)
            {
                return r;
            }

            obj.EhCliente = true;
            obj.EhFornecedor = true;

            _context.Relacionamento.Add(obj);
            await _context.SaveChangesAsync();

            foreach (var c in contatos)
            {
                c.Id = 0;
                c.RelacionamentoId = obj.Id;
                _context.Contato.Add(c);
            }
            await _context.SaveChangesAsync();

            return obj;
        }

        public async Task<Resultado> InsertListAsync(List<Relacionamento> objs, TipoRelacionamento tipor = TipoRelacionamento.Outra)
        {
            Resultado r = new();
            r.SetDefault();

            _context.ChangeTracker.Clear();
            await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (Relacionamento c in objs)
                {
                    await InsertAsync(c, new List<Contato>(), tipor);
                }
                await _context.Database.CommitTransactionAsync();
            }
            catch (Exception exc)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = exc.Message;
            }
            return r;
        }
        public async Task InsertAsync(Relacionamento obj, List<Contato> contatos, TipoRelacionamento tipor = TipoRelacionamento.Outra)
        {
            SessionInfo si = await GetSessionAsync();

            if (obj.Email != string.Empty)
            {
                if (await _context.Relacionamento.AnyAsync(x => x.Email == obj.Email && si.TenantId == si.TenantId))
                {
                    throw new NotFoundException("Email já utilizado por outro registro");
                }
            }
            obj.TenantId = si.TenantId;
            obj.CPFCNPJ = obj.CPFCNPJ.Trim();
            obj.CPFCNPJ = obj.CPFCNPJ.Replace(".", "").Replace("-", "").Replace("/", "");

            if (obj.Tipo == 1 && tipor == TipoRelacionamento.Outra)
            {
                obj.RazaoSocial = obj.Nome;
            }

            if (obj.CPFCNPJ == "999")
            {
                obj.CPFCNPJ = "999" + Constante.Now.ToString("mmss" + si.Seq.ToString("D4"));
            }
            else
            {
                CNPJValidador cn = new(obj.CPFCNPJ);
                if (tipor == TipoRelacionamento.Aluno || tipor == TipoRelacionamento.Professor || obj.Tipo == 1)
                {
                    if (!cn.ValidaCPF(obj.CPFCNPJ))
                    {
                        throw new NotFoundException("CPF inválido");
                    }
                }
                else
                {
                    if (!cn.Valido)
                    {
                        throw new NotFoundException("CNPJ inválido:");
                    }
                }
            }
            if (tipor == TipoRelacionamento.Outra)
            {
                Relacionamento r = await _context.Relacionamento.AsNoTracking().FirstOrDefaultAsync(x => x.CPFCNPJ == obj.CPFCNPJ && x.TenantId == obj.TenantId && x.Id != obj.Id);
                if (r != null)
                {
                    throw new NotFoundException("CNPJ/CPF já cadastrado para:" + r.Nome);
                }
            }
            else
            {
                if (tipor == TipoRelacionamento.Aluno)
                {
                    obj.Tipo = 1;
                    Relacionamento r = await _context.Relacionamento.AsNoTracking().FirstOrDefaultAsync(x => x.CPFCNPJ == obj.CPFCNPJ && x.TenantId == obj.TenantId);
                    if (r != null)
                    {
                        if (await _context.Aluno.AnyAsync(x => x.TenantId == si.TenantId && x.RelacionamentoId == r.Id))
                        {
                            throw new NotFoundException("CPF já cadastrado para o aluno:" + r.Nome);
                        }
                    }
                }
                else
                {
                    if (tipor == TipoRelacionamento.Professor)
                    {
                        obj.Tipo = 1;
                        Relacionamento r = await _context.Relacionamento.AsNoTracking().FirstOrDefaultAsync(x => x.CPFCNPJ == obj.CPFCNPJ && x.TenantId == obj.TenantId);
                        if (r != null)
                        {
                            if (await _context.Professor.AnyAsync(x => x.TenantId == si.TenantId && x.RelacionamentoId == r.Id))
                            {
                                throw new NotFoundException("CPF já cadastrado para o professor:" + r.Nome);
                            }
                        }
                    }
                }
            }
            if (obj.CodigoExternoFornecedor.Trim() != string.Empty)
            {
                Relacionamento r = await _context.Relacionamento.AsNoTracking().FirstOrDefaultAsync(x => x.CodigoExternoFornecedor == obj.CodigoExternoFornecedor && x.TenantId == obj.TenantId && x.Id != obj.Id);
                if (r != null)
                {
                    throw new NotFoundException("Codigo para integração fornecedor já utilizado para o relacionamento:" + r.Nome);
                }
            }

            if (obj.CodigoExternoCliente.Trim() != string.Empty)
            {
                Relacionamento r = await _context.Relacionamento.AsNoTracking().FirstOrDefaultAsync(x => x.CodigoExternoCliente == obj.CodigoExternoCliente && x.TenantId == obj.TenantId && x.Id != obj.Id);
                if (r != null)
                {
                    throw new NotFoundException("Codigo para integração cliente já utilizado para o relacionamento:" + r.Nome);
                }
            }

            bool mytran = (_context.Database.CurrentTransaction == null);
            if (mytran)
            {
                await _context.Database.BeginTransactionAsync();
            }
            try
            {
                if (obj.Id != 0)
                {
                    Relacionamento ru = await _context.Relacionamento.FirstOrDefaultAsync(x => x.Id == obj.Id && x.TenantId == obj.TenantId);
                    if (ru != null)
                    {
                        if (obj.CodigoExternoCliente != string.Empty)
                        {
                            ru.CodigoExternoCliente = obj.CodigoExternoCliente;
                        }
                        if (obj.CodigoExternoFornecedor != string.Empty)
                        {
                            ru.CodigoExternoFornecedor = obj.CodigoExternoFornecedor;
                        }
                        if (obj.CPFCNPJ != string.Empty)
                        {
                            ru.CPFCNPJ = obj.CPFCNPJ;
                        }
                        if (obj.CodigoExternoFornecedor != string.Empty || obj.CodigoExternoCliente != string.Empty || obj.CPFCNPJ != string.Empty)
                        {
                            _context.Relacionamento.Update(ru);
                        }
                    }
                    else
                    {
                        throw new NotFoundException("Relacionamento não cadastrado (id):" + obj.Id.ToString() + " - " + obj.Nome);
                    }
                }
                else
                {
                    _context.Relacionamento.Add(obj);
                }
                await _context.SaveChangesAsync();

                foreach (var c in contatos)
                {
                    c.Id = 0;
                    c.RelacionamentoId = obj.Id;
                    _context.Contato.Add(c);
                }
                await _context.SaveChangesAsync();

                if (tipor == TipoRelacionamento.Professor)
                {
                    Professor p = new()
                    {
                        Relacionamento = null,
                        RelacionamentoId = obj.Id,
                        TenantId = si.TenantId
                    };
                    await _context.Professor.AddAsync(p);
                    await _context.SaveChangesAsync();
                    var laulas = await _context.Aula.Where(x => x.TenantId == si.TenantId).ToListAsync();
                    foreach (var au in laulas)
                    {
                        ProfessorAula pa = new()
                        {
                            TenantId = si.TenantId,
                            AulaId = au.Id,
                            ProfessorId = p.Id
                        };
                        await _context.ProfessorAula.AddAsync(pa);
                    }
                    if (laulas.Count > 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    if (tipor == TipoRelacionamento.Aluno)
                    {
                        if (obj.RelacionamentoFaturaId == 0)
                        {
                            obj.RelacionamentoFaturaId = obj.Id;
                        }
                        Aluno a = new()
                        {
                            RelacionamentoId = obj.Id,
                            RelacionamentoFaturaId = obj.RelacionamentoFaturaId,
                            TenantId = si.TenantId,
                            Relacionamento = null,
                            RelacionamentoFatura = null
                        };
                        await _context.Aluno.AddAsync(a);
                        await _context.SaveChangesAsync();
                    }
                }
                if (mytran)
                {
                    await _context.Database.CommitTransactionAsync();
                }
            }
            catch
            {
                if (mytran)
                {
                    await _context.Database.RollbackTransactionAsync();
                }
                throw;
            }

        }
        public async Task<Relacionamento> FindByNomeAsync(string n)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Relacionamento.FirstOrDefaultAsync(obj => obj.Nome == n && si.TenantId == obj.TenantId);
        }
        public async Task<Relacionamento> FindByCodigoExternoAsync(string n)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Relacionamento.AsNoTracking().FirstOrDefaultAsync(obj => (obj.CodigoExternoCliente == n || obj.CodigoExternoFornecedor == n) && si.TenantId == obj.TenantId);
        }
        public async Task<Relacionamento> FindByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Fornecedor.FirstOrDefaultAsync(obj => obj.Id == id && si.TenantId == obj.TenantId);
        }
        public async Task<Relacionamento> FindByCPNJCPFAsync(string cnpjcpf)
        {
            SessionInfo si = await GetSessionAsync();

            string c = cnpjcpf.Trim();
            c = c.Replace(".", "").Replace("-", "").Replace("/", "");
            return await _context.Fornecedor.FirstOrDefaultAsync(obj => obj.CPFCNPJ == c && si.TenantId == obj.TenantId);
        }
        public async Task<bool> AnyAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Fornecedor.AnyAsync(obj => obj.Id == id && si.TenantId == obj.TenantId);
        }
        public async Task UpdateAsync(Relacionamento obj, List<Contato> contatos, TipoRelacionamento tipor)
        {
            SessionInfo si = await GetSessionAsync();

            bool hasAny = await _context.Relacionamento.AnyAsync(x => x.Id == obj.Id && x.TenantId == si.TenantId);
            if (!hasAny)
            {
                throw new NotFoundException("Codigo não encontrado.");
            }

            if (obj.Email != string.Empty)
            {
                if (await _context.Relacionamento.AnyAsync(x => x.Email == obj.Email && si.TenantId == si.TenantId && x.Id != obj.Id))
                {
                    throw new NotFoundException("Email já utilizado por outro registro");
                }
            }
            if (obj.Tipo == 1 && tipor == TipoRelacionamento.Outra)
            {
                obj.RazaoSocial = obj.Nome;
            }
            obj.CPFCNPJ = obj.CPFCNPJ.Trim();
            obj.CPFCNPJ = obj.CPFCNPJ.Replace(".", "").Replace("-", "").Replace("/", "");
            obj.TenantId = si.TenantId;
            if (obj.CPFCNPJ == "999")
            {
                obj.CPFCNPJ = "999" + Constante.Now.ToString("mmss" + si.Seq.ToString("D4"));
            }
            else
            {
                CNPJValidador cn = new(obj.CPFCNPJ);
                if (tipor == TipoRelacionamento.Aluno || tipor == TipoRelacionamento.Professor || obj.Tipo == 1)
                {
                    if (!cn.ValidaCPF(obj.CPFCNPJ))
                    {
                        throw new NotFoundException("CPF inválido");
                    }
                }
                else
                {
                    if (!cn.Valido)
                    {
                        throw new NotFoundException("CNPJ inválido:");
                    }
                }
            }
            if (tipor == TipoRelacionamento.Outra)
            {
                var xr1 = await (from Relacionamento in _context.Relacionamento select  new {Relacionamento.Nome, Relacionamento.CPFCNPJ, Relacionamento.Id, Relacionamento.TenantId }).AsNoTracking().FirstOrDefaultAsync(x => x.CPFCNPJ == obj.CPFCNPJ && x.TenantId == obj.TenantId && x.Id != obj.Id);
                if (xr1 != null)
                {
                    throw new NotFoundException("CNPJ/CPF já cadastrado para:" + xr1.Nome);
                }
            }
            else
            {
                if (tipor == TipoRelacionamento.Aluno)
                {
                    var xr2 = await (from Relacionamento in _context.Relacionamento select new { Relacionamento.Nome, Relacionamento.CPFCNPJ, Relacionamento.Id, Relacionamento.TenantId }).AsNoTracking().FirstOrDefaultAsync(x => x.CPFCNPJ == obj.CPFCNPJ && x.TenantId == obj.TenantId && x.Id != obj.Id);
                    if (xr2 != null)
                    {
                        if (await _context.Aluno.AnyAsync(x => x.TenantId == si.TenantId && x.RelacionamentoId == xr2.Id))
                        {
                            throw new NotFoundException("CPF já cadastrado para o aluno:" + xr2.Nome);
                        }
                    }
                }
                else
                {
                    if (tipor == TipoRelacionamento.Professor)
                    {
                        var xr3 = await (from Relacionamento in _context.Relacionamento select new { Relacionamento.Nome, Relacionamento.CPFCNPJ, Relacionamento.Id, Relacionamento.TenantId }).AsNoTracking().FirstOrDefaultAsync(x => x.CPFCNPJ == obj.CPFCNPJ && x.TenantId == obj.TenantId && x.Id != obj.Id);
                        if (xr3 != null)
                        {
                            if (await _context.Professor.AnyAsync(x => x.TenantId == si.TenantId && x.RelacionamentoId == xr3.Id))
                            {
                                throw new NotFoundException("CPF já cadastrado para o professor:" + xr3.Nome);
                            }
                        }
                    }
                }
            }

            var xr4 = await (from Relacionamento in _context.Relacionamento select new {Relacionamento.CodigoExternoFornecedor, Relacionamento.Nome, Relacionamento.CPFCNPJ, Relacionamento.Id, Relacionamento.TenantId }).AsNoTracking().FirstOrDefaultAsync(x => x.CodigoExternoFornecedor == obj.CodigoExternoFornecedor && x.TenantId == obj.TenantId && x.Id != obj.Id);

            if (xr4 != null)
            {
                if (xr4.CodigoExternoFornecedor.Length > 0)
                {
                    throw new NotFoundException("Codigo para integração já utilizado para o cliente:" + xr4.Nome);
                }
            }

            _context.Database.BeginTransaction();
            try
            {
                _context.Relacionamento.Update(obj);
                await _context.SaveChangesAsync();

                var cs = await FindAllContatosAsync(obj.Id);
                foreach (var c in cs)
                {
                    _context.Contato.Remove(c);
                }
                await _context.SaveChangesAsync();

                foreach (var c in contatos)
                {
                    c.Id = 0;
                    c.RelacionamentoId = obj.Id;
                    _context.Contato.Add(c);
                }
                await _context.SaveChangesAsync();

                if (tipor == TipoRelacionamento.Aluno)
                {
                    Aluno a = await _context.Aluno.FirstOrDefaultAsync(x => x.RelacionamentoId == obj.Id && x.TenantId == si.TenantId);
                    if (a != null && a.RelacionamentoFaturaId != obj.RelacionamentoFaturaId)
                    {
                        a.RelacionamentoFaturaId = obj.RelacionamentoFaturaId;
                        _context.Aluno.Update(a);
                        await _context.SaveChangesAsync();
                    }
                }

                _context.Database.CommitTransaction();
            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;
            }
        }
        public async Task DeleteAsync(Relacionamento obj, TipoRelacionamento tipor = TipoRelacionamento.Outra)
        {
            SessionInfo si = await GetSessionAsync();
            if (!await _context.Fornecedor.AnyAsync(x => x.Id == obj.Id && x.TenantId == obj.TenantId))
            {
                throw new NotFoundException("Codigo não encontrado.");
            }
            if (si.TenantApp == TenantApp.InCorp)
            {
                if (await _context.LanctoEmpRelacionamento.AnyAsync(x => x.RelacionamentoId == obj.Id && x.TenantId == obj.TenantId))
                {
                    throw new NotFoundException("Fornecedor com movimentação não pode ser excluído!");
                }
                if (await _context.MovtoBanco.AnyAsync(x => x.RelacionamentoId == obj.Id && x.TenantId == obj.TenantId))
                {
                    throw new NotFoundException("Fornecedor com movimentação não pode ser excluído!");
                }
            }

            var cs = await FindAllContatosAsync(obj.Id);

            await _context.Database.BeginTransactionAsync();
            try
            {
                if (tipor == TipoRelacionamento.Professor)
                {
                    Professor p = await _context.Professor.FirstOrDefaultAsync(x => x.RelacionamentoId == obj.Id && x.TenantId == si.TenantId);
                    if (p != null)
                    {
                        await _context.Database.ExecuteSqlRawAsync(" Delete ProfessorAula where TenantId = '" + si.TenantId + "'  and ProfessorId = " + p.Id.ToString() );
                        await _context.SaveChangesAsync();
                        _context.Professor.Remove(p);
                        await _context.SaveChangesAsync();
                    }
                }

                if (tipor == TipoRelacionamento.Aluno)
                {
                    Aluno a = await _context.Aluno.FirstOrDefaultAsync(x => x.RelacionamentoId == obj.Id && x.TenantId == si.TenantId);
                    if (a != null)
                    {
                        _context.Aluno.Remove(a);
                        await _context.SaveChangesAsync();
                    }
                }
                foreach (var c in cs)
                {
                    _context.Contato.Remove(c);
                }
                await _context.SaveChangesAsync();
                _context.Relacionamento.Remove(obj);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }

        }
        public async Task DeleteContatoAsync(Contato c)
        {
            if (c != null)
            {
                _context.Contato.Remove(c);
                await _context.SaveChangesAsync();

            }
        }
        public async Task AddContatoAsync(Contato c)
        {
            SessionInfo si = await GetSessionAsync();
            if (c != null)
            {
                c.TenantId = si.TenantId;
                _context.Contato.Add(c);
                await _context.SaveChangesAsync();

            }
        }
        public async Task UpdateContatoAsync(Contato c)
        {
            SessionInfo si = await GetSessionAsync();
            if (c != null)
            {
                c.TenantId = si.TenantId;
                _context.Contato.Update(c);
                await _context.SaveChangesAsync();
            }
        }




    }
}
