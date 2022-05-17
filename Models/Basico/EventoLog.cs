using InCorpApp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class EventoLog: BaseModel
    {

        public string UserId { get; set; }
        [DisplayName("Usuario")]
        public string UserName { get; set; }
        [DisplayName("Data Evento")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime DataHora { get; set; }
        public DateTime Data { get; set; }
        [DisplayName("Evento")]

        public string Evento { get; set; }
        [DisplayName("Descrição")]

        public string Descricao { get; set; }
        [DisplayName("Nome do Item")]
        public string Item { get; set; }
        [DisplayName("Ident do item")]
        public string ItemId { get; set; }

    }


}
