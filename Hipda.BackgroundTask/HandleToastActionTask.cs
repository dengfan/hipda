using Hipda.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Hipda.BackgroundTask
{
    public sealed class HandleToastActionTask : IBackgroundTask
    {
        static HttpHandle _httpClient = HttpHandle.GetInstance();

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            try
            {
                var cts = new CancellationTokenSource();
                await LoginAsync(cts); // 先登录

                if (taskInstance.TriggerDetails is ToastNotificationActionTriggerDetail)
                {
                    var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
                    var args = details.Argument;
                    if (args.StartsWith("reply_pm="))
                    {
                        await HandleReplyPm(details, args, cts);
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

        async Task HandleReplyPm(ToastNotificationActionTriggerDetail details, string args, CancellationTokenSource cts)
        {
            int userId = Convert.ToInt32(args.Substring("reply_pm=".Length));
            string replyContent = details.UserInput["inputPm"].ToString();
            await ReplyPmAsync(userId, replyContent, cts);
        }

        async Task ReplyPmAsync(int userId, string replyContent, CancellationTokenSource cts)
        {
            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", "b89edaf2"));
            postData.Add(new KeyValuePair<string, object>("handlekey", "pmreply"));
            postData.Add(new KeyValuePair<string, object>("lastdaterange", DateTime.Now.ToString("yyyy-M-d")));
            postData.Add(new KeyValuePair<string, object>("message", replyContent));

            string url = string.Format("http://www.hi-pda.com/forum/pm.php?action=send&uid={0}&pmsubmit=yes&infloat=yes&inajax=1&_={1}", userId, DateTime.Now.Ticks.ToString("x"));
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            Debug.WriteLine("回复结果：" +  (resultContent.StartsWith(@"<?xml version=""1.0"" encoding=""gbk""?><root><![CDATA[<li id=""pm_") && resultContent.Contains(@"images/default/notice_newpm.gif")));
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
    }
}
