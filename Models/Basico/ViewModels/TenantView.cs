using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class TenantView 
    {
        public List<Tenant> Tenants { get; set; } = new List<Tenant>();
    }
}
