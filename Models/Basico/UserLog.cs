using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InCorpApp.Models
{

    public enum UserLogAction : int
    {
        LogIn = 1,
        LogOut = 2
    }

    public class UserLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime LoginDt { get; set; }
        public UserLogAction UserLogAction { get; set; }
        [NotMapped]
        public string UserName {get;set;}
        [NotMapped]
        public string Email { get; set; }
    }
}
