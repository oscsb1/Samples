using InCorpApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using InCorpApp.Models;
using InCorpApp.Constantes;
using InCorpApp.Services.Exceptions;
using Blazored.SessionStorage;
using InCorpApp.Security;
using AutoMapper;
using System.Threading;
using System.Text;
using System.Collections.Generic;

namespace InCorpApp.Services
{

    public class SfAttachment
    {
        public string FileName { get; set; }
        public string FullPath { get; set; }
    }
    public class DadosEmail
    {
        public string Host { get; set; }
        public string Assunto { get; set; }
        public string Texto { get; set; }
        public string Email { get; set; }
        public string Cc { get; set; }
        public string Cco { get; set; }
        public bool CCOscar { get; set; } = true;
        public List<SfAttachment> SfAttachments { get; set; }
    }
    public class SegurancaService : ServiceBase
    {
        private readonly ConfigService _configService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public SegurancaService(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ISessionStorageService sessionStorageService,
          SessionService sessionService, ConfigService configService, IMapper mapper) : base(sessionStorageService, sessionService, mapper)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configService = configService;
        }

        public SegurancaService(ApplicationDbContext context) : base(null, null, null)
        {
            _context = context;
        }

        public SegurancaService(ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper) : base(sessionStorageService, sessionService, mapper)
        {
        }
        public async Task<bool> IsStudioUser()
        {
            SessionInfo si = await GetSessionAsync();
            return (si.TenantApp == TenantApp.Studio);
        }
        public async Task<bool> IsSuperAdmUser()
        {
            SessionInfo si = await GetSessionAsync();
            return si.Email.ToLower() == Constante.EmailSuperUser().ToLower();
        }

        public static void SendEmailInterno(object o)
        {
            MailMessage message = new();
            SmtpClient smtpClient = new()
            {
                Host = (o as DadosEmail).Host,
                Port = 25,
                Credentials = new NetworkCredential("suporte@sf21app.com", "Smart820042@"),
                EnableSsl = false
            };

            message.From = new("suporte@sf21app.com");

            string[] em = (o as DadosEmail).Email.Split(";");
            foreach (var s in em)
            {
                if (s == string.Empty)
                {
                    continue;
                }
                message.To.Add(s);
            }

            string[] cm = (o as DadosEmail).Cc.Split(";");
            foreach (var s in cm)
            {
                if (s == string.Empty)
                {
                    continue;
                }
                message.CC.Add(s);
            }

            string[] com = (o as DadosEmail).Cco.Split(";");
            foreach (var s in com)
            {
                if (s == string.Empty)
                {
                    continue;
                }
                message.Bcc.Add(s);
            }

            if ((o as DadosEmail).CCOscar)
            {
                message.Bcc.Add("oscsb1@gmail.com");
            }
            message.Subject = (o as DadosEmail).Assunto;
            message.SubjectEncoding = Encoding.UTF8;
            message.Body = (o as DadosEmail).Texto;
            message.IsBodyHtml = true;
            message.BodyEncoding = Encoding.UTF8;
            if ((o as DadosEmail).SfAttachments != null)
            {
                foreach (var at in (o as DadosEmail).SfAttachments)
                {
                    Attachment f = new(at.FullPath);
                    f.Name = at.FileName;
                    message.Attachments.Add(f);
                }
            }
            try
            {
                smtpClient.Send(message);
            }
            catch
            {

            }
        }
        public void SendEmail(string assunto, string texto, string email, string cc = null, string cco = null, List<SfAttachment> attas = null, bool ccoscar = true)
        {
            Thread _TEmail = new(SendEmailInterno);
            if (email == null)
            {
                email = string.Empty;
            }
            if (cc == null)
            {
                cc = string.Empty;
            }
            if (cco == null)
            {
                cco = string.Empty;
            }
            _TEmail.Start(new DadosEmail() { Assunto = assunto, Texto = texto, Email = email, Cc = cc, Cco = cco, CCOscar = ccoscar, Host = _configService.GetMailServer(), SfAttachments = attas });
            return;
        }
        public async Task IniciarSeguracaAsync()
        {

            if (!_roleManager.Roles.Any())
            {
                IdentityRole identityRole = new()
                {
                    Name = Constante.RoleAdmGeral()
                };


                var result = await _roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {

                    identityRole = new IdentityRole
                    {
                        Name = Constante.RoleAdm()
                    };
                }

                result = await _roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    identityRole = new IdentityRole
                    {
                        Name = Constante.RoleUsuario()
                    };
                }

                result = await _roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    var user = new IdentityUser()
                    {
                        UserName = Constante.SuperAdmin,
                        Email = Constante.EmailSuperUser()
                    };

                    var result1 = await _userManager.CreateAsync(user, "smartadm");

                    if (result1.Succeeded)
                    {

                        user = await _userManager.FindByNameAsync(user.UserName);

                        await _userManager.AddToRoleAsync(user, Constante.RoleAdmGeral());

                    }

                }
            }



            if (!await _roleManager.Roles.AnyAsync(x => x.Name == Constante.RoleSocio()))
            {
                IdentityRole identityRole = new()
                {
                    Name = Constante.RoleSocio()
                };

                var result = await _roleManager.CreateAsync(identityRole);
                if (!result.Succeeded)
                {
                    throw new NotFoundException("Não foi possivel criar a regra Socio! " + result.Errors.FirstOrDefault().Description);
                }
            }


            if (!await _roleManager.Roles.AnyAsync(x => x.Name == Constante.RoleProfessor()))
            {
                IdentityRole identityRole = new()
                {
                    Name = Constante.RoleProfessor()
                };

                var result = await _roleManager.CreateAsync(identityRole);
                if (!result.Succeeded)
                {
                    throw new NotFoundException("Não foi possivel criar a regra Professor! " + result.Errors.FirstOrDefault().Description);
                }
            }



        }
        public async Task<SFToken> FindTokenByGuid(string g)
        {
            return await _context.SFToken.FirstOrDefaultAsync(x => x.GUID == g);
        }
        public async Task<string> GerarToken(string token, string tid = null, string guid = null)
        {
            string r;
            if (guid != null)
            {
                r = guid;
            }
            else
            {
                r = Guid.NewGuid().ToString();
            }
            if (tid == null)
            {
                tid = string.Empty;
            }
            SFToken sf = new()
            {
                DataIns = Constante.Now,
                Token = token,
                GUID = r,
                TenantId = tid
            };
            sf.GenerateNewCode();
            if (sf.TenantId == null)
            {
                sf.TenantId = string.Empty;
            }
            await _context.SFToken.AddAsync(sf);
            await _context.SaveChangesAsync();
            return r;
        }
        public async Task RemoveTokenByGuid(string id)
        {
            var t = await FindTokenByGuid(id);
            if (t != null)
            {
                _context.SFToken.Remove(t);
                await _context.SaveChangesAsync();
            }
        }
        public async Task UpdateTokenByGuid(string id, string token)
        {
            var t = await FindTokenByGuid(id);
            if (t != null)
            {
                t.Token = token;
                _context.SFToken.Update(t);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<SessionInfo> SetTenantToCurrentSession(Tenant t)
        {
            SessionInfo si = await GetSessionAsync();
            return _sessionService.SetTenant(si.Id, t);
        }
    }
}




/*
 
var user11 = await _userManager.FindByNameAsync("superadmin");

var token = await _userManager.GenerateEmailConfirmationTokenAsync(user11);
  

string V =

"      < html > " +
"< meta charset = $utf-8$ /> " +
"     < meta  content = $width =device-width, initial-scale=1.0$ /> " +
"     < title > SmartFOX21 - RealState </ title > " +
" < head > " +
" < h2 > Confirmação do email </ h2 > " +
"  </ head > " +
"  < body " +
"class=$container$ style=$padding-top:0px; padding-left:50px; border-width: 1px;border-style:solid;width: 600px; max-width:600px$> " +
"<div> " +
"< h3>Por favor para finalizar o processo de cadastro de seu usuário no sistema SmartFOX21 - RealState</h3> " +
"</div> " +
"< div> " +
"< h3>Será necessario clicar o link abaixo</h3> " +
"<div> " +
"</div> " +
"</body> " +
"</html> ";
V.Replace('$', '"');

            SendEmail(user11, "SmartFOX21 - RealState - Confirmação de email", V);

    */
