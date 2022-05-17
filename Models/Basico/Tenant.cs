using System;
using System.ComponentModel.DataAnnotations;
using InCorpApp.Models;

namespace InCorpApp.Models
{
    public class Tenant
    {
        [Key]
        public string Id { get; set; }

        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "Nome")]
        public string Nome { get; set; }


        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "Email do adminstrador")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string EmailAdmin { get; set; }

        public TenantApp TenantApp { get; set; }
        public Tenant()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string URL { get; set; }
        public string EmailSuporte { get; set; } = string.Empty;
        public string PswEmailSuporte { get; set; } = string.Empty;

    }


    public class TenantUsuario
    {
        
        public int Id { get; set; }

        public string UserId { get; set; }
        public string TenantId { get; set; }

    }
}
