using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace InCorpApp.Models
{
    public class SocioUsuario
    {
        public int Id { get; set; }

        public Socio Socio { get; set; }
        public int SocioId { get; set; }
        public IdentityUser IdentityUser { get; set; }
        public string IdentityUserId { get; set; }
    }
}

