using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Constantes;
using InCorpApp.Models;
using InCorpApp.Services;
using InCorpApp.Shared;
using Microsoft.AspNetCore.Components;

namespace InCorpApp.Pages.StudioPages.Operacoes
{
    public class ProgramacaoBase : SFComponenteBase
    {
        [Inject]
        protected StudioCadastroService CadService { get; set; }
        [Inject]
        protected StudioOperacoesService Service { get; set; }
        protected List<Studio> Studios { get; set; }
        protected Studio StudioSel { get; set; }
        protected int StudioId { get; set; }
        protected DateTime DataInicio { get; set; }
        protected DateTime DataFim { get; set; }
        protected async override Task OnAfterRenderAsync(bool firstrender)
        {
            await base.OnAfterRenderAsync(firstrender);
            if (firstrender)
            {
                Studios = await CadService.FindAllStudioAsync();
                StudioSel = Studios.FirstOrDefault(x => x.Id > 0);
                StudioId = StudioSel.Id;
                Resultado.Ok = false;
                Isloading = false;
                Titulo = "Programação das agendas";
                DataInicio = Constante.Today.AddDays(1);
                DataFim = Utils.UtilsClass.GetUltimo(DataInicio);
                Resultado.Ok = false;
                Isloading = false;
                StateHasChanged();
            }
        }
        protected void OnCancel()
        {
            NavigationManager.NavigateTo("StudioPages/StudioIndex/",true);
        }
        protected async Task HandleValidSubmit()
        {
            try
            {
                if (Processando)
                {
                    return;
                }
                Processando = true;
                if (StudioSel == null)
                {
                    Resultado.Ok = false;
                    Resultado.ErrMsg = "Selecionar o studio";
                    return;
                }
                if (DataInicio < Constante.Today)
                {
                    Resultado.Ok = false;
                    Resultado.ErrMsg = "data inválida";
                    return;
                }
                if (DataInicio > Constante.Today.AddMonths(6))
                {
                    Resultado.Ok = false;
                    Resultado.ErrMsg = "data maior que "  + Constante.Today.AddMonths(6).ToString("dd/MM/yyyy") ;
                    return;
                }
                if (DataInicio > DataFim)
                {
                    Resultado.Ok = false;
                    Resultado.ErrMsg = "data maior que a data de fim";
                    return;
                }
                Resultado = await Service.GerarProgramacao(StudioSel.Id, DataInicio, DataFim);
            }
            catch (Exception e)
            {
                Resultado.Ok = false;
                Resultado.ErrMsg = e.Message;
            }
            finally
            {
                Processando = false;
            }
            return;
        }
        protected void MyListStudioValueChangedHandler(int newValue)
        {
            StudioSel = Studios.FirstOrDefault(x => x.Id == int.Parse(newValue.ToString()));
            StudioId = StudioSel.Id;
            StateHasChanged();
        }
    }
}
