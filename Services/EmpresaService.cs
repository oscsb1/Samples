using Microsoft.EntityFrameworkCore;
using InCorpApp.Data;
using InCorpApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Security;
using Blazored.SessionStorage;
using InCorpApp.Constantes;
using AutoMapper;

namespace InCorpApp.Services
{
    public class EmpresaService : ServiceBase
    {
        private readonly ApplicationDbContext _context;
        public EmpresaService(ApplicationDbContext context,
            ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper) : base(sessionStorageService, sessionService, mapper)
        {
            _context = context;
        }
        public async Task<List<Empresa>> FindAllAsync()
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Empresa.Where(x => x.TenantId == si.TenantId).OrderBy(x => x.Nome).ToListAsync();
        }
        public async Task<Resultado> AddEmpresaAsync(Empresa obj)
        {
            Resultado r = new();
            SessionInfo si = await GetSessionAsync();

            obj.CNPJ = obj.CNPJ.Trim();
            obj.CNPJ = obj.CNPJ.Replace(".", "").Replace("-", "").Replace("/", "");
            obj.TenantId = si.TenantId;

            await _context.Empresa.AddAsync(obj);
            await _context.SaveChangesAsync();

            r.Ok = true;
            return r;
        }
        public async Task<Empresa> FindByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Empresa.FirstOrDefaultAsync(obj => obj.Id == id && obj.TenantId == si.TenantId);
        }
        public async Task<Resultado> UpdateAsync(Empresa obj)
        {
            Resultado r = new();
            SessionInfo si = await GetSessionAsync();

            obj.CNPJ = obj.CNPJ.Trim();
            obj.CNPJ = obj.CNPJ.Replace(".", "").Replace("-", "").Replace("/", "");
            obj.TenantId = si.TenantId;

            bool hasAny = await _context.Empresa.AnyAsync(x => x.Id == obj.Id && x.TenantId == si.TenantId);
            if (!hasAny)
            {
                r.Ok = false;
                r.ErrMsg = "Empresa não encontrada";
                return r;
            }

            _context.Empresa.Update(obj);
            _context.SaveChanges();

            r.Ok = true;
            return r;



        }
        public async Task<Resultado> DeleteAsync(Empresa obj)
        {
            Resultado r = new();
            SessionInfo si = await GetSessionAsync();

            obj.CNPJ = obj.CNPJ.Trim();
            obj.CNPJ = obj.CNPJ.Replace(".", "").Replace("-", "").Replace("/", "");
            obj.TenantId = si.TenantId;

            bool hasAny = await _context.Empresa.AnyAsync(x => x.Id == obj.Id && x.TenantId == si.TenantId);
            if (!hasAny)
            {
                r.Ok = false;
                r.ErrMsg = Constante.ItemaNaoCadastrado("Empresa");
                return r;
            }

            hasAny = await _context.Empreendimento.AnyAsync(x => x.EmpresaId == obj.Id && x.TenantId == si.TenantId);
            if (hasAny)
            {
                r.Ok = false;
                r.ErrMsg = Constante.ItemaNaoPodeSerExcluido("Empresa");
                return r;
            }


            _context.Empresa.Remove(obj);
            await _context.SaveChangesAsync();

            r.Ok = true;
            return r;

        }
        public async Task<InCorpConfig> GetInCorpConfig()
        {
            SessionInfo si = await GetSessionAsync();
            var c = await _context.InCorpConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
            if (c == null)
            {
                c = new InCorpConfig()
                {
                    TenantId = si.TenantId
                };
                await _context.InCorpConfig.AddAsync(c);
                await _context.SaveChangesAsync();
            }
            return c;
        }
        public async Task UpdateIncorpConfigAsync(InCorpConfig s)
        {
            SessionInfo si = await GetSessionAsync();
            s.TenantId = si.TenantId;
            _context.InCorpConfig.Update(s);
            await _context.SaveChangesAsync();
        }
    }
}


