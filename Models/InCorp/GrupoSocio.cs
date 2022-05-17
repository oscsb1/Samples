using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace InCorpApp.Models
{
    public class GrupoSocio : BaseModel
    {


        public int EmpreendimentoId { get; set; }
        public Empreendimento Empreendimento { get; set; }
        [DisplayName("Nome do Grupo")]
        public string Nome { get; set; }
        [DisplayName("Ativo")]
        public bool Ativo { get; set; }
        [DisplayName("Utilizar percentual de participação contrato")]
        public bool UtilzarPercentualContrato { get; set; }
        public bool UtilizarGerencial { get; set; }
    }










}
