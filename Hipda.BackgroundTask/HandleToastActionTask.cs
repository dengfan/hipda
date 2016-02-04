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
            toastXml = ReplaceEmojiLabel(toastXml);

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

        static Dictionary<string, string> EmojiDic = new Dictionary<string, string>
        {
            { ":grinning:",     "\uD83D\uDE00" },
            { ":grin:",         "\uD83D\uDE01" },
            { ":joy:",          "\uD83D\uDE02" },
            { ":smiley:",       "\uD83D\uDE03" },
            { ":smile:",        "\uD83D\uDE04" },
            { ":sweat_smile:",  "\uD83D\uDE05" },
            { ":laughing:",     "\uD83D\uDE06" },
            { ":innocent:",     "\uD83D\uDE07" },
            { ":smiling_imp:",  "\uD83D\uDE08" },
            { ":wink:",         "\uD83D\uDE09" },
            { ":blush:",        "\uD83D\uDE0A" },
            { ":yum:",          "\uD83D\uDE0B" },
            { ":satisfied:",    "\uD83D\uDE0C" },
            { ":heart_eyes:",   "\uD83D\uDE0D" },
            { ":sunglasses:",   "\uD83D\uDE0E" },
            { ":smirk:",        "\uD83D\uDE0F" },

            { ":neutral_face:",                 "\uD83D\uDE10" },
            { ":expressionless:",               "\uD83D\uDE11" },
            { ":unamused:",                     "\uD83D\uDE12" },
            { ":sweat:",                        "\uD83D\uDE13" },
            { ":pensive:",                      "\uD83D\uDE14" },
            { ":confused:",                     "\uD83D\uDE15" },
            { ":confounded:",                   "\uD83D\uDE16" },
            { ":kissing:",                      "\uD83D\uDE17" },
            { ":kissing_heart:",                "\uD83D\uDE18" },
            { ":kissing_smiling_eyes:",         "\uD83D\uDE19" },
            { ":kissing_closed_eyes:",          "\uD83D\uDE1A" },
            { ":stuck_out_tongue:",             "\uD83D\uDE1B" },
            { ":wink2:",                        "\uD83D\uDE1C" },
            { ":stuck_out_tongue_closed_eyes:", "\uD83D\uDE1D" },
            { ":disappointed:",                 "\uD83D\uDE1E" },
            { ":worried:",                      "\uD83D\uDE1F" },

            { ":angry:",            "\uD83D\uDE20" },
            { ":rage:",             "\uD83D\uDE21" },
            { ":cry:",              "\uD83D\uDE22" },
            { ":persevere:",        "\uD83D\uDE23" },
            { ":triumph:",          "\uD83D\uDE24" },
            { ":relieved:",         "\uD83D\uDE25" },
            { ":frowning:",         "\uD83D\uDE26" },
            { ":anguished:",        "\uD83D\uDE27" },
            { ":fearful:",          "\uD83D\uDE28" },
            { ":weary:",            "\uD83D\uDE29" },
            { ":sleepy:",           "\uD83D\uDE2A" },
            { ":tired_face:",       "\uD83D\uDE2B" },
            { ":grimacing:",        "\uD83D\uDE2C" },
            { ":sob:",              "\uD83D\uDE2D" },
            { ":open_mouth:",       "\uD83D\uDE2E" },
            { ":hushed:",           "\uD83D\uDE2F" },

            { ":cold_sweat:",       "\uD83D\uDE30" },
            { ":scream:",           "\uD83D\uDE31" },
            { ":astonished:",       "\uD83D\uDE32" },
            { ":flushed:",          "\uD83D\uDE33" },
            { ":sleeping:",         "\uD83D\uDE34" },
            { ":dizzy_face:",       "\uD83D\uDE35" },
            { ":no_mouth:",         "\uD83D\uDE36" },
            { ":mask:",             "\uD83D\uDE37" },
            { ":smile_cat:",        "\uD83D\uDE38" },
            { ":joy_cat:",          "\uD83D\uDE39" },
            { ":smiley_cat:",       "\uD83D\uDE3A" },
            { ":heart_eyes_cat:",   "\uD83D\uDE3B" },
            { ":smirk_cat:",        "\uD83D\uDE3C" },
            { ":kissing_cat:",      "\uD83D\uDE3D" },
            { ":pouting_cat:",      "\uD83D\uDE3E" },
            { ":crying_cat_face:",  "\uD83D\uDE3F" },

            { ":scream_cat:",               "\uD83D\uDE40" },
            //{ "",                           "\uD83D\uDE41" },
            //{ "",                           "\uD83D\uDE42" },
            //{ "",                           "\uD83D\uDE43" },
            //{ "",                           "\uD83D\uDE44" },
            { ":no_good:",                  "\uD83D\uDE45" },
            { ":man_ok:",                   "\uD83D\uDE46" },
            { ":bow:",                      "\uD83D\uDE47" },
            { ":see_no_evil:",              "\uD83D\uDE48" },
            { ":hear_no_evil:",             "\uD83D\uDE49" },
            { ":speak_no_evil:",            "\uD83D\uDE4A" },
            { ":raised_hand:",              "\uD83D\uDE4B" },
            { ":raised_hands:",             "\uD83D\uDE4C" },
            { ":person_frowning:",          "\uD83D\uDE4D" },
            { ":person_with_pouting_face:", "\uD83D\uDE4E" },
            { ":pray:",                     "\uD83D\uDE4F" },

            //{ "",                     "\uD83D\uDC40" },
            //{ "",                     "\uD83D\uDC41" },
            //{ "",                     "\uD83D\uDC42" },
            //{ "",                     "\uD83D\uDC43" },
            //{ "",                     "\uD83D\uDC44" },
            //{ "",                     "\uD83D\uDC45" },
            //{ "",                     "\uD83D\uDC46" },
            //{ "",                     "\uD83D\uDC47" },
            //{ "",                     "\uD83D\uDC48" },
            //{ "",                     "\uD83D\uDC49" },
            { ":punch:",        "\uD83D\uDC4A" },
            { ":wave:",         "\uD83D\uDC4B" },
            { ":hand_ok:",      "\uD83D\uDC4C" },
            { ":thumbsup:",     "\uD83D\uDC4D" },
            { ":thumbsdown:",   "\uD83D\uDC4E" },
            { ":clap:",         "\uD83D\uDC4F" },

            { ":pear:",         "\uD83C\uDF50" },
            { ":peach:",         "\uD83C\uDF51" },
            { ":cherries:",         "\uD83C\uDF52" },
            { ":strawberry:",         "\uD83C\uDF53" },
            { ":hamburger:",         "\uD83C\uDF54" },
            { ":pizza:",         "\uD83C\uDF55" },
            { ":meat_on_bone:",         "\uD83C\uDF56" },
            { ":poultry_leg:",         "\uD83C\uDF57" },
            { ":rice_cracker:",         "\uD83C\uDF58" },
            { ":rice_ball:",         "\uD83C\uDF59" },
            { ":rice:",         "\uD83C\uDF5A" },
            { ":curry:",         "\uD83C\uDF5B" },
            { ":ramen:",         "\uD83C\uDF5C" },
            { ":spaghetti:",         "\uD83C\uDF5D" },
            { ":bread:",         "\uD83C\uDF5E" },
            { ":fries:",         "\uD83C\uDF5F" },

            { ":ribbon:",         "\uD83C\uDF80" },
            { ":gift:",         "\uD83C\uDF81" },
            { ":birthday:",         "\uD83C\uDF82" },
            { ":jack_o_lantern:",         "\uD83C\uDF83" },
            { ":christmas_tree:",         "\uD83C\uDF84" },
            { ":santa:",         "\uD83C\uDF85" },
            { ":fireworks:",         "\uD83C\uDF86" },
            { ":sparkler:",         "\uD83C\uDF87" },
            { ":balloon:",         "\uD83C\uDF88" },
            { ":tada:",         "\uD83C\uDF89" },
            { ":confetti_ball:",         "\uD83C\uDF8A" },
            { ":tanabata_tree:",         "\uD83C\uDF8B" },
            { ":crossed_flags:",         "\uD83C\uDF8C" },
            { ":bamboo:",         "\uD83C\uDF8D" },
            { ":dolls:",         "\uD83C\uDF8E" },
            { ":flags:",         "\uD83C\uDF8F" },

            { ":skull:",         "\uD83D\uDC80" },
            { ":information_desk_person:",         "\uD83D\uDC81" },
            { ":guardsman:",         "\uD83D\uDC82" },
            { ":dancer:",         "\uD83D\uDC83" },
            { ":lipstick:",         "\uD83D\uDC84" },
            { ":nail_care:",         "\uD83D\uDC85" },
            { ":massage:",         "\uD83D\uDC86" },
            { ":haircut:",         "\uD83D\uDC87" },
            { ":barber:",         "\uD83D\uDC88" },
            { ":syringe:",         "\uD83D\uDC89" },
            { ":pill:",         "\uD83D\uDC8A" },
            { ":kiss:",         "\uD83D\uDC8B" },
            { ":love_letter:",         "\uD83D\uDC8C" },
            { ":ring:",         "\uD83D\uDC8D" },
            { ":gem:",         "\uD83D\uDC8E" },
            { ":couplekiss:",         "\uD83D\uDC8F" },

            { ":bouquet:",         "\uD83D\uDC90" },
            { ":couple_with_heart:",         "\uD83D\uDC91" },
            { ":wedding:",         "\uD83D\uDC92" },
            { ":heartbeat:",         "\uD83D\uDC93" },
            { ":broken_heart:",         "\uD83D\uDC94" },
            { ":two_hearts:",         "\uD83D\uDC95" },
            { ":sparkling_heart:",         "\uD83D\uDC96" },
            { ":heartpulse:",         "\uD83D\uDC97" },
            { ":cupid:",         "\uD83D\uDC98" },
            { ":blue_heart:",         "\uD83D\uDC99" },
            { ":green_heart:",         "\uD83D\uDC9A" },
            { ":yellow_heart:",         "\uD83D\uDC9B" },
            { ":purple_heart:",         "\uD83D\uDC9C" },
            { ":gift_heart:",         "\uD83D\uDC9D" },
            { ":revolving_hearts:",         "\uD83D\uDC9E" },
            { ":heart_decoration:",         "\uD83D\uDC9F" },

            { ":diamond_shape_with_a_dot_inside:",         "\uD83D\uDCA0" },
            { ":bulb:",         "\uD83D\uDCA1" },
            { ":anger:",         "\uD83D\uDCA2" },
            { ":bomb:",         "\uD83D\uDCA3" },
            { ":zzz:",         "\uD83D\uDCA4" },
            { ":boom:",         "\uD83D\uDCA5" },
            { ":sweat_drops:",         "\uD83D\uDCA6" },
            { ":droplet:",         "\uD83D\uDCA7" },
            { ":dash:",         "\uD83D\uDCA8" },
            { ":shit:",         "\uD83D\uDCA9" },
            { ":muscle:",         "\uD83D\uDCAA" },
            { ":dizzy:",         "\uD83D\uDCAB" },
            { ":speech_balloon:",         "\uD83D\uDCAC" },
            { ":thought_balloon:",         "\uD83D\uDCAD" },
            { ":white_flower:",         "\uD83D\uDCAE" },
            //{ "",         "\uD83D\uDCAF" },
        };

        static string ReplaceEmojiLabel(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return string.Empty;
            }

            var matchs = new Regex(@":[a-z_]{3,30}:").Matches(txt);
            if (matchs != null && matchs.Count > 0)
            {
                for (int i = 0; i < matchs.Count; i++)
                {
                    var m = matchs[i];

                    string emojiLabel = m.Groups[0].Value; // 要被替换的标签
                    if (EmojiDic.ContainsKey(emojiLabel))
                    {
                        txt = txt.Replace(emojiLabel, EmojiDic[emojiLabel]);
                    }
                }
            }

            return txt;
        }
    }
}
