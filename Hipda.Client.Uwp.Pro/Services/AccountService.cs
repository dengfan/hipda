using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.ViewModels;
using Hipda.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;

namespace Hipda.Client.Uwp.Pro.Services
{
    public class AccountService
    {
        static string _containerKey = "HIPDA";
        static string _dataKey = "AccountData";

        HttpHandle _httpClient;
        ApplicationDataContainer _container;
        List<AccountItemModel> _accountData = new List<AccountItemModel>();

        public string LoginResultMessage { get; set; }
        public static string FormHash { get; set; }
        public static int UserId { get; set; }
        public static string Hash { get; set; }

        public AccountService()
        {
            _httpClient = HttpHandle.GetInstance();
            _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);

            if (_container.Values.ContainsKey(_dataKey))
            {
                // 从本地存储中读取已序列化的账号数据，并反序列化
                string jsonText = _container.Values[_dataKey].ToString();
                _accountData = JsonConvert.DeserializeObject<List<AccountItemModel>>(jsonText);
            }
        }

        async Task LoadHashAndUserId()
        {
            string url = "http://www.hi-pda.com/forum/post.php?action=newthread&fid=2&_=" + DateTime.Now.Ticks.ToString("x");
            var cts = new CancellationTokenSource();
            string htmlContent = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            var nodes = doc.DocumentNode.Descendants();

            // 读取发布文字信息所需要的 hash 值
            var formHashInputNode = nodes.FirstOrDefault(n => n.Name.Equals("input") && n.GetAttributeValue("id", "").Equals("formhash"));
            if (formHashInputNode != null)
            {
                FormHash = formHashInputNode.Attributes[3].Value.ToString();
            }

            // 读取 上载图片所需的 uid 和 hash 值
            var userIdNode = nodes.FirstOrDefault(n => n.Name.Equals("input") && n.GetAttributeValue("name", "").Equals("uid"));
            if (userIdNode != null)
            {
                UserId = Convert.ToInt32(userIdNode.Attributes[2].Value);
            }

            var hashNode = nodes.FirstOrDefault(n => n.Name.Equals("input") && n.GetAttributeValue("name", "").Equals("hash"));
            if (hashNode != null)
            {
                Hash = hashNode.Attributes[2].Value;
            }
        }

        public async Task<bool> AutoLogin()
        {
            var accountItem = _accountData.FirstOrDefault(a => a.IsDefault);
            if (accountItem != null)
            {
                return await LoginAndSave(accountItem.Username, accountItem.Password, accountItem.QuestionId, accountItem.Answer, false);
            }

            return false;
        }

        public async Task<bool> LoginAndSave(string username, string password, int questionId, string answer, bool isSave)
        {
            // 先清除 cookie
            _httpClient.ClearCookies();

            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("username", username));
            postData.Add(new KeyValuePair<string, object>("password", password));
            postData.Add(new KeyValuePair<string, object>("questionid", questionId));
            postData.Add(new KeyValuePair<string, object>("answer", answer));

            var cts = new CancellationTokenSource();
            string resultContent = await _httpClient.PostAsync("http://www.hi-pda.com/forum/logging.php?action=login&loginsubmit=yes&inajax=1", postData, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(resultContent);

            var root = doc.DocumentNode;
            string loginResultMessage = root.InnerText.Replace("<![CDATA[", string.Empty).Replace("]]>", string.Empty);
            LoginResultMessage = loginResultMessage;

            if (loginResultMessage.Contains("欢迎") && !loginResultMessage.Contains("错误") && !loginResultMessage.Contains("失败") && !loginResultMessage.Contains("非激活"))
            {
                if (isSave)
                {
                    _accountData.RemoveAll(a => a.Username.Equals(username));

                    foreach (var item in _accountData)
                    {
                        item.IsDefault = false;
                    }

                    _accountData.Add(new AccountItemModel(username, password, questionId, answer, true));

                    // 序列化并保存
                    string jsonStr = JsonConvert.SerializeObject(_accountData);
                    _container.Values[_dataKey] = jsonStr;
                }

                // 登录成功就获取一次 formhash/uid/hash，用于发布文本信息和上载图片
                await LoadHashAndUserId();

                return true;
            }

            return false;
        }
    }
}
