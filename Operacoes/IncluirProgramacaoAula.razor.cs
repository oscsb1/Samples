using Blazorise;
using Blazorise.Components;
using InCorpApp.Constantes;
using InCorpApp.Models;
using InCorpApp.Services;
using InCorpApp.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Pages.StudioPages.Operacoes
{
    public class IncluirProgramacaoAulaBase : SFComponenteBase
    {

        [Parameter]
        public bool AgendaGeral { get; set; } = false;

        [Parameter]
        public DateTime Dia { get; set; }
        [Parameter]
        public Studio StudioSel { get; set; }
        [Parameter]
        public TipoAula TipoAula { get; set; }
        [Parameter]
        public EventCallback<ChangeEventArgs> OnCancel { get; set; }
        [Parameter]
        public EventCallback<ChangeEventArgs> OnOk { get; set; }
        [Parameter]
        public ProgramacaoAula Item { get; set; }
        [Parameter]
        public bool InterfaceAgenda { get; set; } = true;
        [Parameter]
        public bool EditarComoParcela { get; set; } = false;
        [Inject]
        protected StudioOperacoesService Service { get; set; }
        [Inject]
        protected StudioCadastroService CadService { get; set; }
        protected ProgramacaoAula ItemAnt { get; set; }
        protected List<Aula> Aulas { get; set; }
        protected List<Aula> AllAulas { get; set; }
        protected Aula AulaSel { get; set; }
        protected List<ProfessorAula> ProfessorAulas { get; set; }
        protected List<AlunoPlanoAula> AlunoPlanoAulas { get; set; } = new List<AlunoPlanoAula>();
        protected AlunoPlanoAula AlunoPlanoAulaSel { get; set; } = new AlunoPlanoAula();
        protected List<Studio> Studios { get; set; } = new List<Studio>();
        protected List<Professor> Professores { get; set; } = new List<Professor>();
        protected List<Professor> ProfessoresAll { get; set; } = new List<Professor>();
        protected Professor ProfessorSel { get; set; } = new Professor();
        protected Professor ProfessorRealSel { get; set; } = new Professor();
        public List<Aluno> Alunos { get; set; } = new List<Aluno>();
        protected Aluno AlunoSel { get; set; } = new Aluno();
        protected string HdrTitulo { get; set; } = string.Empty;
        public string BodyTitulo { get; set; } = string.Empty;
        protected Modal Modal { get; set; }
        protected string OBS { get; set; } = string.Empty;
        protected bool ShowModal { get; set; } = false;
        protected int SelectedRelacionamento { get; set; }
        protected Autocomplete<Aluno, int> AutoC01 { get; set; }
        protected bool AjustouC01 { get; set; } = false;
        protected async Task RelacionamentoSearchHandler(int cc)
        {
            SelectedRelacionamento = cc;
            if (SelectedRelacionamento == 0)
            {
                Aulas.Clear();
                Item.Aluno = null;
                Item.AlunoId = 0;
                return;
            }
            if (AlunoSel != null && AlunoSel.Id == SelectedRelacionamento)
            {
                return;
            }
            AlunoSel = Alunos.FirstOrDefault(x => x.Id == SelectedRelacionamento);
            if (AlunoSel != null)
            {
                Item.Aluno = AlunoSel;
                Item.AlunoId = AlunoSel.Id;
                if (TipoAula == TipoAula.Pacote)
                {
                    AlunoPlanoAulas = await CadService.FindAllAulaAByAlunoIdPacotesync(AlunoSel.Id);
                    if (AlunoPlanoAulas.Count == 0)
                    {

                    }
                    foreach (var ap in AlunoPlanoAulas)
                    {
                        ap.NomeAulaEsp = ap.Aula.Nome + " - faltam agendar " + (ap.QtdeAulas - ap.TotalAulasAtivas).ToString() + " de " + ap.QtdeAulas.ToString();
                    }
                    if (AlunoPlanoAulas.Count > 0)
                    {
                        var ap = AlunoPlanoAulas.FirstOrDefault(x => x.Id > 0);
                        Item.Aula = ap.Aula;
                        Item.AulaId = Item.Aula.Id;
                        Item.AlunoPlanoId = ap.AlunoPlanoId;
                        Item.Valor = ap.ValorAula;
                        AlunoPlanoAulaSel = ap;
                    }
                }
                else
                {
                    Aulas = AllAulas;
                    if (Aulas.Count > 0)
                    {
                        Item.Aula = Aulas.FirstOrDefault(x => x.Id > 0);
                        Item.AulaId = Item.Aula.Id;
                        Item.AlunoPlanoId = 0;
                        Item.Valor = Item.Aula.ValorAvulso;
                        AulaSel = Item.Aula;
                    }
                }
                if (Item.AulaId > 0)
                {
                    Professores = ProfessoresAll.Where(x => ProfessorAulas.Where(x => x.AulaId == Item.AulaId).ToList().Select(x => x.ProfessorId).Contains(x.Id)).ToList();
                    if (Professores.Count > 0 && Item.ProfessorId == 0)
                    {
                        ProfessorSel = Professores[0];
                        Item.Professor = ProfessorSel;
                        Item.ProfessorId = ProfessorSel.Id;
                    }
                }
            }
            else
            {
                Aulas = null;
                Item.Aluno = null;
                Item.AlunoId = 0;
            }
            StateHasChanged();
        }
        protected async override Task OnAfterRenderAsync(bool firstrender)
        {
            if (firstrender)
            {
                Studios = await CadService.FindAllStudioAsync();
                if (Studios.Count > 0 && (StudioSel == null || StudioSel.Id == 0))
                {
                    StudioSel = Studios[0];
                }
                ProfessoresAll = await CadService.FindAllProfessorAsync();
                ProfessorAulas = await CadService.FindAllProfessorAula();
                AllAulas = await CadService.FindAllAulaAsync();
                AllAulas = AllAulas.Where(x => x.AulaGrupo == false).ToList();
                if (Item == null)
                {
                    Item = new ProgramacaoAula()
                    {
                        DataProgramada = Dia,
                        Inicio = 720,
                        Fim = 720,
                        OBS = string.Empty,
                        NotaFiscal = string.Empty,
                        Origem = OrigemProgramacao.Manual,
                        ProfessorId = 0,
                        ProfessorRealId = 0,
                        Status = StatusAula.Agendada,
                        Professor = null,
                        ProfessorReal = null,
                        Aula = null,
                        AulaId = 0,
                        TipoAula = TipoAula,
                        StatusFinanceiro = StatusParcela.Aberto,
                        StudioId = StudioSel.Id
                    };
                    TituloContexto = "Inclusão de aula";
                }
                else
                {
                    if (Item.Id == 0)
                    {
                        Item.DataProgramada = Dia;
                        Item.Inicio = 720;
                        Item.Fim = 720;
                        Item.OBS = string.Empty;
                        Item.NotaFiscal = string.Empty;
                        Item.Origem = OrigemProgramacao.Manual;
                        Item.ProfessorId = 0;
                        Item.ProfessorRealId = 0;
                        Item.Status = StatusAula.Agendada;
                        Item.Professor = null;
                        Item.ProfessorReal = null;
                        Item.Aula = null;
                        Item.AulaId = 0;
                        Item.TipoAula = TipoAula;
                        Item.StatusFinanceiro = StatusParcela.Aberto;
                        Item.StudioId = StudioSel.Id;
                    }
                    else
                    {
                        if (Item.NotaFiscal == null)
                        {
                            Item.NotaFiscal = string.Empty;
                        }
                        if (Item.OBS == null)
                        {
                            Item.OBS = string.Empty;
                        }
                        if (EditarComoParcela)
                        {
                            TituloContexto = "Aula avula - Edição de dados financeiros";
                        }
                        else
                        {
                            TituloContexto = "Edição de aula";
                        }
                        ItemAnt = new ProgramacaoAula()
                        {
                            Id = Item.Id,
                            DataProgramada = Item.DataProgramada,
                            Fim = Item.Fim,
                            Inicio = Item.Inicio,
                            OBS = Item.OBS,
                            Origem = Item.Origem,
                            ProfessorId = Item.ProfessorId,
                            ProfessorRealId = Item.ProfessorRealId,
                            Status = Item.Status,
                            Professor = Item.Professor,
                            ProfessorReal = Item.ProfessorReal,
                            Aula = Item.Aula,
                            AulaId = Item.AulaId,
                            Valor = Item.Valor,
                            ValorPago = Item.ValorPago,
                            StatusFinanceiro = Item.StatusFinanceiro,
                            DataPagto = Item.DataPagto,
                            AlunoId = Item.AlunoId,
                            AlunoPlanoAulaId = Item.AlunoPlanoAulaId,
                            AlunoPlanoId = Item.AlunoPlanoId,
                            Faturado = Item.Faturado,
                            StudioId = Item.StudioId,
                            TipoAula = Item.TipoAula,
                            TenantId = Item.TenantId,
                            NotaFiscal = Item.NotaFiscal
                        };
                        ProfessorSel = ProfessoresAll.FirstOrDefault(x => x.Id == Item.ProfessorId);
                        Item.Professor = ProfessorSel;
                        if (Item.Status != StatusAula.Executada)
                        {
                            Item.ProfessorReal = Item.Professor;
                            Item.ProfessorRealId = Item.ProfessorId;
                            ProfessorRealSel = Item.Professor;
                        }
                        else
                        {
                            ProfessorRealSel = ProfessoresAll.FirstOrDefault(x => x.Id == Item.ProfessorRealId);
                            Item.ProfessorReal = ProfessorRealSel;
                        }
                        Alunos = new List<Aluno>() { await CadService.FindAlunoByIdAsync(Item.AlunoId) };
                        AlunoSel = Alunos.FirstOrDefault(x => x.Id > 0);
                        Item.Aluno = AlunoSel;
                        AulaSel = AllAulas.FirstOrDefault(x => x.Id == Item.AulaId);
                        Item.Aula = AulaSel;
                        if (Item.TipoAula == TipoAula.Avulsa)
                        {
                            Aulas = new List<Aula>() { AulaSel };
                            Item.ProfessorReal = ProfessoresAll.FirstOrDefault(x => x.Id == Item.ProfessorRealId);
                        }
                    }
                }
                if (InterfaceAgenda)
                {
                    if (TipoAula == TipoAula.Avulsa)
                    { HdrTitulo = "Agendamento de aula avulsa "; }
                    if (TipoAula == TipoAula.Teste)
                    { HdrTitulo = "Agendamento de aula experimental "; }
                    if (TipoAula == TipoAula.Pacote)
                    { HdrTitulo = "Agendamento de aula pacote "; }
                    if (Item.Id == 0)
                    {
                        Alunos = await CadService.FindAllAlunoAsync();
                    }
                    else
                    {
                        Alunos = new List<Aluno>() { await CadService.FindAlunoByIdAsync(Item.AlunoId) };
                        AlunoSel = Alunos.FirstOrDefault(x => x.Id > 0);
                    }
                }
                else
                {
                    AlunoPlanoAulas = await CadService.FindAllAulaAByAlunoPlanoIdAsync(Item.AlunoPlanoId);
                    AlunoPlanoAulas = AlunoPlanoAulas.Where(x => x.Aula.AulaGrupo == false).ToList();
                    if (AlunoPlanoAulas.Count > 0)
                    {
                        if (Item.Id == 0)
                        {
                            var ap = AlunoPlanoAulas.FirstOrDefault(x => x.Id > 0 && x.Aula.AulaGrupo == false);
                            if (ap != null)
                            {
                                Item.Aula = ap.Aula;
                                Item.AulaId = Item.Aula.Id;
                                Item.AlunoPlanoId = ap.AlunoPlanoId;
                                Item.Valor = ap.ValorAula;
                                AlunoPlanoAulaSel = ap;
                            }
                        }
                        else
                        {
                            var ap = AlunoPlanoAulas.FirstOrDefault(x => x.AulaId == Item.AulaId);
                            double v = Item.Valor;
                            AlunoPlanoAulaSel = ap;
                            StateHasChanged();
                            await Task.Delay(300);
                            Item.Valor = v;
                        }
                    }
                }
                if (TipoAula != TipoAula.Pacote && Item.Id == 0)
                {
                    Aulas = AllAulas;
                }
                if (StudioSel == null && Studios.Count > 0)
                {
                    Item.Studio = Studios.FirstOrDefault(x => x.Id > 0);
                    Item.StudioId = Item.Studio.Id;
                    StudioSel = Item.Studio;
                }
                if (Item.AulaId > 0)
                {
                    Professores = ProfessoresAll.Where(x => ProfessorAulas.Where(x => x.AulaId == Item.AulaId).ToList().Select(x => x.ProfessorId).Contains(x.Id)).ToList();
                }
                if (Professores.Count > 0 && Item.ProfessorId == 0)
                {
                    Item.Professor = Professores.FirstOrDefault(x => x.Id > 0);
                    Item.ProfessorId = Item.Professor.Id;
                    ProfessorSel = Item.Professor;
                }
                Resultado.Ok = false;
                Isloading = false;
                StateHasChanged();
            }
            if (!ShowModal)
            {
                if (Modal != null)
                {
                    await Modal.Show();
                    ShowModal = true;
                    StateHasChanged();
                }
            }
            if (AutoC01 == null)
            {
                AjustouC01 = false;
            }
            if (AutoC01 != null && !AjustouC01)
            {
                AjustouC01 = true;
                try
                {
                    await JsRuntime.InvokeVoidAsync("AjustAutoSelect", "autocomplete01");
                }
                catch
                {

                }
            }
        }
        protected async Task Cancel()
        {
            if (Modal != null)
                await Modal.Hide();
            await OnCancel.InvokeAsync(new ChangeEventArgs() { Value = Item });
        }
        protected async Task Ok()
        {
            Resultado.Ok = false;
            if (Item.AlunoId == 0)
            {
                Resultado.ErrMsg = "Aluno não selecionado";
                return;
            }
            if (Item.StudioId == 0)
            {
                Resultado.ErrMsg = "Studio não selecionada";
                return;
            }
            if (Item.AulaId == 0)
            {
                Resultado.ErrMsg = "Aula não selecionada";
                return;
            }
            if (Item.ProfessorId == 0)
            {
                Resultado.ErrMsg = "Professor não selecionado";
                return;
            }

            if (!ProfessorAulas.Any(x => x.ProfessorId == Item.ProfessorId && x.AulaId == Item.AulaId))
            {
                Resultado.Ok = false;
                Resultado.ErrMsg = "Professor não tem permissão para dar essa aula";
                return;
            }

            int xdia = -90;

            if (InterfaceAgenda && (Item.DataProgramada < Constante.Today.AddDays(xdia) || Item.DataProgramada > Constante.Today.AddMonths(8)))
            {
                Resultado.ErrMsg = "Data anterior a data atual";
                return;
            }
            if (Item.TipoAula == TipoAula.Teste)
            {
                Item.Valor = 0;
            }
            try
            {
                if (Item.Status == StatusAula.Executada && Item.ProfessorRealId == 0)
                {
                    Item.ProfessorRealId = Item.ProfessorId;
                }
                if (Item.Id == 0)
                {
                    if (TipoAula == TipoAula.Avulsa)
                    {
                        Resultado = await CadService.PodeIncluirAulaAvulsa(Item.AlunoId, Item.AulaId);
                        if (!Resultado.Ok)
                        {
                            return;
                        }
                    }
                    Item = await Service.AddProgramaAula(Item);
                }
                else
                {
                    Resultado = await Service.UpdateProgramaAula(Item, ItemAnt);
                    if (!Resultado.Ok)
                    {
                        return;
                    }

                }
            }
            catch (Exception e)
            {
                Resultado.Ok = false;
                Resultado.ErrMsg = e.Message;
                return;
            }
            await OnOk.InvokeAsync(new ChangeEventArgs() { Value = Item });
            if (Modal != null)
            {
                await Modal.Hide();
            }
        }
        protected Task OnModalClosing(ModalClosingEventArgs e)
        {
            // just set Cancel to true to prevent modal from closing
            e.Cancel = true;
            return Task.CompletedTask;
        }
        protected string _inicio = string.Empty;
        protected string ValorInicio
        {
            get
            {
                if (_inicio == string.Empty)
                { _inicio = Item.HoraInicio.ToString("HH:mm"); };
                return _inicio;
            }
            set
            {
                if (value != null && value != string.Empty && value.Length > 2 && int.TryParse(value.Replace(":", ""), out _))
                {
                    string s = value.Replace(":", "");
                    if (int.TryParse(s.Substring(s.Length - 2, 2), out int mm))
                    {
                        if (int.TryParse(s[0..^2], out int hh) && mm < 60)
                        {
                            if (hh < 24)
                            {
                                Item.Inicio = hh * 60 + mm;
                                Resultado.Ok = true;
                                Resultado.ErrMsg = string.Empty;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    Resultado.Ok = false;
                    Resultado.ErrMsg = "Hora de inicio inválida";
                }
            }
        }
        protected void ValorInicioEvent()
        {
            _inicio = Item.HoraInicio.ToString("HH:mm");
            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
        }
        protected string _valorpago = string.Empty;
        protected string Valorpago
        {
            get
            {
                if (_valorpago == string.Empty)
                { _valorpago = Item.ValorPago.ToString("C2", Constante.Culture); };
                return _valorpago;
            }
            set
            {
                if (value != null && value != string.Empty && double.TryParse(value.Replace("$", "").Replace("R", "").Trim().Replace(".", ""), Constante.NumberStyles, Constante.Culture, out double d))
                {
                    Item.ValorPago = d;
                    Resultado.Ok = true;
                    Resultado.ErrMsg = string.Empty;
                }
                else
                {
                    Resultado.Ok = false;
                    Resultado.ErrMsg = "Valor pago inválido";
                }
            }
        }
        protected void ValorEventPago()
        {
            _valorpago = Item.ValorPago.ToString("C2", Constante.Culture);
            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
        }
        protected void MyListStudioValueChangedHandler(int newValue)
        {
            StudioSel = Studios.FirstOrDefault(x => x.Id == int.Parse(newValue.ToString()));
            if (StudioSel != null)
            {
                Item.Studio = StudioSel;
                Item.StudioId = StudioSel.Id;
            }
            else
            {
                Item.Studio = null;
                Item.StudioId = 0;
            }
            StateHasChanged();
        }
        protected void MyListAulaValueChangedHandler(int newValue)
        {
            AulaSel = Aulas.FirstOrDefault(x => x.Id == int.Parse(newValue.ToString()));
            if (AulaSel != null)
            {
                Item.Aula = AulaSel;
                Item.AulaId = AulaSel.Id;
                Item.AlunoPlanoId = 0;
                Item.Valor = Item.Aula.ValorAvulso;
            }
            else
            {
                Item.Aula = null;
                Item.AulaId = 0;
                Item.AlunoPlanoId = 0;
                Item.Valor = 0;
            }
            if (Item.AulaId > 0)
            {
                Professores = ProfessoresAll.Where(x => ProfessorAulas.Where(x => x.AulaId == Item.AulaId).ToList().Select(x => x.ProfessorId).Contains(x.Id)).ToList();
                if (Professores.Count > 0 && Item.ProfessorId == 0)
                {
                    ProfessorSel = Professores[0];
                    Item.Professor = ProfessorSel;
                    Item.ProfessorId = ProfessorSel.Id;
                }
            }
            else
            {
                Professores.Clear();
                ProfessorSel = null;
                Item.Professor = null;
                Item.ProfessorId = 0;
            }
            StateHasChanged();
        }
        protected void MyListAulaPlanoValueChangedHandler(int newValue)
        {
            AlunoPlanoAulaSel = AlunoPlanoAulas.FirstOrDefault(x => x.Id == int.Parse(newValue.ToString()));
            if (AlunoPlanoAulaSel != null)
            {
                Item.Aula = AlunoPlanoAulaSel.Aula;
                Item.AulaId = Item.Aula.Id;
                Item.AlunoPlanoId = AlunoPlanoAulaSel.AlunoPlanoId;
                Item.AlunoPlanoAulaId = AlunoPlanoAulaSel.Id;
                Item.Valor = AlunoPlanoAulaSel.ValorAula;
                _valor = Item.Valor.ToString("C2", Constante.Culture);

                if (Item.AulaId > 0)
                {
                    Professores = ProfessoresAll.Where(x => ProfessorAulas.Where(x => x.AulaId == Item.AulaId).ToList().Select(x => x.ProfessorId).Contains(x.Id)).ToList();
                    if (Professores.Count > 0 && Item.ProfessorId == 0)
                    {
                        ProfessorSel = Professores[0];
                        Item.Professor = ProfessorSel;
                        Item.ProfessorId = ProfessorSel.Id;
                    }
                }
            }
            else
            {
                Item.Aula = null;
                Item.AulaId = 0;
                Item.AlunoPlanoId = 0;
                Item.Valor = 0;
                Professores.Clear();
                ProfessorSel = null;
                Item.Professor = null;
                Item.ProfessorId = 0;
            }
            StateHasChanged();
        }
        protected void MyListProfessorValueChangedHandler(int newValue)
        {
            ProfessorSel = Professores.FirstOrDefault(x => x.Id == int.Parse(newValue.ToString()));
            if (ProfessorSel != null)
            {
                Item.Professor = ProfessorSel;
                Item.ProfessorId = ProfessorSel.Id;
            }
            else
            {
                Item.Professor = null;
                Item.ProfessorId = 0;
            }
            StateHasChanged();
        }

        protected void MyListProfessorRealValueChangedHandler(int newValue)
        {
            ProfessorRealSel = Professores.FirstOrDefault(x => x.Id == int.Parse(newValue.ToString()));
            if (ProfessorRealSel != null)
            {
                Item.ProfessorReal = ProfessorSel;
                Item.ProfessorRealId = ProfessorSel.Id;
            }
            else
            {
                Item.ProfessorReal = null;
                Item.ProfessorRealId = 0;
            }
            StateHasChanged();
        }

        protected string _valor = string.Empty;
        protected string Valor
        {
            get
            {
                if (_valor == string.Empty)
                { _valor = Item.Valor.ToString("C2", Constante.Culture); };
                return _valor;
            }
            set
            {
                if (value != null && value != string.Empty && double.TryParse(value.Replace("$", "").Replace("R", "").Trim().Replace(".", ""), Constante.NumberStyles, Constante.Culture, out double d))
                {
                    Item.Valor = d;
                    Resultado.Ok = true;
                    Resultado.ErrMsg = string.Empty;
                }
                else
                {
                    Resultado.Ok = false;
                    Resultado.ErrMsg = "Valor inválido";
                }
            }
        }
        protected void ValorEvent()
        {
            _valor = Item.Valor.ToString("C2", Constante.Culture);
            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
        }
    }


}
