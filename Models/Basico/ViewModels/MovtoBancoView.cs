using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class MovtoBancoView
    {
        public int Id { get; set; }
        public int ContaCorrenteId { get; set; }
        public int PlanoContaId { get; set; }
        public int RelacionamentoId { get; set; }
        public string RelacionamentoV { get; set; }
        public string PlanoContaV { get; set; }
        public TipoMovtoBanco TipoMovtoBanco { get; set; }
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
        public DateTime DataMovto { get; set; }
        public double Valor { get; set; }
        public string Documento { get; set; }
        public string Historico { get; set; }
        public bool Exportado { get; set; }
        public string ExportadoV { get; set; }
        public string CodigoExterno { get; set; }
        public StatusConciliacao StatusConciliacao { get; set; }
        [NotMapped]
        public string StatusConciliacaoV
        {
            get
            {
                string s = string.Empty;
                switch (StatusConciliacao)
                {
                    case StatusConciliacao.Parcial:
                        s = "parcial";
                        break;
                    case StatusConciliacao.Total:
                        s = "sim";
                        break;
                }
                return s;
            }
            set { }
        }
    }
}
