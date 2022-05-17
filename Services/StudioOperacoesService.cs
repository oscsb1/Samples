using AutoMapper;
using Blazored.SessionStorage;
using InCorpApp.Data;
using InCorpApp.Models;
using InCorpApp.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Constantes;
using InCorpApp.Utils;
using System.Data.Common;

namespace InCorpApp.Services
{
    public class StudioOperacoesService : ServiceBase
    {
        private readonly ApplicationDbContext _context;
        private readonly StudioCadastroService _cadastroService;
        private readonly ConfigService _configService;
        public StudioOperacoesService(ApplicationDbContext context,
                    ISessionStorageService sessionStorageService,
          SessionService sessionService, IMapper mapper,
          StudioCadastroService cadastroService,
           ConfigService configService
            ) : base(sessionStorageService, sessionService, mapper)
        {
            _context = context;
            _cadastroService = cadastroService;
            _configService = configService;
        }
        public async Task<List<ProgramacaoAula>> FindAllProgramacaoAulaByStudioIdDia(int studioid, DateTime dia)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ProgramacaoAula.AsNoTracking().Where(x => x.StudioId == studioid && x.DataProgramada == dia && x.TenantId == si.TenantId).OrderBy(x => x.Inicio).ToListAsync();
        }
        public async Task<ProgramacaoAula> FindProgramacaoAulaId(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ProgramacaoAula.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
        }
        public async Task<Resultado> GerarProgramacao(int studioid, DateTime inicio, DateTime fim)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            List<AgendaDia> las = await _cadastroService.FindAllAgendaDiaByDateAsync(studioid, inicio);
            List<AgendaDiaEspecial> ldes = await _cadastroService.FindAllAgendaDiaEspecialByDatas(studioid, inicio, fim);

            List<ProfessorAgendaDia> lpas = await _cadastroService.FindAllProfessorAgendaDiaByDataAsync(studioid, inicio);
            List<ProfessorAgendaDiaEspecial> lpdes = await _cadastroService.FindAllProfessorlAgendaDiaEspecialByDatas(studioid, inicio, fim);

            List<AlunoAulaAgenda> laas = await _cadastroService.FindAllAlunoAulaAgendaByStudioId(studioid);
            List<AlunoAusencia> laaus = await _cadastroService.FindAllAlunoAusenciaAsync(inicio, fim);

            List<ProgramacaoAula> Lprog = new();

            DateTime data = inicio.AddDays(-1);
            while (data < fim)
            {
                data = data.AddDays(1);
                if (ldes.Any(x => x.TemExpediente == false && x.Data == data))
                {
                    ProgramacaoAula npa1 = new()
                    {
                        DataProgramada = data,
                        StudioId = studioid,
                        Status = StatusAula.NaoProgramada,
                        OBS = "dia especial, studio sem expediente",
                        Origem = OrigemProgramacao.Sistema
                    };
                    Lprog.Add(npa1);
                    continue;
                }
                if (!las.Any(x => x.Dia == data.DayOfWeek))
                {
                    ProgramacaoAula npa2 = new()
                    {
                        DataProgramada = data,
                        StudioId = studioid,
                        Status = StatusAula.NaoProgramada,
                        OBS = "studio sem expediente",
                        Origem = OrigemProgramacao.Sistema
                    };
                    Lprog.Add(npa2);
                    continue;
                }

                List<AlunoAulaAgenda> la = laas.Where(x => x.Dia == data.DayOfWeek).ToList();
                foreach (AlunoAulaAgenda a in la)
                {
                    // verifica se o aluno registrou ausencia
                    if (laaus.Any(x => x.AlunoId == a.AlunoId && x.DataInicio <= data && x.DataFinal >= data))
                    {
                        ProgramacaoAula npa3 = new()
                        {
                            DataProgramada = data,
                            Status = StatusAula.NaoProgramada,
                            OBS = "aluno ausente",
                            AlunoId = a.AlunoId,
                            ProfessorId = a.ProfessorId,
                            StudioId = a.StudioId,
                            AulaId = a.AulaId,
                            Inicio = a.Inicio,
                            Origem = OrigemProgramacao.Sistema
                        };
                        Lprog.Add(npa3);
                        continue;
                    }
                    // verifica se o studio tem horario especial na data
                    var lde = ldes.Where(x => x.Data == data && x.TemExpediente == true).ToList();
                    if (lde.Count > 0)
                    {
                        if (!lde.Any(x => x.Inicio <= a.Inicio && x.Fim >= a.Fim))
                        {
                            ProgramacaoAula npa4 = new()
                            {
                                DataProgramada = data,
                                Status = StatusAula.NaoProgramada,
                                OBS = "studio com horário especial",
                                AlunoId = a.AlunoId,
                                ProfessorId = a.ProfessorId,
                                StudioId = a.StudioId,
                                AulaId = a.AulaId,
                                Inicio = a.Inicio,
                                Origem = OrigemProgramacao.Sistema
                            };
                            Lprog.Add(npa4);
                            continue;
                        }
                    }
                    //verfica se o professor está disponivel
                    if (lpdes.Any(x => x.TemExpediente == false && x.Data == data && x.ProfessorId == a.ProfessorId))
                    {
                        ProgramacaoAula npa5 = new()
                        {
                            DataProgramada = data,
                            Status = StatusAula.NaoProgramada,
                            OBS = "professor ausente",
                            AlunoId = a.AlunoId,
                            ProfessorId = a.ProfessorId,
                            StudioId = a.StudioId,
                            AulaId = a.AulaId,
                            Inicio = a.Inicio,
                            Origem = OrigemProgramacao.Sistema
                        };
                        Lprog.Add(npa5);
                        continue;
                    }
                    if (!lpas.Any(x => x.Dia == data.DayOfWeek && x.ProfessorAgenda.ProfessorId == a.ProfessorId))
                    {
                        ProgramacaoAula npa6 = new()
                        {
                            DataProgramada = data,
                            Status = StatusAula.NaoProgramada,
                            OBS = "professor sem expediente",
                            AlunoId = a.AlunoId,
                            ProfessorId = a.ProfessorId,
                            StudioId = a.StudioId,
                            AulaId = a.AulaId,
                            Inicio = a.Inicio,
                            Origem = OrigemProgramacao.Sistema
                        };
                        Lprog.Add(npa6);
                        continue;
                    }
                    var lpde = lpdes.Where(x => x.Data == data && x.TemExpediente == true && x.ProfessorId == a.ProfessorId).ToList();
                    if (lpde.Count > 0)
                    {
                        if (!lpde.Any(x => x.Inicio <= a.Inicio && x.Fim >= a.Fim))
                        {
                            ProgramacaoAula npa7 = new()
                            {
                                DataProgramada = data,
                                Status = StatusAula.NaoProgramada,
                                OBS = "professor com horário especial",
                                AlunoId = a.AlunoId,
                                ProfessorId = a.ProfessorId,
                                StudioId = a.StudioId,
                                AulaId = a.AulaId,
                                Inicio = a.Inicio,
                                Origem = OrigemProgramacao.Sistema
                            };
                            Lprog.Add(npa7);
                            continue;
                        }
                    }
                    if (!lpas.Any(x => x.Dia == data.DayOfWeek && x.ProfessorAgenda.ProfessorId == a.ProfessorId && x.Inicio <= a.Inicio && x.Fim >= x.Fim))
                    {
                        ProgramacaoAula npa8 = new()
                        {
                            DataProgramada = data,
                            Status = StatusAula.NaoProgramada,
                            OBS = "professor sem agenda para essa hora",
                            AlunoId = a.AlunoId,
                            ProfessorId = a.ProfessorId,
                            StudioId = a.StudioId,
                            AulaId = a.AulaId,
                            Inicio = a.Inicio,
                            Origem = OrigemProgramacao.Sistema
                        };
                        Lprog.Add(npa8);
                        continue;
                    }
                    ProgramacaoAula npa9 = new()
                    {
                        DataProgramada = data,
                        Status = StatusAula.Programada,
                        AlunoId = a.AlunoId,
                        ProfessorId = a.ProfessorId,
                        StudioId = a.StudioId,
                        AulaId = a.AulaId,
                        Inicio = a.Inicio,
                        Origem = OrigemProgramacao.Sistema
                    };
                    Lprog.Add(npa9);
                }
            }

            _context.Database.BeginTransaction();
            try
            {

                var patul = await _context.ProgramacaoAula.Where(x => x.TenantId == si.TenantId && x.StudioId == studioid && x.DataProgramada >= inicio && x.DataProgramada <= fim && (x.Status == StatusAula.Programada || x.Status == StatusAula.NaoProgramada)).ToListAsync();
                _context.RemoveRange(patul);
                await _context.SaveChangesAsync();
                foreach (var it in Lprog)
                {
                    it.Aluno = null;
                    it.Aula = null;
                    it.Professor = null;
                    it.ProfessorReal = null;
                    it.Studio = null;
                    it.Tenant = null;
                    it.TenantId = si.TenantId;
                    it.TipoAula = TipoAula.Plano;
                    _context.ProgramacaoAula.Add(it);
                }
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
        public async Task<Resultado> GerarProgramacaoAlunoAsync(List<int> lstudioid, int alunoid, int alunoplanoid)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            if (await _context.AlunoPlanoParcela.AnyAsync(x => x.AlunoPlanoId == alunoplanoid && x.TenantId == si.TenantId && x.Status != StatusParcela.Aberto && x.Status != StatusParcela.Cancelada))
            {
                r.Ok = false;
                r.ErrMsg = "Todas as parcelas precisam estar em aberto ou canceladas para reprogramar esse plano";
                return r;
            }

            List<AlunoPlano> lap = await _context.AlunoPlano.Where(x => x.AlunoId == alunoid && x.TenantId == si.TenantId && (x.Status == StatusPlanoAluno.NaoProgramado || x.Status == StatusPlanoAluno.PendenteConfirmacao) && x.Id == alunoplanoid).OrderBy(x => x.DataInicio).ToListAsync();
            if (lap.Any(x => x.Status == StatusPlanoAluno.Cancelado))
            {
                r.Ok = false;
                r.ErrMsg = "Plano está cancelado";
                return r;
            }
            if (lap.Any(x => x.Status == StatusPlanoAluno.Ativo))
            {
                r.Ok = false;
                r.ErrMsg = "Plano está ativo, alterar a agenda e parcelas manualmente!";
                return r;
            }
            if (await _context.ProgramacaoAula.AnyAsync(x => x.TenantId == si.TenantId && x.AlunoPlanoId == alunoplanoid && x.Status != StatusAula.Programada && x.Status != StatusAula.NaoProgramada && x.Status != StatusAula.Reserva))
            {
                r.Ok = false;
                r.ErrMsg = "Todas a aulas precisam estar na situação de programada para reprogramar este plano. Alterar a agenda e parcelas manualmente!";
                return r;
            }
            if (await _context.ProgramacaoAula.AnyAsync(x => x.TenantId == si.TenantId && x.AlunoPlanoId == alunoplanoid && x.Status != StatusAula.Programada && x.Status != StatusAula.NaoProgramada))
            {
                r.Ok = false;
                r.ErrMsg = "Operação não permitida para plano com aulas alteradas manualmente. Alterar a agenda e parcelas manualmente!";
                return r;
            }
            List<AlunoAusencia> laaus = await _cadastroService.FindAllAlunoAusenciaByAlunoIdAsync(alunoid);
            List<ProgramacaoAula> Lprog = new();
            try
            {
                foreach (var ap in lap)
                {
                    if (ap.Plano == null)
                    {
                        ap.Plano = await _context.Plano.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == ap.PlanoId);
                    }
                    List<AlunoPlanoAula> lapa = await _cadastroService.FindAllAlunoPlanoAulaByAlunoPlanoIdAsync(ap.Id);
                    lapa = lapa.Where(x => x.Aula.AulaGrupo == false).ToList();
                    foreach (int studioid in lstudioid)
                    {
                        List<AgendaDia> las = null;
                        List<AgendaDiaEspecial> ldes = null;
                        List<ProfessorAgendaDia> lpas = null;
                        List<ProfessorAgendaDiaEspecial> lpdes = null;

                        foreach (AlunoPlanoAula apa in lapa)
                        {
                            if (ap.Plano.TipoPlano == TipoPlano.PacoteQtdeAula && !apa.HorarioFixo)
                            {
                                continue;
                            }
                            List<AlunoAulaAgenda> laasx = await _cadastroService.FindAllAlunoAulaAgendaByAlunoIdAulaIdAsync(alunoid, apa.AulaId);
                            if (laasx.Count == 0)
                            {
                                r.Ok = false;
                                r.ErrMsg = "Aluno sem agenda para a aula " + apa.NomeAula;
                                return r;
                            }
                            List<AlunoAulaAgenda> laas = laasx.Where(x => x.StudioId == studioid).ToList();
                            if (las == null)
                            {
                                las = await _cadastroService.FindAllAgendaDiaByDateAsync(studioid, ap.DataInicio);
                                ldes = await _cadastroService.FindAllAgendaDiaEspecialByDatas(studioid, ap.DataInicio, Constante.Today.AddMonths(24));
                                lpas = await _cadastroService.FindAllProfessorAgendaDiaByDataAsync(studioid, ap.DataInicio);
                                lpdes = await _cadastroService.FindAllProfessorlAgendaDiaEspecialByDatas(studioid, ap.DataInicio, Constante.Today.AddMonths(24));
                            }
                            DateTime data = ap.DataInicio.AddDays(-1);
                            while (data < ap.DataFim)
                            {
                                data = data.AddDays(1);
                                List<AlunoAulaAgenda> la = laas.Where(x => x.Dia == data.DayOfWeek && x.DataInicio <= data && (x.DataFim >= data || x.DataFim <= x.DataInicio)).ToList();
                                foreach (AlunoAulaAgenda a in la)
                                {
                                    if (await _context.ProgramacaoAula.AnyAsync(x => x.TenantId == si.TenantId &&
                                                       x.AlunoId == ap.AlunoId && x.DataProgramada == data && x.Inicio == a.Inicio && x.AlunoPlanoId != ap.Id &&
                                                       x.Status != StatusAula.Cancelada && x.Status != StatusAula.FaltaSemReagendamento && x.Status != StatusAula.ReAgendamento))
                                    {
                                        continue;
                                    }
                                    if (ldes.Any(x => x.TemExpediente == false && x.Data == data))
                                    {
                                        continue;
                                        /*
                                        ProgramacaoAula npa1 = new ProgramacaoAula()
                                        {
                                            DataProgramada = data,
                                            Status = StatusAula.NaoProgramada,
                                            OBS = "dia especial, studio sem expediente",
                                            AlunoId = a.AlunoId,
                                            ProfessorId = a.ProfessorId,
                                            StudioId = a.StudioId,
                                            AlunoPlanoId = ap.Id,
                                            AulaId = a.AulaId,
                                            Inicio = a.Inicio,
                                            Valor = apa.ValorAula,
                                            Origem = OrigemProgramacao.Sistema
                                        };
                                        Lprog.Add(npa1);
                                        continue;
                                        */
                                    }
                                    if (!las.Any(x => x.Dia == data.DayOfWeek))
                                    {
                                        continue;
                                        /*
                                        ProgramacaoAula npa2 = new ProgramacaoAula()
                                        {
                                            DataProgramada = data,
                                            Status = StatusAula.NaoProgramada,
                                            OBS = "studio sem expediente",
                                            AlunoId = a.AlunoId,
                                            ProfessorId = a.ProfessorId,
                                            StudioId = a.StudioId,
                                            AlunoPlanoId = ap.Id,
                                            AulaId = a.AulaId,
                                            Inicio = a.Inicio,
                                            Valor = apa.ValorAula,
                                            Origem = OrigemProgramacao.Sistema
                                        };
                                        Lprog.Add(npa2);
                                        continue;
                                        */
                                    }
                                    // verifica se o aluno registrou ausencia
                                    var aaus = laaus.FirstOrDefault(x => x.AlunoId == a.AlunoId && x.DataInicio <= data && x.DataFinal >= data);
                                    if (aaus != null)
                                    {
                                        ProgramacaoAula npa3 = new()
                                        {
                                            DataProgramada = data,
                                            Status = StatusAula.Reserva,
                                            AlunoId = a.AlunoId,
                                            ProfessorId = a.ProfessorId,
                                            StudioId = a.StudioId,
                                            AulaId = a.AulaId,
                                            AlunoPlanoId = ap.Id,
                                            Inicio = a.Inicio,
                                            Fim = a.Inicio + a.Aula.Duracao,
                                            Origem = OrigemProgramacao.Sistema
                                        };
                                        if (aaus.CobrarReservaHorario)
                                        {
                                            npa3.Valor = Math.Round(apa.ValorAula * aaus.Percentual / 100, 2);
                                            npa3.OBS = "aluno ausente, com reserva de horario";
                                        }
                                        else
                                        {
                                            npa3.Valor = 0;
                                            npa3.OBS = "Aluno ausente, sem reserva de horario";
                                            r.ListErrMsg.Add("Aluno ausente, sem reserva de horario dia " + data.ToString("dd/MM/yyyy"));
                                        }
                                        if (ap.Plano.TipoPlano == TipoPlano.PeriodoValorMensal && aaus.CobrarReservaHorario)
                                        {
                                            Lprog.Add(npa3);
                                        }
                                        continue;
                                    }
                                    // verifica se o studio tem horario especial na data
                                    var lde = ldes.Where(x => x.Data == data && x.TemExpediente == true).ToList();
                                    if (lde.Count > 0)
                                    {
                                        if (!lde.Any(x => x.Inicio <= a.Inicio && x.Fim >= a.Fim))
                                        {
                                            /*
                                            ProgramacaoAula npa4 = new ProgramacaoAula()
                                            {
                                                DataProgramada = data,
                                                Status = StatusAula.NaoProgramada,
                                                OBS = "studio com horário especial",
                                                AlunoId = a.AlunoId,
                                                ProfessorId = a.ProfessorId,
                                                StudioId = a.StudioId,
                                                AulaId = a.AulaId,
                                                AlunoPlanoId = ap.Id,
                                                Inicio = a.Inicio,
                                                Valor = apa.ValorAula,
                                                Origem = OrigemProgramacao.Sistema
                                            };
                                            Lprog.Add(npa4);
                                            */
                                            continue;
                                        }
                                    }
                                    //verfica se o professor está disponivel
                                    if (lpdes.Any(x => x.TemExpediente == false && x.Data == data && x.ProfessorId == a.ProfessorId))
                                    {
                                        ProgramacaoAula npa5 = new()
                                        {
                                            DataProgramada = data,
                                            Status = StatusAula.NaoProgramada,
                                            OBS = "professor ausente",
                                            AlunoId = a.AlunoId,
                                            ProfessorId = a.ProfessorId,
                                            StudioId = a.StudioId,
                                            AulaId = a.AulaId,
                                            AlunoPlanoId = ap.Id,
                                            Inicio = a.Inicio,
                                            Fim = a.Inicio + a.Aula.Duracao,
                                            Valor = apa.ValorAula,
                                            Origem = OrigemProgramacao.Sistema
                                        };
                                        r.ListErrMsg.Add("Professor ausente dia " + data.ToString("dd/MM/yyyy"));
                                        Lprog.Add(npa5);
                                        continue;
                                    }
                                    if (!lpas.Any(x => x.Dia == data.DayOfWeek && x.ProfessorAgenda.ProfessorId == a.ProfessorId))
                                    {
                                        throw new Exception("Professor sem expediente para o dia " + data.ToString("dd/MM/yyyy") + " - " + Constante.DiaSemana[(int)data.DayOfWeek].ToLower() + " " + a.HoraInicioV);
                                        /*
                                        ProgramacaoAula npa6 = new ProgramacaoAula()
                                        {
                                            DataProgramada = data,
                                            Status = StatusAula.NaoProgramada,
                                            OBS = "professor sem expediente",
                                            AlunoId = a.AlunoId,
                                            ProfessorId = a.ProfessorId,
                                            StudioId = a.StudioId,
                                            AulaId = a.AulaId,
                                            AlunoPlanoId = ap.Id,
                                            Inicio = a.Inicio,
                                            Valor = apa.ValorAula,
                                            Origem = OrigemProgramacao.Sistema
                                        };
                                        Lprog.Add(npa6);
                                        continue;
                                      */
                                    }
                                    var lpde = lpdes.Where(x => x.Data == data && x.TemExpediente == true && x.ProfessorId == a.ProfessorId).ToList();
                                    if (lpde.Count > 0)
                                    {
                                        if (!lpde.Any(x => x.Inicio <= a.Inicio && x.Fim >= a.Fim))
                                        {
                                            ProgramacaoAula npa7 = new()
                                            {
                                                DataProgramada = data,
                                                Status = StatusAula.NaoProgramada,
                                                OBS = "professor com horário especial",
                                                AlunoId = a.AlunoId,
                                                ProfessorId = a.ProfessorId,
                                                StudioId = a.StudioId,
                                                AulaId = a.AulaId,
                                                AlunoPlanoId = ap.Id,
                                                Inicio = a.Inicio,
                                                Fim = a.Inicio + a.Aula.Duracao,
                                                Valor = apa.ValorAula,
                                                Origem = OrigemProgramacao.Sistema
                                            };
                                            r.ListErrMsg.Add("Professor com horário especial dia " + data.ToString("dd/MM/yyyy"));
                                            Lprog.Add(npa7);
                                            continue;
                                        }
                                    }
                                    if (!lpas.Any(x => x.Dia == data.DayOfWeek && x.ProfessorAgenda.ProfessorId == a.ProfessorId && x.Inicio <= a.Inicio && x.Fim >= a.Fim))
                                    {
                                        throw new Exception("Professor sem expediente para o dia " + data.ToString("dd/MM/yyyy") + " - " + Constante.DiaSemana[(int)data.DayOfWeek].ToLower() + " " + a.HoraInicioV);
                                        /*
                                        ProgramacaoAula npa8 = new ProgramacaoAula()
                                        {
                                            DataProgramada = data,
                                            Status = StatusAula.NaoProgramada,
                                            OBS = "professor sem agenda para essa hora",
                                            AlunoId = a.AlunoId,
                                            ProfessorId = a.ProfessorId,
                                            StudioId = a.StudioId,
                                            AulaId = a.AulaId,
                                            AlunoPlanoId = ap.Id,
                                            Inicio = a.Inicio,
                                            Valor = apa.ValorAula,
                                            Origem = OrigemProgramacao.Sistema
                                        };
                                        Lprog.Add(npa8);
                                        continue;
                                        */
                                    }
                                    if (await _context.ProgramacaoAula.AnyAsync(x => x.TenantId == si.TenantId && x.ProfessorId == a.ProfessorId && x.DataProgramada == data && x.Fim > a.Inicio && x.Inicio < a.Fim && x.AlunoId != a.AlunoId && x.AulaId != a.AulaId && x.Status != StatusAula.Cancelada && x.Status != StatusAula.Reserva))
                                    {
                                        r.ListErrMsg.Add("Conflito de agenda no dia " + data.ToString("dd/MM/yyyy"));
                                        continue;
                                    }
                                    ProgramacaoAula npa9 = new()
                                    {
                                        DataProgramada = data,
                                        Status = StatusAula.Programada,
                                        AlunoId = a.AlunoId,
                                        ProfessorId = a.ProfessorId,
                                        StudioId = a.StudioId,
                                        AulaId = a.AulaId,
                                        AlunoPlanoId = ap.Id,
                                        Inicio = a.Inicio,
                                        Fim = a.Inicio + a.Aula.Duracao,
                                        Valor = apa.ValorAula,
                                        Origem = OrigemProgramacao.Sistema
                                    };
                                    Lprog.Add(npa9);
                                    if (ap.Plano.TipoPlano == TipoPlano.PacoteQtdeAula && apa.QtdeAulas == Lprog.Count)
                                    {
                                        data = DateTime.MaxValue;
                                    }
                                    //   break;  sem o break permite agendar a mesma aula 2 vezes no dia
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                return r;
            }

            List<AlunoPlanoParcela> lpar = new();
            foreach (AlunoPlano pl in lap)
            {
                if (pl.Plano.TipoPlano == TipoPlano.PeriodoValorMensal)
                {
                    var q = Lprog.Where(x => x.AlunoPlanoId == pl.Id).GroupBy(x => x.DataProgramada.ToString("MM/yyyy"), (mes, linhas) => new { Key = mes, Valor = linhas.Sum(x => x.Valor) });
                    int diavencto = pl.DiaVencto;
                    foreach (var p in q)
                    {
                        int m = int.Parse(p.Key.Substring(0, 2));
                        int y = int.Parse(p.Key.Substring(3, 4));
                        int d = 0;
                        if (diavencto == 0)
                        {
                            d = pl.DataInicio.Day;
                        }
                        else
                        {
                            d = diavencto;
                        }
                        if (m == 2 && d > 28)
                        {
                            d = 28;
                        }
                        AlunoPlanoParcela par = new()
                        {
                            DataVencto = new DateTime(y, m, d),
                            AlunoPlanoId = pl.Id,
                            Valor = p.Valor,
                            Status = StatusParcela.Aberto,
                            TenantId = si.TenantId
                        };
                        lpar.Add(par);
                    }
                }
            }

            _context.Database.BeginTransaction();
            try
            {
                foreach (AlunoPlano pl in lap)
                {
                    var patul = await _context.ProgramacaoAula.Where(x => x.TenantId == si.TenantId && x.AlunoId == alunoid && x.AlunoPlanoId == pl.Id).ToListAsync();
                    _context.RemoveRange(patul);
                    if (pl.Plano.TipoPlano == TipoPlano.PeriodoValorMensal)
                    {
                        var parx = await _context.AlunoPlanoParcela.Where(x => x.TenantId == si.TenantId && x.AlunoPlanoId == pl.Id).ToListAsync();
                        _context.RemoveRange(parx);
                    }
                    await _context.SaveChangesAsync();
                }
                foreach (var it in Lprog)
                {
                    it.Aluno = null;
                    it.Aula = null;
                    it.Professor = null;
                    it.ProfessorReal = null;
                    it.Studio = null;
                    it.Tenant = null;
                    it.TenantId = si.TenantId;
                    it.TipoAula = TipoAula.Plano;
                    _context.ProgramacaoAula.Add(it);
                }
                if (lpar.Count > 0)
                {
                    _context.AlunoPlanoParcela.AddRange(lpar);
                }

                await AjustaValorAulaPlanoFixo(alunoplanoid, Lprog);

                foreach (AlunoPlano pl in lap)
                {
                    pl.Status = StatusPlanoAluno.PendenteConfirmacao;
                    _context.AlunoPlano.Update(pl);
                }
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
        public async Task<Resultado> ReProgramarPlano(int alunoid, int alunoaplanoid, DateTime dtinicio)
        {
            Resultado r = new();
            SessionInfo si = await GetSessionAsync();

            List<ProgramacaoAula> lpa = await _context.ProgramacaoAula.Where(x => x.TenantId == si.TenantId && x.AlunoPlanoId == alunoaplanoid && x.DataProgramada >= dtinicio
                                                                                  && x.Status != StatusAula.Executada
                                                                                  && x.Status != StatusAula.FaltaSemReagendamento
                                                                                  && x.Status != StatusAula.Cancelada).OrderBy(x => x.AulaId).ThenBy(x => x.DataProgramada).ToListAsync();
            if (lpa.Count == 0)
            {
                r.Ok = false;
                r.ErrMsg = "Não existem aulas para reprogramar após " + dtinicio.ToString("dd/MM/yyyy");
                return r;
            }
            ProgramacaoAula firstpa = lpa[0];
            List<AlunoAulaAgenda> laas = await _cadastroService.FindAllAlunoAulaAgendaByAlunoIdAulaIdAsync(alunoid, 0);
            List<AlunoAusencia> laaus = await _cadastroService.FindAllAlunoAusenciaByAlunoIdAsync(alunoid);
            List<AgendaDia> las = null;
            List<AgendaDiaEspecial> ldes = null;
            List<ProfessorAgendaDia> lpas = null;
            List<ProfessorAgendaDiaEspecial> lpdes = null;
            if (las == null)
            {
                las = await _cadastroService.FindAllAgendaDiaByDateAsync(firstpa.StudioId, Constante.Today);
                ldes = await _cadastroService.FindAllAgendaDiaEspecialByDatas(firstpa.StudioId, Constante.Today, Constante.Today.AddMonths(24));
                lpas = await _cadastroService.FindAllProfessorAgendaDiaByDataAsync(firstpa.StudioId, Constante.Today);
                lpdes = await _cadastroService.FindAllProfessorlAgendaDiaEspecialByDatas(firstpa.StudioId, Constante.Today, Constante.Today.AddMonths(24));
            }

            List<AlunoPlanoAula> lapa = await _cadastroService.FindAllAlunoPlanoAulaByAlunoPlanoIdAsync(alunoaplanoid);
            AlunoPlanoAula aulav = null;
            int aulaidant = 0;
            DateTime data = dtinicio;
            bool programou = false;
            try
            {

                foreach (ProgramacaoAula pa in lpa)
                {
                    if (pa.Status == StatusAula.Reserva && pa.DataProgramada <= Constante.Today)
                    {
                        continue;
                    }
                    if (aulaidant != pa.AulaId)
                    {
                        if (dtinicio < Constante.Today)
                        {
                            data = Constante.Today.AddDays(-1);
                        }
                        else
                        {
                            data = dtinicio.AddDays(-1);
                        }
                        aulaidant = pa.AulaId;
                    }
                    programou = false;
                    while (data < Constante.Today.AddYears(2) && !programou)
                    {
                        data = data.AddDays(1);
                        List<AlunoAulaAgenda> la = laas.Where(x => x.Dia == data.DayOfWeek && x.DataInicio <= data && (x.DataFim >= data || x.DataFim <= x.DataInicio)).ToList();
                        foreach (AlunoAulaAgenda a in la)
                        {
                            var outropa = await _context.ProgramacaoAula.FirstOrDefaultAsync(x => x.TenantId == si.TenantId &&
                                               x.AlunoId == firstpa.AlunoId && x.DataProgramada == data && x.Inicio == a.Inicio &&
                                               x.Status != StatusAula.Cancelada && x.Status != StatusAula.FaltaSemReagendamento && x.Status != StatusAula.ReAgendamento);
                            if (outropa != null)
                            {
                                if (!lpa.Any(x => x.Id == outropa.Id))
                                {
                                    continue;
                                }
                            }
                            if (ldes.Any(x => x.TemExpediente == false && x.Data == data))
                            {
                                continue;
                            }
                            if (!las.Any(x => x.Dia == data.DayOfWeek))
                            {
                                continue;
                            }
                            // verifica se o aluno registrou ausencia
                            var aaus = laaus.FirstOrDefault(x => x.AlunoId == a.AlunoId && x.DataInicio <= data && x.DataFinal >= data);
                            if (aaus != null && aaus.CobrarReservaHorario)
                            {
                                pa.DataProgramada = data;
                                pa.Status = StatusAula.Reserva;
                                pa.ProfessorId = a.ProfessorId;
                                pa.ProfessorRealId = 0;
                                pa.Inicio = a.Inicio;
                                pa.Fim = a.Inicio + a.Aula.Duracao;
                                programou = true;
                                aulav = lapa.FirstOrDefault(x => x.AulaId == pa.AulaId);
                                if (aulav != null)
                                {
                                    pa.Valor = Math.Round(aulav.ValorAula * aaus.Percentual / 100, 2);
                                }
                                pa.OBS = "aluno ausente, com reserva de horario";
                                break;
                            }
                            // verifica se o studio tem horario especial na data
                            var lde = ldes.Where(x => x.Data == data && x.TemExpediente == true).ToList();
                            if (lde.Count > 0)
                            {
                                if (!lde.Any(x => x.Inicio <= a.Inicio && x.Fim >= a.Fim))
                                {
                                    continue;
                                }
                            }
                            //verfica se o professor está disponivel
                            if (lpdes.Any(x => x.TemExpediente == false && x.Data == data && x.ProfessorId == a.ProfessorId))
                            {
                                pa.DataProgramada = data;
                                pa.Status = StatusAula.NaoProgramada;
                                pa.ProfessorId = a.ProfessorId;
                                pa.ProfessorRealId = 0;
                                pa.Inicio = a.Inicio;
                                pa.Fim = pa.Inicio + a.Aula.Duracao;
                                aulav = lapa.FirstOrDefault(x => x.AulaId == pa.AulaId);
                                if (aulav != null)
                                {
                                    pa.Valor = aulav.ValorAula;
                                }
                                pa.OBS = "professor ausente";
                                r.ListErrMsg.Add("Professor ausente dia " + data.ToString("dd/MM/yyyy"));
                                programou = true;
                                break;
                            }
                            if (!lpas.Any(x => x.Dia == data.DayOfWeek && x.ProfessorAgenda.ProfessorId == a.ProfessorId))
                            {
                                throw new Exception("Professor sem expediente para o dia " + data.ToString("dd/MM/yyyy") + " - " + Constante.DiaSemana[(int)data.DayOfWeek].ToLower() + " " + a.HoraInicioV);
                            }
                            var lpde = lpdes.Where(x => x.Data == data && x.TemExpediente == true && x.ProfessorId == a.ProfessorId).ToList();
                            if (lpde.Count > 0)
                            {
                                if (!lpde.Any(x => x.Inicio <= a.Inicio && x.Fim >= a.Fim))
                                {
                                    pa.DataProgramada = data;
                                    pa.Status = StatusAula.NaoProgramada;
                                    pa.ProfessorId = a.ProfessorId;
                                    pa.ProfessorRealId = 0;
                                    pa.Inicio = a.Inicio;
                                    pa.Fim = pa.Inicio + a.Aula.Duracao;
                                    pa.OBS = "professor com horário especial";
                                    aulav = lapa.FirstOrDefault(x => x.AulaId == pa.AulaId);
                                    if (aulav != null)
                                    {
                                        pa.Valor = aulav.ValorAula;
                                    }
                                    r.ListErrMsg.Add("Professor com horário especial dia " + data.ToString("dd/MM/yyyy"));
                                    programou = true;
                                    break;
                                }
                            }
                            if (!lpas.Any(x => x.Dia == data.DayOfWeek && x.ProfessorAgenda.ProfessorId == a.ProfessorId && x.Inicio <= a.Inicio && x.Fim >= a.Fim))
                            {
                                throw new Exception("Professor sem expediente para o dia " + data.ToString("dd/MM/yyyy") + " - " + Constante.DiaSemana[(int)data.DayOfWeek].ToLower() + " " + a.HoraInicioV);
                            }
                            if (await _context.ProgramacaoAula.AnyAsync(x => x.TenantId == si.TenantId && x.ProfessorId == a.ProfessorId && x.DataProgramada == data && x.Fim > a.Inicio && x.Inicio < a.Fim && x.AlunoId != a.AlunoId && x.AulaId != a.AulaId && x.Status != StatusAula.Cancelada && x.Status != StatusAula.Reserva))
                            {
                                r.ListErrMsg.Add("Conflito de agenda no dia " + data.ToString("dd/MM/yyyy"));
                                continue;
                            }
                            pa.DataProgramada = data;
                            pa.Status = StatusAula.Agendada;
                            pa.ProfessorId = a.ProfessorId;
                            pa.ProfessorRealId = 0;
                            pa.Inicio = a.Inicio;
                            pa.Fim = pa.Inicio + a.Aula.Duracao;
                            aulav = lapa.FirstOrDefault(x => x.AulaId == pa.AulaId);
                            if (aulav != null)
                            {
                                pa.Valor = aulav.ValorAula;
                            }
                            programou = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                r.Ok = false;
                r.ErrMsg = e.Message;
                _context.ChangeTracker.Clear();
                return r;
            }

            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ProgramacaoAula.UpdateRange(lpa);
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
            return r;
        }
        public async Task<Resultado> ConfirmarPlanoAsync(int id, string token = null)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            AlunoPlano ap = null;
            if (token != null)
            {
                ap = await _context.AlunoPlano.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId && x.TokenAprovacao == token);
            }
            else
            {
                ap = await _context.AlunoPlano.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
            }
            if (ap == null)
            {
                r.Ok = false;
                if (token != null)
                {
                    r.ErrMsg = "Plano do aluno não encontrado, token expirou, solicitar novo token.";
                }
                else
                {
                    r.ErrMsg = "Plano do aluno não encontrado";
                }
                return r;
            }
            if (ap.Status == StatusPlanoAluno.Cancelado)
            {
                r.Ok = false;
                r.ErrMsg = "Plano está cancelado";
                return r;
            }
            if (ap.Status == StatusPlanoAluno.Ativo)
            {
                r.Ok = false;
                r.ErrMsg = "Plano já foi confirmado";
                return r;
            }
            Plano pl = await _context.Plano.FirstOrDefaultAsync(x => x.Id == ap.PlanoId);
            if (ap.Status == StatusPlanoAluno.NaoProgramado && pl.TipoPlano == TipoPlano.PeriodoValorMensal)
            {
                r.Ok = false;
                r.ErrMsg = "Plano não está programado";
                return r;
            }
            _context.Database.BeginTransaction();
            try
            {
                ap.Status = StatusPlanoAluno.Ativo;
                var lp = await _context.ProgramacaoAula.Where(x => x.AlunoPlanoId == ap.Id && x.TenantId == si.TenantId).ToListAsync();
                foreach (var p in lp)
                {
                    if (p.Status == StatusAula.Programada)
                    {
                        p.Status = StatusAula.Agendada;
                    }
                }
                _context.ProgramacaoAula.UpdateRange(lp);
                if (token != null)
                {
                    AlunoPlanoTokenHist ath = new()
                    {
                        AlunoPlanoId = ap.Id,
                        TenantId = si.TenantId,
                        Token = token,
                        DataMovto = Constante.Now,
                        TotalAulas = await _context.ProgramacaoAula.Where(x => x.TenantId == si.TenantId && x.AlunoPlanoId == ap.Id).CountAsync(),
                        ValorTotal = Math.Round(await _context.AlunoPlanoParcela.Where(x => x.TenantId == si.TenantId && x.AlunoPlanoId == ap.Id && x.Status != StatusParcela.Cancelada).SumAsync(x => x.Valor), 2),
                        Historico = "Plano aprovado atraves do link"
                    };
                    await _context.AlunoPlanoTokenHist.AddAsync(ath);
                }
                else
                {
                    if (ap.TokenAprovacao != null && ap.TokenAprovacao != string.Empty)
                    {
                        AlunoPlanoTokenHist ath = new()
                        {
                            AlunoPlanoId = ap.Id,
                            TenantId = si.TenantId,
                            Token = ap.TokenAprovacao,
                            DataMovto = Constante.Now,
                            TotalAulas = 0,
                            ValorTotal = 0,
                            Historico = "Token expirado devido a aprovação do plano atraves do cadastro de planos do aluno"
                        };
                        await _context.AlunoPlanoTokenHist.AddAsync(ath);
                    }
                    ap.TokenAprovacao = string.Empty;
                }
                _context.AlunoPlano.Update(ap);
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
        public async Task<string> GerarLinkAprovacao(int id)
        {
            SessionInfo si = await GetSessionAsync();
            OrigemToken o = OrigemToken.Studio;
            AlunoPlano ap = await _context.AlunoPlano.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);

            int qp = await _context.AlunoPlanoParcela.Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId).CountAsync();
            int qa = await _context.ProgramacaoAula.Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId).CountAsync();
            double va = 0;
            double vp = 0;
            if (qp > 0)
            {
                vp = await _context.AlunoPlanoParcela.Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId).SumAsync(x => x.Valor);
            }
            if (qa > 0)
            {
                va = await _context.ProgramacaoAula.Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId).SumAsync(x => x.Valor);
            }



            OperacaoToken op = OperacaoToken.AprovarPlano;
            int io = (int)o;
            int iop = (int)op;
            string nt = io.ToString() + ";" + iop.ToString() + ";" + ap.Id.ToString() + ";" + vp.ToString() + ";" + qp.ToString() + ";" + va.ToString() + ";" + qa.ToString();
            await _context.Database.BeginTransactionAsync();
            try
            {
                string g = await new SegurancaService(_context).GerarToken(nt, si.TenantId);

                if (ap.TokenAprovacao != null && ap.TokenAprovacao != string.Empty)
                {
                    AlunoPlanoTokenHist atho = new()
                    {
                        AlunoPlanoId = ap.Id,
                        TenantId = si.TenantId,
                        Token = ap.TokenAprovacao,
                        DataMovto = Constante.Now,
                        TotalAulas = 0,
                        ValorTotal = 0,
                        Historico = "Token expirado devido a geração de um novo token"
                    };
                    await _context.AlunoPlanoTokenHist.AddAsync(atho);
                }
                ap.TokenAprovacao = g;
                _context.AlunoPlano.Update(ap);
                AlunoPlanoTokenHist ath = new()
                {
                    AlunoPlanoId = ap.Id,
                    TenantId = si.TenantId,
                    Token = g,
                    DataMovto = Constante.Now,
                    TotalAulas = await _context.ProgramacaoAula.Where(x => x.TenantId == si.TenantId && x.AlunoPlanoId == ap.Id).CountAsync(),
                    ValorTotal = Math.Round(await _context.AlunoPlanoParcela.Where(x => x.TenantId == si.TenantId && x.AlunoPlanoId == ap.Id && x.Status != StatusParcela.Cancelada).SumAsync(x => x.Valor), 2),
                    Historico = "Criação do link para aprovação"
                };
                await _context.AlunoPlanoTokenHist.AddAsync(ath);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();

                return _configService.GetSecurityUrl() + g;
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                return e.Message;
            }

        }
        public async Task<string> GerarLinkExtratoPlano(int id)
        {
            SessionInfo si = await GetSessionAsync();
            OrigemToken o = OrigemToken.Studio;
            OperacaoToken op = OperacaoToken.ExtratoPlano;
            int io = (int)o;
            int iop = (int)op;
            string nt = io.ToString() + ";" + iop.ToString() + ";" + id.ToString();
            string g = await new SegurancaService(_context).GerarToken(nt, si.TenantId);
            return _configService.GetSecurityUrl() + g;
        }
        public async Task<Resultado> CancelarPlanoAsync(int id)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            AlunoPlano ap = await _context.AlunoPlano.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
            if (ap == null)
            {
                r.Ok = false;
                r.ErrMsg = "Plano do aluno não encontrado";
                return r;
            }
            if (ap.Status == StatusPlanoAluno.Cancelado)
            {
                r.Ok = false;
                r.ErrMsg = "Plano está cancelado";
                return r;
            }

            _context.Database.BeginTransaction();
            try
            {
                ap.Status = StatusPlanoAluno.Cancelado;
                var lp = await _context.ProgramacaoAula.Where(x => x.AlunoPlanoId == ap.Id && x.TenantId == si.TenantId && x.DataProgramada >= Constante.Today && x.Status != StatusAula.Executada).ToListAsync();
                foreach (var p in lp)
                {
                    p.Status = StatusAula.Cancelada;
                    p.ProfessorRealId = 0;
                    p.OBS = "Cancelamento do plano";
                }
                _context.ProgramacaoAula.UpdateRange(lp);

                var lpp = await _context.AlunoPlanoParcela.Where(x => x.AlunoPlanoId == ap.Id && x.TenantId == si.TenantId && x.Status == StatusParcela.Aberto).ToListAsync();
                foreach (var pp in lpp)
                {
                    pp.Status = StatusParcela.Cancelada;
                    pp.OBS = "Cancelamento do plano";
                }
                _context.AlunoPlanoParcela.UpdateRange(lpp);
                if (ap.TokenAprovacao != null || ap.TokenAprovacao != string.Empty)
                {
                    AlunoPlanoTokenHist ath = new()
                    {
                        AlunoPlanoId = ap.Id,
                        TenantId = si.TenantId,
                        Token = ap.TokenAprovacao,
                        DataMovto = Constante.Now,
                        TotalAulas = await _context.ProgramacaoAula.Where(x => x.TenantId == si.TenantId && x.AlunoPlanoId == ap.Id).CountAsync(),
                        ValorTotal = Math.Round(await _context.AlunoPlanoParcela.Where(x => x.TenantId == si.TenantId && x.AlunoPlanoId == ap.Id && x.Status != StatusParcela.Cancelada).SumAsync(x => x.Valor), 2),
                        Historico = "Token expirado por cancelamento do plano"
                    };
                    await _context.AlunoPlanoTokenHist.AddAsync(ath);
                    ap.TokenAprovacao = string.Empty;
                }
                _context.AlunoPlano.Update(ap);
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
        public async Task<Resultado> UpdateProgramaAula(ProgramacaoAula a, ProgramacaoAula ant)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (a.DataProgramada < Constante.Today.AddMonths(-24))
            {
                r.Ok = false;
                r.ErrMsg = "Data inválida";
            }

            AlunoPlano ap = await _context.AlunoPlano.FirstOrDefaultAsync(x => x.Id == a.AlunoPlanoId && x.TenantId == si.TenantId);
            if (ap != null)
            {
                if (a.Status == StatusAula.Agendada && ap.Status == StatusPlanoAluno.PendenteConfirmacao)
                {
                    a.Status = StatusAula.Programada;
                }
                if (ap.TipoPlano == TipoPlano.PacoteQtdeAula)
                {
                    a.TipoAula = TipoAula.Pacote;
                }
                else
                {
                    if (ap.TipoPlano == TipoPlano.PeriodoValorMensal || ap.TipoPlano == TipoPlano.PeriodoValorFixo)
                    {
                        a.TipoAula = TipoAula.Plano;
                    }
                }
            }
            if (a.Aula == null)
            {
                var au = await _context.Aula.FirstOrDefaultAsync(x => x.Id == a.AulaId && x.TenantId == si.TenantId);
                if (au == null)
                {
                    throw new Exception("Aula não informada");
                }
                a.Fim = a.Inicio + au.Duracao;
            }
            else
            {
                a.Fim = a.Inicio + a.Aula.Duracao;
            }
            if (a.OBS == null)
            {
                a.OBS = string.Empty;
            }
            if (ant.OBS == null)
            {
                ant.OBS = string.Empty;
            }
            if (a.NotaFiscal == null)
            {
                a.NotaFiscal = string.Empty;
            }
            if (ant.NotaFiscal == null)
            {
                ant.NotaFiscal = string.Empty;
            }
            if (a.Status != StatusAula.Executada)
            {
                a.ProfessorRealId = a.ProfessorId;
            }

            AlunoPlanoAula apa = await _context.AlunoPlanoAula.FirstOrDefaultAsync(x => x.AlunoPlanoId == a.AlunoPlanoId && x.AulaId == a.AulaId);
            if (apa != null)
            {
                if (a.Status != StatusAula.Reserva && ap.TipoPlano != TipoPlano.PeriodoValorFixo)
                {
                    a.Valor = apa.ValorAula;
                }
            }

            var command = _context.Database.GetDbConnection().CreateCommand();

            command.CommandText =

"Update ProgramacaoAula set  " +
"            DataProgramada =Convert(date, @DataProgramada, 112), " +
"            Fim = @Fim,  " +
"            inicio = @Inicio , " +
"            OBS = @OBS,  " +
"            Origem = @Origem , " +
"            ProfessorId = @ProfessorId,  " +
"            ProfessorRealId = @ProfessorRealId , " +
"            Status = @Status, " +
"            Valor = @Valor, " +
"            StatusFinanceiro = @StatusFinanceiro, " +
"            DataPagto = @DataPagto, " +
"            Faturado = @Faturado, " +
"            ValorPago = @ValorPago, " +
"            NotaFiscal = @NotaFiscal, " +
"            TipoAula = @TipoAula " +

" where Id = @Id  and TenantId = @tid";

            command.Parameters.Add(new SqlParameter("@Id", System.Data.SqlDbType.Int));
            command.Parameters["@id"].Value = a.Id;
            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@DataProgramada", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@DataProgramada"].Value = a.DataProgramada.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@Fim", System.Data.SqlDbType.Float));
            command.Parameters["@Fim"].Value = a.Fim;

            command.Parameters.Add(new SqlParameter("@Inicio", System.Data.SqlDbType.Float));
            command.Parameters["@Inicio"].Value = a.Inicio;

            command.Parameters.Add(new SqlParameter("@OBS", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@OBS"].Value = a.OBS;

            command.Parameters.Add(new SqlParameter("@Origem", System.Data.SqlDbType.Int));
            command.Parameters["@Origem"].Value = (int)a.Origem;

            command.Parameters.Add(new SqlParameter("@ProfessorId", System.Data.SqlDbType.Int));
            command.Parameters["@ProfessorId"].Value = a.ProfessorId;

            command.Parameters.Add(new SqlParameter("@ProfessorRealId", System.Data.SqlDbType.Int));
            command.Parameters["@ProfessorRealId"].Value = a.ProfessorRealId;

            command.Parameters.Add(new SqlParameter("@Status", System.Data.SqlDbType.Int));
            command.Parameters["@Status"].Value = (int)a.Status;

            command.Parameters.Add(new SqlParameter("@Valor", System.Data.SqlDbType.Float));
            command.Parameters["@Valor"].Value = a.Valor;

            command.Parameters.Add(new SqlParameter("@StatusFinanceiro", System.Data.SqlDbType.Float));
            command.Parameters["@StatusFinanceiro"].Value = (int)a.StatusFinanceiro;

            command.Parameters.Add(new SqlParameter("@DataPagto", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@DataPagto"].Value = a.DataPagto.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@Faturado", System.Data.SqlDbType.Bit));
            command.Parameters["@Faturado"].Value = a.Faturado;

            command.Parameters.Add(new SqlParameter("@ValorPago", System.Data.SqlDbType.Float));
            command.Parameters["@ValorPago"].Value = a.ValorPago;

            command.Parameters.Add(new SqlParameter("@NotaFiscal", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@NotaFiscal"].Value = a.NotaFiscal;

            command.Parameters.Add(new SqlParameter("@TipoAula", System.Data.SqlDbType.Int));
            command.Parameters["@TipoAula"].Value = (int)a.TipoAula;

            await command.Connection.OpenAsync();
            command.Transaction = await command.Connection.BeginTransactionAsync();
            try
            {
                await command.ExecuteNonQueryAsync();

                if (a.Status != StatusAula.Executada && ant.Status == StatusAula.Executada)
                {
                    command.Parameters.Clear();
                    command.CommandText =
" Delete AlunoProntuario from AlunoProntuarioAula where AlunoProntuario.Id = AlunoProntuarioAula.AlunoProntuarioId and  AlunoProntuarioAula.ProgramacaoAulaId = @ProgramacaoAulaId and AlunoProntuarioAula.TenantId = @tid ";
                    command.Parameters.Add(new SqlParameter("@ProgramacaoAulaId", System.Data.SqlDbType.Int));
                    command.Parameters["@ProgramacaoAulaId"].Value = a.Id;
                    command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                    command.Parameters["@tid"].Value = si.TenantId;
                    await command.ExecuteNonQueryAsync();

                    command.Parameters.Clear();
                    command.CommandText =
" Delete AlunoProntuarioAula where ProgramacaoAulaId = @ProgramacaoAulaId and TenantId = @tid ";
                    command.Parameters.Add(new SqlParameter("@ProgramacaoAulaId", System.Data.SqlDbType.Int));
                    command.Parameters["@ProgramacaoAulaId"].Value = a.Id;
                    command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                    command.Parameters["@tid"].Value = si.TenantId;
                    await command.ExecuteNonQueryAsync();
                }

                if (a.Status == StatusAula.Executada && ant.Status != a.Status && a.ProntuarioAula != string.Empty)
                {
                    AlunoProntuario apt = new()
                    {
                        AlunoId = a.AlunoId,
                        DataRegistro = Constante.Today,
                        Notas = a.ProntuarioAula,
                        TenantId = si.TenantId,
                        ProfessorId = a.ProfessorRealId
                    };
                    _context.Database.UseTransaction(command.Transaction);

                    await _context.AlunoProntuario.AddAsync(apt);
                    await _context.SaveChangesAsync();
                    AlunoProntuarioAula apta = new()
                    {
                        AlunoProntuarioId = apt.Id,
                        TenantId = si.TenantId,
                        ProgramacaoAulaId = a.Id
                    };
                    await _context.AlunoProntuarioAula.AddAsync(apta);
                    await _context.SaveChangesAsync();
                }

                command.Parameters.Clear();
                command.CommandText =

    "Insert ProgramacaoAulaHist ( " +
    "            ProgramacaoAulaId," +
    "            DataProgramadaAnt, " +
    "            DataProgramadaDep, " +
    "            FimAnt,   " +
    "            FimDep,  " +
    "            inicioAnt, " +
    "            inicioDep,  " +
    "            OBS,   " +
    "            ProfessorAnt,  " +
    "            ProfessorDep,  " +
    "            StatusAnt, " +
    "            StatusDep, " +
        "        ValorAnt, " +
    "            ValorDep, " +
    "            UserName, " +
    "            DataAlt, " +
    "            TenantId ) " +

                " values ( " +
    "             @ProgramacaoAulaId ," +
    "            Convert(date, @DataProgramadaAnt, 112), " +
    "            Convert(date, @DataProgramadaDep, 112), " +
    "            @FimAnt,  " +
    "            @FimDep,  " +
    "            @InicioAnt , " +
    "            @InicioDep , " +
    "            @OBS,  " +
    "            @ProfessorAnt,  " +
    "            @ProfessorDep,  " +
    "            @StatusAnt, " +
    "            @StatusDep, " +
            "    @ValorAnt, " +
    "            @ValorDep, " +
    "            @UserName, " +
    "            @DataAlt, " +
    "            @tid ) ";

                command.Parameters.Add(new SqlParameter("@ProgramacaoAulaId", System.Data.SqlDbType.Int));
                command.Parameters["@ProgramacaoAulaId"].Value = a.Id;
                command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                command.Parameters["@tid"].Value = si.TenantId;

                command.Parameters.Add(new SqlParameter("@DataAlt", System.Data.SqlDbType.NVarChar, 30));
                command.Parameters["@DataAlt"].Value = Constante.Now.ToString("yyyyMMdd HH:mm:ss");

                command.Parameters.Add(new SqlParameter("@UserName", System.Data.SqlDbType.NVarChar, 50));
                command.Parameters["@UserName"].Value = si.UserName;

                command.Parameters.Add(new SqlParameter("@DataProgramadaAnt", System.Data.SqlDbType.NVarChar, 8));
                command.Parameters["@DataProgramadaAnt"].Value = ant.DataProgramada.ToString("yyyyMMdd");
                command.Parameters.Add(new SqlParameter("@DataProgramadaDep", System.Data.SqlDbType.NVarChar, 8));
                command.Parameters["@DataProgramadaDep"].Value = a.DataProgramada.ToString("yyyyMMdd");

                command.Parameters.Add(new SqlParameter("@FimAnt", System.Data.SqlDbType.Float));
                command.Parameters["@FimAnt"].Value = ant.Fim;
                command.Parameters.Add(new SqlParameter("@FimDep", System.Data.SqlDbType.Float));
                command.Parameters["@FimDep"].Value = a.Fim;

                command.Parameters.Add(new SqlParameter("@InicioAnt", System.Data.SqlDbType.Float));
                command.Parameters["@InicioAnt"].Value = ant.Inicio;
                command.Parameters.Add(new SqlParameter("@InicioDep", System.Data.SqlDbType.Float));
                command.Parameters["@InicioDep"].Value = a.Inicio;

                command.Parameters.Add(new SqlParameter("@OBS", System.Data.SqlDbType.NVarChar, 100));
                command.Parameters["@OBS"].Value = a.OBS;

                command.Parameters.Add(new SqlParameter("@ProfessorAnt", System.Data.SqlDbType.NVarChar, 50));
                command.Parameters["@ProfessorAnt"].Value = ant.ProfessorNome;

                command.Parameters.Add(new SqlParameter("@ProfessorDep", System.Data.SqlDbType.NVarChar, 50));
                command.Parameters["@ProfessorDep"].Value = a.ProfessorNome;

                command.Parameters.Add(new SqlParameter("@StatusAnt", System.Data.SqlDbType.Int));
                command.Parameters["@StatusAnt"].Value = (int)ant.Status;
                command.Parameters.Add(new SqlParameter("@StatusDep", System.Data.SqlDbType.Int));
                command.Parameters["@StatusDep"].Value = (int)a.Status;

                command.Parameters.Add(new SqlParameter("@ValorAnt", System.Data.SqlDbType.Float));
                command.Parameters["@ValorAnt"].Value = (int)ant.Valor;
                command.Parameters.Add(new SqlParameter("@ValorDep", System.Data.SqlDbType.Float));
                command.Parameters["@ValorDep"].Value = (int)a.Valor;

                await command.ExecuteNonQueryAsync();

                if (a.AlunoPlanoId > 0)
                {
                    if (ap.TipoPlano == TipoPlano.PacoteQtdeAula)
                    {
                        command.Parameters.Clear();
                        command.CommandText =
        " Update AlunoPlano set AlunoPlano.Status = 5  " +
        "where  " +
        " AlunoPlano.TenantId = @tid and " +
        "AlunoPlano.Id = (  " +
    "Select Id from " +
    "(select AlunoPlano.Id, Max(AlunoPlanoAula.QtdeAulas) QtdeAulas, Sum(Case when Isnull(ProgramacaoAula.Id, 0) = 0 then 0 else 1 end )  TotalAulasFeitas " +
    "from AlunoPlano " +
    "join AlunoPLanoAula on AlunoPlanoAula.AlunoPlanoId = AlunoPlano.Id " +
    "left join ProgramacaoAula on ProgramacaoAula.AlunoPlanoId = AlunoPlano.Id and ProgramacaoAula.AulaId = AlunoPLanoAula.AulaId and ProgramacaoAula.Status in (3, 7, 8) " +
    "where " +
    " AlunoPlano.id = @planoid " +
    "group by   AlunoPlano.Id, AlunoPlanoAula.Id ) AulasFeitas " +
    "group by AulasFeitas.Id    " +
        "having Sum(AulasFeitas.QtdeAulas) <= Sum(TotalAulasFeitas)  " +
        ")  ";
                        command.Parameters.Add(new SqlParameter("@planoid", System.Data.SqlDbType.Int));
                        command.Parameters["@planoid"].Value = a.AlunoPlanoId;
                        command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                        command.Parameters["@tid"].Value = si.TenantId;
                        await command.ExecuteNonQueryAsync();

                        command.Parameters.Clear();
                        command.CommandText =
        " Update AlunoPlano set AlunoPlano.Status = 3  " +
        "where  " +
        " AlunoPlano.TenantId = @tid  and AlunoPlano.Status = 5 and  " +
        "AlunoPlano.Id = (  " +
    "Select Id from " +
    "(select AlunoPlano.Id, Max(AlunoPlanoAula.QtdeAulas) QtdeAulas, Sum(Case when Isnull(ProgramacaoAula.Id, 0) = 0 then 0 else 1 end )  TotalAulasFeitas " +
    "from AlunoPlano " +
    "join AlunoPLanoAula on AlunoPlanoAula.AlunoPlanoId = AlunoPlano.Id " +
    "left join ProgramacaoAula on ProgramacaoAula.AlunoPlanoId = AlunoPlano.Id and ProgramacaoAula.AulaId = AlunoPLanoAula.AulaId and ProgramacaoAula.Status in (3, 7, 8) " +
    "where " +
    " AlunoPlano.id = @planoid " +
    "group by   AlunoPlano.Id, AlunoPlanoAula.Id ) AulasFeitas " +
    "group by AulasFeitas.Id    " +
    " having Sum(AulasFeitas.QtdeAulas) > Sum(TotalAulasFeitas)  " +
        ")  ";
                        command.Parameters.Add(new SqlParameter("@planoid", System.Data.SqlDbType.Int));
                        command.Parameters["@planoid"].Value = a.AlunoPlanoId;
                        command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                        command.Parameters["@tid"].Value = si.TenantId;
                        await command.ExecuteNonQueryAsync();
                    }
                    if (ap.TipoPlano == TipoPlano.PeriodoValorMensal)
                    {
                        command.Parameters.Clear();
                        command.CommandText =
        " Update AlunoPlano set AlunoPlano.Status = 5  " +
        "where  " +
        " AlunoPlano.TenantId = @tid and " +
        "AlunoPlano.Id = (  " +
    "Select Id from " +
    "(select AlunoPlano.Id, Max(AlunoPlanoAula.QtdeAulas) QtdeAulas, Sum(Case when Isnull(ProgramacaoAula.Id, 0) = 0 then 0 else 1 end )  TotalAulasFeitas " +
    "from AlunoPlano " +
    "join AlunoPLanoAula on AlunoPlanoAula.AlunoPlanoId = AlunoPlano.Id " +
    "left join ProgramacaoAula on ProgramacaoAula.AlunoPlanoId = AlunoPlano.Id and ProgramacaoAula.AulaId = AlunoPLanoAula.AulaId and ProgramacaoAula.Status in  (1, 2, 5, 6) " +
    "where " +
    " AlunoPlano.id = @planoid " +
    "group by   AlunoPlano.Id, AlunoPlanoAula.Id ) AulasFeitas " +
    "group by AulasFeitas.Id    " +
        "having  Sum(TotalAulasFeitas) = 0 " +
        ")  ";
                        command.Parameters.Add(new SqlParameter("@planoid", System.Data.SqlDbType.Int));
                        command.Parameters["@planoid"].Value = a.AlunoPlanoId;
                        command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                        command.Parameters["@tid"].Value = si.TenantId;
                        await command.ExecuteNonQueryAsync();

                        command.Parameters.Clear();
                        command.CommandText =
        " Update AlunoPlano set AlunoPlano.Status = 3  " +
        "where  " +
        " AlunoPlano.TenantId = @tid  and AlunoPlano.Status = 5 and  " +
        "AlunoPlano.Id = (  " +
    "Select Id from " +
    "(select AlunoPlano.Id, Max(AlunoPlanoAula.QtdeAulas) QtdeAulas, Sum(Case when Isnull(ProgramacaoAula.Id, 0) = 0 then 0 else 1 end )  TotalAulasFeitas " +
    "from AlunoPlano " +
    "join AlunoPLanoAula on AlunoPlanoAula.AlunoPlanoId = AlunoPlano.Id " +
    "left join ProgramacaoAula on ProgramacaoAula.AlunoPlanoId = AlunoPlano.Id and ProgramacaoAula.AulaId = AlunoPLanoAula.AulaId and ProgramacaoAula.Status in (1, 2, 5, 6) " +
    "where " +
    " AlunoPlano.id = @planoid " +
    "group by   AlunoPlano.Id, AlunoPlanoAula.Id ) AulasFeitas " +
    "group by AulasFeitas.Id    " +
    " having Sum(TotalAulasFeitas) > 0  " +
        ")  ";
                        command.Parameters.Add(new SqlParameter("@planoid", System.Data.SqlDbType.Int));
                        command.Parameters["@planoid"].Value = a.AlunoPlanoId;
                        command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                        command.Parameters["@tid"].Value = si.TenantId;
                        await command.ExecuteNonQueryAsync();
                    }
                }

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
            if ((a.Status == StatusAula.Cancelada && ant.Status != StatusAula.Cancelada) || (ant.Status == StatusAula.Cancelada && a.Status != StatusAula.Cancelada))
            {
                if (a.AlunoPlanoId > 0)
                {
                    await ConciliarAlunoPlano(a.AlunoPlanoId);
                }
            }

            return r;
        }
        public async Task<ProgramacaoAula> AddProgramaAula(ProgramacaoAula a)
        {
            SessionInfo si = await GetSessionAsync();

            if (a.DataProgramada < Constante.Today.AddMonths(-24))
            {
                throw new Exception("Data inválida");
            }

            if (a.Aula == null)
            {
                var au = await _context.Aula.FirstOrDefaultAsync(x => x.Id == a.AulaId);
                if (au == null)
                {
                    throw new Exception("Aula não informada");
                }
                a.Fim = a.Inicio + au.Duracao;
            }
            else
            {
                a.Fim = a.Inicio + a.Aula.Duracao;
            }
            AlunoPlano ap = null;
            if (a.AlunoPlanoId != 0)
            {
                ap = a.AlunoPlano;
                if (ap == null)
                {
                    ap = await _context.AlunoPlano.AsNoTracking().FirstOrDefaultAsync(x => x.Id == a.AlunoPlanoId && x.TenantId == si.TenantId);
                }
                if (ap != null)
                {
                    if (ap.TipoPlano == TipoPlano.PacoteQtdeAula)
                    {
                        a.TipoAula = TipoAula.Pacote;
                    }
                    else
                    {
                        if (ap.TipoPlano == TipoPlano.PeriodoValorMensal || ap.TipoPlano == TipoPlano.PeriodoValorFixo)
                        {
                            a.TipoAula = TipoAula.Plano;
                        }
                    }
                }
            }

            a.TenantId = si.TenantId;
            a.Aluno = null;
            a.AlunoPlano = null;
            a.Aula = null;
            a.Professor = null;
            a.Studio = null;

            var command = _context.Database.GetDbConnection().CreateCommand();
            await command.Connection.OpenAsync();
            command.Transaction = await command.Connection.BeginTransactionAsync();
            _context.Database.UseTransaction(command.Transaction);
            try
            {
                _context.ProgramacaoAula.Add(a);
                await _context.SaveChangesAsync();
                if (a.AlunoPlanoId != 0)
                {
                    await ConciliarAlunoPlano(a.AlunoPlanoId);
                }

                if (a.AlunoPlanoId > 0)
                {
                    if (ap.TipoPlano == TipoPlano.PacoteQtdeAula)
                    {
                        command.Parameters.Clear();
                        command.CommandText =
        " Update AlunoPlano set AlunoPlano.Status = 5  " +
        "where  " +
        " AlunoPlano.TenantId = @tid and " +
        "AlunoPlano.Id = (  " +
    "Select Id from " +
    "(select AlunoPlano.Id, Max(AlunoPlanoAula.QtdeAulas) QtdeAulas, Sum(Case when Isnull(ProgramacaoAula.Id, 0) = 0 then 0 else 1 end )  TotalAulasFeitas " +
    "from AlunoPlano " +
    "join AlunoPLanoAula on AlunoPlanoAula.AlunoPlanoId = AlunoPlano.Id " +
    "left join ProgramacaoAula on ProgramacaoAula.AlunoPlanoId = AlunoPlano.Id and ProgramacaoAula.AulaId = AlunoPLanoAula.AulaId and ProgramacaoAula.Status in (3, 7, 8) " +
    "where " +
    " AlunoPlano.id = @planoid " +
    "group by   AlunoPlano.Id, AlunoPlanoAula.Id ) AulasFeitas " +
    "group by AulasFeitas.Id    " +
        "having Sum(AulasFeitas.QtdeAulas) <= Sum(TotalAulasFeitas)  " +
        ")  ";
                        command.Parameters.Add(new SqlParameter("@planoid", System.Data.SqlDbType.Int));
                        command.Parameters["@planoid"].Value = a.AlunoPlanoId;
                        command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                        command.Parameters["@tid"].Value = si.TenantId;
                        await command.ExecuteNonQueryAsync();

                        command.Parameters.Clear();
                        command.CommandText =
        " Update AlunoPlano set AlunoPlano.Status = 3  " +
        "where  " +
        " AlunoPlano.TenantId = @tid  and AlunoPlano.Status = 5 and  " +
        "AlunoPlano.Id = (  " +
    "Select Id from " +
    "(select AlunoPlano.Id, Max(AlunoPlanoAula.QtdeAulas) QtdeAulas, Sum(Case when Isnull(ProgramacaoAula.Id, 0) = 0 then 0 else 1 end )  TotalAulasFeitas " +
    "from AlunoPlano " +
    "join AlunoPLanoAula on AlunoPlanoAula.AlunoPlanoId = AlunoPlano.Id " +
    "left join ProgramacaoAula on ProgramacaoAula.AlunoPlanoId = AlunoPlano.Id and ProgramacaoAula.AulaId = AlunoPLanoAula.AulaId and ProgramacaoAula.Status in (3, 7, 8) " +
    "where " +
    " AlunoPlano.id = @planoid " +
    "group by   AlunoPlano.Id, AlunoPlanoAula.Id ) AulasFeitas " +
    "group by AulasFeitas.Id    " +
    " having Sum(AulasFeitas.QtdeAulas) > Sum(TotalAulasFeitas)  " +
        ")  ";
                        command.Parameters.Add(new SqlParameter("@planoid", System.Data.SqlDbType.Int));
                        command.Parameters["@planoid"].Value = a.AlunoPlanoId;
                        command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                        command.Parameters["@tid"].Value = si.TenantId;
                        await command.ExecuteNonQueryAsync();
                    }
                    if (ap.TipoPlano == TipoPlano.PeriodoValorMensal)
                    {
                        command.Parameters.Clear();
                        command.CommandText =
        " Update AlunoPlano set AlunoPlano.Status = 5  " +
        "where  " +
        " AlunoPlano.TenantId = @tid and " +
        "AlunoPlano.Id = (  " +
    "Select Id from " +
    "(select AlunoPlano.Id, Max(AlunoPlanoAula.QtdeAulas) QtdeAulas, Sum(Case when Isnull(ProgramacaoAula.Id, 0) = 0 then 0 else 1 end )  TotalAulasFeitas " +
    "from AlunoPlano " +
    "join AlunoPLanoAula on AlunoPlanoAula.AlunoPlanoId = AlunoPlano.Id " +
    "left join ProgramacaoAula on ProgramacaoAula.AlunoPlanoId = AlunoPlano.Id and ProgramacaoAula.AulaId = AlunoPLanoAula.AulaId and ProgramacaoAula.Status in  (1, 2, 5, 6) " +
    "where " +
    " AlunoPlano.id = @planoid " +
    "group by   AlunoPlano.Id, AlunoPlanoAula.Id ) AulasFeitas " +
    "group by AulasFeitas.Id    " +
        "having  Sum(TotalAulasFeitas) = 0 " +
        ")  ";
                        command.Parameters.Add(new SqlParameter("@planoid", System.Data.SqlDbType.Int));
                        command.Parameters["@planoid"].Value = a.AlunoPlanoId;
                        command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                        command.Parameters["@tid"].Value = si.TenantId;
                        await command.ExecuteNonQueryAsync();

                        command.Parameters.Clear();
                        command.CommandText =
        " Update AlunoPlano set AlunoPlano.Status = 3  " +
        "where  " +
        " AlunoPlano.TenantId = @tid  and AlunoPlano.Status = 5 and  " +
        "AlunoPlano.Id = (  " +
    "Select Id from " +
    "(select AlunoPlano.Id, Max(AlunoPlanoAula.QtdeAulas) QtdeAulas, Sum(Case when Isnull(ProgramacaoAula.Id, 0) = 0 then 0 else 1 end )  TotalAulasFeitas " +
    "from AlunoPlano " +
    "join AlunoPLanoAula on AlunoPlanoAula.AlunoPlanoId = AlunoPlano.Id " +
    "left join ProgramacaoAula on ProgramacaoAula.AlunoPlanoId = AlunoPlano.Id and ProgramacaoAula.AulaId = AlunoPLanoAula.AulaId and ProgramacaoAula.Status in (1, 2, 5, 6) " +
    "where " +
    " AlunoPlano.id = @planoid " +
    "group by   AlunoPlano.Id, AlunoPlanoAula.Id ) AulasFeitas " +
    "group by AulasFeitas.Id    " +
    " having Sum(TotalAulasFeitas) > 0  " +
        ")  ";
                        command.Parameters.Add(new SqlParameter("@planoid", System.Data.SqlDbType.Int));
                        command.Parameters["@planoid"].Value = a.AlunoPlanoId;
                        command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                        command.Parameters["@tid"].Value = si.TenantId;
                        await command.ExecuteNonQueryAsync();
                    }
                }
                await command.Transaction.CommitAsync();
            }
            catch
            {
                await command.Transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await command.Connection.CloseAsync();
            }
            return await _context.ProgramacaoAula.AsNoTracking().FirstOrDefaultAsync(x => x.Id == a.Id && x.TenantId == si.TenantId);
        }
        public async Task<string> GetProntuario(int id)
        {
            SessionInfo si = await GetSessionAsync();
            var l = await (from ProgramacaoAula in _context.ProgramacaoAula
                           join AlunoProntuarioAula in _context.AlunoProntuarioAula on ProgramacaoAula.Id equals AlunoProntuarioAula.ProgramacaoAulaId
                           join AlunoProntuario in _context.AlunoProntuario on AlunoProntuarioAula.AlunoProntuarioId equals AlunoProntuario.Id
                           select new
                           {
                               Prontuario = AlunoProntuario.Notas,
                               AlunoProntuarioAula.TenantId,
                               ProgramacaoAulaId = ProgramacaoAula.Id
                           }).AsNoTracking().Where(x => x.TenantId == si.TenantId && x.ProgramacaoAulaId == id).FirstOrDefaultAsync();
            if (l != null)
            {
                return l.Prontuario;
            }
            else
            {
                return string.Empty;
            }
        }
        public async Task<Resultado> DeleteProgramacaoAulaAsync(ProgramacaoAula a)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            List<StatusAula> sa = new() { StatusAula.Executada, StatusAula.FaltaSemReagendamento };
            List<StatusAula> sa2 = new() { StatusAula.ReAgendamento, StatusAula.Agendada };

            if (sa.Contains(a.Status))
            {
                r.Ok = false;
                r.ErrMsg = "Situação da aula não permite a exclusão";
                return r;
            }
            if (a.TipoAula != TipoAula.Pacote && sa2.Contains(a.Status))
            {
                r.Ok = false;
                r.ErrMsg = "Situação da aula não permite a exclusão";
                return r;
            }
            var command = _context.Database.GetDbConnection().CreateCommand();

            await command.Connection.OpenAsync();
            command.Transaction = await command.Connection.BeginTransactionAsync();
            try
            {
                command.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int));
                command.Parameters["@id"].Value = a.Id;
                command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
                command.Parameters["@tid"].Value = si.TenantId;

                command.CommandText =
                        "Delete ProgramacaoAulaHist   " +
                        " where ProgramacaoAulaId = @id  and TenantId = @tid";
                await command.ExecuteNonQueryAsync();

                command.CommandText =
                        "Delete AlunoProntuarioAula   " +
                        " where ProgramacaoAulaId = @id  and TenantId = @tid";
                await command.ExecuteNonQueryAsync();

                command.CommandText =
                        "Delete ProgramacaoAula   " +
                        " where Id = @id  and TenantId = @tid";
                await command.ExecuteNonQueryAsync();

                await command.Transaction.CommitAsync();
                r.Ok = true;
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

            if (a.AlunoPlanoId > 0)
            {
                try
                {
                    r = await ConciliarAlunoPlano(a.AlunoPlanoId);
                }
                catch (Exception e)
                {
                    r.Ok = false;
                    r.ErrMsg = e.Message;
                }
            }

            return r;
        }
        public async Task<List<ProgramacaoAulaHist>> LoadProgramacaoHist(int id)
        {
            SessionInfo si = await GetSessionAsync();
            return await _context.ProgramacaoAulaHist.AsNoTracking().Where(x => x.ProgramacaoAulaId == id && x.TenantId == si.TenantId).OrderByDescending(x => x.DataAlt).ToListAsync();
        }
        public async Task AjustaValorAulaPlanoFixo(int id, List<ProgramacaoAula> laothers)
        {
            SessionInfo si = await GetSessionAsync();
            var tpplano = await (from AlunoPlano in _context.AlunoPlano
                                 select new { AlunoPlano.TipoPlano, AlunoPlano.TipoCalculoValorPlano, AlunoPlano.Id, AlunoPlano.TenantId }).Where(x => x.Id == id && x.TenantId == si.TenantId).FirstOrDefaultAsync();


            List<ProgramacaoAula> la = null;
            if (laothers == null)
            {
                la = await _context.ProgramacaoAula.Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId).ToListAsync();
            }
            else
            {
                la = laothers;
            }

            double valortotplano = 0;
            if (tpplano.TipoPlano == TipoPlano.PeriodoValorFixo && tpplano.TipoCalculoValorPlano == TipoCalculoValorPlano.ValorFixoMensal)
            {
                valortotplano = Math.Round(await _context.AlunoPlanoParcela.Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId).SumAsync(x => x.Valor), 2);
                foreach (var a in la)
                {
                    a.Valor = Math.Round(valortotplano / la.Count, 2);
                }
                if (la.Count > 0)
                {
                    la[^1].Valor = Math.Round(la[^1].Valor + valortotplano - Math.Round(la.Sum(x => x.Valor), 2), 2);
                }
                if (laothers == null)
                {
                    _context.ProgramacaoAula.UpdateRange(la);
                    await _context.SaveChangesAsync();
                }
            }
        }
        public async Task<Resultado> ConciliarAlunoPlano(int id)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            double valorpago = Math.Round(await _context.AlunoPlanoParcela.Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId && x.Status == StatusParcela.Pago).SumAsync(x => x.ValorPago), 2);
            double valorconciliado = Math.Round(await _context.AlunoPlanoParcela.Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId && x.Status == StatusParcela.Conciliado).SumAsync(x => x.ValorPago), 2);
            var la = await _context.ProgramacaoAula.Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId).OrderBy(x => x.DataProgramada).ThenBy(x => x.Inicio).ToListAsync();
            double saldo = valorpago + valorconciliado;
            double valoralocado = 0;
            await AjustaValorAulaPlanoFixo(id, la);
            foreach (var a in la)
            {
                if (a.Status != StatusAula.Cancelada)
                {
                    if (a.Valor > saldo && saldo > 0)
                    {
                        a.ValorPago = saldo;
                        valoralocado += saldo;
                        if (valorconciliado >= valoralocado)
                        {
                            a.StatusFinanceiro = StatusParcela.Conciliado;
                        }
                        else
                        {
                            a.StatusFinanceiro = StatusParcela.Pago;
                        }
                        saldo = 0;
                    }
                    else
                    {
                        if (saldo > 0)
                        {
                            a.ValorPago = a.Valor;
                            valoralocado += a.Valor;
                            if (valorconciliado >= valoralocado)
                            {
                                a.StatusFinanceiro = StatusParcela.Conciliado;
                            }
                            else
                            {
                                a.StatusFinanceiro = StatusParcela.Pago;
                            }
                            saldo -= a.Valor;
                        }
                        else
                        {
                            a.ValorPago = 0;
                            a.StatusFinanceiro = StatusParcela.Aberto;
                        }
                    }
                }
                else
                {
                    a.ValorPago = 0;
                    a.StatusFinanceiro = StatusParcela.Aberto;
                }
            }

            bool mytran = (_context.Database.CurrentTransaction == null);
            if (mytran)
            {
                await _context.Database.BeginTransactionAsync();
            }
            try
            {
                _context.ProgramacaoAula.UpdateRange(la);
                await _context.SaveChangesAsync();
                if (mytran)
                {
                    _context.Database.CommitTransaction();
                }
                r.Ok = true;
                return r;
            }
            catch (Exception e)
            {
                if (mytran)
                {
                    _context.Database.RollbackTransaction();
                    r.ErrMsg = e.Message;
                    r.Ok = false;
                    return r;
                }
                else
                {
                    throw;
                }
            }
        }
        public async Task<Resultado> IgualarParcelas(int id, TipoPlano tp)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            var lp = await _context.AlunoPlanoParcela.Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId && x.Status != StatusParcela.Cancelada).ToListAsync();
            if (lp.Count == 0)
            {
                r.Ok = false;
                r.ErrMsg = "Plano sem parcelas";
                return r;
            }
            if (lp.Any(x => x.Status != StatusParcela.Aberto))
            {
                r.Ok = false;
                r.ErrMsg = "Todas as parcelas precisam estar em aberto para igualar os valores";
                return r;
            }
            double vt = 0;
            if (tp == TipoPlano.PeriodoValorMensal)
            {
                vt = Math.Round(await _context.ProgramacaoAula.Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId && x.Status != StatusAula.Cancelada).SumAsync(x => x.Valor), 2);
            }
            else
            {
                if (tp == TipoPlano.PacoteQtdeAula)
                {
                    vt = Math.Round(await _context.AlunoPlanoAula.Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId).SumAsync(x => x.ValorAula * x.QtdeAulas), 2);
                }
                else
                {
                    if (tp == TipoPlano.PeriodoValorFixo)
                    {
                        vt = Math.Round(await _context.AlunoPlanoParcela.Where(x => x.AlunoPlanoId == id && x.TenantId == si.TenantId && x.Status != StatusParcela.Cancelada).SumAsync(x => x.Valor), 2);
                    }
                }
            }
            double vp = Math.Round(vt / lp.Count, 2);
            double vt1 = vt - vp * lp.Count;
            foreach (var a in lp)
            {
                a.Valor = vp;
            }
            lp[0].Valor += vt1;
            bool mytran = (_context.Database.CurrentTransaction == null);
            if (mytran)
            {
                await _context.Database.BeginTransactionAsync();
            }
            try
            {
                _context.AlunoPlanoParcela.UpdateRange(lp);
                await _context.SaveChangesAsync();
                if (mytran)
                {
                    _context.Database.CommitTransaction();
                }
                r.Ok = true;
                return r;
            }
            catch (Exception e)
            {
                if (mytran)
                {
                    _context.Database.RollbackTransaction();
                    r.ErrMsg = e.Message;
                    r.Ok = false;
                    return r;
                }
                else
                {
                    throw;
                }
            }
        }
        public async Task<List<StudioParcelaView>> FindAllParcelasView(DateTime dti, DateTime dtf, int alunoid, List<StatusParcela> statusParcelas, bool vencidas = false)
        {
            SessionInfo si = await GetSessionAsync();
            List<StudioParcelaView> r = new();
            var l = await (from ProgramacaoAula in _context.ProgramacaoAula
                           join Aula in _context.Aula on ProgramacaoAula.AulaId equals Aula.Id
                           join Professor in _context.Professor on ProgramacaoAula.ProfessorId equals Professor.Id
                           join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                           join Aluno in _context.Aluno on ProgramacaoAula.AlunoId equals Aluno.Id
                           join RelacionamentoAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelacionamentoAluno.Id
                           join Studio in _context.Studio on ProgramacaoAula.StudioId equals Studio.Id
                           select new
                           {
                               ProgramacaoAula,
                               NomeAula = Aula.Nome,
                               NomeProfessor = Relacionamento.Nome,
                               Aula.SemCusto,
                               NomeAluno = RelacionamentoAluno.Nome,
                               RelacionamentoAlunoId = RelacionamentoAluno.Id
                           }).AsNoTracking().Where(x => x.ProgramacaoAula.TenantId == si.TenantId && x.SemCusto == false
                           &&
                           (
                           (
                           x.ProgramacaoAula.DataProgramada >= dti && x.ProgramacaoAula.DataProgramada <= dtf
                           && x.ProgramacaoAula.TipoAula == TipoAula.Avulsa && (x.ProgramacaoAula.Status == StatusAula.Executada || x.ProgramacaoAula.Status == StatusAula.FaltaSemReagendamento) && vencidas == false
                           )
                           ||
                           (
                             x.ProgramacaoAula.DataProgramada < Constante.Today
                             && (x.ProgramacaoAula.Status == StatusAula.Executada || x.ProgramacaoAula.Status == StatusAula.FaltaSemReagendamento) && vencidas == true && x.ProgramacaoAula.TipoAula == TipoAula.Avulsa &&
                             (x.ProgramacaoAula.StatusFinanceiro == StatusParcela.Aberto || x.ProgramacaoAula.StatusFinanceiro == StatusParcela.ConciliadoParcial)
                             )
                           )
                           ).ToListAsync();

            foreach (var it in l)
            {
                StudioParcelaView sp = new()
                {
                    ProgramacaoAula = it.ProgramacaoAula
                };
                sp.Id = it.ProgramacaoAula.Id;
                sp.AlunoId = it.ProgramacaoAula.AlunoId;
                sp.AlunoPlanoId = it.ProgramacaoAula.AlunoPlanoId;
                sp.NomeAluno = it.NomeAluno;
                sp.Descricao = it.NomeAula + " - " + it.NomeProfessor;
                sp.TipoAula = TipoAula.Avulsa;
                sp.DataVencto = it.ProgramacaoAula.DataProgramada;
                sp.DataPagto = it.ProgramacaoAula.DataPagto;
                sp.Status = it.ProgramacaoAula.StatusFinanceiro;
                sp.Valor = it.ProgramacaoAula.Valor;
                sp.ValorPago = it.ProgramacaoAula.ValorPago;
                sp.ValorConciliado = it.ProgramacaoAula.ValorConciliado;
                sp.NotaFiscal = it.ProgramacaoAula.NotaFiscal;
                sp.Faturado = it.ProgramacaoAula.Faturado;
                sp.RelacionamentoId = it.RelacionamentoAlunoId;
                r.Add(sp);
            }

            var lp = await (from AlunoPlanoParcela in _context.AlunoPlanoParcela
                            join AlunoPlano in _context.AlunoPlano on AlunoPlanoParcela.AlunoPlanoId equals AlunoPlano.Id
                            join Plano in _context.Plano on AlunoPlano.PlanoId equals Plano.Id
                            join Aluno in _context.Aluno on AlunoPlano.AlunoId equals Aluno.Id
                            join RelacionamentoAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelacionamentoAluno.Id
                            select new
                            {
                                AlunoPlanoParcela,
                                NomePlano = Plano.Nome,
                                Plano.TipoPlano,
                                NomeAluno = RelacionamentoAluno.Nome,
                                RelacionamentoAlunoId = RelacionamentoAluno.Id,
                                AlunoId = Aluno.Id
                            }).AsNoTracking().Where(x => x.AlunoPlanoParcela.TenantId == si.TenantId && x.AlunoPlanoParcela.Valor > 0
                            &&
                            (
                               (x.AlunoPlanoParcela.DataVencto >= dti && x.AlunoPlanoParcela.DataVencto <= dtf && vencidas == false)
                                ||
                               (x.AlunoPlanoParcela.DataVencto < Constante.Today && (x.AlunoPlanoParcela.Status == StatusParcela.Aberto || x.AlunoPlanoParcela.Status == StatusParcela.ConciliadoParcial) && vencidas == true)
                            )
                            ).ToListAsync();

            foreach (var it in lp)
            {
                StudioParcelaView sp = new()
                {
                    AlunoPlanoParcela = it.AlunoPlanoParcela
                };
                sp.Id = it.AlunoPlanoParcela.Id;
                sp.NomeAluno = it.NomeAluno;
                sp.AlunoId = it.AlunoId;
                sp.AlunoPlanoId = it.AlunoPlanoParcela.AlunoPlanoId;
                sp.Descricao = it.NomePlano;
                if (it.TipoPlano == TipoPlano.PacoteQtdeAula)
                {
                    sp.TipoAula = TipoAula.Pacote;
                }
                else
                {
                    sp.TipoAula = TipoAula.Plano;
                }
                sp.DataVencto = it.AlunoPlanoParcela.DataVencto;
                sp.DataPagto = it.AlunoPlanoParcela.DataPagto;
                sp.Status = it.AlunoPlanoParcela.Status;
                sp.Valor = it.AlunoPlanoParcela.Valor;
                sp.ValorPago = it.AlunoPlanoParcela.ValorPago;
                sp.ValorConciliado = it.AlunoPlanoParcela.ValorConciliado;
                sp.Faturado = it.AlunoPlanoParcela.Faturado;
                sp.NotaFiscal = it.AlunoPlanoParcela.NotaFiscal;
                sp.RelacionamentoId = it.RelacionamentoAlunoId;
                r.Add(sp);
            }
            if (alunoid != 0)
            {
                r = r.Where(x => x.AlunoId == alunoid).ToList();
            }
            if (statusParcelas.Count > 0)
            {
                r = r.Where(x => statusParcelas.Contains(x.Status)).ToList();
            }
            return r.OrderBy(x => x.DataVencto).ToList();
        }
        public async Task<List<StudioParcelaView>> FindAllParcelasPagasView(DateTime dti, DateTime dtf)
        {
            SessionInfo si = await GetSessionAsync();
            List<StudioParcelaView> r = new();
            var l = await (from ProgramacaoAula in _context.ProgramacaoAula
                           join Aula in _context.Aula on ProgramacaoAula.AulaId equals Aula.Id
                           join Professor in _context.Professor on ProgramacaoAula.ProfessorId equals Professor.Id
                           join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                           join Aluno in _context.Aluno on ProgramacaoAula.AlunoId equals Aluno.Id
                           join RelacionamentoAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelacionamentoAluno.Id
                           join Studio in _context.Studio on ProgramacaoAula.StudioId equals Studio.Id
                           select new
                           {
                               ProgramacaoAula,
                               NomeAula = Aula.Nome,
                               NomeProfessor = Relacionamento.Nome,
                               Aula.SemCusto,
                               NomeAluno = RelacionamentoAluno.Nome,
                               RelacionamentoAlunoId = RelacionamentoAluno.Id
                           }).AsNoTracking().Where(x => x.ProgramacaoAula.TenantId == si.TenantId && x.SemCusto == false
                           &&
                           (
                           x.ProgramacaoAula.DataPagto >= dti && x.ProgramacaoAula.DataPagto <= dtf
                           && x.ProgramacaoAula.TipoAula == TipoAula.Avulsa && (x.ProgramacaoAula.Status == StatusAula.Executada || x.ProgramacaoAula.Status == StatusAula.FaltaSemReagendamento)
                           )
                           ).ToListAsync();

            foreach (var it in l)
            {
                StudioParcelaView sp = new()
                {
                    ProgramacaoAula = it.ProgramacaoAula
                };
                sp.Id = it.ProgramacaoAula.Id;
                sp.AlunoId = it.ProgramacaoAula.AlunoId;
                sp.AlunoPlanoId = it.ProgramacaoAula.AlunoPlanoId;
                sp.NomeAluno = it.NomeAluno;
                sp.Descricao = it.NomeAula + " - " + it.NomeProfessor;
                sp.TipoAula = TipoAula.Avulsa;
                sp.DataVencto = it.ProgramacaoAula.DataProgramada;
                sp.DataPagto = it.ProgramacaoAula.DataPagto;
                sp.Status = it.ProgramacaoAula.StatusFinanceiro;
                sp.Valor = it.ProgramacaoAula.Valor;
                sp.ValorPago = it.ProgramacaoAula.ValorPago;
                sp.NotaFiscal = it.ProgramacaoAula.NotaFiscal;
                sp.Faturado = it.ProgramacaoAula.Faturado;
                sp.RelacionamentoId = it.RelacionamentoAlunoId;
                r.Add(sp);
            }

            var lp = await (from AlunoPlanoParcela in _context.AlunoPlanoParcela
                            join AlunoPlano in _context.AlunoPlano on AlunoPlanoParcela.AlunoPlanoId equals AlunoPlano.Id
                            join Plano in _context.Plano on AlunoPlano.PlanoId equals Plano.Id
                            join Aluno in _context.Aluno on AlunoPlano.AlunoId equals Aluno.Id
                            join RelacionamentoAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelacionamentoAluno.Id
                            select new
                            {
                                AlunoPlanoParcela,
                                NomePlano = Plano.Nome,
                                Plano.TipoPlano,
                                NomeAluno = RelacionamentoAluno.Nome,
                                RelacionamentoAlunoId = RelacionamentoAluno.Id,
                                AlunoId = Aluno.Id
                            }).AsNoTracking().Where(x => x.AlunoPlanoParcela.TenantId == si.TenantId && x.AlunoPlanoParcela.Valor > 0
                            && x.AlunoPlanoParcela.DataPagto >= dti && x.AlunoPlanoParcela.DataPagto <= dtf
                            && (x.AlunoPlanoParcela.Status == StatusParcela.Conciliado || x.AlunoPlanoParcela.Status == StatusParcela.ConciliadoParcial || x.AlunoPlanoParcela.Status == StatusParcela.Pago)
                            ).ToListAsync();

            foreach (var it in lp)
            {
                StudioParcelaView sp = new()
                {
                    AlunoPlanoParcela = it.AlunoPlanoParcela
                };
                sp.Id = it.AlunoPlanoParcela.Id;
                sp.NomeAluno = it.NomeAluno;
                sp.AlunoId = it.AlunoId;
                sp.AlunoPlanoId = it.AlunoPlanoParcela.AlunoPlanoId;
                sp.Descricao = it.NomePlano;
                if (it.TipoPlano == TipoPlano.PacoteQtdeAula)
                {
                    sp.TipoAula = TipoAula.Pacote;
                }
                else
                {
                    sp.TipoAula = TipoAula.Plano;
                }
                sp.DataVencto = it.AlunoPlanoParcela.DataVencto;
                sp.DataPagto = it.AlunoPlanoParcela.DataPagto;
                sp.Status = it.AlunoPlanoParcela.Status;
                sp.Valor = it.AlunoPlanoParcela.Valor;
                sp.ValorPago = it.AlunoPlanoParcela.ValorPago;
                sp.Faturado = it.AlunoPlanoParcela.Faturado;
                sp.NotaFiscal = it.AlunoPlanoParcela.NotaFiscal;
                sp.RelacionamentoId = it.RelacionamentoAlunoId;
                r.Add(sp);
            }
            return r.OrderBy(x => x.DataVencto).ToList();
        }
        public async Task<StudioParcelaView> FindParcelasViewById(int id, OrigemStudioParcela o)
        {
            SessionInfo si = await GetSessionAsync();
            StudioParcelaView sp = new();
            if (o == OrigemStudioParcela.ProgramacaoAula)
            {
                var l = await (from ProgramacaoAula in _context.ProgramacaoAula
                               join Aula in _context.Aula on ProgramacaoAula.AulaId equals Aula.Id
                               join Professor in _context.Professor on ProgramacaoAula.ProfessorId equals Professor.Id
                               join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                               join Aluno in _context.Aluno on ProgramacaoAula.AlunoId equals Aluno.Id
                               join RelacionamentoAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelacionamentoAluno.Id
                               join Studio in _context.Studio on ProgramacaoAula.StudioId equals Studio.Id
                               select new
                               {
                                   ProgramacaoAula,
                                   NomeAula = Aula.Nome,
                                   NomeProfessor = Relacionamento.Nome,
                                   Aula.SemCusto,
                                   NomeAluno = RelacionamentoAluno.Nome,
                                   RelacionamentoAlunoId = RelacionamentoAluno.Id
                               }).AsNoTracking().Where(x => x.ProgramacaoAula.TenantId == si.TenantId && x.ProgramacaoAula.Id == id).ToListAsync();

                foreach (var it in l)
                {
                    sp = new()
                    {
                        ProgramacaoAula = it.ProgramacaoAula
                    };
                    sp.Id = it.ProgramacaoAula.Id;
                    sp.AlunoId = it.ProgramacaoAula.AlunoId;
                    sp.AlunoPlanoId = it.ProgramacaoAula.AlunoPlanoId;
                    sp.NomeAluno = it.NomeAluno;
                    sp.Descricao = it.NomeAula + " - " + it.NomeProfessor;
                    sp.TipoAula = TipoAula.Avulsa;
                    sp.DataVencto = it.ProgramacaoAula.DataProgramada;
                    sp.DataPagto = it.ProgramacaoAula.DataPagto;
                    sp.Status = it.ProgramacaoAula.StatusFinanceiro;
                    sp.Valor = it.ProgramacaoAula.Valor;
                    sp.ValorPago = it.ProgramacaoAula.ValorPago;
                    sp.NotaFiscal = it.ProgramacaoAula.NotaFiscal;
                    sp.Faturado = it.ProgramacaoAula.Faturado;
                    sp.RelacionamentoId = it.RelacionamentoAlunoId;
                }
                return sp;
            }
            else
            {
                if (o == OrigemStudioParcela.PlanoParcela)
                {
                    var lp = await (from AlunoPlanoParcela in _context.AlunoPlanoParcela
                                    join AlunoPlano in _context.AlunoPlano on AlunoPlanoParcela.AlunoPlanoId equals AlunoPlano.Id
                                    join Plano in _context.Plano on AlunoPlano.PlanoId equals Plano.Id
                                    join Aluno in _context.Aluno on AlunoPlano.AlunoId equals Aluno.Id
                                    join RelacionamentoAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelacionamentoAluno.Id
                                    select new
                                    {
                                        AlunoPlanoParcela,
                                        NomePlano = Plano.Nome,
                                        Plano.TipoPlano,
                                        NomeAluno = RelacionamentoAluno.Nome,
                                        RelacionamentoAlunoId = RelacionamentoAluno.Id,
                                        AlunoId = Aluno.Id
                                    }).AsNoTracking().Where(x => x.AlunoPlanoParcela.TenantId == si.TenantId && x.AlunoPlanoParcela.Id == id).ToListAsync();

                    foreach (var it in lp)
                    {
                        sp = new()
                        {
                            AlunoPlanoParcela = it.AlunoPlanoParcela
                        };
                        sp.Id = it.AlunoPlanoParcela.Id;
                        sp.NomeAluno = it.NomeAluno;
                        sp.AlunoId = it.AlunoId;
                        sp.AlunoPlanoId = it.AlunoPlanoParcela.AlunoPlanoId;
                        sp.Descricao = it.NomePlano;
                        if (it.TipoPlano == TipoPlano.PacoteQtdeAula)
                        {
                            sp.TipoAula = TipoAula.Pacote;
                        }
                        else
                        {
                            sp.TipoAula = TipoAula.Plano;
                        }
                        sp.DataVencto = it.AlunoPlanoParcela.DataVencto;
                        sp.DataPagto = it.AlunoPlanoParcela.DataPagto;
                        sp.Status = it.AlunoPlanoParcela.Status;
                        sp.Valor = it.AlunoPlanoParcela.Valor;
                        sp.ValorPago = it.AlunoPlanoParcela.ValorPago;
                        sp.Faturado = it.AlunoPlanoParcela.Faturado;
                        sp.NotaFiscal = it.AlunoPlanoParcela.NotaFiscal;
                        sp.RelacionamentoId = it.RelacionamentoAlunoId;
                    }
                    return sp;
                }
                else
                {
                    return null;
                }
            }
        }
        public async Task<List<ProfessorLotePagtoView>> FindProfessorLotePagtoViewByDate(DateTime dtini, DateTime dtfim, int professorid = 0)
        {
            SessionInfo si = await GetSessionAsync();
            var r = await (from ProfessorLotePagto in _context.ProfessorLotePagto
                           join Professor in _context.Professor on ProfessorLotePagto.ProfessorId equals Professor.Id
                           join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                           select new ProfessorLotePagtoView()
                           {
                               NomeProfessor = Relacionamento.Nome,
                               Id = ProfessorLotePagto.Id,
                               RelacionamentoId = Relacionamento.Id,
                               Margem = ProfessorLotePagto.Margem,
                               MargemPercentual = ProfessorLotePagto.MargemPercentual,
                               ProfessorId = ProfessorLotePagto.ProfessorId,
                               Status = ProfessorLotePagto.Status,
                               TenantId = ProfessorLotePagto.TenantId,
                               TotalAulas = ProfessorLotePagto.TotalAulas,
                               ValorTotalAulas = ProfessorLotePagto.ValorTotalAulas,
                               ValorTotal = ProfessorLotePagto.ValorTotal,
                               ValorTotalPagar = ProfessorLotePagto.ValorTotalPagar,
                               Bonus = ProfessorLotePagto.Bonus,
                               BonusObs = ProfessorLotePagto.BonusObs,
                               DataFim = ProfessorLotePagto.DataFim,
                               DataInicio = ProfessorLotePagto.DataInicio,
                               DataPagto = ProfessorLotePagto.DataPagto,
                               Desconto = ProfessorLotePagto.Desconto,
                               DescontoObs = ProfessorLotePagto.DescontoObs,
                               Assinado = ProfessorLotePagto.Assinado,
                               DataAssinatura = ProfessorLotePagto.DataAssinatura
                           }
                           ).Where(x => x.TenantId == si.TenantId && x.DataFim >= dtini && x.DataInicio <= dtfim
                           ).ToListAsync();
            if (professorid != 0)
            {
                return r.Where(x => x.ProfessorId == professorid && x.Status != StatusProfessorLotePagto.Aberto).ToList();
            }
            else
            {
                return r;
            }
        }
        public async Task<List<ProfessorLotePagtoView>> GerarLotePagotProfessor(DateTime dtini, DateTime dtfim, int professorid = 0)
        {
            SessionInfo si = await GetSessionAsync();
            List<ProfessorLotePagtoView> r = new();
            List<ProfessorSalarioAula> lvalor = new();

            var lv = await (from ProfessorSalario in _context.ProfessorSalario
                            join ProfessorSalarioAula in _context.ProfessorSalarioAula on ProfessorSalario.Id equals ProfessorSalarioAula.ProfessorSalarioId
                            select new
                            {
                                DataRef = ProfessorSalario.DataInicio,
                                ProfessorSalario.ProfessorId,
                                ProfessorSalarioAula
                            }
                            ).AsNoTracking().Where(x => x.ProfessorSalarioAula.TenantId == si.TenantId).OrderByDescending(x => x.ProfessorId).OrderByDescending(x => x.DataRef).ToListAsync();
            foreach (var it in lv)
            {
                lvalor.Add(it.ProfessorSalarioAula);
                it.ProfessorSalarioAula.ProfessorId = it.ProfessorId;
                it.ProfessorSalarioAula.DataInicio = it.DataRef;
            }

            try
            {

                var l = await (from ProgramacaoAula in _context.ProgramacaoAula
                               join Aula in _context.Aula on ProgramacaoAula.AulaId equals Aula.Id
                               join ProfessorReal in _context.Professor on ProgramacaoAula.ProfessorRealId equals ProfessorReal.Id
                               join RelacionamentoReal in _context.Relacionamento on ProfessorReal.RelacionamentoId equals RelacionamentoReal.Id
                               join ProfessorAgenda in _context.Professor on ProgramacaoAula.ProfessorId equals ProfessorAgenda.Id
                               join RelacionamentoAgenda in _context.Relacionamento on ProfessorAgenda.RelacionamentoId equals RelacionamentoAgenda.Id
                               join Aluno in _context.Aluno on ProgramacaoAula.AlunoId equals Aluno.Id
                               join RelacionamentoAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelacionamentoAluno.Id
                               join Studio in _context.Studio on ProgramacaoAula.StudioId equals Studio.Id
                               select new
                               {
                                   ProgramacaoAula,
                                   NomeAula = Aula.Nome,
                                   NomeProfessorReal = RelacionamentoReal.Nome,
                                   NomeProfessorAgenda = RelacionamentoAgenda.Nome,
                                   NomeAluno = RelacionamentoAluno.Nome,
                                   NomeStudio = Studio.Nome
                               }
                               ).AsNoTracking().Where(x => x.ProgramacaoAula.TenantId == si.TenantId && x.ProgramacaoAula.DataProgramada >= dtini && x.ProgramacaoAula.DataProgramada <= dtfim
                               && (x.ProgramacaoAula.Status == StatusAula.Executada || x.ProgramacaoAula.Status == StatusAula.FaltaSemReagendamento || x.ProgramacaoAula.Status == StatusAula.Reserva)
                               && x.ProgramacaoAula.ProfessorLotePagtoId == 0).OrderBy(x => x.NomeProfessorAgenda).ThenBy(x => x.ProgramacaoAula.DataProgramada).ToListAsync();

                ProfessorLotePagtoView lt = null;
                foreach (var it in l)
                {
                    if (it.ProgramacaoAula.Status == StatusAula.Executada)
                    {
                        lt = r.FirstOrDefault(x => x.ProfessorId == it.ProgramacaoAula.ProfessorRealId);
                        if (lt == null)
                        {
                            lt = new()
                            {
                                ProfessorId = it.ProgramacaoAula.ProfessorRealId,
                                DataInicio = dtini,
                                DataFim = dtfim,
                                NomeProfessor = it.NomeProfessorReal
                            };
                            r.Add(lt);
                        }
                    }
                    else
                    {
                        lt = r.FirstOrDefault(x => x.ProfessorId == it.ProgramacaoAula.ProfessorId);
                        if (lt == null)
                        {
                            lt = new()
                            {
                                ProfessorId = it.ProgramacaoAula.ProfessorRealId,
                                DataInicio = dtini,
                                DataFim = dtfim,
                                NomeProfessor = it.NomeProfessorAgenda
                            };
                            r.Add(lt);
                        }
                    }
                    ProgramacaoAulaPagtoView a = new()
                    {
                        ProgramacaoAulaId = it.ProgramacaoAula.Id,
                        AlunoId = it.ProgramacaoAula.AulaId,
                        DataRef = it.ProgramacaoAula.DataProgramada,
                        AulaId = it.ProgramacaoAula.AulaId,
                        Status = it.ProgramacaoAula.Status,
                        NomeAluno = it.NomeAluno,
                        NomeAula = it.NomeAula,
                        NomeStudio = it.NomeStudio,
                        StatusFinanceiro = it.ProgramacaoAula.StatusFinanceiro,
                        TipoAula = it.ProgramacaoAula.TipoAula,
                        ValorAula = it.ProgramacaoAula.Valor
                    };
                    lt.ProgramacaoAulaPagtos.Add(a);

                    ProfessorSalarioAula va = lvalor.FirstOrDefault(x => x.ProfessorId == lt.ProfessorId && x.AulaId == a.AulaId && x.DataInicio <= a.DataRef);
                    if (va == null)
                    {
                        a.Erro = "Professor sem tabela de valores para pagamento.";
                        continue;
                    }
                    if (a.Status == StatusAula.Executada)
                    {
                        if (va.PercentualExecutada)
                        {
                            a.ValorProfessor = a.ValorAula * va.ValorExecutada / 100;
                        }
                        else
                        {
                            a.ValorProfessor = va.ValorExecutada;
                        }
                    }
                    if (a.Status == StatusAula.FaltaSemReagendamento)
                    {
                        if (va.PercentualFalta)
                        {
                            a.ValorProfessor = a.ValorAula * va.ValorFalta / 100;
                        }
                        else
                        {
                            a.ValorProfessor = va.ValorFalta;
                        }
                    }
                    if (a.Status == StatusAula.Reserva)
                    {
                        if (va.PercentualReserva)
                        {
                            a.ValorProfessor = a.ValorAula * va.ValorReserva / 100;
                        }
                        else
                        {
                            a.ValorProfessor = va.ValorReserva;
                        }
                    }
                    if (a.TipoAula == TipoAula.Teste)
                    {
                        a.ValorProfessor = 0;
                    }
                }

                foreach (var it in r)
                {
                    it.TotalAulas = it.ProgramacaoAulaPagtos.Count;
                    it.ValorTotal = it.ProgramacaoAulaPagtos.Sum(x => x.ValorProfessor);
                    it.ValorTotalAulas = it.ProgramacaoAulaPagtos.Sum(x => x.ValorAula);
                    it.Margem = it.ValorTotalAulas - it.ValorTotal;
                    if (it.ValorTotalAulas != 0)
                    {
                        it.MargemPercentual = it.Margem / it.ValorTotalAulas * 100;
                    }
                    it.ValorTotalPagar = it.ValorTotal + it.Bonus - it.Desconto;
                    it.ProgramacaoAulaPagtos = it.ProgramacaoAulaPagtos.OrderBy(x => x.DataRef).ThenBy(x => x.NomeAluno).ToList();
                }
                r = r.OrderBy(x => x.NomeProfessor).ToList();
            }
            catch (Exception e)
            {
                if (e.Message == string.Empty)
                    return null;
            }
            if (professorid != 0)
            {
                r = r.Where(x => x.ProfessorId == professorid).ToList();
            }
            return r;
        }
        public async Task<Resultado> SaveProfessorLote(ProfessorLotePagtoView pl)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (pl.BonusObs == null)
            {
                pl.BonusObs = string.Empty;
            }
            if (pl.DescontoObs == null)
            {
                pl.DescontoObs = string.Empty;
            }

            await _context.Database.BeginTransactionAsync();
            try
            {
                ProfessorLotePagto lpg = new()
                {
                    TenantId = si.TenantId,
                    DataInicio = pl.DataInicio,
                    DataFim = pl.DataFim,
                    ProfessorId = pl.ProfessorId,
                    Status = pl.Status,
                    Bonus = pl.Bonus,
                    Desconto = pl.Desconto,
                    Margem = pl.Margem,
                    MargemPercentual = pl.MargemPercentual,
                    DescontoObs = pl.DescontoObs,
                    BonusObs = pl.BonusObs,
                    TotalAulas = pl.TotalAulas,
                    ValorTotal = pl.ValorTotal,
                    ValorTotalAulas = pl.ValorTotalAulas,
                    ValorTotalPagar = pl.ValorTotalPagar,
                    Assinado = false,
                    DataAssinatura = DateTime.MinValue
                };
                await _context.ProfessorLotePagto.AddAsync(lpg);
                await _context.SaveChangesAsync();
                pl.Id = lpg.Id;
                foreach (var a in pl.ProgramacaoAulaPagtos)
                {
                    ProgramacaoAula pa = await _context.ProgramacaoAula.FirstOrDefaultAsync(x => x.Id == a.ProgramacaoAulaId);
                    if (pa == null)
                    {
                        throw new Exception("Programação aula não encontrada:" + a.ProgramacaoAulaId.ToString());
                    }

                    pa.ValorProfessor = a.ValorProfessor;
                    pa.ProfessorLotePagtoId = lpg.Id;
                    _context.ProgramacaoAula.Update(pa);
                }
                await _context.SaveChangesAsync();

                await _context.Database.CommitTransactionAsync();
                _context.ChangeTracker.Clear();
                r.Ok = true;
                return r;
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                _context.ChangeTracker.Clear();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<Resultado> AssinarProfessor(int id)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();
            var lu = await _context.ProfessorLotePagto.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.Id == id);
            if (lu == null)
            {
                r.Ok = false;
                r.ErrMsg = "Lote não encontrado";
                return r;
            }
            if (lu.Assinado)
            {
                r.Ok = false;
                r.ErrMsg = "Lote não encontrado";
                return r;
            }

            lu.Assinado = true;
            lu.DataAssinatura = Constante.Now;
            lu.Status = StatusProfessorLotePagto.Liberado;
            _context.ProfessorLotePagto.Update(lu);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            r.Ok = true;
            return r;
        }
        public async Task<Resultado> ExcluirProfessorLote(int id)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            ProfessorLotePagto l = await _context.ProfessorLotePagto.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == si.TenantId);
            if (l.Status != StatusProfessorLotePagto.Aberto)
            {
                r.Ok = false;
                r.ErrMsg = "Somente lote com situação em aberto pode ser excluído.";
                return r;
            }

            var la = await _context.ProgramacaoAula.Where(x => x.TenantId == si.TenantId && x.ProfessorLotePagtoId == id).ToListAsync();
            foreach (var a in la)
            {
                a.ValorProfessor = 0;
                a.ProfessorLotePagtoId = 0;
            }
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ProgramacaoAula.UpdateRange(la);
                _context.ProfessorLotePagto.Remove(l);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
                _context.ChangeTracker.Clear();
                r.Ok = true;
            }
            catch (Exception e)
            {
                await _context.Database.RollbackTransactionAsync();
                _context.ChangeTracker.Clear();
                r.Ok = false;
                r.ErrMsg = e.Message;
            }
            return r;
        }
        public async Task<ProfessorLotePagtoView> FindProfessorLoteById(int id)
        {
            SessionInfo si = await GetSessionAsync();
            var l = await (from ProfessorLotePagto in _context.ProfessorLotePagto
                           join ProgramacaoAula in _context.ProgramacaoAula on ProfessorLotePagto.Id equals ProgramacaoAula.ProfessorLotePagtoId
                           join Aula in _context.Aula on ProgramacaoAula.AulaId equals Aula.Id
                           join Professor in _context.Professor on ProfessorLotePagto.ProfessorId equals Professor.Id
                           join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                           join Aluno in _context.Aluno on ProgramacaoAula.AlunoId equals Aluno.Id
                           join RelacionamentoAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelacionamentoAluno.Id
                           join Studio in _context.Studio on ProgramacaoAula.StudioId equals Studio.Id
                           select new
                           {
                               ProfessorLotePagto,
                               ProgramacaoAula,
                               NomeAula = Aula.Nome,
                               NomeProfessor = Relacionamento.Nome,
                               NomeAluno = RelacionamentoAluno.Nome,
                               NomeStudio = Studio.Nome
                           }
                           ).AsNoTracking().Where(x => x.ProfessorLotePagto.TenantId == si.TenantId && x.ProfessorLotePagto.Id == id).ToListAsync();

            ProfessorLotePagtoView r = new();
            r.ProgramacaoAulaPagtos = new();
            foreach (var it in l)
            {
                r.Id = it.ProfessorLotePagto.Id;
                r.Margem = it.ProfessorLotePagto.Margem;
                r.MargemPercentual = it.ProfessorLotePagto.MargemPercentual;
                r.NomeProfessor = it.NomeProfessor;
                r.ProfessorId = it.ProfessorLotePagto.ProfessorId;
                r.Status = it.ProfessorLotePagto.Status;
                r.TenantId = it.ProfessorLotePagto.TenantId;
                r.TotalAulas = it.ProfessorLotePagto.TotalAulas;
                r.ValorTotalAulas = it.ProfessorLotePagto.ValorTotalAulas;
                r.ValorTotal = it.ProfessorLotePagto.ValorTotal;
                r.ValorTotalPagar = it.ProfessorLotePagto.ValorTotalPagar;
                r.Bonus = it.ProfessorLotePagto.Bonus;
                r.BonusObs = it.ProfessorLotePagto.BonusObs;
                r.DataFim = it.ProfessorLotePagto.DataFim;
                r.DataInicio = it.ProfessorLotePagto.DataInicio;
                r.DataPagto = it.ProfessorLotePagto.DataPagto;
                r.Desconto = it.ProfessorLotePagto.Desconto;
                r.DescontoObs = it.ProfessorLotePagto.DescontoObs;
                r.Assinado = it.ProfessorLotePagto.Assinado;
                r.DataAssinatura = it.ProfessorLotePagto.DataAssinatura;
                ProgramacaoAulaPagtoView a = new()
                {
                    AlunoId = it.ProgramacaoAula.AlunoId,
                    AulaId = it.ProgramacaoAula.AulaId,
                    DataRef = it.ProgramacaoAula.DataProgramada,
                    NomeAluno = it.NomeAluno,
                    NomeAula = it.NomeAula,
                    NomeStudio = it.NomeStudio,
                    TipoAula = it.ProgramacaoAula.TipoAula,
                    ProfessorLotePagtoId = r.Id,
                    ProgramacaoAulaId = it.ProgramacaoAula.Id,
                    Status = it.ProgramacaoAula.Status,
                    ValorAula = it.ProgramacaoAula.Valor,
                    ValorProfessor = it.ProgramacaoAula.ValorProfessor,
                    StatusFinanceiro = it.ProgramacaoAula.StatusFinanceiro
                };
                r.ProgramacaoAulaPagtos.Add(a);

            }
            r.ProgramacaoAulaPagtos = r.ProgramacaoAulaPagtos.OrderBy(x => x.DataRef).ThenBy(x => x.NomeAluno).ToList();
            return r;
        }
        public async Task<Resultado> UpdateProfessorLotePagto(ProfessorLotePagtoView pl)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            ProfessorLotePagto p = await _context.ProfessorLotePagto.FirstOrDefaultAsync(x => x.Id == pl.Id && x.TenantId == si.TenantId);
            if (p == null)
            {
                r.Ok = false;
                r.ErrMsg = "Lote não encontrado.";
                return r;
            }

            if ((pl.Status == StatusProfessorLotePagto.Conciliado || pl.Status == StatusProfessorLotePagto.ConciliadoParcial || pl.Status == StatusProfessorLotePagto.Pago) &&
                pl.DataPagto <= Constante.Today.AddYears(-20))
            {
                r.Ok = false;
                r.ErrMsg = "Data de pagamento inválida.";
                return r;
            }

            p.Status = pl.Status;
            p.Bonus = pl.Bonus;
            p.BonusObs = pl.BonusObs;
            p.Desconto = pl.Desconto;
            p.DescontoObs = pl.DescontoObs;
            p.ValorTotalPagar = pl.ValorTotal + pl.Bonus - pl.Desconto;
            p.DataPagto = pl.DataPagto;
            if (p.Status == StatusProfessorLotePagto.Aberto || p.Status == StatusProfessorLotePagto.ConferenciaProfessor)
            {
                p.Assinado = false;
                p.DataAssinatura = DateTime.MinValue;
            }
            _context.ProfessorLotePagto.Update(p);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            return r;

        }
        public async Task<List<AlunoProntuario>> GetProntuarios(int id, DateTime dtini)
        {
            SessionInfo si = await GetSessionAsync();
            List<AlunoProntuario> r = new();

            var l = await (from AlunoProntuario in _context.AlunoProntuario
                           join Professor in _context.Professor on AlunoProntuario.ProfessorId equals Professor.Id
                           join Relacionamento in _context.Relacionamento on Professor.RelacionamentoId equals Relacionamento.Id
                           join Aluno in _context.Aluno on AlunoProntuario.AlunoId equals Aluno.Id
                           join RelacionamentoAluno in _context.Relacionamento on Aluno.RelacionamentoId equals RelacionamentoAluno.Id
                           select new
                           {
                               AlunoProntuario,
                               NomeProfessor = Relacionamento.Nome,
                               NomeAluno = RelacionamentoAluno.Nome
                           }
                           ).AsNoTracking().Where(x => x.AlunoProntuario.TenantId == si.TenantId && x.AlunoProntuario.AlunoId == id && x.AlunoProntuario.DataRegistro >= dtini).ToListAsync();
            foreach (var p in l)
            {
                p.AlunoProntuario.NomeProfessor = p.NomeProfessor;
                p.AlunoProntuario.NomeAluno = p.NomeAluno;
                r.Add(p.AlunoProntuario);
            };

            return r.OrderByDescending(x => x.DataRegistro).ToList();

        }
        public async Task<StudioResultadoView> GetResultadoFianceiro(int studioid, DateTime dtini, DateTime dtfim)
        {
            SessionInfo si = await GetSessionAsync();
            StudioResultadoView r = new()
            {
                StudioId = studioid,
                DtIni = dtini,
                DtFim = dtfim
            };

            List<PlanoConta> lpc = await _context.PlanoConta.Where(x => x.TenantId == si.TenantId).ToListAsync();
            List<PlanoGerencial> lpg = await _context.PlanoGerencial.Where(x => x.TenantId == si.TenantId).ToListAsync();

            StudioPeriodoContaView pcv = null;
            StudioPeriodoResultadoView pcr = null;
            DateTime dta = UtilsClass.GetInicio(dtini);
            dtfim = UtilsClass.GetUltimo(dtfim);
            while (dta < dtfim)
            {
                pcr = new()
                {
                    DataRef = dta
                };
                r.Periodos.Add(pcr);
                dta = UtilsClass.GetInicio(dta.AddDays(35));
            }

            StudioConfig scfg = await _context.StudioConfig.FirstOrDefaultAsync(x => x.TenantId == si.TenantId);
            PlanoConta pca = lpc.FirstOrDefault(x => x.Id == scfg.PlanoContaIdAluno);
            if (pca == null)
            {
                pca = new()
                {
                    Id = 99992212,
                    Nome = "Receita aulas",
                    Tipo = TipoPlanoConta.Receita
                };
                lpc.Add(pca);
            }
            PlanoConta pcp = lpc.FirstOrDefault(x => x.Id == scfg.PlanoContaIdProfessor);
            if (pcp == null)
            {
                pcp = new()
                {
                    Id = 99932222,
                    Nome = "Pagamento de professores",
                    Tipo = TipoPlanoConta.Despesa
                };
                lpc.Add(pcp);
            }

            ////
            //////
            //////
            ///
            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =


"            select   " +
"MovtoBanco.DataMovto DataPagto,  " +
"MovtoBancoStudioParcela.Valor ValorPago " +
"from AlunoPlanoParcela " +
"inner join MovtoBancoStudioParcela on MovtoBancoStudioParcela.StudioParcelaId = AlunoPlanoParcela.Id " +
"inner " +
"join MovtoBanco on MovtoBanco.Id = MovtoBancoStudioParcela.MovtoBancoId " +
"where AlunoPlanoParcela.tenantid = @tid1 " +
"and MovtoBanco.DataMovto >=@dtini1 and MovtoBanco.DataMovto <= @dtfim1 " +
"and MovtoBanco.TipoMovtoBanco = 1 " +
"and MovtoBanco.Transferencia = 0 " +
"and MovtoBancoStudioParcela.OrigemParcela = 2 " +
"union all " +
" " +
"select " +
"AlunoPlanoParcela.DataPagto, " +
"Max(AlunoPlanoParcela.ValorPago) - sum(Isnull(MovtoBancoStudioParcela.Valor, 0.0)) ValorPago " +
" from AlunoPlanoParcela " +
" left join MovtoBancoStudioParcela on MovtoBancoStudioParcela.StudioParcelaId = AlunoPlanoParcela.Id and " +
"MovtoBancoStudioParcela.OrigemParcela = 2 " +
"where AlunoPlanoParcela.tenantid = @tid2 " +
"and AlunoPlanoParcela.DataPagto >=@dtini2 and AlunoPlanoParcela.DataPagto <= @dtfim2 " +
"group by " +
"AlunoPlanoParcela.Id, " +
"AlunoPlanoParcela.DataPagto " +
" " +
"union all " +
" " +
"select " +
"ProgramacaoAula.DataPagto, " +
"Max(ProgramacaoAula.ValorPago) - sum(Isnull(MovtoBancoStudioParcela.Valor, 0.0)) ValorPago " +
" from ProgramacaoAula " +
" left join MovtoBancoStudioParcela on MovtoBancoStudioParcela.StudioParcelaId = ProgramacaoAula.Id and " +
"MovtoBancoStudioParcela.OrigemParcela = 1 " +
"where ProgramacaoAula.tenantid = @tid3 " +
"and ProgramacaoAula.DataPagto >=@dtini3 and ProgramacaoAula.DataPagto <= @dtfim3 " +
"group by " +
"ProgramacaoAula.Id, " +
"ProgramacaoAula.DataPagto " +
" " +
"union all " +
" " +
"select " +
"MovtoBanco.DataMovto DataPagto, " +
"MovtoBancoStudioParcela.Valor ValorPago " +
"from ProgramacaoAula " +
"inner join MovtoBancoStudioParcela on MovtoBancoStudioParcela.StudioParcelaId = ProgramacaoAula.Id " +
"inner join  MovtoBanco on MovtoBanco.Id = MovtoBancoStudioParcela.MovtoBancoId " +
"where ProgramacaoAula.tenantid = @tid4 " +
"and MovtoBanco.DataMovto >=@dtini4 and MovtoBanco.DataMovto <= @dtfim4 " +
"and MovtoBanco.TipoMovtoBanco = 1 " +
"and MovtoBanco.Transferencia = 0 " +
"and MovtoBancoStudioParcela.OrigemParcela = 1 " +
"and TipoAula = 3 ";

            command.Parameters.Add(new SqlParameter("@tid1", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid1"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@dtini1", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini1"].Value = dtini.ToString("yyyyMMdd");
            command.Parameters.Add(new SqlParameter("@dtfim1", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim1"].Value = dtfim.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@tid2", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid2"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@dtini2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini2"].Value = dtini.ToString("yyyyMMdd");
            command.Parameters.Add(new SqlParameter("@dtfim2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim2"].Value = dtfim.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@tid3", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid3"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@dtini3", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini3"].Value = dtini.ToString("yyyyMMdd");
            command.Parameters.Add(new SqlParameter("@dtfim3", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim3"].Value = dtfim.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@tid4", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid4"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@dtini4", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini4"].Value = dtini.ToString("yyyyMMdd");
            command.Parameters.Add(new SqlParameter("@dtfim4", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim4"].Value = dtfim.ToString("yyyyMMdd");



            bool mycon = command.Connection.State != System.Data.ConnectionState.Open;
            if (mycon)
            {
                await command.Connection.OpenAsync();
            }
            List<StudioParcelaView> pv = new();
            try
            {
                var lu = await command.ExecuteReaderAsync();

                while (await lu.ReadAsync())
                {
                    StudioParcelaView np = new()
                    {
                        DataPagto = (DateTime)lu["DataPagto"],
                        ValorPago = (double)lu["ValorPago"]
                    };
                    pv.Add(np);
                }
            }
            catch (Exception e)
            {
                r.StudioNome = e.Message;
                if (mycon)
                {
                    await command.Connection.CloseAsync();
                }
            }
            finally
            {
                if (mycon)
                {
                    await command.Connection.CloseAsync();
                }
            }


            command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
"select  " +
"ProfessorLotePagto.ProfessorId,  " +
"MovtoBanco.DataMovto DataPagto,  " +
"MovtoBancoStudioParcela.Valor ValorPago,  " +
"Relacionamento.Nome NomeProfessor " +
"from ProfessorLotePagto  " +
"inner join Professor on Professor.Id = ProfessorLotePagto.ProfessorId  " +
"inner join Relacionamento on Relacionamento.Id = Professor.RelacionamentoId  " +
"inner join MovtoBancoStudioParcela on MovtoBancoStudioParcela.StudioParcelaId = ProfessorLotePagto.Id  " +
"inner join  MovtoBanco on MovtoBanco.Id = MovtoBancoStudioParcela.MovtoBancoId  " +
"where ProfessorLotePagto.tenantid = @tid1  " +
"and MovtoBanco.DataMovto >= @dtini1 and MovtoBanco.DataMovto <= @dtfim1  " +
"and MovtoBanco.TipoMovtoBanco = 2  " +
"and MovtoBanco.Transferencia = 0  " +
"and MovtoBancoStudioParcela.OrigemParcela = 3  " +
"union all  " +
"select  " +
"ProfessorLotePagto.ProfessorId,  " +
"ProfessorLotePagto.DataPagto,  " +
"Max(ProfessorLotePagto.ValorTotalPagar) - sum(Isnull(MovtoBancoStudioParcela.Valor, 0.0)) ValorPago,  " +
"Relacionamento.Nome NomeProfessor " +
"from ProfessorLotePagto  " +
"inner join Professor on Professor.Id = ProfessorLotePagto.ProfessorId  " +
"inner join Relacionamento on Relacionamento.Id = Professor.RelacionamentoId  " +
"left join MovtoBancoStudioParcela on MovtoBancoStudioParcela.StudioParcelaId = ProfessorLotePagto.Id and  " +
"MovtoBancoStudioParcela.OrigemParcela = 3  " +
"where ProfessorLotePagto.tenantid = @tid2  " +
"and ProfessorLotePagto.DataPagto >= @dtini2 and ProfessorLotePagto.DataPagto <= @dtfim2  " +
"group by  " +
"ProfessorLotePagto.ProfessorId,  " +
"ProfessorLotePagto.Id,  " +
"ProfessorLotePagto.DataPagto,  " +
"Relacionamento.Nome";

            command.Parameters.Add(new SqlParameter("@tid1", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid1"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@dtini1", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini1"].Value = dtini.ToString("yyyyMMdd");
            command.Parameters.Add(new SqlParameter("@dtfim1", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim1"].Value = dtfim.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@tid2", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid2"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@dtini2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini2"].Value = dtini.ToString("yyyyMMdd");
            command.Parameters.Add(new SqlParameter("@dtfim2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim2"].Value = dtfim.ToString("yyyyMMdd");

            mycon = command.Connection.State != System.Data.ConnectionState.Open;
            if (mycon)
            {
                await command.Connection.OpenAsync();
            }
            List<ProfessorLotePagtoView> lp = new();
            try
            {
                var lu = await command.ExecuteReaderAsync();
                while (await lu.ReadAsync())
                {
                    ProfessorLotePagtoView nl = new()
                    {
                        DataPagto = (DateTime)lu["DataPagto"],
                        ValorTotalPagar = (double)lu["ValorPago"],
                        ProfessorId = (int)lu["ProfessorId"],
                        NomeProfessor = lu["NomeProfessor"].ToString()
                    };
                    lp.Add(nl);
                }
            }
            catch (Exception e)
            {
                r.StudioNome = e.Message;
                if (mycon)
                {
                    await command.Connection.CloseAsync();
                }
            }
            finally
            {
                if (mycon)
                {
                    await command.Connection.CloseAsync();
                }
            }

            command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
            "            select  " +
            "Titulo.PlanoContaId, " +
            "MovtoBanco.DataMovto DataPagto, " +
            "MovtoBancoTitulo.Valor ValorPago " +
            "from Titulo " +
            "inner join TituloParcela on TituloParcela.TituloId = Titulo.Id " +
            "inner join MovtoBancoTitulo on MovtoBancoTitulo.TiTuloParcelaId = TituloParcela.Id " +
            "inner join MovtoBanco on MovtoBanco.Id = MovtoBancoTitulo.MovtoBancoId " +
            "where Titulo.tenantid = @tid1 " +
            "and MovtoBanco.DataMovto >= @dtini1 and MovtoBanco.DataMovto <= @dtfim1 " +
            "and MovtoBanco.Transferencia = 0 " +
            " " +
            "union all " +
            " " +
            "select " +
            "Titulo.PlanoContaId, " +
            "TituloParcela.DataPagto, " +
            "Max(TituloParcela.ValorPago) - sum(Isnull(MovtoBancoTitulo.Valor, 0.0)) ValorPago " +
            "from Titulo " +
            "inner join TituloParcela on TituloParcela.TituloId = Titulo.Id " +
            "left join MovtoBancoTitulo on MovtoBancoTitulo.TiTuloParcelaId = TituloParcela.Id " +
            "where Titulo.tenantid = @tid2 " +
            "and TituloParcela.DataPagto >= @dtini2 and TituloParcela.DataPagto <=  @dtfim2  " +
            "group by " +
            "TituloParcela.Id, " +
            "Titulo.PlanoContaId, " +
            "TituloParcela.DataPagto ";
            command.Parameters.Add(new SqlParameter("@tid1", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid1"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@dtini1", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini1"].Value = dtini.ToString("yyyyMMdd");
            command.Parameters.Add(new SqlParameter("@dtfim1", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim1"].Value = dtfim.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@tid2", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid2"].Value = si.TenantId;
            command.Parameters.Add(new SqlParameter("@dtini2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini2"].Value = dtini.ToString("yyyyMMdd");
            command.Parameters.Add(new SqlParameter("@dtfim2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim2"].Value = dtfim.ToString("yyyyMMdd");

            mycon = command.Connection.State != System.Data.ConnectionState.Open;
            if (mycon)
            {
                await command.Connection.OpenAsync();
            }
            List<TituloView> tp = new();
            try
            {
                var lu = await command.ExecuteReaderAsync();
                while (await lu.ReadAsync())
                {
                    TituloView nt = new()
                    {
                        Titulo = new()
                        {
                            PlanoContaId = (int)lu["PlanoContaId"]
                        },
                        TituloParcela = new()
                        {
                            DataPagto = (DateTime)lu["DataPagto"],
                            ValorPago = (double)lu["ValorPago"]
                        }
                    };
                    tp.Add(nt);
                }
            }
            catch (Exception e)
            {
                r.StudioNome = e.Message;
                if (mycon)
                {
                    await command.Connection.CloseAsync();
                }
            }
            finally
            {
                if (mycon)
                {
                    await command.Connection.CloseAsync();
                }
            }


            StudioContaView cv = null;
            StudioContaView cv2 = null;
            StudioContaView cv3 = null;
            PlanoGerencial pga = lpg.FirstOrDefault(x => x.Id == pca.PlanoGerencialId);
            if (pga == null)
            {
                pga = new()
                {
                    Id = 3432124,
                    Nome = "Receitas",
                    TipoPlanoConta = TipoPlanoConta.Receita
                };
                lpg.Add(pga);
            }
            PlanoGerencial pgp = lpg.FirstOrDefault(x => x.Id == pcp.PlanoGerencialId);
            if (pgp == null)
            {
                pgp = new()
                {
                    Id = 7765333,
                    Nome = "Despesas",
                    TipoPlanoConta = TipoPlanoConta.Despesa
                };
                lpg.Add(pgp);
            }
            PlanoGerencial pg = null;
            PlanoConta pc = null;
            foreach (var p in pv)
            {
                cv = r.Contas.FirstOrDefault(x => x.ContaId == pga.Id);
                if (cv == null)
                {
                    cv = new()
                    {
                        ContaId = pga.Id,
                        ContaNome = pga.Nome,
                        Tipo = pga.TipoPlanoConta
                    };
                    r.Contas.Add(cv);
                }
                cv2 = cv.Contas.FirstOrDefault(x => x.ContaId == pca.Id);
                if (cv2 == null)
                {
                    cv2 = new()
                    {
                        ContaId = pca.Id,
                        ContaNome = pca.Nome,
                        Tipo = pca.Tipo
                    };
                    cv.Contas.Add(cv2);
                }
                pcv = cv.Periodos.FirstOrDefault(x => x.MesAno == p.DataPagto.ToString("MM/yyyy"));
                if (pcv == null)
                {
                    pcv = new()
                    {
                        DataRef = p.DataPagto
                    };
                    cv.Periodos.Add(pcv);
                }
                pcv.Valor += p.ValorPago;

                pcv = cv2.Periodos.FirstOrDefault(x => x.MesAno == p.DataPagto.ToString("MM/yyyy"));
                if (pcv == null)
                {
                    pcv = new()
                    {
                        DataRef = p.DataPagto
                    };
                    cv2.Periodos.Add(pcv);
                }
                pcv.Valor += p.ValorPago;
            }

            foreach (var l in lp)
            {
                cv = r.Contas.FirstOrDefault(x => x.ContaId == pgp.Id);
                if (cv == null)
                {
                    cv = new()
                    {
                        ContaId = pgp.Id,
                        ContaNome = pgp.Nome,
                        Tipo = pgp.TipoPlanoConta
                    };
                    r.Contas.Add(cv);
                }
                pcv = cv.Periodos.FirstOrDefault(x => x.MesAno == l.DataPagto.ToString("MM/yyyy"));
                if (pcv == null)
                {
                    pcv = new()
                    {
                        DataRef = l.DataPagto
                    };
                    cv.Periodos.Add(pcv);
                }
                pcv.Valor += l.ValorTotalPagar;

                cv2 = cv.Contas.FirstOrDefault(x => x.ContaId == pcp.Id);
                if (cv2 == null)
                {
                    cv2 = new()
                    {
                        ContaId = pcp.Id,
                        ContaNome = pcp.Nome,
                        Tipo = pcp.Tipo
                    };
                    cv.Contas.Add(cv2);
                }
                pcv = cv2.Periodos.FirstOrDefault(x => x.MesAno == l.DataPagto.ToString("MM/yyyy"));
                if (pcv == null)
                {
                    pcv = new()
                    {
                        DataRef = l.DataPagto
                    };
                    cv2.Periodos.Add(pcv);
                }
                pcv.Valor += l.ValorTotalPagar;


                cv3 = cv2.Contas.FirstOrDefault(x => x.ContaId == l.ProfessorId);
                if (cv3 == null)
                {
                    cv3 = new()
                    {
                        ContaId = l.ProfessorId,
                        ContaNome = l.NomeProfessor,
                        Tipo = TipoPlanoConta.Despesa
                    };
                    cv2.Contas.Add(cv3);
                }
                pcv = cv3.Periodos.FirstOrDefault(x => x.MesAno == l.DataPagto.ToString("MM/yyyy"));
                if (pcv == null)
                {
                    pcv = new()
                    {
                        DataRef = l.DataPagto
                    };
                    cv3.Periodos.Add(pcv);
                }
                pcv.Valor += l.ValorTotalPagar;

            }

            foreach (var t in tp)
            {
                if (pc == null || pc.Id != t.PlanoContaId)
                {
                    pc = lpc.FirstOrDefault(x => x.Id == t.PlanoContaId);
                    if (pg == null || pg.Id != pc.PlanoGerencialId)
                    {
                        pg = lpg.FirstOrDefault(x => x.Id == pc.PlanoGerencialId);
                    }
                }

                cv = r.Contas.FirstOrDefault(x => x.ContaId == pc.PlanoGerencialId);
                if (cv == null)
                {
                    cv = new()
                    {
                        ContaId = pg.Id,
                        ContaNome = pg.Nome,
                        Tipo = pg.TipoPlanoConta
                    };
                    r.Contas.Add(cv);
                }

                cv2 = cv.Contas.FirstOrDefault(x => x.ContaId == t.PlanoContaId);
                if (cv2 == null)
                {
                    cv2 = new()
                    {
                        ContaId = t.PlanoContaId,
                        ContaNome = pc.Nome,
                        Tipo = pc.Tipo
                    };
                    cv.Contas.Add(cv2);
                }
                pcv = cv.Periodos.FirstOrDefault(x => x.MesAno == t.TituloParcela.DataPagto.ToString("MM/yyyy"));
                if (pcv == null)
                {
                    pcv = new()
                    {
                        DataRef = t.TituloParcela.DataPagto
                    };
                    cv.Periodos.Add(pcv);
                }
                pcv.Valor += t.TituloParcela.ValorPago;

                pcv = cv2.Periodos.FirstOrDefault(x => x.MesAno == t.TituloParcela.DataPagto.ToString("MM/yyyy"));
                if (pcv == null)
                {
                    pcv = new()
                    {
                        DataRef = t.TituloParcela.DataPagto
                    };
                    cv2.Periodos.Add(pcv);
                }
                pcv.Valor += t.TituloParcela.ValorPago;
            }

            command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
"select  " +
"MovtoBanco.PlanoContaId,  " +
"MovtoBanco.DataMovto, " +
"Max(MovtoBanco.Valor) - sum(Isnull(MovtoBancoStudioParcela.Valor, 0.0)) - sum(Isnull(MovtoBancoTitulo.Valor, 0.0)) Valor , " +
"Max(MovtoBanco.TipoMovtoBanco) TipoMovtoBanco " +
"from MovtoBanco " +
"left join MovtoBancoStudioParcela on MovtoBancoStudioParcela.MovtoBancoId = MovtoBanco.Id " +
"left join MovtoBancoTitulo on MovtoBancoTitulo.MovtoBancoId = MovtoBanco.Id " +
"where MovtoBanco.Tenantid = @tid " +
"and DataMovto  >= @dtini and DataMovto <= @dtfim " +
"and MovtoBanco.Transferencia = 0 " +
"group by " +
"MovtoBanco.Id, " +
"MovtoBanco.DataMovto, " +
"MovtoBanco.PlanoContaId ";

            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@dtini", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim"].Value = dtfim.ToString("yyyyMMdd");

            mycon = command.Connection.State != System.Data.ConnectionState.Open;
            if (mycon)
            {
                await command.Connection.OpenAsync();
            }
            try
            {
                var lu = await command.ExecuteReaderAsync();

                MovtoBanco mbi = new();
                while (await lu.ReadAsync())
                {
                    mbi.PlanoContaId = (int)lu["PlanoContaId"];
                    mbi.DataMovto = (DateTime)lu["DataMovto"];
                    mbi.Valor = (double)lu["Valor"];
                    mbi.TipoMovtoBanco = (TipoMovtoBanco)lu["TipoMovtoBanco"];
                    if (pcp == null || pcp.Id != mbi.PlanoContaId)
                    {
                        pcp = lpc.FirstOrDefault(x => x.Id == mbi.PlanoContaId);
                        if (pcp == null)
                        {
                            continue;
                        }
                        pgp = lpg.FirstOrDefault(x => x.Id == pcp.PlanoGerencialId);
                    }
                    cv = r.Contas.FirstOrDefault(x => x.ContaId == pgp.Id);
                    if (cv == null)
                    {
                        cv = new()
                        {
                            ContaId = pgp.Id,
                            ContaNome = pgp.Nome,
                            Tipo = pgp.TipoPlanoConta
                        };
                        r.Contas.Add(cv);
                    }
                    pcv = cv.Periodos.FirstOrDefault(x => x.MesAno == mbi.DataMovto.ToString("MM/yyyy"));
                    if (pcv == null)
                    {
                        pcv = new()
                        {
                            DataRef = mbi.DataMovto
                        };
                        cv.Periodos.Add(pcv);
                    }
                    if (cv.Tipo == TipoPlanoConta.Auxiliar)
                    {
                        if (mbi.TipoMovtoBanco == TipoMovtoBanco.Debito)
                        {
                            mbi.Valor *= -1; 
                        }
                    }
                    pcv.Valor += mbi.Valor;

                    cv2 = cv.Contas.FirstOrDefault(x => x.ContaId == pcp.Id);
                    if (cv2 == null)
                    {
                        cv2 = new()
                        {
                            ContaId = pcp.Id,
                            ContaNome = pcp.Nome,
                            Tipo = pcp.Tipo
                        };
                        cv.Contas.Add(cv2);
                    }
                    pcv = cv2.Periodos.FirstOrDefault(x => x.MesAno == mbi.DataMovto.ToString("MM/yyyy"));
                    if (pcv == null)
                    {
                        pcv = new()
                        {
                            DataRef = mbi.DataMovto
                        };
                        cv2.Periodos.Add(pcv);
                    }
                    pcv.Valor += mbi.Valor;
                }
            }
            catch (Exception e)
            {
                r.StudioNome = e.Message;
                if (mycon)
                {
                    await command.Connection.CloseAsync();
                }
            }
            finally
            {
                if (mycon)
                {
                    await command.Connection.CloseAsync();
                }
            }

            r.Receitas = r.Contas.Where(x => x.Tipo == TipoPlanoConta.Receita).OrderBy(x => x.ContaNome).ToList();
            r.Despesas = r.Contas.Where(x => x.Tipo == TipoPlanoConta.Despesa).OrderBy(x => x.ContaNome).ToList();
            r.Auxliares = r.Contas.Where(x => x.Tipo == TipoPlanoConta.Auxiliar).OrderBy(x => x.ContaNome).ToList();
            r.Periodos = r.Periodos.OrderBy(x => x.DataRef).ToList();
            foreach (var c in r.Receitas)
            {
                foreach (var cr1 in c.Contas)
                {
                    cr1.Periodos = cr1.Periodos.OrderBy(x => x.DataRef).ToList();
                    foreach (var p1 in cr1.Periodos)
                    {
                        pcr = r.Periodos.FirstOrDefault(x => x.MesAno == p1.MesAno);
                        if (pcr == null)
                        {
                            pcr = new()
                            {
                                DataRef = p1.DataRef
                            };
                            r.Periodos.Add(pcr);
                        }
                        if (scfg.PlanoContaDefaultAporteId == cr1.ContaId)
                        {
                            pcr.Aportes += p1.Valor;
                        }
                        pcr.ReceitaTotal += p1.Valor;
                        pcr.TemMovto = true;
                    }
                }
            }
            foreach (var c in r.Despesas)
            {
                foreach (var cr1 in c.Contas)
                {
                    cr1.Periodos = cr1.Periodos.OrderBy(x => x.DataRef).ToList();
                    foreach (var p1 in cr1.Periodos)
                    {
                        pcr = r.Periodos.FirstOrDefault(x => x.MesAno == p1.MesAno);
                        if (pcr == null)
                        {
                            pcr = new()
                            {
                                DataRef = p1.DataRef
                            };
                            r.Periodos.Add(pcr);
                        }
                        if (scfg.PlanoContaDefaultDividendoId == cr1.ContaId)
                        {
                            pcr.Dividendos += p1.Valor;
                        }
                        pcr.DespesaTotal += p1.Valor;
                        pcr.TemMovto = true;
                    }
                }
            }
            r.Periodos = r.Periodos.Where(x => x.TemMovto == true).OrderBy(x => x.DataRef).ToList();
            return r;
        }
        public async Task<StudioResultadoMovtoBancoView> GetPosicaoContaCorrenteByDate(DateTime dtini, DateTime dtfim)
        {
            SessionInfo si = await GetSessionAsync();
            StudioResultadoMovtoBancoView r = new();

            StudioPeriodoMovtoBancoView pg = null;
            DateTime dta = UtilsClass.GetInicio(dtini);
            dtfim = UtilsClass.GetUltimo(dtfim);
            while (dta < dtfim)
            {
                pg = new()
                {
                    DataRef = dta
                };
                r.Periodos.Add(pg);
                dta = UtilsClass.GetInicio(dta.AddDays(35));
            }

            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
        "            select " +
        "Studio.Id StudioId, " +
        "Max(Studio.Nome) StudioNome, " +
        "ContaCorrente.Id ContaCorrenteId, " +
        "max(MovtoBanco.DataMovto) DataMovto, " +
        "max(ContaCorrente.Nome) ContaCorrenteNome, " +
         "Sum(case when MovtoBanco.DataMovto >= @dtini1 then " +
        "0.0 " +
        "else " +
        "     case when MovtoBanco.TipoMovtoBanco = 2 then  MovtoBanco.Valor * -1 else MovtoBanco.Valor end end )  SaldoAnterior, " +


        "sum( " +
        "case when  MovtoBanco.DataMovto < @dtini2 and MovtoBanco.TipoMovtoBanco = 1 and MovtoBanco.Transferencia = 0 then " +
        " MovtoBanco.Valor " +
        "else " +
        "  0.0 end )  EntradasAnterior, " +
        "sum(case when  MovtoBanco.DataMovto < @dtini3 and MovtoBanco.TipoMovtoBanco = 2 and MovtoBanco.Transferencia = 0 then " +
        "MovtoBanco.Valor * -1 " +
        "else " +
        "                0.0 end )  SaidasAnterior, " +
        "Sum(case when  MovtoBanco.DataMovto < @dtini4 and Transferencia = 1 then " +
        "   case when MovtoBanco.TipoMovtoBanco = 2 then  MovtoBanco.Valor * -1 else MovtoBanco.Valor end " +
        "else " +
        "                0.0 end )  TransferenciasAnterior, " +

        "sum( " +
        "case when  MovtoBanco.DataMovto <= @dtfim and MovtoBanco.TipoMovtoBanco = 1 and MovtoBanco.Transferencia = 0 then " +
        " MovtoBanco.Valor " +
        "else " +
        "  0.0 end )  EntradasFinal, " +
        "sum(case when  MovtoBanco.DataMovto <= @dtfim and MovtoBanco.TipoMovtoBanco = 2 and MovtoBanco.Transferencia = 0 then " +
        "MovtoBanco.Valor * -1 " +
        "else " +
        "                0.0 end )  SaidasFinal, " +
        "Sum(case when  MovtoBanco.DataMovto <= @dtfim and Transferencia = 1 then " +
        "   case when MovtoBanco.TipoMovtoBanco = 2 then  MovtoBanco.Valor * -1 else MovtoBanco.Valor end " +
        "else " +
        "                0.0 end )  TransferenciasFinal, " +

        "sum( " +
        "case when  MovtoBanco.DataMovto >= @dtini2 and MovtoBanco.TipoMovtoBanco = 1 and MovtoBanco.Transferencia = 0 then " +
        "MovtoBanco.Valor " +
        "else " +
        "                0.0 end )  Entradas, " +
        "sum(case when  MovtoBanco.DataMovto >= @dtini3 and MovtoBanco.TipoMovtoBanco = 2 and MovtoBanco.Transferencia = 0 then " +
        "MovtoBanco.Valor * -1 " +
        "else " +
        "                0.0 end )  Saidas, " +
        "Sum(case when  MovtoBanco.DataMovto >= @dtini4 and Transferencia = 1 then " +
        "   case when MovtoBanco.TipoMovtoBanco = 2 then  MovtoBanco.Valor * -1 else MovtoBanco.Valor end " +
        "else " +
        "                0.0 end )  Transferencias " +
        "                from ContaCorrente " +
        "inner join MovtoBanco on MovtoBanco.ContaCorrenteId = ContaCorrente.Id " +
        "inner join ContaCorrenteStudio on ContaCorrenteStudio.ContaCorrenteId =  ContaCorrente.Id " +
        "inner join Studio on Studio.Id = ContaCorrenteStudio.StudioId  " +
        "where MovtoBanco.DataMovto <=  Convert(date, @dtfim, 112) and ContaCorrente.TenantId = @tid  " +
        "group by Substring(Convert(varchar(10),MovtoBanco.DataMovto,112),1,6),  Studio.Id, ContaCorrente.Id  ";


            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@dtini1", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini1"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtini2", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini2"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtini3", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini3"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtini4", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini4"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim"].Value = dtfim.ToString("yyyyMMdd");

            bool mycon = command.Connection.State != System.Data.ConnectionState.Open;
            if (mycon)
            {
                await command.Connection.OpenAsync();
            }
            var lu = await command.ExecuteReaderAsync();
            try
            {
                while (await lu.ReadAsync())
                {
                    StudioMovtoBancoContaView sct = r.Contas.FirstOrDefault(x => x.ContaId == (int)lu["StudioId"]);
                    if (sct == null)
                    {
                        sct = new()
                        {
                            ContaId = (int)lu["StudioId"],
                            ContaNome = lu["StudioNome"].ToString(),
                        };
                        r.Contas.Add(sct);
                    }
                    pg = r.Periodos.FirstOrDefault(x => x.MesAno == ((DateTime)lu["DataMovto"]).ToString("MM/yyyy"));
                    if (pg == null)
                    {
                        pg = new()
                        {
                            DataRef = (DateTime)lu["DataMovto"]
                        };
                        r.Periodos.Add(pg);
                    }
                    pg.TemMovto = true;
                    sct.SaldoAnterior += (double)lu["SaldoAnterior"];
                    StudioPeriodoMovtoBancoView ps = sct.Periodos.FirstOrDefault(x => x.MesAno == ((DateTime)lu["DataMovto"]).ToString("MM/yyyy"));
                    if (ps == null)
                    {
                        ps = new()
                        {
                            DataRef = (DateTime)lu["DataMovto"]
                        };
                        sct.Periodos.Add(ps);
                    }
                    ps.Entradas += (double)lu["Entradas"];
                    ps.Saidas += (double)lu["Saidas"];
                    ps.Transferencias += (double)lu["Transferencias"];

                    StudioMovtoBancoContaView cct = sct.Contas.FirstOrDefault(x => x.ContaId == (int)lu["ContaCorrenteId"]);
                    if (cct == null)
                    {
                        cct = new()
                        {
                            ContaId = (int)lu["ContaCorrenteId"],
                            ContaNome = lu["ContaCorrenteNome"].ToString()
                        };
                        sct.Contas.Add(cct);
                    }
                    cct.SaldoAnterior += (double)lu["SaldoAnterior"];
                    StudioPeriodoMovtoBancoView psc = cct.Periodos.FirstOrDefault(x => x.MesAno == ((DateTime)lu["DataMovto"]).ToString("MM/yyyy"));
                    if (psc == null)
                    {
                        psc = new()
                        {
                            DataRef = (DateTime)lu["DataMovto"]
                        };
                        cct.Periodos.Add(psc);
                    }
                    psc.Entradas += (double)lu["Entradas"];
                    psc.Saidas += (double)lu["Saidas"];
                    psc.Transferencias += (double)lu["Transferencias"];
                }
                r.Periodos = r.Periodos.Where(x => x.TemMovto == true).OrderBy(x => x.DataRef).ToList();
                foreach (var c in r.Contas)
                {
                    double sa = c.SaldoAnterior;
                    c.Periodos = c.Periodos.OrderBy(x => x.DataRef).ToList();
                    foreach (var p in c.Periodos)
                    {
                        p.SaldoAnterior = sa;
                        sa = p.SaldoFinal;
                    }
                    foreach (var cc in c.Contas)
                    {
                        double sa1 = cc.SaldoAnterior;
                        cc.Periodos = cc.Periodos.OrderBy(x => x.DataRef).ToList();
                        foreach (var p2 in cc.Periodos)
                        {
                            p2.SaldoAnterior = sa1;
                            sa1 = p2.SaldoFinal;
                        }
                    }
                }
            }
            finally
            {
                if (mycon)
                {
                    await command.Connection.CloseAsync();
                }
            }
            return r;
        }
        public async Task<StudioResultadoAulaView> GetResultadoAulas(DateTime dtini, DateTime dtfim)
        {
            SessionInfo si = await GetSessionAsync();
            StudioResultadoAulaView r = new();

            StudioPeriodoAulaView pg = null;
            DateTime dta = UtilsClass.GetInicio(dtini);
            dtfim = UtilsClass.GetUltimo(dtfim);
            while (dta < dtfim)
            {
                pg = new()
                {
                    DataRef = dta
                };
                r.Periodos.Add(pg);
                dta = UtilsClass.GetInicio(dta.AddDays(35));
            }


            var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText =
        "           select count(*) TotalAula, Relacionamento.Nome ProfessorNome, Max( ProgramacaoAula.DataProgramada) DataMovto, Aula.Nome AulaNome , Studio.Nome studionome, studio.Id studioid, " +
"case  " +
"when ProgramacaoAula.TipoAula = 1 or ProgramacaoAula.TipoAula = 0 then 'Plano' " +
"when ProgramacaoAula.TipoAula = 2 then 'Pacote' " +
"when ProgramacaoAula.TipoAula = 3 then 'Avulsa' " +
"when ProgramacaoAula.TipoAula = 4 then 'Experimental' end TipoAulaV " +
"from ProgramacaoAula " +
"inner join Aula on Aula.Id = ProgramacaoAula.AulaId " +
"inner " +
"join Professor on Professor.Id = ProgramacaoAula.ProfessorRealId " +
"inner " +
"join Relacionamento on Relacionamento.Id = Professor.RelacionamentoId " +
"inner " +
"join Studio on Studio.Id = ProgramacaoAula.StudioId " +
"where ProgramacaoAula.Status = 3 and ProgramacaoAula.DataProgramada >= Convert(date, @dtini, 112)  and ProgramacaoAula.DataProgramada <= Convert(date, @dtfim, 112) " +
"and  ProgramacaoAula.TenantId = @tid " +
"group by Relacionamento.Nome , SubString(Convert(varchar(10), ProgramacaoAula.DataProgramada, 112), 1, 6), Aula.Nome, Studio.Nome,  studio.Id,case  " +
"when ProgramacaoAula.TipoAula = 1  or ProgramacaoAula.TipoAula = 0 then 'Plano' " +
"when ProgramacaoAula.TipoAula = 2 then 'Pacote' " +
"when ProgramacaoAula.TipoAula = 3 then 'Avulsa' " +
"when ProgramacaoAula.TipoAula = 4 then 'Experimental' end ";


            command.Parameters.Add(new SqlParameter("@tid", System.Data.SqlDbType.NVarChar, 100));
            command.Parameters["@tid"].Value = si.TenantId;

            command.Parameters.Add(new SqlParameter("@dtini", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtini"].Value = dtini.ToString("yyyyMMdd");

            command.Parameters.Add(new SqlParameter("@dtfim", System.Data.SqlDbType.NVarChar, 8));
            command.Parameters["@dtfim"].Value = dtfim.ToString("yyyyMMdd");

            bool mycon = command.Connection.State != System.Data.ConnectionState.Open;
            if (mycon)
            {
                await command.Connection.OpenAsync();
            }

            try
            {
                var lu = await command.ExecuteReaderAsync();
                StudioContaAulaView cct = null;
                StudioContaAulaView cct2 = null;
                StudioPeriodoAulaView psc = null;

                while (await lu.ReadAsync())
                {
                    StudioContaAulaView sct = r.Contas.FirstOrDefault(x => x.ContaId == (int)lu["StudioId"]);
                    if (sct == null)
                    {
                        sct = new()
                        {
                            ContaId = (int)lu["StudioId"],
                            ContaNome = lu["StudioNome"].ToString(),
                            TipoConta = StudioTipoContaAulaView.Studio
                        };
                        r.Contas.Add(sct);
                    }
                    pg = r.Periodos.FirstOrDefault(x => x.MesAno == ((DateTime)lu["DataMovto"]).ToString("MM/yyyy"));
                    if (pg == null)
                    {
                        pg = new()
                        {
                            DataRef = (DateTime)lu["DataMovto"],
                        };
                        r.Periodos.Add(pg);
                    }
                    pg.Qtde += (int)lu["TotalAula"];
                    pg.TemMovto = true;
                    StudioPeriodoAulaView ps = sct.Periodos.FirstOrDefault(x => x.MesAno == ((DateTime)lu["DataMovto"]).ToString("MM/yyyy"));
                    if (ps == null)
                    {
                        ps = new()
                        {
                            DataRef = (DateTime)lu["DataMovto"]
                        };
                        sct.Periodos.Add(ps);
                    }
                    ps.Qtde += (int)lu["TotalAula"];

                    cct = sct.Contas.FirstOrDefault(x => x.ContaNome == lu["ProfessorNome"].ToString() && x.TipoConta == StudioTipoContaAulaView.Professor);
                    if (cct == null)
                    {
                        cct = new()
                        {
                            TipoConta = StudioTipoContaAulaView.Professor,
                            ContaNome = lu["ProfessorNome"].ToString()
                        };
                        sct.Contas.Add(cct);
                    }
                    psc = cct.Periodos.FirstOrDefault(x => x.MesAno == ((DateTime)lu["DataMovto"]).ToString("MM/yyyy"));
                    if (psc == null)
                    {
                        psc = new()
                        {
                            DataRef = (DateTime)lu["DataMovto"]
                        };
                        cct.Periodos.Add(psc);
                    }
                    psc.Qtde += (int)lu["TotalAula"];


                    cct2 = cct.Aulas.FirstOrDefault(x => x.ContaNome == lu["AulaNome"].ToString() && x.TipoConta == StudioTipoContaAulaView.Aula);
                    if (cct2 == null)
                    {
                        cct2 = new()
                        {
                            TipoConta = StudioTipoContaAulaView.Aula,
                            ContaNome = lu["AulaNome"].ToString()
                        };
                        cct.Aulas.Add(cct2);
                    }
                    psc = cct2.Periodos.FirstOrDefault(x => x.MesAno == ((DateTime)lu["DataMovto"]).ToString("MM/yyyy"));
                    if (psc == null)
                    {
                        psc = new()
                        {
                            DataRef = (DateTime)lu["DataMovto"]
                        };
                        cct2.Periodos.Add(psc);
                    }
                    psc.Qtde += (int)lu["TotalAula"];

                    cct2 = cct.TipoPlano.FirstOrDefault(x => x.ContaNome == lu["TipoAulaV"].ToString() && x.TipoConta == StudioTipoContaAulaView.TipoPlano);
                    if (cct2 == null)
                    {
                        cct2 = new()
                        {
                            TipoConta = StudioTipoContaAulaView.TipoPlano,
                            ContaNome = lu["TipoAulaV"].ToString()
                        };
                        cct.TipoPlano.Add(cct2);
                    }
                    psc = cct2.Periodos.FirstOrDefault(x => x.MesAno == ((DateTime)lu["DataMovto"]).ToString("MM/yyyy"));
                    if (psc == null)
                    {
                        psc = new()
                        {
                            DataRef = (DateTime)lu["DataMovto"]
                        };
                        cct2.Periodos.Add(psc);
                    }
                    psc.Qtde += (int)lu["TotalAula"];





                    cct = sct.Contas.FirstOrDefault(x => x.ContaNome == lu["AulaNome"].ToString() && x.TipoConta == StudioTipoContaAulaView.Aula);
                    if (cct == null)
                    {
                        cct = new()
                        {
                            TipoConta = StudioTipoContaAulaView.Aula,
                            ContaNome = lu["AulaNome"].ToString()
                        };
                        sct.Contas.Add(cct);
                    }
                    psc = cct.Periodos.FirstOrDefault(x => x.MesAno == ((DateTime)lu["DataMovto"]).ToString("MM/yyyy"));
                    if (psc == null)
                    {
                        psc = new()
                        {
                            DataRef = (DateTime)lu["DataMovto"]
                        };
                        cct.Periodos.Add(psc);
                    }
                    psc.Qtde += (int)lu["TotalAula"];


                    cct = sct.Contas.FirstOrDefault(x => x.ContaNome == lu["TipoAulaV"].ToString() && x.TipoConta == StudioTipoContaAulaView.TipoPlano);
                    if (cct == null)
                    {
                        cct = new()
                        {
                            TipoConta = StudioTipoContaAulaView.TipoPlano,
                            ContaNome = lu["TipoAulaV"].ToString()
                        };
                        sct.Contas.Add(cct);
                    }
                    psc = cct.Periodos.FirstOrDefault(x => x.MesAno == ((DateTime)lu["DataMovto"]).ToString("MM/yyyy"));
                    if (psc == null)
                    {
                        psc = new()
                        {
                            DataRef = (DateTime)lu["DataMovto"]
                        };
                        cct.Periodos.Add(psc);
                    }
                    psc.Qtde += (int)lu["TotalAula"];

                }
                await lu.CloseAsync();

                command.CommandText =
"select DataMovto, StudioNome, StudioId, Sum(qtdalunos) TotalAlunos " +
"from " +
"( " +
"select  max(ProgramacaoAula.DataProgramada) DataMovto, 1 qtdalunos, Studio.Nome StudioNome, Studio.Id StudioId from ProgramacaoAula " +
" inner join Studio on Studio.Id = ProgramacaoAula.StudioId " +
"where ProgramacaoAula.Status = 3 and ProgramacaoAula.DataProgramada >= Convert(date, @dtini, 112)  and ProgramacaoAula.DataProgramada <= Convert(date, @dtfim, 112) " +
"and  ProgramacaoAula.TenantId = @tid " +
" group by SubString(Convert(varchar(10), ProgramacaoAula.DataProgramada, 112), 1, 6), ProgramacaoAula.alunoid, Studio.Nome,Studio.Id " +
") AlunosAulas " +
"group by DataMovto,StudioNome, StudioId ";

                lu = await command.ExecuteReaderAsync();


                while (await lu.ReadAsync())
                {
                    StudioContaAulaView sct = r.Contas.FirstOrDefault(x => x.ContaId == (int)lu["StudioId"]);
                    if (sct == null)
                    {
                        sct = new()
                        {
                            ContaId = (int)lu["StudioId"],
                            ContaNome = lu["StudioNome"].ToString(),
                            TipoConta = StudioTipoContaAulaView.Studio
                        };
                        r.Contas.Add(sct);
                    }

                    cct = sct.Contas.FirstOrDefault(x => x.TipoConta == StudioTipoContaAulaView.Aluno);
                    if (cct == null)
                    {
                        cct = new()
                        {
                            TipoConta = StudioTipoContaAulaView.Aluno,
                            ContaNome = "Quantidade de alunos"
                        };
                        sct.Contas.Add(cct);
                    }
                    psc = cct.Periodos.FirstOrDefault(x => x.MesAno == ((DateTime)lu["DataMovto"]).ToString("MM/yyyy"));
                    if (psc == null)
                    {
                        psc = new()
                        {
                            DataRef = (DateTime)lu["DataMovto"]
                        };
                        cct.Periodos.Add(psc);
                    }
                    psc.Qtde += (int)lu["TotalAlunos"];


                }

                foreach (var s in r.Contas)
                {
                    s.Professores = s.Contas.Where(x => x.TipoConta == StudioTipoContaAulaView.Professor).OrderBy(x => x.ContaNome).ToList();
                    s.Alunos = s.Contas.Where(x => x.TipoConta == StudioTipoContaAulaView.Aluno).OrderBy(x => x.ContaNome).ToList();
                    s.Aulas = s.Contas.Where(x => x.TipoConta == StudioTipoContaAulaView.Aula).OrderBy(x => x.ContaNome).ToList();
                    s.TipoPlano = s.Contas.Where(x => x.TipoConta == StudioTipoContaAulaView.TipoPlano).OrderBy(x => x.ContaNome).ToList();
                }

                r.Periodos = r.Periodos.Where(x => x.TemMovto == true).OrderBy(x => x.DataRef).ToList();
            }
            finally
            {
                if (mycon)
                {
                    await command.Connection.CloseAsync();
                }
            }
            return r;
        }
        public async Task<Resultado> ChangeProfessorAulaAgenda(AulaAgenda ag, DateTime dia, int professorid, int studiosalaid)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (await _context.ProgramacaoAula.AnyAsync(x => x.TenantId == si.TenantId && x.AulaAgendaId == ag.Id && x.DataProgramada == dia && x.Inicio == ag.Inicio))
            {
                r.Ok = false;
                r.ErrMsg = "Aula registrada não pode ser alterada";
                return r;
            }

            await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Database.ExecuteSqlRawAsync(" Delete AulaAgendaProfessor where TenantId = '" + si.TenantId + "'  and AulaAgendaId = " + ag.Id.ToString()
                                               + " and Dia = '" + dia.ToString("yyyyMMdd") + "'");

                await _context.Database.ExecuteSqlRawAsync(" Insert AulaAgendaProfessor (TenantId, AulaAgendaId, Dia, ProfessorId, studiosalaid) Values ( '" +
                                                     si.TenantId + "', " + ag.Id.ToString() + ",'" + dia.ToString("yyyyMMdd") + "'," +
                                                     professorid.ToString() + ", " + studiosalaid.ToString() + ")");
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
        public async Task<Resultado> RegistrarAulaAgenda(AulaAgenda ag, DateTime dia)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            if (await _context.ProgramacaoAula.AnyAsync(x => x.TenantId == si.TenantId && x.AulaAgendaId == ag.Id))
            {
                r.Ok = false;
                r.ErrMsg = "Aula já registrada.";
                return r;
            }

            AulaAgendaProfessor aap = await _context.AulaAgendaProfessor.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.AulaAgendaId == ag.Id);

            var s = await (from AulaAgenda in _context.AulaAgenda
                           join Aula in _context.Aula on AulaAgenda.AulaId equals Aula.Id
                           join StudioSala in _context.StudioSala on AulaAgenda.StudioSalaId equals StudioSala.Id
                           join Studio in _context.Studio on StudioSala.StudioId equals Studio.Id
                           select new
                           {
                               AulaAgenda,
                               ValorAula = Aula.ValorAvulso,
                               Studio
                           }).FirstOrDefaultAsync(x => x.AulaAgenda.TenantId == si.TenantId && x.AulaAgenda.Id == ag.Id);


            await _context.Database.BeginTransactionAsync();
            try
            {

                if (s.Studio.AlunoIdDefault == 0 || (!await _context.Aluno.AnyAsync(x => x.Id == s.Studio.AlunoIdDefault && x.TenantId == si.TenantId)))
                {
                    Relacionamento nr = new()
                    {
                        Nome = s.Studio.Nome,
                        TenantId = si.TenantId,
                        Tipo = 2,
                        CPFCNPJ = s.Studio.CNPJ,
                        Ativo = true,
                        Sistema = true
                    };
                    await _context.Relacionamento.AddAsync(nr);
                    await _context.SaveChangesAsync();
                    Aluno na = new()
                    {
                        Nome = s.Studio.Nome,
                        TenantId = si.TenantId,
                        RelacionamentoId = nr.Id,
                        RelacionamentoFaturaId = nr.Id

                    };
                    await _context.Aluno.AddAsync(na);
                    await _context.SaveChangesAsync();
                    s.Studio.AlunoIdDefault = na.Id;
                    _context.Studio.Update(s.Studio);
                    await _context.SaveChangesAsync();
                }

                if (aap == null)
                {
                    aap = new()
                    {
                        TenantId = si.TenantId,
                        AulaAgendaId = ag.Id,
                        Dia = dia,
                        ProfessorId = ag.ProfessorId,
                        StudioSalaId = ag.StudioSalaId,
                        Registro = true
                    };
                    await _context.AulaAgendaProfessor.AddAsync(aap);
                }
                else
                {
                    aap.Registro = true;
                    aap.ProfessorId = ag.ProfessorId;
                    aap.StudioSalaId = ag.StudioSalaId;
                    _context.AulaAgendaProfessor.Update(aap);
                }
                await _context.SaveChangesAsync();

                await _context.Database.ExecuteSqlRawAsync(" Delete ProgramacaoAula where TenantId = '" + si.TenantId + "'  and AulaAgendaId = " + ag.Id.ToString()
                                               + " and DataProgramada = '" + dia.ToString("yyyyMMdd") + "'");

                ProgramacaoAula pa = new()
                {
                    TenantId = si.TenantId,
                    StudioId = s.Studio.Id,
                    AlunoId = s.Studio.AlunoIdDefault,
                    AulaId = ag.AulaId,
                    AlunoPlanoId = 0,
                    AulaAgendaId = ag.Id,
                    DataPagto = DateTime.MinValue,
                    DataProgramada = dia,
                    Inicio = ag.Inicio,
                    Fim = ag.Fim,
                    OBS = "Aula em grupo registrada em " + Constante.Today.ToString("dd/MM/yyyy"),
                    Origem = OrigemProgramacao.none,
                    ProfessorId = ag.ProfessorId,
                    ProfessorRealId = ag.ProfessorId,
                    Status = StatusAula.Executada,
                    StatusFinanceiro = StatusParcela.none,
                    TipoAula = TipoAula.EmGrupo,
                    Valor = s.ValorAula,
                    Studio = null,
                    Aula = null,
                    Professor = null,
                    ProfessorReal = null,
                    AlunoPlano = null,
                    Aluno = null,
                    StudioConfig = null,
                    Tenant = null
                };
                await _context.ProgramacaoAula.AddAsync(pa);
                await _context.SaveChangesAsync();

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
        public async Task<Resultado> ReabrirAulaAgenda(AulaAgenda ag, DateTime dia)
        {
            SessionInfo si = await GetSessionAsync();
            Resultado r = new();

            var pa = await _context.ProgramacaoAula.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.AulaAgendaId == ag.Id && x.DataProgramada == dia && x.Inicio == ag.Inicio);
            if (pa == null)
            {
                r.Ok = false;
                r.ErrMsg = "Aula não está registrada, não pode reaberta";
                return r;
            }

            if (pa.ProfessorLotePagtoId != 0)
            {
                r.Ok = false;
                r.ErrMsg = "Aula faz parte do lote de pagamento para o professor, não pode ser reaberta.";
                return r;
            }

            AulaAgendaProfessor aap = await _context.AulaAgendaProfessor.FirstOrDefaultAsync(x => x.TenantId == si.TenantId && x.AulaAgendaId == ag.Id);

            await _context.Database.BeginTransactionAsync();
            try
            {
                if (aap != null)
                {
                    aap.Registro = false;
                    _context.AulaAgendaProfessor.Update(aap);
                    await _context.SaveChangesAsync();
                }

                _context.ProgramacaoAula.Remove(pa);
                await _context.SaveChangesAsync();

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
    }
}



