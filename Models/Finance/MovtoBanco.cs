using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using InCorpApp.Constantes;
using InCorpApp.Models;

namespace InCorpApp.Models
{
    public class MovtoBanco : BaseModel
    {
        public int ContaCorrenteId { get; set; }
        public ContaCorrente ContaCorrente { get; set; }
        public string CodigoExterno { get; set; } = string.Empty;
        public int PlanoContaId { get; set; }
        public PlanoConta PlanoConta { get; set; }
        [NotMapped]
        public string PlanoContaV { get; set; } = string.Empty;
        [NotMapped]
        public string PlanoContaCodigoExternoV { get; set; } = string.Empty;
        [NotMapped]
        public int PlanoGerencialId { get; set; }
        [NotMapped]
        public string PlanoGerencialV { get; set; } = string.Empty;
        [NotMapped]
        public string PlanoGerencialCodigoExternoV { get; set; } = string.Empty;
        public TipoMovtoBanco TipoMovtoBanco { get; set; }
        [NotMapped]
        public string TipoMovtoBancoV
        {
            get
            {
                string s = string.Empty;
                switch (TipoMovtoBanco)
                {
                    case TipoMovtoBanco.Credito:
                        s = "Crédito";
                        break;
                    case TipoMovtoBanco.Debito:
                        s = "Débito";
                        break;
                }
                return s;
            }
            set
            { }
        }
        public bool Transferencia { get; set; }
        public StatusConciliacao StatusConciliacao { get; set; }
        [NotMapped]
        public string TransferenciaV
        {
            get
            {
                string s = string.Empty;
                switch (Transferencia)
                {
                    case false:
                        s = string.Empty;
                        break;
                    case true:
                        s = "sim";
                        break;
                }
                return s;
            }
            set
            { }
        }
        public int ContaCorrenteIdDestno { get; set; }
        public DateTime DataMovto { get; set; }
        [NotMapped]
        public string DataMovtoV => DataMovto.ToString("dd/MM/yyyy");
        public int RelacionamentoId { get; set; }
        public Relacionamento Relacionamento { get; set; }
        private string _nomerelacionamento = string.Empty;
        [NotMapped]
        public string RelacionamentoV { get { if (Relacionamento != null) { return Relacionamento.Nome; } else { return _nomerelacionamento; } } set { _nomerelacionamento = value; } }
        private string _relacionamentoCodigoExternoV = string.Empty;
        [NotMapped]
        public string RelacionamentoCodigoExternoV { get { if (Relacionamento != null) { return Relacionamento.Nome; } else { return _relacionamentoCodigoExternoV; } } set { _relacionamentoCodigoExternoV = value; } }

        public double Valor { get; set; }
        [NotMapped]
        public string ValorV
        {
            get
            {
                if (TipoMovtoBanco == TipoMovtoBanco.Credito)
                {
                    return Valor.ToString("C2", Constante.Culture);
                }
                else
                {
                    return (Valor * -1).ToString("C2", Constante.Culture);
                }
            }
        }
        public string Documento { get; set; }
        public string Historico { get; set; }
        public double Seq { get; set; }
        [NotMapped]
        public bool Erro { get; set; } = false;
        [NotMapped]
        public string ErroMsg { get; set; } = string.Empty;
        public static string FieldsName()
        {
            return "Tipo C-Credito/D-Debito;Data;Documento;Histórico;Valor;transferencia;Código plano conta;Plano de conta;Codigo favorecido;Favorecido;Código plano gerencial;Plano de Gerencial;Número da conta;Código externo";
        }
        public static string FieldsCaptions()
        {
            return "Tipo C-Credito/D-Debito;Data;Documento;Histórico;Valor;transferencia;Código plano conta;Plano de conta;Codigo favorecido;Favorecido;Código plano gerencial;Plano de Gerencial;Número da conta;Código externo";
        }
        public int LoteMovtoBancoId { get; set; }
        [NotMapped]
        public bool Exportado { get; set; }
        [NotMapped]
        public string ExportadoV
        {
            get
            {
                string s = string.Empty;
                switch (Exportado)
                {
                    case false:
                        s = string.Empty;
                        break;
                    case true:
                        s = "sim";
                        break;
                }
                return s;
            }
            set
            { }
        }
    }


    public class LoteMovtoBanco : BaseModel
    {
        public DateTime DataImp { get; set; }
        public string DataImpV => DataImp.ToString("dd/MM/yyyy HH:mm:ss");
        public int ContaCorrenteId { get; set; }
        public ContaCorrente ContaCorrente { get; set; }
        public string FileName {get;set;}
        public string GUID { get; set; }
        [NotMapped]
        public bool Deleted { get; set; } = false;
    }

    public class ContaCorrenteExtrato : BaseModel
    {
        public int ContaCorrenteid { get; set; }
        public ContaCorrente ContaCorrente {get;set;}
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int FileId { get; set; }
    }
    public class MovtoBancoTitulo : BaseModel
    {
        public int MovtoBancoId { get; set; }
        public MovtoBanco MovtoBanco  { get;set;}
        public int TituloParcelaId { get; set; }
        public TituloParcela TituloParcela { get; set; }
        public double Valor { get; set; }
    }
    public class MovtoBancoStudioParcela : BaseModel
    {
        public int MovtoBancoId { get; set; }
        public MovtoBanco MovtoBanco { get; set; }
        public int StudioParcelaId { get; set; }
        public OrigemStudioParcela OrigemParcela { get; set; }
        public double Valor { get; set; }
    }
    

}
