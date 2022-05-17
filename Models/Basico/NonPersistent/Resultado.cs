using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{
    public class Resultado
    {
        public bool Ok { get; set; }
        public bool Grave { get; set; }
        private string _errmsg = string.Empty;
        public string ErrMsg { get { return _errmsg; } set { _errmsg = value; if (value == string.Empty) { ListErrMsg.Clear(); } ; } }
        public List<string> ListErrMsg { get; set; } = new List<string>();
        public string Detalhe { get; set; }

        public Resultado()
        {
            Ok = true;
            Grave = false;
            ErrMsg = "";
            Detalhe = "";
        }
        public void SetDefault()
        {
            Ok = true;
            ErrMsg = string.Empty;
        }
        public object Item { get; set; }
        public void SetMsg(Exception e)
        {
            Ok = false;
            ErrMsg = e.Message;
            if (e.InnerException != null)
            {
                ErrMsg = e.InnerException.Message;
            }
        }
    }
}
