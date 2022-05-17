
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.RegularExpressions;
using InCorpApp.Services;

namespace InCorpApp.Models
{
    public sealed class CNPJ : ValidationAttribute, IClientModelValidator
    {

        public string[] ErrorMessageList = {
            "CPF/CNPJ inválido"
        };


        public int Tipo;
        public CNPJ()
        {

        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {



            if (CNPJValidador.TryParse(value.ToString(), out CNPJValidador cnpjx))
            {
                return ValidationResult.Success;
            }
            else
            {
                var valido = cnpjx.Valido;
                if (valido)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult ("cnpj/cpf inválido", new[] { validationContext.MemberName });
                }
            }

        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-cnpj", GetValidationClientErrorMessage(context));
            context.Attributes.Add("data-val-cnpj-field", "CNPJ");
        }

        private string GetValidationClientErrorMessage(ClientModelValidationContext context)
        {
            var str = (!string.IsNullOrEmpty(ErrorMessage)) ? ErrorMessage : ErrorMessageList[0];
            return string.Format(str, context.ModelMetadata?.GetDisplayName(), "CNPJ");
        }

 

    }
}
