using InCorpApp.Data;
using InCorpApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Blazored.SessionStorage;
using InCorpApp.Security;
using AutoMapper;

namespace InCorpApp.Services
{
    public class ClienteService:RelacionamentoService
    {
        public ClienteService(ApplicationDbContext context,
         RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager, ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper ): base(context, roleManager, userManager, sessionStorageService, sessionService, mapper)
        {
        }
        public async Task<List<Relacionamento>> FindAllAsync()
        {
            return await FindAllAsync(1);
        }
    }



}

