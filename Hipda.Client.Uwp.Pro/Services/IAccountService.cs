using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Services
{
    interface IAccountService
    {
        List<AccountItemModel> GetAccountList();

        Task AutoLogin();

        Task<bool> LoginAndSave(string username, string password, int questionId, string answer, bool isSave);

        Task Delete(string accountKeyName);

        Task SetDefault(string accountKeyName);

        AccountItemModel GetDefault();
    }
}
