

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCorpApp.Models
{
    public class ProfessorAgenda : BaseModel
    {
        public int ProfessorId { get; set; }
        public Professor Professor { get; set; }
        public int StudioId { get; set; }
        public Studio Studio { get; set; }
        public int AgendaId { get; set; }
        public Agenda Agenda { get; set; }
    }
}
