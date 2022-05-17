using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class LanctoUploadView
    {

        public LanctoLoteImportacao LanctoLoteImportacao { get; set; } = new LanctoLoteImportacao();

        public IFormFile Arquivo { get; set; }
    }
}
