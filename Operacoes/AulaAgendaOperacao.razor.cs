
using InCorpApp.Models;
using InCorpApp.Services;
using InCorpApp.Shared;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Constantes;
using System.ComponentModel;
using Blazorise;
using System;

namespace InCorpApp.Pages.StudioPages.Operacoes
{
    public enum TipoOperacaoAulaAgendaOperacao : int
    {
        TrocarProfessorSala = 1,
        RegistrarAula = 2,
        ReabrirAula = 3
    }
    public class AulaAgendaOperacaoBase : SFComponenteBase
    {
        [Parameter]
        public EventCallback<ChangeEventArgs> OnCancel { get; set; }
        [Parameter]
        public EventCallback<ChangeEventArgs> OnOk { get; set; }
        [Parameter]
        public Studio Studio { get; set; }
        [Parameter]
        public AulaAgenda Item { get; set; }
        [Parameter]
        public DateTime Dia { get; set; }
        [Parameter]
        public List<Professor> Professores { get; set; }
        [Inject]
        protected StudioOperacoesService Service { get; set; }
        [Inject]
        protected StudioCadastroService CadService { get; set; }
        public TipoOperacaoAulaAgendaOperacao Operacao { get; set; }
        protected string HdrTitulo { get; set; }
        public string BodyTitulo { get; set; }
        protected Modal Modal { get; set; }
        protected bool ShowConfimaProfDif { get; set; } = false;
        protected bool DisableBT { get; set; } = false;
        protected List<StudioSala> StudioSalas { get; set; }
        protected bool ShowModal { get; set; } = false;

        protected async override Task OnAfterRenderAsync(bool firstrender)
        {
            if (firstrender)
            {
                StudioSalas = await CadService.FindAllStudioSala(Studio.Id);
                HdrTitulo = Item.AulaNome;
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
            await base.OnAfterRenderAsync(firstrender);
        }
        protected async Task Cancel()
        {
            await Modal.Hide();
            await OnCancel.InvokeAsync(new ChangeEventArgs() { Value = Item });
        }
        protected async Task Ok()
        {
            if (Processando)
            {
                return;
            }
            Processando = true;
            try
            {

                if (Operacao == TipoOperacaoAulaAgendaOperacao.TrocarProfessorSala)
                {
                  Resultado = await Service.ChangeProfessorAulaAgenda(Item, Dia, Item.ProfessorId, Item.StudioSalaId);
                }
                if (Operacao == TipoOperacaoAulaAgendaOperacao.RegistrarAula)
                {
                    Resultado = await Service.RegistrarAulaAgenda(Item, Dia);
                }
                if (Operacao == TipoOperacaoAulaAgendaOperacao.ReabrirAula)
                {
                    Resultado = await Service.ReabrirAulaAgenda(Item, Dia);
                }
                if (Resultado.Ok)
                {
                    await OnOk.InvokeAsync(new ChangeEventArgs() { Value = Item });
                }
            }
            finally
            {
                Processando = false;
            }
            await Modal.Hide();
        }

        protected void OnCancelRegistro()
        {
            ShowConfimaProfDif = false;
            StateHasChanged();
        }

        protected async Task OnConfirmaRegistro()
        {
            await Ok();
            ShowConfimaProfDif = false;
            StateHasChanged();
        }

        protected Task OnModalClosing(ModalClosingEventArgs e)
        {
            e.Cancel = true;
            return Task.CompletedTask;
        }
        protected void RegistrarClick()
        {
            Resultado.SetDefault();
            DisableBT = true;
            Operacao = TipoOperacaoAulaAgendaOperacao.RegistrarAula;
            StateHasChanged();
        }
        protected void ReabrirClick()
        {
            Resultado.SetDefault();
            DisableBT = true;
            Operacao = TipoOperacaoAulaAgendaOperacao.ReabrirAula;
            StateHasChanged();
        }
        protected void MyListProfessorValueChangedHandler(int newValue)
        {
            Item.Professor = Professores.FirstOrDefault(x => x.Id == int.Parse(newValue.ToString()));
            Item.ProfessorId = Item.Professor.Id;
            StateHasChanged();
        }
        protected void MyListSalaValueChangedHandler(int newValue)
        {
            Item.StudioSala = StudioSalas.FirstOrDefault(x => x.Id == int.Parse(newValue.ToString()));
            Item.StudioSalaId = Item.StudioSala.Id;
            StateHasChanged();
        }

        protected void MudarProfessorSalaClick()
        {
            Resultado.SetDefault();
            DisableBT = true;
            Operacao = TipoOperacaoAulaAgendaOperacao.TrocarProfessorSala;
            StateHasChanged();
        }
        
    }
}
