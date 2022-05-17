using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class ContaContabilView
    {
        public ContaContabil Pai { get; set; }
        public ContaContabil Filha { get; set; } = new ContaContabil();
    }
}
