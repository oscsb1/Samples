using System;


namespace InCorpApp
{
    public enum SFMSGTipoFormato : int
    {
        Default = 1,
        Oracle = 2
    }

    public enum SFMSGTipoDB : int
    {
        SQL = 1,
        Oracle = 2
    }

    public enum SFMSGOperacao : int
    {
        ValidaToken = 1,
        ImportarCC = 2,
        ImportarLancto = 3,
        UpdateStatus = 4
    }

    public enum SFMSGStatus : int
    {
        Criado = 0,
        Iniciado = 1,
        Finalizado = 2,
        Erro = 3
    }

    public class SFMsgBase
    {
        public int version { get; set; } = 10;
        public string token { get; set; }
        public SFMSGOperacao operacao { get; set; }
        public string urlretorno { get; set; }
        public string urllocal { get; set; }
        public bool erro { get; set; } = false;
        public string erromsg { get; set; } = string.Empty;
        public SFMSGStatus status { get; set; }
        public SFMSGTipoDB dbType { get; set; }
        public string dbstringcon { get; set; } = string.Empty;
        public SFMSGTipoFormato format { get; set; }
        public string empreendimento { get; set; } = string.Empty;
        public string contacorrente { get; set; } = string.Empty;
        public DateTime datainicio { get; set; }
        public DateTime datafinal { get; set; }
        public DateTime dataultalt { get; set; }
        public int totalregistro { get; set; }
        public int registroatual { get; set; }
        public string lancto { get; set; } = string.Empty;
    }
}