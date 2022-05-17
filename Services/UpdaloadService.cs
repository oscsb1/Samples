using AutoMapper;
using Blazored.SessionStorage;
using InCorpApp.Data;
using InCorpApp.Models;
using InCorpApp.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using InCorpApp.Constantes;
using System.Collections.Generic;
using System.Linq;

namespace InCorpApp.Services
{
    public class UpdaloadService : ServiceBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ConfigService _configService;

        public UpdaloadService(ApplicationDbContext context,
                ISessionStorageService sessionStorageService,
                SessionService sessionService, IMapper mapper, IWebHostEnvironment webHostEnvironment,
                ConfigService configService
            ) : base(sessionStorageService, sessionService, mapper)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _configService = configService;
        }
        public async Task<int> SaveToDB(OrigemAnexo origem, int key, string filenome, MemoryStream dados, TipoSaveMode savemode = TipoSaveMode.OnlyDB, string titulo = null)
        {
            SessionInfo si = await GetSessionAsync();
            if (titulo == null)
            {
                titulo = string.Empty;
            }
            Arquivo a = new()
            {
                DataInclusao = Constante.Now,
                FileNome = filenome,
                TenantId = si.TenantId,
                Titulo = titulo,
                OrigemAnexo = origem,
                IdOrigem = key,
                SaveMode = savemode
            };

            if (savemode == TipoSaveMode.OnlyDB)
            {
                a.File = new byte[dados.Length];
                await dados.ReadAsync(a.File);
            }
            await _context.Arquivo.AddAsync(a);
            await _context.SaveChangesAsync();

            if (savemode == TipoSaveMode.OnlyFolder)
            {
                dados.Position = 0;
           //     string fn = Guid.NewGuid().ToString();
                string r = Path.Combine(_configService.GetFileDBPath(), si.TenantId, origem.ToString(), a.Id.ToString() + Path.GetExtension(filenome));
                string d = Path.Combine(_configService.GetFileDBPath(), si.TenantId, origem.ToString());
                Resultado r1 = new();

                try
                {
                    Directory.CreateDirectory(d);
                    using FileStream fw = new(r, FileMode.Create, FileAccess.Write);
                    dados.WriteTo(fw);
                }
                catch (Exception e)
                {
                    r1.ErrMsg = e.Message;
                }
            }

            return a.Id;
        }

        public async Task<MemoryStream> LoadFromDB(int id)
        {
            SessionInfo si = await GetSessionAsync();
            MemoryStream r = new();
            Arquivo a = await _context.Arquivo.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == id);
            if (a != null)
            {
                await r.WriteAsync(a.File.AsMemory(0, a.File.Length));
            }
            r.Position = 0;
            return r;
        }

        public async Task<string> SaveToFolder(int id)
        {
            SessionInfo si = await GetSessionAsync();
            Arquivo a = await _context.Arquivo.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == id);

            if (a.SaveMode == TipoSaveMode.OnlyFolder)
            {
                return Path.Combine(_webHostEnvironment.ContentRootPath, si.TenantId, a.OrigemAnexo.ToString(), a.Id.ToString() + Path.GetExtension(a.FileNome));
            }

            MemoryStream m = new();
            if (a != null)
            {
                await m.WriteAsync(a.File.AsMemory(0, a.File.Length));
                m.Position = 0;
            }
            else
            {
                return string.Empty;
            }
            m.Position = 0;
            string fn = Guid.NewGuid().ToString();
            string r = Path.Combine(_webHostEnvironment.WebRootPath, "tempfile", fn + "_" + a.FileNome);
            using FileStream fw = new(r, FileMode.Create, FileAccess.Write);
            m.WriteTo(fw);
            return Path.Combine("tempfile", fn + "_" + a.FileNome);
        }
        public async Task Delete(int id)
        {
            SessionInfo si = await GetSessionAsync();
            Arquivo a = await _context.Arquivo.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == id);
            if (a != null)
            {
                _context.Arquivo.Remove(a);
                await _context.SaveChangesAsync();
            }
        }

        public void ExcluirFileFromFolder(string filenome)
        {
            try
            {
                if (File.Exists(filenome))
                {
                    File.Delete(filenome);
                    return;
                }
                string p = Path.Combine(_webHostEnvironment.WebRootPath, filenome);
                if (File.Exists(p))
                {
                    File.Delete(p);
                }
            }
            catch
            {
            }
        }

        public bool FileExists(string filenome)
        {
            try
            {
                return File.Exists(filenome);
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Arquivo>> GetFiles(int objid, OrigemAnexo origemAnexo)
        {
            SessionInfo si = await GetSessionAsync();
            List<Arquivo> r = new();
            return await _context.Arquivo.Where(x => x.IdOrigem == objid && x.OrigemAnexo == origemAnexo && x.TenantId == si.TenantId).ToListAsync();
        }

        public async Task<Resultado> RemoveFileAsync(Arquivo a)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            if (si.TenantId != a.TenantId)
            {
                r.Ok = false;
                r.ErrMsg = "TenantId sessão diferente do Arquivo";
                return r;
            }
            try
            {
                if (a.SaveMode == TipoSaveMode.OnlyFolder)
                {
                    string f = Path.Combine(_configService.GetFileDBPath(), si.TenantId, a.OrigemAnexo.ToString(), a.Id.ToString() + Path.GetExtension(a.FileNome));
                    if (File.Exists(f))
                    {
                        File.Delete(f);
                    }
                }
                _context.Arquivo.Remove(a);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }

        public async Task<string> CopyToTempFolder(Arquivo a)
        {
            SessionInfo si = await GetSessionAsync();
            string fn = Guid.NewGuid().ToString();
            File.Copy(Path.Combine(_configService.GetFileDBPath(), si.TenantId, a.OrigemAnexo.ToString(), a.Id.ToString() + Path.GetExtension(a.FileNome)),
                Path.Combine(_webHostEnvironment.WebRootPath, "tempfile", fn + "_" + a.FileNome), true);
            return Path.Combine("tempfile", fn + "_" + a.FileNome);
        }
    }
}
