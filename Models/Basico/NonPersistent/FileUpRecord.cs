using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{

    public class SFFileUpFieldHdr
    {
        public string Name { get; set; }
        public string Caption { get; set; }
    }
    public class SFFileUpRecord
    {
        public int Numero { get; set; }
        public string DescricaoErro { get; set; }
        public string[] Dados { get; set; }
        public string F1 { get => Dados[0]; }
        public string F2 { get => Dados[1]; }
        public string F3 { get => Dados[2]; }
        public string F4 { get => Dados[3]; }
        public string F5 { get => Dados[4]; }
        public string F6 { get => Dados[5]; }
        public string F7 { get => Dados[6]; }
        public string F8 { get => Dados[7]; }
        public string F9 { get => Dados[8]; }
        public string F10 { get => Dados[9]; }
        public string F11 { get => Dados[10]; }
        public string F12 { get => Dados[11]; }
        public string F13 { get => Dados[12]; }
        public string F14{ get => Dados[13]; }
        public string F15 { get => Dados[14]; }
        public string F16 { get => Dados[15]; }
        public string F17 { get => Dados[16]; }
        public string F18 { get => Dados[17]; }
        public string F19 { get => Dados[18]; }
        public string F20 { get => Dados[19]; }


        public SFFileUpRecord(string r,int num, string err, List<SFFileUpFieldHdr> h)
        {
            Numero = num;
            DescricaoErro = err;

            string[] s = r.Split(";");
            string s1 = r;
            while (s.Length < h.Count)
            {
                s1 += ";";
                s = s1.Split(";");
            }
            Dados = s1.Split(";");
        }

    }

    public class SFFileUp
    {
        public List<SFFileUpFieldHdr> Headers { get; set; } = new List<SFFileUpFieldHdr>();
        public List<SFFileUpRecord> Records { get; set; } = new List<SFFileUpRecord>();
        public SFFileUp(string cap, string columns)
        {

            string[] cl = columns.Split(";");
            string[] cp = cap.Split(";");
            if (cl.Length != cp.Length)
            {
                throw new Exception("Tamanho da lista de campos diferentes da lista de captions");
            }
            int i = -1;
            foreach (string s in cl)
            {
                i++;
                SFFileUpFieldHdr f = new ()
                {
                    Name = s,
                    Caption = cl[i]
                };
                Headers.Add(f);
            }
        }

        public void AddRecord(string d, int num, string err)
        {
            SFFileUpRecord r = new (d,num,err, Headers);
            Records.Add(r);
        }

        public void Clear()
        {
            Records.Clear();
        }
    }

}
