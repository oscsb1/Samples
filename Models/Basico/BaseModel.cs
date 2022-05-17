using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;


namespace InCorpApp.Models
{
    public class BaseModel
    {
        [Key]
        public int Id { get; set; }
        [AllowNull]
        public string TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }
}
