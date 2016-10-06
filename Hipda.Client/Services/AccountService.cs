using Hipda.Client.Models;
using Hipda.Client.ViewModels;
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
using Windows.UI.Popups;

namespace Hipda.Client.Services
{
    public class AccountService
    {
        static HttpHandle _httpClient = HttpHandle.GetInstance();
        static ApplicationDataContainer _container = ApplicationData.Current.RoamingSettings.CreateContainer("AccountData", ApplicationDataCreateDisposition.Always);

        public static string FormHash { get; set; }
        public static int UserId { get; set; }
        public static string Username { get; set; }
        public static string Hash { get; set; }


        public async Task<bool> LoginAsync()
        {
            foreach (var item in _container.Values)
            {
                var acc = JsonConvert.DeserializeObject<AccountItemModel>(item.Value.ToString());
                if (acc != null && acc.IsDefault)
                {
                    return await LoginAsync(acc.Username, acc.Password, acc.QuestionId, acc.Answer);
                }
            }

            return false;
        }

        public async Task<bool> LoginAsync(string username, string password, int questionId, string answer)
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
            if (loginResultMessage.Contains("欢迎") && !loginResultMessage.Contains("错误") && !loginResultMessage.Contains("失败") && !loginResultMessage.Contains("非激活"))
            {
                // 登录成功就获取一次 formhash/uid/hash，用于发布文本信息和上载图片
                await LoadHashAndUserIdAsync();
                Username = username;

                // 先清除之前账号的首选状态
                ClearDefaultStatus();

                var acc = new AccountItemModel(UserId, username, password, questionId, answer, true);
                string jsonStr = JsonConvert.SerializeObject(acc);
                if (_container.Values.ContainsKey(UserId.ToString()))
                {
                    _container.Values[UserId.ToString()] = jsonStr;
                }
                else
                {
                    _container.Values.Add(UserId.ToString(), jsonStr);
                }

                return true;
            }
            else
            {
                await new MessageDialog(loginResultMessage, "登录失败").ShowAsync();
                return false;
            }
        }

        async Task LoadHashAndUserIdAsync()
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

        public static void ClearDefaultStatus()
        {
            foreach (var item in _container.Values)
            {
                var acc = JsonConvert.DeserializeObject<AccountItemModel>(item.Value.ToString());
                if (acc != null && acc.IsDefault)
                {
                    acc.IsDefault = false;
                    string jsonStr = JsonConvert.SerializeObject(acc);
                    _container.Values[acc.UserId.ToString()] = jsonStr;
                }
            }
        }
    }
}
