using InCorpApp.Data;
using InCorpApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Constantes;

namespace InCorpApp.Services
{
    public class EventoLogService
    {

        private readonly ApplicationDbContext _context;

        public EventoLogService(ApplicationDbContext context)
        {
            _context = context;
        }
        public int AddEvento(EventoLog e)
        {

            e.Data = Constante.Today;
            e.DataHora = Constante.Now;

            _context.EventoLog.Add(e);
            _context.SaveChanges();

            return e.Id;

        }

    }
}
