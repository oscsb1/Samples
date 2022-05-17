using AutoMapper;
using Blazored.SessionStorage;
using InCorpApp.Data;
using InCorpApp.Models;
using InCorpApp.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Constantes;
using Microsoft.Data.SqlClient;
using InCorpApp.Utils;

namespace InCorpApp.Services
{
    public class StudioCadastroService : ServiceBase
    {
        private readonly ApplicationDbContext _context;
        private readonly RelacionamentoService _relacionamentoService;
        private readonly UserService _userService;
        public StudioCadastroService(ApplicationDbContext context,
                    ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper, RelacionamentoService relacionamentoservice, UserService userService) : base(sessionStorageService, sessionService, mapper)
        {
            _context = context;
            _relacionamentoService = relacionamentoservice;
            _userService = userService;
        }
        public async Task<List<Studio>> FindAllStudioAsync()
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Studio.Where(x => x.TenantId == si.TenantId).OrderBy(x => x.Nome).ToListAsync();
        }
        public async Task<Studio> FindStudioByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            Studio s = await _context.Studio.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
            if (s != null)
            {
                s.StudioPlanos = await FindAllPlanosByStudioIdAsync(s.Id);
            }
            return s;
        }
        public async Task<Studio> AddStudioAsync(Studio s)
        {
            SessionInfo si = await GetSessionAsync();

            s.TenantId = si.TenantId;
            await _context.Studio.AddAsync(s);
            await _context.SaveChangesAsync();
            return s;
        }
        public async Task<Resultado> RemoveStudioAsync(Studio s)
        {
            SessionInfo si = await GetSessionAsync();

            Resultado r = new();

            if (await _context.StudioPlano.AnyAsync(x => x.StudioId == s.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Studio possue plano cadastradas, não pode ser excluído.";
                return r;
            }

            if (await _context.Agenda.AnyAsync(x => x.StudioId == s.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Studio possue agenda cadastrada, não pode ser excluído.";
                return r;
            }

            try
            {
                _context.Studio.Remove(s);
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
        public async Task<Resultado> UpdadeStudioAsync(Studio s)
        {
            SessionInfo si = await GetSessionAsync();

            Resultado r = new();

            if (await _context.Studio.AnyAsync(x => x.Nome == s.Nome && x.Id != s.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Nome do studio já cadastrado.";
                return r;
            }


            try
            {
                _context.Studio.Update(s);
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
        public async Task<List<StudioPlano>> FindAllPlanosByStudioIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            List<StudioPlano> r = new();

            return await (from StudioPlano in _context.StudioPlano
                          join Plano in _context.Plano on StudioPlano.PlanoId equals Plano.Id
                          select new StudioPlano { StudioId = StudioPlano.StudioId, PlanoId = StudioPlano.PlanoId, Plano = Plano, Id = StudioPlano.Id, TenantId = StudioPlano.TenantId }
                          ).Where(x => x.StudioId == id && x.TenantId == si.TenantId).ToListAsync();
        }
        public async Task<Resultado> DeleteStudioPlanoAsync(StudioPlano p)
        {
            Resultado r = new();
            try
            {
                _context.StudioPlano.Remove(p);
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
        public async Task<Resultado> AddStudioPlanoAsync(StudioPlano p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (await _context.StudioPlano.AnyAsync(x => x.TenantId == si.TenantId && x.PlanoId == p.PlanoId && x.StudioId == p.StudioId))
            {
                r.Ok = false;
                r.ErrMsg = "Plano já cadastrada para este studio";
                return r;
            }
            try
            {
                p.TenantId = si.TenantId;
                await _context.StudioPlano.AddAsync(p);
                await _context.SaveChangesAsync();
                r.Ok = true;
                r.ErrMsg = string.Empty;
                return r;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<Resultado> UpdateStudioPlanoAsync(StudioPlano p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            if (await _context.StudioPlano.AnyAsync(x => x.TenantId == si.TenantId && x.PlanoId == p.PlanoId && x.StudioId == p.StudioId && x.Id != p.Id))
            {
                r.Ok = false;
                r.ErrMsg = "Plano já cadastrada para este studio";
                return r;
            }
            try
            {
                _context.StudioPlano.Update(p);
                await _context.SaveChangesAsync();
                r.Ok = true;
                r.ErrMsg = string.Empty;
                return r;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<List<Agenda>> FindAllAgendaByStudioId(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from Agenda in _context.Agenda
                          join Studio in _context.Studio on Agenda.StudioId equals Studio.Id
                          select Agenda
                            ).Where(x => x.TenantId == si.TenantId && x.StudioId == id).OrderByDescending(x => x.DataInicio).ToListAsync();
        }
        public async Task<Agenda> FindAgendaByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Agenda.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == id);
        }
        public async Task UpdateAgendaAsync(Agenda a)
        {
            SessionInfo si = await GetSessionAsync();
            if (a.DataInicio == DateTime.MinValue)
            {
                a.DataInicio = Constante.Today;
            }
            if (await _context.Agenda.AnyAsync(x => x.TenantId == si.TenantId && x.StudioId == a.StudioId && x.Id != a.Id && x.DataInicio == a.DataInicio))
            {
                throw new Exception("Já existe uma agenda cadastrada com a data de " + a.DataInicio.ToString("dd/MM/yyyy"));

            }
            _context.Agenda.Update(a);
            await _context.SaveChangesAsync();
        }
        public async Task<int> AddAgendaAsync(Agenda a)
        {
            SessionInfo si = await GetSessionAsync();
            a.TenantId = si.TenantId;
            if (a.DataInicio == DateTime.MinValue)
            {
                a.DataInicio = Constante.Today;
            }
            if (await _context.Agenda.AnyAsync(x => x.TenantId == si.TenantId && x.StudioId == a.StudioId && x.Id != a.Id && x.DataInicio == a.DataInicio))
            {
                throw new Exception("Já existe uma agenda cadastrada com a data de " + a.DataInicio.ToString("dd/MM/yyyy"));

            }
            _context.Agenda.Add(a);
            await _context.SaveChangesAsync();
            return a.Id;
        }
        public async Task DeleteAgendaAsync(Agenda a)
        {
            SessionInfo si = await GetSessionAsync();
            _context.Database.BeginTransaction();
            try
            {
                var ld = await _context.AgendaDia.Where(x => x.TenantId == si.TenantId && x.AgendaId == a.Id).ToListAsync();
                if (ld.Count > 0)
                {
                    _context.AgendaDia.RemoveRange(ld);
                    await _context.SaveChangesAsync();
                }
                _context.Agenda.Remove(a);
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;
            }
        }
        public async Task<List<AgendaDia>> FindAllAgendaDiaByAgendaIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from AgendaDia in _context.AgendaDia
                          join Agenda in _context.Agenda on AgendaDia.AgendaId equals Agenda.Id
                          select AgendaDia
                           ).Where(x => x.AgendaId == id && x.TenantId == si.TenantId).ToListAsync();
        }
        public async Task<List<AgendaDia>> FindAllAgendaDiaByDateAsync(int studioid, DateTime data)
        {
            SessionInfo si = await GetSessionAsync();
            var la = await _context.Agenda.Where(x => x.StudioId == studioid && x.TenantId == si.TenantId && x.DataInicio <= data).OrderByDescending(x => x.DataInicio).ToListAsync();
            if (la.Count == 0)
            {
                throw new Exception("Studio sem agenda cadastrada");
            }
            return await FindAllAgendaDiaByAgendaIdAsync(la.FirstOrDefault(x => x.Id > 0).Id);
        }
        public async Task<AgendaDia> FindAgendaDiaByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.AgendaDia.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == id);
        }
        public async Task<Resultado> DeleteAgendaDiaAsync(AgendaDia p)
        {
            Resultado r = new();
            _context.AgendaDia.Remove(p);
            await _context.SaveChangesAsync();
            r.Ok = true;
            r.ErrMsg = string.Empty;
            return r;
        }
        public async Task<Resultado> AddAgendaDiaAsync(AgendaDia ap)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            ap.TenantId = si.TenantId;
            await _context.AgendaDia.AddAsync(ap);
            await _context.SaveChangesAsync();
            r.Ok = true;
            r.ErrMsg = string.Empty;
            return r;
        }
        public async Task<Resultado> UpdateAgendaDiaAsync(AgendaDia ap)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            ap.TenantId = si.TenantId;
            _context.AgendaDia.Update(ap);
            await _context.SaveChangesAsync();
            r.Ok = true;
            r.ErrMsg = string.Empty;
            return r;
        }
        public async Task<List<AgendaDiaEspecial>> FindAllAgendaDiaEspecialByStudioId(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from AgendaDiaEspecial in _context.AgendaDiaEspecial
                          join Studio in _context.Studio on AgendaDiaEspecial.StudioId equals Studio.Id
                          select AgendaDiaEspecial
                            ).Where(x => x.TenantId == si.TenantId && x.StudioId == id && x.Data >= Constante.Today.AddMonths(-3)).OrderBy(x => x.Data).ToListAsync();
        }
        public async Task<List<AgendaDiaEspecial>> FindAllAgendaDiaEspecialByDatas(int studioid, DateTime di, DateTime df)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from AgendaDiaEspecial in _context.AgendaDiaEspecial
                          join Studio in _context.Studio on AgendaDiaEspecial.StudioId equals Studio.Id
                          select AgendaDiaEspecial
                            ).Where(x => x.TenantId == si.TenantId && x.StudioId == studioid &&
                            x.Data >= di && x.Data <= df).OrderBy(x => x.Data).ToListAsync();
        }
        public async Task<AgendaDiaEspecial> FindAgendaDiaEspecialByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.AgendaDiaEspecial.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == id);
        }
        public async Task UpdateAgendaDiaEspecialAsync(AgendaDiaEspecial a)
        {
            if (a.Data < Constante.Today.AddMonths(-6))
            {
                throw new Exception("Data inferior a data atual");
            }
            _context.AgendaDiaEspecial.Update(a);
            await _context.SaveChangesAsync();
        }
        public async Task<int> AddAgendaDiaEspecialAsync(AgendaDiaEspecial a)
        {
            SessionInfo si = await GetSessionAsync();
            a.TenantId = si.TenantId;
            if (a.Data < Constante.Today.AddMonths(-6))
            {
                throw new Exception("Data inferior a data atual");
            }
            _context.AgendaDiaEspecial.Add(a);
            await _context.SaveChangesAsync();
            return a.Id;
        }
        public async Task DeleteAgendaDiaEspecialAsync(AgendaDiaEspecial a)
        {
            _context.AgendaDiaEspecial.Remove(a);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Aula>> FindAllAulaAsync()
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Aula.Where(x => x.TenantId == si.TenantId).OrderBy(x => x.Nome).ToListAsync();
        }
        public async Task<List<Aula>> FindAllAulaAByAlunoIdAsync(int alunoid)
        {
            SessionInfo si = await GetSessionAsync();
            var la = await (from Aula in _context.Aula
                            join AlunoPlanoAula in _context.AlunoPlanoAula on Aula.Id equals AlunoPlanoAula.AulaId
                            join AlunoPlano in _context.AlunoPlano on AlunoPlanoAula.AlunoPlanoId equals AlunoPlano.Id
                            select new { Aula, AlunoPlano }).Where(x => x.AlunoPlano.TenantId == si.TenantId && x.AlunoPlano.AlunoId == alunoid).ToListAsync();
            List<Aula> r = new();
            foreach (var it in la)
            {
                if (!r.Any(x => x.Id == it.Aula.Id))
                {
                    r.Add(it.Aula);
                }
            }
            return r;
        }
        public async Task<int> GetTotalAulasDadasByPlanoIdAulaId(int alunoid, int alunoplanoid, int aulaid)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ProgramacaoAula.Where(
                x => x.AlunoId == alunoid && x.AlunoPlanoId == alunoplanoid &&
                x.AulaId == aulaid && x.TenantId == si.TenantId && (x.Status == StatusAula.Executada || x.Status == StatusAula.FaltaSemReagendamento)).CountAsync();
        }
        public async Task<int> GetTotalAulasAtivasByPlanoIdAulaId(int alunoid, int alunoplanoid, int aulaid)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ProgramacaoAula.Where(
                x => x.AlunoId == alunoid && x.AlunoPlanoId == alunoplanoid &&
                x.AulaId == aulaid && x.TenantId == si.TenantId && (x.Status != StatusAula.Cancelada)).CountAsync();
        }
        public async Task<List<AlunoPlanoAula>> FindAllAulaAByAlunoIdPacotesync(int alunoid)
        {
            SessionInfo si = await GetSessionAsync();
            List<AlunoPlanoAula> r = new();
            var la = await (from Aula in _context.Aula
                            join AlunoPlanoAula in _context.AlunoPlanoAula on Aula.Id equals AlunoPlanoAula.AulaId
                            join AlunoPlano in _context.AlunoPlano on AlunoPlanoAula.AlunoPlanoId equals AlunoPlano.Id
                            join Plano in _context.Plano on AlunoPlano.PlanoId equals Plano.Id
                            select new { Aula, AlunoPlano, Plano, AlunoPlanoAula }).Where(x => x.AlunoPlano.TenantId == si.TenantId
                                         && x.AlunoPlano.AlunoId == alunoid && x.Plano.TipoPlano == TipoPlano.PacoteQtdeAula).ToListAsync();

            foreach (var it in la)
            {
                int t = await GetTotalAulasDadasByPlanoIdAulaId(it.AlunoPlano.AlunoId, it.AlunoPlano.Id, it.Aula.Id);
                int at = await GetTotalAulasAtivasByPlanoIdAulaId(it.AlunoPlano.AlunoId, it.AlunoPlano.Id, it.Aula.Id);
                if (it.AlunoPlanoAula.QtdeAulas > at)
                {
                    it.AlunoPlanoAula.Aula = it.Aula;
                    it.AlunoPlanoAula.AlunoPlano = it.AlunoPlano;
                    it.AlunoPlanoAula.AlunoPlano.Plano = it.Plano;
                    it.AlunoPlanoAula.TotalAulasFeitas = t;
                    it.AlunoPlanoAula.TotalAulasAtivas = at;
                    r.Add(it.AlunoPlanoAula);
                }
            }
            return r;
        }
        public async Task<Resultado> PodeIncluirAulaAvulsa(int alunoid, int aulaid)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            var ap = await FindAllAulaAByAlunoIdPacotesync(alunoid);
            var aa = ap.FirstOrDefault(x => x.AulaId == aulaid);
            if (aa != null)
            {
                if (aa.QtdeAulas - aa.TotalAulasAtivas > 0)
                {
                    r.Ok = false;
                    r.ErrMsg = "Aluno possui " + (aa.QtdeAulas - aa.TotalAulasAtivas).ToString() + " aulas  pendentes no pacote, não há necessidade de criar aula avulsa.";
                    return r;
                }
            }

            int ta = await _context.ProgramacaoAula.Where(x => x.AlunoId == alunoid && x.AulaId == aulaid && x.TipoAula != TipoAula.Avulsa &&
                   (x.Status == StatusAula.Agendada || x.Status == StatusAula.NaoProgramada || x.Status == StatusAula.Programada || x.Status == StatusAula.ReAgendamento)).CountAsync();
            if (ta > 0)
            {
                r.Ok = false;
                r.ErrMsg = "Aluno possui " + (ta).ToString() + " aulas  pendentes no plano, não há necessidade de criar aula avulsa.";
                return r;
            }
            r.Ok = true;
            return r;
        }
        public async Task<List<AlunoPlanoAula>> FindAllAulaAByAlunoPlanoIdAsync(int alunoplanoid)
        {
            SessionInfo si = await GetSessionAsync();
            var la = await (from Aula in _context.Aula
                            join AlunoPlanoAula in _context.AlunoPlanoAula on Aula.Id equals AlunoPlanoAula.AulaId
                            select new { Aula, AlunoPlanoAula }).Where(x => x.AlunoPlanoAula.TenantId == si.TenantId
                                         && x.AlunoPlanoAula.AlunoPlanoId == alunoplanoid).ToListAsync();
            List<AlunoPlanoAula> r = new();
            foreach (var it in la)
            {
                it.AlunoPlanoAula.Aula = it.Aula;
                r.Add(it.AlunoPlanoAula);
            }
            return r;
        }
        public async Task<Aula> FindAulaByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            var a = await _context.Aula.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
            if (a.Codigo == null)
            {
                a.Codigo = string.Empty;
            }
            return a;
        }
        public async Task<Aula> AddAulaAsync(Aula s)
        {
            SessionInfo si = await GetSessionAsync();
            s.TenantId = si.TenantId;
           await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Aula.AddAsync(s);
                await _context.SaveChangesAsync();
                var lp = await _context.Professor.Where(x => x.TenantId == si.TenantId).ToListAsync();
                foreach (var p in lp)
                {
                    ProfessorAula pa = new()
                    {
                        TenantId = si.TenantId,
                        AulaId = s.Id,
                        ProfessorId = p.Id
                    };
                    await _context.ProfessorAula.AddAsync(pa);
                }
                if (lp.Count > 0)
                {
                    await _context.SaveChangesAsync();
                }
                await _context.Database.CommitTransactionAsync();
            }
            catch 
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
            return s;
        }
        public async Task<Resultado> RemoveAulaAsync(Aula s)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            if (await _context.PlanoAula.AnyAsync(x => x.AulaId == s.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Aula vinculado ao studio, não pode ser excluído.";
                return r;
            }
            if (await _context.ProgramacaoAula.AnyAsync(x => x.AulaId == s.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Aula com agenda não pode ser excluído.";
                return r;
            }
            if (await _context.AlunoAulaAgenda.AnyAsync(x => x.AulaId == s.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Aula com agenda não pode ser excluído.";
                return r;
            }

            try
            {
                await _context.Database.BeginTransactionAsync();
                await _context.Database.ExecuteSqlRawAsync(" Delete ProfessorAula where TenantId = '" + si.TenantId + "'  and AulaId = " + s.Id.ToString() );
                _context.Aula.Remove(s);
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
        public async Task<Resultado> UpdadeAulaAsync(Aula s)
        {
            SessionInfo si = await GetSessionAsync();

            Resultado r = new();

            if (await _context.Aula.AnyAsync(x => x.Nome == s.Nome && x.Id != s.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Nome da Aula já cadastrado.";
                return r;
            }
            try
            {
                await _context.Database.BeginTransactionAsync();
                _context.Aula.Update(s);
                var la = await _context.AulaAgenda.Where(x => x.TenantId == si.TenantId && x.AulaId == s.Id).ToListAsync();
                foreach (var aa in la)
                {
                    aa.Fim = aa.Inicio + s.Duracao;
                    _context.AulaAgenda.Update(aa);
                }
                await _context.SaveChangesAsync();
                r.Ok = true;
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
        public async Task<List<Professor>> FindAllProfessorAsync(bool soativo = true)
        {
            SessionInfo si = await GetSessionAsync();

            if (soativo)
            {
                return await (from Professor in _context.Professor
                              join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id

                              select new Professor() { Id = Professor.Id, RelacionamentoId = Relacionamento.Id, Relacionamento = Relacionamento, TenantId = Professor.TenantId }
                              ).Where(x => x.TenantId == si.TenantId && x.Relacionamento.Ativo == true).OrderBy(x => x.Relacionamento.Nome).ToListAsync();
            }
            else
            {
                return await (from Professor in _context.Professor
                              join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id

                              select new Professor() { Id = Professor.Id, RelacionamentoId = Relacionamento.Id, Relacionamento = Relacionamento, TenantId = Professor.TenantId }
                              ).Where(x => x.TenantId == si.TenantId).OrderBy(x => x.Relacionamento.Nome).ToListAsync();
            }
        }
        public async Task<Professor> FindProfessorByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from Professor in _context.Professor
                          join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                          select new Professor() { Id = Professor.Id, RelacionamentoId = Relacionamento.Id, Relacionamento = Relacionamento, TenantId = Professor.TenantId }
                          ).Where(x => x.TenantId == si.TenantId && x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Professor> FindProfessorByUserIdAsync(string id)
        {
            SessionInfo si = await GetSessionAsync();
            var p1 = await (from ProfessorUsuario in _context.ProfessorUsuario
                            join Professor in _context.Professor on ProfessorUsuario.ProfessorId equals Professor.Id
                            join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                            select new { Professor, ProfessorUsuario.IdentityUserId }
                          ).Where(x => x.Professor.TenantId == si.TenantId && x.IdentityUserId == id).FirstOrDefaultAsync();

            if (p1 != null)
            {
                return p1.Professor;
            }
            else
            {
                return null;
            }
        }
        public async Task<Professor> FindProfessorByRelacionamentoIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Professor.Where(x => x.TenantId == si.TenantId && x.RelacionamentoId == id).FirstOrDefaultAsync();
        }
        public async Task<Resultado> LiberarAcessoProfessor(Professor p, string email)
        {
            Resultado r = new();
            try
            {
                await _context.Database.BeginTransactionAsync();
                await _userService.CriarUsuarioProfessor(p, email);
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
        public async Task<Resultado> UpdateProfessor(Professor p)
        {
            Resultado r = new();
            try
            {
                _context.Professor.Update(p);
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
        public async Task<Resultado> RemoveProfessorAsync(Professor s)
        {
            SessionInfo si = await GetSessionAsync();

            Resultado r = new();

            if (await (from Professor in _context.Professor
                       join ProgramacaoAula in _context.ProgramacaoAula on Professor.Id equals ProgramacaoAula.ProfessorId
                       select new { ProgramacaoAula, Professor }
                          ).AnyAsync(x => x.ProgramacaoAula.TenantId == si.TenantId && x.Professor.Id == s.Id))
            {
                r.Ok = false;
                r.ErrMsg = "Professor possue aulas regisradas, não pode ser excluído.";
                return r;
            }

            try
            {
                await _relacionamentoService.DeleteAsync(s.Relacionamento, TipoRelacionamento.Professor);
                r.Ok = true;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<List<ProfessorAula>> FindAllProfessorAula()
        {
            SessionInfo si = await GetSessionAsync();
            List<ProfessorAula> r = new();
            r = await _context.ProfessorAula.AsNoTracking().Where(x => x.TenantId == si.TenantId).ToListAsync();
            r.ForEach(x => x.Vinculado = true);
            return r;
        }
        public async Task<Resultado> AddProfessorAula(int professorid, int aulaid)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            ProfessorAula pa = new()
            {
                TenantId = si.TenantId,
                ProfessorId = professorid,
                AulaId = aulaid
            };
            await _context.ProfessorAula.AddAsync(pa);
            await _context.SaveChangesAsync();
            r.Ok = true;
            return r;
        }
        public async Task<Resultado> DeleteProfessorAula(int professorid, int aulaid)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            ProfessorAula pa = await _context.ProfessorAula.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.ProfessorId == professorid && x.AulaId == aulaid);
            if (pa != null)
            {
                _context.ProfessorAula.Remove(pa);
               await _context.SaveChangesAsync();
            }
            r.Ok = true;
            return r;
        }
        public async Task<List<ProfessorAgenda>> FindAllProfessorAgendaByProfessorId(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from ProfessorAgenda in _context.ProfessorAgenda
                          join Studio in _context.Studio on ProfessorAgenda.StudioId equals Studio.Id
                          select ProfessorAgenda
                            ).Where(x => x.TenantId == si.TenantId && x.ProfessorId == id).OrderByDescending(x => x.DataInicio).ToListAsync();
        }
        public async Task<ProfessorAgenda> FindProfessorAgendaByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ProfessorAgenda.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == id);
        }
        public async Task UpdateProfessorAgendaAsync(ProfessorAgenda a)
        {
            SessionInfo si = await GetSessionAsync();
            if (a.DataInicio == DateTime.MinValue)
            {
                a.DataInicio = Constante.Today;
            }
            if (await _context.ProfessorAgenda.AnyAsync(x => x.TenantId == si.TenantId && x.ProfessorId == a.ProfessorId && x.StudioId == a.StudioId && x.Id != a.Id && x.DataInicio == a.DataInicio))
            {
                throw new Exception("Já existe uma agenda cadastrada com a data de " + a.DataInicio.ToString("dd/MM/yyyy"));
            }
            _context.ProfessorAgenda.Update(a);
            await _context.SaveChangesAsync();
        }
        public async Task<int> AddProfessorAgendaAsync(ProfessorAgenda a)
        {
            SessionInfo si = await GetSessionAsync();
            a.TenantId = si.TenantId;
            if (a.DataInicio == DateTime.MinValue)
            {
                a.DataInicio = Constante.Today;
            }
            if (await _context.ProfessorAgenda.AnyAsync(x => x.TenantId == si.TenantId && x.ProfessorId == a.ProfessorId && x.StudioId == a.StudioId && x.Id != a.Id && x.DataInicio == a.DataInicio))
            {
                throw new Exception("Já existe uma agenda cadastrada com a data de " + a.DataInicio.ToString("dd/MM/yyyy"));
            }
            _context.ProfessorAgenda.Add(a);
            await _context.SaveChangesAsync();
            return a.Id;
        }
        public async Task DeleteProfessorAgendaAsync(ProfessorAgenda a)
        {
            SessionInfo si = await GetSessionAsync();
            _context.Database.BeginTransaction();
            a.Studio = null;
            a.Professor = null;
            try
            {
                var ld = await _context.ProfessorAgendaDia.Where(x => x.TenantId == si.TenantId && x.ProfessorAgendaId == a.Id).ToListAsync();
                if (ld.Count > 0)
                {
                    _context.ProfessorAgendaDia.RemoveRange(ld);
                    await _context.SaveChangesAsync();
                }
                _context.ProfessorAgenda.Remove(a);
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;
            }
        }
        public async Task<List<ProfessorAgendaDia>> FindAllProfessorAgendaDiaByProfessorAgendaIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from ProfessorAgendaDia in _context.ProfessorAgendaDia
                          join ProfessorAgenda in _context.ProfessorAgenda on ProfessorAgendaDia.ProfessorAgendaId equals ProfessorAgenda.Id
                          select ProfessorAgendaDia
                           ).Where(x => x.ProfessorAgendaId == id && x.TenantId == si.TenantId).ToListAsync();
        }
        public async Task<List<ProfessorAgendaDia>> FindAllProfessorAgendaDiaByDataAsync(int studioid, DateTime data)
        {
            SessionInfo si = await GetSessionAsync();
            var profs = await FindAllProfessorAsync();
            List<ProfessorAgendaDia> r = new();
            foreach (Professor p in profs)
            {
                var la = await _context.ProfessorAgenda.Where(x => x.ProfessorId == p.Id && x.StudioId == studioid && x.TenantId == si.TenantId && x.DataInicio <= data).OrderByDescending(x => x.DataInicio).ToListAsync();
                if (la.Count > 0)
                {
                    var lp = await FindAllProfessorAgendaDiaByProfessorAgendaIdAsync(la.FirstOrDefault(x => x.Id > 0).Id);
                    foreach (var pa in lp)
                    {
                        r.Add(pa);
                    }
                }
            }
            return r;
        }
        public async Task<List<ProfessorAgendasDia>> FindAllProfessorAgendaDiaByDayOfTheWeek(int studioid, DateTime data)
        {
            SessionInfo si = await GetSessionAsync();
            var profs = await FindAllProfessorAsync();
            profs = profs.OrderBy(x => x.Relacionamento.Nome).ToList();
            List<ProfessorAgendasDia> r = new();
            var ldi = await FindAllProfessorlAgendaDiaEspecialByDatas(studioid, data, data);
            foreach (Professor p in profs)
            {
                ProfessorAgendasDia npa;
                var la = await _context.ProfessorAgenda.Where(x => x.ProfessorId == p.Id && x.StudioId == studioid && x.TenantId == si.TenantId && x.DataInicio <= data).OrderByDescending(x => x.DataInicio).ToListAsync();
                if (la.Count > 0)
                {
                    var a = la.FirstOrDefault(x => x.Id > 0);
                    npa = new()
                    {
                        Professor = p
                    };
                    r.Add(npa);
                    npa.ProfessorAgendaDias = await _context.ProfessorAgendaDia.Where(x => x.ProfessorAgendaId == a.Id && x.TenantId == si.TenantId && x.Dia == data.DayOfWeek).OrderBy(x => x.Inicio).ToListAsync();
                }
                else
                {
                    continue;
                }
                var lpdi = ldi.Where(x => x.ProfessorId == p.Id).OrderBy(x => x.Inicio).ToList();
                if (lpdi.Count > 0)
                {
                    npa.ProfessorAgendaDias.Clear();
                    foreach (var pdi in lpdi)
                    {
                        if (pdi.TemExpediente)
                        {
                            ProfessorAgendaDia pad = new()
                            {
                                Dia = data.DayOfWeek,
                                Inicio = pdi.Inicio,
                                Fim = pdi.Fim
                            };
                            npa.ProfessorAgendaDias.Add(pad);
                        }
                    }
                }
            }
            return r;
        }
        public async Task<ProfessorAgendaDia> FindProfessorAgendaDiaByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ProfessorAgendaDia.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == id);
        }
        public async Task<Resultado> DeleteProfessorAgendaDiaAsync(ProfessorAgendaDia p)
        {
            Resultado r = new();
            _context.ProfessorAgendaDia.Remove(p);
            await _context.SaveChangesAsync();
            r.Ok = true;
            r.ErrMsg = string.Empty;
            return r;
        }
        public async Task<Resultado> AddProfessorAgendaDiaAsync(ProfessorAgendaDia ap, int professorid)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            ap.TenantId = si.TenantId;

            if (await (from ProfessorAgendaDia in _context.ProfessorAgendaDia
                       join ProfessorAgenda in _context.ProfessorAgenda on ProfessorAgendaDia.ProfessorAgendaId equals ProfessorAgenda.Id
                       select new { ProfessorAgendaDia, ProfessorAgenda }
                           ).AnyAsync(x => x.ProfessorAgendaDia.TenantId == si.TenantId && x.ProfessorAgendaDia.Dia == ap.Dia && x.ProfessorAgendaDia.Fim > ap.Inicio && x.ProfessorAgendaDia.Inicio < ap.Fim && x.ProfessorAgenda.ProfessorId == professorid))
            {
                r.Ok = false;
                r.ErrMsg = "Horário com sobreposição.";
                return r;
            }

            await _context.ProfessorAgendaDia.AddAsync(ap);
            await _context.SaveChangesAsync();
            r.Ok = true;
            r.ErrMsg = string.Empty;
            return r;
        }
        public async Task<Resultado> UpdateProfessorAgendaDiaAsync(ProfessorAgendaDia ap, int professorid)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            ap.TenantId = si.TenantId;
            if (await (from ProfessorAgendaDia in _context.ProfessorAgendaDia
                       join ProfessorAgenda in _context.ProfessorAgenda on ProfessorAgendaDia.ProfessorAgendaId equals ProfessorAgenda.Id
                       select new { ProfessorAgendaDia, ProfessorAgenda }
                           ).AnyAsync(x => x.ProfessorAgendaDia.TenantId == si.TenantId && x.ProfessorAgendaDia.Dia == ap.Dia && x.ProfessorAgendaDia.Fim > ap.Inicio
                                          && x.ProfessorAgendaDia.Inicio < ap.Fim && x.ProfessorAgenda.ProfessorId == professorid && x.ProfessorAgendaDia.Id != ap.Id))
            {
                r.Ok = false;
                r.ErrMsg = "Horário com sobreposição.";
                return r;
            }
            _context.ProfessorAgendaDia.Update(ap);
            await _context.SaveChangesAsync();
            r.Ok = true;
            r.ErrMsg = string.Empty;
            return r;
        }
        public async Task<List<ProfessorAgendaDiaEspecial>> FindAllProfessorAgendaDiaEspecialByProfessorId(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from ProfessorAgendaDiaEspecial in _context.ProfessorAgendaDiaEspecial
                          join Studio in _context.Studio on ProfessorAgendaDiaEspecial.StudioId equals Studio.Id
                          select ProfessorAgendaDiaEspecial
                            ).Where(x => x.TenantId == si.TenantId && x.ProfessorId == id && x.Data >= Constante.Today).OrderBy(x => x.Data).ToListAsync();
        }
        public async Task<List<ProfessorAgendaDiaEspecial>> FindAllProfessorlAgendaDiaEspecialByDatas(int studioid, DateTime di, DateTime df)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from ProfessorAgendaDiaEspecial in _context.ProfessorAgendaDiaEspecial
                          join Studio in _context.Studio on ProfessorAgendaDiaEspecial.StudioId equals Studio.Id
                          select ProfessorAgendaDiaEspecial
                            ).Where(x => x.TenantId == si.TenantId && x.StudioId == studioid &&
                            x.Data >= di && x.Data <= df).OrderBy(x => x.Data).ToListAsync();
        }
        public async Task<ProfessorAgendaDiaEspecial> FindProfessorAgendaDiaEspecialByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ProfessorAgendaDiaEspecial.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == id);
        }
        public async Task UpdateProfessorAgendaDiaEspecialAsync(ProfessorAgendaDiaEspecial a)
        {
            if (a.Data < Constante.Today)
            {
                throw new Exception("Data inferior a data atual");
            }
            if (!a.TemExpediente)
            {
                a.Inicio = 0;
                a.Fim = 0;
            }
            _context.ProfessorAgendaDiaEspecial.Update(a);
            await _context.SaveChangesAsync();
        }
        public async Task<int> AddProfessorAgendaDiaEspecialAsync(ProfessorAgendaDiaEspecial a)
        {
            SessionInfo si = await GetSessionAsync();
            a.TenantId = si.TenantId;
            if (a.Data < Constante.Today)
            {
                throw new Exception("Data inferior a data atual");
            }
            if (!a.TemExpediente)
            {
                a.Inicio = 0;
                a.Fim = 0;
            }
            _context.ProfessorAgendaDiaEspecial.Add(a);
            await _context.SaveChangesAsync();
            return a.Id;
        }
        public async Task DeleteProfessorAgendaDiaEspecialAsync(ProfessorAgendaDiaEspecial a)
        {
            _context.ProfessorAgendaDiaEspecial.Remove(a);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Aluno>> FindAllAlunoAsync()
        {
            SessionInfo si = await GetSessionAsync();
            return await (from Aluno in _context.Aluno
                          join Relacionamento in _context.Relacionamento on Aluno.RelacionamentoId equals Relacionamento.Id
                          select new Aluno() { Id = Aluno.Id, RelacionamentoFaturaId = Aluno.RelacionamentoFaturaId, RelacionamentoId = Relacionamento.Id, Relacionamento = Relacionamento, TenantId = Aluno.TenantId }
                          ).AsNoTracking().Where(x => x.TenantId == si.TenantId && x.Relacionamento.Sistema == false).OrderBy(x => x.Relacionamento.Nome).ToListAsync();
        }
        public async Task<Aluno> FindAlunoByIdAsync(int id, bool onlycad = false)
        {
            SessionInfo si = await GetSessionAsync();
            Aluno a = await (from Aluno in _context.Aluno
                             join Relacionamento in _context.Relacionamento on Aluno.RelacionamentoId equals Relacionamento.Id
                             select new Aluno() { RelacionamentoId = Relacionamento.Id, Relacionamento = Relacionamento, TenantId = Aluno.TenantId, Id = Aluno.Id, RelacionamentoFaturaId = Aluno.RelacionamentoFaturaId }
                             ).Where(x => x.TenantId == si.TenantId && x.Id == id).FirstOrDefaultAsync();
            if (a != null && !onlycad)
            {
                a.Planos = await FindAllAlunoPlanoByAlunoIdAsync(a.Id);
            }
            return a;
        }
        public async Task<Aluno> FindAlunoByRelacionamentoIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from Aluno in _context.Aluno
                          join Relacionamento in _context.Relacionamento on Aluno.RelacionamentoId equals Relacionamento.Id
                          select new Aluno() { RelacionamentoId = Relacionamento.Id, Relacionamento = Relacionamento, TenantId = Aluno.TenantId, Id = Aluno.Id, RelacionamentoFaturaId = Aluno.RelacionamentoFaturaId }
                          ).AsNoTracking().Where(x => x.TenantId == si.TenantId && x.RelacionamentoId == id).FirstOrDefaultAsync();
        }
        public async Task<Resultado> RemoveAlunoAsync(Aluno s)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (await _context.AlunoPlano.AnyAsync(x => x.TenantId == si.TenantId && x.Aluno.Id == s.Id))
            {
                r.Ok = false;
                r.ErrMsg = "Aluno possue plano registrado, não pode ser excluído.";
                return r;
            }
            if (await _context.ProgramacaoAula.AnyAsync(x => x.TenantId == si.TenantId && x.Aluno.Id == s.Id))
            {
                r.Ok = false;
                r.ErrMsg = "Aluno possue aulas registradas, não pode ser excluído.";
                return r;
            }


            try
            {
                await _relacionamentoService.DeleteAsync(s.Relacionamento, TipoRelacionamento.Aluno);
                r.Ok = true;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<List<AlunoAusencia>> FindAllAlunoAusenciaByAlunoId(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from AlunoAusencia in _context.AlunoAusencia
                          join Aluno in _context.Aluno on AlunoAusencia.AlunoId equals Aluno.Id
                          select AlunoAusencia
                            ).Where(x => x.TenantId == si.TenantId && x.AlunoId == id).OrderByDescending(x => x.DataInicio).ToListAsync();
        }
        public async Task<AlunoAusencia> FindAlunoAusenciaByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.AlunoAusencia.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == id);
        }
        public async Task UpdateAlunoAusenciaAsync(AlunoAusencia a)
        {
            SessionInfo si = await GetSessionAsync();
            if (a.DataInicio < Constante.Today)
            {
                throw new Exception("Data inferior a data atual");
            }
            if (a.DataInicio > a.DataFinal)
            {
                throw new Exception("Data de inicio superior a data de fim");
            }
            _context.Database.BeginTransaction();
            try
            {
                var la = await _context.ProgramacaoAula.Where(x => x.TenantId == si.TenantId && x.AlunoId == a.AlunoId &&
                                                              (x.Status == StatusAula.Agendada || x.Status == StatusAula.Programada) &&
                                                              x.DataProgramada >= a.DataInicio && x.DataProgramada <= a.DataFinal).ToListAsync();
                foreach (var it in la)
                {
                    it.Status = StatusAula.ReAgendamento;
                    it.OBS = "Aluno ausente de " + a.DataInicioV + " até " + a.DataFinalV;
                    _context.ProgramacaoAula.Update(it);
                }
                _context.AlunoAusencia.Update(a);
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;
            }
        }
        public async Task<int> AddAlunoAusenciaAsync(AlunoAusencia a)
        {
            SessionInfo si = await GetSessionAsync();
            a.TenantId = si.TenantId;
            if (a.DataInicio < Constante.Today.AddMonths(-4))
            {
                throw new Exception("Data inferior a data atual");
            }
            if (a.DataInicio > a.DataFinal)
            {
                throw new Exception("Data de inicio superior a data de fim");
            }
            _context.Database.BeginTransaction();
            try
            {
                var la = await _context.ProgramacaoAula.Where(x => x.TenantId == si.TenantId && x.AlunoId == a.AlunoId &&
                                                              (x.Status == StatusAula.Agendada || x.Status == StatusAula.Programada) &&
                                                              x.DataProgramada >= a.DataInicio && x.DataProgramada <= a.DataFinal).ToListAsync();
                foreach (var it in la)
                {
                    it.Status = StatusAula.ReAgendamento;
                    it.OBS = "Aluno ausente de " + a.DataInicioV + " até " + a.DataFinalV;
                    _context.ProgramacaoAula.Update(it);
                }
                _context.AlunoAusencia.Add(a);
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;
            }
            return a.Id;
        }
        public async Task DeleteAlunoAusenciaAsync(AlunoAusencia a)
        {
            _context.AlunoAusencia.Remove(a);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Plano>> FindAllPlanoAsync()
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.Plano.Where(x => x.TenantId == si.TenantId).OrderBy(x => x.Nome).ToListAsync();
        }
        public async Task<Plano> FindPlanoByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            Plano p = await _context.Plano.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
            if (p != null)
            {
                p.Aulas = await FindAllPlanoAulaByPlanoIdAsync(id);
            }
            return p;
        }
        public async Task<Plano> AddPlanoAsync(Plano s)
        {
            SessionInfo si = await GetSessionAsync();
            s.TenantId = si.TenantId;

            _context.Database.BeginTransaction();
            try
            {
                await _context.Plano.AddAsync(s);
                await _context.SaveChangesAsync();
                /*
                                foreach (var ia in s.Aulas)
                                {
                                    PlanoAula pa = new PlanoAula()
                                    {
                                        AulaId = ia.AulaId,
                                        PlanoId = s.Id,
                                        QtdeAulas = ia.QtdeAulas,
                                        TenantId = si.TenantId,
                                        ValorAula = ia.ValorAula
                                    };
                                }
                */
                _context.Database.CommitTransaction();
            }
            catch
            {
                _context.Database.RollbackTransaction();
                throw;
            }
            return s;
        }
        public async Task<Resultado> RemovePlanoAsync(Plano s)
        {
            SessionInfo si = await GetSessionAsync();

            Resultado r = new();

            if (await _context.PlanoAula.AnyAsync(x => x.PlanoId == s.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Plano possue aulas cadastradas, não pode ser excluído.";
                return r;
            }

            if (await _context.AlunoPlano.AnyAsync(x => x.PlanoId == s.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Exite aluno vinculado a esse plano, não pode ser excluído.";
                return r;
            }

            try
            {
                _context.Plano.Remove(s);
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
        public async Task<Resultado> UpdatePlanoAsync(Plano s)
        {
            SessionInfo si = await GetSessionAsync();

            Resultado r = new();

            if (await _context.Plano.AnyAsync(x => x.Nome == s.Nome && x.Id != s.Id && x.TenantId == si.TenantId))
            {
                r.Ok = false;
                r.ErrMsg = "Nome do Plano já cadastrado.";
                return r;
            }

            try
            {
                _context.Plano.Update(s);
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
        public async Task<TabelaPreco> GetTabelaPrecoById(int id)
        {
            SessionInfo si = await GetSessionAsync();
            TabelaPreco r = new();
            _context.ChangeTracker.Clear();
            r.PlanoTabelaPreco = await _context.PlanoTabelaPreco.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
            if (r.PlanoTabelaPreco == null)
            {
                r.PlanoTabelaPreco = new()
                {
                    Ativo = true,
                    TenantId = si.TenantId,
                    DataInicio = Constante.Today
                };
            }
            r.Aulas = await _context.Aula.Where(x => x.TenantId == si.TenantId).OrderBy(x => x.Nome).ToListAsync();
            r.Planos = await _context.Plano.Where(x => x.TenantId == si.TenantId && x.Ativo == true).OrderBy(x => x.Nome).ToListAsync();
            r.PlanoAulas = await _context.PlanoAula.Where(x => x.TenantId == si.TenantId).ToListAsync();
            r.TabelaPrecos = await _context.PlanoTabelaPreco.Where(x => x.TenantId == si.TenantId).OrderByDescending(x => x.DataInicio).ToListAsync();

            if (r.TabelaPrecos.Count == 0)
            {
                r.PlanoTabelaPreco = new()
                {
                    DataInicio = Constante.Today,
                    Nome = "Tabela padrão",
                    Ativo = true,
                    TenantId = si.TenantId
                };
                _context.PlanoTabelaPreco.Add(r.PlanoTabelaPreco);
                await _context.SaveChangesAsync();
                r.TabelaPrecos.Add(r.PlanoTabelaPreco);
            }
            if (id == 0)
            {
                r.PlanoTabelaPreco = r.TabelaPrecos[0];
            }
            else
            {
                r.PlanoTabelaPreco = r.TabelaPrecos.FirstOrDefault(x => x.Id == id);
            }
            r.PlanoAulaPrecos = await _context.PlanoAulaPreco.Where(x => x.TenantId == si.TenantId && x.PlanoTabelaPrecoId == r.PlanoTabelaPreco.Id).ToListAsync();
            List<PlanoAulaPreco> lnpap = new();
            foreach (var pa in r.PlanoAulas)
            {
                var ntp = r.PlanoAulaPrecos.FirstOrDefault(x => x.PlanoAulaId == pa.Id);
                if (ntp == null)
                {
                    ntp = new()
                    {
                        PlanoAulaId = pa.Id,
                        TenantId = si.TenantId,
                        PlanoTabelaPrecoId = r.PlanoTabelaPreco.Id
                    };
                    var a = r.Aulas.FirstOrDefault(x => x.Id == pa.AulaId);
                    if (pa.ValorAula != 0)
                    {
                        ntp.ValorAula = pa.ValorAula;
                    }
                    else
                    {
                        if (a != null && !a.SemCusto)
                        {
                            ntp.ValorAula = a.ValorAvulso;
                        }
                    }
                    lnpap.Add(ntp);
                    r.PlanoAulaPrecos.Add(ntp);
                }
            }
            if (lnpap.Count > 0)
            {
                await _context.PlanoAulaPreco.AddRangeAsync(lnpap);
                await _context.SaveChangesAsync();
            }
            return r;
        }
        public async Task<List<PlanoAulaPreco>> FindAllPlanoAulaPrecoByPlanoAula(PlanoAula pa, bool nova = false)
        {
            SessionInfo si = await GetSessionAsync();
            var lt = await _context.PlanoTabelaPreco.Where(x => x.TenantId == si.TenantId && x.Ativo == true).OrderByDescending(x => x.DataInicio).ToListAsync();
            var la = await _context.Aula.Where(x => x.TenantId == si.TenantId).ToListAsync();
            Aula a = la.FirstOrDefault(x => x.Id == pa.AulaId);
            List<PlanoAulaPreco> r = await _context.PlanoAulaPreco.Where(x => x.TenantId == si.TenantId && x.PlanoAulaId == pa.Id).ToListAsync();
            foreach (var t in lt)
            {
                PlanoAulaPreco nr = r.FirstOrDefault(x => x.PlanoTabelaPrecoId == t.Id);
                if (nr == null)
                {
                    nr = new()
                    {
                        PlanoAulaId = pa.Id,
                        TenantId = si.TenantId,
                        PlanoTabelaPrecoId = t.Id,
                        ValorAula = a.ValorAvulso,
                        TabelaPrecoNome = t.Nome
                    };
                    r.Add(nr);
                }
                else
                {
                    if (nova)
                    {
                        nr.ValorAula = a.ValorAvulso;
                    }
                    nr.TabelaPrecoNome = t.Nome;
                }
            };
            return r;
        }
        public async Task<List<PlanoTabelaPreco>> FindAllPlanoTabelaPreco()
        {
            SessionInfo si = await GetSessionAsync();
            var lp = await _context.PlanoTabelaPreco.Where(x => x.TenantId == si.TenantId && x.Ativo == true).OrderByDescending(x => x.DataInicio).ToListAsync();
            if (lp.Count == 0)
            {
                PlanoTabelaPreco t = new()
                {
                    DataInicio = Constante.Today,
                    Nome = "Tabela padrão",
                    Ativo = true,
                    TenantId = si.TenantId
                };
                _context.PlanoTabelaPreco.Add(t);
                await _context.SaveChangesAsync();
                lp.Add(t);
            }
            return lp;
        }
        public async Task<Resultado> AddPlanoTabelaPreco(PlanoTabelaPreco p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            var lt = await _context.PlanoTabelaPreco.Where(x => x.TenantId == si.TenantId && x.Ativo == true).OrderByDescending(x => x.DataInicio).ToListAsync();
            List<PlanoAulaPreco> ltp = new();
            if (lt.Count > 0)
            {
                ltp = await _context.PlanoAulaPreco.Where(x => x.TenantId == si.TenantId && x.PlanoTabelaPrecoId == lt[0].Id).ToListAsync();
            }
            r.SetDefault();
            try
            {
                await _context.Database.BeginTransactionAsync();
                List<PlanoAulaPreco> lntp = new();
                p.TenantId = si.TenantId;
                await _context.PlanoTabelaPreco.AddAsync(p);
                await _context.SaveChangesAsync();
                foreach (var tp in ltp)
                {
                    PlanoAulaPreco np = new()
                    {
                        PlanoAulaId = tp.PlanoAulaId,
                        TenantId = si.TenantId,
                        PlanoTabelaPrecoId = p.Id,
                        ValorAula = tp.ValorAula
                    };
                    lntp.Add(np);
                }
                if (lntp.Count > 0)
                {
                    await _context.PlanoAulaPreco.AddRangeAsync(lntp);
                    await _context.SaveChangesAsync();
                }
                await _context.Database.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            r.Item = p;
            return r;
        }
        public async Task<Resultado> AtivarPlanoTabelaPreco(int id, bool ativar)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            r.SetDefault();
            _context.ChangeTracker.Clear();

            if (ativar)
            {
                var t = await _context.PlanoTabelaPreco.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == id);
                if (t != null)
                {
                    t.Ativo = true;
                    _context.PlanoTabelaPreco.Update(t);
                    await _context.SaveChangesAsync();
                    return r;
                }
            }
            else
            {
                var lt = await _context.PlanoTabelaPreco.Where(x => x.TenantId == si.TenantId).OrderByDescending(x => x.DataInicio).ToListAsync();
                if (lt.Count == 1)
                {
                    r.Ok = false;
                    r.ErrMsg = "Tabela não pode ser inativada, ao menos deve existir uma tabela ativa";
                    return r;
                }
                var t = lt.FirstOrDefault(x => x.Id == id);
                await _context.Database.BeginTransactionAsync();
                try
                {
                    if (t != null)
                    {
                        t.Ativo = false;
                        _context.PlanoTabelaPreco.Update(t);
                        await _context.SaveChangesAsync();
                    }
                    if (!lt.Any(x => x.Ativo == true))
                    {
                        if (lt.Count > 0)
                        {
                            var t2 = lt.FirstOrDefault(x => x.Id != t.Id);
                            t2.Ativo = true;
                            _context.PlanoTabelaPreco.Update(t2);
                            await _context.SaveChangesAsync();
                        }
                    }
                    await _context.Database.CommitTransactionAsync();
                }
                catch (Exception e)
                {
                    await _context.Database.RollbackTransactionAsync();
                    r.Ok = false;
                    r.ErrMsg = e.Message;
                }
            }
            _context.ChangeTracker.Clear();
            return r;
        }
        public async Task<Resultado> DeletePlanoTabelaPreco(PlanoTabelaPreco p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            r.SetDefault();
            _context.ChangeTracker.Clear();

            await _context.Database.BeginTransactionAsync();
            try
            {
                var l = await _context.PlanoAulaPreco.Where(x => x.PlanoTabelaPrecoId == p.Id && x.TenantId == si.TenantId).ToListAsync();
                if (l.Count > 0)
                {
                    _context.PlanoAulaPreco.RemoveRange(l);
                    await _context.SaveChangesAsync();
                }
                _context.PlanoTabelaPreco.Remove(p);
                await _context.SaveChangesAsync();

                var lt = await _context.PlanoTabelaPreco.Where(x => x.TenantId == si.TenantId).OrderByDescending(x => x.DataInicio).ToListAsync();
                if (!lt.Any(x => x.Ativo == true))
                {
                    if (lt.Count > 0)
                    {
                        lt[0].Ativo = true;
                        _context.PlanoTabelaPreco.Update(lt[0]);
                        await _context.SaveChangesAsync();
                    }
                }

                await _context.Database.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            _context.ChangeTracker.Clear();
            return r;
        }
        public async Task<Resultado> UpdatePlanoAulaPreco(PlanoAulaPreco pap)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            r.SetDefault();
            pap.TenantId = si.TenantId;
            try
            {
                if (pap.Id == 0)
                {
                    _context.PlanoAulaPreco.Add(pap);
                }
                else
                {
                    _context.PlanoAulaPreco.Update(pap);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<List<PlanoAula>> FindAllPlanoAulaByPlanoIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from PlanoAula in _context.PlanoAula
                          join Plano in _context.Plano on PlanoAula.PlanoId equals Plano.Id
                          join Aula in _context.Aula on PlanoAula.AulaId equals Aula.Id
                          select new PlanoAula()
                          {
                              Aula = Aula,
                              Plano = Plano,
                              Id = PlanoAula.Id,
                              TenantId = PlanoAula.TenantId,
                              AulaId = Aula.Id,
                              PlanoId = Plano.Id,
                              QtdeAulas = PlanoAula.QtdeAulas,
                              HorarioFixo = PlanoAula.HorarioFixo,
                              QtdeAulasSemana = PlanoAula.QtdeAulasSemana
                          }
                            ).Where(x => x.PlanoId == id && x.TenantId == si.TenantId && x.Aula.TenantId == si.TenantId).ToListAsync();
        }
        public async Task<PlanoAula> FindPlanoAulaByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from PlanoAula in _context.PlanoAula
                          join Plano in _context.Plano on PlanoAula.PlanoId equals Plano.Id
                          join Aula in _context.Aula on PlanoAula.AulaId equals Aula.Id
                          select new PlanoAula()
                          {
                              Aula = Aula,
                              Plano = Plano,
                              Id = PlanoAula.Id,
                              TenantId = PlanoAula.TenantId,
                              AulaId = Aula.Id,
                              PlanoId = Plano.Id,
                              QtdeAulas = PlanoAula.QtdeAulas,
                              HorarioFixo = PlanoAula.HorarioFixo,
                              QtdeAulasSemana = PlanoAula.QtdeAulasSemana
                          }
                            ).FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId && x.Aula.TenantId == si.TenantId);
        }
        public async Task<Resultado> DeletePlanoAula(PlanoAula p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            try
            {
                await _context.Database.BeginTransactionAsync();
                var l = await _context.PlanoAulaPreco.Where(x => x.TenantId == si.TenantId && x.PlanoAulaId == p.Id).ToListAsync();
                if (l.Count > 0)
                {
                    _context.PlanoAulaPreco.RemoveRange(l);
                    await _context.SaveChangesAsync();
                }
                _context.PlanoAula.Remove(p);
                await _context.SaveChangesAsync();
                r.Ok = true;
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
        public async Task<Resultado> AddPlanoAulaAsync(PlanoAula p, List<PlanoAulaPreco> lpap)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (await _context.PlanoAula.AnyAsync(x => x.TenantId == si.TenantId && x.PlanoId == p.PlanoId && x.AulaId == p.AulaId))
            {
                r.Ok = false;
                r.ErrMsg = "Aula já cadastrada para este plano";
                return r;
            }
            try
            {
                await _context.Database.BeginTransactionAsync();
                p.TenantId = si.TenantId;
                p.Aula = null;
                await _context.PlanoAula.AddAsync(p);
                await _context.SaveChangesAsync();
                foreach (var tp in lpap)
                {
                    tp.PlanoAulaId = p.Id;
                }
                if (lpap.Count > 0)
                {
                    await _context.PlanoAulaPreco.AddRangeAsync(lpap);
                    await _context.SaveChangesAsync();
                }
                r.Ok = true;
                r.ErrMsg = string.Empty;
                await _context.Database.CommitTransactionAsync();
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
        public async Task<Resultado> UpdatePlanoAulaAsync(PlanoAula p, List<PlanoAulaPreco> lpap)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (await _context.PlanoAula.AnyAsync(x => x.TenantId == si.TenantId && x.PlanoId == p.PlanoId && x.AulaId == p.AulaId && x.Id != p.Id))
            {
                r.Ok = false;
                r.ErrMsg = "Aula já cadastrada para este plano";
                return r;
            }

            try
            {
                _context.PlanoAula.Update(p);
                await _context.SaveChangesAsync();

                foreach (var tp in lpap)
                {
                    tp.PlanoAulaId = p.Id;
                    tp.TenantId = si.TenantId;
                    if (tp.Id == 0)
                    {
                        await _context.PlanoAulaPreco.AddAsync(tp);
                    }
                    else
                    {
                        _context.PlanoAulaPreco.Update(tp);
                    }
                }
                if (lpap.Count > 0)
                {
                    await _context.SaveChangesAsync();
                }
                r.Ok = true;
                r.ErrMsg = string.Empty;
                return r;
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<List<AlunoPlano>> FindAllAlunoPlanoByAlunoIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            var lap = await (from AlunoPlano in _context.AlunoPlano
                             join Plano in _context.Plano on AlunoPlano.PlanoId equals Plano.Id
                             select new AlunoPlano()
                             {
                                 AlunoId = AlunoPlano.AlunoId,
                                 PlanoId = Plano.Id,
                                 Id = AlunoPlano.Id,
                                 TenantId = AlunoPlano.TenantId,
                                 Plano = Plano,
                                 DataInclusao = AlunoPlano.DataInclusao,
                                 DataInicio = AlunoPlano.DataInicio,
                                 DataFim = AlunoPlano.DataFim,
                                 TipoPlano = AlunoPlano.TipoPlano,
                                 Status = AlunoPlano.Status,
                                 TokenAprovacao = AlunoPlano.TokenAprovacao
                             }
                             ).Where(x => x.AlunoId == id && x.TenantId == si.TenantId && x.Plano.TenantId == si.TenantId).OrderByDescending(x => x.DataInclusao).ToListAsync();
            foreach (var ap in lap)
            {
                ap.Aulas = await FindAllAlunoPlanoAulaByAlunoPlanoIdAsync(ap.Id);
                ap.Parcelas = await FindAllAlunoPlanoParcelaByAlunoPlanoIdAsync(ap.Id);
            }
            return lap;
        }
        public async Task<List<AlunoPlanoView>> FindAllPlanoAlunoView(DateTime dti, DateTime dtf, int alunoid, List<StatusPlanoAluno> statusplanos, int qtdaulas = 0, int qtdaulasate = 0)
        {
            SessionInfo si = await GetSessionAsync();
            List<AlunoPlanoView> r = new();

            if (dti > DateTime.MinValue || dtf > DateTime.MinValue)
            {
                var l = await (from AlunoPlano in _context.AlunoPlano
                               join Plano in _context.Plano on AlunoPlano.PlanoId equals Plano.Id
                               join Aluno in _context.Aluno on AlunoPlano.AlunoId equals Aluno.Id
                               join RelacionamentoAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelacionamentoAluno.Id
                               select new
                               {
                                   AlunoPlano,
                                   Plano,
                                   NomeAluno = RelacionamentoAluno.Nome
                               }).AsNoTracking().Where(x => x.AlunoPlano.TenantId == si.TenantId
                               &&
                               x.AlunoPlano.DataFim >= dti && x.AlunoPlano.DataFim <= dtf
                               ).ToListAsync();

                foreach (var it in l)
                {
                    AlunoPlanoView sp = new()
                    {
                        AlunoPlano = it.AlunoPlano
                    };
                    sp.AlunoPlano.NomeAluno = it.NomeAluno;
                    sp.AlunoPlano.Plano = it.Plano;
                    sp.AlunoPlano.NomeAluno = it.NomeAluno;
                    r.Add(sp);
                }

            }

            if (alunoid > 0 && statusplanos.Count == 0)
            {
                var l = await (from AlunoPlano in _context.AlunoPlano
                               join Plano in _context.Plano on AlunoPlano.PlanoId equals Plano.Id
                               join Aluno in _context.Aluno on AlunoPlano.AlunoId equals Aluno.Id
                               join RelacionamentoAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelacionamentoAluno.Id
                               select new
                               {
                                   AlunoPlano,
                                   Plano,
                                   NomeAluno = RelacionamentoAluno.Nome
                               }).AsNoTracking().Where(x => x.AlunoPlano.TenantId == si.TenantId
                               &&
                               x.AlunoPlano.AlunoId == alunoid
                               ).ToListAsync();

                foreach (var it in l)
                {
                    AlunoPlanoView sp = new()
                    {
                        AlunoPlano = it.AlunoPlano
                    };
                    sp.AlunoPlano.NomeAluno = it.NomeAluno;
                    sp.AlunoPlano.Plano = it.Plano;
                    sp.AlunoPlano.NomeAluno = it.NomeAluno;
                    r.Add(sp);
                }

            }

            if (statusplanos.Count > 0 && alunoid == 0)
            {
                var l = await (from AlunoPlano in _context.AlunoPlano
                               join Plano in _context.Plano on AlunoPlano.PlanoId equals Plano.Id
                               join Aluno in _context.Aluno on AlunoPlano.AlunoId equals Aluno.Id
                               join RelacionamentoAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelacionamentoAluno.Id
                               select new
                               {
                                   AlunoPlano,
                                   Plano,
                                   NomeAluno = RelacionamentoAluno.Nome
                               }).AsNoTracking().Where(x => x.AlunoPlano.TenantId == si.TenantId
                               &&
                               statusplanos.Contains(x.AlunoPlano.Status)
                               ).ToListAsync();

                foreach (var it in l)
                {
                    AlunoPlanoView sp = new()
                    {
                        AlunoPlano = it.AlunoPlano
                    };
                    sp.AlunoPlano.NomeAluno = it.NomeAluno;
                    sp.AlunoPlano.Plano = it.Plano;
                    sp.AlunoPlano.NomeAluno = it.NomeAluno;
                    r.Add(sp);
                }

            }

            if (statusplanos.Count > 0 && alunoid > 0)
            {
                var l = await (from AlunoPlano in _context.AlunoPlano
                               join Plano in _context.Plano on AlunoPlano.PlanoId equals Plano.Id
                               join Aluno in _context.Aluno on AlunoPlano.AlunoId equals Aluno.Id
                               join RelacionamentoAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelacionamentoAluno.Id
                               select new
                               {
                                   AlunoPlano,
                                   Plano,
                                   NomeAluno = RelacionamentoAluno.Nome
                               }).AsNoTracking().Where(x => x.AlunoPlano.TenantId == si.TenantId
                               &&
                               statusplanos.Contains(x.AlunoPlano.Status) && x.AlunoPlano.AlunoId == alunoid
                               ).ToListAsync();

                foreach (var it in l)
                {
                    AlunoPlanoView sp = new()
                    {
                        AlunoPlano = it.AlunoPlano
                    };
                    sp.AlunoPlano.NomeAluno = it.NomeAluno;
                    sp.AlunoPlano.Plano = it.Plano;
                    sp.AlunoPlano.NomeAluno = it.NomeAluno;
                    r.Add(sp);
                }

            }

            if (qtdaulas > 0)
            {

                var command = _context.Database.GetDbConnection().CreateCommand();

                command.CommandText =
    "Select Id from " +
    "(select AlunoPlano.Id, Max(AlunoPlanoAula.QtdeAulas) QtdeAulas, Sum(Case when Isnull(ProgramacaoAula.Id,0) = 0 then 0 else 1 end )  TotalAulasFeitas " +
    "from AlunoPlano " +
    "join AlunoPLanoAula on AlunoPlanoAula.AlunoPlanoId = AlunoPlano.Id " +
    "left join ProgramacaoAula on ProgramacaoAula.AlunoPlanoId = AlunoPlano.Id and ProgramacaoAula.AulaId = AlunoPLanoAula.AulaId and ProgramacaoAula.Status in (3, 7, 8) " +
    "where AlunoPlano.TipoPlano = 1 and AlunoPlano.TenantId = @tid " +
    "group by   AlunoPlano.Id, AlunoPlanoAula.Id ) AulasFeitas " +
    "group by AulasFeitas.Id " +
    "having (Sum(AulasFeitas.QtdeAulas) - Sum(TotalAulasFeitas)) <= @qtdaulasate and (Sum(AulasFeitas.QtdeAulas) - Sum(TotalAulasFeitas)) >= @qtdaulas ";

                command.Parameters.Add(new SqlParameter("@qtdaulas", System.Data.SqlDbType.Int));
                command.Parameters["@qtdaulas"].Value = qtdaulas;
                command.Parameters.Add(new SqlParameter("@qtdaulasate", System.Data.SqlDbType.Int));
                command.Parameters["@qtdaulasate"].Value = qtdaulasate;
                command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                command.Parameters["@tid"].Value = si.TenantId;

                await command.Connection.OpenAsync();
                List<int> lid = new();
                try
                {
                    var lu = await command.ExecuteReaderAsync();
                    while (await lu.ReadAsync())
                    {
                        lid.Add((int)lu["Id"]);
                    }
                    lu.Close();

                    command.Parameters.Clear();
                    command.CommandText =
    "Select Id from " +
    "(select AlunoPlano.Id, Max(AlunoPlanoAula.QtdeAulas) QtdeAulas, Sum(Case when Isnull(ProgramacaoAula.Id,0) = 0 then 0 else 1 end )  TotalAulasFeitas " +
    "from AlunoPlano " +
    "join AlunoPLanoAula on AlunoPlanoAula.AlunoPlanoId = AlunoPlano.Id " +
    "left join ProgramacaoAula on ProgramacaoAula.AlunoPlanoId = AlunoPlano.Id and ProgramacaoAula.AulaId = AlunoPLanoAula.AulaId and ProgramacaoAula.Status in (1, 2, 5, 6) " +
    "where AlunoPlano.TipoPlano = 3 " +
    "and AlunoPlano.TenantId = @tid " +
    "group by   AlunoPlano.Id, AlunoPlanoAula.Id ) AulasFeitas " +
    "group by AulasFeitas.Id " +
    "having Sum(TotalAulasFeitas) <= @qtdaulasate and  Sum(TotalAulasFeitas) >= @qtdaulas ";

                    command.Parameters.Add(new SqlParameter("@qtdaulas", System.Data.SqlDbType.Int));
                    command.Parameters["@qtdaulas"].Value = qtdaulas;
                    command.Parameters.Add(new SqlParameter("@qtdaulasate", System.Data.SqlDbType.Int));
                    command.Parameters["@qtdaulasate"].Value = qtdaulasate;
                    command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                    command.Parameters["@tid"].Value = si.TenantId;

                    lu = await command.ExecuteReaderAsync();
                    while (await lu.ReadAsync())
                    {
                        lid.Add((int)lu["Id"]);
                    }
                    lu.Close();
                    var l = await (from AlunoPlano in _context.AlunoPlano
                                   join Plano in _context.Plano on AlunoPlano.PlanoId equals Plano.Id
                                   join Aluno in _context.Aluno on AlunoPlano.AlunoId equals Aluno.Id
                                   join RelacionamentoAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelacionamentoAluno.Id

                                   select new
                                   {
                                       AlunoPlano,
                                       Plano,
                                       NomeAluno = RelacionamentoAluno.Nome
                                   }).AsNoTracking().Where(x => x.AlunoPlano.TenantId == si.TenantId
                                   &&
                                   lid.Contains(x.AlunoPlano.Id)
                                   ).ToListAsync();

                    foreach (var it in l)
                    {
                        AlunoPlanoView sp = new()
                        {
                            AlunoPlano = it.AlunoPlano
                        };
                        sp.AlunoPlano.NomeAluno = it.NomeAluno;
                        sp.AlunoPlano.Plano = it.Plano;
                        sp.AlunoPlano.NomeAluno = it.NomeAluno;
                        r.Add(sp);
                    }
                }
                catch
                {
                    command.Connection.Close();
                    throw;
                }
                finally
                {
                    command.Connection.Close();
                }
            }
            return r.OrderBy(x => x.AlunoPlano.DataFim).ToList();
        }
        public async Task<AlunoPlano> FindAlunoPlanoByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            var ap = await (from AlunoPlano in _context.AlunoPlano
                            join Plano in _context.Plano on AlunoPlano.PlanoId equals Plano.Id
                            select new
                            {
                                AlunoPlano,
                                Plano
                            }
                             ).AsNoTracking().FirstOrDefaultAsync(x => x.AlunoPlano.Id == id && x.AlunoPlano.TenantId == si.TenantId && x.Plano.TenantId == si.TenantId);
            if (ap != null)
            {
                if (ap.AlunoPlano.TokenAprovacao == null)
                {
                    ap.AlunoPlano.TokenAprovacao = string.Empty;
                }
                ap.AlunoPlano.Plano = ap.Plano;
                ap.AlunoPlano.Aulas = await FindAllAlunoPlanoAulaByAlunoPlanoIdAsync(ap.AlunoPlano.Id);
                ap.AlunoPlano.Parcelas = await FindAllAlunoPlanoParcelaByAlunoPlanoIdAsync(ap.AlunoPlano.Id);
                return ap.AlunoPlano;
            }
            return null;
        }
        public async Task<List<AlunoPlanoAula>> FindAllAlunoPlanoAulaByAlunoPlanoIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from AlunoPlanoAula in _context.AlunoPlanoAula
                          join Aula in _context.Aula on AlunoPlanoAula.AulaId equals Aula.Id
                          select new AlunoPlanoAula()
                          {
                              Id = AlunoPlanoAula.Id,
                              AlunoPlanoId = AlunoPlanoAula.AlunoPlanoId,
                              AulaId = Aula.Id,
                              Aula = Aula,
                              HorarioFixo = AlunoPlanoAula.HorarioFixo,
                              QtdeAulas = AlunoPlanoAula.QtdeAulas,
                              QtdeAulasSemana = AlunoPlanoAula.QtdeAulasSemana,
                              ValorAula = AlunoPlanoAula.ValorAula,
                              TenantId = AlunoPlanoAula.TenantId
                          }
                            ).AsNoTracking().Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId && x.Aula.TenantId == si.TenantId).OrderBy(x => x.Aula.Nome).ToListAsync();
        }
        public async Task<AlunoPlanoAula> FindAlunoPlanoAulaByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from AlunoPlanoAula in _context.AlunoPlanoAula
                          join Aula in _context.Aula on AlunoPlanoAula.AulaId equals Aula.Id
                          select new AlunoPlanoAula()
                          {
                              Id = AlunoPlanoAula.Id,
                              AlunoPlanoId = AlunoPlanoAula.AlunoPlanoId,
                              AulaId = Aula.Id,
                              Aula = Aula,
                              HorarioFixo = AlunoPlanoAula.HorarioFixo,
                              QtdeAulas = AlunoPlanoAula.QtdeAulas,
                              QtdeAulasSemana = AlunoPlanoAula.QtdeAulasSemana,
                              ValorAula = AlunoPlanoAula.ValorAula,
                              TenantId = AlunoPlanoAula.TenantId
                          }
                            ).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId && x.Aula.TenantId == si.TenantId);
        }
        public async Task<List<AlunoPlanoParcela>> FindAllAlunoPlanoParcelaByAlunoPlanoIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            _context.ChangeTracker.Clear();
            return await _context.AlunoPlanoParcela.Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId).OrderBy(x => x.DataVencto).ToListAsync();
        }
        public async Task<Resultado> UpdateAlunoPlanoAulaAsync(AlunoPlanoAula a, AlunoPlano ap, Plano p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            var command = _context.Database.GetDbConnection().CreateCommand();

            command.CommandText =

    "Update AlunoPlanoAula set  " +
    "            QtdeAulas = @QtdeAulas,  " +
    "            ValorAula = @ValorAula , " +
    "            QtdeAulasSemana = @QtdeAulasSemana, " +
    "            HorarioFixo = @HorarioFixo  " +
    " where Id = @Id  and TenantId = @tid";


            command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.Int));
            command.Parameters["@id"].Value = a.Id;
            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;


            command.Parameters.Add(new SqlParameter("@QtdeAulas", System.Data.SqlDbType.Int));
            command.Parameters["@QtdeAulas"].Value = a.QtdeAulas;

            command.Parameters.Add(new SqlParameter("@QtdeAulasSemana", System.Data.SqlDbType.Int));
            command.Parameters["@QtdeAulasSemana"].Value = a.QtdeAulasSemana;

            command.Parameters.Add(new SqlParameter("@ValorAula", System.Data.SqlDbType.Float));
            command.Parameters["@ValorAula"].Value = a.ValorAula;

            command.Parameters.Add(new SqlParameter("@HorarioFixo", System.Data.SqlDbType.Bit));
            command.Parameters["@HorarioFixo"].Value = a.HorarioFixo;

            await command.Connection.OpenAsync();
            command.Transaction = await command.Connection.BeginTransactionAsync();
            try
            {
                await command.ExecuteNonQueryAsync();
                _context.Database.UseTransaction(command.Transaction);
                _context.AlunoPlanoParcela.RemoveRange(ap.Parcelas);
                ap.Parcelas = await AjustaParcelas(p, ap);
                await _context.AlunoPlanoParcela.AddRangeAsync(ap.Parcelas);
                await _context.SaveChangesAsync();
                await command.Transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await command.Transaction.RollbackAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            finally
            {
                await command.Connection.CloseAsync();
            }
            return r;
        }
        public async Task<Resultado> AddAlunoPlanoParcelaAsync(AlunoPlanoParcela p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new() { Ok = false };
            p.TenantId = si.TenantId;
            p.DataInclusao = Constante.Today;

            await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.AlunoPlanoParcela.AddAsync(p);
                await _context.SaveChangesAsync();
                StudioOperacoesService op = new(_context, _sessionStorageService, _sessionService, Mapper, null, null);
                if (p.Status == StatusParcela.Conciliado || p.Status == StatusParcela.Pago)
                {
                    await op.ConciliarAlunoPlano(p.AlunoPlanoId);
                }
                else
                {
                    await op.AjustaValorAulaPlanoFixo(p.AlunoPlanoId, null);
                }
                _context.Database.CommitTransaction();
                r.Ok = true;
                return r;
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<ResumoFinanceiroPlano> GetResumoFinanceiroPlano(int alunoplanoid, TipoPlano tp)
        {
            SessionInfo si = await GetSessionAsync();
            ResumoFinanceiroPlano r = new();
            if (tp == TipoPlano.PeriodoValorMensal)
            {
                r.ValorTotalAulas = Math.Round(await _context.ProgramacaoAula.Where(x => x.AlunoPlanoId == alunoplanoid && x.TenantId == si.TenantId && x.Status != StatusAula.Cancelada).SumAsync(x => x.Valor), 2);
            }
            else
            {
                if (tp == TipoPlano.PacoteQtdeAula)
                {
                    r.ValorTotalAulas = Math.Round(await _context.AlunoPlanoAula.Where(x => x.AlunoPlanoId == alunoplanoid && x.TenantId == si.TenantId ).SumAsync(x => x.ValorAula * x.QtdeAulas), 2);
                }
            }
            r.ValorTotalAulasPagas = Math.Round(await _context.ProgramacaoAula.Where(x => x.AlunoPlanoId == alunoplanoid && x.TenantId == si.TenantId && x.Status != StatusAula.Cancelada).SumAsync(x => x.ValorPago), 2);
            r.ValorTotalAulasExecutadas = Math.Round(await _context.ProgramacaoAula.Where(x => x.AlunoPlanoId == alunoplanoid && x.TenantId == si.TenantId && x.Status == StatusAula.Executada).SumAsync(x => x.Valor), 2);
            r.ValorTotalParcelas = Math.Round(await _context.AlunoPlanoParcela.Where(x => x.AlunoPlanoId == alunoplanoid && x.TenantId == si.TenantId && x.Status != StatusParcela.Cancelada).SumAsync(x => x.Valor), 2);
            r.ValorTotalParcelasPagas = Math.Round(await _context.AlunoPlanoParcela.Where(x => x.AlunoPlanoId == alunoplanoid && x.TenantId == si.TenantId && (x.Status == StatusParcela.Pago || x.Status == StatusParcela.Conciliado)).SumAsync(x => x.ValorPago), 2);
            return r;
        }
        public async Task<double> GetValorParcelasAulas(int alunoplanoid)
        {
            SessionInfo si = await GetSessionAsync();
            return Math.Round(await _context.AlunoPlanoParcela.Where(x => x.AlunoPlanoId == alunoplanoid && x.TenantId == si.TenantId).DefaultIfEmpty(new() { Valor = 0 }).SumAsync(x => x.Valor), 2);
        }
        public async Task<Resultado> DeleteParcela(AlunoPlanoParcela p)
        {
            Resultado r = new() { Ok = false };

            if (p.Status != StatusParcela.Cancelada && p.Status != StatusParcela.Aberto)
            {
                r.ErrMsg = "Somente parcelas canceladas ou em aberto podem ser excluídas.";
            }

            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.AlunoPlanoParcela.Remove(p);
                await _context.SaveChangesAsync();
                StudioOperacoesService op = new(_context, _sessionStorageService, _sessionService, Mapper, null, null);
                await op.AjustaValorAulaPlanoFixo(p.AlunoPlanoId, null);
                _context.Database.CommitTransaction();
                r.Ok = true;
                return r;
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<Resultado> AddAlunoPlanoAula(AlunoPlanoAula a, AlunoPlano ap, Plano p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new() { Ok = false };
            a.Aula = null;
            a.TenantId = si.TenantId;
            await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.AlunoPlanoAula.AddAsync(a);
                await _context.SaveChangesAsync();
                _context.AlunoPlanoParcela.RemoveRange(ap.Parcelas);
                ap.Aulas.Add(a);
                ap.Parcelas = await AjustaParcelas(p, ap);
                await _context.AlunoPlanoParcela.AddRangeAsync(ap.Parcelas);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }

            r.Ok = true;
            return r;
        }
        public async Task<Resultado> UpdateAlunoPlanoParcelaAsync(AlunoPlanoParcela p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new() { Ok = false };
            p.TenantId = si.TenantId;
            await _context.Database.BeginTransactionAsync();
            try
            {
                bool conciliar = (
                    (await _context.AlunoPlanoParcela.AnyAsync(x => x.Id == p.Id && x.TenantId == p.TenantId && (x.Status == StatusParcela.Conciliado || x.Status == StatusParcela.Pago)
                    && p.Status != StatusParcela.Conciliado && p.Status != StatusParcela.Pago))
                    ||
                    (await _context.AlunoPlanoParcela.AnyAsync(x => x.Id == p.Id && x.TenantId == p.TenantId && x.Status != StatusParcela.Conciliado && x.Status != StatusParcela.Pago
                    && (p.Status == StatusParcela.Conciliado || p.Status == StatusParcela.Pago)))
                    );
                _context.AlunoPlanoParcela.Update(p);
                await _context.SaveChangesAsync();
                StudioOperacoesService op = new(_context, _sessionStorageService, _sessionService, Mapper, null, null); ;
                if (conciliar)
                {
                    await op.ConciliarAlunoPlano(p.AlunoPlanoId);
                }
                else
                {
                    await op.AjustaValorAulaPlanoFixo(p.AlunoPlanoId, null);
                }
                _context.Database.CommitTransaction();
                r.Ok = true;
                return r;
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<Resultado> DeleteAlunoPlanoAsync(AlunoPlano p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            List<AlunoPlanoParcela> lpp = await _context.AlunoPlanoParcela.Where(x => x.AlunoPlanoId == p.Id && x.TenantId == si.TenantId).ToListAsync();
            if (lpp.Any(x => x.Status != StatusParcela.Aberto))
            {
                r.Ok = false;
                r.ErrMsg = "Todas as parcelas do plano precisam estar em aberto para permitir a exclusão";
                return r;
            }
            List<ProgramacaoAula> lpa = await _context.ProgramacaoAula.Where(x => x.AlunoPlanoId == p.Id && x.TenantId == si.TenantId).ToListAsync();
            if (lpa.Any(x => x.Status == StatusAula.Executada))
            {
                r.Ok = false;
                r.ErrMsg = "Plano com aula dada, não pode ser excluído";
                return r;
            }
            List<AlunoPlanoAula> lap = await _context.AlunoPlanoAula.Where(x => x.AlunoPlanoId == p.Id && x.TenantId == si.TenantId).ToListAsync();
            List<AlunoPlanoTokenHist> lapt = await _context.AlunoPlanoTokenHist.Where(x => x.AlunoPlanoId == p.Id && x.TenantId == si.TenantId).ToListAsync();

            _context.Database.BeginTransaction();
            try
            {
                _context.AlunoPlanoAula.RemoveRange(lap);
                _context.AlunoPlanoParcela.RemoveRange(lpp);
                _context.ProgramacaoAula.RemoveRange(lpa);
                _context.AlunoPlanoTokenHist.RemoveRange(lapt);
                _context.AlunoPlano.Remove(p);
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
                r.Ok = true;
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> AddAlunoPlanoAsync(AlunoPlano ap)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            _context.Database.BeginTransaction();
            try
            {
                ap.TenantId = si.TenantId;
                ap.DataInclusao = Constante.Today;
                if (ap.Aulas == null)
                {
                    ap.Aulas = new List<AlunoPlanoAula>();
                }
                if (ap.Parcelas == null)
                {
                    ap.Parcelas = new List<AlunoPlanoParcela>();
                }
                await _context.AlunoPlano.AddAsync(ap);
                await _context.SaveChangesAsync();
                Plano p = await FindPlanoByIdAsync(ap.PlanoId);
                if (p.TipoPlano == TipoPlano.PeriodoValorMensal || p.TipoPlano == TipoPlano.PeriodoValorFixo)
                {
                    ap.Status = StatusPlanoAluno.NaoProgramado;
                }
                else
                {
                    if (p.TipoPlano == TipoPlano.PacoteQtdeAula)
                    {
                        ap.Status = StatusPlanoAluno.PendenteConfirmacao;
                    }
                }
                double vt = 0;
                foreach (AlunoPlanoAula apa in ap.Aulas)
                {
                    apa.Aula = null;
                    apa.TenantId = si.TenantId;
                    apa.AlunoPlanoId = ap.Id;
                    await _context.AlunoPlanoAula.AddAsync(apa);
                    vt += apa.ValorAula * apa.QtdeAulas;
                };

                ap.Parcelas = await AjustaParcelas(p, ap);
                /*
                                if (p.TipoPlano == TipoPlano.PacoteQtdeAula)
                                {
                                    double vp = vt / ap.QtdeParcelas;
                                    DateTime dv = Constante.Today;
                                    if (p.DiaVencto == 0)
                                    {
                                        p.DiaVencto = ap.DataInicio.Day;
                                    }
                                    int diav;
                                    for (int i = 1; i <= ap.QtdeParcelas; i++)
                                    {
                                        if (i == 1)
                                        {
                                            dv = ap.DataInicio;
                                            diav = ap.DataInicio.Day;
                                        }
                                        else
                                        {
                                            dv = ap.DataInicio.AddMonths(i - 1);
                                            diav = p.DiaVencto;
                                        }
                                        if (dv.Month == 2 && p.DiaVencto > 28)
                                        {
                                            diav = 28;
                                        }
                                        AlunoPlanoParcela app = new AlunoPlanoParcela()
                                        {
                                            AlunoPlanoId = ap.Id,
                                            DataVencto = new DateTime(dv.Year, dv.Month, diav),
                                            TenantId = si.TenantId,
                                            Valor = Math.Round(vp, 2),
                                            Status = StatusParcela.Aberto,
                                            DataInclusao = Constante.Today
                                        };
                                        await _context.AlunoPlanoParcela.AddAsync(app);
                                    }
                                }
                                else
                                {
                                    if (p.TipoPlano == TipoPlano.PeriodoValorFixo)
                                    {
                                        if (p.DiaVencto == 0)
                                        {
                                            p.DiaVencto = 1;
                                        }
                                        DateTime dv = ap.DataInicio.AddMonths(-1);
                                        double df = ap.DataInicio.Day / 30;
                                        bool f = true;
                                        while (dv <= ap.DataFim)
                                        {
                                            dv = dv.AddMonths(1);
                                            int diav = p.DiaVencto;
                                            if (dv.Month == 2 && p.DiaVencto > 28)
                                            {
                                                diav = 28;
                                            }
                                            AlunoPlanoParcela app = new AlunoPlanoParcela()
                                            {
                                                AlunoPlanoId = ap.Id,
                                                DataVencto = new DateTime(dv.Year, dv.Month, diav),
                                                TenantId = si.TenantId,
                                                Status = StatusParcela.Aberto,
                                                DataInclusao = Constante.Today
                                            };
                                            if (f)
                                            {
                                                f = false;
                                                app.Valor = Math.Round(vt * df, 2);
                                            }
                                            else
                                            {
                                                app.Valor = Math.Round(vt, 2);
                                            }
                                            await _context.AlunoPlanoParcela.AddAsync(app);
                                        }
                                    }
                                }
                */
                await _context.AlunoPlanoParcela.AddRangeAsync(ap.Parcelas);
                await _context.SaveChangesAsync();

                _context.Database.CommitTransaction();

                r.Ok = true;
                r.ErrMsg = string.Empty;
                return r;
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<List<AlunoAulaAgenda>> FindAllAlunoAulaAgendaByAlunoIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            var la = await (from AlunoAulaAgenda in _context.AlunoAulaAgenda
                            join Aula in _context.Aula on AlunoAulaAgenda.AulaId equals Aula.Id
                            join Studio in _context.Studio on AlunoAulaAgenda.StudioId equals Studio.Id
                            select new AlunoAulaAgenda()
                            {
                                AlunoId = AlunoAulaAgenda.AlunoId,
                                AulaId = Aula.Id,
                                Aula = Aula,
                                StudioId = AlunoAulaAgenda.StudioId,
                                Studio = Studio,
                                Id = AlunoAulaAgenda.Id,
                                TenantId = AlunoAulaAgenda.TenantId,
                                Dia = AlunoAulaAgenda.Dia,
                                Inicio = AlunoAulaAgenda.Inicio,
                                Fim = AlunoAulaAgenda.Fim,
                                DataInicio = AlunoAulaAgenda.DataInicio,
                                DataFim = AlunoAulaAgenda.DataFim
                            }
                            ).Where(x => x.AlunoId == id && x.TenantId == si.TenantId && x.Aula.TenantId == si.TenantId).OrderBy(x => x.Dia).ThenBy(x => x.Inicio).ToListAsync();

            foreach (var g in la)
            {
                var a = await (from AlunoAulaAgendaProfessor in _context.AlunoAulaAgendaProfessor
                               join Professor in _context.Professor on AlunoAulaAgendaProfessor.ProfessorId equals Professor.Id
                               join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                               select new
                               { AlunoAulaAgendaProfessor, Professor, Relacionamento }
                              ).FirstOrDefaultAsync(x => x.AlunoAulaAgendaProfessor.TenantId == si.TenantId && x.AlunoAulaAgendaProfessor.AlunoAulaAgendaId == g.Id);

                if (a != null)
                {
                    g.AlunoAulaAgendaProfessor = a.AlunoAulaAgendaProfessor;
                    g.AlunoAulaAgendaProfessor.Professor = a.Professor;
                    g.AlunoAulaAgendaProfessor.Professor.Relacionamento = a.Relacionamento;
                }
            }

            return la;
        }
        public async Task<List<AlunoAulaAgenda>> FindAllAlunoAulaAgendaByAlunoIdAulaIdAsync(int id, int aulaid)
        {
            SessionInfo si = await GetSessionAsync();
            var la = await (from AlunoAulaAgenda in _context.AlunoAulaAgenda
                            join Aula in _context.Aula on AlunoAulaAgenda.AulaId equals Aula.Id
                            join Studio in _context.Studio on AlunoAulaAgenda.StudioId equals Studio.Id
                            select new AlunoAulaAgenda()
                            {
                                AlunoId = AlunoAulaAgenda.AlunoId,
                                AulaId = Aula.Id,
                                Aula = Aula,
                                StudioId = AlunoAulaAgenda.StudioId,
                                Studio = Studio,
                                Id = AlunoAulaAgenda.Id,
                                TenantId = AlunoAulaAgenda.TenantId,
                                Dia = AlunoAulaAgenda.Dia,
                                Inicio = AlunoAulaAgenda.Inicio,
                                Fim = AlunoAulaAgenda.Fim,
                                DataInicio = AlunoAulaAgenda.DataInicio,
                                DataFim = AlunoAulaAgenda.DataFim
                            }
                            ).Where(x => x.AlunoId == id && x.TenantId == si.TenantId && x.Aula.TenantId == si.TenantId).OrderBy(x => x.Dia).ThenBy(x => x.Inicio).ToListAsync();

            if (aulaid != 0)
            {
                la = la.Where(x => x.AulaId == aulaid).OrderBy(x => x.Dia).ThenBy(x => x.Inicio).ToList();
            }

            foreach (var g in la)
            {
                var a = await (from AlunoAulaAgendaProfessor in _context.AlunoAulaAgendaProfessor
                               join Professor in _context.Professor on AlunoAulaAgendaProfessor.ProfessorId equals Professor.Id
                               join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                               select new
                               { AlunoAulaAgendaProfessor, Professor, Relacionamento }
                              ).FirstOrDefaultAsync(x => x.AlunoAulaAgendaProfessor.TenantId == si.TenantId && x.AlunoAulaAgendaProfessor.AlunoAulaAgendaId == g.Id);

                if (a != null)
                {
                    g.AlunoAulaAgendaProfessor = a.AlunoAulaAgendaProfessor;
                    g.AlunoAulaAgendaProfessor.Professor = a.Professor;
                    g.AlunoAulaAgendaProfessor.Professor.Relacionamento = a.Relacionamento;
                }
            }

            return la;
        }
        public async Task<List<AlunoAulaAgenda>> FindAllAlunoAulaAgendaByStudioId(int studioid)
        {
            SessionInfo si = await GetSessionAsync();
            var r = await _context.AlunoAulaAgenda.Where(x => x.StudioId == studioid && x.TenantId == si.TenantId).ToListAsync();
            foreach (var it in r)
            {
                it.AlunoAulaAgendaProfessor = await _context.AlunoAulaAgendaProfessor.Where(x => x.TenantId == si.TenantId && x.AlunoAulaAgendaId == it.Id).FirstOrDefaultAsync();
            }
            return r;
        }
        public async Task<List<AlunoAusencia>> FindAllAlunoAusenciaAsync(DateTime di, DateTime df)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.AlunoAusencia.Where(x => x.TenantId == si.TenantId && x.DataInicio >= di && x.DataFinal <= df).ToListAsync();
        }
        public async Task<List<AlunoAusencia>> FindAllAlunoAusenciaByAlunoIdAsync(int alunoid)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.AlunoAusencia.Where(x => x.TenantId == si.TenantId && x.AlunoId >= alunoid && x.DataFinal >= Constante.Today).ToListAsync();
        }
        public async Task<AlunoAulaAgenda> FindAlunoAgendaByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            var ag = await (from AlunoAulaAgenda in _context.AlunoAulaAgenda
                            join Aula in _context.Aula on AlunoAulaAgenda.AulaId equals Aula.Id
                            join Studio in _context.Studio on AlunoAulaAgenda.StudioId equals Studio.Id
                            select new AlunoAulaAgenda()
                            {
                                AlunoId = AlunoAulaAgenda.AlunoId,
                                AulaId = Aula.Id,
                                Aula = Aula,
                                StudioId = AlunoAulaAgenda.StudioId,
                                Studio = Studio,
                                Id = AlunoAulaAgenda.Id,
                                TenantId = AlunoAulaAgenda.TenantId,
                                Dia = AlunoAulaAgenda.Dia,
                                Inicio = AlunoAulaAgenda.Inicio,
                                Fim = AlunoAulaAgenda.Fim,
                                DataInicio = AlunoAulaAgenda.DataInicio,
                                DataFim = AlunoAulaAgenda.DataFim
                            }
                            ).FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId && x.Aula.TenantId == si.TenantId);
            var a = await (from AlunoAulaAgendaProfessor in _context.AlunoAulaAgendaProfessor
                           join Professor in _context.Professor on AlunoAulaAgendaProfessor.ProfessorId equals Professor.Id
                           join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                           select new
                           { AlunoAulaAgendaProfessor, Professor, Relacionamento }
                          ).FirstOrDefaultAsync(x => x.AlunoAulaAgendaProfessor.TenantId == si.TenantId && x.AlunoAulaAgendaProfessor.AlunoAulaAgendaId == ag.Id);
            if (a != null)
            {
                ag.AlunoAulaAgendaProfessor = a.AlunoAulaAgendaProfessor;
                ag.AlunoAulaAgendaProfessor.Professor = a.Professor;
                ag.AlunoAulaAgendaProfessor.Professor.Relacionamento = a.Relacionamento;
            }
            return ag;
        }
        public async Task<Resultado> DeleteAlunoAulaAgendaAsync(AlunoAulaAgenda p)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            //    if (await _context.AlunoProfessorAgendaHist.) verificar se pode deletar
            _context.Database.BeginTransaction();
            try
            {
                p.Aula = null;
                p.Studio = null;
                if (p.AlunoAulaAgendaProfessor != null)
                {
                    p.AlunoAulaAgendaProfessor.Professor = null;
                    p.AlunoAulaAgendaProfessor = null;
                }
                AlunoAulaAgendaProfessor ap = await _context.AlunoAulaAgendaProfessor.FirstOrDefaultAsync(x => x.AlunoAulaAgendaId == p.Id && x.TenantId == si.TenantId);
                if (ap != null)
                {
                    _context.AlunoAulaAgendaProfessor.Remove(ap);
                    await _context.SaveChangesAsync();
                }
                p.Aula = null;
                p.AlunoAulaAgendaProfessor = null;
                _context.AlunoAulaAgenda.Remove(p);
                await _context.SaveChangesAsync();
                _context.Database.CommitTransaction();
                r.Ok = true;
                r.ErrMsg = string.Empty;
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> AddAlunoAulaAgendaAsync(AlunoAulaAgenda ap)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            var a = await _context.Aula.FirstOrDefaultAsync(x => x.Id == ap.AulaId && x.TenantId == si.TenantId);
            DateTime di = ap.DataFim;
            if (ap.DataFim <= DateTime.MinValue)
            {
                di = DateTime.MaxValue;
            }

            DateTime dip = ap.DataInicio;
            if (ap.DataInicio <= DateTime.MinValue)
            {
                dip = DateTime.MaxValue;
            }

            var l = await (from AlunoAulaAgenda in _context.AlunoAulaAgenda
                           join AlunoAulaAgendaProfessor in _context.AlunoAulaAgendaProfessor on AlunoAulaAgenda.Id equals AlunoAulaAgendaProfessor.AlunoAulaAgendaId
                           join Studio in _context.Studio on AlunoAulaAgenda.StudioId equals Studio.Id
                           join Aula in _context.Aula on AlunoAulaAgenda.AulaId equals Aula.Id
                           select new
                           {
                               AlunoAulaAgenda,
                               AlunoAulaAgendaProfessor,
                               Aula

                           }
                             ).AsNoTracking().Where(x => x.AlunoAulaAgenda.Dia == ap.Dia
                                                      && x.AlunoAulaAgenda.Inicio < ap.Fim
                                                      && x.AlunoAulaAgenda.Fim >= ap.Inicio
                                                      && x.AlunoAulaAgendaProfessor.ProfessorId == ap.ProfessorId
                                                      && x.AlunoAulaAgenda.Id != ap.Id
                                                      && x.AlunoAulaAgenda.DataInicio <= di
                                                      && (x.AlunoAulaAgenda.DataFim >= ap.DataInicio || x.AlunoAulaAgenda.DataFim <= DateTime.MinValue)
                                                      && x.AlunoAulaAgenda.TenantId == si.TenantId).ToListAsync();

            if (l.Count >= a.QtdeMaximaAluno)
            {
                r.Ok = false;
                r.ErrMsg = "Professor indisponível para esse dia/hora, limite de aluno excedido para essa aula.";
                return r;
            }
            if (l.Count > 0)
            {
                foreach (var i in l)
                {
                    if (l.Count >= i.Aula.QtdeMaximaAluno)
                    {
                        r.Ok = false;
                        r.ErrMsg = "Professor indisponível para esse dia/hora";
                        return r;
                    }

                }
            }

            if (!await (from ProfessorAgenda in _context.ProfessorAgenda
                        join ProfessorAgendaDia in _context.ProfessorAgendaDia on ProfessorAgenda.Id equals ProfessorAgendaDia.ProfessorAgendaId
                        join Studio in _context.Studio on ProfessorAgenda.StudioId equals Studio.Id
                        select new
                        {
                            ProfessorAgenda,
                            ProfessorAgendaDia
                        }
                             ).AnyAsync(x => x.ProfessorAgendaDia.Dia == ap.Dia
                                                      && x.ProfessorAgendaDia.Inicio < ap.Fim
                                                      && x.ProfessorAgendaDia.Fim >= ap.Inicio
                                                      && x.ProfessorAgenda.ProfessorId == ap.ProfessorId
                                                      && x.ProfessorAgenda.StudioId == ap.StudioId
                                                      && x.ProfessorAgenda.DataInicio <= dip
                                                      && x.ProfessorAgenda.TenantId == si.TenantId))

            {
                r.Ok = false;
                r.ErrMsg = "Professor sem agenda aberta para esse dia/hora";
                return r;
            }

            _context.Database.BeginTransaction();
            try
            {
                ap.TenantId = si.TenantId;
                int idprof = 0;
                if (ap.AlunoAulaAgendaProfessor != null && ap.AlunoAulaAgendaProfessor.ProfessorId > 0)
                {
                    idprof = ap.AlunoAulaAgendaProfessor.ProfessorId;
                }
                ap.Aula = null;
                ap.AlunoAulaAgendaProfessor = null;
                await _context.AlunoAulaAgenda.AddAsync(ap);
                await _context.SaveChangesAsync();
                if (idprof > 0)
                {
                    AlunoAulaAgendaProfessor nap = new()
                    {
                        AlunoAulaAgendaId = ap.Id,
                        ProfessorId = idprof,
                        TenantId = si.TenantId
                    };
                    await _context.AlunoAulaAgendaProfessor.AddAsync(nap);
                    await _context.SaveChangesAsync();
                }
                _context.Database.CommitTransaction();
                r.Ok = true;
                r.ErrMsg = string.Empty;
                return r;
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<Resultado> UpdateAlunoAulaAgendaAsync(AlunoAulaAgenda ap)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            var a = await _context.Aula.FirstOrDefaultAsync(x => x.Id == ap.AulaId && x.TenantId == si.TenantId);
            DateTime di = ap.DataFim;
            if (ap.DataFim <= DateTime.MinValue)
            {
                di = DateTime.MaxValue;
            }

            DateTime dip = ap.DataInicio;
            if (ap.DataInicio <= DateTime.MinValue)
            {
                dip = DateTime.MaxValue;
            }

            var l = await (from AlunoAulaAgenda in _context.AlunoAulaAgenda
                           join AlunoAulaAgendaProfessor in _context.AlunoAulaAgendaProfessor on AlunoAulaAgenda.Id equals AlunoAulaAgendaProfessor.AlunoAulaAgendaId
                           join Studio in _context.Studio on AlunoAulaAgenda.StudioId equals Studio.Id
                           join Aula in _context.Aula on AlunoAulaAgenda.AulaId equals Aula.Id
                           select new
                           {
                               AlunoAulaAgenda,
                               AlunoAulaAgendaProfessor,
                               Aula

                           }
                             ).AsNoTracking().Where(x => x.AlunoAulaAgenda.Dia == ap.Dia
                                                      && x.AlunoAulaAgenda.Inicio < ap.Fim
                                                      && x.AlunoAulaAgenda.Fim >= ap.Inicio
                                                      && x.AlunoAulaAgendaProfessor.ProfessorId == ap.ProfessorId
                                                      && x.AlunoAulaAgenda.Id != ap.Id
                                                      && x.AlunoAulaAgenda.DataInicio <= di
                                                      && (x.AlunoAulaAgenda.DataFim >= ap.DataInicio || x.AlunoAulaAgenda.DataFim <= DateTime.MinValue)
                                                      && x.AlunoAulaAgenda.TenantId == si.TenantId).ToListAsync();

            if (l.Count >= a.QtdeMaximaAluno)
            {
                r.Ok = false;
                r.ErrMsg = "Professor indisponível para esse dia/hora, limite de aluno excedido para essa aula.";
                return r;
            }
            if (l.Count > 0)
            {
                foreach (var i in l)
                {
                    if (l.Count >= i.Aula.QtdeMaximaAluno)
                    {
                        r.Ok = false;
                        r.ErrMsg = "Professor indisponível para esse dia/hora";
                        return r;
                    }

                }
            }

            if (!await (from ProfessorAgenda in _context.ProfessorAgenda
                        join ProfessorAgendaDia in _context.ProfessorAgendaDia on ProfessorAgenda.Id equals ProfessorAgendaDia.ProfessorAgendaId
                        join Studio in _context.Studio on ProfessorAgenda.StudioId equals Studio.Id
                        select new
                        {
                            ProfessorAgenda,
                            ProfessorAgendaDia
                        }
                             ).AnyAsync(x => x.ProfessorAgendaDia.Dia == ap.Dia
                                                      && x.ProfessorAgendaDia.Inicio < ap.Fim
                                                      && x.ProfessorAgendaDia.Fim >= ap.Inicio
                                                      && x.ProfessorAgenda.ProfessorId == ap.ProfessorId
                                                      && x.ProfessorAgenda.StudioId == ap.StudioId
                                                      && x.ProfessorAgenda.DataInicio <= dip
                                                      && x.ProfessorAgenda.TenantId == si.TenantId))

            {
                r.Ok = false;
                r.ErrMsg = "Professor sem agenda aberta para esse dia/hora";
                return r;
            }

            _context.Database.BeginTransaction();
            try
            {
                ap.TenantId = si.TenantId;
                ap.Aula = null;
                int idprof = 0;
                if (ap.AlunoAulaAgendaProfessor != null && ap.AlunoAulaAgendaProfessor.ProfessorId > 0)
                {
                    idprof = ap.AlunoAulaAgendaProfessor.ProfessorId;
                    ap.AlunoAulaAgendaProfessor.Professor = null;
                    ap.AlunoAulaAgendaProfessor = null;
                }
                var oap = await _context.AlunoAulaAgendaProfessor.FirstOrDefaultAsync(x => x.AlunoAulaAgendaId == ap.Id && x.TenantId == si.TenantId);
                if (oap != null)
                {
                    _context.AlunoAulaAgendaProfessor.Remove(oap);
                    await _context.SaveChangesAsync();
                }
                if (idprof > 0)
                {
                    AlunoAulaAgendaProfessor nap = new()
                    {
                        AlunoAulaAgendaId = ap.Id,
                        ProfessorId = idprof,
                        TenantId = si.TenantId
                    };
                    await _context.AlunoAulaAgendaProfessor.AddAsync(nap);
                    await _context.SaveChangesAsync();
                }
                _context.AlunoAulaAgenda.Update(ap);
                await _context.SaveChangesAsync();

                _context.Database.CommitTransaction();
                r.Ok = true;
                r.ErrMsg = string.Empty;
                return r;
            }
            catch (Exception e)
            {
                _context.Database.RollbackTransaction();
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }
        }
        public async Task<List<ProgramacaoAula>> FindAllProgramacaoByAlunoPlanoId(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await (from ProgramacaoAula in _context.ProgramacaoAula
                          join Aula in _context.Aula on ProgramacaoAula.AulaId equals Aula.Id
                          join Professor in _context.Professor on ProgramacaoAula.ProfessorId equals Professor.Id
                          join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                          join Studio in _context.Studio on ProgramacaoAula.StudioId equals Studio.Id
                          select new ProgramacaoAula()
                          {
                              AlunoId = ProgramacaoAula.AlunoId,
                              AlunoPlanoId = ProgramacaoAula.AlunoPlanoId,
                              AulaId = ProgramacaoAula.AulaId,
                              Aula = Aula,
                              DataProgramada = ProgramacaoAula.DataProgramada,
                              HoraInicio = ProgramacaoAula.HoraInicio,
                              Id = ProgramacaoAula.Id,
                              Inicio = ProgramacaoAula.Inicio,
                              OBS = ProgramacaoAula.OBS,
                              Origem = ProgramacaoAula.Origem,
                              ProfessorId = ProgramacaoAula.ProfessorId,
                              Professor = Professor,
                              ProfessorNome = Relacionamento.Nome,
                              ProfessorRealId = ProgramacaoAula.ProfessorRealId,
                              Status = ProgramacaoAula.Status,
                              StudioId = ProgramacaoAula.StudioId,
                              Studio = Studio,
                              Valor = ProgramacaoAula.Valor,
                              ValorPago = ProgramacaoAula.ValorPago,
                              StatusFinanceiro = ProgramacaoAula.StatusFinanceiro,
                              TipoAula = ProgramacaoAula.TipoAula,
                              TenantId = ProgramacaoAula.TenantId,

                          }
                             ).AsNoTracking().Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId).OrderBy(x => x.DataProgramada).ThenBy(x => x.Inicio).ToListAsync();
        }
        public async Task<List<ProgramacaoAula>> FindAllProgramacaoByDate(int studioid, DateTime dtini, DateTime dtfim, int alunoid, int professorid, List<StatusAula> statusAula)
        {
            SessionInfo si = await GetSessionAsync();
            List<ProgramacaoAula> r = new();
            var l = await (from ProgramacaoAula in _context.ProgramacaoAula
                           join Aula in _context.Aula on ProgramacaoAula.AulaId equals Aula.Id
                           join Professor in _context.Professor on ProgramacaoAula.ProfessorId equals Professor.Id
                           join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id


                           join Professorreal in _context.Professor on ProgramacaoAula.ProfessorRealId equals Professorreal.Id into pr
                           from pr1 in pr.DefaultIfEmpty()
                           join Relacionamentoreal in _context.Relacionamento on pr1.RelacionamentoId equals Relacionamentoreal.Id into rreal
                           from rreal1 in rreal.DefaultIfEmpty()


                           join Aluno in _context.Aluno on ProgramacaoAula.AlunoId equals Aluno.Id
                           join RelAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelAluno.Id
                           join Studio in _context.Studio on ProgramacaoAula.StudioId equals Studio.Id
                           select new
                           {
                               ProgramacaoAula,
                               AulaNome = Aula.Nome,
                               Aula,
                               ProfessorNome = Relacionamento.Nome,
                               RelacionamentoId = Relacionamento.Id,
                               StudioNome = Studio.Nome,
                               ProfessorRealNome = rreal1.Nome,
                               AlunoNome = RelAluno.Nome
                           }
                             ).AsNoTracking().Where(x => x.ProgramacaoAula.DataProgramada >= dtini
                                                      && x.ProgramacaoAula.DataProgramada <= dtfim
                                                      && x.ProgramacaoAula.TenantId == si.TenantId
                                                      && x.ProgramacaoAula.StudioId == studioid

                             ).OrderBy(x => x.ProgramacaoAula.DataProgramada).ThenBy(x => x.ProgramacaoAula.Inicio).ToListAsync();

            if (alunoid != 0)
            {
                l = l.Where(x => x.ProgramacaoAula.AlunoId == alunoid).ToList();
            }
            if (professorid != 0)
            {
                l = l.Where(x => x.ProgramacaoAula.ProfessorId == professorid).ToList();
            }
            if (statusAula.Count > 0)
            {
                l = l.Where(x => statusAula.Contains(x.ProgramacaoAula.Status)).ToList();
            }

            foreach (var it in l)
            {
                it.ProgramacaoAula.Aula = new() { Id = it.ProgramacaoAula.AulaId, Nome = it.AulaNome };
                it.ProgramacaoAula.Professor = new() { Id = it.ProgramacaoAula.ProfessorId, Relacionamento = new() { Id = it.RelacionamentoId, Nome = it.ProfessorNome } };
                it.ProgramacaoAula.Studio = new() { Id = it.ProgramacaoAula.StudioId, Nome = it.StudioNome };
                it.ProgramacaoAula.Aluno = new() { Id = it.ProgramacaoAula.AlunoId, Relacionamento = new() { Nome = it.AlunoNome } };
                it.ProgramacaoAula.ProfessorReal = new() { Id = it.ProgramacaoAula.ProfessorRealId, Relacionamento = new() { Nome = it.ProfessorRealNome } };
                it.ProgramacaoAula.Aula = it.Aula;
                r.Add(it.ProgramacaoAula);
            }
            l.Clear();
            return r;
        }
        public async Task<StudioConfig> GetStudioConfig()
        {
            SessionInfo si = await GetSessionAsync();
            var r = await _context.StudioConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
            if (r == null)
            {
                r = new StudioConfig()
                {
                    TenantId = si.TenantId,
                    HdrContratoAproval = "Não configurado!!! Variaveis @aluno @plano @studio",
                    MsgPacoteHoras = "Não configurado!! Texto para aprovação de plano tipo pacote de horas",
                    FooterContratoAproval = "Não configurado",
                    StatusAulaAgendada = "#86e869",
                    StatusAulaCancelada = "#f291af",
                    StatusAulaExecutada = "#76c0eb",
                    StatusAulaNaoProgramada = "#e4da6a",
                    StatusAulaProgramada = "#cfeb86",
                    StatusAulaReAgendamento = "#e5a449",
                    StatusAulaFaltaSemReagendamento = "#e976ee",
                    StatusAulaReserva = "#d0cd91"
                };
                await _context.StudioConfig.AddAsync(r);
                await _context.SaveChangesAsync();
            }
            else
            {
                if (r.HdrContratoAproval == null)
                {
                    r.HdrContratoAproval = string.Empty;

                }
                if (r.MsgPacoteHoras == null)
                {
                    r.MsgPacoteHoras = string.Empty;

                }
                if (r.FooterContratoAproval == null)
                {
                    r.FooterContratoAproval = string.Empty;

                }
            }
            return r;
        }
        public async Task UpdateStudioConfigAsync(StudioConfig s)
        {
            _context.StudioConfig.Update(s);
            await _context.SaveChangesAsync();
        }
        protected async Task<List<AlunoPlanoParcela>> AjustaParcelas(Plano p, AlunoPlano ap)
        {
            SessionInfo si = await GetSessionAsync();
            List<AlunoPlanoParcela> r = new();

            double vt = ap.ValorTotal;
            if (ap.QtdeParcelas == 0)
            {
                ap.QtdeParcelas = 1;
            }
            double vp = Math.Round(vt / ap.QtdeParcelas, 2);
            double vt1 = vt - vp * ap.QtdeParcelas;

            if (p.TipoPlano == TipoPlano.PacoteQtdeAula)
            {
                DateTime dv;
                if (p.DiaVencto == 0)
                {
                    p.DiaVencto = ap.DataInicio.Day;
                }
                int diav;
                for (int i = 1; i <= ap.QtdeParcelas; i++)
                {
                    if (i == 1)
                    {
                        dv = ap.DataInicio;
                        diav = ap.DataInicio.Day;
                    }
                    else
                    {
                        dv = ap.DataInicio.AddMonths(i - 1);
                        diav = p.DiaVencto;
                    }
                    if (dv.Month == 2 && p.DiaVencto > 28)
                    {
                        diav = 28;
                    }
                    AlunoPlanoParcela app = new()
                    {
                        AlunoPlanoId = ap.Id,
                        DataVencto = new DateTime(dv.Year, dv.Month, diav),
                        TenantId = si.TenantId,
                        Valor = Math.Round(vp, 2),
                        Status = StatusParcela.Aberto,
                        DataInclusao = Constante.Today
                    };
                    r.Add(app);
                }
                if (r.Count > 1)
                {
                    r[0].Valor += vt1;
                }
            }
            else
            {
                if (p.TipoPlano == TipoPlano.PeriodoValorFixo)
                {
                    if (p.DiaVencto == 0)
                    {
                        p.DiaVencto = 1;
                    }
                    int qtp = 1;
                    DateTime dv = ap.DataInicio;
                    while (qtp <= p.QtdeParcelas)
                    {
                        int diav = p.DiaVencto;
                        if (dv.Month == 2 && p.DiaVencto > 28)
                        {
                            diav = 28;
                        }
                        AlunoPlanoParcela app = new()
                        {
                            AlunoPlanoId = ap.Id,
                            DataVencto = new DateTime(dv.Year, dv.Month, diav),
                            TenantId = si.TenantId,
                            Status = StatusParcela.Aberto,
                            DataInclusao = Constante.Today,
                            Valor = ap.ValorParcela
                        };
                        if (app.DataVencto < ap.DataInicio)
                        {
                            dv = UtilsClass.GetUltimo(ap.DataInicio).AddDays(10);
                            app.DataVencto = new DateTime(dv.Year, dv.Month, diav);
                        }
                        dv = UtilsClass.GetUltimo(dv).AddDays(10);
                        qtp++;
                        r.Add(app);
                    }
                }
            }
            return r;
        }
        public async Task<List<AlunoAulaAgenda>> FindAllProfessorAgendaAluno(int studioid, DateTime dia)
        {
            SessionInfo si = await GetSessionAsync();
            List<AlunoAulaAgenda> r = new();
            var lp = await
                (from AlunoAulaAgendaProfessor in _context.AlunoAulaAgendaProfessor
                 join AlunoAulaAgenda in _context.AlunoAulaAgenda on AlunoAulaAgendaProfessor.AlunoAulaAgendaId equals AlunoAulaAgenda.Id
                 join Aluno in _context.Aluno on AlunoAulaAgenda.AlunoId equals Aluno.Id
                 join Relacionamento in _context.Relacionamento on Aluno.RelacionamentoId equals Relacionamento.Id
                 select new
                 {
                     AlunoAulaAgendaProfessor,
                     AlunoAulaAgenda,
                     Aluno,
                     Relacionamento
                 }).Where(x => x.AlunoAulaAgenda.TenantId == si.TenantId && x.AlunoAulaAgenda.StudioId == studioid
                            && x.AlunoAulaAgenda.Dia == dia.DayOfWeek
                            && x.AlunoAulaAgenda.DataInicio <= dia && (x.AlunoAulaAgenda.DataFim >= dia || x.AlunoAulaAgenda.DataFim <= x.AlunoAulaAgenda.DataInicio)
                         ).ToListAsync();
            foreach (var it in lp)
            {
                it.AlunoAulaAgenda.AlunoAulaAgendaProfessor = it.AlunoAulaAgendaProfessor;
                it.AlunoAulaAgenda.Aluno = it.Aluno;
                it.AlunoAulaAgenda.Aluno.Relacionamento = it.Relacionamento;
                r.Add(it.AlunoAulaAgenda);
            }
            return r;
        }
        public async Task<List<AlunoPlanoTokenHist>> FindAllAlunoPlanoTokenHistByPlanoId(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.AlunoPlanoTokenHist.Where(x => x.TenantId == si.TenantId && x.AlunoPlanoId == id).OrderBy(x => x.DataMovto).ToListAsync();
        }
        public async Task<List<ProfessorSalario>> FindAllProfessorSalarioByProfessorId(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ProfessorSalario.Where(x => x.TenantId == si.TenantId && x.ProfessorId == id).OrderByDescending(x => x.DataInicio).ToListAsync();
        }
        public async Task<ProfessorSalario> FindProfessorSalarioByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ProfessorSalario.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == id);
        }
        public async Task UpdateProfessorSalarioAsync(ProfessorSalario a)
        {
            SessionInfo si = await GetSessionAsync();
            if (a.DataInicio == DateTime.MinValue)
            {
                a.DataInicio = Constante.Today;
            }
            if (await _context.ProfessorSalario.AnyAsync(x => x.TenantId == si.TenantId && x.ProfessorId == a.ProfessorId && x.Id != a.Id && x.DataInicio == a.DataInicio))
            {
                throw new Exception("Já existe uma Salario cadastrada com a data de " + a.DataInicio.ToString("dd/MM/yyyy"));
            }
            _context.ProfessorSalario.Update(a);
            await _context.SaveChangesAsync();
        }
        public async Task<int> AddProfessorSalarioAsync(ProfessorSalario a)
        {
            SessionInfo si = await GetSessionAsync();
            a.TenantId = si.TenantId;
            if (a.DataInicio == DateTime.MinValue)
            {
                a.DataInicio = Constante.Today;
            }
            if (await _context.ProfessorSalario.AnyAsync(x => x.TenantId == si.TenantId && x.ProfessorId == a.ProfessorId && x.Id != a.Id && x.DataInicio == a.DataInicio))
            {
                throw new Exception("Já existe uma Salario cadastrada com a data de " + a.DataInicio.ToString("dd/MM/yyyy"));
            }
            _context.ProfessorSalario.Add(a);
            await _context.SaveChangesAsync();
            return a.Id;
        }
        public async Task DeleteProfessorSalarioAsync(ProfessorSalario a)
        {
            SessionInfo si = await GetSessionAsync();
            await _context.Database.BeginTransactionAsync();
            a.Professor = null;
            try
            {
                var ld = await _context.ProfessorSalarioAula.Where(x => x.TenantId == si.TenantId && x.ProfessorSalarioId == a.Id).ToListAsync();
                if (ld.Count > 0)
                {
                    _context.ProfessorSalarioAula.RemoveRange(ld);
                }
                await _context.SaveChangesAsync();
                _context.ProfessorSalario.Remove(a);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
        public async Task<List<ProfessorSalarioAula>> FindAllProfessorSalarioAulaByProfessorSalarioIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            var ls = await (from ProfessorSalarioAula in _context.ProfessorSalarioAula
                            join ProfessorSalario in _context.ProfessorSalario on ProfessorSalarioAula.ProfessorSalarioId equals ProfessorSalario.Id
                            join Aula in _context.Aula on ProfessorSalarioAula.AulaId equals Aula.Id
                            select new { ProfessorSalarioAula, NomeAula = Aula.Nome }
                           ).Where(x => x.ProfessorSalarioAula.ProfessorSalarioId == id && x.ProfessorSalarioAula.TenantId == si.TenantId).ToListAsync();
            List<ProfessorSalarioAula> r = new();
            foreach (var i in ls)
            {
                i.ProfessorSalarioAula.NomeAula = i.NomeAula;
                r.Add(i.ProfessorSalarioAula);
            }
            return r.OrderBy(x => x.NomeAula).ToList();
        }
        public async Task<ProfessorSalarioAula> FindProfessorSalarioAulaByIdAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ProfessorSalarioAula.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == id);
        }
        public async Task<Resultado> DeleteProfessorSalarioAulaAsync(ProfessorSalarioAula p)
        {
            Resultado r = new();
            _context.ProfessorSalarioAula.Remove(p);
            await _context.SaveChangesAsync();
            r.Ok = true;
            r.ErrMsg = string.Empty;
            return r;
        }
        public async Task<Resultado> AddProfessorSalarioAulaAsync(ProfessorSalarioAula ap)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (await _context.ProfessorSalarioAula.AnyAsync(x => x.TenantId == si.TenantId && x.ProfessorSalarioId == ap.ProfessorSalarioId && x.AulaId == ap.AulaId))
            {
                r.Ok = false;
                r.ErrMsg = "Aula duplicada ";
                return r;
            }

            ap.TenantId = si.TenantId;
            await _context.ProfessorSalarioAula.AddAsync(ap);
            await _context.SaveChangesAsync();
            r.Ok = true;
            r.ErrMsg = string.Empty;
            return r;
        }
        public async Task<Resultado> UpdateProfessorSalarioAulaAsync(ProfessorSalarioAula ap)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            if (!await _context.ProfessorSalarioAula.AnyAsync(x => x.TenantId == si.TenantId && x.ProfessorSalarioId == ap.ProfessorSalarioId && x.AulaId == ap.AulaId))
            {
                r.Ok = false;
                r.ErrMsg = "Aula/remuneração não encontrada.";
                return r;
            }
            ap.TenantId = si.TenantId;
            _context.ProfessorSalarioAula.Update(ap);
            await _context.SaveChangesAsync();
            r.Ok = true;
            r.ErrMsg = string.Empty;
            return r;
        }
        public async Task<Resultado> AddStudioSala(StudioSala ss)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            try
            {
                ss.TenantId = si.TenantId;
                await _context.StudioSala.AddAsync(ss);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> UpdateStudioSala(StudioSala ss)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            try
            {
                ss.TenantId = si.TenantId;
                _context.StudioSala.Update(ss);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> DeleteStudioSala(StudioSala ss)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            var aa = await _context.AulaAgenda.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.StudioSalaId == ss.Id);
            if (aa != null)
            {
                var a = await _context.Aula.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == aa.AulaId);
                if (a != null)
                {
                    r.Ok = false;
                    r.ErrMsg = "Sala está na agenda da aula " + a.Nome + ". Não pode ser excluída";
                    return r;
                }
            }

            try
            {
                _context.StudioSala.Remove(ss);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<List<StudioSala>> FindAllStudioSala(int studioid)
        {
            SessionInfo si = await GetSessionAsync();
            List<StudioSala> r = new();
            var ls = await (from StudioSala in _context.StudioSala
                            join Studio in _context.Studio on StudioSala.StudioId equals Studio.Id
                            select new
                            {
                                StudioSala,
                                StudioNome = Studio.Nome
                            }).Where(x => x.StudioSala.TenantId == si.TenantId
                                      && (x.StudioSala.StudioId == studioid || studioid == 0))
                            .OrderBy(x => x.StudioNome).ThenBy(x => x.StudioSala.Nome).ToListAsync();
            foreach (var s in ls)
            {
                s.StudioSala.StudioNome = s.StudioNome;
                r.Add(s.StudioSala);
            }
            return r;
        }
        public Resultado ValidarAulaAgenda(AulaAgenda ag)
        {
            Resultado r = new();

            if (ag.StudioSalaId == 0)
            {
                r.Ok = false;
                r.ErrMsg = "Sala não informada";
                return r;
            }
            if (ag.ProfessorId == 0)
            {
                r.Ok = false;
                r.ErrMsg = "Professor não informado";
                return r;
            }
            r.Ok = true;
            return r;
        }
        public async Task<Resultado> AddAulaAgenda(AulaAgenda ag)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = ValidarAulaAgenda(ag);
            if (r.Ok == false)
            {
                return r;
            }
            try
            {
                ag.TenantId = si.TenantId;
                await _context.AulaAgenda.AddAsync(ag);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> UpdateAulaAgenda(AulaAgenda ag)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = ValidarAulaAgenda(ag);

            if (await _context.ProgramacaoAula.AnyAsync(x => x.TenantId == si.TenantId && x.AulaAgendaId == ag.Id))
            {
                r.Ok = false;
                r.ErrMsg = "Aula registrada não pode ser excluída";
                return r;
            }

            if (r.Ok == false)
            {
                return r;
            }
            try
            {
                ag.TenantId = si.TenantId;
                _context.AulaAgenda.Update(ag);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> DeleteAulaAgenda(AulaAgenda ag)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (await _context.ProgramacaoAula.AnyAsync(x => x.TenantId == si.TenantId && x.AulaAgendaId == ag.Id))
            {
                r.Ok = false;
                r.ErrMsg = "Aula registrada não pode ser excluída";
                return r;
            }

            var laa = await (from AulaAgendaAlunoPlanoAula in _context.AulaAgendaAlunoPlanoAula
                             join AlunoPlanoAula in _context.AlunoPlanoAula on AulaAgendaAlunoPlanoAula.AulaAgendaId equals AlunoPlanoAula.Id
                             join AlunoPlano in _context.AlunoPlano on AlunoPlanoAula.AlunoPlanoId equals AlunoPlano.Id
                             join Aluno in _context.Aluno on AlunoPlano.AlunoId equals Aluno.Id
                             join Relacionamento in _context.Relacionamento on Aluno.RelacionamentoId equals Relacionamento.Id
                             select new
                             {
                                 AulaAgendaAlunoPlanoAula,
                                 Relacionamento.Nome,
                                 AulaAgendaAlunoPlanoAula.TenantId,
                                 AulaAgendaAlunoPlanoAula.AulaAgendaId,
                                 AlunoPlano.Status

                             }
                            ).Where(x => x.TenantId == si.TenantId && x.AulaAgendaId == ag.Id).ToListAsync();
            laa = laa.Where(x => x.Status == StatusPlanoAluno.Ativo || x.Status == StatusPlanoAluno.PendenteConfirmacao).ToList();
            if (laa.Count > 0)
            {
                r.Ok = false;
                r.ErrMsg = "Essa agenda está no plano do aluno " + laa[0].Nome + ". Não pode ser excluída";
                return r;
            }

            try
            {
                await _context.Database.BeginTransactionAsync();

                if (laa.Count > 0)
                {
                    foreach (var aa in laa)
                    {
                        _context.AulaAgendaAlunoPlanoAula.Remove(aa.AulaAgendaAlunoPlanoAula);
                    }
                    await _context.SaveChangesAsync();
                }

                _context.AulaAgenda.Remove(ag);
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
        public async Task<List<AulaAgenda>> FindAgendaByAulaId(int id)
        {
            SessionInfo si = await GetSessionAsync();
            List<AulaAgenda> r = new();
            var ls = await (from AulaAgenda in _context.AulaAgenda
                            join StudioSala in _context.StudioSala on AulaAgenda.StudioSalaId equals StudioSala.Id
                            join Studio in _context.Studio on StudioSala.StudioId equals Studio.Id
                            join Aula in _context.Aula on AulaAgenda.AulaId equals Aula.Id
                            join Professor in _context.Professor on AulaAgenda.ProfessorId equals Professor.Id
                            join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                            select new
                            {
                                AulaAgenda,
                                StudioNome = Studio.Nome,
                                StudioSalaNome = StudioSala.Nome,
                                AulaNome = Aula.Nome,
                                ProfessorNome = Relacionamento.Nome
                            }).Where(x => x.AulaAgenda.TenantId == si.TenantId
                                      && (x.AulaAgenda.AulaId == id))
                            .OrderBy(x => x.StudioNome).ThenBy(x => x.AulaAgenda.Dia).ThenBy(x => x.AulaAgenda.Inicio).ToListAsync();
            foreach (var s in ls)
            {
                s.AulaAgenda.StudioNome = s.StudioNome;
                s.AulaAgenda.AulaNome = s.AulaNome;
                s.AulaAgenda.ProfessorNome = s.ProfessorNome;
                s.AulaAgenda.StudioSalaNome = s.StudioSalaNome;
                r.Add(s.AulaAgenda);
            }
            return r;
        }
        public async Task<List<AulaAgenda>> FindAgendaByStudioDia(int studioid, DateTime dia)
        {
            SessionInfo si = await GetSessionAsync();

            var StudioConfig = await GetStudioConfig();

            List<AulaAgenda> r = new();
            var ls = await (from AulaAgenda in _context.AulaAgenda
                            join StudioSala in _context.StudioSala on AulaAgenda.StudioSalaId equals StudioSala.Id
                            join Studio in _context.Studio on StudioSala.StudioId equals Studio.Id
                            join Aula in _context.Aula on AulaAgenda.AulaId equals Aula.Id
                            join Professor in _context.Professor on AulaAgenda.ProfessorId equals Professor.Id
                            join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                            select new
                            {
                                AulaAgenda,
                                StudioId = Studio.Id,
                                StudioNome = Studio.Nome,
                                StudioSalaNome = StudioSala.Nome,
                                AulaNome = Aula.Nome,
                                Aula.Cor,
                                ProfessorNome = Relacionamento.Nome
                            }).Where(x => x.StudioId == studioid
                                      && x.AulaAgenda.TenantId == si.TenantId
                                      && x.AulaAgenda.Dia == (int)dia.DayOfWeek
                                      && x.AulaAgenda.DataInicio <= dia
                                      && (x.AulaAgenda.DataFim >= dia || x.AulaAgenda.DataFim < dia.AddYears(-20)))
                            .OrderBy(x => x.StudioNome).ThenBy(x => x.AulaAgenda.Dia).ThenBy(x => x.AulaAgenda.Inicio).AsNoTracking().ToListAsync();
            foreach (var s in ls)
            {
                s.AulaAgenda.StudioNome = s.StudioNome;
                s.AulaAgenda.AulaNome = s.AulaNome;
                s.AulaAgenda.ProfessorNome = s.ProfessorNome;
                s.AulaAgenda.StudioSalaNome = s.StudioSalaNome;
                s.AulaAgenda.Cor = s.Cor;
                r.Add(s.AulaAgenda);
            }

            var laap = await (from AulaAgendaProfessor in _context.AulaAgendaProfessor
                              join AulaAgenda in _context.AulaAgenda on AulaAgendaProfessor.AulaAgendaId equals AulaAgenda.Id
                              join StudioSala in _context.StudioSala on AulaAgendaProfessor.StudioSalaId equals StudioSala.Id
                              join Studio in _context.Studio on StudioSala.StudioId equals Studio.Id
                              join Aula in _context.Aula on AulaAgenda.AulaId equals Aula.Id
                              join Professor in _context.Professor on AulaAgendaProfessor.ProfessorId equals Professor.Id
                              join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                              select new
                              {
                                  AulaAgendaProfessor,
                                  AulaAgenda,
                                  StudioId = Studio.Id,
                                  StudioNome = Studio.Nome,
                                  StudioSalaNome = StudioSala.Nome,
                                  AulaNome = Aula.Nome,
                                  Aula.Cor,
                                  ProfessorNome = Relacionamento.Nome
                              }).Where(x => x.StudioId == studioid
                                        && x.AulaAgendaProfessor.TenantId == si.TenantId
                                        && x.AulaAgendaProfessor.Dia == dia)
                            .OrderBy(x => x.StudioNome).ThenBy(x => x.AulaAgenda.Dia).ThenBy(x => x.AulaAgenda.Inicio).ToListAsync();
            foreach (var s in laap)
            {
                var aa = r.FirstOrDefault(x => x.Id == s.AulaAgendaProfessor.AulaAgendaId);
                if (aa != null)
                {
                    aa.ProfessorId = s.AulaAgendaProfessor.ProfessorId;
                    aa.ProfessorNome = s.ProfessorNome;
                    aa.StudioSalaId = s.AulaAgendaProfessor.StudioSalaId;
                    aa.StudioSalaNome = s.StudioSalaNome;
                    aa.Registro = s.AulaAgendaProfessor.Registro;
                    if (aa.Registro)
                    {
                        aa.Cor = StudioConfig.StatusAulaExecutada;
                    }
                }
            }
            return r;
        }
    }
}
