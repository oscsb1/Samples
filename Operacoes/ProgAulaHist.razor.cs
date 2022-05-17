using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using InCorpApp.Services;
using InCorpApp.Models;
using InCorpApp.Constantes;
using InCorpApp.Shared;
using Microsoft.JSInterop;
using Blazorise;
using System.ComponentModel;

namespace InCorpApp.Pages.StudioPages.Operacoes
{
    public class ProgAulaHistBase : SFComponenteBase
    {
        [Parameter]
        public ProgramacaoAula ProgramacaoAula { get; set; }
        [Parameter]
        public EventCallback<ChangeEventArgs> OnCancel { get; set; }
        [Inject]
        protected StudioOperacoesService Service { get; set; }
        public List<ProgramacaoAulaHist> Items { get; set; }
        protected Modal Modal { get; set; }
        protected string HdrTitulo { get; set; }
        protected bool ShowModal { get; set; } = false;
        protected async override Task OnAfterRenderAsync(bool firstrender)
        {
            if (firstrender)
            {
                HdrTitulo = ProgramacaoAula.Aluno.Nome;
                Items = await Service.LoadProgramacaoHist(ProgramacaoAula.Id);
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

        protected Task OnModalClosing(ModalClosingEventArgs e)
        {
            // just set Cancel to true to prevent modal from closing
            e.Cancel = true;
            return Task.CompletedTask;
        }

        protected async Task Cancel()
        {
            await Modal.Hide();
            await OnCancel.InvokeAsync(new ChangeEventArgs() { Value = ProgramacaoAula });
        }
    }
}