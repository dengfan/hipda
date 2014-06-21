using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace hipda.Data
{
    public class AccountHelper
    {
        private static string accountDataKeyName = "accountData";
        private static string defaultAccountKeyName = "defaultAccount";
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private static AccountHelper _accountHelper = new AccountHelper();

        private ObservableCollection<Account> _list = new ObservableCollection<Account>();
        public static ObservableCollection<Account> List
        {
            get
            {
                return _accountHelper._list;
            }
        }

        public AccountHelper()
        {
            if (!localSettings.Containers.ContainsKey(accountDataKeyName)) return;

            var accountDataContainer = localSettings.Containers[accountDataKeyName];
            if (!accountDataContainer.Values.ContainsKey(defaultAccountKeyName)) return;

            string defaultName = accountDataContainer.Values[defaultAccountKeyName].ToString();
            var items = accountDataContainer.Values.Where(v => v.Key != defaultAccountKeyName);
            foreach (var item in items)
            {
                var accountData = (ApplicationDataCompositeValue)item.Value;
                if (accountData != null && accountData.ContainsKey("username") && accountData.ContainsKey("password"))
                {
                    string key = item.Key;
                    string username = accountData["username"].ToString();
                    string password = accountData["password"].ToString();
                    bool isDefault = key.Equals(defaultName);
                    _list.Add(new Account(key, username, password, isDefault));
                }
            }
        }

        private static HttpHandle httpClient = HttpHandle.getInstance();

        public static async Task AutoLogin()
        {
            ApplicationDataContainer container = localSettings.CreateContainer(accountDataKeyName, ApplicationDataCreateDisposition.Always);
            var accountDataContainer = localSettings.Containers[accountDataKeyName];
            if (accountDataContainer.Values.ContainsKey(defaultAccountKeyName))
            {
                string name = accountDataContainer.Values[defaultAccountKeyName].ToString();

                var accountData = (ApplicationDataCompositeValue)accountDataContainer.Values[name];
                if (accountData != null && accountData.ContainsKey("username") && accountData.ContainsKey("password"))
                {
                    string username = accountData["username"].ToString();
                    string password = accountData["password"].ToString();

                    await LoginAndAdd(username, password, false);
                }
            }
        }

        public static async Task<bool> LoginAndAdd(string username, string password, bool isSave)
        {
            var postData = new Dictionary<string, object>();
            postData.Add("username", username);
            postData.Add("password", password);

            string resultContent = await httpClient.HttpPost("http://www.hi-pda.com/forum/logging.php?action=login&loginsubmit=yes&inajax=1", postData);
            if (resultContent.Contains("欢迎") && !resultContent.Contains("错误") && !resultContent.Contains("失败") && !resultContent.Contains("非激活"))
            {
                if (isSave)
                {
                    // 保存到本地
                    var accountData = new ApplicationDataCompositeValue();
                    accountData["username"] = username;
                    accountData["password"] = password;

                    ApplicationDataContainer container = localSettings.CreateContainer(accountDataKeyName, ApplicationDataCreateDisposition.Always);
                    var accountDataContainer = localSettings.Containers[accountDataKeyName];

                    string key = string.Format("user_{0:yyyyMMddHHmmss}", DateTime.Now);
                    accountDataContainer.Values[key] = accountData;
                    accountDataContainer.Values[defaultAccountKeyName] = key;

                    foreach (var item in _accountHelper._list)
                    {
                        item.IsDefault = false;
                    }

                    _accountHelper._list.Add(new Account(key, username, password, true));
                }

                return true;
            }

            return false;
        }

        public static async Task Delete(string accountKeyName)
        {
            var accountDataContainer = localSettings.Containers[accountDataKeyName];
            accountDataContainer.Values.Remove(accountKeyName);
            _accountHelper._list.Remove(_accountHelper._list.Single(a => a.Key == accountKeyName));

            // 删除后，取另一个为登录账号
            var items = accountDataContainer.Values.Where(v => v.Key != defaultAccountKeyName);
            foreach (var item in items)
            {
                var accountData = (ApplicationDataCompositeValue)item.Value;
                if (accountData != null && accountData.ContainsKey("username") && accountData.ContainsKey("password"))
                {
                    string key = item.Key;
                    accountDataContainer.Values[defaultAccountKeyName] = key;

                    await SetDefault(key);
                    break;
                }
            }
        }

        public static async Task SetDefault(string accountKeyName)
        {
            // 切换到此账号登录，并设置为默认账号
            var accountDataContainer = localSettings.Containers[accountDataKeyName];
            var accountData = (ApplicationDataCompositeValue)accountDataContainer.Values[accountKeyName];
            if (accountData != null && accountData.ContainsKey("username") && accountData.ContainsKey("password"))
            {
                string username = accountData["username"].ToString();
                string password = accountData["password"].ToString();

                await LoginAndAdd(username, password, false);
            }

            foreach (var item in _accountHelper._list)
            {
                if (item.Key.Equals(accountKeyName))
                {
                    item.IsDefault = true;
                }
                else
                {
                    item.IsDefault = false;
                }
            }
        }

        public static Account GetDefault()
        {
            return _accountHelper._list.SingleOrDefault(a => a.IsDefault);
        }
    }

    public class Account : INotifyPropertyChanged
    {
        public Account(string key, string username, string password, bool isDefault)
        {
            this.Key = key;
            this.Username = username;
            this.Password = password;
            this.IsDefault = isDefault;
        }

        public string Key { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        private bool isDefault;

        public bool IsDefault
        {
            get
            {
                return this.isDefault;
            }
            set
            {
                if (this.isDefault != value)
                {
                    this.isDefault = value;
                    OnPropertyChanged("IsDefault");
                    OnPropertyChanged("Label");
                }
            }
        }

        public string Label
        {
            get
            {
                return this.IsDefault ? " ● " : string.Empty;
            }
        }

        

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
