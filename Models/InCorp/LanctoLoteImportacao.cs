using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InCorpApp.Models;

namespace InCorpApp.Models
{

    public class LanctoLoteImportacao : BaseModel
    {

        public int PeriodoId { get; set;}
        public Periodo Periodo { get; set; }
        [Display(Name = "Identificação")]
        public string IdExterno { get; set; }
        public DateTime DataProcessamento { get; set; }
        public DateTime DataImportacao { get; set; }
        [Display(Name = "Situação")]
        public StatusLote Status { get; set; }

        [NotMapped]
        public string NomeStatus
        {
            get
            {
                string s = string.Empty;
                switch (Status)
                {
                    case StatusLote.Erro:
                        s = "Lançamento(s) com erro(s)";
                        break;
                    case StatusLote.Pendente:
                        s = "Pendente de processamento";
                        break;
                    case StatusLote.Processado:
                        s = "Processado";
                        break;
                }
                return s;
            }

            set { }
            
        }
        public string Origem { get; set; }
        [NotMapped]
        public string DataImportacaoV
        {
            get
            {
                if (DataImportacao <= DateTime.MinValue) { return ""; } else { return DataImportacao.ToString("dd/MM/yyyy HH:mm:ss"); };
            }
            set { }
        }
        [NotMapped]
        public string DataProcessamentoV
        {
            get
            {
                if (DataProcessamento <= DateTime.MinValue) { return ""; } else { return DataProcessamento.ToString("dd/MM/yyyy HH:mm:ss"); };
            }
            set { }
        }

        public string MasterLoteGuid { get; set; }
    }
}
