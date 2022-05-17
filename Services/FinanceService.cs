using Microsoft.EntityFrameworkCore;
using InCorpApp.Data;
using InCorpApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Services.Exceptions;
using Microsoft.Data.SqlClient;
using Blazored.SessionStorage;
using InCorpApp.Security;
using InCorpApp.Constantes;
using AutoMapper;
using System;
using InCorpApp.Utils;
using BancoCentralIndice;
using System.Data.Common;

namespace InCorpApp.Services
{
    public class FinanceService : ServiceBase
    {
        private readonly ApplicationDbContext _context;
        public FinanceService(ApplicationDbContext context,
 ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper) : base(sessionStorageService, sessionService, mapper)
        {
            _context = context;
        }

        public async Task<List<MovtoBancoView>> FindAllMovtoBancoByLoteGUID(string loteguid)
        {
            SessionInfo si = await GetSessionAsync();
            List<MovtoBancoView> r = new();

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
"select " +
"                    MovtoBanco.Id , " +
"                    MovtoBanco.ContaCorrenteId , " +
"                    MovtoBanco.DataMovto , " +
"                    MovtoBanco.Documento , " +
"                    MovtoBanco.Historico , " +
" case when  Isnull(LanctoEmpreendimento.Id,0) = 0 then 'não' else 'sim' end ExportadoV, " +
" convert(bit, case  when  Isnull(LanctoEmpreendimento.Id,0) = 0 then 0 else 1 end ) Exportado, " +
"                    MovtoBanco.PlanoContaId, " +
" Isnull(PlanoConta.Nome, '') PlanoContaV , " +
"  Isnull(Relacionamento.Nome, '')  RelacionamentoV,  " +
"  MovtoBanco.RelacionamentoId ,  " +
"                    MovtoBanco.TipoMovtoBanco , " +
"                    MovtoBanco.Transferencia , " +
"                    MovtoBanco.Valor, " +
"                    MovtoBanco.StatusConciliacao, " +
" MovtoBanco.CodigoExterno " +
"from  MovtoBanco " +
"inner join LoteMovtoBanco on LoteMovtoBanco.Id = MovtoBanco.LoteMovtoBancoId " +
"left join PlanoConta on PlanoConta.Id = MovtoBanco.PlanoContaId " +
"left join Relacionamento on Relacionamento.Id = MovtoBanco.RelacionamentoId " +
"left join LanctoEmpreendimento on LanctoEmpreendimento.OrigemId = MovtoBanco.Id and LanctoEmpreendimento.Origem =  4 " +
"where MovtoBanco.TenantId = @tid and " +
" LoteMovtoBanco.GUID = @GUID  " +
" order by MovtoBanco.DataMovto, MovtoBanco.Seq ";

            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@GUID", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@GUID"].Value = loteguid;

            await command.Connection.OpenAsync();
            var lu = await command.ExecuteReaderAsync();
            try
            {
                while (await lu.ReadAsync())
                {
                    MovtoBancoView m = new()
                    {
                        Id = (int)lu["Id"],
                        ContaCorrenteId = (int)lu["ContaCorrenteId"],
                        DataMovto = (DateTime)lu["DataMovto"],
                        Documento = lu["Documento"].ToString(),
                        Historico = lu["Historico"].ToString(),
                        ExportadoV = lu["ExportadoV"].ToString(),
                        PlanoContaId = (int)lu["PlanoContaId"],
                        PlanoContaV = lu["PlanoContaV"].ToString(),
                        RelacionamentoId = (int)lu["RelacionamentoId"],
                        RelacionamentoV = lu["RelacionamentoV"].ToString(),
                        TipoMovtoBanco = (TipoMovtoBanco)lu["TipoMovtoBanco"],
                        Transferencia = (bool)lu["Transferencia"],
                        Valor = (double)lu["Valor"],
                        Exportado = (bool)lu["Exportado"],
                        CodigoExterno = lu["CodigoExterno"].ToString(),
                        StatusConciliacao = (StatusConciliacao)lu["StatusConciliacao"]
                    };
                    if (m.CodigoExterno == null)
                        m.CodigoExterno = string.Empty;
                    if (m.Documento == null)
                        m.Documento = string.Empty;
                    if (m.Historico == null)
                        m.Historico = string.Empty;
                    if (m.PlanoContaV == null)
                        m.PlanoContaV = string.Empty;
                    if (m.RelacionamentoV == null)
                        m.RelacionamentoV = string.Empty;
                    r.Add(m);
                }
            }
            finally
            {
                await command.Connection.CloseAsync();

            }
            return r;
        }
        public async Task<List<MovtoBancoView>> FindAllMovtoBancoByContaId(int id, DateTime dti, DateTime dtf)
        {
            SessionInfo si = await GetSessionAsync();
            List<MovtoBancoView> r = new();

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
"select " +
"                    MovtoBanco.Id , " +
"                    MovtoBanco.ContaCorrenteId , " +
"                    MovtoBanco.DataMovto , " +
"                    MovtoBanco.Documento , " +
"                    MovtoBanco.Historico , " +
" case when  Isnull(LanctoEmpreendimento.Id,0) = 0 then 'não' else 'sim' end ExportadoV, " +
" convert(bit, case  when  Isnull(LanctoEmpreendimento.Id,0) = 0 then 0 else 1 end ) Exportado, " +
"                    MovtoBanco.PlanoContaId, " +
" Isnull(PlanoConta.Nome, '') PlanoContaV , " +
"  Isnull(Relacionamento.Nome, '')  RelacionamentoV,  " +
"  MovtoBanco.RelacionamentoId ,  " +
"                    MovtoBanco.TipoMovtoBanco , " +
"                    MovtoBanco.Transferencia , " +
"                    MovtoBanco.Valor, " +
"                    MovtoBanco.StatusConciliacao, " +
" MovtoBanco.CodigoExterno " +
"from  MovtoBanco " +
"left join PlanoConta on PlanoConta.Id = MovtoBanco.PlanoContaId " +
"left join Relacionamento on Relacionamento.Id = MovtoBanco.RelacionamentoId " +
"left join LanctoEmpreendimento on LanctoEmpreendimento.OrigemId = MovtoBanco.Id and LanctoEmpreendimento.Origem =  4 " +
"where MovtoBanco.TenantId = @tid and " +
" MovtoBanco.ContaCorrenteId = @id and " +
" MovtoBanco.DataMovto >= @dti and MovtoBanco.DataMovto <= @dtf " +
" order by MovtoBanco.DataMovto, MovtoBanco.Seq ";

            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@dti", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dti"].Value = dti.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtf", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtf"].Value = dtf.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));
            command.Parameters["@id"].Value = id;

            await command.Connection.OpenAsync();
            var lu = await command.ExecuteReaderAsync();
            try
            {
                while (await lu.ReadAsync())
                {
                    MovtoBancoView m = new()
                    {
                        Id = (int)lu["Id"],
                        ContaCorrenteId = (int)lu["ContaCorrenteId"],
                        DataMovto = (DateTime)lu["DataMovto"],
                        Documento = lu["Documento"].ToString(),
                        Historico = lu["Historico"].ToString(),
                        ExportadoV = lu["ExportadoV"].ToString(),
                        PlanoContaId = (int)lu["PlanoContaId"],
                        PlanoContaV = lu["PlanoContaV"].ToString(),
                        RelacionamentoId = (int)lu["RelacionamentoId"],
                        RelacionamentoV = lu["RelacionamentoV"].ToString(),
                        TipoMovtoBanco = (TipoMovtoBanco)lu["TipoMovtoBanco"],
                        Transferencia = (bool)lu["Transferencia"],
                        Valor = (double)lu["Valor"],
                        Exportado = (bool)lu["Exportado"],
                        CodigoExterno = lu["CodigoExterno"].ToString(),
                        StatusConciliacao = (StatusConciliacao)lu["StatusConciliacao"]
                    };
                    if (m.CodigoExterno == null)
                        m.CodigoExterno = string.Empty;
                    if (m.Documento == null)
                        m.Documento = string.Empty;
                    if (m.Historico == null)
                        m.Historico = string.Empty;
                    if (m.PlanoContaV == null)
                        m.PlanoContaV = string.Empty;
                    if (m.RelacionamentoV == null)
                        m.RelacionamentoV = string.Empty;
                    r.Add(m);
                }
            }
            finally
            {
                await command.Connection.CloseAsync();

            }
            return r;
        }
        public async Task<ContaCorrente> FindCCByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            var cc = await _context.ContaCorrente.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
            if (si.TenantApp == TenantApp.InCorp)
            {
                var ce = await _context.ContaCorrenteEmpreendimento.FirstOrDefaultAsync(x => x.ContaCorrenteId == id && x.TenantId == si.TenantId);
                if (ce != null)
                    cc.EmpreendimentoId = ce.EmpreendimentoId;
            }
            else
            {
                if (si.TenantApp == TenantApp.Studio)
                {
                    var ce = await _context.ContaCorrenteStudio.FirstOrDefaultAsync(x => x.ContaCorrenteId == id && x.TenantId == si.TenantId);
                    if (ce != null)
                        cc.StudioId = ce.StudioId;
                }
            }
            return cc;
        }
        public async Task<List<ContaCorrente>> FindAllContaCorrenteByEmpIdAsync(int empid)
        {
            SessionInfo si = await GetSessionAsync();
            List<ContaCorrente> r = new();
            var command = _context.Database.GetDbConnection().CreateCommand();

            command.CommandText =

"Select ContaCorrente.*, " +
"Banco.Nome Banco, " +
"Case " +
"when Tipo = 1 then 'Caixa' " +
"when Tipo = 2 then 'Conta corrente' " +
"when Tipo = 3 then 'Cartão crédito' " +
"when Tipo = 4 then 'Investimento' " +
"when Tipo = 5 then 'Conta corrente sócio' " +
"when Tipo = 6 then 'Outra' end " +
"TipoCC " +
"from ContaCorrente " +
"inner join ContaCorrenteEmpreendimento on ContaCorrenteEmpreendimento.ContaCorrenteId = ContaCorrente.Id " +
"LEFT join Banco on Banco.Id = ContaCorrente.BancoId " +
 "where ContaCorrenteEmpreendimento.EmpreendimentoId = @id and ContaCorrente.TenantId = @tid";


            command.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));
            command.Parameters["@id"].Value = empid;

            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;


            await command.Connection.OpenAsync();
            var lu = await command.ExecuteReaderAsync();


            try
            {
                while (await lu.ReadAsync())
                {
                    var s = new ContaCorrente()
                    {
                        Id = (int)lu["id"],
                        EmpreendimentoId = empid,
                        BancoId = (int)lu["BancoId"],
                        Nome = lu["Nome"].ToString(),
                        Numero = lu["Numero"].ToString(),
                        Agencia = lu["Agencia"].ToString(),
                        TenantId = lu["TenantId"].ToString(),
                        CodigoExterno = lu["CodigoExterno"].ToString(),
                        Tipo = (TipoCCorrente)lu["Tipo"]
                    };
                    r.Add(s);
                };
            }
            finally
            {
                lu.Close();
                command.Connection.Close();
            }
            return r;

        }
        public async Task<List<ContaCorrente>> FindAllContaCorrenteByStudioIdAsync(int studioid, bool comsaldo = false)
        {
            SessionInfo si = await GetSessionAsync();
            List<ContaCorrente> r = new();
            var command = _context.Database.GetDbConnection().CreateCommand();

            string sqlcomp = string.Empty;
            string sqlcomp1 = string.Empty;
            string sqlcomp2 = string.Empty;
            if (comsaldo)
            {
                sqlcomp = " , sum( case when Isnull(MovtoBanco.TipoMovtoBanco,0) = 0 then 0.0  when Isnull(MovtoBanco.TipoMovtoBanco,0) = 1 then MovtoBanco.Valor else 0.0 end) Credito  " +
                          " , sum(case when Isnull(MovtoBanco.TipoMovtoBanco,0) = 0 then 0.0  when Isnull(MovtoBanco.TipoMovtoBanco,0) = 2 then MovtoBanco.Valor else 0.0 end) Debito  " +
                          " , sum( case when Isnull(MovtoBanco.TipoMovtoBanco,0) = 0 then 0.0  when Isnull(MovtoBanco.TipoMovtoBanco,0) = 1 then MovtoBanco.Valor else 0.0 end)   " +
                          " - sum(case when Isnull(MovtoBanco.TipoMovtoBanco,0) = 0 then 0.0  when Isnull(MovtoBanco.TipoMovtoBanco,0) = 2 then MovtoBanco.Valor else 0.0 end) Saldo  ";
                sqlcomp1 = " left join MovtoBanco on MovtoBanco.ContacorrenteId = ContaCorrente.Id ";
                sqlcomp2 = " group by Studio.Nome, ContaCorrente.Id,ContaCorrente.TenantId,ContaCorrente.Nome,ContaCorrente.Tipo,ContaCorrente.BancoId,ContaCorrente.Agencia,ContaCorrente.DigitoAgencia,ContaCorrente.Numero,ContaCorrente.DigitoNumero,ContaCorrente.CodigoExterno, Banco.Nome  ";
            }

            string s1 = " where ContaCorrenteStudio.StudioId = @id and ContaCorrente.TenantId = @tid ";
            if (studioid == 0)
            {
                s1 = " where ContaCorrenteStudio.StudioId > @id and ContaCorrente.TenantId = @tid ";
            }

            command.CommandText =
"Select ContaCorrente.*, " +
"Banco.Nome Banco " +
sqlcomp +
",Studio.Nome StudioNome, " +
"Case " +
"when Tipo = 1 then 'Caixa' " +
"when Tipo = 2 then 'Conta corrente' " +
"when Tipo = 3 then 'Cartão crédito' " +
"when Tipo = 4 then 'Investimento' " +
"when Tipo = 5 then 'Conta corrente sócio' " +
"when Tipo = 6 then 'Outra' end " +
"TipoCC " +
"from ContaCorrente " +
"inner join ContaCorrenteStudio on ContaCorrenteStudio.ContaCorrenteId = ContaCorrente.Id " +
"inner join Studio on ContaCorrenteStudio.StudioId = Studio.Id " +
"LEFT join Banco on Banco.Id = ContaCorrente.BancoId " +
sqlcomp1 +
s1 +
sqlcomp2;

            command.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));
            command.Parameters["@id"].Value = studioid;

            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            await command.Connection.OpenAsync();
            var lu = await command.ExecuteReaderAsync();

            try
            {
                while (await lu.ReadAsync())
                {
                    var s = new ContaCorrente()
                    {
                        Id = (int)lu["id"],
                        StudioId = studioid,
                        BancoId = (int)lu["BancoId"],
                        Nome = lu["Nome"].ToString(),
                        Numero = lu["Numero"].ToString(),
                        Agencia = lu["Agencia"].ToString(),
                        TenantId = lu["TenantId"].ToString(),
                        CodigoExterno = lu["CodigoExterno"].ToString(),
                        StudioNome = lu["StudioNome"].ToString(),
                        Tipo = (TipoCCorrente)lu["Tipo"]
                    };
                    if (comsaldo)
                    {
                        s.Debitos = (double)lu["Debito"];
                        s.Creditos = (double)lu["Credito"];
                        s.SaldoAtual = (double)lu["Saldo"];
                    }
                    r.Add(s);
                };
            }
            finally
            {
                lu.Close();
                command.Connection.Close();
            }
            return r;

        }
        public async Task<List<ContaCorrente>> FindAllCCViewByEmpIdAsync(int empid, bool comsaldo = false)
        {
            SessionInfo si = await GetSessionAsync();
            List<ContaCorrente> r = new();
            var command = _context.Database.GetDbConnection().CreateCommand();

            string sqlcomp = string.Empty;
            string sqlcomp1 = string.Empty;
            string sqlcomp2 = string.Empty;
            if (comsaldo)
            {
                sqlcomp = " , sum( case when Isnull(MovtoBanco.TipoMovtoBanco,0) = 0 then 0.0  when Isnull(MovtoBanco.TipoMovtoBanco,0) = 1 then MovtoBanco.Valor else 0.0 end) Credito  " +
                          " , sum(case when Isnull(MovtoBanco.TipoMovtoBanco,0) = 0 then 0.0  when Isnull(MovtoBanco.TipoMovtoBanco,0) = 2 then MovtoBanco.Valor else 0.0 end) Debito  " +
                          " , sum( case when Isnull(MovtoBanco.TipoMovtoBanco,0) = 0 then 0.0  when Isnull(MovtoBanco.TipoMovtoBanco,0) = 1 then MovtoBanco.Valor else 0.0 end)   " +
                          " - sum(case when Isnull(MovtoBanco.TipoMovtoBanco,0) = 0 then 0.0  when Isnull(MovtoBanco.TipoMovtoBanco,0) = 2 then MovtoBanco.Valor else  0.0 end) Saldo  ";
                sqlcomp1 = " left join MovtoBanco on MovtoBanco.ContacorrenteId = ContaCorrente.Id ";
                sqlcomp2 = " group by ContaCorrente.Id,ContaCorrente.TenantId,ContaCorrente.Nome,ContaCorrente.Tipo,ContaCorrente.BancoId,ContaCorrente.Agencia,ContaCorrente.DigitoAgencia,ContaCorrente.Numero,ContaCorrente.DigitoNumero,ContaCorrente.CodigoExterno, Banco.Nome  ";
            }

            command.CommandText =

"Select ContaCorrente.*, " +
"Banco.Nome Banco " +
sqlcomp +
"from ContaCorrente " +
"inner join ContaCorrenteEmpreendimento on ContaCorrenteEmpreendimento.ContaCorrenteId = ContaCorrente.Id " +
"LEFT join Banco on Banco.Id = ContaCorrente.BancoId " +
sqlcomp1 +
 "where ContaCorrenteEmpreendimento.EmpreendimentoId = @id and ContaCorrente.TenantId = @tid" +
 sqlcomp2;


            command.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));
            command.Parameters["@id"].Value = empid;

            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            DbDataReader lu = null;
            await command.Connection.OpenAsync();
            try
            {
                lu = await command.ExecuteReaderAsync();
                while (await lu.ReadAsync())
                {
                    var s = new ContaCorrente()
                    {
                        Id = (int)lu["id"],
                        EmpreendimentoId = empid,
                        BancoId = (int)lu["BancoId"],
                        Nome = lu["Nome"].ToString(),
                        Numero = lu["Numero"].ToString(),
                        Agencia = lu["Agencia"].ToString(),
                        TenantId = lu["TenantId"].ToString(),
                        CodigoExterno = lu["CodigoExterno"].ToString(),
                        Banco = lu["Banco"].ToString(),
                        Tipo = (TipoCCorrente)lu["Tipo"]
                    };
                    if (comsaldo)
                    {
                        s.Debitos = (double)lu["Debito"];
                        s.Creditos = (double)lu["Credito"];
                        s.SaldoAtual = (double)lu["Saldo"];
                    }
                    r.Add(s);
                };
            }
            catch
            {
                throw;
            }
            finally
            {
                lu.Close();
                command.Connection.Close();
            }
            return r;

        }
        public async Task UpdateCCAsync(ContaCorrente cc)
        {
            SessionInfo si = await GetSessionAsync();
            cc.TenantId = si.TenantId;

            await _context.Database.BeginTransactionAsync();
            try
            {
                if (si.TenantApp == TenantApp.InCorp)
                {
                    ContaCorrenteEmpreendimento ce = await _context.ContaCorrenteEmpreendimento.FirstOrDefaultAsync(x => x.ContaCorrenteId == cc.Id);
                    if (ce == null)
                    {
                        ce = new ContaCorrenteEmpreendimento()
                        {
                            ContaCorrenteId = cc.Id,
                            EmpreendimentoId = cc.EmpreendimentoId,
                            TenantId = si.TenantId
                        };
                        await _context.ContaCorrenteEmpreendimento.AddAsync(ce);
                    }
                }
                else
                {
                    if (si.TenantApp == TenantApp.Studio)
                    {
                        ContaCorrenteStudio ce = await _context.ContaCorrenteStudio.FirstOrDefaultAsync(x => x.ContaCorrenteId == cc.Id && x.TenantId == si.TenantId);
                        if (ce == null)
                        {
                            ce = new ContaCorrenteStudio()
                            {
                                ContaCorrenteId = cc.Id,
                                StudioId = cc.StudioId,
                                TenantId = si.TenantId
                            };
                            await _context.ContaCorrenteStudio.AddAsync(ce);
                        }
                    }
                }
                _context.ContaCorrente.Update(cc);
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;
            }
        }
        public async Task AddCCAsync(ContaCorrente cc)
        {
            SessionInfo si = await GetSessionAsync();
            cc.TenantId = si.TenantId.ToString();
            int empid = cc.EmpreendimentoId;

            await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.ContaCorrente.AddAsync(cc);
                await _context.SaveChangesAsync();
                if (si.TenantApp == TenantApp.InCorp)
                {
                    ContaCorrenteEmpreendimento ce = new()
                    {
                        ContaCorrenteId = cc.Id,
                        EmpreendimentoId = empid,
                        TenantId = si.TenantId
                    };
                    await _context.ContaCorrenteEmpreendimento.AddAsync(ce);
                }
                else
                {
                    if (si.TenantApp == TenantApp.Studio)
                    {
                        ContaCorrenteStudio ce = new()
                        {
                            ContaCorrenteId = cc.Id,
                            StudioId = cc.StudioId,
                            TenantId = si.TenantId
                        };
                        await _context.ContaCorrenteStudio.AddAsync(ce);
                    }
                }
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;
            }
        }
        public async Task<List<Banco>> FindAllBancosAsync()
        {
            return await _context.Banco.OrderBy(x => x.Nome).ToListAsync();
        }
        public async Task DeleteCCAsync(ContaCorrente cc)
        {
            if (await _context.MovtoBanco.AnyAsync(x => x.ContaCorrenteId == cc.Id))
            {
                throw new NaoPodeExcluirException("Conta corrente com movimento não pode ser excluída");
            }
            await _context.Database.BeginTransactionAsync();
            try
            {
                ContaCorrenteEmpreendimento ce = await _context.ContaCorrenteEmpreendimento.FirstOrDefaultAsync(x => x.ContaCorrenteId == cc.Id);
                if (ce != null)
                {
                    _context.ContaCorrenteEmpreendimento.Remove(ce);
                }
                ContaCorrenteStudio cs = await _context.ContaCorrenteStudio.FirstOrDefaultAsync(x => x.ContaCorrenteId == cc.Id);
                if (cs != null)
                {
                    _context.ContaCorrenteStudio.Remove(cs);
                }
                _context.ContaCorrente.Remove(cc);
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;
            }
        }
        public async Task<Resultado> DeleteMovtoBanco(MovtoBancoView m)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            LanctoEmpreendimento le = null;
            Periodo pe = null;
            if (m.Exportado)
            {
                le = await _context.LanctoEmpreendimento.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.OrigemId == m.Id && x.Origem == OrigemLancto.ImportadoCC);
                if (le != null)
                {
                    pe = await _context.Periodo.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == le.PeriodoId);
                    if (pe != null)
                    {
                        if (pe.Status != StatusPeriodo.EntradaDeDados)
                        {
                            r.Ok = false;
                            r.ErrMsg = "Período referente a esse lançamento está na fase " + pe.NomeStatus + ", não permite alterações";
                            return r;
                        }
                    }
                }
            }

            var ce = await _context.ContaCorrenteEmpreendimento.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.ContaCorrenteId == m.ContaCorrenteId);
            if (ce != null)
            {
                pe = await _context.Periodo.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.EmpreendimentoId == ce.EmpreendimentoId &&
                                                                    x.DataInicio <= m.DataMovto && x.DataFim >= m.DataMovto);
                if (pe != null)
                {
                    if (pe.Status != StatusPeriodo.EntradaDeDados)
                    {
                        r.Ok = false;
                        r.ErrMsg = "Período referente a esse lançamento está na fase " + pe.NomeStatus + ", não permite alterações";
                        return r;
                    }
                }
            }

            MovtoBanco mb = await _context.MovtoBanco.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == m.Id);
            if (mb == null)
            {
                r.Ok = false;
                r.ErrMsg = "Lançamento não encontrado no banco de dados";
                return r;
            }
            await _context.Database.BeginTransactionAsync();
            try
            {
                if (m.Exportado && le != null)
                {
                    var ilr = await _context.LanctoEmpRelacionamento.FirstOrDefaultAsync(x => x.LanctoEmpreendimentoId == le.Id && x.TenantId == si.TenantId);
                    if (ilr != null)
                    {
                        _context.LanctoEmpRelacionamento.Remove(ilr);
                        await _context.SaveChangesAsync();
                    }
                    _context.LanctoEmpreendimento.Remove(le);
                    await _context.SaveChangesAsync();
                }
                _context.MovtoBanco.Remove(mb);
                await _context.SaveChangesAsync();
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
        public async Task<List<PlanoConta>> FindAllPlanoContaByEmpId(int id)
        {
            SessionInfo si = await GetSessionAsync();
            if (si.TenantApp == TenantApp.InCorp)
            {
                return await _context.PlanoConta.Where(x => x.EmpreendimentoId == id && x.TenantId == si.TenantId).OrderBy(x => x.Nome).ToListAsync();
            }
            else
            {
                return null;
            }
        }
        public async Task<Empreendimento> GetStudioEmpreendimento()
        {
            SessionInfo si = await GetSessionAsync();
            List<PlanoConta> r = new();
            if (si.TenantApp == TenantApp.Studio)
            {
                var emp = await _context.Empreendimento.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
                if (emp == null)
                {
                    throw new Exception("Empreendimento para studio não cadastrado!");
                }
                return emp;
            }
            else
            {
                return null;
            }
        }
        public async Task<List<PlanoConta>> FindAllPlanoConta()
        {
            SessionInfo si = await GetSessionAsync();
            List<PlanoConta> r = new();
            if (si.TenantApp == TenantApp.Studio)
            {
                var emp = await _context.Empreendimento.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
                if (emp == null)
                {
                    Empresa e1 = new()
                    {
                        TenantId = si.TenantId,
                        Nome = si.TenantName,
                        RazaoSocial = si.TenantName,
                        CNPJ = "999" + Constante.Now.ToString("ddMMHHmm")
                    };
                    await _context.Empresa.AddAsync(e1);
                    await _context.SaveChangesAsync();
                    emp = new()
                    {
                        CNPJ = e1.CNPJ,
                        Casa = false,
                        Apto = false,
                        CodigoExterno = string.Empty,
                        EmpresaId = e1.Id,
                        TenantId = si.TenantId,
                        Nome = e1.Nome,
                        RazaoSocial = e1.Nome
                    };
                    await _context.Empreendimento.AddAsync(emp);
                    await _context.SaveChangesAsync();
                }
                var lp = await (from PlanoConta in _context.PlanoConta
                                join PlanoGerencial in _context.PlanoGerencial on PlanoConta.PlanoGerencialId equals PlanoGerencial.Id
                                select new { PlanoConta, nomeg = PlanoGerencial.Nome }).AsNoTracking().Where(x => x.PlanoConta.EmpreendimentoId == emp.Id && x.PlanoConta.TenantId == si.TenantId).ToListAsync();
                foreach (var p in lp)
                {
                    p.PlanoConta.PlanoContaGerencialNome = p.nomeg;
                    r.Add(p.PlanoConta);
                }
                r = r.OrderBy(x => x.Nome).ToList();
                return r;
            }
            else
            {
                return null;
            }
        }
        public async Task<List<PlanoGerencial>> FindAllPlanoGerencialByTenant()
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.PlanoGerencial.Where(x => x.TenantId == si.TenantId).ToListAsync();
        }
        public async Task<Resultado> AddMovtoBanco(MovtoBancoView m)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            MovtoBanco mb = new()
            {
                TenantId = si.TenantId
            };
            mb = ParseMovtoViewToMovto(m, mb);
            try
            {
                await _context.MovtoBanco.AddAsync(mb);
                await _context.SaveChangesAsync();
                r.Ok = true;
                r.ErrMsg = string.Empty;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        protected MovtoBanco ParseMovtoViewToMovto(MovtoBancoView m, MovtoBanco mb)
        {
            if (m.CodigoExterno == null)
            {
                m.CodigoExterno = string.Empty;
            }
            mb.ContaCorrenteId = m.ContaCorrenteId;
            mb.Valor = m.Valor;
            mb.DataMovto = m.DataMovto;
            mb.Documento = m.Documento;
            mb.Historico = m.Historico;
            mb.PlanoContaId = m.PlanoContaId;
            mb.RelacionamentoId = m.RelacionamentoId;
            mb.CodigoExterno = m.CodigoExterno;
            mb.TipoMovtoBanco = m.TipoMovtoBanco;
            mb.Transferencia = m.Transferencia;
            return mb;
        }
        public async Task<Resultado> UpdateMovtoBanco(MovtoBancoView m)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            LanctoEmpreendimento le = null;
            Periodo pe = null;
            if (m.Exportado)
            {
                le = await _context.LanctoEmpreendimento.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.OrigemId == m.Id && x.Origem == OrigemLancto.ImportadoCC);
                if (le != null)
                {
                    pe = await _context.Periodo.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == le.PeriodoId);
                    if (pe != null)
                    {
                        if (pe.Status != StatusPeriodo.EntradaDeDados)
                        {
                            r.Ok = false;
                            r.ErrMsg = "Período referente a esse lançamento está na fase " + pe.NomeStatus + ", não permite alterações";
                            return r;
                        }
                    }
                }
            }

            var ce = await _context.ContaCorrenteEmpreendimento.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.ContaCorrenteId == m.ContaCorrenteId);
            if (ce != null)
            {
                pe = await _context.Periodo.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.EmpreendimentoId == ce.EmpreendimentoId &&
                                                                    x.DataInicio <= m.DataMovto && x.DataFim >= m.DataMovto);
                if (pe != null)
                {
                    if (pe.Status != StatusPeriodo.EntradaDeDados)
                    {
                        r.Ok = false;
                        r.ErrMsg = "Período referente a esse lançamento está na fase " + pe.NomeStatus + ", não permite alterações";
                        return r;
                    }
                }
            }

            MovtoBanco mb = await _context.MovtoBanco.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == m.Id);
            if (mb == null)
            {
                r.Ok = false;
                r.ErrMsg = "lançamento não encontrado no banco de dados";
                return r;
            }

            mb = ParseMovtoViewToMovto(m, mb);
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.MovtoBanco.Update(mb);
                await _context.SaveChangesAsync();
                if (m.Exportado && pe != null)
                {
                    var ilr = await _context.LanctoEmpRelacionamento.FirstOrDefaultAsync(x => x.LanctoEmpreendimentoId == le.Id && x.TenantId == si.TenantId);
                    if (ilr != null)
                    {
                        _context.LanctoEmpRelacionamento.Remove(ilr);
                        await _context.SaveChangesAsync();
                    }
                    _context.LanctoEmpreendimento.Remove(le);
                    await _context.SaveChangesAsync();
                    var nle = ConvertMovtoBancoToLanctoEmp(m, pe.Id);
                    nle.TenantId = si.TenantId;
                    await _context.LanctoEmpreendimento.AddAsync(nle);
                    await _context.SaveChangesAsync();
                    if (nle.RelacionamentoId != 0)
                    {
                        nle.LanctoEmpRelacionamento.LanctoEmpreendimentoId = nle.Id;
                        nle.LanctoEmpRelacionamento.TenantId = si.TenantId;
                        await _context.LanctoEmpRelacionamento.AddAsync(nle.LanctoEmpRelacionamento);
                    }
                    await _context.SaveChangesAsync();
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
        public async Task<Resultado> IncluirMovtoBancoImportadoStudioAsync(ContaCorrente c, string filename, List<MovtoBanco> lm, Func<int, int> callback)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            RelacionamentoService rs = new(_context, null, null, _sessionStorageService, _sessionService, _mapper);
            List<LanctoLoteImportacao> ll = new();
            List<PlanoConta> lpc = null;
            if (si.TenantApp == TenantApp.Studio)
            {
            lpc = await FindAllPlanoConta();
            }
            else
            {
                var ctaemp = await _context.ContaCorrenteEmpreendimento.Where(x => x.TenantId == si.TenantId && x.ContaCorrenteId == c.Id).FirstOrDefaultAsync();
                lpc = await FindAllPlanoContaByEmpId(ctaemp.EmpreendimentoId);
            }
            List<PlanoGerencial> lpg = await FindAllPlanoGerencialByTenant();
            DateTime dtini = lm.Min(x => x.DataMovto);
            DateTime dtfim = lm.Max(x => x.DataMovto);

            List<MovtoBanco> ld = await _context.MovtoBanco.Where(x => x.TenantId == si.TenantId && x.ContaCorrenteId == c.Id && x.DataMovto >= dtini && x.DataMovto <= dtfim).ToListAsync();

            int relacionamentoDefaultId;
            StudioConfig studioConfig = null;
            InCorpConfig inCorpConfig = null;

            if (si.TenantApp == TenantApp.Studio)
            {
                studioConfig = await _context.StudioConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
                relacionamentoDefaultId = studioConfig.RelacionamentoDefaultId;
            }
            else
            {
                inCorpConfig = await _context.InCorpConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
                relacionamentoDefaultId = inCorpConfig.RelacionamentoDefaultId;
            }

            var emp = await _context.Empreendimento.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);

            try
            {
                _context.Database.BeginTransaction();

                _context.MovtoBanco.RemoveRange(ld);
                await _context.SaveChangesAsync();
                ld.Clear();
                ld = null;

                int i = 0;
                int j = -1;
                LoteMovtoBanco lt = new()
                {
                    DataImp = Constante.Now,
                    ContaCorrenteId = c.Id,
                    TenantId = si.TenantId,
                    FileName = filename
                };
                await _context.LoteMovtoBanco.AddAsync(lt);
                await _context.SaveChangesAsync();

                foreach (MovtoBanco l in lm)
                {
                    l.ContaCorrenteId = c.Id;
                    l.TenantId = si.TenantId;
                    l.LoteMovtoBancoId = lt.Id;
                    l.Seq = double.Parse(l.DataMovto.ToString("yyyyMMdd")) + si.Seq;
                    if (l.RelacionamentoId == 0)
                    {
                        Relacionamento rela = await _context.Relacionamento.FirstOrDefaultAsync(x => x.Nome == l.RelacionamentoV && x.TenantId == si.TenantId);
                        if (rela == null)
                        {
                            if (relacionamentoDefaultId == 0)
                            {
                                rela = await _context.Relacionamento.FirstOrDefaultAsync(x => x.Id == relacionamentoDefaultId && x.TenantId == si.TenantId);
                                if (rela == null)
                                {
                                    rela = new Relacionamento()
                                    {
                                        Nome = "Relacionamento padrão",
                                        RazaoSocial = "Relacionamento padrão",
                                        CPFCNPJ = string.Empty,
                                        CodigoExternoCliente = l.RelacionamentoCodigoExternoV,
                                        CodigoExternoFornecedor = l.RelacionamentoCodigoExternoV
                                    };
                                    rela = await rs.InsertIncompletoAsync(rela, new List<Contato>());
                                    if (si.TenantApp == TenantApp.Studio)
                                    {
                                        studioConfig.RelacionamentoDefaultId = rela.Id;
                                        _context.StudioConfig.Update(studioConfig);
                                    }
                                    else
                                    {
                                        inCorpConfig.RelacionamentoDefaultId = rela.Id;
                                        _context.InCorpConfig.Update(inCorpConfig);
                                    }
                                    relacionamentoDefaultId = rela.Id;
                                    await _context.SaveChangesAsync();
                                }
                            }
                            l.RelacionamentoId = relacionamentoDefaultId;
                        }
                        else
                        {
                            l.RelacionamentoId = rela.Id;
                        }
                    }

                    PlanoConta pc = null;
                    if (l.PlanoContaId == 0 && !l.Transferencia)
                    {
                        pc = lpc.FirstOrDefault(x => x.Nome.ToLower() == l.PlanoContaV.ToLower());
                        if (pc == null)
                        {
                            int pcid;
                            string nomepc = string.Empty;
                            TipoPlanoConta tpplano;
                            if (l.TipoMovtoBanco == TipoMovtoBanco.Credito)
                            {
                                if (si.TenantApp == TenantApp.Studio)
                                {
                                    pcid = studioConfig.PlanoContaDefaultReceitaId;
                                }
                                else
                                {
                                    pcid = inCorpConfig.PlanoContaDefaultReceitaId;
                                }
                                nomepc = "Receita Padrão";
                                tpplano = TipoPlanoConta.Receita;
                            }
                            else
                            {
                                if (l.TipoMovtoBanco == TipoMovtoBanco.Debito)
                                {
                                    if (si.TenantApp == TenantApp.Studio)
                                    {
                                        pcid = studioConfig.PlanoContaDefaultDespesaId;
                                    }
                                    else
                                    {
                                        pcid = inCorpConfig.PlanoContaDefaultDespesaId;
                                    }
                                    nomepc = "Despesa Padrão";
                                    tpplano = TipoPlanoConta.Despesa;
                                }
                                else
                                {
                                    throw new Exception("Não implementado tipomovto dif cred e deb");
                                }
                            }
                            pc = lpc.FirstOrDefault(x => x.Id == pcid);
                            if (pc == null)
                            {
                                PlanoGerencial pg = new()
                                {
                                    TenantId = si.TenantId,
                                    AporteDistribuicao = false,
                                    Nome = nomepc,
                                    CodigoExterno = string.Empty,
                                    TipoPlanoConta = tpplano
                                };
                                await _context.PlanoGerencial.AddAsync(pg);
                                await _context.SaveChangesAsync();
                                pc = new()
                                {
                                    TenantId = si.TenantId,
                                    AporteDistribuicao = false,
                                    Nome = nomepc,
                                    EmpreendimentoId = emp.Id,
                                    Ratear = false,
                                    CodigoExterno = string.Empty,
                                    Tipo = tpplano,
                                    PlanoGerencialId = pg.Id
                                };
                                await _context.PlanoConta.AddAsync(pc);
                                await _context.SaveChangesAsync();
                                lpc.Add(pc);
                                if (pc.Tipo == TipoPlanoConta.Receita)
                                {
                                    if (si.TenantApp == TenantApp.Studio)
                                    {
                                        studioConfig.PlanoContaDefaultReceitaId = pc.Id;
                                        _context.StudioConfig.Update(studioConfig);
                                    }
                                    else
                                    {
                                        inCorpConfig.PlanoContaDefaultReceitaId = pc.Id;
                                        _context.InCorpConfig.Update(inCorpConfig);
                                    }
                                }
                                else
                                {
                                    if (si.TenantApp == TenantApp.Studio)
                                    {
                                        studioConfig.PlanoContaDefaultDespesaId = pc.Id;
                                        _context.StudioConfig.Update(studioConfig);
                                    }
                                    else
                                    {
                                        inCorpConfig.PlanoContaDefaultReceitaId = pc.Id;
                                        _context.InCorpConfig.Update(inCorpConfig);
                                    }
                                }
                                await _context.SaveChangesAsync();
                            };

                        }
                    };
                    l.PlanoContaId = pc.Id;

                    await _context.MovtoBanco.AddAsync(l);

                    if (callback != null)
                    {
                        i++;
                        double v0 = i;
                        double v00 = lm.Count;
                        double v1 = v0 / v00 * 100;
                        int v2 = Convert.ToInt32(Math.Round(v1, 0));
                        if (v2 > j)
                        {
                            j = v2 + 10;
                            callback(v2);
                        }
                    }
                }
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
                callback(999);
                r.Ok = true;
                return r;
            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;
            }
        }
        public async Task<Resultado> IncluirMovtoBancoImportadoNContasAsync(List<ContaCorrente> lc, string filename, List<MovtoBanco> lm, Func<int, int> callback)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            string loteGuid = Guid.NewGuid().ToString();
            try
            {

                await _context.Database.BeginTransactionAsync();
                foreach (var c in lc)
                {
                    var lmc = lm.Where(x => x.ContaCorrenteId == c.Id).ToList();
                    if (lmc.Count > 0)
                    {
                        r = await IncluirMovtoBancoImportadoAsync(c, filename, lmc, callback, loteGuid);
                        if (r.Ok == false)
                        {
                            await _context.Database.RollbackTransactionAsync();
                            return r;
                        }
                    }
                }
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
            r.Detalhe = loteGuid;
            r.Ok = true;
            return r;
        }
        public async Task<Resultado> IncluirMovtoBancoImportadoAsync(ContaCorrente c, string filename, List<MovtoBanco> lm, Func<int, int> callback, string LoteGuid = null)
        {
            if (LoteGuid == null)
            {
                LoteGuid = string.Empty;
            }

            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            RelacionamentoService rs = new(_context, null, null, _sessionStorageService, _sessionService, _mapper);
            List<LanctoLoteImportacao> ll = new();
            List<PlanoConta> lpc = await FindAllPlanoContaByEmpId(c.EmpreendimentoId);
            List<PlanoGerencial> lpg = await FindAllPlanoGerencialByTenant();
            DateTime dtini = lm.Min(x => x.DataMovto);
            DateTime dtfim = lm.Max(x => x.DataMovto);
            if (await (from ContaCorrente in _context.ContaCorrente
                       join ContaCorrenteEmpreendimento in _context.ContaCorrenteEmpreendimento on ContaCorrente.Id equals ContaCorrenteEmpreendimento.ContaCorrenteId
                       join Periodo in _context.Periodo on ContaCorrenteEmpreendimento.EmpreendimentoId equals Periodo.EmpreendimentoId
                       select new { Periodo, ContaCorrente }).AsNoTracking().AnyAsync(x => x.Periodo.Status != StatusPeriodo.EntradaDeDados
                      && x.Periodo.DataInicio >= dtini && x.Periodo.DataFim <= dtfim
                      && x.ContaCorrente.Id == c.Id
                      && x.Periodo.TenantId == si.TenantId))
            {

                r.Ok = false;
                r.ErrMsg = "Todos os períodos entre " + dtini.ToString("MM/yyyy") + " a " + dtfim.ToString("MM/yyyy") + " devem estar na fase de entrada de dados.";
                return r;
            }

            List<MovtoBanco> ld = await _context.MovtoBanco.Where(x => x.TenantId == si.TenantId && x.ContaCorrenteId == c.Id && x.DataMovto >= dtini && x.DataMovto <= dtfim).ToListAsync();

            var lanctos = await (from MovtoBanco in _context.MovtoBanco
                                 join LanctoEmpreendimento in _context.LanctoEmpreendimento on MovtoBanco.Id equals LanctoEmpreendimento.OrigemId
                                 select new { LanctoEmpreendimento, MovtoBanco.DataMovto, MovtoBanco.TenantId, MovtoBanco.ContaCorrenteId }).Where(
                                      x => x.TenantId == si.TenantId
                                           && x.ContaCorrenteId == c.Id
                                           && x.DataMovto >= dtini
                                           && x.DataMovto <= dtfim
                                           && x.LanctoEmpreendimento.Origem == OrigemLancto.ImportadoCC).ToListAsync();

            bool mytrans = (_context.Database.CurrentTransaction == null);
            if (mytrans)
            {
                await _context.Database.BeginTransactionAsync();
            }
            try
            {
                foreach (var it in lanctos)
                {
                    _context.LanctoEmpreendimento.Remove(it.LanctoEmpreendimento);
                }
                _context.MovtoBanco.RemoveRange(ld);
                await _context.SaveChangesAsync();
                lanctos.Clear();
                lanctos = null;
                ld.Clear();
                ld = null;

                int i = 0;
                int j = -1;
                LoteMovtoBanco lt = new()
                {
                    DataImp = Constante.Now,
                    ContaCorrenteId = c.Id,
                    TenantId = si.TenantId,
                    FileName = filename,
                    GUID = LoteGuid
                };
                await _context.LoteMovtoBanco.AddAsync(lt);
                await _context.SaveChangesAsync();

                foreach (MovtoBanco l in lm)
                {
                    l.ContaCorrenteId = c.Id;
                    l.TenantId = si.TenantId;
                    l.LoteMovtoBancoId = lt.Id;
                    l.Seq = double.Parse(l.DataMovto.ToString("yyyyMMdd")) + si.Seq;
                    if (l.RelacionamentoId == 0)
                    {
                        Relacionamento rela = await _context.Relacionamento.FirstOrDefaultAsync(x => x.Nome == l.RelacionamentoV && x.TenantId == si.TenantId);
                        if (rela == null)
                        {
                            rela = new Relacionamento()
                            {
                                Nome = l.RelacionamentoV,
                                CPFCNPJ = string.Empty,
                                CodigoExternoCliente = l.RelacionamentoCodigoExternoV,
                                CodigoExternoFornecedor = l.RelacionamentoCodigoExternoV
                            };
                            rela = await rs.InsertIncompletoAsync(rela, new List<Contato>());
                        }
                        l.RelacionamentoId = rela.Id;
                    }
                    PlanoGerencial pg = null;
                    if (l.PlanoGerencialId == 0)
                    {
                        if (l.PlanoGerencialCodigoExternoV != string.Empty)
                        {
                            pg = lpg.FirstOrDefault(x => x.CodigoExterno.ToLower() == l.PlanoGerencialCodigoExternoV.ToLower());
                            if (pg == null)
                            {
                                pg = lpg.FirstOrDefault(x => x.Nome.ToLower() == l.PlanoGerencialV.ToLower());
                            }
                        }
                        else
                        {
                            pg = lpg.FirstOrDefault(x => x.Nome.ToLower() == l.PlanoGerencialV.ToLower());
                        }
                        if (pg == null)
                        {
                            pg = new PlanoGerencial()
                            {
                                TenantId = si.TenantId,
                                AporteDistribuicao = false,
                                Nome = l.PlanoGerencialV,
                                CodigoExterno = l.PlanoGerencialCodigoExternoV,
                                NomeGrupoContaAuxiliar = string.Empty
                            };
                            if (l.Transferencia == true)
                            {
                                pg.TipoPlanoConta = TipoPlanoConta.Auxiliar;
                            }
                            else
                            {
                                if (l.TipoMovtoBanco == TipoMovtoBanco.Credito)
                                {
                                    pg.TipoPlanoConta = TipoPlanoConta.Receita;
                                }
                                else
                                {
                                    if (l.TipoMovtoBanco == TipoMovtoBanco.Debito)
                                    {
                                        pg.TipoPlanoConta = TipoPlanoConta.Despesa;
                                    }
                                    else
                                    {
                                        throw new Exception("Não implementado tipomovto dif cred e deb");
                                    }
                                }
                            }
                            await _context.PlanoGerencial.AddAsync(pg);
                            await _context.SaveChangesAsync();
                            lpg.Add(pg);
                        }
                    }
                    else
                    {
                        pg = lpg.FirstOrDefault(x => x.Id == l.PlanoGerencialId);
                    }
                    PlanoConta pc = null;
                    if (l.PlanoContaId == 0)
                    {
                        if (l.PlanoContaCodigoExternoV != string.Empty)
                        {
                            pc = lpc.FirstOrDefault(x => x.CodigoExterno.ToLower() == l.PlanoContaCodigoExternoV.ToLower());
                            if (pc == null)
                            {
                                pc = lpc.FirstOrDefault(x => x.Nome.ToLower() == l.PlanoContaV.ToLower());
                            }
                        }
                        else
                        {
                            pc = lpc.FirstOrDefault(x => x.Nome.ToLower() == l.PlanoContaV.ToLower());
                        }
                        if (pc == null)
                        {
                            pc = new PlanoConta()
                            {
                                TenantId = si.TenantId,
                                AporteDistribuicao = false,
                                Nome = l.PlanoContaV,
                                EmpreendimentoId = c.EmpreendimentoId,
                                Ratear = true,
                                CodigoExterno = l.PlanoContaCodigoExternoV
                            };
                            if (l.Transferencia == true)
                            {
                                pc.Ratear = false;
                            }
                            if (pg == null)
                            {
                                throw new Exception(" Plano de conta sem conta gerencial " + l.PlanoContaV);
                            }
                            pc.PlanoGerencialId = pg.Id;
                            pc.Tipo = pg.TipoPlanoConta;
                            if (pc.NomeCurto == null || pc.NomeCurto == string.Empty)
                            {
                                pc.NomeCurto = pc.Nome;
                            }
                            await _context.PlanoConta.AddAsync(pc);
                            await _context.SaveChangesAsync();
                            lpc.Add(pc);
                        };
                        l.PlanoContaId = pc.Id;
                    }

                    await _context.MovtoBanco.AddAsync(l);

                    if (callback != null)
                    {
                        i++;
                        double v0 = i;
                        double v00 = lm.Count;
                        double v1 = v0 / v00 * 100;
                        int v2 = Convert.ToInt32(Math.Round(v1, 0));
                        if (v2 > j)
                        {
                            j = v2 + 10;
                            callback(v2);
                        }
                    }
                }
                await _context.SaveChangesAsync();
                if (mytrans)
                {
                    await _context.Database.CommitTransactionAsync();
                }
                callback(999);
                r.Ok = true;
                return r;
            }
            catch
            {
                if (mytrans)
                {
                    await _context.Database.RollbackTransactionAsync();
                }
                throw;
            }
        }
        public async Task<List<LoteMovtoBanco>> FindAllLotesByCCIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.LoteMovtoBanco.Where(x => x.ContaCorrenteId == id && x.TenantId == si.TenantId).OrderByDescending(x => x.DataImp).ToListAsync();
        }
        public async Task<Resultado> ExcluirLote(LoteMovtoBanco ilote)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            DateTime mindt;
            DateTime maxdt;

            List<LoteMovtoBanco> lotes = new();
            if (ilote.GUID != null && ilote.GUID != string.Empty)
            {
                _context.ChangeTracker.Clear();
                lotes = await _context.LoteMovtoBanco.Where(x => x.TenantId == si.TenantId && x.GUID == ilote.GUID).ToListAsync();
            }
            else
            {
                lotes.Add(ilote);
            }


            foreach (var lote in lotes)
            {
                if (await _context.MovtoBanco.AnyAsync(x => x.LoteMovtoBancoId == lote.Id && x.TenantId == si.TenantId))
                {
                    mindt = await _context.MovtoBanco.Where(x => x.LoteMovtoBancoId == lote.Id && x.TenantId == si.TenantId).MinAsync(x => x.DataMovto);
                    maxdt = await _context.MovtoBanco.Where(x => x.LoteMovtoBancoId == lote.Id && x.TenantId == si.TenantId).MaxAsync(x => x.DataMovto);
                    lote.Deleted = false;
                }
                else
                {
                    _context.LoteMovtoBanco.Remove(lote);
                    lote.Deleted = true;
                    await _context.SaveChangesAsync();
                    continue;
                }

                if (await (from LanctoEmpreendimento in _context.LanctoEmpreendimento
                           join Periodo in _context.Periodo on LanctoEmpreendimento.PeriodoId equals Periodo.Id
                           join Empreendimento in _context.Empreendimento on Periodo.EmpreendimentoId equals Empreendimento.Id
                           join ContaCorrenteEmpreendimento in _context.ContaCorrenteEmpreendimento on Empreendimento.Id equals ContaCorrenteEmpreendimento.EmpreendimentoId
                           join ContaCorrente in _context.ContaCorrente on ContaCorrenteEmpreendimento.ContaCorrenteId equals ContaCorrente.Id
                           select new
                           {
                               ContaCorrenteId = ContaCorrente.Id,
                               LanctoEmpreendimento.TenantId,
                               Periodo.Status,
                               LanctoEmpreendimento.DataMovto
                           }).AsNoTracking().AnyAsync(x => x.ContaCorrenteId == lote.ContaCorrenteId
                                                        && x.TenantId == si.TenantId
                                                        && x.Status != StatusPeriodo.EntradaDeDados
                                                        && x.DataMovto >= mindt
                                                        && x.DataMovto <= maxdt))
                {
                    r.Ok = false;
                    r.ErrMsg = "Todos os períodos entre " + mindt.ToString("MM/yyyy") + " a " + maxdt.ToString("MM/yyyy") + " devem estar na fase de entrada de dados.";
                    return r;
                }
            }

            lotes = lotes.Where(x => x.Deleted == false).ToList();
            if (lotes.Count == 0)
            {
                r.Ok = true;
                return r;
            }

            await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var lote in lotes)
                {
                    var ler = await (from LanctoEmpRelacionamento in _context.LanctoEmpRelacionamento
                                     join LanctoEmpreendimento in _context.LanctoEmpreendimento on LanctoEmpRelacionamento.LanctoEmpreendimentoId equals LanctoEmpreendimento.Id
                                     join MovtoBanco in _context.MovtoBanco on LanctoEmpreendimento.OrigemId equals MovtoBanco.Id
                                     select new
                                     {
                                         LanctoEmpRelacionamento,
                                         LanctoEmpreendimento.Origem,
                                         MovtoBanco.LoteMovtoBancoId
                                     }).Where(x => x.Origem == OrigemLancto.ImportadoCC
                                                                  && x.LanctoEmpRelacionamento.TenantId == si.TenantId
                                                                  && x.LoteMovtoBancoId == lote.Id).ToListAsync();
                    foreach (var iler in ler)
                    {
                        _context.LanctoEmpRelacionamento.Remove(iler.LanctoEmpRelacionamento);
                    }
                    if (ler.Count > 0)
                    {
                        await _context.SaveChangesAsync();
                    }

                    var le = await (from LanctoEmpreendimento in _context.LanctoEmpreendimento
                                    join MovtoBanco in _context.MovtoBanco on LanctoEmpreendimento.OrigemId equals MovtoBanco.Id
                                    select new
                                    {
                                        LanctoEmpreendimento,
                                        MovtoBanco.LoteMovtoBancoId
                                    }).Where(x => x.LanctoEmpreendimento.Origem == OrigemLancto.ImportadoCC
                                                                 && x.LanctoEmpreendimento.TenantId == si.TenantId
                                                                 && x.LoteMovtoBancoId == lote.Id).ToListAsync();
                    List<LanctoEmpreendimento> led = new();
                    foreach (var ile in le)
                    {
                        led.Add(ile.LanctoEmpreendimento);
                    }
                    _context.LanctoEmpreendimento.RemoveRange(led);
                    var l = await _context.MovtoBanco.Where(x => x.LoteMovtoBancoId == lote.Id && x.TenantId == si.TenantId).ToListAsync();
                    _context.MovtoBanco.RemoveRange(l);
                    await _context.SaveChangesAsync();
                    _context.LoteMovtoBanco.Remove(lote);
                    await _context.SaveChangesAsync();
                    l.Clear();
                    l = null;
                    led.Clear();
                    led = null;
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
        public async Task<List<ContaCorrenteExtrato>> FindContaCorrenteExtratoByContaCorrenteData(int contacid, DateTime dtini, DateTime dtfim)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ContaCorrenteExtrato.Where(x => x.TenantId == si.TenantId && x.ContaCorrenteid == contacid && x.DataFim <= dtfim && x.DataInicio >= dtini).OrderBy(x => x.DataInicio).ToListAsync();
        }
        public async Task<Resultado> AddContaCorrenteExtrato(ContaCorrenteExtrato c)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (await _context.ContaCorrenteExtrato.AnyAsync(x => x.TenantId == si.TenantId && x.ContaCorrenteid == c.ContaCorrenteid &&
                           x.DataInicio <= c.DataFim && x.DataFim >= c.DataInicio))
            {
                r.Ok = false;
                r.ErrMsg = "Já existe um extrato no intervalo de data informado. Permitido somente 1 extrato por mes.";
                return r;
            }

            c.TenantId = si.TenantId;
            try
            {
                await _context.ContaCorrenteExtrato.AddAsync(c);
                await _context.SaveChangesAsync();
                r.Ok = true;
                return r;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<Resultado> UpdateContaCorrenteExtrato(ContaCorrenteExtrato c)
        {
            Resultado r = new();
            try
            {
                _context.ContaCorrenteExtrato.Update(c);
                await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear();
                r.Ok = true;
                return r;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<Resultado> DeleteContaCorrenteExtrato(ContaCorrenteExtrato c)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            c.TenantId = si.TenantId;
            try
            {
                await _context.Database.BeginTransactionAsync();
                _context.ContaCorrenteExtrato.Remove(c);
                await _context.SaveChangesAsync();
                if (c.FileId > 0)
                {
                    UpdaloadService us = new(_context, _sessionStorageService, _sessionService, _mapper, null, null);
                    await us.Delete(c.FileId);
                }
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
        /*
             public LanctoBaseCorrecao CalcularCorrecao(List<IndiceEconomicoMovto> li, LanctoBaseCorrecao b)
              {
                  LanctoBaseCorrecao r = new()
                  {
                      DataInicio = b.DataInicio,
                      DataFinal = b.DataFinal,
                      IndiceEconomicoId = b.IndiceEconomicoId,
                      ValorInicial = b.ValorInicial,
                      ValorFinal = b.ValorInicial,
                      Id = b.Id
                  };
                  foreach (var i in li)
                  {
                      i.DataFim = UtilsClass.GetUltimo(i.DataFim);
                      if (i.DataInicio < b.DataInicio || i.DataInicio > b.DataFinal)
                      {
                          continue;
                      }
                      LanctoCorrigido c = new()
                      {
                          DataRef = UtilsClass.GetUltimo(i.DataFim),
                          Correcao = Math.Round( r.ValorFinal * (i.Valor / 100 + b.TaxaMensal / 100),2),
                          Indice = i.Valor,
                          TaxaFixa = b.TaxaMensal
                      };
                      r.LanctoCorrigido.Add(c);
                      r.ValorFinal += c.Correcao;
                      c.ValorAcumulado = r.ValorFinal;
                      r.DataFinal = i.DataFim;
                  }
                  return r;
              }
        */
        public SocioResultadoExtratoPeriodo CalcularCorrecaoDistribuicao(IndiceEconomico indice, List<IndiceEconomicoMovto> li, SocioResultadoExtratoPeriodo resultado, SocioResultadoExtrato so, DateTime dtfinal)
        {

            if (indice.TipoPeriodoIndice == TipoPeriodoIndice.Mensal)
            {
                foreach (var i in li)
                {
                    i.DataFim = UtilsClass.GetUltimo(i.DataInicio);
                    if (i.DataInicio < resultado.DataUltimaCorrecao || i.DataInicio < resultado.DataRef || i.DataInicio > dtfinal)
                    {
                        continue;
                    }
                    if (resultado.GetValorByDate(i.DataFim) <= 0)
                    {
                        return resultado;
                    }
                    double fator = 1;
                    if (i.DataInicio <= resultado.DataUltimaCorrecao && i.DataFim > resultado.DataUltimaCorrecao)
                    {
                        fator = i.DataFim.Subtract(resultado.DataUltimaCorrecao).TotalDays / 30.0;
                    }
                    double basecalculo = Math.Round(resultado.GetValorByDate(i.DataFim), 2);
                    SocioResultadoPeriodoLancto sdl = new()
                    {
                        DataRef = i.DataFim,
                        PeriodoId = resultado.PeriodoId,
                        SocioResultadoPeriodoId = resultado.SocioResultadoPeriodoId,
                        TipoLancto = TipoLanctoSocioResultado.Correcao,
                        Valor = Math.Round(basecalculo * (i.Valor * fator / 100 + resultado.Taxa * fator / 100), 2)
                    };
                    if (resultado.Taxa != 0)
                    {
                        sdl.Historico = "Correção monetária referente a distribuição do resultado do mes " + resultado.DataRef.ToString("MM/yyyy") + " retida, " +
                            " base de cálculo " + basecalculo.ToString("N2", Constante.Culture) + " índice " +
                             i.Valor.ToString("N5", Constante.Culture) + " taxa fixa: " + resultado.Taxa.ToString("N2", Constante.Culture) + "%";
                    }
                    else
                    {
                        sdl.Historico = "Correção monetária referente a distribuição do resultado do mes " + resultado.DataRef.ToString("MM/yyyy") + " retida, " +
                            " base de cálculo " + basecalculo.ToString("N2", Constante.Culture) + " índice " +
                            i.Valor.ToString("N5", Constante.Culture) + "%";
                    }
                    resultado.Lanctos.Add(sdl);
                    resultado.Lanctos = resultado.Lanctos.OrderBy(x => x.DataRef).ToList();
                    AjustarDistribuicao(so);
                    resultado.DataUltimaCorrecao = i.DataFim;
                }
            }
            else
            {
                if (indice.TipoPeriodoIndice == TipoPeriodoIndice.Diario)
                {
                    string mmaa = string.Empty;
                    double indiceacu = 1;
                    double valorinicialmes = resultado.GetValorByDate(resultado.DataUltimaCorrecao);
                    double valorant = 0;
                    double amortizacao = 0;
                    double fator = 1;
                    int qtdedias = 0;
                    double taxadiaria = 0;
                    if (dtfinal.ToString("MMyyyy") == Constante.Today.ToString("MMyyyy"))
                    {
                        dtfinal = UtilsClass.GetInicio(Constante.Today);
                    }
                    double dias = UtilsClass.GetUltimo(resultado.DataUltimaCorrecao).Subtract(resultado.DataUltimaCorrecao).TotalDays;
                    if (dias > 0)
                    {
                        fator = dias / 30;
                    }
                    SocioResultadoPeriodoLancto sdl = null;
                    foreach (var i in li)
                    {
                        if (i.DataInicio <= resultado.DataUltimaCorrecao || i.DataInicio >= dtfinal)
                        {
                            continue;
                        }
                        if (mmaa != i.DataInicio.ToString("MMyyyy"))
                        {
                            sdl = new()
                            {
                                DataRef = i.DataInicio,
                                PeriodoId = resultado.PeriodoId,
                                SocioResultadoPeriodoId = resultado.SocioResultadoPeriodoId,
                                TipoLancto = TipoLanctoSocioResultado.Correcao,
                                Valor = 0,
                                Historico = string.Empty
                            };
                            resultado.Lanctos = resultado.Lanctos.OrderBy(x => x.DataRef).ToList();
                            if (mmaa != string.Empty)
                            {
                                fator = 1;
                            }
                            qtdedias = li.Where(x => x.DataInicio >= UtilsClass.GetInicio(i.DataInicio) && x.DataInicio <= UtilsClass.GetUltimo(i.DataInicio)).Count();
                            if (resultado.Taxa != 0)
                            {
                                taxadiaria = Math.Pow((resultado.Taxa / 100 + 1), 1 / (qtdedias * 1.0)) - 1.0;
                            }
                            else
                            {
                                taxadiaria = 0;
                            }
                            mmaa = i.DataInicio.ToString("MMyyyy");
                            valorinicialmes = resultado.GetValorByDate(i.DataInicio);
                            amortizacao = -1;
                            indiceacu = 1;
                            resultado.Lanctos.Add(sdl);
                        };
                        if (amortizacao == -1)
                        {
                            amortizacao = 0;
                        }
                        else
                        {
                            amortizacao = resultado.Lanctos.Where(x => x.TipoLancto == TipoLanctoSocioResultado.Pagamento && x.DataRef > resultado.DataUltimaCorrecao && x.DataRef <= i.DataInicio).Sum(x => x.Valor);
                        }
                        indiceacu += indiceacu * i.Valor / 100;
                        valorant = valorinicialmes - amortizacao;
                        if (valorinicialmes < 0.01)
                        {
                            valorinicialmes = 0;
                            valorant = 0;
                        }
                        valorinicialmes += (valorinicialmes - amortizacao) * (i.Valor / 100 + taxadiaria) - amortizacao;
                        sdl.Valor += valorinicialmes - valorant;
                        if (resultado.Taxa != 0)
                        {
                            sdl.Historico = "Correção monetária referente a distribuição do resultado do mes " + resultado.DataRef.ToString("MM/yyyy") + " retida " +
                                            " , indice: " + ((indiceacu - 1) * 100).ToString("N6", Constante.Culture) + "% juros mensal: " + (resultado.Taxa * fator).ToString("N5", Constante.Culture);
                        }
                        else
                        {
                            sdl.Historico = "Correção monetária referente a distribuição do resultado do mes " + resultado.DataRef.ToString("MM/yyyy") + " retida " +
                                            " , indice: " + ((indiceacu - 1) * 100).ToString("N6", Constante.Culture) + "% ";
                        }
                        sdl.DataRef = i.DataInicio;
                        resultado.DataUltimaCorrecao = i.DataInicio;
                        AjustarDistribuicao(so);
                    }
                }
            }
            return resultado;
        }
        public SocioDebitoView CalcularCorrecaoDebito(IndiceEconomico indice, List<IndiceEconomicoMovto> li, SocioDebitoView debito, DateTime dtfinal)
        {

            if (indice.TipoPeriodoIndice == TipoPeriodoIndice.Mensal)
            {
                foreach (var i in li)
                {
                    i.DataFim = UtilsClass.GetUltimo(i.DataInicio);
                    if (i.DataInicio < debito.SocioDebito.DataUltimaCorrecao || i.DataInicio < debito.SocioDebito.DataInicioCorrecao || i.DataInicio > dtfinal)
                    {
                        continue;
                    }
                    if (debito.GetValorSaldoByData(i.DataFim) <= 0)
                    {
                        return debito;
                    }
                    double fator = 1;
                    if (i.DataInicio <= debito.SocioDebito.DataUltimaCorrecao && i.DataFim > debito.SocioDebito.DataUltimaCorrecao)
                    {
                        fator = i.DataFim.Subtract(debito.SocioDebito.DataUltimaCorrecao).TotalDays / 30.0;
                    }
                    SocioDebitoLancto sdl = new()
                    {
                        SocioDebitoId = debito.SocioDebito.Id,
                        IndiceEconomicoMovotId = i.Id,
                        DataLancto = i.DataFim,
                        TipoLanctoDebito = TipoLanctoDebito.Juros,
                        Valor = Math.Round(debito.GetValorSaldoByData(i.DataFim) * (i.Valor * fator / 100 + debito.SocioDebito.Taxa * fator / 100), 2),
                        Origem = OrigemLancto.SistemaJuros,
                        TenantId = debito.SocioDebito.TenantId,
                        GUID = Guid.NewGuid().ToString(),
                        EmpreendimentoSocioId = debito.SocioDebito.EmpreendimentoSocioId,
                        DataCriacao = Constante.Today
                    };
                    if (debito.SocioDebito.Taxa != 0)
                    {
                        sdl.Historico = "Correção monetária, indice: " + i.Valor.ToString("N5", Constante.Culture) + " taxa fixa: " + debito.SocioDebito.Taxa.ToString("N2", Constante.Culture) + "%";
                    }
                    else
                    {
                        sdl.Historico = "Correção monetária, indice: " + i.Valor.ToString("N5", Constante.Culture) + "%";
                    }
                    debito.Lanctos.Add(sdl);
                    debito.Lanctos = debito.Lanctos.OrderBy(x => x.DataLancto).ToList();
                    debito.SocioDebito.DataUltimaCorrecao = i.DataFim;
                }
            }
            else
            {
                if (indice.TipoPeriodoIndice == TipoPeriodoIndice.Diario)
                {
                    string mmaa = string.Empty;
                    double indiceacu = 1;
                    double valorinicialmes = debito.GetValorSaldoByData(debito.SocioDebito.DataUltimaCorrecao);
                    double valorant = 0;
                    double amortizacao = 0;
                    double fator = 1;
                    int qtdedias = 0;
                    double taxadiaria = 0;
                    if (dtfinal.ToString("MMyyyy") == Constante.Today.ToString("MMyyyy"))
                    {
                        dtfinal = UtilsClass.GetInicio(Constante.Today);
                    }
                    double dias = UtilsClass.GetUltimo(debito.SocioDebito.DataUltimaCorrecao).Subtract(debito.SocioDebito.DataUltimaCorrecao).TotalDays;
                    if (dias > 0)
                    {
                        fator = dias / 30;
                    }
                    SocioDebitoLancto sdl = null;
                    foreach (var i in li)
                    {
                        if (i.DataInicio <= debito.SocioDebito.DataUltimaCorrecao || i.DataInicio >= dtfinal)
                        {
                            continue;
                        }
                        if (mmaa != i.DataInicio.ToString("MMyyyy"))
                        {
                            sdl = new()
                            {
                                SocioDebitoId = debito.SocioDebito.Id,
                                IndiceEconomicoMovotId = i.Id,
                                DataLancto = i.DataInicio,
                                TipoLanctoDebito = TipoLanctoDebito.Juros,
                                Valor = 0,
                                Historico = string.Empty,
                                Origem = OrigemLancto.SistemaJuros,
                                TenantId = debito.SocioDebito.TenantId,
                                GUID = Guid.NewGuid().ToString(),
                                EmpreendimentoSocioId = debito.SocioDebito.EmpreendimentoSocioId,
                                DataCriacao = Constante.Today
                            };
                            if (mmaa != string.Empty)
                            {
                                fator = 1;
                            }
                            qtdedias = li.Where(x => x.DataInicio >= UtilsClass.GetInicio(i.DataInicio) && x.DataInicio <= UtilsClass.GetUltimo(i.DataInicio)).Count();
                            if (debito.SocioDebito.Taxa != 0)
                            {
                                taxadiaria = Math.Pow((debito.SocioDebito.Taxa / 100 + 1), 1 / (qtdedias * 1.0)) - 1.0;
                            }
                            else
                            {
                                taxadiaria = 0;
                            }
                            mmaa = i.DataInicio.ToString("MMyyyy");
                            valorinicialmes = debito.GetValorSaldoByData(i.DataInicio);
                            amortizacao = -1;
                            indiceacu = 1;
                            debito.Lanctos.Add(sdl);
                        };
                        if (amortizacao == -1)
                        {
                            amortizacao = 0;
                        }
                        else
                        {
                            amortizacao = debito.Lanctos.Where(x => x.TipoLanctoDebito == TipoLanctoDebito.Amortizacao && x.DataLancto > debito.SocioDebito.DataUltimaCorrecao && x.DataLancto <= i.DataInicio).Sum(x => x.Valor);
                        }
                        indiceacu += indiceacu * i.Valor / 100;
                        valorant = valorinicialmes - amortizacao;
                        if (valorinicialmes < 0.01)
                        {
                            valorinicialmes = 0;
                            valorant = 0;
                        }
                        valorinicialmes += (valorinicialmes - amortizacao) * (i.Valor / 100 + taxadiaria);
                        sdl.Valor += valorinicialmes - valorant;
                        if (debito.SocioDebito.Taxa != 0)
                        {
                            sdl.Historico = "Correção monetária referente empréstimo/adiantamento registrado em " + debito.SocioDebito.DataLancto.ToString("dd/MM/yyyy") +
                                            " , indice: " + ((indiceacu - 1) * 100).ToString("N6", Constante.Culture) + "% juros mensal: " + (debito.SocioDebito.Taxa * fator).ToString("N5", Constante.Culture);
                        }
                        else
                        {
                            sdl.Historico = "Correção monetária referente empréstimo/adiantamento registrado em " + debito.SocioDebito.DataLancto.ToString("dd/MM/yyyy") +
                                            " , indice: " + ((indiceacu - 1) * 100).ToString("N6", Constante.Culture) + "% ";
                        }
                        sdl.DataLancto = i.DataInicio;
                        debito.SocioDebito.DataUltimaCorrecao = i.DataInicio;
                    }
                }
            }
            return debito;
        }
        protected void AjustarDistribuicao(SocioResultadoExtrato so)
        {
            so.Distribuicoes = so.Distribuicoes.OrderBy(x => x.DataDeposito).ToList();
            so.SocioResultadoExtratoPeriodos = so.SocioResultadoExtratoPeriodos.OrderBy(x => x.DataRef).ToList();
            foreach (var r1 in so.SocioResultadoExtratoPeriodos)
            {
                r1.Lanctos.RemoveAll(x => x.TipoLancto == TipoLanctoSocioResultado.Pagamento);
                r1.Lanctos = r1.Lanctos.OrderBy(x => x.DataRef).ToList();
            }
            foreach (var dis in so.Distribuicoes)
            {
                double valor = dis.Valor;
                foreach (var resul in so.SocioResultadoExtratoPeriodos)
                {
                    double saldo = Math.Round(resul.GetValorByDate(dis.DataDeposito), 2);
                    if (valor < saldo)
                    {
                        SocioResultadoPeriodoLancto namor = new()
                        {
                            DataRef = dis.DataDeposito,
                            PeriodoId = dis.PeriodoId,
                            SocioResultadoPeriodoId = resul.SocioResultadoPeriodoId,
                            TipoLancto = TipoLanctoSocioResultado.Pagamento,
                            Valor = valor
                        };
                        resul.Lanctos.Add(namor);
                        valor = 0;
                        break;
                    }
                    else
                    {
                        if (saldo > 0)
                        {
                            SocioResultadoPeriodoLancto namor = new()
                            {
                                DataRef = dis.DataDeposito,
                                PeriodoId = dis.PeriodoId,
                                SocioResultadoPeriodoId = resul.SocioResultadoPeriodoId,
                                TipoLancto = TipoLanctoSocioResultado.Pagamento,
                                Valor = saldo
                            };
                            valor -= saldo;
                            resul.Lanctos.Add(namor);
                        }
                    }
                }
            }
            foreach (var r1 in so.SocioResultadoExtratoPeriodos)
            {
                r1.Lanctos = r1.Lanctos.OrderBy(x => x.DataRef).ToList();
            }

        }
        private async Task<List<SocioResultadoExtrato>> GerarListaSocioResultadoExtrato(List<SocioResultado> sr)
        {

            List<SocioResultadoExtrato> lse = new();

            foreach (var rr in sr)
            {
                var so = lse.FirstOrDefault(x => x.EmpreendimentoSocio.Id == rr.EmpreendimentoSocio.Id);
                if (so == null)
                {
                    so = new()
                    {
                        EmpreendimentoSocio = rr.EmpreendimentoSocio,
                        SocioResultadoExtratoPeriodos = new()
                    };
                    lse.Add(so);
                    var lvdis = await (from SocioRetirada in _context.SocioRetirada
                                       join SocioRetiradaLancto in _context.SocioRetiradaLancto on SocioRetirada.Id equals SocioRetiradaLancto.SocioRetiradaId
                                       select new
                                       {
                                           SocioRetirada.EmpreendimentoSocioId,
                                           SocioRetiradaLancto.DataDeposito,
                                           SocioRetiradaLancto.Valor
                                       }
                                       ).Where(x => x.EmpreendimentoSocioId == so.EmpreendimentoSocio.Id).ToListAsync();
                    foreach (var vdis in lvdis)
                    {
                        SocioRetiradaLancto nvd = new()
                        {
                            EmpreendimentoSocioId = vdis.EmpreendimentoSocioId,
                            DataDeposito = vdis.DataDeposito,
                            Valor = vdis.Valor
                        };
                        so.Distribuicoes.Add(nvd);
                    }
                    var lvamor = await (from SocioRetirada in _context.SocioRetirada
                                        join SocioRetiradaDebitoLancto in _context.SocioRetiradaDebitoLancto on SocioRetirada.Id equals SocioRetiradaDebitoLancto.SocioRetiradaId
                                        join SocioDebitoLancto in _context.SocioDebitoLancto on SocioRetiradaDebitoLancto.SocioDebitoLanctoId equals SocioDebitoLancto.Id
                                        select new
                                        {
                                            SocioRetirada.EmpreendimentoSocioId,
                                            SocioDebitoLancto.DataLancto,
                                            SocioDebitoLancto.Valor
                                        }
                                       ).Where(x => x.EmpreendimentoSocioId == so.EmpreendimentoSocio.Id).ToListAsync();
                    foreach (var vdis in lvamor)
                    {
                        SocioRetiradaLancto nvd = new()
                        {
                            EmpreendimentoSocioId = vdis.EmpreendimentoSocioId,
                            DataDeposito = vdis.DataLancto,
                            Valor = vdis.Valor
                        };
                        so.Distribuicoes.Add(nvd);
                    }
                }
                SocioResultadoExtratoPeriodo srp = new()
                {
                    SocioResultadoPeriodoId = rr.SocioResultadoPeriodo.Id,
                    DataRef = rr.SocioResultadoPeriodo.DataMovto,
                    PeriodoId = rr.SocioResultadoPeriodo.PeriodoId,
                    DataUltimaCorrecao = rr.SocioResultadoPeriodo.DataUltimaCorrecao,
                    Taxa = so.EmpreendimentoSocio.Taxa,
                    Valor = rr.SocioResultadoPeriodo.Valor
                };
                if (srp.DataUltimaCorrecao < srp.DataRef)
                {
                    srp.DataUltimaCorrecao = srp.DataRef;
                }
                var lcor = await _context.SocioCorrecaoResultadoRetida.AsNoTracking().Where(x => x.SocioResultadoPeriodoId == rr.SocioResultadoPeriodo.Id).ToListAsync();
                foreach (var cor in lcor)
                {
                    SocioResultadoPeriodoLancto ncor = new()
                    {
                        Id = cor.Id,
                        DataRef = cor.DataRef,
                        PeriodoId = rr.SocioResultadoPeriodo.PeriodoId,
                        SocioResultadoPeriodoId = cor.SocioResultadoPeriodoId,
                        Valor = cor.Valor,
                        TipoLancto = TipoLanctoSocioResultado.Correcao,
                        Origem = OrigemLancto.SistemaJuros,
                        Historico = cor.Historico
                    };
                    srp.Lanctos.Add(ncor);
                }
                so.SocioResultadoExtratoPeriodos.Add(srp);
            }

            if (lse.Count > 0)
            {
                foreach (var so in lse)
                {
                    AjustarDistribuicao(so);
                }
            }
            return lse;

        }
        public async Task<Resultado> AplicarCorrecao(int indiceid, DateTime data)
        {
            Resultado r = new();

            List<SocioResultadoExtrato> lse = new();

            List<SocioResultado> lrr = await (from SocioResultadoPeriodo in _context.SocioResultadoPeriodo
                                              join EmpreendimentoSocio in _context.EmpreendimentoSocio on SocioResultadoPeriodo.EmpreendimentoSocioId equals EmpreendimentoSocio.Id
                                              select new SocioResultado()
                                              {
                                                  EmpreendimentoSocio = EmpreendimentoSocio,
                                                  SocioResultadoPeriodo = SocioResultadoPeriodo
                                              }
                          ).AsNoTracking().Where(x => x.EmpreendimentoSocio.AplicarCorrecaoDistRetida == true
                                                   && x.EmpreendimentoSocio.IndiceEconomicoId == indiceid
                                                   && x.EmpreendimentoSocio.CorrecaoAutomatica == true
                                                   ).OrderBy(x => x.EmpreendimentoSocio.Id).ThenBy(x => x.SocioResultadoPeriodo.DataMovto).ToListAsync();

            lse = await GerarListaSocioResultadoExtrato(lrr);

            var ld = await (from SocioDebito in _context.SocioDebito
                            join SocioDebitoLancto in _context.SocioDebitoLancto on SocioDebito.Id equals SocioDebitoLancto.SocioDebitoId into dl
                            from dl1 in dl.DefaultIfEmpty()
                            select new
                            {
                                SocioDebito,
                                SocioDebitoLancto = dl1
                            }).Where(x => x.SocioDebito.IndiceEconomicoId == indiceid && x.SocioDebito.DataUltimaCorrecao < UtilsClass.GetUltimo(data).AddDays(-35) && x.SocioDebito.Quitado == false && x.SocioDebito.CorrecaoAutomatica == true).ToListAsync();
            if (ld.Count == 0 && lse.Count == 0)
            {
                r.Ok = false;
                r.ErrMsg = "Todos os empréstimos/adiantamentos estão atualizados.";
                return r;
            }
            List<SocioDebitoView> lsd = new();
            foreach (var i in ld)
            {
                SocioDebitoView d = lsd.FirstOrDefault(x => x.SocioDebito.Id == i.SocioDebito.Id);
                if (d == null)
                {
                    d = new()
                    {
                        SocioDebito = i.SocioDebito,
                        Lanctos = new()
                    };
                    if (d.SocioDebito.DataUltimaCorrecao < d.SocioDebito.DataInicioCorrecao)
                    {
                        d.SocioDebito.DataUltimaCorrecao = d.SocioDebito.DataInicioCorrecao;
                    }
                    lsd.Add(d);
                }
                if (i.SocioDebitoLancto != null)
                {
                    d.Lanctos.Add(i.SocioDebitoLancto);
                }
            }
            ld.Clear();
            ld = null;
            DateTime dtini = DateTime.MaxValue;
            if (lsd.Count > 0)
            {
                dtini = lsd.Min(x => x.SocioDebito.DataUltimaCorrecao);
            }
            foreach (var so in lse)
            {
                if (so.SocioResultadoExtratoPeriodos.Count > 0)
                {
                    DateTime drmin = so.SocioResultadoExtratoPeriodos.Min(x => x.DataUltimaCorrecao);
                    if (drmin < dtini)
                    {
                        dtini = drmin;
                    }
                }
            }

            var indice = await _context.IndiceEconomico.FirstOrDefaultAsync(x => x.Id == indiceid);
            List<IndiceEconomicoMovto> li = null;
            if (indice.TipoPeriodoIndice == TipoPeriodoIndice.Mensal)
            {
                li = await _context.IndiceEconomicoMovto.Where(x => x.IndiceEconomicoId == indiceid && x.DataInicio > dtini && x.DataFim <= data).OrderBy(x => x.DataInicio).ToListAsync();
            }
            else
            {
                if (indice.TipoPeriodoIndice == TipoPeriodoIndice.Diario)
                {
                    li = await _context.IndiceEconomicoMovto.Where(x => x.IndiceEconomicoId == indiceid && x.DataInicio >= dtini && x.DataInicio < data).OrderBy(x => x.DataInicio).ToListAsync();
                }
            }
            try
            {
                await _context.Database.BeginTransactionAsync();
                foreach (var i in lsd)
                {
                    _context.SocioDebitoLancto.RemoveRange(i.Lanctos.Where(x => x.DataLancto > i.DataUltimaCorrecao && x.TipoLanctoDebito == TipoLanctoDebito.Juros && x.Origem == OrigemLancto.SistemaJuros).ToList());
                    i.Lanctos.RemoveAll(x => x.DataLancto > i.DataUltimaCorrecao && x.TipoLanctoDebito == TipoLanctoDebito.Juros && x.Origem == OrigemLancto.SistemaJuros);
                    CalcularCorrecaoDebito(indice, li, i, data);
                    _context.SocioDebito.Update(i.SocioDebito);
                    double jt1 = i.Lanctos.Where(x => x.Id == 0 && x.Valor > 0.01).Sum(x => x.Valor);
                    i.Lanctos.Where(x => x.Id == 0 && x.Valor > 0.01).ToList().ForEach(x => x.Valor = Math.Round(x.Valor, 2));
                    double jt2 = i.Lanctos.Where(x => x.Id == 0 && x.Valor > 0.01).Sum(x => x.Valor);
                    jt1 -= jt2;
                    if (jt1 != 0)
                    {
                        int ik = i.Lanctos.Count - 1;
                        i.Lanctos[ik].Valor = Math.Round(i.Lanctos[ik].Valor + jt1, 2);
                    }
                    await _context.SocioDebitoLancto.AddRangeAsync(i.Lanctos.Where(x => x.Id == 0 && x.Valor > 0.01));
                }

                foreach (var so in lse)
                {
                    foreach (var sor in so.SocioResultadoExtratoPeriodos.Where(x => x.Valor > 0).ToList())
                    {
                        var lrl = await _context.SocioCorrecaoResultadoRetida.Where(x => x.DataRef > sor.DataUltimaCorrecao && x.Origem == OrigemLancto.SistemaJuros
                                                                                      && x.SocioResultadoPeriodoId == sor.SocioResultadoPeriodoId).ToListAsync();
                        _context.SocioCorrecaoResultadoRetida.RemoveRange(lrl);
                        sor.Lanctos.RemoveAll(x => x.DataRef > sor.DataUltimaCorrecao && x.TipoLancto == TipoLanctoSocioResultado.Correcao && x.Origem == OrigemLancto.SistemaJuros);
                        if (so.SocioResultadoExtratoPeriodos.Any(x => x.DataRef < sor.DataRef))
                        {
                            sor.PrejuizoAcumulado = so.SocioResultadoExtratoPeriodos.Where(x => x.DataRef < sor.DataRef).Sum(x => x.Valor);
                            if (sor.PrejuizoAcumulado > 0)
                            {
                                sor.PrejuizoAcumulado = 0;
                            }
                            sor.PrejuizoAcumulado = Math.Abs(sor.PrejuizoAcumulado);
                        }
                        CalcularCorrecaoDistribuicao(indice, li, sor, so, data);
                        var resultado = await _context.SocioResultadoPeriodo.FirstOrDefaultAsync(x => x.Id == sor.SocioResultadoPeriodoId);
                        if (resultado != null)
                        {
                            resultado.DataUltimaCorrecao = sor.DataUltimaCorrecao;
                            _context.SocioResultadoPeriodo.Update(resultado);
                        }
                        double jt1 = sor.Lanctos.Where(x => x.Id == 0 && x.Valor > 0.01).Sum(x => x.Valor);
                        sor.Lanctos.Where(x => x.Id == 0 && x.Valor > 0.01).ToList().ForEach(x => x.Valor = Math.Round(x.Valor, 2));
                        double jt2 = sor.Lanctos.Where(x => x.Id == 0 && x.Valor > 0.01).Sum(x => x.Valor);
                        jt1 -= jt2;
                        if (jt1 != 0)
                        {
                            int ik = sor.Lanctos.Count - 1;
                            sor.Lanctos[ik].Valor = Math.Round(sor.Lanctos[ik].Valor + jt1, 2);
                        }
                        foreach (var n in sor.Lanctos.Where(x => x.Id == 0 && x.Valor > 0.01 && x.TipoLancto == TipoLanctoSocioResultado.Correcao))
                        {
                            SocioCorrecaoResultadoRetida nl = new()
                            {
                                DataRef = n.DataRef,
                                Historico = n.Historico,
                                Origem = OrigemLancto.SistemaJuros,
                                SocioResultadoPeriodoId = sor.SocioResultadoPeriodoId,
                                TenantId = so.EmpreendimentoSocio.TenantId,
                                Valor = n.Valor
                            };
                            await _context.SocioCorrecaoResultadoRetida.AddAsync(nl);
                        }
                    }
                }

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
        public async Task<Resultado> ExcluirCorrecao(SocioDebitoView sd)
        {
            Resultado r = new();
            try
            {
                await _context.Database.BeginTransactionAsync();
                sd.SocioDebito.DataUltimaCorrecao = sd.SocioDebito.DataInicioCorrecao;
                _context.SocioDebito.Update(sd.SocioDebito);
                _context.SocioDebitoLancto.RemoveRange(sd.Lanctos.Where(x => x.Origem == OrigemLancto.SistemaJuros));
                await _context.SaveChangesAsync();
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
        public async Task<Resultado> ExcluirCorrecaoDistribuicao(int id)
        {
            Resultado r = new();
            SessionInfo si = await GetSessionAsync();
            try
            {
                var r1 = await _context.SocioResultadoPeriodo.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
                if (r1 == null)
                {
                    r.Ok = false;
                    r.ErrMsg = "Registro não encontrado";
                    return r;
                }
                await _context.Database.BeginTransactionAsync();
                r1.DataUltimaCorrecao = DateTime.MinValue;
                _context.SocioResultadoPeriodo.Update(r1);
                await _context.Database.ExecuteSqlRawAsync(" Delete SocioCorrecaoResultadoRetida where TenantId = '" + si.TenantId + "'  and SocioResultadoPeriodoId = " + r1.Id.ToString());
                await _context.SaveChangesAsync();
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
        public async Task<Resultado> AplicarCorrecaoDistribuicao(int id)
        {
            Resultado r = new();
            SessionInfo si = await GetSessionAsync();

            var r1 = await _context.SocioResultadoPeriodo.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
            if (r1 == null)
            {
                r.Ok = false;
                r.ErrMsg = "Resultado não encontrado";
                return r;
            }
            var so = await _context.EmpreendimentoSocio.AsNoTracking().FirstOrDefaultAsync(x => x.Id == r1.EmpreendimentoSocioId && x.TenantId == si.TenantId);
            if (so == null)
            {
                r.Ok = false;
                r.ErrMsg = "Sócio não encontrado";
                return r;
            }
            r = await AplicarCorrecao(so.IndiceEconomicoId, Constante.Today);

            return r;

        }
        public async Task<List<SocioResultadoExtrato>> GetResultadoSocioByDate(int empid, DateTime di, DateTime df)
        {
            SessionInfo si = await GetSessionAsync();

            List<SocioResultado> lrr = await (from SocioResultadoPeriodo in _context.SocioResultadoPeriodo
                                              join EmpreendimentoSocio in _context.EmpreendimentoSocio on SocioResultadoPeriodo.EmpreendimentoSocioId equals EmpreendimentoSocio.Id
                                              select new SocioResultado()
                                              {
                                                  EmpreendimentoSocio = EmpreendimentoSocio,
                                                  SocioResultadoPeriodo = SocioResultadoPeriodo
                                              }
                          ).AsNoTracking().Where(x => x.EmpreendimentoSocio.EmpreendimentoId == empid
                                                   && x.SocioResultadoPeriodo.DataMovto <= df
                                                   && x.SocioResultadoPeriodo.TenantId == si.TenantId
                                                   ).OrderBy(x => x.EmpreendimentoSocio.Nome).ThenBy(x => x.SocioResultadoPeriodo.DataMovto).ToListAsync();

            var r = await GerarListaSocioResultadoExtrato(lrr);
            foreach (var i in r)
            {
                i.SocioResultadoExtratoPeriodos.RemoveAll(x => x.DataRef < di);
            }
            return r;
        }
        public async Task<List<IndiceEconomico>> FindAllIndices()
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.IndiceEconomico.Where(x => x.OrigemIndice == OrigemIndice.Geral || (x.OrigemIndice == OrigemIndice.Especifico && x.TenantId == si.TenantId)).OrderBy(x => x.Nome).ToListAsync();
        }
        public async Task<Resultado> AddIndiceEconomicoMovto(IndiceEconomicoMovto i)
        {
            Resultado r = new();
            if (await _context.IndiceEconomicoMovto.AnyAsync(x => x.IndiceEconomicoId == i.IndiceEconomicoId && x.DataInicio >= i.DataInicio && x.DataInicio <= i.DataFim))
            {
                r.Ok = false;
                r.ErrMsg = "Já existe um registro para o período informado";
                return r;
            }
            try
            {
                i.DataFim = UtilsClass.GetUltimo(i.DataInicio);
                await _context.IndiceEconomicoMovto.AddAsync(i);
                await _context.SaveChangesAsync();
                r.Ok = true;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> DeleteIndiceEconomicoMovto(IndiceEconomicoMovto i)
        {
            Resultado r = new();
            if (await _context.SocioDebitoLancto.AnyAsync(x => x.IndiceEconomicoMovotId == i.Id))
            {
                r.Ok = false;
                r.ErrMsg = "Indice já utlizado para na correção de débitos, não pode ser excluído";
            }

            try
            {
                _context.IndiceEconomicoMovto.Remove(i);
                await _context.SaveChangesAsync();
                r.Ok = true;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<List<IndiceEconomicoMovto>> FindIndiceEconomicoMovto(int id, DateTime dtini, DateTime dtfim)
        {
            return await _context.IndiceEconomicoMovto.Where(x => x.IndiceEconomicoId == id && x.DataInicio >= dtini && x.DataInicio <= dtfim).OrderBy(x => x.DataInicio).ToListAsync();
        }
        public async Task<Resultado> IncluirIndiceEconomicoAsync(List<IndiceEconomicoMovto> lm)
        {
            Resultado r = new();

            if (lm.Count == 0)
            {
                r.Ok = false;
                r.ErrMsg = "Lista de valores de indices vazia";
                return r;
            }

            DateTime d1 = lm.Min(x => x.DataInicio);
            DateTime d2 = lm.Max(x => x.DataInicio);

            await _context.Database.BeginTransactionAsync();
            try
            {
                var lmd = await _context.IndiceEconomicoMovto.Where(x => x.IndiceEconomicoId == lm[0].IndiceEconomicoId && x.DataInicio >= d1 && x.DataInicio <= d2).ToListAsync();
                if (lmd.Count > 0)
                {
                    _context.IndiceEconomicoMovto.RemoveRange(lmd);
                }
                await _context.IndiceEconomicoMovto.AddRangeAsync(lm);
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
        public LanctoEmpreendimento ConvertMovtoBancoToLanctoEmp(MovtoBancoView l, int periodoid)
        {
            TipoLancto tl = TipoLancto.Despesa;
            if (l.TipoMovtoBanco == TipoMovtoBanco.Credito)
            {
                tl = TipoLancto.Receita;
            }
            LanctoEmpreendimento r = new()
            {
                CodigoExterno = l.CodigoExterno,
                DataCompetencia = l.DataMovto,
                DataMovto = l.DataMovto,
                Descricao = l.Historico,
                Documento = l.Documento,
                Erro = false,
                ErroMsg = string.Empty,
                Origem = OrigemLancto.ImportadoCC,
                PeriodoId = periodoid,
                PlanoContaId = l.PlanoContaId,
                Rateado = false,
                RelacionamentoId = l.RelacionamentoId,
                Tipo = tl,
                Valor = l.Valor,
                CodigoUnidade = string.Empty,
                OrigemId = l.Id
            };
            if (r.LanctoEmpRelacionamento == null)
            {
                if (l.RelacionamentoId != 0)
                {
                    r.LanctoEmpRelacionamento.RelacionamentoId = l.RelacionamentoId;
                }
            }
            return r;
        }
        public async Task<Resultado> AtualizaIndiceBC()
        {
            Resultado r = new();
            BCService bcservice = new();
            DateTime dtini = UtilsClass.GetInicio(UtilsClass.GetUltimo(Constante.Today).AddDays(-35));
            DateTime dtfim = UtilsClass.GetUltimo(UtilsClass.GetUltimo(Constante.Today).AddDays(-35));
            var li = await FindAllIndices();
            List<IndiceEconomicoMovto> novos = new();
            foreach (var id in li)
            {
                if (id.TipoIndice == TipoIndice.Especifico || id.TipoIndice == TipoIndice.Fixo || id.TipoIndice == TipoIndice.SemCorrecao)
                    continue;
                if (id.TipoPeriodoIndice == TipoPeriodoIndice.Mensal)
                {
                    DateTime dmx = await _context.IndiceEconomicoMovto.Where(x => x.IndiceEconomicoId == id.Id).MaxAsync(x => x.DataFim);
                    if (dmx >= dtini)
                    {
                        continue;
                    }
                    dtini = UtilsClass.GetUltimo(dmx).AddDays(1);
                    dtfim = UtilsClass.GetUltimo(UtilsClass.GetUltimo(Constante.Today).AddDays(-35));
                    r = bcservice.IndicePorData(id.TipoIndice, id.TipoPeriodoIndice, dtini, dtfim);
                    List<IndiceEconomicoMovto> lim = r.Item as List<IndiceEconomicoMovto>;
                    foreach (var mim in lim)
                    {
                        mim.IndiceEconomicoId = id.Id;
                        novos.Add(mim);
                    }
                }
                else
                {
                    DateTime dmx = await _context.IndiceEconomicoMovto.Where(x => x.IndiceEconomicoId == id.Id).MaxAsync(x => x.DataFim);
                    if (dmx > Constante.Today.AddDays(-2))
                    {
                        continue;
                    }
                    dtini = dmx.AddDays(1);
                    dtfim = Constante.Today.AddDays(-1);
                    r = bcservice.IndicePorData(id.TipoIndice, id.TipoPeriodoIndice, dtini, dtfim);
                    List<IndiceEconomicoMovto> lim = r.Item as List<IndiceEconomicoMovto>;
                    foreach (var mim in lim)
                    {
                        mim.IndiceEconomicoId = id.Id;
                        novos.Add(mim);
                    }
                }
            }
            await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.IndiceEconomicoMovto.AddRangeAsync(novos);
                await _context.SaveChangesAsync();
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
        public async Task<List<TituloBaseView>> FindAllTitulosByMovtoBanco(MovtoBancoView m, DateTime dtini, DateTime dtfim, bool filtrarPorValor = true)
        {
            SessionInfo si = await GetSessionAsync();
            List<TituloBaseView> r = new();

            if (dtini == DateTime.MinValue)
            {
                dtini = m.DataMovto.AddDays(-5);
                dtfim = m.DataMovto.AddDays(5);
            }
            int companyid;
            if (si.TenantApp == TenantApp.Studio)
            {
                var studio = await _context.ContaCorrenteStudio.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.ContaCorrenteId == m.ContaCorrenteId);
                if (studio != null)
                {
                    companyid = studio.StudioId;
                }
                else
                {
                    throw new Exception("ContaCorrenteStudio não encontrado");
                }
            }
            else
            {
                if (si.TenantApp == TenantApp.InCorp)
                {
                    var emp = await _context.ContaCorrenteEmpreendimento.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.ContaCorrenteId == m.ContaCorrenteId);
                    if (emp != null)
                    {
                        companyid = emp.Id;
                    }
                    else
                    {
                        throw new Exception("ContaCorrenteEmpreendimento não encontrado");
                    }
                }
                else
                {
                    throw new Exception("não implementado");
                }
            }
            List<StatusParcela> lsp = new() { StatusParcela.Aberto, StatusParcela.Agendado, StatusParcela.Pago, StatusParcela.ConciliadoParcial };
            StudioOperacoesService stdope = new(_context, _sessionStorageService, _sessionService, _mapper, null, null);
            StudioConfig scfg = await _context.StudioConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
            if (si.TenantApp == TenantApp.Studio && m.TipoMovtoBanco == TipoMovtoBanco.Credito)
            {
                var lp = await stdope.FindAllParcelasView(dtini, dtfim, 0, lsp, false);
                scfg = await _context.StudioConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
                PlanoConta pc = await _context.PlanoConta.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == scfg.PlanoContaIdAluno);
                if (filtrarPorValor)
                {
                    lp = lp.Where(x => x.Valor >= m.Valor * 0.95 && x.Valor <= m.Valor * 1.05).ToList();
                }
                foreach (var p in lp)
                {
                    TituloBaseView tv = new()
                    {
                        DataVencto = p.DataVencto,
                        DataPagto = p.DataPagto,
                        Documento = p.Descricao,
                        Historico = p.Descricao,
                        RelacionamentoId = p.RelacionamentoId,
                        Id = p.Id,
                        Status = p.Status,
                        TipoTitulo = TipoTitulo.ContasReceber,
                        RelacionamentoNome = p.NomeAluno,
                        PlanoContaId = scfg.PlanoContaIdAluno
                    };
                    if (p.ProgramacaoAula != null)
                    {
                        tv.Classe = ClasseTituloBase.ProgramacaoAula;
                    }
                    else
                    {
                        if (p.AlunoPlanoParcela != null)
                        {
                            tv.Classe = ClasseTituloBase.ParcelaStudio;
                        }
                        else
                        {
                            throw new Exception("Classe não implementada");
                        }
                    }
                    tv.Valor = p.Valor;
                    if (pc != null)
                    {
                        tv.PlanoContaId = pc.Id;
                        tv.PlanoContaNome = pc.Nome;
                    }
                    r.Add(tv);
                }
            }
            if (si.TenantApp == TenantApp.Studio && m.TipoMovtoBanco == TipoMovtoBanco.Debito)
            {
                var lotesprof = await stdope.FindProfessorLotePagtoViewByDate(m.DataMovto.AddDays(-90), Constante.Today, 0);
                lotesprof = lotesprof.Where(x => x.Status != StatusProfessorLotePagto.Conciliado).ToList();
                PlanoConta pc = await _context.PlanoConta.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == scfg.PlanoContaIdProfessor);
                foreach (var pp in lotesprof)
                {
                    TituloBaseView tv = new()
                    {
                        Classe = ClasseTituloBase.LotePagtoProfessor,
                        DataVencto = pp.DataPagto,
                        DataPagto = pp.DataPagto,
                        Documento = "lote " + pp.Id.ToString(),
                        Historico = "ref: " + pp.DataInicio.ToString("dd/MM/yyyy") + " a " + pp.DataFim.ToString("dd/MM/yyyy"),
                        RelacionamentoId = pp.RelacionamentoId,
                        Id = pp.Id,
                        Valor = pp.ValorTotalPagar,
                        TipoTitulo = TipoTitulo.ContasPagar,
                        RelacionamentoNome = pp.NomeProfessor
                    };
                    if (pc != null)
                    {
                        tv.PlanoContaId = pc.Id;
                        tv.PlanoContaNome = pc.Nome;
                    }
                    tv.Status = Constante.ConvertStatusLoteProfessor(pp.Status);
                    r.Add(tv);
                }
            }

            TituloService titservice = new(_context, _sessionStorageService, _sessionService, _mapper);
            TituloFilter tituloFilter = new()
            {
                CompanyId = companyid,
                DtIni = dtini,
                DtFim = dtfim,
                StatusParcelas = lsp,
                TipoTitulo = TipoTitulo.ContasPagar
            };
            if (m.TipoMovtoBanco == TipoMovtoBanco.Credito)
            {
                tituloFilter.TipoTitulo = TipoTitulo.ContasReceber;
            }
            List<TituloView> lt = await titservice.GetTitulosReceber(tituloFilter);
            if (filtrarPorValor)
            {
                lt = lt.Where(x => x.Valor >= m.Valor * 0.95 && x.Valor <= m.Valor * 1.05).ToList();
            }
            foreach (var t in lt)
            {
                TituloBaseView tv = new()
                {
                    DataVencto = t.DataVencto,
                    DataPagto = t.DataPagto,
                    Documento = t.Descricao,
                    Historico = t.Descricao,
                    RelacionamentoId = t.RelacionamentoId,
                    Id = t.ParcelaId,
                    Status = t.Status,
                    TipoTitulo = TipoTitulo.ContasReceber,
                    RelacionamentoNome = t.RelacionamentoV,
                    PlanoContaId = t.PlanoContaId,
                    PlanoContaNome = t.PlanoContaV,
                    Classe = ClasseTituloBase.TituloReceber,
                    Valor = t.Valor
                };
                if (m.TipoMovtoBanco == TipoMovtoBanco.Credito)
                {
                    tv.Classe = ClasseTituloBase.TituloReceber;
                }
                else
                {
                    tv.Classe = ClasseTituloBase.TituloPagar;
                }
                r.Add(tv);
            }
            return r;
        }
        public async Task<List<TituloBaseView>> GetTitulosVinculadosByMovtoBancoId(int id)
        {
            SessionInfo si = await GetSessionAsync();
            List<TituloBaseView> r = new();

            var ls = await _context.MovtoBancoStudioParcela.Where(x => x.TenantId == si.TenantId && x.MovtoBancoId == id).ToListAsync();
            foreach (var s in ls)
            {
                if (s.OrigemParcela == OrigemStudioParcela.LotePagtoProfessor)
                {
                    StudioOperacoesService stdope = new(_context, _sessionStorageService, _sessionService, _mapper, null, null);
                    ProfessorLotePagtoView pp = await stdope.FindProfessorLoteById(s.StudioParcelaId);
                    StudioConfig scfg = await _context.StudioConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
                    PlanoConta pc = await _context.PlanoConta.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == scfg.PlanoContaIdProfessor);
                    if (pp != null)
                    {
                        TituloBaseView tv = new()
                        {
                            Classe = ClasseTituloBase.LotePagtoProfessor,
                            DataVencto = pp.DataPagto,
                            DataPagto = pp.DataPagto,
                            Documento = "lote " + pp.Id.ToString(),
                            Historico = "ref: " + pp.DataInicio.ToString("dd/MM/yyyy") + " a " + pp.DataFim.ToString("dd/MM/yyyy"),
                            RelacionamentoId = pp.RelacionamentoId,
                            Id = pp.Id,
                            Valor = pp.ValorTotalPagar,
                            ValorConciliado = s.Valor,
                            TipoTitulo = TipoTitulo.ContasPagar,
                            RelacionamentoNome = pp.NomeProfessor
                        };
                        if (pc != null)
                        {
                            tv.PlanoContaId = pc.Id;
                            tv.PlanoContaNome = pc.Nome;
                        }
                        tv.Status = Constante.ConvertStatusLoteProfessor(pp.Status);
                        r.Add(tv);
                    }
                    else
                    {
                        _context.MovtoBancoStudioParcela.Remove(s);
                        await _context.SaveChangesAsync();
                    }

                }
                if (s.OrigemParcela == OrigemStudioParcela.ProgramacaoAula)
                {
                    StudioConfig scfg = await _context.StudioConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
                    StudioOperacoesService stdope = new(_context, _sessionStorageService, _sessionService, _mapper, null, null);
                    scfg = await _context.StudioConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
                    PlanoConta pc = await _context.PlanoConta.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == scfg.PlanoContaIdAluno);
                    var p = await stdope.FindParcelasViewById(s.StudioParcelaId, OrigemStudioParcela.ProgramacaoAula);
                    if (p == null)
                    {
                        _context.MovtoBancoStudioParcela.Remove(s);
                        await _context.SaveChangesAsync();
                        continue;
                    }
                    TituloBaseView tv = new()
                    {
                        DataVencto = p.DataVencto,
                        DataPagto = p.DataPagto,
                        Documento = p.Descricao,
                        Historico = p.Descricao,
                        RelacionamentoId = p.RelacionamentoId,
                        Id = p.Id,
                        Status = p.Status,
                        TipoTitulo = TipoTitulo.ContasReceber,
                        RelacionamentoNome = p.NomeAluno,
                        ValorConciliado = s.Valor,
                        PlanoContaId = scfg.PlanoContaIdAluno
                    };
                    if (p.ProgramacaoAula != null)
                    {
                        tv.Classe = ClasseTituloBase.ProgramacaoAula;
                    }
                    else
                    {
                        if (p.AlunoPlanoParcela != null)
                        {
                            tv.Classe = ClasseTituloBase.ParcelaStudio;
                        }
                        else
                        {
                            throw new Exception("Classe não implementada");
                        }
                    }
                    if (p.ValorPago > 0)
                    {
                        tv.Valor = p.ValorPago;
                    }
                    else
                    {
                        tv.Valor = p.Valor;
                    }
                    if (pc != null)
                    {
                        tv.PlanoContaId = pc.Id;
                        tv.PlanoContaNome = pc.Nome;
                    }
                    r.Add(tv);
                }
                if (s.OrigemParcela == OrigemStudioParcela.PlanoParcela)
                {
                    StudioConfig scfg = await _context.StudioConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
                    StudioOperacoesService stdope = new(_context, _sessionStorageService, _sessionService, _mapper, null, null);
                    scfg = await _context.StudioConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
                    PlanoConta pc = await _context.PlanoConta.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == scfg.PlanoContaIdAluno);
                    var p = await stdope.FindParcelasViewById(s.StudioParcelaId, OrigemStudioParcela.PlanoParcela);
                    if (p == null)
                    {
                        _context.MovtoBancoStudioParcela.Remove(s);
                        await _context.SaveChangesAsync();
                        continue;
                    }
                    TituloBaseView tv = new()
                    {
                        DataVencto = p.DataVencto,
                        DataPagto = p.DataPagto,
                        Documento = p.Descricao,
                        Historico = p.Descricao,
                        RelacionamentoId = p.RelacionamentoId,
                        Id = p.Id,
                        Status = p.Status,
                        TipoTitulo = TipoTitulo.ContasReceber,
                        RelacionamentoNome = p.NomeAluno,
                        ValorConciliado = s.Valor,
                        PlanoContaId = scfg.PlanoContaIdAluno
                    };
                    if (p.ProgramacaoAula != null)
                    {
                        tv.Classe = ClasseTituloBase.ProgramacaoAula;
                    }
                    else
                    {
                        if (p.AlunoPlanoParcela != null)
                        {
                            tv.Classe = ClasseTituloBase.ParcelaStudio;
                        }
                        else
                        {
                            throw new Exception("Classe não implementada");
                        }
                    }
                    if (p.ValorPago > 0)
                    {
                        tv.Valor = p.ValorPago;
                    }
                    else
                    {
                        tv.Valor = p.Valor;
                    }
                    if (pc != null)
                    {
                        tv.PlanoContaId = pc.Id;
                        tv.PlanoContaNome = pc.Nome;
                    }
                    r.Add(tv);
                }
            }

            var lt = await _context.MovtoBancoTitulo.Where(x => x.TenantId == si.TenantId && x.MovtoBancoId == id).ToListAsync();
            TituloService titservice = new(_context, _sessionStorageService, _sessionService, _mapper);
            foreach (var t in lt)
            {
                var tp = await titservice.GetTituloViewByTituloParcelaId(t.TituloParcelaId);
                if (tp == null)
                {
                    _context.MovtoBancoTitulo.Remove(t);
                    await _context.SaveChangesAsync();
                    continue;
                }
                TituloBaseView tv = new()
                {
                    DataVencto = tp.DataVencto,
                    DataPagto = tp.DataPagto,
                    Documento = tp.Descricao,
                    Historico = tp.Descricao,
                    RelacionamentoId = tp.RelacionamentoId,
                    Id = tp.ParcelaId,
                    Status = tp.Status,
                    TipoTitulo = TipoTitulo.ContasPagar,
                    RelacionamentoNome = tp.RelacionamentoV,
                    PlanoContaId = tp.PlanoContaId,
                    ValorConciliado = t.Valor,
                    PlanoContaNome = tp.PlanoContaV
                };
                if (tp.Titulo.TipoTitulo == TipoTitulo.ContasReceber)
                {
                    tv.Classe = ClasseTituloBase.TituloReceber;
                }
                else
                {
                    tv.Classe = ClasseTituloBase.TituloPagar;
                }
                if (tp.ValorPago > 0)
                {
                    tv.Valor = tp.ValorPago;
                }
                else
                {
                    tv.Valor = t.Valor;
                }
                r.Add(tv);
            }

            return r;
        }
        public async Task<Resultado> VincularMovtoParcela(int movtoid, ClasseTituloBase c, int parcelaid, double valor, double valormovto)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            var m = await _context.MovtoBanco.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == movtoid);
            if (m == null)
            {
                r.Ok = false;
                r.ErrMsg = "Movimento bancário não encontrado";
                return r;
            }

            if (c == ClasseTituloBase.TituloPagar || c == ClasseTituloBase.TituloReceber)
            {
                var t = await _context.TituloParcela.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == parcelaid);
                if (t == null)
                {
                    r.Ok = false;
                    r.ErrMsg = "Título não encontrado";
                    return r;
                }
                double valormovtoconciliado = Math.Round(await _context.MovtoBancoTitulo.Where(x => x.MovtoBancoId == movtoid && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                double valortitconciliado = Math.Round(await _context.MovtoBancoTitulo.Where(x => x.TituloParcelaId == parcelaid && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                valormovtoconciliado = valormovto - valormovtoconciliado;
                valortitconciliado = valor - valortitconciliado;
                double valcon = 0;
                if (valormovtoconciliado < valortitconciliado)
                {
                    valcon = valormovtoconciliado;
                }
                else
                {
                    valcon = valortitconciliado;
                }
                MovtoBancoTitulo mt = new()
                {
                    MovtoBancoId = movtoid,
                    TenantId = si.TenantId,
                    TituloParcelaId = parcelaid,
                    Valor = valcon
                };
                var titulo = await _context.Titulo.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == t.TituloId);
                await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.MovtoBancoTitulo.AddAsync(mt);
                    await _context.SaveChangesAsync();
                    double maxtc = Math.Round(await _context.MovtoBancoTitulo.Where(x => x.TituloParcelaId == parcelaid && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                    t.ValorPago = maxtc;
                    if (maxtc == t.Valor)
                    {
                        t.Status = StatusParcela.Conciliado;
                        t.DataPagto = m.DataMovto;
                    }
                    else
                    {
                        t.DataPagto = DateTime.MinValue;
                        t.Status = StatusParcela.ConciliadoParcial;
                    }
                    _context.TituloParcela.Update(t);

                    double maxv = Math.Round(await _context.MovtoBancoTitulo.Where(x => x.MovtoBancoId == movtoid && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                    if (Math.Round(maxv, 2) == Math.Round(m.Valor, 2))
                    {
                        m.StatusConciliacao = StatusConciliacao.Total;
                    }
                    else
                    {
                        m.StatusConciliacao = StatusConciliacao.Parcial;
                    }
                    m.RelacionamentoId = titulo.RelacionamentoId;
                    m.PlanoContaId = titulo.PlanoContaId;
                    _context.MovtoBanco.Update(m);
                    await _context.SaveChangesAsync();
                    await _context.Database.CommitTransactionAsync();
                    r.Ok = true;
                }
                catch (Exception e)
                {
                    await _context.Database.CommitTransactionAsync();
                    r.Ok = false;
                    r.ErrMsg = e.Message;
                    return r;
                }
            }


            if (c == ClasseTituloBase.LotePagtoProfessor)
            {
                var lp = await _context.ProfessorLotePagto.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == parcelaid);
                if (lp == null)
                {
                    r.Ok = false;
                    r.ErrMsg = "Lote professor não encontrado";
                    return r;
                }
                var prof = await _context.Professor.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == lp.ProfessorId);
                var rel = await _context.Relacionamento.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == prof.RelacionamentoId);
                double valconciliado = 0;
                if (await _context.MovtoBancoStudioParcela.AnyAsync(x => x.MovtoBancoId == m.Id && x.TenantId == si.TenantId))
                {
                    valconciliado = Math.Round(await _context.MovtoBancoStudioParcela.Where(x => x.MovtoBancoId == m.Id && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                }
                double valconciliar = m.Valor - valconciliado;
                MovtoBancoStudioParcela mp = new()
                {
                    MovtoBancoId = movtoid,
                    TenantId = si.TenantId,
                    StudioParcelaId = parcelaid,
                    OrigemParcela = OrigemStudioParcela.LotePagtoProfessor
                };
                double valorConciliadoLote = await _context.MovtoBancoStudioParcela.Where(x => x.StudioParcelaId == parcelaid && x.TenantId == si.TenantId && x.OrigemParcela == OrigemStudioParcela.LotePagtoProfessor).SumAsync(x => x.Valor);
                double valorConciliarLote = lp.ValorTotalPagar - valorConciliadoLote;
                if (Math.Round(valconciliar, 2) >= Math.Round(valorConciliarLote, 2))
                {
                    mp.Valor = valorConciliarLote;
                    lp.Status = StatusProfessorLotePagto.Conciliado;
                }
                else
                {
                    mp.Valor = valconciliar;
                    lp.Status = StatusProfessorLotePagto.ConciliadoParcial;
                }
                await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.MovtoBancoStudioParcela.AddAsync(mp);
                    if (lp.Status == StatusProfessorLotePagto.Conciliado)
                    {
                        lp.DataPagto = m.DataMovto;
                    }
                    else
                    {
                        lp.DataPagto = DateTime.MinValue;
                    }
                    _context.ProfessorLotePagto.Update(lp);
                    await _context.SaveChangesAsync();
                    double maxv = Math.Round(await _context.MovtoBancoStudioParcela.Where(x => x.MovtoBancoId == movtoid && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                    if (maxv == m.Valor)
                    {
                        m.StatusConciliacao = StatusConciliacao.Total;
                    }
                    else
                    {
                        m.StatusConciliacao = StatusConciliacao.Parcial;
                    }
                    m.RelacionamentoId = rel.Id;
                    StudioConfig scfg = await _context.StudioConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
                    m.PlanoContaId = scfg.PlanoContaIdProfessor;
                    _context.MovtoBanco.Update(m);
                    await _context.SaveChangesAsync();
                    await _context.Database.CommitTransactionAsync();
                    r.Ok = true;
                }
                catch (Exception e)
                {
                    await _context.Database.CommitTransactionAsync();
                    r.Ok = false;
                    r.ErrMsg = e.Message;
                    return r;
                }
            }
            if (c == ClasseTituloBase.ParcelaStudio)
            {
                var ap = await _context.AlunoPlanoParcela.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == parcelaid);
                if (ap == null)
                {
                    r.Ok = false;
                    r.ErrMsg = "Aluno plano parcela não encontrado";
                    return r;
                }
                var alunop = await _context.AlunoPlano.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == ap.AlunoPlanoId);
                var aluno = await _context.Aluno.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == alunop.AlunoId);
                var rel = await _context.Relacionamento.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == aluno.RelacionamentoId);
                StudioConfig scfg = await _context.StudioConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
                double maxvp = Math.Round(await _context.MovtoBancoStudioParcela.Where(x => x.MovtoBancoId == movtoid && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                double maxvpar = Math.Round(await _context.MovtoBancoStudioParcela.Where(x => x.StudioParcelaId == parcelaid && x.TenantId == si.TenantId && x.OrigemParcela == OrigemStudioParcela.PlanoParcela).SumAsync(x => x.Valor), 2);
                maxvpar = valor - maxvpar;
                double valc = maxvpar;
                if (valc > (m.Valor - maxvp))
                {
                    valc = (m.Valor - maxvp);
                }
                MovtoBancoStudioParcela mp = new()
                {
                    MovtoBancoId = movtoid,
                    TenantId = si.TenantId,
                    StudioParcelaId = parcelaid,
                    OrigemParcela = OrigemStudioParcela.PlanoParcela,
                    Valor = valc
                };
                await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.MovtoBancoStudioParcela.AddAsync(mp);
                    await _context.SaveChangesAsync();

                    maxvpar = Math.Round(await _context.MovtoBancoStudioParcela.Where(x => x.StudioParcelaId == ap.Id && x.TenantId == si.TenantId && x.OrigemParcela == OrigemStudioParcela.PlanoParcela).SumAsync(x => x.Valor), 2);
                    if (Math.Round(maxvpar, 2) < Math.Round(ap.Valor, 2))
                    {
                        ap.Status = StatusParcela.ConciliadoParcial;
                        ap.DataPagto = DateTime.MinValue;
                    }
                    else
                    {
                        ap.Status = StatusParcela.Conciliado;
                        ap.DataPagto = m.DataMovto;
                    }
                    ap.ValorPago = maxvpar;
                    ap.ValorConciliado = maxvpar;

                    _context.AlunoPlanoParcela.Update(ap);

                    double maxv = Math.Round(await _context.MovtoBancoStudioParcela.Where(x => x.MovtoBancoId == movtoid && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                    if (maxv == m.Valor)
                    {
                        m.StatusConciliacao = StatusConciliacao.Total;
                    }
                    else
                    {
                        m.StatusConciliacao = StatusConciliacao.Parcial;
                    }
                    m.RelacionamentoId = aluno.RelacionamentoId;
                    m.PlanoContaId = scfg.PlanoContaIdAluno;
                    _context.MovtoBanco.Update(m);
                    await _context.SaveChangesAsync();

                    StudioOperacoesService stdope = new(_context, _sessionStorageService, _sessionService, _mapper, null, null);
                    r = await stdope.ConciliarAlunoPlano(alunop.Id);

                    await _context.Database.CommitTransactionAsync();
                    r.Ok = true;
                }
                catch (Exception e)
                {
                    await _context.Database.CommitTransactionAsync();
                    r.Ok = false;
                    r.ErrMsg = e.Message;
                    return r;
                }
            }
            if (c == ClasseTituloBase.ProgramacaoAula)
            {
                var pa = await _context.ProgramacaoAula.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == parcelaid);
                if (pa == null)
                {
                    r.Ok = false;
                    r.ErrMsg = "Aluno aula avulsa não encontrado";
                    return r;
                }
                var aluno = await _context.Aluno.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == pa.AlunoId);
                var rel = await _context.Relacionamento.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == aluno.RelacionamentoId);
                StudioConfig scfg = await _context.StudioConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
                double maxvp = Math.Round(await _context.MovtoBancoStudioParcela.Where(x => x.MovtoBancoId == movtoid && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                double maxvpar = Math.Round(await _context.MovtoBancoStudioParcela.Where(x => x.StudioParcelaId == parcelaid && x.TenantId == si.TenantId && x.OrigemParcela == OrigemStudioParcela.ProgramacaoAula).SumAsync(x => x.Valor), 2);
                maxvpar = valor - maxvpar;
                double valc = maxvpar;
                if (valc > (m.Valor - maxvp))
                {
                    valc = (m.Valor - maxvp);
                }
                MovtoBancoStudioParcela mp = new()
                {
                    MovtoBancoId = movtoid,
                    TenantId = si.TenantId,
                    StudioParcelaId = parcelaid,
                    OrigemParcela = OrigemStudioParcela.ProgramacaoAula,
                    Valor = valc
                };
                await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.MovtoBancoStudioParcela.AddAsync(mp);
                    await _context.SaveChangesAsync();

                    maxvpar = Math.Round(await _context.MovtoBancoStudioParcela.Where(x => x.StudioParcelaId == pa.Id && x.TenantId == si.TenantId && x.OrigemParcela == OrigemStudioParcela.ProgramacaoAula).SumAsync(x => x.Valor), 2);
                    if (maxvpar < pa.Valor)
                    {
                        pa.StatusFinanceiro = StatusParcela.ConciliadoParcial;
                        pa.DataPagto = DateTime.MinValue;
                    }
                    else
                    {
                        pa.StatusFinanceiro = StatusParcela.Conciliado;
                        pa.DataPagto = m.DataMovto;
                    }
                    pa.ValorPago = maxvpar;
                    pa.ValorConciliado = maxvpar;
                    _context.ProgramacaoAula.Update(pa);

                    double maxv = Math.Round(await _context.MovtoBancoStudioParcela.Where(x => x.MovtoBancoId == movtoid && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                    if (Math.Round(maxv, 2) == Math.Round(m.Valor, 2))
                    {
                        m.StatusConciliacao = StatusConciliacao.Total;
                    }
                    else
                    {
                        m.StatusConciliacao = StatusConciliacao.Parcial;
                    }
                    m.RelacionamentoId = aluno.RelacionamentoId;
                    m.PlanoContaId = scfg.PlanoContaIdAluno;
                    _context.MovtoBanco.Update(m);
                    await _context.SaveChangesAsync();
                    await _context.Database.CommitTransactionAsync();
                    r.Ok = true;
                }
                catch (Exception e)
                {
                    await _context.Database.CommitTransactionAsync();
                    r.Ok = false;
                    r.ErrMsg = e.Message;
                    return r;
                }
            }
            _context.ChangeTracker.Clear();
            return r;
        }
        public async Task<Resultado> DesvincularMovtoBanco(int movtoid, TituloBaseView tv)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            try
            {
                _context.ChangeTracker.Clear();
                await _context.Database.BeginTransactionAsync();
                if (tv.Classe == ClasseTituloBase.LotePagtoProfessor || tv.Classe == ClasseTituloBase.ParcelaStudio || tv.Classe == ClasseTituloBase.ProgramacaoAula)
                {
                    var ms = await _context.MovtoBancoStudioParcela.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.MovtoBancoId == movtoid && x.StudioParcelaId == tv.Id);
                    if (ms != null)
                    {
                        _context.MovtoBancoStudioParcela.Remove(ms);
                        await _context.SaveChangesAsync();
                    }
                    if (tv.Classe == ClasseTituloBase.LotePagtoProfessor)
                    {
                        var lp = await _context.ProfessorLotePagto.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == tv.Id);
                        if (lp != null)
                        {
                            double maxvpar = Math.Round(await _context.MovtoBancoStudioParcela.Where(x => x.StudioParcelaId == lp.Id && x.MovtoBancoId != movtoid && x.OrigemParcela == OrigemStudioParcela.LotePagtoProfessor && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                            if (maxvpar == 0)
                            {
                                lp.Status = StatusProfessorLotePagto.Aberto;
                                lp.DataPagto = DateTime.MinValue;
                            }
                            else
                            {
                                if (maxvpar < lp.ValorTotalPagar)
                                {
                                    lp.DataPagto = DateTime.MinValue;
                                    lp.Status = StatusProfessorLotePagto.ConciliadoParcial;
                                }
                                else
                                {
                                    lp.Status = StatusProfessorLotePagto.Conciliado;
                                }
                            }
                            _context.ProfessorLotePagto.Update(lp);
                        }
                    }
                    if (tv.Classe == ClasseTituloBase.ParcelaStudio)
                    {
                        var ap = await _context.AlunoPlanoParcela.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == tv.Id);
                        if (ap != null)
                        {
                            double maxvpar = Math.Round(await _context.MovtoBancoStudioParcela.Where(x => x.StudioParcelaId == ap.Id && x.MovtoBancoId != movtoid && x.OrigemParcela == OrigemStudioParcela.PlanoParcela && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                            if (maxvpar == 0)
                            {
                                ap.Status = StatusParcela.Aberto;
                                ap.DataPagto = DateTime.MinValue;
                            }
                            else
                            {
                                if (maxvpar < ap.Valor)
                                {
                                    ap.Status = StatusParcela.ConciliadoParcial;
                                    ap.DataPagto = DateTime.MinValue;
                                }
                                else
                                {
                                    ap.Status = StatusParcela.Conciliado;
                                }
                            }
                            ap.ValorPago = maxvpar;
                            ap.ValorConciliado = maxvpar;
                            _context.AlunoPlanoParcela.Update(ap);
                        }
                    }
                    if (tv.Classe == ClasseTituloBase.ProgramacaoAula)
                    {
                        var pa = await _context.ProgramacaoAula.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == tv.Id);
                        if (pa != null)
                        {
                            double maxvpar = Math.Round(await _context.MovtoBancoStudioParcela.Where(x => x.StudioParcelaId == pa.Id && x.MovtoBancoId != movtoid && x.OrigemParcela == OrigemStudioParcela.ProgramacaoAula && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                            if (maxvpar == 0)
                            {
                                pa.StatusFinanceiro = StatusParcela.Aberto;
                                pa.DataPagto = DateTime.MinValue;
                            }
                            else
                            {
                                if (maxvpar < pa.Valor)
                                {
                                    pa.DataPagto = DateTime.MinValue;
                                    pa.StatusFinanceiro = StatusParcela.ConciliadoParcial;
                                }
                                else
                                {
                                    pa.StatusFinanceiro = StatusParcela.Conciliado;
                                }
                            }
                            pa.ValorPago = maxvpar;
                            pa.ValorConciliado = maxvpar;
                            _context.ProgramacaoAula.Update(pa);
                        }
                    }
                }
                else
                {
                    if (tv.Classe == ClasseTituloBase.TituloPagar)
                    {
                        var mp = await _context.MovtoBancoTitulo.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.MovtoBancoId == movtoid && x.TituloParcelaId == tv.Id);
                        if (mp != null)
                        {
                            _context.MovtoBancoTitulo.Remove(mp);
                        }
                        await _context.SaveChangesAsync();
                        var p = await _context.TituloParcela.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == tv.Id);
                        if (p != null)
                        {
                            double valortitconciliado = Math.Round(await _context.MovtoBancoTitulo.Where(x => x.TituloParcelaId == p.Id && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                            if (valortitconciliado == 0)
                            {
                                p.Status = StatusParcela.Aberto;
                                p.DataPagto = DateTime.MinValue;
                            }
                            else
                            {
                                p.Status = StatusParcela.ConciliadoParcial;
                                p.DataPagto = DateTime.MinValue;
                            }
                            p.ValorPago = valortitconciliado;
                            _context.TituloParcela.Update(p);
                        }
                    }
                }
                var m = await _context.MovtoBanco.FirstOrDefaultAsync(x => x.Id == movtoid && x.TenantId == si.TenantId);
                if (m != null)
                {
                    if (si.TenantApp == TenantApp.Studio)
                    {
                        StudioConfig scfg = await _context.StudioConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
                        if (m.TipoMovtoBanco == TipoMovtoBanco.Credito)
                        {
                            m.PlanoContaId = scfg.PlanoContaDefaultReceitaId;
                        }
                        else
                        {
                            m.PlanoContaId = scfg.PlanoContaDefaultDespesaId;
                        }
                        m.RelacionamentoId = scfg.RelacionamentoDefaultId;
                    }
                    else
                    {
                        m.PlanoContaId = 0;
                    }
                    if (await _context.MovtoBancoStudioParcela.AnyAsync(x => x.TenantId == si.TenantId && x.MovtoBancoId == m.Id) ||
                        await _context.MovtoBancoTitulo.AnyAsync(x => x.TenantId == si.TenantId && x.MovtoBancoId == m.Id))
                    {
                        m.StatusConciliacao = StatusConciliacao.Parcial;
                    }
                    else
                    {
                        m.StatusConciliacao = StatusConciliacao.Pendente;
                    }
                    m.RelacionamentoId = 0;
                    m.PlanoContaId = 0;
                    _context.MovtoBanco.Update(m);
                }
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                r.Ok = true;
                _context.ChangeTracker.Clear();
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
            return r;
        }
    }
}




