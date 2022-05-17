using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class ChangePasswordView
    {
        [Required(ErrorMessage = "senha ogrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha atual")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "senha ogrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Nova senha")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "senha ogrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar")]
        [Compare("NewPassword", ErrorMessage =
            "A nova senha difernte da confirmação.")]
        public string ConfirmPassword { get; set; }
        public string Token { get; set; }
    }
}
