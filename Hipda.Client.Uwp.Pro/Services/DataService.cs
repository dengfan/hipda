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
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.Services
{
    public partial class DataService : IDataService
    {
        static int _threadPageSize = 75;
        static int _replyPageSize = 50;
        static int _searchPageSize = 50;

        static HttpHandle _httpClient = HttpHandle.getInstance();

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

        public async Task<bool> DeleteThreadForMyFavoritesAsync(List<int> deleteThreadIds)
        {
            if (deleteThreadIds.Count == 0)
            {
                return false;
            }

            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("favsubmit", "true"));
            foreach (var tid in deleteThreadIds)
            {
                postData.Add(new KeyValuePair<string, object>("delete[]", tid));
            }

            string url = string.Format("http://www.hi-pda.com/forum/my.php?item=favorites&type=thread&_={0}", DateTime.Now.Ticks.ToString("x"));
            var cts = new CancellationTokenSource();
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            return resultContent.Contains("收藏夹已成功更新，现在将转入更新后的收藏夹。");
        }
        #endregion

        #region reply
        static List<ReplyPageModel> _replyData = new List<ReplyPageModel>();
        int _replyMaxPageNo = 1;
        bool _isScrollCompleted = false;

        async Task LoadReplyDataAsync(int threadId, int threadAuthorUserId, int pageNo, CancellationTokenSource cts)
        {
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

            #region 先判断页码是否已超过最大页码，以免造成重复加载
            if (pageNo > 1)
            {
                var forumControlNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("forumcontrol s_clear"));
                var pagesNode = forumControlNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("pages"));
                if (pagesNode == null) // 没有超过两页
                {
                    return;
                }
                else
                {
                    var actualCurrentPageNode = pagesNode.Descendants().FirstOrDefault(n => n.NodeType == HtmlNodeType.Element && n.Name == "strong");
                    if (actualCurrentPageNode != null)
                    {
                        int currentPage = Convert.ToInt32(actualCurrentPageNode.InnerText);
                        if (pageNo > currentPage)
                        {
                            return;
                        }
                    }
                }
            }
            #endregion

            var data = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("id", "").Equals("postlist")).ChildNodes;
            if (data == null)
            {
                return;
            }

            // 读取最大页码
            var pagesNode2 = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("pages"));
            if (pagesNode2 != null)
            {
                var nodeList = pagesNode2.Descendants().Where(n => n.Name.Equals("a") || n.Name.Equals("strong")).ToList();
                nodeList.RemoveAll(n => n.InnerText.Equals("下一页"));
                string lastPageNodeValue = nodeList.Last().InnerText.Replace("... ", string.Empty);
                _replyMaxPageNo = Convert.ToInt32(lastPageNodeValue);
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

                ReplyItemModel reply = new ReplyItemModel(i, floor, postId, pageNo, threadId, threadTitle, threadAuthorUserId, authorUserId, authorUsername, textContent, htmlContent, xamlContent, postTime, imageCount);
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

        public ICollectionView GetViewForReplyPage(int startPageNo, int threadId, int threadAuthorUserId, Action beforeLoad, Action<int, int> afterLoad)
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

        public ICollectionView GetViewForReplyPage(int startPageNo, int threadId, int threadAuthorUserId, int floorIndex, Action beforeLoad, Action<int, int> afterLoad, Action<int> listViewScroll)
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
        /// 用于载入我的回复，并返回回复所在楼层及所在页码
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="targetPostId"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public async Task<int[]> LoadReplyDataForRedirectPageAsync(int threadId, int targetPostId, CancellationTokenSource cts)
        {
            // 先清空本贴的回复数据，以便重新加载
            _replyData.RemoveAll(r => r.ThreadId == threadId);
            var threadReply = new ReplyPageModel { ThreadId = threadId, Replies = new List<ReplyItemModel>() };
            _replyData.Add(threadReply);

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/redirect.php?goto=findpost&pid={0}&ptid={1}&_={2}", targetPostId, threadId, DateTime.Now.Ticks.ToString("x"));
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            var data = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("id", "").Equals("postlist")).ChildNodes;

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

                ReplyItemModel reply = new ReplyItemModel(i, floor, postId, pageNo, threadId, threadTitle, 0, authorUserId, authorUsername, textContent, htmlContent, xamlContent, postTime, imageCount);
                threadReply.Replies.Add(reply);

                i++;
            }

            int index = threadReply.Replies.Single(r => r.PostId == targetPostId).Index;
            return new int[] { pageNo, index };
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
            await LoadReplyDataForRedirectPageAsync(threadId, postId, cts);
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

            var node = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("id", "").Equals("profilecontent"));
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

        async Task<List<UserMessageItemModel>> LoadUserMessageDataAsync(int userId, int limitCount, CancellationTokenSource cts)
        {
            var data = new List<UserMessageItemModel>();

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/pm.php?uid={0}&filter=privatepm&daterange=5&_={1}", userId, DateTime.Now.Ticks.ToString("x"));
            string htmlContent = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            var messageListNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("pm_list"));
            if (messageListNode != null)
            {
                var nodeList = messageListNode.Descendants().Where(n => n.GetAttributeValue("id", "").StartsWith("pm_"));
                if (nodeList != null)
                {
                    int total = nodeList.Count();
                    if (limitCount != -1 && total > limitCount)
                    {
                        nodeList = nodeList.Skip(total - limitCount);
                    }

                    foreach (var item in nodeList)
                    {
                        int uid = 0;
                        var userIdNode = item.ChildNodes[3];
                        var userInfoNode = item.ChildNodes[5];
                        var messageNode = item.ChildNodes[7];
                        string userIdStr = userIdNode.Attributes[0].Value;
                        if (userIdStr.Equals("new"))
                        {
                            userIdNode = item.ChildNodes[4];
                            userInfoNode = item.ChildNodes[6];
                            messageNode = item.ChildNodes[8];
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

                        var messageItem = new UserMessageItemModel
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
                        data.Add(messageItem);
                    }
                }
            }

            return data;
        }

        public async Task<List<UserMessageItemModel>> GetUserMessageData(int userId, int limitCount)
        {
            var cts = new CancellationTokenSource();
            return await LoadUserMessageDataAsync(userId, limitCount, cts);
        }

        public async Task<bool> PostUserMessage(string message, int userId)
        {
            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("handlekey", "pmreply"));
            postData.Add(new KeyValuePair<string, object>("lastdaterange", DateTime.Now.ToString("yyyy-MM-dd")));
            postData.Add(new KeyValuePair<string, object>("message", FaceService.FaceReplace(message)));

            string url = string.Format("http://www.hi-pda.com/forum/pm.php?action=send&uid={0}&pmsubmit=yes&_={1}", userId, DateTime.Now.Ticks.ToString("x"));
            var cts = new CancellationTokenSource();
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            return resultContent.StartsWith(@"<?xml version=""1.0"" encoding=""gbk""?><root><![CDATA[<li id=""pm_") && resultContent.Contains(@"images/default/notice_newpm.gif");
        }
        #endregion

    }
}
