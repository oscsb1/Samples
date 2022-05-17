using InCorpApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class Arquivo : BaseModel
    {
        public byte[] File { get; set; }
        public int IdOrigem { get; set; }
        public OrigemAnexo OrigemAnexo { get; set; }
        public string FileNome {get;set;}
        public string Titulo { get; set; }
        public DateTime DataInclusao { get; set; }
        public TipoSaveMode SaveMode { get; set; } = TipoSaveMode.OnlyDB;
    }

    public class ArquivoObjeto : BaseModel
    {
        public OrigemAnexo OrigemAnexo { get; set; }
        public int ObjId { get; set; }
        public int ArquivoId { get; set; }
    }
}
