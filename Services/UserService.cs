using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using InCorpApp.Data;
using InCorpApp.Models;
using InCorpApp.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using InCorpApp.Security;
using InCorpApp.Constantes;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components;

namespace InCorpApp.Services
{
    public class UserService
    {
        private readonly ConfigService _configService;
        private readonly ApplicationDbContext _context;

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ISessionStorageService _sessionStorageService;
        private readonly SessionService _sessionService;

        public UserService(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
    ISessionStorageService sessionStorageService,
  SessionService sessionService, ConfigService configService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _sessionStorageService = sessionStorageService;
            _sessionService = sessionService;
            _configService = configService;
        }

        public UserManager<IdentityUser> UserManager { get => _userManager; }
        private static async Task<List<Usuario>> ObterListaUsuarioAsync(DbCommand command)
        {

            bool iopen = false;

            if (command.Connection.State == System.Data.ConnectionState.Closed)
            {
                iopen = true;
                await command.Connection.OpenAsync();
            }
            var lu = await command.ExecuteReaderAsync();
            List<Usuario> r = new();

            try

            {
                while (lu.Read())
                {
                    var u = new Usuario()
                    {
                        Email = lu["email"].ToString(),
                        Nome = lu["Nome"].ToString(),
                        Id = lu["Id"].ToString()
                    };
                    r.Add(u);
                };
                lu.Close();
            }
            catch
            {

            }
            finally
            {
                if (iopen)
                {
                    command.Connection.Close();
                }
            }

            return r;
        }
        public async Task SetTenantDBSession(SessionInfo si)
        {

            if (si.Email == Constante.EmailSuperUser())
            {
                return;
            }

            var c = _context.Database.GetDbConnection();
            if (c.State == System.Data.ConnectionState.Closed)
            {
                c.Open();

            }

            var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = @"exec sp_set_session_context @key=N'TenantId', @value=@TenantId";
            cmd.Parameters.Add(new SqlParameter("@TenantId", System.Data.SqlDbType.NVarChar));
            cmd.Parameters["@TenantId"].Value = si.TenantId.ToString();
            await cmd.ExecuteNonQueryAsync();

            /*
                        cmd.CommandText = @"EXECUTE AS USER = @user";
                        cmd.Parameters.Add(new SqlParameter("@user", System.Data.SqlDbType.NVarChar));
                        cmd.Parameters["@user"].Value = si.DbUser;
                        await cmd.ExecuteNonQueryAsync();

                        */




        }
        public async Task<Resultado> SetTenant(SessionInfo si)
        {
            Resultado r = new();

            if (si.Email.ToLower() == Constante.EmailSuperUser().ToLower())
            {
                r.Ok = true;
                r.ErrMsg = "";
                return r;
            }


            Socio so = await _context.Socio.FirstOrDefaultAsync(x => x.Email == si.Email);
            if (so != null)
            {
                si.SocioId = so.Id;
                si.DbUser = "sociouser";
            }
            else
            {
                si.SocioId = 0;
                si.DbUser = "normaluser";
            }
            if (so == null)
            {
                EmpSocioUser esociouser = await _context.EmpSocioUser.FirstOrDefaultAsync(x => x.Email == si.Email);
                if (esociouser != null)
                {
                    EmpreendimentoSocio empsocio = await _context.EmpreendimentoSocio.FirstOrDefaultAsync(x => x.Id == esociouser.EmpreendimentoSocioId);
                    if (empsocio != null)
                    {
                        so = await _context.Socio.FirstOrDefaultAsync(x => x.Id == empsocio.SocioId);
                        if (so != null)
                        {
                            si.SocioId = so.Id;
                            si.DbUser = "sociouser";
                        }
                    }
                }
            }

            var tu = await _context.TenantUsuario.FirstOrDefaultAsync(x => x.UserId == si.UserId);

            if (tu == null)
            {
                var put = await _context.ProfessorUsuario.FirstOrDefaultAsync(x => x.IdentityUserId == si.UserId);
                if (put != null)
                {
                    si.TenantId = put.TenantId;
                    si.TenantApp = TenantApp.Studio;
                    r.Ok = true;
                    return r;
                }
                else
                {
                    si.TenantId = string.Empty;
                    if (so == null)
                    {
                        r.Ok = false;
                        r.ErrMsg = "Usuario sem tenant!";
                        return r;
                    }
                }
            }
            else
            {
                var t = await _context.Tenant.FirstOrDefaultAsync(x => x.Id == tu.TenantId);
                if (t == null)
                {
                    si.TenantId = string.Empty;
                    if (so == null)
                    {
                        r.Ok = false;
                        r.ErrMsg = "Usuario sem tenant!";
                        return r;
                    }
                }
                else
                {
                    si.TenantId = tu.TenantId;
                    si.TenantApp = t.TenantApp;
                }
            }

            r.Ok = true;
            return r;

        }
        public async Task<IdentityUser> GetUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }
        public async Task<IdentityUser> FindByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }
        public async Task<SessionInfo> GetSessionAsync()
        {

            var accessToken = await _sessionStorageService.GetItemAsync<string>(Constante.Accesskey);
            SessionInfo si;

            if (accessToken != null && accessToken != string.Empty)
            {

                si = _sessionService.GetById(accessToken);

                if (si == null)
                {
                    throw new SessionInfoInvalidaException("Sessão inválida!");
                }
            }
            else
            {
                throw new SessionInfoInvalidaException("Sessão inválida!");

            }

            return si;

        }
        public async Task<List<Usuario>> FindAllAsync()
        {
            SessionInfo si = await GetSessionAsync();

            var command = _context.Database.GetDbConnection().CreateCommand();

            if (si.UserName == Constante.SuperAdmin)
            {
                command.CommandText = "SELECT  UserName Nome, email, Id from AspNetUsers ";
                return (await ObterListaUsuarioAsync(command));
            }
            else
            {

                Tenant t = await _context.Tenant.FirstOrDefaultAsync(x => x.Id == si.TenantId);
                if (t == null)
                {
                    throw new NotFoundException("Tenant não cadastrado => " + si.TenantId);
                }

                command.CommandText =
                "Select AspNetUsers.UserName Nome, AspNetUsers.email, AspNetUsers.Id from AspNetUsers " +
                                                " inner join TenantUsuario on TenantUsuario.UserId = AspNetUsers.Id " +
                                                " and TenantUsuario.TenantId = @tid " +
                                                " and  AspNetUsers.Id <> @userid  " +
                                                " and  AspNetUsers.Email <> @email  ";
                command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                command.Parameters["@tid"].Value = si.TenantId;
                command.Parameters.Add(new SqlParameter("@userid", System.Data.SqlDbType.NVarChar, 100));
                command.Parameters["@userid"].Value = si.UserId;
                command.Parameters.Add(new SqlParameter("@email", System.Data.SqlDbType.NVarChar, 100));
                command.Parameters["@email"].Value = t.EmailAdmin;
                return (await ObterListaUsuarioAsync(command));
            }


        }
        public async Task<Usuario> ObterAcessosUsuario()
        {
            ClaimStore cs = new();

            Usuario result = new();

            foreach (SFClaim s in cs.AllClaims()) { result.SFClaims.Add(s); }



            await _roleManager.Roles.ForEachAsync<IdentityRole>(x =>
            {
                if (x.NormalizedName != Constante.RoleAdmGeral().ToUpper() && x.NormalizedName != Constante.RoleSocio().ToUpper() && x.NormalizedName != Constante.RoleProfessor().ToUpper())
                {
                    Perfil p = new()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        NormalizedName = x.NormalizedName,
                        Selecionado = false
                    };
                    result.Perfis.Add(p);
                }
            });


            return result;

        }
        public async Task CriarUsuarioProfessor(Professor p, string email)
        {
            SessionInfo si = await GetSessionAsync();

            ProfessorUsuario pu = await _context.ProfessorUsuario.AsNoTracking().FirstOrDefaultAsync(x => x.ProfessorId == p.Id);
            IdentityUser u;
            if (pu != null)
            {
                u = await _userManager.FindByIdAsync(pu.IdentityUserId);
                if (u != null)
                {
                    if (u.Email != email)
                    {
                        var token1 = await _userManager.GenerateChangeEmailTokenAsync(u, email);
                        var result = await _userManager.ChangeEmailAsync(u, email, token1);
                        if (result.Succeeded)
                        {
                            await EnviarEmailConfirmacaoEmail(u);
                        }
                        else
                        {
                            if (result.Errors.Any())
                            {
                                if (result.Errors.First().Code == "DuplicateEmail")
                                {
                                    throw new NotFoundException("Email já cadastrado.");
                                }
                                else
                                {
                                    throw new NotFoundException(result.Errors.First().Code + " => " + result.Errors.First().Description);
                                }
                            }
                            else
                            {
                                throw new NotFoundException("Erro na criação do usuario para o socio: " + p.Nome);
                            }
                        }
                    }
                    else
                    {
                        p.AcessoLiberado = true;
                        _context.Professor.Update(p);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    _context.ProfessorUsuario.Remove(pu);
                    await _context.SaveChangesAsync();
                }
            }

            u = await _userManager.FindByEmailAsync(email);
            if (u == null)
            {
                Usuario nu = new()
                {
                    Ativo = true,
                    Email = email,
                    Nome = email
                };

                IdentityRole r = await _roleManager.FindByNameAsync(Constante.RoleProfessor());
                if (r == null)
                {
                    throw new NotFoundException("Perfil professor não encontrado");
                }
                Perfil pr = new()
                {
                    Name = r.Name,
                    NormalizedName = r.NormalizedName,
                    Id = r.Id,
                    Selecionado = true
                };

                nu.Perfis.Add(pr);
                await IncluirUsuario(nu, TipoUsuario.Professor);
                u = await _userManager.FindByEmailAsync(email);
                await EnviarEmailConfirmacaoEmail(u);
            }
            else
            {
                await _userManager.AddToRoleAsync(u, Constante.RoleProfessor());
            }

            if (!await _context.ProfessorUsuario.AnyAsync(x => x.IdentityUserId == u.Id && x.ProfessorId == p.Id && x.TenantId == si.TenantId))
            {
                await _context.ProfessorUsuario.AddAsync(new ProfessorUsuario() { IdentityUserId = u.Id, ProfessorId = p.Id, TenantId = si.TenantId });
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Resultado> CriarUsuarioEmpreendimento(EmpSocioUser s)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            if (await _context.EmpSocioUser.AnyAsync(x => x.EmpreendimentoSocioId == s.EmpreendimentoSocioId && x.TenantId == si.TenantId && x.Email == s.Email))
            {
                r.Ok = false;
                r.ErrMsg = "Email já cadastrado";
                return r;
            }
            IdentityUser u = await _userManager.FindByEmailAsync(s.Email);
            if (u != null)
            {
                await _context.Database.BeginTransactionAsync();
                try
                {
                    if (!await _userManager.IsInRoleAsync(u, Constante.RoleSocio()))
                    {
                        await _userManager.AddToRoleAsync(u, Constante.RoleSocio());
                    }
                    s.UserId = u.Id;
                    s.TenantId = si.TenantId;
                    await _context.EmpSocioUser.AddAsync(s);
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

            Usuario nu = new()
            {
                Ativo = true,
                Email = s.Email,
                Nome = s.Nome.Trim()
            };

            IdentityRole ro = await _roleManager.FindByNameAsync(Constante.RoleSocio());
            if (ro == null)
            {
                r.Ok = false;
                r.ErrMsg = "Perfil socio não encontrado";
                return r;
            }
            Perfil p = new()
            {
                Name = ro.Name,
                NormalizedName = ro.NormalizedName,
                Id = ro.Id,
                Selecionado = true
            };
            nu.Perfis.Add(p);
            try
            {
                await _context.Database.BeginTransactionAsync();
                await IncluirUsuario(nu, TipoUsuario.Socio);
                u = await _userManager.FindByEmailAsync(s.Email);
                s.UserId = u.Id;
                s.TenantId = si.TenantId;
                await _context.EmpSocioUser.AddAsync(s);
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
        public async Task<Resultado> UpdateUsuarioEmpreendimento(EmpSocioUser s)
        {
            Resultado r = new(); ;
            try
            {
                _context.EmpSocioUser.Update(s);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> RemoverUsuarioEmpreendimento(EmpSocioUser s)
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
                _context.EmpSocioUser.Remove(s);
                await _context.SaveChangesAsync();
                if (!await _context.SocioUsuario.AnyAsync(x => x.IdentityUserId == s.UserId) &&
                    !await _context.TenantUsuario.AnyAsync(x => x.UserId == s.UserId) &&
                    !await _context.ProfessorUsuario.AnyAsync(x => x.IdentityUserId == s.UserId))
                {
                    IdentityUser u = await _userManager.FindByIdAsync(s.UserId);
                    if (u != null)
                    {
                        await _userManager.RemoveFromRoleAsync(u, Constante.RoleSocio());
                        await _userManager.DeleteAsync(u);
                    }
                }
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
        public async Task CriarUsuarioSocio(Socio s)
        {
            SessionInfo si = await GetSessionAsync();

            SocioUsuario su = await _context.SocioUsuario.AsNoTracking().FirstOrDefaultAsync(x => x.SocioId == s.Id);
            IdentityUser u;
            if (su != null)
            {
                u = await _userManager.FindByIdAsync(su.IdentityUserId);
                if (u != null)
                {
                    if (u.Email != s.Email)
                    {
                        var nuser = await _userManager.FindByEmailAsync(s.Email);
                        if (nuser != null)
                        {
                            su.IdentityUserId = nuser.Id;
                            _context.SocioUsuario.Update(su);
                            await _context.SaveChangesAsync();
                            return;
                        }
                        var token1 = await _userManager.GenerateChangeEmailTokenAsync(u, s.Email);
                        var result = await _userManager.ChangeEmailAsync(u, s.Email, token1);
                        if (result.Succeeded)
                        {
                            await EnviarEmailConfirmacaoEmail(u);
                        }
                        else
                        {
                            if (result.Errors.Any())
                            {
                                if (result.Errors.First().Code == "DuplicateEmail")
                                {
                                    throw new NotFoundException("Email já cadastrado.");
                                }
                                else
                                {
                                    throw new NotFoundException(result.Errors.First().Code + " => " + result.Errors.First().Description);
                                }
                            }
                            else
                            {
                                throw new NotFoundException("Erro na criação do usuario para o socio: " + s.Email);
                            }
                        }


                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    _context.SocioUsuario.Remove(su);
                    await _context.SaveChangesAsync();
                }
            }

            u = await _userManager.FindByEmailAsync(s.Email);
            if (u == null)
            {
                Usuario nu = new()
                {
                    Ativo = true,
                    Email = s.Email,
                    Nome = s.Email
                };

                IdentityRole r = await _roleManager.FindByNameAsync(Constante.RoleSocio());
                if (r == null)
                {
                    throw new NotFoundException("Perfil socio não encontrado");
                }
                Perfil p = new()
                {
                    Name = r.Name,
                    NormalizedName = r.NormalizedName,
                    Id = r.Id,
                    Selecionado = true
                };

                nu.Perfis.Add(p);
                await IncluirUsuario(nu, TipoUsuario.Socio);
                u = await _userManager.FindByEmailAsync(s.Email);
            }
            else
            {
                await _userManager.AddToRoleAsync(u, Constante.RoleSocio());
            }

            if (!await _context.SocioUsuario.AnyAsync(x => x.IdentityUserId == u.Id && x.SocioId == s.Id))
            {
                await _context.SocioUsuario.AddAsync(new SocioUsuario() { IdentityUserId = u.Id, SocioId = s.Id });
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Resultado> EnviarEmailConfirmacaoEmail(IdentityUser u, string assunto = "SmartFOX - Confirmação do email e cadastramento de senha para acesso ao sistema.")
        {
            Resultado r = new();
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(u);
            OrigemToken o = OrigemToken.Seguranca;
            OperacaoToken op = OperacaoToken.ConfirmarEmail;
            int io = (int)o;
            int iop = (int)op;
            string nt = io.ToString() + ";" + iop.ToString() + ";" + u.Id + ";" + token;
            string g = await GerarToken(nt);
            SFToken sftoken = await FindTokenByGuid(g);
            SegurancaService sgs = new(_context, _userManager, _roleManager, _sessionStorageService, _sessionService, _configService, null);
            try
            {

                string texto =
                    "<!DOCTYPE html> " +
                    " <head> " +
                    "<meta charset = 'utf-8' /> " +
                    "<title> SmartFOX - Sistema de gestão empresarial </title >  " +
                    "</head> " +
                    "<body> " +
                    "<div style='border: 1px solid 4e226c;' >" +
                    "<p style = 'background-color: #4e226c;text-align:center;margin:50px;margin-top:30px;margin-bottom:0px;font-size:30px;font-weight:bold; font-family:verdana;color:#4e226c;min-height:10px'>.</p>" +
                    "<p style = 'background-color: #4e226c;text-align:center;margin:50px;margin-bottom:0px;padding:0px;margin-top:0px;font-size:24px;font-weight:bold; font-family:verdana;color:white;min-height:60px'> " +
                    "SmartFOX - Sistema de gestão empresarial</p> " +
                    "<p style = 'background-color: #4e226c;text-align:center;margin:50px;margin-top:0px;margin-bottom:0px;font-size:30px;font-weight:bold; font-family:verdana;color:#4e226c;min-height:10px'>.</p>" +
                    "<p style = 'background-color: white;text-align:left;margin:50px;margin-bottom:0px;margin-top:20px;padding-left:10px;font-size:16px;font-weight:normal;font-family:verdana;'> " +
                    "Olá " + u.Email + " ! </p>" +
                    "<p style = 'background-color: white;text-align:left;margin:50px;margin-top:30px;margin-left:100px;margin-bottom:10px;padding-left:10px;font-size:16px;font-weight:normal; font-family:verdana;'> " +
                "Você recebeu esse email para definição de sua senha de acesso ao sistema, siga os passos abaixo:  </p>" +
                    "<p style = 'background-color: white;text-align:left;margin-left:100px;margin-top:10px;padding-left:10px;font-size:16px;font-weight:normal; font-family:verdana;'> " +
                    "1. Acessar o sistema através da url sf21app.com  </p>" +
                    "<p style = 'background-color: white;text-align:left;margin-left:100px;margin-top:10px;padding-left:10px;font-size:16px;font-weight:normal;font-family:verdana;'> " +
                    "2. Na tela de login, informar o seu email e clicar no botão login </p>" +
                    "<p style = 'background-color: white;text-align:left;margin-left:100px;margin-top:10px;padding-left:10px;font-size:16px;font-weight:normal;font-family:verdana;'> " +
                    "3. Na próxima tela informar o código de segurança abaixo e digitar a nova senha  </p>" +
                    "<p style = 'background-color: whitesmoke;text-align:center;margin:50px;padding:30px;font-size:20px;font-weight:bold;font-family: verdana;'>  " +
                    "Código: " + sftoken.Code + "</p>" + 
                    "<p style = 'background-color: white;text-align:left;margin-top:30px;margin-left:100px;padding-left:10px;font-size:14px;font-weight:normal; font-family:verdana ;'> " +
                    "O código é válido por 72 horas, após esse prazo vocé deverá solicitar um novo código atraves do link sf21app.com  'Esqueci a senha'  </p>" +
                    "<p style = 'background-color: white;text-align:left;margin-top:10px;margin-left:100px;padding-left:10px;font-size:14px;font-weight:normal; font-family: verdana; '> " +
                    "Se tiver algima dúvida, enviar um email para suporte@sf21app.com </p>" +
                    "</div>" +
                    "</body>" +
                    "</html> ";

                sgs.SendEmail(assunto, texto, u.Email);
                r.Ok = true;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<IEnumerable<Claim>> GetClaims(IdentityUser user)
        {
            return await _userManager.GetClaimsAsync(user);
        }
        public async Task<IEnumerable<string>> GetRoles(IdentityUser user)
        {
            return await _userManager.GetRolesAsync(user); ;
        }
        public async Task<Usuario> ObterAcessosUsuario(IdentityUser user)
        {

            var result = new Usuario
            {
                Nome = user.UserName,
                Email = user.Email,
                Id = user.Id
            };

            var uc = await _userManager.GetClaimsAsync(user);
            var ur = await _userManager.GetRolesAsync(user);

            ClaimStore cs = new();

            foreach (SFClaim sf in cs.AllClaims())
            {
                if (sf.ControlaAcesso)
                {
                    sf.Selecionado = uc.Any(x => x.Type == sf.Type);
                }
                result.SFClaims.Add(sf);
            };

            await _roleManager.Roles.ForEachAsync<IdentityRole>(x =>
           {
               if (x.NormalizedName != Constante.RoleAdmGeral().ToUpper() && x.NormalizedName != Constante.RoleSocio().ToUpper() && x.NormalizedName != Constante.RoleProfessor().ToUpper())
               {
                   var p = new Perfil()
                   {
                       Id = x.Id,
                       Name = x.Name,
                       NormalizedName = x.NormalizedName,
                       Selecionado = ur.Any(y => y == x.Name)
                   };
                   result.Perfis.Add(p);
               }
           });

            result.Ativo = (user.LockoutEnd == null || user.LockoutEnd < Constante.Today);

            return (result);

        }
        public async Task<string> GetIdByName(string nome)
        {
            var user = await _userManager.FindByNameAsync(nome);
            if (user != null)
            {
                return user.Id;

            }
            else { return ""; };
        }
        public async Task<Resultado> AddUserToPerfil(string email, string role)
        {
            Resultado r = new();
            var u = await _userManager.FindByEmailAsync(email);
            try
            {
                await _context.Database.BeginTransactionAsync();
                if (u != null)
                {
                    await _userManager.RemoveFromRoleAsync(u, role);
                    await _userManager.AddToRoleAsync(u, role);
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
        public async Task IncluirUsuario(Usuario usuario, TipoUsuario tipo)
        {
            SessionInfo si = await GetSessionAsync();
            IdentityUser eu = await _userManager.FindByEmailAsync(usuario.Email);
            if (eu != null)
            {
                if (tipo == TipoUsuario.Usuario)
                {
                    TenantUsuario tu = await _context.TenantUsuario.FirstOrDefaultAsync(x => x.UserId == eu.Id && x.TenantId == si.TenantId);
                    if (tu != null)
                    {
                        return;
                    }
                    tu = new();
                    tu.UserId = eu.Id;
                    tu.TenantId = si.TenantId;
                    await _context.TenantUsuario.AddAsync(tu);
                    await _context.SaveChangesAsync();
                    return;
                }
            }
            IdentityUser user = new()
            {
                UserName = usuario.Nome.Replace(" ", ""),
                Email = usuario.Email
            };

            bool mytrans = (_context.Database.CurrentTransaction == null);
            if (mytrans)
            {
                _context.Database.BeginTransaction();
            }
            try
            {

                var result = await _userManager.CreateAsync(user, Constante.DefaultPassword);
                if (result.Succeeded)
                {
                    foreach (Perfil p in usuario.Perfis)
                    {
                        if (p.Selecionado)
                            await _userManager.AddToRoleAsync(user, p.Name);
                    }

                    foreach (SFClaim sf in usuario.SFClaims)
                    {
                        await _userManager.RemoveClaimAsync(user, new Claim(sf.Type, sf.Value));

                        if (sf.Selecionado)
                        {
                            await _userManager.AddClaimAsync(user, new Claim(sf.Type, sf.Value));

                        }
                    }

                    if (tipo == TipoUsuario.Usuario)
                    {
                        await EnviarEmailConfirmacaoEmail(user);
                    }
                }
                else
                {
                    if (result.Errors.Any())
                    {
                        if (result.Errors.First().Code == "DuplicateEmail")
                        {
                            throw new NotFoundException("Email já cadastrado.");
                        }
                        if (result.Errors.First().Code == "DuplicateUserName")
                        {
                            throw new NotFoundException("Nome já utilizado por outro usuário");
                        }
                        if (result.Errors.First().Code == "InvalidUserName")
                        {
                            throw new NotFoundException("Nome do usuario contém caracter inválido, digitar somente letras e numéros.");
                        }
                        else
                        {
                            throw new NotFoundException(result.Errors.First().Code + " => " + result.Errors.First().Description);
                        }
                    }
                }
                if (tipo == TipoUsuario.Usuario)
                {
                    TenantUsuario tu = new();
                    eu = await _userManager.FindByEmailAsync(usuario.Email);
                    tu.UserId = eu.Id;
                    tu.TenantId = si.TenantId;
                    await _context.TenantUsuario.AddAsync(tu);
                    await _context.SaveChangesAsync();
                }
                if (mytrans)
                {
                    _context.Database.CommitTransaction();
                }
            }
            catch
            {
                if (mytrans)
                {
                    _context.Database.RollbackTransaction();
                }
                throw;
            }
        }
        public async Task<List<Tenant>> FindAllTenants(string id)
        {
            List<Tenant> r = new();
            var i = await (from TenantUsuario in _context.TenantUsuario
                           join Tenant in _context.Tenant on TenantUsuario.TenantId equals Tenant.Id
                           select new { Tenant, TenantUsuario }).Where(x => x.TenantUsuario.UserId == id).ToListAsync();

            foreach (var t in i)
            {
                r.Add(t.Tenant);
            }

            var lu = await (from EmpSocioUser in _context.EmpSocioUser
                            join Tenant in _context.Tenant on EmpSocioUser.TenantId equals Tenant.Id
                            select new { Tenant, EmpSocioUser }).Where(x => x.EmpSocioUser.UserId == id).ToListAsync();

            foreach (var u in lu)
            {
                if (!r.Any(x => x.Id == u.Tenant.Id))
                {
                    r.Add(u.Tenant);
                }
            }
            return r;

        }
        public async Task<List<Socio>> FindSociosByUserIdAsync(string id)
        {
            var s = await (from SocioUsuario in _context.SocioUsuario
                           join Socio in _context.Socio on SocioUsuario.SocioId equals Socio.Id
                           select new { Socio, SocioUsuario }
              ).Where(x => x.SocioUsuario.IdentityUserId == id).ToListAsync();

            List<Socio> r = new();
            foreach (var i in s)
            {
                r.Add(i.Socio);
            }
            var lsu = await (from EmpSocioUser in _context.EmpSocioUser
                             join EmpreendimentoSocio in _context.EmpreendimentoSocio on EmpSocioUser.EmpreendimentoSocioId equals EmpreendimentoSocio.Id
                             join Socio in _context.Socio on EmpreendimentoSocio.SocioId equals Socio.Id
                             select new { EmpreendimentoSocio, Socio, EmpSocioUser.UserId }
              ).Where(x => x.UserId == id).ToListAsync();

            foreach (var i in lsu)
            {
                if (!r.Any(x => x.Id == i.Socio.Id))
                {
                    r.Add(i.Socio);
                }
            }
            return r;
        }
        public async Task<List<ProfessorUsuario>> FindProfessorByUserIdAsync(string id)
        {
            return await _context.ProfessorUsuario.Where(x => x.IdentityUserId == id).ToListAsync();
        }
        public async Task AlterarUsuarioAsync(Usuario usuario)
        {
            IdentityUser user = await _userManager.FindByIdAsync(usuario.Id);
            user.UserName = usuario.Nome;
            user.Email = usuario.Email;

            if (!usuario.Ativo)
            {
                await _userManager.SetLockoutEndDateAsync(user, new DateTime(2050, 12, 31));
            }
            else
            {
                if (user.LockoutEnd != null && user.LockoutEnd > Constante.Today.AddYears(10))
                {
                    await _userManager.SetLockoutEndDateAsync(user, new DateTime(2020, 01, 01));

                }

            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                foreach (Perfil p in usuario.Perfis)
                {
                    await _userManager.RemoveFromRoleAsync(user, p.Name);
                    if (p.Selecionado)
                        await _userManager.AddToRoleAsync(user, p.Name);
                }



                foreach (SFClaim sf in usuario.SFClaims)
                {
                    await _userManager.RemoveClaimAsync(user, new Claim(sf.Type, sf.Value));

                    if (sf.Selecionado)
                    {
                        await _userManager.AddClaimAsync(user, new Claim(sf.Type, sf.Value));

                    }
                }
            }
            else
            {
                if (result.Errors.Any())
                {
                    if (result.Errors.First().Code == "DuplicateEmail")
                    {
                        throw new NotFoundException("Email já cadastrado.");
                    }
                    else
                    {
                        throw new NotFoundException(result.Errors.First().Code + " => " + result.Errors.First().Description);
                    }
                }

            }

        }
        public async Task<LoginUserView> Login(LoginUserView model)
        {
            if (model.Email == null || model.Password == null)
            {
                model.Message = "Usuario/email inválido.";
                model.TemErro = true;
                return model;
            }
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                model.Message = "Usuario/email inválido.";
                model.TemErro = true;
                return model;
            }

            var cp = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!cp)
            {
                var lt = await _context.SFToken.Where(x => x.Token.Contains(user.Id) && x.DataIns >= Constante.Today.AddDays(-3)).OrderByDescending(x => x.DataIns).ToListAsync();
                if (lt.Count > 0)
                {
                    if (lt[0].GetOrigemToken() == OrigemToken.Seguranca && (lt[0].GetOperacaoToken() == OperacaoToken.ConfirmarEmail || lt[0].GetOperacaoToken() == OperacaoToken.TrocarSenha))
                    {
                        model.Code = lt[0].Code;
                        model.TemCode = true;
                        model.Token = lt[0].GUID;
                        model.Message = "";
                        model.TemErro = true;
                        model.User = user;
                        lt = lt.Where(x => x.GUID != lt[0].GUID).ToList();
                        if (lt.Count > 0)
                        {
                            _context.SFToken.RemoveRange(lt);
                            await _context.SaveChangesAsync();
                        }
                        return model;
                    }
                }
                model.Message = "Usuario/email inválido.";
                model.TemErro = true;
                return model;
            }

            model.User = user;
            return model;
        }
        public async Task<Resultado> DeleteAsync(Usuario u)
        {
            Resultado r = new();

            SessionInfo si = await GetSessionAsync();

            var user = await _userManager.FindByEmailAsync(u.Email);

            if (user == null)
            {
                r.ErrMsg = Constante.ItemaNaoCadastrado("Usuario");
                r.Ok = false;
                return r;
            }

            var lr = await _userManager.GetRolesAsync(user);

            bool mytrans = (_context.Database.CurrentTransaction == null);
            if (mytrans)
            {
                await _context.Database.BeginTransactionAsync();
            }
            try
            {
                TenantUsuario tu = await _context.TenantUsuario.FirstOrDefaultAsync(x => x.UserId == user.Id && x.TenantId == si.TenantId);
                if (tu != null)
                {
                    _context.TenantUsuario.Remove(tu);
                    await _context.SaveChangesAsync();
                }

                if ((!await _context.TenantUsuario.AnyAsync(x => x.UserId == user.Id && x.TenantId != si.TenantId))
                    && (!await _context.SocioUsuario.AnyAsync(x => x.IdentityUserId == user.Id))
                    && (!await _context.EmpSocioUser.AnyAsync(x => x.UserId == user.Id)))
                {
                    await _userManager.RemoveFromRolesAsync(user, lr);
                    var dr = await _userManager.DeleteAsync(user);
                    if (!dr.Succeeded)
                    {
                        if (dr.Errors.Any())
                        {
                            r.ErrMsg = dr.Errors.FirstOrDefault().Description;
                            r.Ok = false;
                            return r;
                        }
                    }
                }
                if (mytrans)
                {
                    await _context.Database.CommitTransactionAsync();
                }
            }
            catch (Exception e)
            {
                if (mytrans)
                {
                    await _context.Database.RollbackTransactionAsync();
                    r.ErrMsg = e.Message;
                    r.Ok = false;
                    return r;
                }
                else
                {
                    throw;
                }
            }
            r.Ok = true;
            return r;
        }
        public async Task ChangePasswordAsync(string email, string psw, string npsw)
        {

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new NotFoundException("Usuario não cadastrado");
            }

            var result = await _userManager.ChangePasswordAsync(user, psw, npsw);

            if (!result.Succeeded)
            {
                string s = string.Empty;
                foreach (var error in result.Errors)
                {
                    s += error.Code;
                }
                throw new Exception(s);
            }

        }
        public async Task<string> GeneratePswResetTokenAsync(string email)
        {
            var u = await _userManager.FindByEmailAsync(email);
            if (u == null)
            {
                return string.Empty;
            }
            return await _userManager.GeneratePasswordResetTokenAsync(u);
        }
        public async Task<IdentityUser> FindByEmail(string e)
        {
            return await _userManager.FindByEmailAsync(e);
        }
        public async Task<IdentityUser> FindByTokenEmail(string e)
        {
            return await _userManager.FindByEmailAsync(e);
        }
        public async Task<Resultado> ResetPsw(IdentityUser u, string t, string psw)
        {
            Resultado r = new();
            var result = await _userManager.ResetPasswordAsync(u, t, psw);
            if (result.Succeeded)
            {
                if (await _userManager.IsLockedOutAsync(u))
                {
                    await _userManager.SetLockoutEndDateAsync(u, DateTimeOffset.UtcNow);
                }
                if (!u.EmailConfirmed)
                {
                    string te = await _userManager.GenerateEmailConfirmationTokenAsync(u);
                    result = await _userManager.ConfirmEmailAsync(u, te);
                    if (!result.Succeeded)
                    {
                        r.Ok = false;
                        foreach (var error in result.Errors)
                        {
                            r.ErrMsg += error.Code;
                        }
                        return r;
                    }
                }
                r.Ok = true;
            }
            else
            {
                r.Ok = false;
                foreach (var error in result.Errors)
                {
                    r.ErrMsg += error.Code;
                }

            }
            return r;
        }
        public async Task<Resultado> EmailConfirmation(string g)
        {
            Resultado r = new();
            SFToken sft = await FindTokenByGuid(g);
            if (sft == null)
            {
                r.Ok = false;
                r.ErrMsg = "Token inválido";
                return r;
            }
            if (!sft.IsValid())
            {
                r.Ok = false;
                r.ErrMsg = "Token inválido";
                return r;
            }

            var u = await _userManager.FindByIdAsync(sft.GetUserIdToken());
            if (u == null)
            {
                r.Ok = false;
                r.ErrMsg = "Usuário inválido";
                return r;
            }
            var result = await _userManager.ConfirmEmailAsync(u, sft.GetSecToken());
            if (result.Succeeded)
            {
                if (await _userManager.IsLockedOutAsync(u))
                {
                    await _userManager.SetLockoutEndDateAsync(u, DateTimeOffset.UtcNow);
                }
                r.Ok = true;
            }
            else
            {
                r.Ok = false;
                foreach (var error in result.Errors)
                {
                    r.ErrMsg += " - " + error.Description;
                }

            }
            return r;
        }
        public async Task<SFToken> FindTokenByGuid(string g)
        {
            return await new SegurancaService(_context).FindTokenByGuid(g);
        }
        public async Task<string> GerarToken(string token)
        {
            string it = string.Empty;
            try
            {
                SessionInfo si = await GetSessionAsync();
                it = si.TenantId;
            }
            catch
            {
            }
            return await new SegurancaService(_context).GerarToken(token, it);
        }
        public async Task RemoveTokenByGuid(string id)
        {
            await new SegurancaService(_context).RemoveTokenByGuid(id);
        }
        public async Task AddUserLog(string userid, UserLogAction ua)
        {
            UserLog ul = new()
            {
                UserId = userid,
                UserLogAction = ua,
                LoginDt = Constante.Now
            };
            await _context.UserLog.AddAsync(ul);
            await _context.SaveChangesAsync();
        }
        public async Task<List<UserLog>> GetUserLogs()
        {
            List<UserLog> lu = new();
            var ld = await (from UserLog in _context.UserLog
                            join Users in _context.Users on UserLog.UserId equals Users.Id
                            select new
                            {
                                UserLog,
                                Users.UserName,
                                Users.Email
                            }).Where(x => x.UserLog.LoginDt >= Constante.Today.AddDays(-10)).OrderBy(x => x.UserLog.LoginDt).ToListAsync();

            foreach (var u in ld)
            {
                UserLog nu = new()
                {
                    Id = u.UserLog.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    LoginDt = u.UserLog.LoginDt
                };
                lu.Add(nu);
            }
            return lu;

        }

    }
}