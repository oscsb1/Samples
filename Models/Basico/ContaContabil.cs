using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{




    public class ContaContabil : BaseModel
    {

        public int Seq { get; set; }
        [Display(Name = "Codigo")]
        public string Codigo { get; set; }
        public string CodigoNormalizado { get; set; }
        [Display(Name = "Codigo resumido")]
        public string Resumido { get; set; }
        [Required(ErrorMessage = "{0} obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(60, MinimumLength = 1, ErrorMessage = "{0} tamanho deve ser entre {2} and {1}")]
        public string Nome { get; set; }
        [Display(Name = "Codigo para integraçao")]
        public string CodigoExterno { get; set; }
        public int Tipo { get; set; }
        public bool Analitica { get; set; }
        public int Nivel { get; set; }


        public static string FieldsName()
        {
            return "Código;Nome;Código resumido;Código integração";
        }
        public static string FieldsCaptions()
        {
            return "Código;Nome;Código resumido;Código integração";
        }


    }
}
