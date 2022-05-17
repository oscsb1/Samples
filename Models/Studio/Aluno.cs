using InCorpApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace InCorpApp.Models
{
    public class Aluno:BaseModel
    {
        public int RelacionamentoId { get; set; }
        public Relacionamento Relacionamento { get; set; }
        public int RelacionamentoFaturaId { get; set; }
        public Relacionamento RelacionamentoFatura { get; set; }
        [NotMapped]
        public string Nome { get => Relacionamento.Nome; set {; } }
        [NotMapped]
        public string Email { get => Relacionamento.Email; set {; } }
        [NotMapped]
        public string Telefone { get => Relacionamento.Telefone; }
        [NotMapped]
        public DateTime DataNascimento { get => Relacionamento.DataNascimento; }
        [NotMapped]
        public string MesAniver { get => Relacionamento.DataNascimento.ToString("MM"); }
        [NotMapped]
        public List<AlunoPlano> Planos { get; set; }
        [NotMapped]
        public List<AlunoAulaAgenda> Agendas { get; set; }
        public static string FieldsName()
        {
            return "Nome;CPF;Telefone;Email;Data Nascimento";
        }
        public static string FieldsCaptions()
        {
            return "Nome;CPF;Telefone;Email;Data Nascimento";
        }
    }
    public class AlunoAvaliacao:BaseModel
    {
        public DateTime DataAvaliacao { get; set; } 
        public int ProfessorId { get; set; }
        public string Avaliacao { get; set; }
    }
    public class AlunoAvaliacaoDocs:BaseModel
    {
        public int AlunoAvaliacaoId { get; set; }
        public string DocName { get; set; }
        public int FileId { get; set; }
    }
    public class AlunoProntuario :BaseModel
    {
        public int AlunoId { get; set; }
        public Aluno Aluno { get; set; }
        public DateTime DataRegistro { get; set; }
        public string Notas { get; set; }
        public int FiledId { get; set; }
        public int ProfessorId { get; set; }
        [NotMapped]
        public string NomeAluno { get; set; }
        [NotMapped]
        public string NomeProfessor { get; set; }

    }
    public class AlunoProntuarioAula : BaseModel
    {
        public int AlunoProntuarioId { get; set; }
        public AlunoProntuario AlunoProntuario { get; set; }
        public int ProgramacaoAulaId { get; set; }
        public ProgramacaoAula ProgramacaoAula {get;set;}
    }
}
