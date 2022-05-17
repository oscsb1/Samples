using Microsoft.AspNetCore.Identity;
using InCorpApp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InCorpApp.Models
{

    public class Perfil : IdentityRole
    {
  
        public bool Selecionado { get; set; }
        public Perfil() { }
        public Perfil(string id, string roleName) { Id = id; Name = roleName; }

    }


    public class Usuario
    {

        public string Id { get; set; }
        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "Nome")]
        public string Nome { get; set; }
        [EmailAddress(ErrorMessage = "{0} inválido")]
        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public bool Ativo { get; set; }

        public List<Perfil> Perfis { get; set; } = new List<Perfil>();
        public List<SFClaim> SFClaims { get; set; } = new List<SFClaim>();
        public Usuario() { }
    }
}
