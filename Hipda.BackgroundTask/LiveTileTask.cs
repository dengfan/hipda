using Hipda.BackgroundTask.Models;
using Hipda.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.UI.Notifications;

namespace Hipda.BackgroundTask
{
    public sealed class LiveTileTask : IBackgroundTask
    {
        static HttpHandle _httpClient = HttpHandle.GetInstance();

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            await GetPromptData();

            deferral.Complete();
        }

        private async Task GetPromptData()
        {
            try
            {
                var cts = new CancellationTokenSource();
                var promptData = await LoadNoticeDataAsync(cts);
                UpdatePrimaryTile(promptData);
            }
            catch (Exception)
            {
                // 忽略
            }
        }

        async Task<List<NoticeItemModel>> LoadNoticeDataAsync(CancellationTokenSource cts)
        {
            var data = new List<NoticeItemModel>();

            string url = string.Format("http://www.hi-pda.com/forum/notice.php?_={0}", DateTime.Now.Ticks.ToString("x"));
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            var items = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("ul") && n.GetAttributeValue("class", "").Equals("feed")).ChildNodes;
            if (items == null)
            {
                return null;
            }

            foreach (var item in items)
            {
                NoticeType noticeType;
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
                            noticeType = NoticeType.QuoteOrReply;
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

                            data.Add(new NoticeItemModel(noticeType, isNew, username, actionTime, new string[] {
                                userId,         // 0
                                threadId,       // 1
                                threadTitle,    // 2
                                originalText,   // 3
                                actionText,     // 4
                                repostStr,      // 5
                                postId          // 6
                            }));
                        }
                        break;
                    case "f_thread":
                        var nodes = divNode.ChildNodes;
                        isNew = nodes.Count(n => n.Name.Equals("img") && n.GetAttributeValue("alt", "").Equals("NEW")) == 1;
                        if (isNew)
                        {
                            noticeType = NoticeType.Thread;

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

                            data.Add(new NoticeItemModel(noticeType, isNew, username, actionTime, new string[] {
                                threadId,
                                threadTitle,
                                postId
                            }));
                        }
                        break;
                    case "f_buddy":
                        isNew = divNode.ChildNodes[3].Name.Equals("img");
                        if (isNew)
                        {
                            noticeType = NoticeType.Buddy;
                            userLinkNode = divNode.ChildNodes[0];
                            userId = userLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/space.php?from=notice&uid=".Length);
                            username = userLinkNode.InnerText.Trim();
                            actionTime = divNode.ChildNodes[2].InnerText.Trim();

                            data.Add(new NoticeItemModel(noticeType, isNew, username, actionTime, new string[] {
                                userId
                            }));
                        }
                        break;
                }
            }

            return data;
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

        private void UpdatePrimaryTile(List<NoticeItemModel> promptData)
        {
            if (promptData == null || !promptData.Any())
            {
                return;
            }

            try
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.EnableNotificationQueueForWide310x150(true);
                updater.EnableNotificationQueueForSquare150x150(true);
                updater.EnableNotificationQueueForSquare310x310(true);
                updater.EnableNotificationQueue(true);
                updater.Clear();

                foreach (var promptItem in promptData)
                {
                    var doc = new XmlDocument();
                    if (promptItem.NoticeType == NoticeType.QuoteOrReply)
                    {
                        var xml = string.Format(TileTemplateXml, GetSmallAvatarUrlByUserId(Convert.ToInt32(promptItem.ActionInfo[0])), promptItem.Username, promptItem.ActionInfo[4]);
                        doc.LoadXml(WebUtility.HtmlDecode(xml), new XmlLoadSettings
                        {
                            ProhibitDtd = false,
                            ValidateOnParse = false,
                            ElementContentWhiteSpace = false,
                            ResolveExternals = false
                        });

                        updater.Update(new TileNotification(doc));
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static string GetSmallAvatarUrlByUserId(int userId)
        {
            var s = new int[10];
            for (int i = 0; i < s.Length - 1; ++i)
            {
                s[i] = userId % 10;
                userId = (userId - s[i]) / 10;
            }

            return "http://www.hi-pda.com/forum/uc_server/data/avatar/" + s[8] + s[7] + s[6] + "/" + s[5] + s[4] + "/" + s[3] + s[2] + "/" + s[1] + s[0] + "_avatar_small.jpg";
        }
    }
}
