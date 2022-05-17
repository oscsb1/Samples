using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Shared;
using InCorpApp.Models;
using Microsoft.AspNetCore.Components;
using InCorpApp.Services;
using InCorpApp.Constantes;
using Microsoft.JSInterop;
using InCorpApp.Security;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace InCorpApp.Pages.StudioPages.Operacoes
{
    public class LinhaPlanoAula
    {
        public string MesAno { get; set; }
        public string AnoMes { get; set; }
        public double Valor { get; set; }
        public List<List<ProgramacaoAula>> Aulas { get; set; } = new List<List<ProgramacaoAula>>();
    }
    public class AprovacaoPlanoBase : SFComponenteBase
    {
        [Parameter]
        public string Id { get; set; }
        [Inject]
        protected StudioOperacoesService Service { get; set; }
        [Inject]
        protected SegurancaService SegurancaService { get; set; }
        [Inject]
        protected StudioCadastroService CadService { get; set; }
        [Inject]
        protected AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        protected string MsgHDR { get; set; }
        protected string[] MsgHDRL => MsgHDR.Split(".");
        protected string MsgFooter { get; set; }
        protected string[] MsgFooterL => MsgFooter.Split(".");
        protected string MsgPacoteAula { get; set; }
        protected string[] MsgPacoteAulaL => MsgPacoteAula.Split(".");
        protected bool ShowConfirmacaoAprov { get; set; }
        private int _alunoPlanoId;
        protected int Aprovado { get; set; } = 0;
        protected List<ProgramacaoAula> Aulas { get; set; }
        protected Plano Plano { get; set; }
        protected AlunoPlano AlunoPlano { get; set; }
        protected List<AlunoPlanoParcela> Parcelas { get; set; }
        protected List<LinhaPlanoAula> Qaulas { get; set; } = new List<LinhaPlanoAula>();
        protected async override Task OnAfterRenderAsync(bool firstrender)
        {
            if (firstrender)
            {
                var token = await SegurancaService.FindTokenByGuid(Id);
                if (token == null)
                {
                    NavigationManager.NavigateTo("/", true);
                    return;
                }
                if (!int.TryParse(token.GetParmToken(2), out _alunoPlanoId))
                {
                    NavigationManager.NavigateTo("/", true);
                    return;
                }
                try
                {
                    await ((CustomAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsLoggedOut();
                }
                catch
                {
                }
                IdentityUser user = new()
                {
                    UserName = Constante.GuestUser,
                    Id = Guid.NewGuid().ToString()
                };
                if ((await ((CustomAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsAuthenticated(user)).Ok)
                {
                    await SegurancaService.SetTenantIdToCurrentSession(token.TenantId);
                }
                else
                {
                    NavigationManager.NavigateTo("/", true);
                    return;
                }
                StudioConfig sc = await CadService.GetStudioConfig();
                AlunoPlano = await CadService.FindAlunoPlanoByIdAsync(_alunoPlanoId);
                if (AlunoPlano.TokenAprovacao != Id)
                {
                    Resultado.Ok = false;
                    Aprovado = 3;
                    Resultado.ErrMsg = "Token para aprovação expirou, solicitar novo token.";
                    StateHasChanged();
                    return;
                }
                Plano = await CadService.FindPlanoByIdAsync(AlunoPlano.PlanoId);
                Aluno al = await CadService.FindAlunoByIdAsync(AlunoPlano.AlunoId);
                MsgHDR = sc.HdrContratoAproval.Replace("@aluno", al.Nome);
                MsgFooter = sc.FooterContratoAproval.Replace("@aluno", al.Nome);
                MsgHDR = MsgHDR.Replace("@plano", Plano.Nome);
                MsgFooter = MsgFooter.Replace("@plano", Plano.Nome);
                MsgPacoteAula = sc.MsgPacoteHoras;
                Aulas = await CadService.FindAllProgramacaoByAlunoPlanoId(_alunoPlanoId);
                Parcelas = await CadService.FindAllAlunoPlanoParcelaByAlunoPlanoIdAsync(_alunoPlanoId);

                if (double.TryParse(token.GetParmToken(3), out double vp)
                  && int.TryParse(token.GetParmToken(4), out int qp)
                  && double.TryParse(token.GetParmToken(5), out double va)
                  && int.TryParse(token.GetParmToken(6), out int qa))
                {
                    if (vp != Parcelas.Sum(x => x.Valor)
                        || qp != Parcelas.Count
                        || va != Aulas.Sum(x => x.Valor)
                        || qa != Aulas.Count
                        )
                    {
                        Resultado.Ok = false;
                        Aprovado = 3;
                        Resultado.ErrMsg = "Token para aprovação expirou, solicitar novo token.";
                        StateHasChanged();
                        return;
                    }
                }
                foreach (var a in AlunoPlano.Aulas)
                {
                    a.TotalAulasFeitas = await CadService.GetTotalAulasDadasByPlanoIdAulaId(AlunoPlano.AlunoId, AlunoPlano.Id, a.AulaId);
                }

                var q = Aulas.GroupBy(
     linha => linha.AnoMes,
    (AnoMes, Linhas) => new
    {
        Key = AnoMes,
        MesAno = Linhas.Max(x => x.MesAno),
        Valor = Linhas.Sum(x => x.Valor),
        Linhas
    });

                foreach (var l in q)
                {
                    LinhaPlanoAula la = new()
                    {
                        AnoMes = l.Key,
                        MesAno = l.MesAno,
                        Valor = l.Valor
                    };
                    int i = 0;
                    foreach (var a in l.Linhas)
                    {
                        i++;
                        if (i == 1 || i == 5 || i == 9 || i == 13 || i == 17 || i == 21 || i == 25 || i == 29)
                        {
                            List<ProgramacaoAula> npa = new();
                            la.Aulas.Add(npa);
                        }
                        la.Aulas[^1].Add(a);
                    };
                    Qaulas.Add(la);
                }
                Qaulas = Qaulas.OrderBy(x => x.AnoMes).ToList();

                Isloading = false;
                StateHasChanged();
                try
                {
                    await JsRuntime.InvokeVoidAsync("ajustatable", "tabler", Constante.GapTable);
                }
                catch
                {

                }
            }
            await base.OnAfterRenderAsync(firstrender);
        }
        protected void OnAprovoClick()
        {
            ShowConfirmacaoAprov = true;
            StateHasChanged();
        }
        protected async Task OnNaoAprovoClick()
        {
            ShowConfirmacaoAprov = false;
            Aprovado = 2;
            try
            {
                await ((CustomAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsLoggedOut();
            }
            catch
            {
            }
            StateHasChanged();
        }
        protected async Task OnProcessarAprovacao()
        {
            ShowConfirmacaoAprov = false;
            try
            {
                Resultado = await Service.ConfirmarPlanoAsync(_alunoPlanoId, Id);
                //    await SegurancaService.RemoveTokenByGuid(Id);
            }
            catch
            {

            }
            if (Resultado.Ok)
            {
                Aprovado = 1;
            }
            else
            {
                Aprovado = 3;
            }
            try
            {
                await ((CustomAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsLoggedOut();
            }
            catch
            {
            }
            StateHasChanged();
        }

        protected async Task OnProcessarNaoAprovacao()
        {
            ShowConfirmacaoAprov = false;
            Aprovado = 2;
            try
            {
                await ((CustomAuthenticationStateProvider)AuthenticationStateProvider).MarkUserAsLoggedOut();
            }
            catch
            {
            }
            StateHasChanged();
        }
    }
}
