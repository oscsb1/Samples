using InCorpApp.Constantes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class Titulo : BaseModel
    {
        public TituloOrigem Origem { get; set; }
        public int CompanyId { get; set; }
        [NotMapped]
        public string CompanyName { get; set; }
        public TipoTitulo TipoTitulo { get; set; }
        public int RelacionamentoId { get; set; }
        public Relacionamento Relacionamento { get; set; }
        public int PlanoContaId { get; set; }
        public PlanoConta PlanoConta { get; set; }
        public string Documento { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataInclusao { get; set; } = Constante.Today;
        [NotMapped]
        public List<TituloParcela> Parcelas { get; set; } = new();
        [NotMapped]
        public string PlanoContaV { get; set; }
        [NotMapped]
        public TituloFrequencia Frequencia { get; set; } = TituloFrequencia.Unico;
        [NotMapped]
        public DateTime GerarParcelasAte { get; set; }
    }
    public class TituloParcela : BaseModel
    {
        public int TituloId { get; set; }
        public Titulo Titulo { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string CodigoBarras { get; set; } = string.Empty;
        public bool ValorEstimado { get; set; } = false;
        public double Valor { get; set; }
        public double ValorPago { get; set; }
        public DateTime DataInclusao { get; set; } = Constante.Today;
        public DateTime DataVencto { get; set; }
        public DateTime DataPagto { get; set; }
        public StatusParcela Status { get; set; } = StatusParcela.Aberto;
        [NotMapped]
        public string StatusV
        {
            get
            {
                return Constante.GetNomeStatusParcela(Status);
            }
            set { }
        }
        [NotMapped]
        public List<TituloParcelaMovto> Movtos { get; set; } = new();
        [NotMapped]
        public List<TituloParcelaConciliacaoView> Conciliacoes { get; set; } = new();
    }
    public class TituloParcelaMovto : BaseModel
    {
        public int TituloParcelaId { get; set; }
        public TituloParcela TituloParcela { get; set; }
        public TipoMovtoParcela TipoMovtoParcela { get; set; }
        public DateTime DataMovto { get; set; }
        public double Valor { get; set; }
        public string Historico { get; set; }
    }

    public class TituloView
    {
        public Titulo Titulo { get; set; }
        public TituloParcela TituloParcela { get; set; }
        public int RelacionamentoId
        {
            get
            { return Titulo.RelacionamentoId; }
            set
            { Titulo.RelacionamentoId = value; }
        }
        public string RelacionamentoV { get; set; }
        public int PlanoContaId
        {
            get
            { return Titulo.PlanoContaId; }
            set
            { Titulo.PlanoContaId = value; }
        }
        public string PlanoContaV { get; set; }
        public string Documento
        {
            get
            { return Titulo.Documento; }
            set
            { Titulo.Documento = value; }
        }
        public string CompanyName
        {
            get
            { return Titulo.CompanyName; }
            set
            { Titulo.CompanyName = value; }
        }
        public string Descricao
        {
            get
            { return Titulo.Descricao; }
            set
            { Titulo.Descricao = value; }
        }
        public double Valor
        {
            get
            { return TituloParcela.Valor; }
            set
            { TituloParcela.Valor = value; }
        }
        public double ValorPago
        {
            get
            { return TituloParcela.ValorPago; }
            set
            { TituloParcela.ValorPago = value; }
        }
        public DateTime DataVencto
        {
            get
            { return TituloParcela.DataVencto; }
            set
            { TituloParcela.DataVencto = value; }
        }
        public DateTime DataPagto
        {
            get
            { return TituloParcela.DataPagto; }
            set
            { TituloParcela.DataPagto = value; }
        }
        public StatusParcela Status
        {
            get
            { return TituloParcela.Status; }
            set
            { TituloParcela.Status = value; }
        }
        public string StatusV
        {
            get
            { 
                string s = string.Empty;
                switch (TituloParcela.Status)
                {
                    case StatusParcela.Aberto:
                        s = "Aberto";
                        break;
                    case StatusParcela.Pago:
                        s = "Pago";
                        break;
                    case StatusParcela.Conciliado:
                        s = "Conciliado";
                        break;
                    case StatusParcela.Inadimplente:
                        s = "Inadimplente";
                        break;
                    case StatusParcela.ConciliadoParcial:
                        s = "Conciliado parcial";
                        break;
                }
                return s;
            }
set
{ }
        }
        public int ParcelaId
        {
            get
            { return TituloParcela.Id; }
            set
            { ; }
        }
        public string Numero
        {
            get
            { return TituloParcela.Numero; }
            set
            {; }
        }
        public string CodigoBarras
        {
            get
            { return TituloParcela.CodigoBarras; }
            set
            {; }
        }
    }

    public class TituloParcelaConciliacaoView : BaseModel
    {
        public int TituloParcelaId { get; set; }
        public int MovtoBancoId { get; set; }
        public int ContaCorrenteId { get; set; }
        public string ContaCorrenteNome { get; set; }
        public DateTime DataMovto { get; set; }
        public double Valor { get; set; }
    }
}
