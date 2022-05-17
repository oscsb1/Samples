using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class TenantUsuarioView
    {
        public Tenant Tenant { get; set; }
        public List<TenantUsuario> Usuarios { get; set; }
    }
}
