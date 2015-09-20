using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class AccountItemModel
    {
        public AccountItemModel(string key, string username, string password, int questionId, string answer, bool isDefault)
        {
            this.Key = key;
            this.Username = username;
            this.Password = password;
            this.QuestionId = questionId;
            this.Answer = answer;
            this.IsDefault = IsDefault;
        }

        public string Key { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public int QuestionId { get; set; }

        public string Answer { get; set; }

        public bool IsDefault { get; set; }
    }
}
