using InCorpApp.Constantes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class SocioAporteView
    {
        public SocioAporte SocioAporte { get; set; }

        public List<SocioAporteDeposito> Depositos { get; set; } = new List<SocioAporteDeposito>();
        public string NomeSocio { get; set; }

        public string DataSolicitacaoV => SocioAporte.DataSolicitacao.ToString("dd/MM/yyyy");
        public string DataLimiteDepositoV => SocioAporte.DataLimiteDeposito.ToString("dd/MM/yyyy");
        public string ValorV => SocioAporte.Valor.ToString("C2", Constante.Culture);

        public string ValorDepositado => Depositos.Sum(x => x.Valor).ToString("C2", Constante.Culture);
        public string ValorSaldo => (SocioAporte.Valor - Depositos.Sum(x => x.Valor)).ToString("C2", Constante.Culture);
        public bool Erro { get; set; } = false;
        public string ErroMsg { get; set; } = string.Empty;

    }
}
