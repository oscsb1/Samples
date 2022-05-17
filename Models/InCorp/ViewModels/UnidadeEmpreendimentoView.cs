using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class UnidadeEmpreendimentoView : BaseModel
    {
        public UnidadeEmpreendimento Unidade { get; set; } = new();
        public UnidadeMovto Movto { get; set; } = new();
        public int UnidadeLoteImportacaoId { get; set; }
        public DateTime DataMovto { get; set; }
        public string StatusColor { get; set; }
        public bool Erro { get; set; } = false;
        public string ErroMsg { get; set; } = string.Empty;
        public TipoUE TipoUE => Unidade.TipoEmp;
        public string Quadra => Unidade.Quadra;
        public string Nro => Unidade.Nro;
        public string CodigoExterno => Unidade.CodigoExterno;
        public double OffsetX => Unidade.OffsetX;
        public double OffsetY => Unidade.OffsetY;
        public string Torre => Unidade.Torre;
        public string Andar => Unidade.Andar;
        public double Area => Unidade.Area;
        public StatusUnidade StatusUnidade => Movto.StatusUnidade;
        public string StatusUnidadeV => Movto.StatusUnidadeV;
        public StatusFinanceiroUnidade StatusFinanceiroUnidade => Movto.StatusFinanceiroUnidade;
        public string StatusFinanceiroUnidadeV => Movto.StatusFinanceiroUnidadeV;
        public double ValorAtualVenda => Movto.ValorAtualVenda;
        public double ValorAtualAluguel  => Movto.ValorAtualAluguel;
        public DateTime DataContrato  => Movto.DataContrato;
        public double ValorContrato  => Movto.ValorContrato;
        public DateTime DataDistrato  => Movto.DataDistrato;
        public double ValorReembolso  => Movto.ValorReembolso;
        public double ValorAte30  => Movto.ValorAte30;
        public double ValorAte60  => Movto.ValorAte60;
        public double ValorAte90  => Movto.ValorAte90;
        public double ValorAte120  => Movto.ValorAte120;
        public double ValorApos120  => Movto.ValorApos120;

        public static string FieldsName()
        {
            return "Tipo;Código integração;Andar;Número;Torre;Quadra;Data referencia;Situação;Situação financeira;Valor atual venda;Valor atual aluguel;Data do contrato;Valor do contrato;Data distrato;Valor do reembolso;atraso ate 30;atraso ate 60;atraso ate 90;atraso ate 120;atraso acima 120";
        }
        public static string FieldsCaptions()
        {
            return "Tipo;Código integração;Andar;Número;Torre;Quadra;Data referencia;Situação;Situação financeira;Valor atual venda;Valor atual aluguel;Data do contrato;Valor do contrato;Data distrato;Valor do reembolso;atraso ate 30;atraso ate 60;atraso ate 90;atraso ate 120;atraso acima 120";
        }
    }
}
