using Hipda.Client.Models;
using Hipda.Client.ViewModels;
using Hipda.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hipda.Client.Services
{
    public class NoticeService
    {
        static HttpHandle _httpClient = HttpHandle.GetInstance();

        async Task<List<NoticeItemViewModel>> LoadNoticeDataAsync(CancellationTokenSource cts)
        {
            var data = new List<NoticeItemViewModel>();

            string url = string.Format("http://www.hi-pda.com/forum/notice.php?_={0}", DateTime.Now.Ticks.ToString("x"));
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            var items = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("ul") && n.GetAttributeValue("class", "").Equals("feed"))?.ChildNodes;
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
                        noticeType = NoticeType.QuoteOrReply;
                        userLinkNode = divNode.ChildNodes[0];
                        userId = userLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/space.php?from=notice&uid=".Length).Split('&')[0];
                        username = userLinkNode.InnerText.Trim();

                        threadLinkNode = divNode.ChildNodes[2];
                        threadId = threadLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/viewthread.php?from=notice&tid=".Length).Split('&')[0];
                        threadTitle = threadLinkNode.InnerText.Trim();

                        actionTime = divNode.ChildNodes[4].InnerText.Trim();

                        HtmlNode actionContentNode;
                        HtmlNode buttonsNode;
                        if (divNode.ChildNodes[5].Name.Equals("img"))
                        {
                            isNew = true;

                            actionContentNode = divNode.ChildNodes[7];
                            buttonsNode = divNode.ChildNodes[9];
                        }
                        else
                        {
                            actionContentNode = divNode.ChildNodes[6];
                            buttonsNode = divNode.ChildNodes[8];
                        }

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

                        data.Add(new NoticeItemViewModel(noticeType, isNew, username, actionTime, new string[] {
                            userId,         // 0
                            threadId,       // 1
                            threadTitle,    // 2
                            originalText,   // 3
                            actionText,     // 4
                            repostStr,      // 5
                            postId          // 6
                        }));
                        break;
                    case "f_thread":
                        noticeType = NoticeType.Thread;
                        var nodes = divNode.ChildNodes;
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
                        isNew = nodes.Count(n => n.Name.Equals("img") && n.GetAttributeValue("alt", "").Equals("NEW")) == 1;

                        data.Add(new NoticeItemViewModel(noticeType, isNew, username, actionTime, new string[] {
                            threadId,
                            threadTitle,
                            postId
                        }));
                        break;
                    case "f_buddy":
                        noticeType = NoticeType.Buddy;
                        userLinkNode = divNode.ChildNodes[0];
                        userId = userLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/space.php?from=notice&uid=".Length);
                        username = userLinkNode.InnerText.Trim();
                        actionTime = divNode.ChildNodes[2].InnerText.Trim();
                        isNew = divNode.ChildNodes[3].Name.Equals("img");

                        data.Add(new NoticeItemViewModel(noticeType, isNew, username, actionTime, new string[] {
                            userId
                        }));
                        break;
                }
            }

            #region 还原通知数据的“NEW”状态后，清除 toast notice data
            var noticeToastTempData = ToastService.GetNoticeToastTempData();
            if (noticeToastTempData != null)
            {
                foreach (var i in noticeToastTempData)
                {
                    foreach (var j in data)
                    {
                        string key = string.Format("{0}#{1}", (int)j.NoticeType, j.ActionTime);
                        if (key.Equals(i) && j.IsNew == false)
                        {
                            j.IsNew = true;
                            break;
                        }
                    }
                }
            }

            ToastService.ClearNoticeToastTempData();
            #endregion

            return data;
        }

        public async Task<List<NoticeItemViewModel>> GetNoticeData()
        {
            var cts = new CancellationTokenSource();
            return await LoadNoticeDataAsync(cts);
        }
    }
}
