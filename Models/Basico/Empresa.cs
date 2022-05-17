using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Models;

namespace InCorpApp.Models
{
    public class Empresa : BaseModel
    {


        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(80, MinimumLength = 5, ErrorMessage = "{0} tamanho deve ser entre {2} and {1}")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "Razão Social")]
        [StringLength(80, MinimumLength = 5, ErrorMessage = "{0} tamanho deve ser entre {2} and {1}")]
        public string RazaoSocial { get; set; }

        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "CNPJ")]
        [CNPJ(ErrorMessage = "CNPJ inválido")]
        public string CNPJ { get; set; }
        public int LogoFileId { get; set; }
        public string EmailRI { get; set; }

    }
}
 