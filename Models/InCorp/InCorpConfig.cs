using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class InCorpConfig :BaseModel
    {
        public int PlanoContaReceitaIdPadrao { get; set; }
        public int PlanoContaDespesaIdPadrao { get; set; }
        public bool CriarPlanoContaImportacao { get; set; } = true;
        public bool CriarRelacionamentoImportacao { get; set; } = true;
        public SFMSGTipoDB SFMSGTipoDB { get; set; }
        public SFMSGTipoFormato SFMSGTipoFormato { get; set; }
        public string DBStringCon { get; set; }
        public string UrlLocal { get; set; }
        public string StatusUnidadeNone { get; set; }
        public string StatusUnidadeReservado { get; set; }
        public string StatusUnidadeBloqueado { get; set; }
        public string StatusUnidadeAlugado { get; set; }
        public string StatusUnidadeDisponivel { get; set; }
        public string StatusUnidadeEmDistrato { get; set; }
        public string StatusUnidadeVendida { get; set; }
        public string StatusFinanceiroUnidadeNA { get; set; }
        public string StatusFinanceiroUnidadeAdimplente { get; set; }
        public string StatusFinanceiroUnidadeInadimplente { get; set; }
        public string StatusFinanceiroUnidadeAtraso { get; set; }
        public string Cor30 { get; set; }
        public string Cor60 { get; set; }
        public string Cor90 { get; set; }
        public string Cor120 { get; set; }
        public string CorApos120 { get; set; }
        public OrigemImnportacaoLanctoEmp OrigemImnportacaoLanctoEmp { get; set; }

        public bool CriarPlanoContaPorContaCorrente { get; set; }
        public int RelacionamentoDefaultId { get; set; }
        public int PlanoContaDefaultDespesaId { get; set; }
        public int PlanoContaDefaultReceitaId { get; set; }

    }
}
