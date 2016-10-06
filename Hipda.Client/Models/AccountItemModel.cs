using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Models
{
    public class AccountItemModel
    {
        public AccountItemModel(int userId, string username, string password, int questionId, string answer, bool isDefault)
        {
            this.UserId = userId;
            this.Username = username;
            this.Password = password;
            this.QuestionId = questionId;
            this.Answer = answer;
            this.IsDefault = isDefault;
        }

        public int UserId { get; private set; }

        public string Username { get; private set; }

        public string Password { get; private set; }

        public int QuestionId { get; private set; }

        public string Answer { get; private set; }

        public bool IsDefault { get; set; }
    }
}
