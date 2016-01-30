using Hipda.Http;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Hipda.BackgroundTask
{
    public sealed class HandleToastActionTask : IBackgroundTask
    {
        HttpHandle _httpClient = HttpHandle.GetInstance();
        string _formHash = string.Empty;
        int _userId = 0;
        string _hash = string.Empty;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            try
            {
                var cts = new CancellationTokenSource();
                await LoginAsync(cts); // 先登录
                if (cts.IsCancellationRequested) return;
                await GetFormHashAsync(cts); // 再获取formhash

                if (taskInstance.TriggerDetails is ToastNotificationActionTriggerDetail)
                {
                    var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
                    var args = details.Argument;
                    if (args.StartsWith("reply_post="))
                    {
                        await HandleReplyPost(details, args, cts);
                    }
                    else if (args.StartsWith("reply_pm="))
                    {
                        await HandleReplyPm(details, args, cts);
                    }
                    else if (args.StartsWith("add_buddy="))
                    {
                        await HandleAddBuddy(details, args, cts);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("出错了：" + ex.Message);
            }
            finally
            {
                deferral.Complete();
            }
        }

        async Task HandleReplyPost(ToastNotificationActionTriggerDetail details, string args, CancellationTokenSource cts)
        {
            string[] tary = args.Replace("__AND__", "&").Substring("reply_post=".Length).Split(',');
            int threadId = Convert.ToInt32(tary[0]);
            string noticeauthor = tary[1];
            string noticetrimstr = tary[2];
            string noticeauthormsg = tary[3];
            string replyContent = details.UserInput["inputPost"].ToString();

            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", _formHash));
            postData.Add(new KeyValuePair<string, object>("wysiwyg", "1"));
            postData.Add(new KeyValuePair<string, object>("noticeauthor", noticeauthor));
            postData.Add(new KeyValuePair<string, object>("noticetrimstr", noticetrimstr));
            postData.Add(new KeyValuePair<string, object>("noticeauthormsg", noticeauthormsg));
            postData.Add(new KeyValuePair<string, object>("subject", string.Empty));
            postData.Add(new KeyValuePair<string, object>("message", $"{noticetrimstr}{replyContent}\r\n \r\n[img=16,16]http://www.hi-pda.com/forum/attachments/day_140621/1406211752793e731a4fec8f7b.png[/img]"));

            string url = string.Format("http://www.hi-pda.com/forum/post.php?action=reply&tid={0}&replysubmit=yes&handlekey=fastpost&inajax=1", threadId);
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            var flag = resultContent.Contains("您的回复已经发布");
            if (cts.IsCancellationRequested || flag == false)
            {
                string simpleContent = replyContent.Length > 10 ? replyContent.Substring(0, 9) + "..." : replyContent;
                string _xml = "<toast>" +
                                "<visual>" +
                                    "<binding template='ToastGeneric'>" +
                                        "<text>对不起，您两次发表间隔少于 30 秒，请不要灌水！</text>" +
                                        $"<text>“ {simpleContent} ” 回复不成功！</text>" +
                                    "</binding>" +
                                "</visual>" +
                                "</toast>";
                SendToast(_xml);
            }
            Debug.WriteLine("贴子回复结果：" + flag);
        }

        async Task HandleReplyPm(ToastNotificationActionTriggerDetail details, string args, CancellationTokenSource cts)
        {
            int userId = Convert.ToInt32(args.Substring("reply_pm=".Length));
            string replyContent = details.UserInput["inputPm"].ToString();
            if (string.IsNullOrEmpty(replyContent))
            {
                string _xml = "<toast>" +
                                "<visual>" +
                                    "<binding template='ToastGeneric'>" +
                                        "<text>对不起</text>" +
                                        "<text>您输入的内容有误，消息发送不成功！</text>" +
                                    "</binding>" +
                                "</visual>" +
                                "</toast>";
                SendToast(_xml);
            }

            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", _formHash));
            postData.Add(new KeyValuePair<string, object>("handlekey", "pmreply"));
            postData.Add(new KeyValuePair<string, object>("lastdaterange", DateTime.Now.ToString("yyyy-M-d")));
            postData.Add(new KeyValuePair<string, object>("message", replyContent));

            string url = string.Format("http://www.hi-pda.com/forum/pm.php?action=send&uid={0}&pmsubmit=yes&infloat=yes&inajax=1&_={1}", userId, DateTime.Now.Ticks.ToString("x"));
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            bool flag = resultContent.StartsWith(@"<?xml version=""1.0"" encoding=""gbk""?><root><![CDATA[<li id=""pm_") && resultContent.Contains(@"images/default/notice_newpm.gif");
            if (cts.IsCancellationRequested || flag == false)
            {
                string simpleContent = replyContent.Length > 10 ? replyContent.Substring(0, 9) + "..." : replyContent;
                string _xml = "<toast>" +
                                "<visual>" +
                                    "<binding template='ToastGeneric'>" +
                                        "<text>对不起</text>" +
                                        $"<text>“ {simpleContent} ” 发送不成功！</text>" +
                                    "</binding>" +
                                "</visual>" +
                                "</toast>";
                SendToast(_xml);
            }
            Debug.WriteLine("短消息回复结果：" + flag);
        }

        async Task HandleAddBuddy(ToastNotificationActionTriggerDetail details, string args, CancellationTokenSource cts)
        {
            string[] tary = args.Substring("add_buddy=".Length).Split(',');
            int userId = Convert.ToInt32(tary[0]);
            string username = tary[1];

            string url = string.Format("http://www.hi-pda.com/forum/my.php?from=notice&item=buddylist&newbuddyid={0}&buddysubmit=yes&inajax=1&_={1}", userId, DateTime.Now.Ticks.ToString("x"));
            await _httpClient.GetAsync(url, cts);

            string _xml = "<toast>" +
                                "<visual>" +
                                    "<binding template='ToastGeneric'>" +
                                        "<text>恭喜您</text>" +
                                        $"<text>成功添加 “{username}” 为好友！</text>" +
                                    "</binding>" +
                                "</visual>" +
                                "</toast>";
            SendToast(_xml);
        }

        void SendToast(string toastXml)
        {
            toastXml = ReplaceHexadecimalSymbols(toastXml);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(toastXml);

            // 创建通知实例
            var notification = new ToastNotification(xmlDoc);

            // 单击响应
            notification.Activated += OnNotification;

            // 显示通知
            var tn = ToastNotificationManager.CreateToastNotifier();
            tn.Show(notification);
        }

        private void OnNotification(ToastNotification sender, object args)
        {
            Debug.WriteLine("[Toast] in OnNotification");
        }

        async Task LoginAsync(CancellationTokenSource cts)
        {
            // 先从 settings 中读取是否有账号
            string _containerKey = "HIPDA";
            string _dataKey = "AccountData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);
            var data = _container.Values[_dataKey];
            if (data == null)
            {
                return;
            }

            string jsonText = data.ToString();
            var accountData = JsonConvert.DeserializeObject<List<AccountItemModel>>(jsonText);
            var defaultAccount = accountData.FirstOrDefault(a => a.IsDefault);
            if (defaultAccount == null)
            {
                return;
            }

            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("username", defaultAccount.Username));
            postData.Add(new KeyValuePair<string, object>("password", defaultAccount.Password));
            postData.Add(new KeyValuePair<string, object>("questionid", defaultAccount.QuestionId));
            postData.Add(new KeyValuePair<string, object>("answer", defaultAccount.Answer));

            string loginResultMessage = await _httpClient.PostAsync("http://www.hi-pda.com/forum/logging.php?action=login&loginsubmit=yes&inajax=1", postData, cts);
            Debug.WriteLine("登录结果：" + (loginResultMessage.Contains("欢迎") && !loginResultMessage.Contains("错误") && !loginResultMessage.Contains("失败") && !loginResultMessage.Contains("非激活")));
        }

        async Task GetFormHashAsync(CancellationTokenSource cts)
        {
            string url = "http://www.hi-pda.com/forum/post.php?action=newthread&fid=2&_=" + DateTime.Now.Ticks.ToString("x");
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
                _formHash = formHashInputNode.Attributes[3].Value.ToString();
            }

            // 读取 上载图片所需的 uid 和 hash 值
            var userIdNode = nodes.FirstOrDefault(n => n.Name.Equals("input") && n.GetAttributeValue("name", "").Equals("uid"));
            if (userIdNode != null)
            {
                _userId = Convert.ToInt32(userIdNode.Attributes[2].Value);
            }

            var hashNode = nodes.FirstOrDefault(n => n.Name.Equals("input") && n.GetAttributeValue("name", "").Equals("hash"));
            if (hashNode != null)
            {
                _hash = hashNode.Attributes[2].Value;
            }

            Debug.WriteLine("获取HASH码：" + _formHash + ", " + _userId + ", " + _hash);
        }

        /// <summary>
        /// 过滤掉不能出现在XML中的字符
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }
    }
}
