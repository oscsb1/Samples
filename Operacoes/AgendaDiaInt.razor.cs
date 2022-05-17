using InCorpApp.Models;
using InCorpApp.Services;
using InCorpApp.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using InCorpApp.Constantes;
using Microsoft.AspNetCore.Components.Web;

namespace InCorpApp.Pages.StudioPages.Operacoes
{

    public enum TipoIntervaloAgenda : int
    {
        Aula = 30,
        AgendaAluno = 10,
        AulaGrupo = 20
    }

    public class LinhaIntervalo
    {
        public string Id { get; set; }
        public TipoIntervaloAgenda Tipo { get; set; }
        public bool OverlapFixo { get; set; } = false;
        public bool OverlapAula { get; set; } = false;
        public int OverlapX { get; set; }
        public int OverlapY { get; set; }
        public double Inicio { get; set; }
        public double Fim { get; set; }
        public int SeqOrder
        {
            get
            {
                if (Tipo == TipoIntervaloAgenda.Aula)
                {
                    switch (ProgramacaoAula.Status)
                    {
                        case StatusAula.Agendada:
                            return 90;
                        case StatusAula.Cancelada:
                            return 20;
                        case StatusAula.Executada:
                            return 70;
                        case StatusAula.NaoProgramada:
                            return 50;
                        case StatusAula.Programada:
                            return 80;
                        case StatusAula.ReAgendamento:
                            return 40;
                        case StatusAula.FaltaSemReagendamento:
                            return 60;
                        case StatusAula.Reserva:
                            return 30;
                    }
                }
                else
                {
                    if (Tipo == TipoIntervaloAgenda.AulaGrupo)
                    {
                        return 10;
                    }
                    else
                    {
                        if (Tipo == TipoIntervaloAgenda.AgendaAluno)
                        {
                            return 200;
                        }
                    }
                }
                return 0;
            }
        }
        public ProgramacaoAula ProgramacaoAula { get; set; }
        public AlunoAulaAgenda AlunoFixo { get; set; }
        public AulaAgenda AulaEmGrupo { get; set; }
    }
    public class HorarioProfessor
    {
        public string Id { get; set; }
        public double Inicio { get; set; }
        public double Fim { get; set; }
        public bool SemAgenda { get; set; } = false;
        public List<LinhaIntervalo> Intervalos { get; set; } = new();
    }

    public class DiaProfessor
    {
        public string Id { get; set; }
        public DateTime Dia { get; set; }
        public DayOfWeek DiaOfTheWeek => Dia.DayOfWeek;
        public string DiaNome => Dia.ToString("dd/MM") + " " + Dia.ToString("ddd");
        public List<HorarioProfessor> HorariosProfessor { get; set; } = new();
    }

    public class DiaLinhaAgenda
    {
        public string Id { get; set; }
        public DateTime Dia { get; set; }
        public DayOfWeek DiaOfTheWeek => Dia.DayOfWeek;
        public string DiaNome => Dia.ToString("dd/MM") + " " + Dia.ToString("ddd");
        public List<LinhaAgenda> LinhasAgenda { get; set; } = new();
    }

    public class ColunaProfessor
    {
        public Professor Professor { get; set; }
        public List<DiaProfessor> DiasProfessor { get; set; } = new();
    }
    public class LinhaAgenda
    {
        public int Posicao { get; set; }
        public double Inicio { get; set; }
        public double Fim { get; set; }
        public DateTime HoraInicio
        {
            get => Constante.Today.AddMinutes(Inicio);
            set => Inicio = value.Hour * 60 + value.Minute;
        }
        public DateTime HoraFim
        {
            get => Constante.Today.AddMinutes(Fim);
            set => Fim = value.Hour * 60 + value.Minute;
        }
        public string HoraInicioV
        {
            get => HoraInicio.ToString("HH:mm");
            set {; }
        }
        public string HoraFimV
        {
            get => HoraFim.ToString("HH:mm");
            set {; }
        }
        public bool Expediente { get; set; } = true;

    }

    public class AgendaDoDiaIntBase : SFComponenteBase
    {
        [Parameter]
        public int ProfessorId { get; set; } = 0;
        [Inject]
        protected StudioCadastroService CadService { get; set; }
        [Inject]
        protected StudioOperacoesService Service { get; set; }
        public Studio Studio { get; set; }
        protected StudioConfig StudioConfig { get; set; }
        public List<Studio> Studios { get; set; }
        public DateTime Dia { get; set; } = Constante.Today;
        public DateTime DiaSemana { get; set; } = Constante.Today;
        protected List<Professor> Professores { get; set; } = new List<Professor>();
        protected List<Aluno> Alunos { get; set; } = new List<Aluno>();
        protected List<Aula> Aulas { get; set; } = new List<Aula>();
        protected List<DiaLinhaAgenda> Linhas { get; set; } = new List<DiaLinhaAgenda>();
        public Professor ProfessorSel { get; set; } = null;
        protected bool ShowProgramacaoAula { get; set; } = false;
        protected bool ShowAulaAgendaEdit { get; set; } = false;
        protected bool ShowProgramacaoAulaHist { get; set; } = false;
        protected bool ShowIncluiProgramacaoAula { get; set; } = false;
        protected bool ShowReagendar { get; set; } = false;
        protected bool ShowProntuario { get; set; } = false;
        protected int AlunoIdProntuario { get; set; }
        protected TipoAula TipoAula { get; set; }
        protected ProgramacaoAula SelectedProgramacaoAula { get; set; }
        protected AulaAgenda SelectedAulaAgenda { get; set; }
        protected List<ColunaProfessor> ColunasProfessor { get; set; } = new();
        protected double InicioDia { get; set; } = 1;
        protected double FinalDia { get; set; } = 2;
        public MouseEventArgs MousePosition { get; set; }
        protected async override Task OnAfterRenderAsync(bool firstrender)
        {
            if (firstrender)
            {
                Studios = await CadService.FindAllStudioAsync();
                if (Studios.Count > 0)
                {
                    Studio = Studios[0];
                }
                else
                {
                    NavigationManager.NavigateTo("/StudioPages/StudioIndex", true);
                }
                Isloading = false;

                Titulo = "Agenda de aulas";
                TituloContexto = Dia.ToString("D", Constante.Culture).ToUpper();
                StudioConfig = await CadService.GetStudioConfig();
                await CarregarDados(Dia);
            }
            await base.OnAfterRenderAsync(firstrender);
        }
        protected async Task OnAfterChangeDate()
        {
            ProfessorSel = null;
            await CarregarDados(Dia);
        }
        protected async Task CarregarDados(DateTime dia, bool cleasrlist = true)
        {
            if (Processando)
            {
                return;
            }
            if (cleasrlist)
            {
                ColunasProfessor = new();
                Professores = new();
                Linhas = new();
            }
            Processando = true;
            Isloading = true;
            if (ProfessorSel == null)
            {
                await Task.Delay(200);
                Resultado.Ok = true;
                Resultado.ErrMsg = string.Empty;
                StateHasChanged();
            }
            List<ProgramacaoAula> programacao = null;
            List<AlunoAulaAgenda> professoresAgendaAluno = null;
            List<ProfessorProgramacaoAulas> professorAulas = new();
            List<ProfessorAgendasDia> lprofessorAgendasDia = null;
            List<AgendaDiaEspecial> agendaStudioDiaEspecial = null;
            List<AgendaDia> agendaStudio = null;
            try
            {

                programacao = await Service.FindAllProgramacaoAulaByStudioIdDia(Studio.Id, dia);
                programacao = programacao.Where(x => (x.TipoAula != TipoAula.Pacote && x.TipoAula != TipoAula.EmGrupo) || (x.TipoAula == TipoAula.Pacote && x.Status != StatusAula.Cancelada)).OrderBy(x => x.Inicio).ToList();
                foreach (var p in programacao)
                {
                    p.StudioConfig = StudioConfig;
                }
                lprofessorAgendasDia = await CadService.FindAllProfessorAgendaDiaByDayOfTheWeek(Studio.Id, dia);
                professoresAgendaAluno = await CadService.FindAllProfessorAgendaAluno(Studio.Id, dia);

                foreach (var p in lprofessorAgendasDia)
                {
                    Professores.Add(p.Professor);
                }
                try
                {
                    agendaStudio = (await CadService.FindAllAgendaDiaByDateAsync(Studio.Id, dia)).Where(x => x.Dia == dia.DayOfWeek).OrderBy(x => x.Inicio).ToList();
                }
                catch (Exception e)
                {
                    Resultado.Ok = false;
                    Resultado.ErrMsg = e.Message;
                    return;
                }

                agendaStudioDiaEspecial = await CadService.FindAllAgendaDiaEspecialByDatas(Studio.Id, dia, dia);
                if (agendaStudioDiaEspecial.Count > 0)
                {
                    agendaStudio = new();
                    foreach (var die in agendaStudioDiaEspecial)
                    {
                        if (!die.TemExpediente)
                        {
                            break;
                        }
                        AgendaDia a = new()
                        {
                            Inicio = die.Inicio,
                            Fim = die.Fim,
                            Dia = dia.DayOfWeek
                        };
                        agendaStudio.Add(a);
                    }
                    agendaStudio = agendaStudio.OrderBy(x => x.Inicio).ToList();
                }

                agendaStudio = agendaStudio.OrderBy(x => x.Inicio).ToList();
                if (agendaStudio.Count == 0 && programacao.Count > 0)
                {
                    AgendaDia a = new()
                    {
                        Inicio = programacao[0].Inicio,
                        Fim = programacao[^1].Fim,
                        Dia = dia.DayOfWeek,
                        Expediente = false
                    };
                    agendaStudio.Add(a);
                    agendaStudio = agendaStudio.OrderBy(x => x.Inicio).ToList();
                    if (ProfessorSel == null)
                    {
                        Resultado.Ok = false;
                        Resultado.ErrMsg = "Studio sem agenda para o dia, porem com aulas programadas!!!";
                    }
                }
                else
                {
                    if (programacao.Count > 0 && agendaStudio[0].Inicio > programacao[0].Inicio)
                    {
                        AgendaDia a = new()
                        {
                            Inicio = programacao[0].Inicio,
                            Fim = agendaStudio[0].Inicio,
                            Dia = dia.DayOfWeek,
                            Expediente = false
                        };
                        agendaStudio.Add(a);
                        agendaStudio = agendaStudio.OrderBy(x => x.Inicio).ToList();
                        if (ProfessorSel == null)
                        {
                            Resultado.Ok = false;
                            Resultado.ErrMsg = "Aulas programadas para antes do horário de abertura do studio!!!";
                        }
                    }
                    if (programacao.Count > 0 && agendaStudio[^1].Fim < programacao[^1].Fim)
                    {
                        AgendaDia a = new()
                        {
                            Inicio = agendaStudio[^1].Fim,
                            Fim = programacao[^1].Fim,
                            Dia = dia.DayOfWeek,
                            Expediente = false
                        };
                        agendaStudio.Add(a);
                        agendaStudio = agendaStudio.OrderBy(x => x.Inicio).ToList();
                        if (ProfessorSel == null)
                        {
                            Resultado.Ok = false;
                            Resultado.ErrMsg = "Aulas programadas para após do horário de fechamento do studio!!!";
                        }
                    }
                }
                foreach (var p in programacao)
                {
                    var xp = Professores.FirstOrDefault(x => x.Id == p.ProfessorId);
                    if (xp == null)
                    {
                        xp = await CadService.FindProfessorByIdAsync(p.ProfessorId);
                        if (xp == null)
                        {
                            xp = new()
                            {
                                Relacionamento = new()
                                {
                                    Nome = "Não definido"
                                },
                                Id = p.ProfessorId
                            };
                        }
                        Professores.Add(xp);
                    }

                    if (p.ProfessorRealId != 0)
                    {
                        if (p.ProfessorRealId == p.ProfessorId)
                        {
                            p.ProfessorReal = xp;
                        }
                        else
                        {
                            var xpr = Professores.FirstOrDefault(x => x.Id == p.ProfessorRealId);
                            if (xpr == null)
                            {
                                xpr = await CadService.FindProfessorByIdAsync(p.ProfessorRealId);
                                if (xpr == null)
                                {
                                    xpr = new()
                                    {
                                        Relacionamento = new()
                                        {
                                            Nome = "Não definido"
                                        },
                                        Id = p.ProfessorRealId
                                    };
                                }
                                Professores.Add(xpr);
                            }
                            p.ProfessorReal = xpr;
                        }
                    }

                    p.Professor = xp;
                    var ap = Alunos.FirstOrDefault(x => x.Id == p.AlunoId);
                    if (ap == null)
                    {
                        ap = await CadService.FindAlunoByIdAsync(p.AlunoId);
                        if (ap == null)
                        {
                            ap = new()
                            {
                                Relacionamento = new()
                                {
                                    Nome = "Não definido"
                                },
                                Id = p.AlunoId
                            };
                        }
                        Alunos.Add(ap);
                    }
                    p.Aluno = ap;
                    var a = Aulas.FirstOrDefault(x => x.Id == p.AulaId);
                    if (a == null)
                    {
                        a = await CadService.FindAulaByIdAsync(p.AulaId);
                        if (a == null)
                        {
                            a = new Aula()
                            {
                                Nome = "Não definido",
                                Id = p.AulaId
                            };
                        }
                        Aulas.Add(a);
                    }
                    p.Aula = a;
                }
                Professores = Professores.OrderBy(x => x.Nome).ToList();
                foreach (var pe in Professores)
                {
                    ProfessorProgramacaoAulas npap = new()
                    {
                        Professor = pe
                    };
                    npap.ProfessorAulas = programacao.Where(x => x.ProfessorId == pe.Id && x.Status != StatusAula.Executada).OrderBy(x => x.Inicio).ToList();
                    npap.ProfessorAulas.AddRange(programacao.Where(x => x.ProfessorRealId == pe.Id && x.Status == StatusAula.Executada).OrderBy(x => x.Inicio).ToList());
                    professorAulas.Add(npap);
                };
                if (agendaStudio.Count == 0)
                {
                    return;
                }
                int i = Convert.ToInt32(agendaStudio[0].Inicio);
                foreach (var a in agendaStudio)
                {
                    DiaLinhaAgenda diaLinhaAgenda = Linhas.FirstOrDefault(x => x.Dia == dia);
                    if (diaLinhaAgenda == null)
                    {
                        diaLinhaAgenda = new()
                        {
                            Dia = dia,
                            Id = ((int)dia.DayOfWeek).ToString()
                        };
                        Linhas.Add(diaLinhaAgenda);
                    }
                    while (i <= agendaStudio[^1].Fim)
                    {
                        LinhaAgenda l = new();
                        Math.DivRem(i, 60, out int rm);
                        if (i == agendaStudio[0].Inicio && rm != 0)
                        {
                            l.Inicio = i;
                            l.Fim = i + 60 - rm;
                            i += 60 - rm;
                        }
                        else
                        {
                            l.Inicio = i;
                            l.Fim = i + 60;
                            i += 60;
                        }
                        if (i > agendaStudio[^1].Fim)
                        {
                            l.Fim = agendaStudio[^1].Fim;
                        }
                        diaLinhaAgenda.LinhasAgenda.Add(l);
                    }
                }
                DiaLinhaAgenda diaLinhaAge = Linhas.FirstOrDefault(x => x.Dia == dia);
                foreach (var l in diaLinhaAge.LinhasAgenda)
                {
                    l.Expediente = agendaStudio.Any(x => x.Fim > l.Inicio && x.Inicio < l.Fim && x.Expediente == true);
                }

                List<AulaAgenda> aulasEmGrupo = await CadService.FindAgendaByStudioDia(Studio.Id, dia);

                foreach (var p in Professores)
                {
                    ColunaProfessor cp = ColunasProfessor.FirstOrDefault(x => x.Professor.Id == p.Id);
                    if (cp == null)
                    {
                        cp = new()
                        {
                            Professor = p
                        };
                        ColunasProfessor.Add(cp);
                    }
                    var diap = cp.DiasProfessor.FirstOrDefault(x => x.Dia == dia);
                    if (diap == null)
                    {
                        diap = new()
                        {
                            Dia = dia,
                            Id = p.Id.ToString() + ((int)dia.DayOfWeek).ToString()
                        };
                        cp.DiasProfessor.Add(diap);
                    }

                    var pa = lprofessorAgendasDia.FirstOrDefault(x => x.Professor.Id == p.Id);
                    if (pa != null)
                    {
                        foreach (var ag in pa.ProfessorAgendaDias)
                        {
                            HorarioProfessor hp = new()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Inicio = ag.Inicio,
                                Fim = ag.Fim
                            };
                            diap.HorariosProfessor.Add(hp);
                        }
                        diap.HorariosProfessor = diap.HorariosProfessor.OrderBy(x => x.Inicio).ToList();
                    }
                }

                foreach (var cp in ColunasProfessor)
                {
                    DiaProfessor diap = cp.DiasProfessor.FirstOrDefault(x => x.Dia == dia);

                    List<LinhaIntervalo> ltmp = new();
                    var a = professorAulas.FirstOrDefault(x => x.Professor.Id == cp.Professor.Id);
                    foreach (var au in a.ProfessorAulas.OrderBy(x => x.Inicio).ThenBy(x => x.SeqOrder).ToList())
                    {
                        LinhaIntervalo ni = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Inicio = au.Inicio,
                            Fim = au.Fim,
                            ProgramacaoAula = au,
                            Tipo = TipoIntervaloAgenda.Aula
                        };
                        var lo = ltmp.Where(x => x.Fim > au.Inicio && x.Inicio < au.Fim && x.ProgramacaoAula.ProfessorId == au.ProfessorId && x.ProgramacaoAula.Id != au.Id).OrderBy(x => x.Inicio).ToList();
                        if (lo.Count > 0)
                        {
                            var ict = 0;
                            for (int io = 0; io < lo.Count; io++)
                            {
                                lo[io].OverlapAula = true;
                                if (io == 0)
                                {
                                    lo[io].OverlapX = 0;
                                    lo[io].OverlapY = 0;
                                }
                                else
                                {
                                    if (lo[io].Inicio < lo[io - 1].Inicio - 30)
                                    {
                                        lo[io].OverlapX = ict;
                                        lo[io].OverlapY = lo[io - 1].OverlapY + 30;
                                    }
                                }
                                ict += 15;
                            }
                            if (ni.Inicio < lo[^1].Inicio + 20)
                            {
                                ni.OverlapY = lo[^1].OverlapY + 20;
                            }
                            ni.OverlapX = ict;
                            ni.OverlapAula = true;
                        }
                        ltmp.Add(ni);
                    }

                    foreach (var ag in aulasEmGrupo.Where(x => x.ProfessorId == cp.Professor.Id).OrderBy(x => x.Inicio).ToList())
                    {
                        LinhaIntervalo ni = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Inicio = ag.Inicio,
                            Fim = ag.Fim,
                            AulaEmGrupo = ag,
                            Tipo = TipoIntervaloAgenda.AulaGrupo
                        };
                        ltmp.Add(ni);
                    }


                    foreach (var au in professoresAgendaAluno.Where(x => x.ProfessorId == cp.Professor.Id).ToList())
                    {
                        if (a.ProfessorAulas.Any(x => x.AlunoId == au.AlunoId && x.Fim > au.Inicio && x.Inicio < au.Fim && x.ProfessorId == au.ProfessorId))
                        {
                            continue;
                        }
                        LinhaIntervalo ni = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Inicio = au.Inicio,
                            Fim = au.Fim,
                            AlunoFixo = au,
                            Tipo = TipoIntervaloAgenda.AgendaAluno
                        };

                        var loa = ltmp.Where(x => x.Fim > au.Inicio && x.Inicio < au.Fim).ToList();
                        if (loa.Count > 0)
                        {
                            ni.OverlapFixo = true;
                            if (loa[^1].Inicio + loa[^1].OverlapY < ni.Inicio - 25)
                            {
                                ni.OverlapX = 30;
                            }
                            else
                            {
                                ni.OverlapX = 30;
                                ni.OverlapY = 30 + loa[^1].OverlapY;
                            }
                        }

                        ltmp.Add(ni);
                    }

                    ltmp = ltmp.OrderBy(x => x.Inicio).ThenBy(x => x.SeqOrder).ToList();
                    if (diap.HorariosProfessor.Count == 0 && ltmp.Count > 0)
                    {
                        HorarioProfessor hp = null;
                        for (int j = 0; j < ltmp.Count; j++)
                        {
                            if (j == 0)
                            {
                                hp = new()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Inicio = ltmp[0].Inicio,
                                    Fim = ltmp[0].Fim,
                                    SemAgenda = true
                                };
                                diap.HorariosProfessor.Add(hp);
                            }
                            else
                            {
                                if (ltmp[j - 1].Fim > ltmp[j].Inicio)
                                {
                                    if (ltmp[j - 1].Fim > ltmp[j].Fim)
                                    {
                                        hp.Fim = ltmp[j - 1].Fim;
                                    }
                                    else
                                    {
                                        hp.Fim = ltmp[j].Fim;
                                    }
                                }
                                else
                                {
                                    hp = new()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        Inicio = ltmp[j].Inicio,
                                        Fim = ltmp[j].Fim,
                                        SemAgenda = true
                                    };
                                    diap.HorariosProfessor.Add(hp);
                                }
                            }

                        }
                    }
                    else
                    {
                        if (ltmp.Count > 0)
                        {
                            foreach (var ag in ltmp)
                            {
                                var hps = diap.HorariosProfessor.FirstOrDefault(x => x.Fim > ag.Inicio && x.Inicio < ag.Fim);
                                if (hps == null)
                                {
                                    hps = new()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        Inicio = ag.Inicio,
                                        Fim = ag.Fim,
                                        SemAgenda = true
                                    };
                                    diap.HorariosProfessor.Add(hps);
                                }
                                else
                                {
                                    if (hps.Inicio > ag.Inicio)
                                    {
                                        hps.SemAgenda = true;
                                        hps.Inicio = ag.Inicio;
                                    }
                                    if (hps.Fim < ag.Fim)
                                    {
                                        hps.SemAgenda = true;
                                        hps.Fim = ag.Fim;
                                    }

                                }
                            }
                        }
                    }

                    foreach (var hp in diap.HorariosProfessor)
                    {
                        hp.Intervalos = ltmp.Where(x => x.Inicio >= hp.Inicio && x.Inicio < hp.Fim).OrderBy(x => x.Inicio).OrderBy(x => x.SeqOrder).ToList();
                        if (hp.Intervalos.Count > 0)
                        {
                            var maxfim = hp.Intervalos.Max(x => x.Fim);
                            if (hp.Fim < maxfim)
                            {
                                hp.Fim = maxfim;
                                hp.SemAgenda = true;
                            }
                        }
                    }

                }

                ColunasProfessor.Where(x => x.DiasProfessor.Count == 0).ToList().ForEach(x => ColunasProfessor.Remove(x));

                InicioDia = 99999;
                FinalDia = 0;
                foreach (var diaa in Linhas)
                {
                    if (diaa.LinhasAgenda.Count > 0)
                    {
                        if (InicioDia > diaa.LinhasAgenda[0].Inicio)
                        {
                            InicioDia = diaa.LinhasAgenda[0].Inicio;
                        }
                        if (FinalDia < diaa.LinhasAgenda[^1].Fim)
                        {
                            FinalDia = diaa.LinhasAgenda[^1].Fim;
                        }
                    }
                }
                if (Linhas.Count > 0)
                {
                    if (Linhas[0].LinhasAgenda.Count > 0)
                    {
                        if (Linhas[0].LinhasAgenda[0].Inicio > InicioDia)
                        {
                            Linhas[0].LinhasAgenda[0].Inicio = InicioDia;
                        }
                        if (Linhas[0].LinhasAgenda[^1].Fim < FinalDia)
                        {
                            Linhas[0].LinhasAgenda[^1].Fim = FinalDia;
                        }
                        Linhas[0].LinhasAgenda[0].Posicao = 1;
                        Linhas[0].LinhasAgenda[^1].Posicao = 2;
                    }
                }
            }
            finally
            {
                Isloading = false;
                Processando = false;

                if (ProfessorSel == null)
                {
                    TituloContexto = Dia.ToString("D", Constante.Culture).ToUpper();
                    StateHasChanged();
                    try
                    {
              //          await Task.Delay(300);
                        await JsRuntime.InvokeVoidAsync("ajustatable", "tabler", Constante.GapTable);
                    }
                    catch
                    {
                    }
                    DrawAgenda();
                }
            }
        }
        protected void DrawAgenda()
        {
            try
            {
                if (Linhas.Count == 0)
                {
                    return;
                }
                foreach (var l in Linhas[0].LinhasAgenda)
                {
                    if (l.Inicio == Linhas[0].LinhasAgenda[0].Inicio)
                    {
                        JsRuntime.InvokeVoidAsync("drawhora", l.HoraInicioV, l.Inicio - InicioDia, l.Fim - l.Inicio, l.Posicao, l.Expediente, true);
                    }
                    else
                    {
                        JsRuntime.InvokeVoidAsync("drawhora", l.HoraInicioV, l.Inicio - InicioDia, l.Fim - l.Inicio, l.Posicao, l.Expediente, false);
                    }
                }
                foreach (var p in ColunasProfessor)
                {
                    foreach (var diap in p.DiasProfessor)
                    {
                        string canvasid = diap.Id;
                        if (diap.HorariosProfessor.Count == 0)
                        {
                            JsRuntime.InvokeVoidAsync("clearcanvas", canvasid);
                            continue;
                        }
                        bool clear = true;
                        foreach (var hp in diap.HorariosProfessor)
                        {
                            if (clear)
                            {
                                JsRuntime.InvokeVoidAsync("drawintervalo", canvasid, StudioConfig.CorAgendaProfessor, hp.Inicio - InicioDia, hp.Fim - hp.Inicio, hp.SemAgenda, clear);
                            }
                            else
                            {
                                JsRuntime.InvokeVoidAsync("drawintervalo", canvasid, StudioConfig.CorAgendaProfessor, hp.Inicio - InicioDia, hp.Fim - hp.Inicio, hp.SemAgenda, clear);
                            }
                            clear = false;
                            foreach (var a in hp.Intervalos)
                            {
                                string anome = string.Empty;
                                switch (a.Tipo)
                                {
                                    case TipoIntervaloAgenda.AgendaAluno:

                                        if (a.AlunoFixo.Aluno.Nome.Length > 15)
                                        {
                                            anome = a.AlunoFixo.Aluno.Nome.Substring(0, 15);
                                        }
                                        else
                                        {
                                            anome = a.AlunoFixo.Aluno.Nome;
                                        }
                                        JsRuntime.InvokeVoidAsync("drawalunofixo", canvasid, anome, StudioConfig.CorAlunoHorarioFixo, a.Inicio - InicioDia, a.Fim - a.Inicio, a.OverlapFixo, a.OverlapX, a.OverlapY, a.AlunoFixo.HoraInicioV, a.AlunoFixo.HoraFimV);
                                        break;
                                    case TipoIntervaloAgenda.Aula:

                                        if (a.ProgramacaoAula.AlunoNome.Length > 15)
                                        {
                                            anome = a.ProgramacaoAula.AlunoNome.Substring(0, 15);
                                        }
                                        else
                                        {
                                            anome = a.ProgramacaoAula.AlunoNome;
                                        }
                                        JsRuntime.InvokeVoidAsync("drawaula", canvasid, anome, a.ProgramacaoAula.Aula.Codigo, a.ProgramacaoAula.StatusCor, a.Inicio - InicioDia, a.Fim - a.Inicio, a.OverlapAula, a.OverlapX, a.OverlapY, a.ProgramacaoAula.HoraInicioV, a.ProgramacaoAula.HoraFimV, a.ProgramacaoAula.StatusFinanceiro, a.ProgramacaoAula.StatusNome);
                                        break;
                                    case TipoIntervaloAgenda.AulaGrupo:
                                        JsRuntime.InvokeVoidAsync("drawaulagrupo", canvasid, a.AulaEmGrupo.AulaNome, a.AulaEmGrupo.StudioSalaNome, a.AulaEmGrupo.Cor, a.Inicio - InicioDia, a.Fim - a.Inicio);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Resultado.Ok = false;
                Resultado.ErrMsg = e.Message;
            }
        }
        protected async Task CarregarSemana()
        {
            DateTime dt = DiaSemana;
            while (dt <= DiaSemana.AddDays(6))
            {
                if (dt == DiaSemana)
                {
                    await CarregarDados(dt, true);
                }
                else
                {
                    await CarregarDados(dt, false);
                }

                foreach (var cp in ColunasProfessor)
                {
                    if (!cp.DiasProfessor.Any(x => x.Dia == dt))
                    {
                        DiaProfessor nd = new()
                        {
                            Dia = dt

                        };
                        cp.DiasProfessor.Add(nd);
                    }
                }

                dt = dt.AddDays(1);
            };
            TituloContexto = Dia.ToString("D", Constante.Culture).ToUpper();
            StateHasChanged();
            try
            {
             //   await Task.Delay(300);
                await JsRuntime.InvokeVoidAsync("ajustatable", "tabler", Constante.GapTable);
                await JsRuntime.InvokeVoidAsync("scrolltb", "tabler");
            }
            catch
            {
            }
            DrawAgenda();
        }
        protected async Task OnShowAgendaProfessorClick(int id)
        {
            if (Processando)
            {
                return;
            }
            if (ProfessorSel != null && ProfessorSel.Id == id)
            {
                ProfessorSel = null;
                await CarregarDados(Dia);
            }
            else
            {
                if (ProfessorSel != null)
                {
                    ProfessorSel = Professores.FirstOrDefault(x => x.Id == id);
                    StateHasChanged();
                    try
                    {
                        Processando = true;
                        await Task.Delay(200);
                        await JsRuntime.InvokeVoidAsync("ajustatable", "tabler", Constante.GapTable);
                        await JsRuntime.InvokeVoidAsync("scrolltb", "tabler");
                        DrawAgenda();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        Processando = false;
                    }

                }
                else
                {
                    ProfessorSel = Professores.FirstOrDefault(x => x.Id == id);
                    DiaSemana = Dia.AddDays(((int)Dia.DayOfWeek - 1) * -1);
                    await CarregarSemana();

                }
            }
        }
        protected void OnMouseUpAgenda(MouseEventArgs e)
        {
            MousePosition = e;
            MousePosition.OffsetY += InicioDia;

        }
        protected void OnClickAgenda(DiaProfessor p)
        {
            if (MousePosition == null || MousePosition.OffsetX > 150 || (MousePosition.OffsetX < 90 && MousePosition.OffsetX > 15))
                return;

            var hp = p.HorariosProfessor.FirstOrDefault(x => x.Inicio < MousePosition.OffsetY && x.Fim > MousePosition.OffsetY);
            if (hp == null)
            {
                return;
            }

            var lit = hp.Intervalos.Where(x => x.Inicio + x.OverlapY < MousePosition.OffsetY && x.Inicio + x.OverlapY + 25 > MousePosition.OffsetY).ToList();
            if (lit.Count == 0)
            {
                return;
            }

            foreach (var it in lit)
            {

                if (it.Tipo == TipoIntervaloAgenda.Aula)
                {
                    if (it.OverlapY == 0)
                    {
                        if (MousePosition.OffsetX >= 93 && MousePosition.OffsetX <= 106)
                        {
                            OnClickAulaHist(it.ProgramacaoAula);
                        }
                        if (MousePosition.OffsetX >= 107 && MousePosition.OffsetX <= 121)
                        {
                            OnClickProntuario(it.ProgramacaoAula.AlunoId);
                        }
                        if (MousePosition.OffsetX >= 122 && MousePosition.OffsetX <= 145)
                        {
                            OnClickAula(it.ProgramacaoAula);
                        }
                    }
                    else
                    {
                        if (MousePosition.OffsetX >= 122 && MousePosition.OffsetX <= 145)
                        {
                            OnClickAula(it.ProgramacaoAula);
                        }
                    }
                }
                else
                {
                    if (it.Tipo == TipoIntervaloAgenda.AulaGrupo)
                    {
                        if (MousePosition.OffsetX >= 5 && MousePosition.OffsetX <= 15)
                        {
                            OnClickAulaAgenda(it.AulaEmGrupo);
                        }
                    }
                }
            }
        }
        protected void OnClickAula(ProgramacaoAula a)
        {
            if (Processando) { return; }
            Processando = true;
            ShowProgramacaoAula = true;
            SelectedProgramacaoAula = a;
            StateHasChanged();
            Processando = false;

        }
        protected void OnClickAulaAgenda(AulaAgenda a)
        {
            if (Processando) { return; }
            Processando = true;
            ShowAulaAgendaEdit = true;
            SelectedAulaAgenda = a;
            StateHasChanged();
            Processando = false;

        }
        protected void OnClickProntuario(int alunoid)
        {
            if (Processando) { return; }
            Processando = true;
            ShowProntuario = true;
            AlunoIdProntuario = alunoid;
            StateHasChanged();
            Processando = false;
        }
        protected async Task OnVolarProntuario()
        {
            ShowProntuario = false;
            ShowProgramacaoAulaHist = false;
            ShowProgramacaoAula = false;
            SelectedProgramacaoAula = null;
            ShowIncluiProgramacaoAula = false;
            if (ProfessorSel == null)
            {
                await CarregarDados(Dia);
            }
            else
            {
                await CarregarSemana();
            }
        }
        protected void OnClickAulaHist(ProgramacaoAula a)
        {
            if (Processando) { return; }
            Processando = true;
            ShowProgramacaoAulaHist = true;
            SelectedProgramacaoAula = a;
            StateHasChanged();
            Processando = false;
        }
        protected async Task MyListStudioValueChangedHandler(int newValue)
        {
            Studio = Studios.FirstOrDefault(x => x.Id == int.Parse(newValue.ToString()));
            if (Processando)
            {
                return;
            }
            ProfessorSel = null;
            await CarregarDados(Dia);
        }
        protected async Task PesquisarClick()
        {
            ProfessorSel = null;
            await CarregarDados(Dia);
        }
        protected async Task ProximoDiaClick()
        {
            if (Processando)
            {
                return;
            }
            ProfessorSel = null;
            Dia = Dia.AddDays(1);
            await CarregarDados(Dia);
        }
        protected async Task DiaAnteriorClick()
        {
            if (Processando)
            {
                return;
            }
            Dia = Dia.AddDays(-1);
            ProfessorSel = null;
            await CarregarDados(Dia);
        }
        protected async Task SemanaAnteriorClick()
        {
            if (Processando)
            {
                return;
            }
            DiaSemana = DiaSemana.AddDays(-7);
            await CarregarSemana();
        }
        protected async Task ProximoSemanaClick()
        {
            if (Processando)
            {
                return;
            }
            DiaSemana = DiaSemana.AddDays(7);
            await CarregarSemana();
        }
        protected async Task CloseHist()
        {
            ShowProgramacaoAulaHist = false;
            ShowProgramacaoAula = false;
            SelectedProgramacaoAula = null;
            ShowIncluiProgramacaoAula = false;
            if (ProfessorSel == null)
            {
                await CarregarDados(Dia);
            }
            else
            {
                await CarregarSemana();
            }
        }
        protected async Task OnCancelAulaDet()
        {
            if (Processando)
            { return; }
            ShowProgramacaoAula = false;
            SelectedProgramacaoAula = null;
            ShowIncluiProgramacaoAula = false;
            if (ProfessorSel == null)
            {
                await CarregarDados(Dia);
            }
            else
            {
                await CarregarSemana();
            }
        }
        protected async Task OnOkAulaDet()
        {
            if (Processando)
            { return; }
            ShowProgramacaoAula = false;
            SelectedProgramacaoAula = null;
            if (ProfessorSel == null)
            {
                await CarregarDados(Dia);
            }
            else
            {
                await CarregarSemana();
            }
        }
        protected async Task OnOkAulaAgenda()
        {
            if (Processando)
            { return; }
            ShowAulaAgendaEdit = false;
            SelectedAulaAgenda = null;
            if (ProfessorSel == null)
            {
                await CarregarDados(Dia);
            }
            else
            {
                await CarregarSemana();
            }
        }
        protected async Task OnCancelAulaAgendaDet()
        {
            if (Processando)
            { return; }
            ShowAulaAgendaEdit = false;
            SelectedAulaAgenda = null;
            if (ProfessorSel == null)
            {
                await CarregarDados(Dia);
            }
            else
            {
                await CarregarSemana();
            }
        }
        protected async Task OnOkIncAula()
        {
            if (Processando)
            { return; }
            ShowProgramacaoAula = false;
            ShowIncluiProgramacaoAula = false;
            SelectedProgramacaoAula = null;
            if (ProfessorSel == null)
            {
                await CarregarDados(Dia);
            }
            else
            {
                await CarregarSemana();
            }
        }
        protected void OnIncluirAulaAvulsaClick()
        {
            if (Processando) { return; }
            Processando = true;
            try
            {
                ShowIncluiProgramacaoAula = true;
                TipoAula = TipoAula.Avulsa;
            }
            finally
            {
                Processando = false;
            }
            StateHasChanged();
        }
        protected void OnIncluirAulaTesteClick()
        {
            if (Processando) { return; }
            Processando = true;
            try
            {
                ShowIncluiProgramacaoAula = true;
                TipoAula = TipoAula.Teste;
            }
            finally
            {
                Processando = false;
            }
            StateHasChanged();
        }
        protected void OnAgendaPacoteAulaClick()
        {
            if (Processando) { return; }
            Processando = true;
            try
            {
                ShowIncluiProgramacaoAula = true;
                TipoAula = TipoAula.Pacote;
            }
            finally
            {
                Processando = false;
            }
            StateHasChanged();
        }
        protected void OnReagendar()
        {
            if (Processando) { return; }
            Processando = true;
            try
            {
                ShowReagendar = true;
            }
            finally
            {
                Processando = false;
            }
            StateHasChanged();
        }
        protected async Task OnRetornoReagendar(int programacaoaulaid)
        {
            ShowReagendar = false;
            if (programacaoaulaid == 0)
            {
                ShowProntuario = false;
                ShowProgramacaoAulaHist = false;
                ShowProgramacaoAula = false;
                SelectedProgramacaoAula = null;
                ShowIncluiProgramacaoAula = false;
                if (ProfessorSel == null)
                {
                    await CarregarDados(Dia);
                }
                else
                {
                    await CarregarSemana();
                }
                return;
            }
            SelectedProgramacaoAula = await Service.FindProgramacaoAulaId(programacaoaulaid);
            if (SelectedProgramacaoAula != null)
            {
                ShowProgramacaoAula = true;
                SelectedProgramacaoAula.Professor = await CadService.FindProfessorByIdAsync(SelectedProgramacaoAula.ProfessorId);
                SelectedProgramacaoAula.Aula = await CadService.FindAulaByIdAsync(SelectedProgramacaoAula.AulaId);
                SelectedProgramacaoAula.Aluno = await CadService.FindAlunoByIdAsync(SelectedProgramacaoAula.AlunoId);
            }
            StateHasChanged();
        }

    }
}
