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

namespace InCorpApp.Pages.StudioPages.Operacoes
{
    public enum TipoOperacaoAula : int
    {
        none = 0,
        RegistrarAulaExecutada = 1
    }
    public class ProgramacaoAulaDetailBase : SFComponenteBase
    {
        [Parameter]
        public EventCallback<ChangeEventArgs> OnCancel { get; set; }
        [Parameter]
        public EventCallback<ChangeEventArgs> OnOk { get; set; }
        [Parameter]
        public ProgramacaoAula Item { get; set; }
        [Parameter]
        public List<Professor> Professores { get; set; }
        [Parameter]
        public int ProfessorId { get; set; } = 0;
        [Inject]
        protected StudioOperacoesService Service { get; set; }
        [Inject]
        protected StudioCadastroService CadService { get; set; }
        protected string HdrTitulo { get; set; }
        public string BodyTitulo { get; set; }
        protected Modal Modal { get; set; }
        protected bool DataDisp { get; set; } = false;
        protected bool ProfDisp { get; set; } = false;
        protected bool ProfRealDisp { get; set; } = false;
        protected bool ShowConfimaProfDif { get; set; } = false;
        protected string OBS { get; set; } = string.Empty;
        protected string Prontuario { get; set; } = string.Empty;
        protected StatusAula NovoStatus { get; set; }
        protected bool ShowModal { get; set; } = false;
        protected bool DisableBts { get; set; } = false;
        protected TipoOperacaoAula OperacaoAula { get; set; }
        protected List<ProfessorAula> ProfessorAulas { get; set; }
        protected List<Professor> ProfessoresPossivel { get; set; } = new();
        protected ProgramacaoAula ProgramacaoAulaTemp { get; set; }
        protected ProgramacaoAula ProgramacaoAulaTemp2 { get; set; }
        protected Resultado Resultado2 { get; set; } = new();
        protected async override Task OnAfterRenderAsync(bool firstrender)
        {
            if (firstrender)
            {
                HdrTitulo = Item.Aluno.Nome;
                if (Item.TipoAula == TipoAula.Avulsa)
                {
                    HdrTitulo = Item.Aluno.Nome.ToUpper() + " - AULA AVULSA";
                }
                if (Item.TipoAula == TipoAula.Teste)
                {
                    HdrTitulo = Item.Aluno.Nome.ToUpper() + " - AULA EXPERIMENTAL";
                }
                if (Item.TipoAula == TipoAula.Pacote)
                {
                    HdrTitulo = Item.Aluno.Nome.ToUpper() + " - PACOTE DE AULAS";
                    var p = await CadService.FindAlunoPlanoByIdAsync(Item.AlunoPlanoId);
                    if (p != null)
                    {
                        var a = p.Aulas.FirstOrDefault(x => x.AulaId == Item.AulaId);
                        if (a != null)
                        {
                            int tot = await CadService.GetTotalAulasDadasByPlanoIdAulaId(Item.AlunoId, Item.AlunoPlanoId, Item.AulaId);
                            if (a.QtdeAulas - tot == 0)
                            {
                                HdrTitulo = Item.Aluno.Nome + " - Todas as " + a.QtdeAulas + " do pacote foram executadas.";
                            }
                            else
                            {
                                HdrTitulo = Item.Aluno.Nome + " - faltam " + (a.QtdeAulas - tot).ToString() + " de " + a.QtdeAulas.ToString();
                            }
                        }
                    }
                }
                if (Item.TipoAula == TipoAula.Plano)
                {
                    var p = await CadService.FindAlunoPlanoByIdAsync(Item.AlunoPlanoId);
                    if (p != null)
                    {
                        HdrTitulo = Item.Aluno.Nome.ToUpper() + " - Plano: " + p.Plano.Nome + " venc: " + p.DataFimV;
                    }
                    else
                    {
                        HdrTitulo = Item.Aluno.Nome.ToUpper();
                    }
                }

                if (Item.ProfessorRealId == 0)
                {
                    Item.ProfessorRealId = Item.ProfessorId;
                    Item.ProfessorReal = Item.Professor;
                }
                ProgramacaoAulaTemp = new ProgramacaoAula()
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
                    DataPagto = Item.DataPagto
                };
                ProgramacaoAulaTemp2 = new ProgramacaoAula()
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
                    DataPagto = Item.DataPagto
                };

                if (Item.Status == StatusAula.Executada)
                {
                    Item.ProntuarioAula = await Service.GetProntuario(Item.Id);
                }

                ProfessorAulas = await CadService.FindAllProfessorAula();
                ProfessoresPossivel = Professores.Where(x => ProfessorAulas.Where(x => x.AulaId == Item.AulaId).ToList().Select(x => x.ProfessorId).Contains(x.Id)).ToList();

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
            await Modal .Hide();
            OperacaoAula = TipoOperacaoAula.none;
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
                if (ProgramacaoAulaTemp.Status == StatusAula.Excluir)
                {
                    Resultado = await Service.DeleteProgramacaoAulaAsync(Item);
                    await OnOk.InvokeAsync(new ChangeEventArgs() { Value = Item });
                    return;
                }

                if (ProgramacaoAulaTemp.DataProgramada < Constante.Today && Item.Status == StatusAula.ReAgendamento &&
                   (ProgramacaoAulaTemp.Status == StatusAula.Agendada || ProgramacaoAulaTemp.Status == StatusAula.Programada))
                {
                    Resultado.Ok = false;
                    Resultado.ErrMsg = "Data anterior a data atual";
                    return;
                }
                if (!ShowConfimaProfDif && ProgramacaoAulaTemp.ProfessorRealId != ProgramacaoAulaTemp.ProfessorId && ProgramacaoAulaTemp.Status == StatusAula.Executada)
                {
                    ShowConfimaProfDif = true;
                    StateHasChanged();
                    return;
                }
                Item.DataProgramada = ProgramacaoAulaTemp.DataProgramada;
                Item.Fim = ProgramacaoAulaTemp.Fim;
                Item.Inicio = ProgramacaoAulaTemp.Inicio;
                Item.OBS = ProgramacaoAulaTemp.OBS;
                Item.Origem = ProgramacaoAulaTemp.Origem;
                Item.ProfessorId = ProgramacaoAulaTemp.ProfessorId;
                Item.Status = ProgramacaoAulaTemp.Status;
                Item.Professor = ProgramacaoAulaTemp.Professor;
                Item.ProfessorReal = ProgramacaoAulaTemp.ProfessorReal;
                if (Item.Status != StatusAula.Executada)
                {
                    Item.ProfessorReal = ProgramacaoAulaTemp.Professor;
                    Item.ProfessorRealId = ProgramacaoAulaTemp.ProfessorId;
                }
                else
                {
                    Item.ProfessorReal = ProgramacaoAulaTemp.ProfessorReal;
                    Item.ProfessorRealId = ProgramacaoAulaTemp.ProfessorRealId;
                }
                Resultado = await Service.UpdateProgramaAula(Item, ProgramacaoAulaTemp2);
                if (!Resultado.Ok)
                {
                    Item.DataProgramada = ProgramacaoAulaTemp2.DataProgramada;
                    Item.Fim = ProgramacaoAulaTemp2.Fim;
                    Item.Inicio = ProgramacaoAulaTemp2.Inicio;
                    Item.OBS = ProgramacaoAulaTemp2.OBS;
                    Item.Origem = ProgramacaoAulaTemp2.Origem;
                    Item.ProfessorId = ProgramacaoAulaTemp2.ProfessorId;
                    Item.ProfessorRealId = ProgramacaoAulaTemp2.ProfessorRealId;
                    Item.Status = ProgramacaoAulaTemp2.Status;
                    Item.Professor = ProgramacaoAulaTemp2.Professor;
                    Item.ProfessorReal = ProgramacaoAulaTemp2.ProfessorReal;
                    return;
                }
                await OnOk.InvokeAsync(new ChangeEventArgs() { Value = Item });
            }
            finally
            {
                OperacaoAula = TipoOperacaoAula.none;
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
            // just set Cancel to true to prevent modal from closing
            e.Cancel = true;
            return Task.CompletedTask;
        }
        protected void RegistrarClick()
        {
            Resultado2.Ok = true;
            Resultado2.ErrMsg = string.Empty;
            if (Item.Status == StatusAula.FaltaSemReagendamento)
            {
                Resultado.Ok = false;
                Resultado.ErrMsg = "Aula com falta do aluno, sem direito a reagendamento";
                return;
            }
            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
            if (ProgramacaoAulaTemp.ProfessorRealId == 0)
            {
                Resultado.Ok = false;
                Resultado.ErrMsg = "Professor que deu a aula não foi selecioado";
                return;
            }
            ProgramacaoAulaTemp.Status = StatusAula.Executada;
            if (ProgramacaoAulaTemp.OBS == null || ProgramacaoAulaTemp.OBS == string.Empty || ProgramacaoAulaTemp.OBS == ProgramacaoAulaTemp2.OBS)
            {
                ProgramacaoAulaTemp.OBS = "Aula registrada em " + Constante.Now.ToString("dd/MM/yyyy HH:mm");
            }
            DataDisp = false;
            ProfDisp = false;
            Resultado2.ErrMsg = ProgramacaoAulaTemp.OBS;
            Resultado2.Ok = false;
            DisableBts = true;
            OperacaoAula = TipoOperacaoAula.RegistrarAulaExecutada;
            StateHasChanged();
        }
        protected void ReabrirClick()
        {
            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
            ProgramacaoAulaTemp.Status = StatusAula.Agendada;
            if (ProgramacaoAulaTemp.OBS == null || ProgramacaoAulaTemp.OBS == string.Empty || ProgramacaoAulaTemp.OBS == ProgramacaoAulaTemp2.OBS)
            {
                ProgramacaoAulaTemp.OBS = "Aula reaberta em " + Constante.Now.ToString("dd/MM/yyyy HH:mm");
            }
            Resultado2.ErrMsg = ProgramacaoAulaTemp.OBS;
            Resultado2.Ok = false;
            DisableBts = true;
            StateHasChanged();
        }
        protected async Task ReagendarClick()
        {
            Resultado2.Ok = true;
            Resultado2.ErrMsg = string.Empty;

            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
            if (Item.Status == StatusAula.FaltaSemReagendamento)
            {
                Resultado.Ok = false;
                Resultado.ErrMsg = "Aula com falta do aluno, sem direito a reagendamento";
                return;
            }

            AlunoPlano ap = await CadService.FindAlunoPlanoByIdAsync(Item.AlunoPlanoId);
            if (ap != null)
            {
                if (ap.Status == StatusPlanoAluno.PendenteConfirmacao)
                {
                    ProgramacaoAulaTemp.Status = StatusAula.Programada;
                }
                else
                {
                    ProgramacaoAulaTemp.Status = StatusAula.Agendada;
                }
            }
            else
            {
                ProgramacaoAulaTemp.Status = StatusAula.Agendada;
            }
            ProgramacaoAulaTemp.Origem = OrigemProgramacao.Manual;
            if (ProgramacaoAulaTemp.OBS == null || ProgramacaoAulaTemp.OBS == string.Empty || ProgramacaoAulaTemp.OBS == ProgramacaoAulaTemp2.OBS)
            {
                ProgramacaoAulaTemp.OBS = "Aula reagendada em " + Constante.Now.ToString("dd/MM/yyyy HH:mm");
            }
            Resultado2.ErrMsg = ProgramacaoAulaTemp.OBS;
            Resultado2.Ok = false;
            DataDisp = true;
            ProfDisp = true;
            DisableBts = true;
            StateHasChanged();
        }
        protected void DesmarcarClick()
        {
            Resultado2.Ok = true;
            Resultado2.ErrMsg = string.Empty;
            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
            ProgramacaoAulaTemp.Origem = OrigemProgramacao.Manual;
            ProgramacaoAulaTemp.Status = StatusAula.ReAgendamento;
            if (ProgramacaoAulaTemp.OBS == null || ProgramacaoAulaTemp.OBS == string.Empty || ProgramacaoAulaTemp.OBS == ProgramacaoAulaTemp2.OBS)
            {
                ProgramacaoAulaTemp.OBS = "Aula desmarcada em " + Constante.Now.ToString("dd/MM/yyyy HH:mm");
            }
            Resultado2.ErrMsg = ProgramacaoAulaTemp.OBS;
            Resultado2.Ok = false;
            DisableBts = true;
            StateHasChanged();
        }
        protected void RegistrarFaltaClick()
        {
            Resultado2.Ok = true;
            Resultado2.ErrMsg = string.Empty;
            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
            ProgramacaoAulaTemp.Status = StatusAula.FaltaSemReagendamento;
            ProgramacaoAulaTemp.Origem = OrigemProgramacao.Manual;
            if (ProgramacaoAulaTemp.OBS == null || ProgramacaoAulaTemp.OBS == string.Empty || ProgramacaoAulaTemp.OBS == ProgramacaoAulaTemp2.OBS)
            {
                ProgramacaoAulaTemp.OBS = "Registrado falta em " + Constante.Now.ToString("dd/MM/yyyy HH:mm");
            }
            Resultado2.ErrMsg = ProgramacaoAulaTemp.OBS;
            Resultado2.Ok = false;
            DataDisp = false;
            ProfDisp = false;
            DisableBts = true;
            StateHasChanged();
        }
        protected string _inicio = string.Empty;
        protected string ValorInicio
        {
            get
            {
                if (_inicio == string.Empty)
                { _inicio = ProgramacaoAulaTemp.HoraInicio.ToString("HH:mm"); };
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
                                ProgramacaoAulaTemp.Inicio = hh * 60 + mm;
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
            _inicio = ProgramacaoAulaTemp.HoraInicio.ToString("HH:mm");
            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
        }
        protected void MyListProfessorValueChangedHandler(int newValue)
        {
            ProgramacaoAulaTemp.Professor = Professores.FirstOrDefault(x => x.Id == int.Parse(newValue.ToString()));
            ProgramacaoAulaTemp.ProfessorId = ProgramacaoAulaTemp.Professor.Id;
            if (ProgramacaoAulaTemp.Status != StatusAula.Executada)
            {
                ProgramacaoAulaTemp.ProfessorReal = ProgramacaoAulaTemp.Professor;
                ProgramacaoAulaTemp.ProfessorRealId = ProgramacaoAulaTemp.ProfessorReal.Id;
            }
            StateHasChanged();
        }
        protected void MyListProfessorRealValueChangedHandler(int newValue)
        {
            ProgramacaoAulaTemp.ProfessorReal = Professores.FirstOrDefault(x => x.Id == int.Parse(newValue.ToString()));
            ProgramacaoAulaTemp.ProfessorRealId = ProgramacaoAulaTemp.ProfessorReal.Id;
            StateHasChanged();
        }
        protected void CancelarTesteClick()
        {
            Resultado2.Ok = true;
            Resultado2.ErrMsg = string.Empty;
            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
            ProgramacaoAulaTemp.Origem = OrigemProgramacao.Manual;
            ProgramacaoAulaTemp.Status = StatusAula.Cancelada;
            if (ProgramacaoAulaTemp.OBS == null || ProgramacaoAulaTemp.OBS == string.Empty || ProgramacaoAulaTemp.OBS == ProgramacaoAulaTemp2.OBS)
            {
                ProgramacaoAulaTemp.OBS = "Cancelada aula experimental " + Constante.Now.ToString("dd/MM/yyyy HH:mm");
            }
            Resultado2.ErrMsg = ProgramacaoAulaTemp.OBS;
            Resultado2.Ok = false;
            DisableBts = true;
            StateHasChanged();


        }
        protected void CancelarPctClick()
        {
            Resultado2.Ok = true;
            Resultado2.ErrMsg = string.Empty;
            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
            ProgramacaoAulaTemp.Origem = OrigemProgramacao.Manual;
            ProgramacaoAulaTemp.Status = StatusAula.Cancelada;
            if (ProgramacaoAulaTemp.OBS == null || ProgramacaoAulaTemp.OBS == string.Empty || ProgramacaoAulaTemp.OBS == ProgramacaoAulaTemp2.OBS)
            {
                ProgramacaoAulaTemp.OBS = "Cancelada aula pacote " + Constante.Now.ToString("dd/MM/yyyy HH:mm");
            }
            Resultado2.ErrMsg = ProgramacaoAulaTemp.OBS;
            Resultado2.Ok = false;
            DisableBts = true;
            StateHasChanged();
        }

        protected void ExcluirPctClick()
        {
            Resultado2.Ok = true;
            Resultado2.ErrMsg = string.Empty;
            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
            ProgramacaoAulaTemp.Origem = OrigemProgramacao.Manual;
            ProgramacaoAulaTemp.Status = StatusAula.Excluir;
            if (ProgramacaoAulaTemp.OBS == null || ProgramacaoAulaTemp.OBS == string.Empty || ProgramacaoAulaTemp.OBS == ProgramacaoAulaTemp2.OBS)
            {
                ProgramacaoAulaTemp.OBS = "Excluir aula pacote " + Constante.Now.ToString("dd/MM/yyyy HH:mm");
            }
            Resultado2.ErrMsg = ProgramacaoAulaTemp.OBS;
            Resultado2.Ok = false;
            DisableBts = true;
            StateHasChanged();
        }


        protected void CancelarAvulsaClick()
        {
            Resultado2.Ok = true;
            Resultado2.ErrMsg = string.Empty;
            Resultado.Ok = true;
            Resultado.ErrMsg = string.Empty;
            ProgramacaoAulaTemp.Origem = OrigemProgramacao.Manual;
            ProgramacaoAulaTemp.Status = StatusAula.Cancelada;
            if (ProgramacaoAulaTemp.OBS == null || ProgramacaoAulaTemp.OBS == string.Empty || ProgramacaoAulaTemp.OBS == ProgramacaoAulaTemp2.OBS)
            {
                ProgramacaoAulaTemp.OBS = "Cancelada aula avulsa " + Constante.Now.ToString("dd/MM/yyyy HH:mm");
            }
            Resultado2.ErrMsg = ProgramacaoAulaTemp.OBS;
            Resultado2.Ok = false;
            DisableBts = true;
            StateHasChanged();
        }
    }
}
