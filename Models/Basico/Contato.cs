using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class Contato :BaseModel
    {
        public int RelacionamentoId { get; set; }
        public Relacionamento Relacionamento { get; set; }
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
    }
}
