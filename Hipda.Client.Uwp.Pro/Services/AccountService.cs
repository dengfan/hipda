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
        private static string _containerKey = "HIPDA";
        private static string _dataKey = "AccountData";

        private HttpHandle _httpClient;
        private ApplicationDataContainer _container;
        private List<AccountItemModel> _accountData = new List<AccountItemModel>();

        public string LoginResultMessage { get; private set; }
        public string FormHash { get; private set; }
        public string UserId { get; private set; }
        public string Hash { get; private set; }

        public AccountService()
        {
            _httpClient = HttpHandle.getInstance();
            _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);

            if (_container.Values.ContainsKey(_dataKey))
            {
                // 从本地存储中读取已序列化的账号数据，并反序列化
                string jsonText = _container.Values[_dataKey].ToString();
                _accountData = JsonConvert.DeserializeObject<List<AccountItemModel>>(jsonText);
            }
        }

        private async Task LoadHashAndUserId()
        {
            string url = "http://www.hi-pda.com/forum/post.php?action=newthread&fid=2&_=" + DateTime.Now.Ticks.ToString("x");
            var cts = new CancellationTokenSource();
            string htmlContent = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            // 读取发布文字信息所需要的 hash 值
            var postNode = doc.DocumentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").Equals("content editorcontent"));
            if (postNode != null)
            {
                var formHashInputNode = postNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("name", "").Equals("formhash"));
                if (formHashInputNode != null)
                {
                    this.FormHash = formHashInputNode.Attributes[3].Value.ToString();
                }
            }

            // 读取 上载图片所需的 uid 和 hash 值
            var imgAttachNode = doc.DocumentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("id", "").Equals("imgattachbtnhidden"));
            if (imgAttachNode != null)
            {
                var userIdNode = imgAttachNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("name", "").Equals("uid"));
                if (userIdNode != null)
                {
                    this.UserId = userIdNode.Attributes[2].Value;
                }

                var hashNode = imgAttachNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("name", "").Equals("hash"));
                if (hashNode != null)
                {
                    this.Hash = hashNode.Attributes[2].Value;
                }
            }
        }

        public async Task AutoLogin()
        {
            var accountItem = _accountData.FirstOrDefault(a => a.IsDefault);
            if (accountItem != null)
            {
                await LoginAndSave(accountItem.Username, accountItem.Password, accountItem.QuestionId, accountItem.Answer, false);
            }
        }

        public async Task<bool> LoginAndSave(string username, string password, int questionId, string answer, bool isSave)
        {
            // 先清除 cookie
            _httpClient.ClearCookies();

            var postData = new Dictionary<string, object>();
            postData.Add("username", username);
            postData.Add("password", password);
            postData.Add("questionid", questionId);
            postData.Add("answer", answer);

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
                    string key = string.Format("user_{0:yyyyMMddHHmmss}", DateTime.Now);
                    var accountItem = new AccountItemModel(key, username, password, questionId, answer, true);

                    _accountData.RemoveAll(a => a.Username.Equals(username));
                    foreach (var item in _accountData)
                    {
                        item.IsDefault = false;
                    }

                    _accountData.Add(accountItem);

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
