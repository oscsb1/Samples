using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InCorpApp.Services;
using InCorpApp.Models;
using InCorpApp.Constantes;
using Blazorise.DataGrid;
using InCorpApp.Shared;
using Microsoft.JSInterop;
using InCorpApp.Utils;
using Blazorise.Components;
using System.Linq;

namespace InCorpApp.Pages.StudioPages.Operacoes
{
    public class AgendaGeralBase : SFComponenteBase
    {
        [Parameter]
        public Studio Studio { get; set; }
        [Parameter]
        public List<Studio> Studios { get; set; }
        [Parameter]
        public bool Reagendar { get; set; } = false;
        [Parameter]
        public EventCallback<int> OnReagendar { get; set; }
        [Inject]
        protected StudioOperacoesService Service { get; set; }
        [Inject]
        protected StudioCadastroService CadService { get; set; }
        [Inject]
        protected BasicDBService BasicDBService { get; set; }
        protected List<ProgramacaoAula> Items { get; set; } = new();
        protected ProgramacaoAula SelectedRow { get; set; }
        protected DataGrid<ProgramacaoAula> Grid { get; set; }
        protected DateTime DtIni { get; set; }
        protected DateTime DtFim { get; set; }
        protected bool ShowEdit { get; set; }
        protected List<Aluno> Alunos { get; set; }
        protected List<Professor> Professores { get; set; }
        protected int SelectedRelacionamento { get; set; } = 0;
        protected int SelectedProfessor { get; set; } = 0;
        protected bool ShowDDStatus { get; set; } = false;
        protected string DropStatusnome { get; set; } = string.Empty;
        protected List<StatusAula> SelectedListStatus { get; set; } = new();
        protected Autocomplete<Aluno, int> AutoC01 { get; set; }
        protected Autocomplete<Professor, int> AutoC02 { get; set; }
        protected bool AjustouC01 { get; set; } = false;
        protected void RelacionamentoSearchHandler(int cc)
        {
            SelectedRelacionamento = cc;
            StateHasChanged();
        }
        protected void ProfessorSearchHandler(int cc)
        {
            SelectedProfessor = cc;
            StateHasChanged();
        }
        protected async override Task OnAfterRenderAsync(bool firstrender)
        {
            if (firstrender)
            {
                try
                {
                    if (Studio == null || Studios == null)
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
                    }
                    if (Reagendar)
                    {
                        SelectedListStatus.Add(StatusAula.ReAgendamento);
                        DtIni = DateTime.MinValue;
                        DtFim = DateTime.MaxValue;
                    }
                    else
                    {
                        DtIni = UtilsClass.GetInicio(Constante.Today);
                        DtFim = UtilsClass.GetUltimo(Constante.Today);
                        DropStatusnome = "Selecionar status";
                    }
                    Alunos = await CadService.FindAllAlunoAsync();
                    Professores = await CadService.FindAllProfessorAsync();
                }
                catch (Exception e)
                {
                    Resultado.Ok = false;
                    Resultado.ErrMsg = e.Message;
                    Resultado.Grave = true;
                    Items = null;
                }
                Isloading = false;
                Titulo = "Agenda geral";
                StateHasChanged();
                if (Grid != null)
                {
                    TotalPages = GetTotalPages(Items.Count, Grid.PageSize);
                }
            }

            await base.OnAfterRenderAsync(firstrender);
            if (firstrender)
            {
                try
                {
                    await Task.Delay(300);
                    await JsRuntime.InvokeVoidAsync("ajustatable", "tabler", Constante.GapTable);
                }
                catch
                {
                }
            }
        }
        protected async Task OnReturn()
        {
            ShowEdit = false;
            SelectedRow = null;
            await OnPorData();
        }
        protected async Task OnPorData()
        {
            if (Processando)
                return;

            if (Reagendar)
            {
                if (SelectedRelacionamento == 0)
                {
                    Resultado.Ok = false;
                    Resultado.ErrMsg = "Aluno não selecionado";
                    return;
                }
            }
            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
            Processando = true;
            try
            {
                Isloading = true;
                StateHasChanged();
                await Task.Delay(200);
                BasicDBService.ClearTracking();
                Items = await CadService.FindAllProgramacaoByDate(Studio.Id, DtIni, DtFim, SelectedRelacionamento, SelectedProfessor, SelectedListStatus);
            }
            finally
            {
                Isloading = false;
                Processando = false;
                ShowDDStatus = false;
            }
            SelectedRow = null;

            if (Items.Count == 0)
            {
                Resultado.Ok = false;
                Resultado.ErrMsg = "Nenhuma aula selecionada";
            }
            else
            {
                Resultado.Ok = false;
                Resultado.ErrMsg = "Total de " + Items.Count + " selecionadas";
            }
            StateHasChanged();
            try
            {
                await Task.Delay(300);
                await JsRuntime.InvokeVoidAsync("ajustatable", "tabler", Constante.GapTable);
            }
            catch
            {
            }
        }
        protected async Task OnMesAnt()
        {
            if (Processando)
                return;
            DtFim = UtilsClass.GetUltimo(DtFim.AddDays(-35));
            DtIni = UtilsClass.GetInicio(DtFim);
            await OnPorData();

        }
        protected async Task OnMesDep()
        {
            if (Processando)
                return;
            DtIni = UtilsClass.GetInicio(DtIni.AddDays(35));
            DtFim = UtilsClass.GetUltimo(DtIni);
            await OnPorData();
        }
        protected void OnEditClick()
        {
            if (SelectedRow == null)
            {
                Resultado.Ok = false;
                Resultado.ErrMsg = Constante.RowNotSelected;
                StateHasChanged();
                return;
            }
            ShowEdit = true;
        }
        protected async Task OnClickAgendar()
        {
            if (Processando)
                return;
            Processando = true;
            try
            {
                if (SelectedRow == null)
                {
                    Resultado.Ok = false;
                    Resultado.ErrMsg = Constante.RowNotSelected;
                    StateHasChanged();
                    return;
                }
                await OnReagendar.InvokeAsync(SelectedRow.Id);
            }
            finally
            {
                Processando = false;
            }
        }
        protected async Task OnVoltar()
        {
            await OnReagendar.InvokeAsync(0);
        }
        protected void OnSelectListStatus(StatusAula s)
        {
            if (SelectedListStatus.Contains(s))
            {
                SelectedListStatus.Remove(s);
            }
            else
            {
                SelectedListStatus.Add(s);
            }
            if (SelectedListStatus.Count == 0)
            {
                DropStatusnome = "Selecionar status";
            }
            else
            {
                DropStatusnome = SelectedListStatus.Count + " status selecionados";
            }
            ShowDDStatus = true;
            StateHasChanged();
        }
        protected void OnClickDrop()
        {
            ShowDDStatus = !ShowDDStatus;
            StateHasChanged();
        }
        protected bool Checked(StatusAula s)
        {
            return SelectedListStatus.Contains(s);
        }
        protected void MyListStudioValueChangedHandler(int newValue)
        {
            Studio = Studios.FirstOrDefault(x => x.Id == int.Parse(newValue.ToString()));
        }
    }
}

