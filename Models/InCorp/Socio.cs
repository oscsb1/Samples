using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{


    public class Socio
    {

        [Key]
        public int Id { get; set; }

        /*
                [Required(ErrorMessage = "{0} obrigatório")]
                [Display(Name = "Nome")]
                [StringLength(80, MinimumLength = 2, ErrorMessage = "{0} tamanho deve ser entre {2} and {1}")]
                public string Nome { get; set; }


                [Display(Name = "Razão Social")]
                [StringLength(80, MinimumLength = 2, ErrorMessage = "{0} tamanho deve ser entre {2} and {1}")]
                public string RazaoSocial { get; set; }

                [Display(Name = "Tipo")]
                [Required(ErrorMessage = "{0} obrigatório")]
                public int Tipo { get; set; } = 1;

                [Required(ErrorMessage = "{0} obrigatório")]
                [Display(Name = "CPF/CNPJ")]
                [CNPJ(ErrorMessage = "CPF/CNPJ inválido")]
                public string CPFCNPJ { get; set; }
        */

        [EmailAddress(ErrorMessage = "{0} inválido")]
        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }


}
