using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.ViewModels;
using Hipda.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.Services
{
    public partial class DataService
    {
        static int _threadPageSize = 75;
        static int _replyPageSize = 50;
        static int _searchPageSize = 50;

        static HttpHandle _httpClient = HttpHandle.GetInstance();

        #region prompt
        static void GetPromptData(HtmlNode promptContentNode)
        {
            try
            {
                if (promptContentNode != null)
                {
                    var promtpViewModel = MainPageViewModel.GetInstance();
                    var ulNode = promptContentNode.ChildNodes[1];
                    promtpViewModel.PromptPm = Convert.ToInt32(ulNode.ChildNodes[0].InnerText.Trim().Substring("私人消息 (".Length).Replace(")", string.Empty));
                    promtpViewModel.PromptAnnouncePm = Convert.ToInt32(ulNode.ChildNodes[1].InnerText.Trim().Substring("公共消息 (".Length).Replace(")", string.Empty));
                    promtpViewModel.PromptSystemPm = Convert.ToInt32(ulNode.ChildNodes[2].InnerText.Trim().Substring("系统消息 (".Length).Replace(")", string.Empty));
                    promtpViewModel.PromptFriend = Convert.ToInt32(ulNode.ChildNodes[3].InnerText.Trim().Substring("好友消息 (".Length).Replace(")", string.Empty));
                    promtpViewModel.PromptThreads = Convert.ToInt32(ulNode.ChildNodes[4].InnerText.Trim().Substring("帖子消息 (".Length).Replace(")", string.Empty));
                }
            }
            catch (Exception e)
            {
                string errorDetails = string.Format("{0}", e.Message);
                Common.PostErrorEmailToDeveloper("提醒数据解析出现异常", errorDetails);
            }
        }
        #endregion

        #region page number
        static int GetMaxPageNo(HtmlNode pagesNode)
        {
            int maxPageNo = 1;

            try
            {
                if (pagesNode != null)
                {
                    var nodeList = pagesNode.Descendants().Where(n => n.Name.Equals("a") || n.Name.Equals("strong")).ToList();
                    nodeList.RemoveAll(n => n.InnerText.Equals("下一页"));
                    string lastPageNodeValue = nodeList.Last().InnerText.Replace("... ", string.Empty);
                    maxPageNo = Convert.ToInt32(lastPageNodeValue);
                }
            }
            catch (Exception e)
            {
                string errorDetails = string.Format("{0}", e.Message);
                Common.PostErrorEmailToDeveloper("页码数据解析出现异常", errorDetails);
            }

            return maxPageNo;
        }
        #endregion

        #region thread
        public static ObservableCollection<ThreadItemModelBase> ReadHistoryData = new ObservableCollection<ThreadItemModelBase>();

        Style GetReadStatusStyle(int threadId)
        {
            string styleName = IsRead(threadId) ? "ReadColorStyle" : "UnReadColorStyle";
            return (Style)App.Current.Resources[styleName];
        }

        public bool IsRead(int threadId)
        {
            return ReadHistoryData.Count(h => h.ThreadId == threadId) > 0;
        }

        public string GetThreadTitleFromReplyData(int threadId)
        {
            var replyData = _replyData.FirstOrDefault(r => r.ThreadId == threadId);
            if (replyData != null)
            {
                var replyItem = replyData.Replies.FirstOrDefault(i => i.PageNo == 1 && i.Index == 0);
                if (replyItem != null)
                {
                    return replyItem.ThreadTitle;
                }
            }

            return string.Empty;
        }

        public string GetThreadTitleFromThreadData(int threadId)
        {
            var threadData = _threadData.FirstOrDefault(t => t.ThreadId == threadId);
            if (threadData != null)
            {
                return threadData.Title;
            }
            else
            {
                var threadDataForMyPosts = _threadDataForMyPosts.FirstOrDefault(t => t.ThreadId == threadId);
                if (threadDataForMyPosts != null)
                {
                    return threadDataForMyPosts.Title;
                }
                else
                {
                    var threadDataForMyThreads = _threadDataForMyThreads.FirstOrDefault(t => t.ThreadId == threadId);
                    if (threadDataForMyThreads != null)
                    {
                        return threadDataForMyThreads.Title;
                    }
                }
            }

            return string.Empty;
        }
        #endregion

        #region reply
        static List<ReplyPageModel> _replyData = new List<ReplyPageModel>();
        int _replyMaxPageNo = 1;
        bool _isScrollCompleted = false;

        async Task LoadReplyDataAsync(int threadId, int threadAuthorUserId, int pageNo, CancellationTokenSource cts)
        {
            if (pageNo > 1 && pageNo > _replyMaxPageNo)
            {
                return;
            }

            // 如果页面已存在，则不重新从网站拉取数据，以便节省流量， 
            var threadReply = _replyData.FirstOrDefault(r => r.ThreadId == threadId);
            if (threadReply != null)
            {
                int count = threadReply.Replies.Count(r => r.PageNo == pageNo);
                if (count > 0)
                {
                    return;
                }
            }
            else
            {
                threadReply = new ReplyPageModel { ThreadId = threadId, Replies = new List<ReplyItemModel>() };
                _replyData.Add(threadReply);
            }

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/viewthread.php?tid={0}&page={1}&ordertype={2}&_={3}", threadId, pageNo, string.Empty, DateTime.Now.Ticks.ToString("x"));
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            // 最先读取提醒数据
            var promptContentNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("promptcontent"));
            GetPromptData(promptContentNode);

            // 读取最大页码
            var pagesNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("pages"));
            _replyMaxPageNo = GetMaxPageNo(pagesNode);

            var data = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("id", "").Equals("postlist")).ChildNodes;
            if (data == null)
            {
                return;
            }

            int i = threadReply.Replies.Count();
            foreach (var item in data)
            {
                var mainTable = item.Descendants().FirstOrDefault(n => n.Name.Equals("table") && n.GetAttributeValue("summary", "").StartsWith("pid") && n.GetAttributeValue("id", "").Equals(n.GetAttributeValue("summary", "")));
                var postAuthorNode = mainTable // table
                        .ChildNodes[1] // tr
                        .ChildNodes[1]; // td.postauthor

                var postContentNode = mainTable // table
                        .ChildNodes[1] // tr
                        .ChildNodes[3]; // td.postcontent

                int authorUserId = 0;
                string authorUsername = string.Empty;
                var authorNode = postAuthorNode.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("postinfo"));
                if (authorNode != null)
                {
                    authorNode = authorNode.ChildNodes[1]; // a
                    string authorUserIdStr = authorNode.Attributes[1].Value.Substring("space.php?uid=".Length);
                    if (authorUserIdStr.Contains("&"))
                    {
                        authorUserId = Convert.ToInt32(authorUserIdStr.Split('&')[0]);
                    }
                    else
                    {
                        authorUserId = Convert.ToInt32(authorUserIdStr);
                    }
                    authorUsername = authorNode.InnerText;
                }

                var floorPostInfoNode = postContentNode.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("class", "").StartsWith("postinfo")); // div
                var floorLinkNode = floorPostInfoNode.ChildNodes[1].ChildNodes[0]; // a
                int postId = Convert.ToInt32(floorLinkNode.Attributes["id"].Value.Replace("postnum", string.Empty));
                var floorNumNode = floorLinkNode.ChildNodes[0]; // em
                int floor = Convert.ToInt32(floorNumNode.InnerText);
                string threadTitle = string.Empty;
                if (floor == 1)
                {
                    var threadTitleNode = postContentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("id", "").Equals("threadtitle"));
                    if (threadTitleNode != null)
                    {
                        var h1 = threadTitleNode.ChildNodes[1];
                        var a = h1.Descendants().FirstOrDefault(n => n.Name.Equals("a"));
                        if (a != null)
                        {
                            h1.RemoveChild(a); // 移除版块名称
                        }

                        threadTitle = h1.InnerText.Trim();
                    }
                }

                string postTime = string.Empty;
                var postTimeNode = postContentNode.Descendants().FirstOrDefault(n => n.Name.Equals("em") && n.GetAttributeValue("id", "").StartsWith("authorposton")); // em
                if (postTimeNode != null)
                {
                    postTime = postTimeNode.InnerText
                        .Replace("发表于 ", string.Empty)
                        .Replace(string.Format("{0}-{1}-{2} ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), string.Empty)
                        .Replace(string.Format("{0}-", DateTime.Now.Year), string.Empty);
                }

                string textContent = string.Empty;
                string htmlContent = string.Empty;
                string xamlContent = string.Empty;
                int imageCount = 0;
                var contentNode = postContentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("t_msgfontfix"));
                if (contentNode != null)
                {
                    // 用于回复引用
                    textContent = contentNode.InnerText.Trim();
                    textContent = new Regex("\r\n").Replace(textContent, "↵");
                    textContent = new Regex("\r").Replace(textContent, "↵");
                    textContent = new Regex("\n").Replace(textContent, "↵");
                    textContent = new Regex(@"↵{1,}").Replace(textContent, "\r\n");
                    textContent = textContent.Replace("&nbsp;", "  ");

                    // 用于显示原始内容
                    htmlContent = contentNode.InnerHtml.Trim();

                    // 转换HTML为XAML
                    xamlContent = Html.HtmlToXaml.ConvertPost(threadId, htmlContent, 20, ref imageCount);
                }
                else
                {
                    xamlContent = @"<RichTextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><Paragraph>{0}</Paragraph></RichTextBlock>";
                    xamlContent = string.Format(xamlContent, @"作者被禁止或删除&#160;内容自动屏蔽");
                }

                ReplyItemModel reply = new ReplyItemModel(i, floor, postId, pageNo, threadId, threadTitle, threadAuthorUserId, authorUserId, authorUsername, textContent, htmlContent, xamlContent, postTime, imageCount, false);
                threadReply.Replies.Add(reply);

                i++;
            }
        }

        public async Task<int> LoadMoreReplyItemsAsync(int threadId, int threadAuthorUserId, int pageNo, Action beforeLoad, Action<int, int> afterLoad)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadReplyDataAsync(threadId, threadAuthorUserId, pageNo, cts);
            if (afterLoad != null) afterLoad(threadId, pageNo);

            return _replyData.Single(t => t.ThreadId == threadId).Replies.Count;
        }

        public static ReplyItemModel GetReplyItemByIndex(int threadId, int index)
        {
            return _replyData.Single(t => t.ThreadId == threadId).Replies[index];
        }

        public ICollectionView GetViewForReplyPageByThreadId(int startPageNo, int threadId, int threadAuthorUserId, Action beforeLoad, Action<int, int> afterLoad)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<ReplyItemModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await LoadMoreReplyItemsAsync(threadId, threadAuthorUserId, pageNo, beforeLoad, afterLoad);
                },
                (index) =>
                {
                    // 从静态类中返回需要显示出来的数据
                    return GetReplyItemByIndex(threadId, index);
                },
                () =>
                {
                    return GetReplyMaxPageNo();
                });

            return cvs.View;
        }

        public ICollectionView GetViewForRedirectReplyPageByThreadId(int startPageNo, int threadId, int threadAuthorUserId, int floorIndex, Action beforeLoad, Action<int, int> afterLoad, Action<int> listViewScroll)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<ReplyItemModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await LoadMoreReplyItemsAsync(threadId, threadAuthorUserId, pageNo, beforeLoad, afterLoad);
                },
                (index) =>
                {
                    // 滚动到指定的项
                    if (listViewScroll != null)
                    {
                        listViewScroll(floorIndex);
                    }

                    // 从静态类中返回需要显示出来的数据
                    return GetReplyItemByIndex(threadId, index);
                },
                () =>
                {
                    return GetReplyMaxPageNo();
                });

            return cvs.View;
        }

        /// <summary>
        /// 用于我的回复、全文搜索等列表页，
        /// 他们共同的特点是只根据 post id 来定位到指定的回复项，
        /// 事先并不知道 thread id 及 page no
        /// 请求的地址格式如 .../redirect.php?goto=findpost&pid=12345...
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public async Task<int[]> LoadReplyDataForRedirectReplyPageAsync(int targetPostId, CancellationTokenSource cts)
        {
            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/redirect.php?goto=findpost&pid={0}&_={1}", targetPostId, DateTime.Now.Ticks.ToString("x"));
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            // 获取当前 thread id
            var postReplyLink = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("id", "").Equals("post_reply")).ChildNodes[0];
            string linkUrl = postReplyLink.Attributes[0].Value.Trim();
            string threadIdStr = linkUrl.Substring("post.php?action=reply&amp;fid=14&amp;tid=".Length);
            int threadId = 0;
            if (!int.TryParse(threadIdStr, out threadId))
            {
                return null;
            }

            // 先清空本贴的回复数据，以便重新加载
            _replyData.RemoveAll(r => r.ThreadId == threadId);
            var threadReply = new ReplyPageModel { ThreadId = threadId, Replies = new List<ReplyItemModel>() };
            _replyData.Add(threadReply);

            // 获取当前页码，及最大页码
            int pageNo = 1;
            var forumControlNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("forumcontrol s_clear"));
            var pagesNode = forumControlNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("pages"));
            if (pagesNode != null)
            {
                var nodeList = pagesNode.Descendants().Where(n => n.Name.Equals("a") || n.Name.Equals("strong")).ToList();
                nodeList.RemoveAll(n => n.InnerText.Equals("下一页"));
                string lastPageNodeValue = nodeList.Last().InnerText.Replace("... ", string.Empty);
                _replyMaxPageNo = Convert.ToInt32(lastPageNodeValue);

                var currentPageNode = nodeList.FirstOrDefault(n => n.Name.Equals("strong"));
                if (currentPageNode != null)
                {
                    pageNo = Convert.ToInt32(currentPageNode.InnerText);
                }
            }

            var data = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("id", "").Equals("postlist")).ChildNodes;
            int i = 0;
            foreach (var item in data)
            {
                var mainTable = item.Descendants().FirstOrDefault(n => n.Name.Equals("table") && n.GetAttributeValue("summary", "").StartsWith("pid"));
                var tableRowNode = mainTable.ChildNodes[1]; // tr

                var postAuthorNode = tableRowNode.ChildNodes[1]; // td.postauthor
                if (string.IsNullOrEmpty(postAuthorNode.InnerText))
                {
                    tableRowNode = mainTable.ChildNodes[3]; // tr
                    postAuthorNode = tableRowNode.ChildNodes[1]; // td.postauthor
                }

                var postContentNode = tableRowNode.ChildNodes[3]; // td.postcontent

                int authorUserId = 0;
                string authorUsername = string.Empty;
                var authorNode = postAuthorNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("postinfo"));
                if (authorNode != null)
                {
                    authorNode = authorNode.ChildNodes[1]; // a
                    string authorUserIdStr = authorNode.Attributes[1].Value.Substring("space.php?uid=".Length);
                    if (authorUserIdStr.Contains("&"))
                    {
                        authorUserId = Convert.ToInt32(authorUserIdStr.Split('&')[0]);
                    }
                    else
                    {
                        authorUserId = Convert.ToInt32(authorUserIdStr);
                    }
                    authorUsername = authorNode.InnerText;
                }

                var floorPostInfoNode = postContentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").StartsWith("postinfo")); // div
                var floorLinkNode = floorPostInfoNode.ChildNodes[1].ChildNodes[0]; // a
                int postId = Convert.ToInt32(floorLinkNode.Attributes["id"].Value.Replace("postnum", string.Empty));
                var floorNumNode = floorLinkNode.ChildNodes[0]; // em
                int floor = Convert.ToInt32(floorNumNode.InnerText);
                string threadTitle = string.Empty;
                if (floor == 1)
                {
                    var threadTitleNode = postContentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("id", "").Equals("threadtitle"));
                    if (threadTitleNode != null)
                    {
                        threadTitle = threadTitleNode.InnerText.Trim();
                    }
                }

                string postTime = string.Empty;
                var postTimeNode = postContentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("id", "").StartsWith("authorposton")); // em
                if (postTimeNode != null)
                {
                    postTime = postTimeNode.InnerText
                        .Replace("发表于 ", string.Empty)
                        .Replace(string.Format("{0}-{1}-{2} ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), string.Empty)
                        .Replace(string.Format("{0}-", DateTime.Now.Year), string.Empty);
                }

                string textContent = string.Empty;
                string htmlContent = string.Empty;
                string xamlContent = string.Empty;
                int imageCount = 0;
                var contentNode = postContentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("t_msgfontfix"));
                if (contentNode != null)
                {
                    // 用于回复引用
                    textContent = contentNode.InnerText.Trim();
                    textContent = new Regex("\r\n").Replace(textContent, "↵");
                    textContent = new Regex("\r").Replace(textContent, "↵");
                    textContent = new Regex("\n").Replace(textContent, "↵");
                    textContent = new Regex(@"↵{1,}").Replace(textContent, "\r\n");
                    textContent = textContent.Replace("&nbsp;", "  ");

                    // 用于显示原始内容
                    htmlContent = contentNode.InnerHtml.Trim();

                    // 转换HTML为XAML
                    xamlContent = Html.HtmlToXaml.ConvertPost(threadId, htmlContent, 20, ref imageCount);
                }
                else
                {
                    xamlContent = @"<RichTextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><Paragraph>{0}</Paragraph></RichTextBlock>";
                    xamlContent = string.Format(xamlContent, @"作者被禁止或删除&#160;内容自动屏蔽");
                }

                ReplyItemModel reply = new ReplyItemModel(i, floor, postId, pageNo, threadId, threadTitle, 0, authorUserId, authorUsername, textContent, htmlContent, xamlContent, postTime, imageCount, targetPostId == postId);
                threadReply.Replies.Add(reply);

                i++;
            }

            int index = threadReply.Replies.Single(r => r.PostId == targetPostId).Index;
            return new int[] { pageNo, index, threadId };
        }

        public int GetReplyMaxPageNo()
        {
            return _replyMaxPageNo;
        }

        public int GetReplyMinPageNoInLoadedData(int threadId)
        {
            var data = _replyData.Single(d => d.ThreadId == threadId);
            return data.Replies.Min(r => r.PageNo);
        }

        public void ClearReplyData(int threadId)
        {
            _replyData.RemoveAll(t => t.ThreadId == threadId);
        }

        public void SetScrollState(bool isCompleted)
        {
            _isScrollCompleted = isCompleted;
        }

        public bool GetScrollState()
        {
            return _isScrollCompleted;
        }

        public bool CheckIsShowButtonForLoadPrevReplyPage(int threadId)
        {
            var data = _replyData.FirstOrDefault(d => d.ThreadId == threadId);
            if (data != null)
            {
                if (data.Replies.Min(r => r.PageNo) != 1)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<ReplyItemModel> GetPostDetail(int postId, int threadId)
        {
            var replyData = _replyData.FirstOrDefault(d => d.ThreadId == threadId);
            if (replyData != null)
            {
                var replyItemData = replyData.Replies.FirstOrDefault(r => r.PostId == postId);
                if (replyItemData != null)
                {
                    return replyItemData;
                }
            }

            // 由于回复列表页不一定是从第一页开始载入，所以会存在在缓存中找不到的情况
            // 故需要在此处作处理
            var cts = new CancellationTokenSource();
            await LoadReplyDataForRedirectReplyPageAsync(postId, cts);
            return _replyData.FirstOrDefault(d => d.ThreadId == threadId).Replies.FirstOrDefault(r => r.PostId == postId);
        }
        #endregion

        #region user
        static Dictionary<int, string> _userInfoData = new Dictionary<int, string>();

        async Task LoadUserDataAsync(int userId, CancellationTokenSource cts)
        {
            if (_userInfoData.ContainsKey(userId))
            {
                return;
            }

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/space.php?uid={0}&_={1}", userId, DateTime.Now.Ticks.ToString("x"));
            string htmlContent = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            // 最先读取提醒数据
            var promptContentNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("promptcontent"));
            GetPromptData(promptContentNode);

            var node = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("id", "").Equals("profilecontent"));
            if (node != null)
            {
                string xaml = Html.HtmlToXaml.ConvertUserInfo(node.InnerHtml);
                _userInfoData.Add(userId, xaml);
            }
        }

        public async Task<string> GetXamlForUserInfo(int userId)
        {
            if (!_userInfoData.ContainsKey(userId))
            {
                var cts = new CancellationTokenSource();
                await LoadUserDataAsync(userId, cts);
            }

            return _userInfoData[userId];
        }

        async Task<UserMessageDataModel> LoadUserMessageDataAsync(int userId, int limitCount, CancellationTokenSource cts)
        {
            var listData = new ObservableCollection<UserMessageItemModel>();
            int total = 0;

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/pm.php?uid={0}&filter=privatepm&daterange=5&_={1}", userId, DateTime.Now.Ticks.ToString("x"));
            string htmlContent = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            // 最先读取提醒数据
            var promptContentNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("promptcontent"));
            GetPromptData(promptContentNode);

            var messageListNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("pm_list"));
            if (messageListNode != null)
            {
                var nodeList = messageListNode.Descendants().Where(n => n.GetAttributeValue("id", "").StartsWith("pm_"));
                if (nodeList != null)
                {
                    total = nodeList.Count();
                    if (limitCount != -1 && total > limitCount)
                    {
                        nodeList = nodeList.Skip(total - limitCount);
                    }

                    foreach (var item in nodeList)
                    {
                        listData.Add(GetUserMessageItem(item));
                    }
                }
            }

            return new UserMessageDataModel { ListData = listData, Total = total };
        }

        private UserMessageItemModel GetUserMessageItem(HtmlNode htmlNode)
        {
            int uid = 0;
            var userIdNode = htmlNode.ChildNodes[3];
            var userInfoNode = htmlNode.ChildNodes[5];
            var messageNode = htmlNode.ChildNodes[7];
            string userIdStr = userIdNode.Attributes[0].Value;
            if (userIdStr.Equals("new"))
            {
                userIdNode = htmlNode.ChildNodes[4];
                userInfoNode = htmlNode.ChildNodes[6];
                messageNode = htmlNode.ChildNodes[8];
                userIdStr = userIdNode.Attributes[0].Value;
            }

            if (!userIdStr.Equals("avatar"))
            {
                userIdStr = userIdStr.Substring("space.php?uid=".Length);
                if (userIdStr.Contains("&"))
                {
                    uid = Convert.ToInt32(userIdStr.Split('&')[0]);
                }
                else
                {
                    uid = Convert.ToInt32(userIdStr);
                }
            }

            bool isRead = !userInfoNode.InnerHtml.Contains("notice_newpm.gif");
            string str = userInfoNode.InnerText.Trim().Replace("&nbsp;", string.Empty).Replace("\n", "$");
            string[] strAry = str.Split('$');
            string username = strAry[0].Trim();
            string time = strAry[1].Trim();
            string date = time.Split(' ')[0];

            string textStr = messageNode.InnerText;
            string htmlStr = messageNode.InnerHtml;
            string xamlStr = Html.HtmlToXaml.ConvertUserMessage(htmlStr);

            return new UserMessageItemModel
            {
                Date = date,
                Time = time,
                UserId = uid,
                Username = username,
                TextStr = textStr,
                HtmlStr = htmlStr,
                XamlStr = xamlStr,
                IsRead = isRead
            };
        }

        public async Task<UserMessageDataModel> GetUserMessageData(int userId, int limitCount)
        {
            var cts = new CancellationTokenSource();
            return await LoadUserMessageDataAsync(userId, limitCount, cts);
        }

        public async Task<UserMessageItemModel> PostUserMessage(string message, int userId)
        {
            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("handlekey", "pmreply"));
            postData.Add(new KeyValuePair<string, object>("lastdaterange", DateTime.Now.ToString("yyyy-MM-dd")));
            postData.Add(new KeyValuePair<string, object>("message", FaceService.FaceReplace(message)));

            string url = string.Format("http://www.hi-pda.com/forum/pm.php?action=send&uid={0}&pmsubmit=yes&_={1}", userId, DateTime.Now.Ticks.ToString("x"));
            var cts = new CancellationTokenSource();
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            if (resultContent.StartsWith(@"<?xml version=""1.0"" encoding=""gbk""?><root><![CDATA[<li id=""pm_") && resultContent.Contains(@"images/default/notice_newpm.gif"))
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(resultContent);
                string html = xdoc.ChildNodes[1].InnerText;

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                var htmlNode = doc.DocumentNode.ChildNodes[0];
                return GetUserMessageItem(htmlNode);
            }

            return null;
        }
        #endregion

        #region notice
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

                        data.Add(new NoticeItemModel(noticeType, isNew, username, actionTime, new string[] {
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

                        data.Add(new NoticeItemModel(noticeType, isNew, username, actionTime, new string[] {
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

                        data.Add(new NoticeItemModel(noticeType, isNew, username, actionTime, new string[] {
                            userId
                        }));
                        break;
                }
            }

            return data;
        }

        public async Task<List<NoticeItemModel>> GetNoticeData()
        {
            var cts = new CancellationTokenSource();
            return await LoadNoticeDataAsync(cts);
        }
        #endregion

        #region pm
        static List<UserMessageListItemModel> _userMessageListData = new List<UserMessageListItemModel>();
        int _userMessageListMaxPageNo = 1;

        async Task LoadUserMessageListDataAsync(int pageNo, CancellationTokenSource cts)
        {
            if (pageNo == 1)
            {
                // 如果是新打开，则清空所有短消息列表数据
                _userMessageListData.Clear();
            }

            if (pageNo > _userMessageListMaxPageNo)
            {
                return;
            }

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/pm.php?filter=privatepm&page={0}&_={1}", pageNo, DateTime.Now.Ticks.ToString("x"));
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            // 最先读取提醒数据
            var promptContentNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("promptcontent"));
            GetPromptData(promptContentNode);

            var items = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("ul") && n.GetAttributeValue("class", "").Equals("pm_list")).ChildNodes;
            if (items == null)
            {
                return;
            }

            // 读取最大页码
            var pagesNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("pages"));
            _userMessageListMaxPageNo = GetMaxPageNo(pagesNode);

            int i = _userMessageListData.Count;
            foreach (var item in items)
            {
                var linkNode = item.ChildNodes[3].ChildNodes[1].ChildNodes[0];
                int userId = Convert.ToInt32(linkNode.Attributes[0].Value.Substring("space.php?uid=".Length).Split('&')[0]);
                string username = linkNode.InnerText.Trim();
                string lastMessageTime = item.ChildNodes[3].ChildNodes[2].InnerText.Trim();
                string lastMessageText = item.ChildNodes[5].InnerText.Trim();

                var userMessageListItem = new UserMessageListItemModel(i, pageNo, userId, username, lastMessageTime, lastMessageText);
                _userMessageListData.Add(userMessageListItem);

                i++;
            }
        }

        public async Task<int> LoadMoreUserMessageListAsync(int pageNo, Action afterLoaded)
        {
            var cts = new CancellationTokenSource();
            await LoadUserMessageListDataAsync(pageNo, cts);

            if (afterLoaded != null)
            {
                afterLoaded();
            }

            return _userMessageListData.Count;
        }

        public static UserMessageListItemModel GetUserMessageListItemByIndex(int index)
        {
            return _userMessageListData.FirstOrDefault(li => li.Index == index);
        }

        public int GetUserMessageListMaxPageNo()
        {
            return _userMessageListMaxPageNo;
        }

        public ICollectionView GetViewForUserMessageList(int startPageNo, Action afterLoaded)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<UserMessageListItemModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await LoadMoreUserMessageListAsync(pageNo, afterLoaded);
                },
                (index) =>
                {
                    // 从静态类中返回需要显示出来的数据
                    return GetUserMessageListItemByIndex(index);
                },
                () =>
                {
                    return GetUserMessageListMaxPageNo();
                });

            return cvs.View;
        }

        public void ClearUserMessageListData()
        {
            _userMessageListData.Clear();
        }

        async Task<bool> DeleteUserMessageListItemAsync(List<int> delUserIdList, CancellationTokenSource cts)
        {
            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("readopt", "0"));
            foreach (int userId in delUserIdList)
            {
                postData.Add(new KeyValuePair<string, object>("uid[]", userId.ToString()));
            }

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/pm.php?action=del&filter=privatepm&page=1&_={0}", DateTime.Now.Ticks.ToString("x"));
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            if (resultContent.StartsWith("<!DOCTYPE html"))
            {
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteUserMessageListItem(List<int> delUserIdList)
        {
            var cts = new CancellationTokenSource();
            return await DeleteUserMessageListItemAsync(delUserIdList, cts);
        }
        #endregion
    }
}
