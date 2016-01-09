using Hipda.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media;

namespace Hipda.BackgroundTask
{
    public sealed class UpdateToastTask : IBackgroundTask
    {
        HttpHandle _httpClient = HttpHandle.GetInstance();

        string _xmlForLogin = @"<toast>" +
                        "<visual>" +
                            "<binding template='ToastGeneric'>" +
                                "<text>登录状态已过期，无法为您推送消息</text>" +
                                "<text>打开APP以便刷新登录状态</text>" +
                            "</binding>" +
                        "</visual>" +
                     "</toast>";

        string _xmlForQuoteOrReply = @"<toast>" +
                        "<visual>" +
                            "<binding template='ToastGeneric'>" +
                                "<text>{0}</text>" +
                                "<text>引用或答复了您在《{1}》主题的贴子</text>" +
                                "<image placement='appLogoOverride' src='{2}' hint-crop='circle'/>" +
                                "<text>“ {3} ”</text>" +
                            "</binding>" +
                        "</visual>" +
                        "<actions>" +
                            "<input id='replyContent' type='text' placeHolderContent='输入回复内容' />" +
                            "<action content='回复' arguments='post' />" +
                        "</actions>" +
                     "</toast>";

        string _xmlForThread = @"<toast>" +
                        "<visual>" +
                            "<binding template='ToastGeneric'>" +
                                "<text>{0}</text>" +
                                "<text>回复了您关注的主题《{1}》</text>" +
                            "</binding>" +
                        "</visual>" +
                     "</toast>";

        string _xmlForBuddy = @"<toast>" +
                        "<visual>" +
                            "<binding template='ToastGeneric'>" +
                                "<text>{0}</text>" +
                                "<text>添加您为好友</text>" +
                                "<image placement='appLogoOverride' src='{1}' hint-crop='circle'/>" +
                            "</binding>" +
                        "</visual>" +
                        "<actions>" +
                            "<action content='加对方为好友' arguments='post' />" +
                        "</actions>" +
                     "</toast>";

        string _xmlForPm = @"<toast>" +
                        "<visual>" +
                            "<binding template='ToastGeneric'>" +
                                "<text>{0}</text>" +
                                "<text>发来私信 “ {1} ”</text>" +
                                "<image placement='appLogoOverride' src='{2}' hint-crop='circle'/>" +
                            "</binding>" +
                        "</visual>" +
                        "<actions>" +
                            "<input id='replyPmContent' type='text' placeHolderContent='输入回复内容' />" +
                            "<action content='回复' arguments='post' />" +
                        "</actions>" +
                     "</toast>";

        async Task UpdateNoticeToastAsync(CancellationTokenSource cts)
        {
            string url = string.Format("http://www.hi-pda.com/forum/notice.php?_={0}", DateTime.Now.Ticks.ToString("x"));
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            var items = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("ul") && n.GetAttributeValue("class", "").Equals("feed"))?.ChildNodes;
            if (items == null)
            {
                SendToast(_xmlForLogin);
                return;
            }

            foreach (var item in items)
            {
                bool isNew = false;
                string userId = string.Empty;
                string username = string.Empty;
                string actionTime = string.Empty;
                string threadId = string.Empty;
                string threadTitle = string.Empty;
                string originalText = string.Empty;
                string actionText = string.Empty;
                string repostStr = string.Empty;
                string postId = string.Empty;

                HtmlNode userLinkNode;
                HtmlNode threadLinkNode;

                var divNode = item.ChildNodes[0];
                string nodeClass = divNode.Attributes[0].Value.Trim().ToLower();
                switch (nodeClass)
                {
                    case "f_quote":
                    case "f_reply":
                        isNew = divNode.ChildNodes[5].Name.Equals("img");
                        if (isNew)
                        {
                            userLinkNode = divNode.ChildNodes[0];
                            userId = userLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/space.php?from=notice&uid=".Length).Split('&')[0];
                            username = userLinkNode.InnerText.Trim();

                            threadLinkNode = divNode.ChildNodes[2];
                            threadId = threadLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/viewthread.php?from=notice&tid=".Length).Split('&')[0];
                            threadTitle = threadLinkNode.InnerText.Trim();

                            actionTime = divNode.ChildNodes[4].InnerText.Trim();

                            var actionContentNode = divNode.ChildNodes[7];
                            var buttonsNode = divNode.ChildNodes[9];

                            originalText = actionContentNode.ChildNodes[0].ChildNodes[1].ChildNodes[0]
                                .InnerText.Trim()
                                .Replace("\r", " ")
                                .Replace("\n", " ");
                            actionText = actionContentNode.ChildNodes[0].ChildNodes[1].ChildNodes[2]
                                .InnerText.Trim()
                                .Replace("\r", " ")
                                .Replace("\n", " ");

                            var replyLinkNode = buttonsNode.ChildNodes[0];
                            repostStr = replyLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/post.php?from=notice&action=reply&fid=2&tid=1778684&reppost=".Length).Split('&')[0];
                            var viewLinkNode = buttonsNode.ChildNodes[2];
                            postId = viewLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/redirect.php?from=notice&goto=findpost&pid=".Length).Split('&')[0];

                            // 保存数据，以便打开APP时还原“NEW”状态
                            SaveNoticeToastTempData(0, actionTime);

                            // 发送
                            _xmlForQuoteOrReply = string.Format(_xmlForQuoteOrReply, username, threadTitle, GetSmallAvatarUrlByUserId(Convert.ToInt32(userId)), actionText);
                            SendToast(_xmlForQuoteOrReply);
                        }
                        break;
                    case "f_thread":
                        var nodes = divNode.ChildNodes;
                        isNew = nodes.Count(n => n.Name.Equals("img") && n.GetAttributeValue("alt", "").Equals("NEW")) == 1;
                        if (isNew)
                        {
                            var usernames = new List<string>();
                            var usernameNodes = nodes.Where(n => n.Name.Equals("a") && n.Attributes[0].Value.StartsWith("space.php?username="));
                            foreach (var n in usernameNodes)
                            {
                                usernames.Add(n.InnerText.Trim());
                            }
                            username = string.Join(",", usernames);

                            threadLinkNode = nodes.FirstOrDefault(n => n.Name.Equals("a") && n.Attributes[0].Value.StartsWith("http://www.hi-pda.com/forum/redirect.php?from=notice&goto=findpost&pid="));
                            string linkUrlStr = threadLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/redirect.php?from=notice&goto=findpost&pid=".Length).Replace("ptid=", string.Empty);
                            string[] idsAry = linkUrlStr.Split('&');
                            postId = idsAry[0];
                            threadId = idsAry[1];
                            threadTitle = threadLinkNode.InnerText.Trim();
                            actionTime = nodes.FirstOrDefault(n => n.Name.Equals("em")).InnerText.Trim();

                            // 保存数据，以便打开APP时还原“NEW”状态
                            SaveNoticeToastTempData(1, actionTime);

                            // 发送
                            _xmlForThread = string.Format(_xmlForThread, username, threadTitle);
                            SendToast(_xmlForThread);
                        }
                        break;
                    case "f_buddy":
                        isNew = divNode.ChildNodes[3].Name.Equals("img");
                        if (isNew)
                        {
                            userLinkNode = divNode.ChildNodes[0];
                            userId = userLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/space.php?from=notice&uid=".Length);
                            username = userLinkNode.InnerText.Trim();
                            actionTime = divNode.ChildNodes[2].InnerText.Trim();

                            // 保存数据，以便打开APP时还原“NEW”状态
                            SaveNoticeToastTempData(2, actionTime);

                            // 发送
                            _xmlForBuddy = string.Format(_xmlForBuddy, username, GetSmallAvatarUrlByUserId(Convert.ToInt32(userId)));
                            SendToast(_xmlForBuddy);
                        }
                        break;
                }
            }
        }

        async Task UpdatePmToastAsync(CancellationTokenSource cts)
        {
            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/pm.php?filter=privatepm&_={0}", DateTime.Now.Ticks.ToString("x"));
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            var items = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("ul") && n.GetAttributeValue("class", "").Equals("pm_list"))?.ChildNodes;
            if (items == null)
            {
                SendToast(_xmlForLogin);
                return;
            }

            foreach (var item in items)
            {
                var isNew = item.ChildNodes[3].ChildNodes.Count >= 4 && item.ChildNodes[3].ChildNodes[3].Name.Equals("img");
                if (isNew)
                {
                    var citeNode = item.ChildNodes[3].ChildNodes[1];
                    var linkNode = citeNode.ChildNodes[0];
                    int userId = Convert.ToInt32(linkNode.Attributes[0].Value.Substring("space.php?uid=".Length).Split('&')[0]);
                    string username = linkNode.InnerText.Trim();
                    string lastMessageTime = item.ChildNodes[3].ChildNodes[2].InnerText.Trim();
                    string lastMessageText = item.ChildNodes[5].InnerText.Trim();

                    var flag = CheckIsExistedForPmToastTempData(userId);
                    if (flag == false)
                    {
                        // 保存数据，以便打开APP时还原“NEW”状态
                        SavePmToastTempData(userId);

                        _xmlForPm = string.Format(_xmlForPm, username, lastMessageText, GetSmallAvatarUrlByUserId(userId));
                        SendToast(_xmlForPm);
                    }
                }
            }
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

        void SaveNoticeToastTempData(int noticeType, string actionTime)
        {
            string _containerKey = "HIPDA";
            string _dataKey = "NoticeToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);

            // 保存数据，以便打开APP时还原“NEW”状态
            string toastData = string.Format("{0}#{1}", noticeType, actionTime);
            if (_container.Values.ContainsKey(_dataKey))
            {
                _container.Values[_dataKey] = _container.Values[_dataKey].ToString().Trim() + "," + toastData;
            }
            else
            {
                _container.Values[_dataKey] = toastData;
            }
        }

        void SavePmToastTempData(int userId)
        {
            string _containerKey = "HIPDA";
            string _dataKey = "PmToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);

            // 保存数据，以便打开APP时还原“NEW”状态
            string toastData = userId.ToString();
            if (_container.Values.ContainsKey(_dataKey))
            {
                _container.Values[_dataKey] = _container.Values[_dataKey].ToString().Trim() + "," + toastData;
            }
            else
            {
                _container.Values[_dataKey] = toastData;
            }
        }

        bool CheckIsExistedForPmToastTempData(int userId)
        {
            string _containerKey = "HIPDA";
            string _dataKey = "PmToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);

            if (_container.Values.ContainsKey(_dataKey))
            {
                var list = _container.Values[_dataKey].ToString().Split(',').ToList();
                return list.Any(u => u.Equals(userId.ToString()));
            }

            return false;
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var cost = BackgroundWorkCost.CurrentBackgroundWorkCost;
            if (cost == BackgroundWorkCostValue.High)
            {
                Debug.Write("任务取消（cost is high）");
            }

            var cancel = new CancellationTokenSource();
            taskInstance.Canceled += (s, e) => 
            {
                cancel.Cancel();
                cancel.Dispose();
            };

            var deferral = taskInstance.GetDeferral();
            try
            {
                var cts = new CancellationTokenSource();
                await UpdateNoticeToastAsync(cts);
                await UpdatePmToastAsync(cts);
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

        private void OnNotification(ToastNotification sender, object args)
        {
            Debug.WriteLine("[Toast] in OnNotification");
        }



        private const string TileTemplateXml = @"<tile>
  <visual branding=""nameAndLogo"">
    <binding template=""TileMedium"" hint-textStacking=""center"">
      <image src=""{{0}}"" placement=""peek"" hint-crop=""circle""/>
      <text hint-style=""captionSubtle"" hint-align=""center"">{{1}}</text>
      <text hint-style=""captionSubtle"" hint-align=""center"">{{2}}</text>
    </binding>

    <binding template=""TileWide"">
      <group>
        <subgroup hint-weight=""33"">
          <image src=""{{0}}"" hint-crop=""circle"" />
        </subgroup>
        <subgroup hint-textStacking=""center"">
          <text hint-style=""subtitleSubtle"">{{1}}</text>
          <text hint-style=""subtitleSubtle"">{{2}}</text>
        </subgroup>
      </group>
    </binding>

    <binding template=""TileLarge"" hint-textStacking=""center"">
      <group>
        <subgroup hint-weight=""1""/>
        <subgroup hint-weight=""2"">
          <image src=""{{0}}"" hint-crop=""circle""/>
        </subgroup>
        <subgroup hint-weight=""1""/>
      </group>
      <text hint-style=""title"" hint-align=""center"">{{1}}</text>
      <text hint-style=""subtitleSubtle"" hint-align=""center"">{{2}}</text>
    </binding>
  </visual>
</tile>";

        //private void UpdatePrimaryTile(List<NoticeItemModel> promptData)
        //{
        //    if (promptData == null || !promptData.Any())
        //    {
        //        return;
        //    }

        //    try
        //    {
        //        var updater = TileUpdateManager.CreateTileUpdaterForApplication();
        //        updater.EnableNotificationQueueForWide310x150(true);
        //        updater.EnableNotificationQueueForSquare150x150(true);
        //        updater.EnableNotificationQueueForSquare310x310(true);
        //        updater.EnableNotificationQueue(true);
        //        updater.Clear();

        //        foreach (var promptItem in promptData)
        //        {
        //            var doc = new XmlDocument();
        //            if (promptItem.NoticeType == NoticeType.QuoteOrReply)
        //            {
        //                var xml = string.Format(TileTemplateXml, GetSmallAvatarUrlByUserId(Convert.ToInt32(promptItem.ActionInfo[0])), promptItem.Username, promptItem.ActionInfo[4]);
        //                doc.LoadXml(WebUtility.HtmlDecode(xml), new XmlLoadSettings
        //                {
        //                    ProhibitDtd = false,
        //                    ValidateOnParse = false,
        //                    ElementContentWhiteSpace = false,
        //                    ResolveExternals = false
        //                });

        //                updater.Update(new TileNotification(doc));
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        // ignored
        //    }
        //}

        static string GetSmallAvatarUrlByUserId(int userId)
        {
            var s = new int[10];
            for (int i = 0; i < s.Length - 1; ++i)
            {
                s[i] = userId % 10;
                userId = (userId - s[i]) / 10;
            }
            return "http://www.hi-pda.com/forum/uc_server/data/avatar/" + s[8] + s[7] + s[6] + "/" + s[5] + s[4] + "/" + s[3] + s[2] + "/" + s[1] + s[0] + "_avatar_small.jpg";
        }

        /// <summary>
        /// 过滤掉不能出现在XML中的字符
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        static string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }
    }
}
