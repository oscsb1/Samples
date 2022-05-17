using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Blazored.SessionStorage;
using InCorpApp.Constantes;
using InCorpApp.Data;
using InCorpApp.Models;
using InCorpApp.Security;
using Microsoft.EntityFrameworkCore;



namespace InCorpApp.Services
{
    public class MSGService : ServiceBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ConfigService _configService;
        private string _token = string.Empty;
        private string _tenantid = string.Empty;
        public MSGService(ApplicationDbContext context,
            ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper, ConfigService configService) : base(sessionStorageService, sessionService, mapper)
        {
            _context = context;
            _configService = configService;
        }
        public async Task<SFMsgBase> ValidarToken(string token)
        {
            SegurancaService segs = new(_context);
            SFMsgBase msg = new();
            var xt = await segs.FindTokenByGuid(token);
            if (xt == null)
            {
                msg.erro = true;
                msg.erromsg = "Token invalído";
                return msg;
            }
            try
            {
                msg = JsonSerializer.Deserialize<SFMsgBase>(xt.Token);
                if (msg == null)
                {
                    msg = new()
                    {
                        erro = true,
                        erromsg = "Token invalído. Deserialize"
                    };
                }
                msg.erro = false;
            }
            catch (Exception e)
            {
                msg = new()
                {
                    erro = true,
                    erromsg = e.Message
                };
            }
            return msg;
        }
        public async Task<SFMsgBase> GerarToken(int empId, int cc, DateTime dtini, DateTime dtfim)
        {
            SessionInfo si = await GetSessionAsync();
            InCorpConfig config = await _context.InCorpConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
            if (config == null)
            {
                config = new()
                {
                    SFMSGTipoDB = SFMSGTipoDB.SQL,
                    DBStringCon = "Server=DESKTOP-3JSS4PD\\SQLEXPRESS;Database=DES;User Id=sa;Password=sa820042",
                    TenantId = si.TenantId,
                    SFMSGTipoFormato = SFMSGTipoFormato.Default,
                    UrlLocal = "http://localhost:8001/"
                };
                await _context.InCorpConfig.AddAsync(config);
                await _context.SaveChangesAsync();
            }
            if (config.UrlLocal == string.Empty || config.UrlLocal == null)
            {
                config.UrlLocal = "http://localhost:8001/";
                _context.InCorpConfig.Update(config);
                await _context.SaveChangesAsync();
            }
            var emp = _context.Empreendimento.FirstOrDefault(x => x.Id == empId && x.TenantId == si.TenantId);
            if (emp == null)
            {
                throw new Exception("Empreendimento inválido.");
            }
            string urlr = string.Empty;
            string g = Guid.NewGuid().ToString();
            if (cc != 0)
            {
                var conta = _context.ContaCorrente.FirstOrDefault(x => x.Id == cc && x.TenantId == si.TenantId);
                if (conta == null)
                {
                    throw new Exception("Conta conrrente inválido");
                }
                urlr = _configService.GetBaseUrl() + "Banking/ContasCorrentes/" + empId.ToString() + ";" + cc.ToString() + ";" + g ;
            }
            SFMsgBase msg = new()
            {
                empreendimento = emp.CodigoExterno,
                datainicio = dtini,
                datafinal = dtfim,
                dbstringcon = config.DBStringCon,
                dbType = config.SFMSGTipoDB,
                format = config.SFMSGTipoFormato,
                urlretorno = urlr,
                token = g,
                dataultalt = Constante.Now,
                status = SFMSGStatus.Criado,
                erro = false,
                erromsg = string.Empty,
                urllocal = config.UrlLocal
            };
            SegurancaService segs = new(_context);
            string smsg = JsonSerializer.Serialize(msg);
            msg.token = await segs.GerarToken(smsg, si.TenantId, msg.token);
            return msg;
        }
        public async Task<SFMsgBase>ImportLancto(SFMsgBase msgLancto)
        {
            try
            {
                if (_token == string.Empty || _token != msgLancto.token)
                {
                    SegurancaService ss = new(_context);
                    SFToken t = await ss.FindTokenByGuid(msgLancto.token);
                    if (t == null)
                    {
                        msgLancto.erro = true;
                        msgLancto.erromsg = "Token inválido - import lancto";
                        return msgLancto;
                    }
                    _tenantid = t.TenantId;
                    _token = msgLancto.token;
                }
                LanctoImportDireto l = new()
                {
                    Dados = msgLancto.lancto,
                    TenantId = _tenantid,
                    Token = msgLancto.token
                };
                await _context.LanctoImportDireto.AddAsync(l);
                await _context.SaveChangesAsync();
                msgLancto.erro = false;
            }
            catch (Exception e)
            {
                msgLancto.erro = true;
                msgLancto.erromsg = e.Message;
            }
            return msgLancto;
        }
        public async Task<SFMsgBase> UpdateToken(SFMsgBase msgToken)
        {
            SFMsgBase r = new();
            try
            {
                SegurancaService segs = new(_context);
                string smsg = JsonSerializer.Serialize(msgToken);
                await segs.UpdateTokenByGuid(msgToken.token, smsg);
            }
            catch (Exception e)
            {
                r.erro = true;
                r.erromsg = e.Message;
            }
            return r;
        }
        public async Task<SFMsgBase> GetTokenByGuid(string guid)
        {
            SegurancaService segs = new(_context);
            SFMsgBase r = new();
            var xt = await segs.FindTokenByGuid(guid);
            if (xt == null)
            {
                r.erro = true;
                r.erromsg = "Token invalído";
                return r;
            }
            try
            {
                r = JsonSerializer.Deserialize<SFMsgBase>(xt.Token);
                if (r == null)
                {
                    r = new()
                    {
                        erro = true,
                        erromsg = "Token invalído. Deserialize"
                    };
                }
                r.erro = false;
            }
            catch (Exception e)
            {
                r = new()
                {
                    erro = true,
                    erromsg = e.Message
                };
            }
            return r;
        }
    }
}

