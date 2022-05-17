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
using InCorpApp.Security;
using Blazored.SessionStorage;
using InCorpApp.Constantes;
using AutoMapper;

namespace InCorpApp.Services
{
    public class FornecedorService : RelacionamentoService
    {

        public FornecedorService(ApplicationDbContext context,
         RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager, ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper) : base(context, roleManager, userManager, sessionStorageService, sessionService,mapper)
        {

        }


        public async Task<List<Relacionamento>> FindAllAsync()
        {

            return await FindAllAsync(2);
        }

 



    }



}

