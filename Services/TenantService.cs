using Microsoft.EntityFrameworkCore;
using InCorpApp.Data;
using InCorpApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

using InCorpApp.Constantes;
using System;

namespace InCorpApp.Services
{
    public class TenantService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public TenantService(ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }
        public async Task<List<Tenant>> GetTenantAllAsync()
        {

            return await _context.Tenant.OrderBy(x => x.Nome).ToListAsync();
        }
        public async Task<Tenant> GetTenantByIdAsync(string id)
        {

            return await _context.Tenant.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<Resultado> AddTenant(Tenant td)
        {
            Resultado r = new();

            IdentityUser u = await _userManager.FindByEmailAsync(td.EmailAdmin);
            bool novo = u == null;
            if (novo)
            {
                u = new IdentityUser()
                {
                    UserName = td.EmailAdmin,
                    Email = td.EmailAdmin
                };
            }

            await _context.Database.BeginTransactionAsync();
            try
            {
                if (novo)
                {
                    await _userManager.CreateAsync(u, "admin");
                }
                await _userManager.AddToRoleAsync(u, "Administrador");
                await _context.Tenant.AddAsync(td);
                await _context.SaveChangesAsync();
                TenantUsuario tu = new()
                {
                    UserId = u.Id,
                    TenantId = td.Id
                };
                await _context.TenantUsuario.AddAsync(tu);
                await _context.SaveChangesAsync();
                if (td.TenantApp == TenantApp.Studio)
                {
                    Empresa em = new()
                    {
                        Nome = td.Nome,
                        CNPJ = "9991" + Constante.Now.ToString("ddmmHHMMs"),
                        RazaoSocial = td.Nome,
                        TenantId = td.Id
                    };
                    await _context.Empresa.AddAsync(em);
                    await _context.SaveChangesAsync();
                    Empreendimento emp = new()
                    {
                        EmpresaId = em.Id,
                        DataInicioOperacao = Constante.Today,
                        CNPJ = em.CNPJ,
                        Nome = em.Nome,
                        RazaoSocial = em.Nome,
                        TenantId = td.Id
                    };
                    await _context.Empreendimento.AddAsync(emp);
                    await _context.SaveChangesAsync();
                }
                await _context.Database.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.ErrMsg = e.Message;
                r.Ok = false;
                return r;
            }
            r.Ok = true;
            return r;
        }
        public async Task<Resultado> DeleteTenant(string id)
        {
            Resultado r = new();


            if (await _context.Empresa.AnyAsync(x => x.TenantId == id))
            {

                r.Ok = false;
                r.ErrMsg = "Tenant com empresa associada";
                return r;
            }

            Tenant td = await _context.Tenant.FirstOrDefaultAsync(x => x.Id == id);
            if (td == null)
            {

                r.Ok = false;
                r.ErrMsg = "Tenant não encontrado";
                return r;
            }

            IdentityUser u = await _userManager.FindByEmailAsync(td.EmailAdmin);
            if (u != null)
            {
                await _userManager.DeleteAsync(u);
            }

            await _userManager.AddToRoleAsync(u, Constante.RoleAdm());

            await _context.Tenant.AddAsync(td);

            _context.TenantUsuario.RemoveRange(await _context.TenantUsuario.Where(x => x.TenantId == id).ToArrayAsync());


            _context.SaveChanges();

            r.Ok = true;
            return r;
        }
    }
}
