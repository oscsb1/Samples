using InCorpApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using InCorpApp.Constantes;

namespace InCorpApp.Models
{

    public class SFToken
    {
        [Key]
        public string GUID { get; set; }
        public string TenantId { get; set; }
        public string Token { get; set; }
        public DateTime DataIns { get; set; }
        public string Code { get; set; }
        public OrigemToken GetOrigemToken()
        {
            string[] s = Token.Split(";");
            if (s.Length > 0)
            {
                return (OrigemToken)int.Parse(s[0].ToString());
            }
            else
            {
                return OrigemToken.Invalid;
            }
        }

        public OperacaoToken GetOperacaoToken()
        {
            string[] s = Token.Split(";");
            if (s.Length > 1)
            {
                return (OperacaoToken)int.Parse(s[1].ToString());
            }
            else
            {
                return OperacaoToken.Invalid;
            }
        }

        public string GetUserIdToken()
        {
            string[] s = Token.Split(";");
            if (s.Length > 2)
            {
                return s[2];
            }
            else
            {
                return string.Empty;
            }
        }
        public string GetSecToken()
        {
            string[] s = Token.Split(";");
            if (s.Length > 3)
            {
                return s[3];
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetParmToken(int i)
        {
            string[] s = Token.Split(";");
            if (s.Length > i)
            {
                return s[i];
            }
            else
            {
                return string.Empty;
            }
        }
        public bool IsValid (int days = 3)
        
        {  return DateTime.Compare(DataIns.AddDays(days), Constante.Now) > 0; }
        public string GenerateNewCode()
        {
            Code = Constante.Now.ToString("fff").ToString() +  Constante.Now.ToString("ss").ToString() + Constante.Now.ToString("mm").ToString();
            return Code;
        }

    }
}
