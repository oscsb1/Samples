using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Services.Exceptions
{
 
    public class NaoPodeExcluirException : ApplicationException
    {
        public NaoPodeExcluirException(string message) : base(message)
        {
        }
    }

}
