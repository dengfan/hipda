using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.ViewModels;
using Hipda.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.Services
{
    public class ReplyListService
    {
        static RoamingSettingsDependencyObject _myRoamingSettings = (RoamingSettingsDependencyObject)App.Current.Resources["MyRoamingSettings"];
        static HistoryThreadListViewViewModel _threadHistoryListBoxViewModel = (HistoryThreadListViewViewModel)App.Current.Resources["ThreadHistoryListBoxViewModel"];
        static List<ReplyPageModel> _replyData = new List<ReplyPageModel>();
        static Dictionary<int, string[]> _floorNoDic = new Dictionary<int, string[]>(); // 用于引用块的UI显示，如通过 postid 获取楼层信息
        static HttpHandle _httpClient = HttpHandle.GetInstance();
        static int _pageSize = 50;
        int _maxPageNo = 1;
        bool _isScrollCompleted = false;

        public static Dictionary<string, string> InAppLinkUrlDic = new Dictionary<string, string>();

        async Task LoadReplyDataAsync(int threadId, int pageNo, CancellationTokenSource cts)
        {
            if (pageNo > 1 && pageNo > _maxPageNo)
            {
                return;
            }

            int threadAuthorUserId = 0;
            string threadAuthorUsername = string.Empty;
            string threadTitle = string.Empty;

            // 如果页面已存在，则不重新从网站拉取数据，以便节省流量， 
            var threadReply = _replyData.FirstOrDefault(r => r.ThreadId == threadId);
            if (threadReply != null && threadReply.Replies.Count > 0)
            {
                #region 如果第一项没有贴子及作者数据，则加载第一页读取贴子标题及作者数据
                if (pageNo > 1)
                {
                    var item = threadReply.Replies.FirstOrDefault();
                    if (item == null || item.ThreadAuthorUserId == 0 || string.IsNullOrEmpty(item.ThreadTitle))
                    {
                        string val = await LoadTopReplyDataAsync(threadId, cts); 
                        string[] ary = val.Split(',');
                        threadAuthorUserId = Convert.ToInt32(ary[0]);
                        threadAuthorUsername = ary[1];
                        threadTitle = ary[2];
                    }
                    else
                    {
                        threadAuthorUserId = item.ThreadAuthorUserId;
                        threadAuthorUsername = item.AuthorUsername;
                        threadTitle = item.ThreadTitle;
                    }
                }
                #endregion

                var pageData = threadReply.Replies.Where(r => r.PageNo == pageNo);
                if (pageData != null && pageData.Count() > 0)
                {
                    var lastItem = pageData.Last();
                    if (lastItem != null)
                    {
                        if (lastItem.Index2 == _pageSize) // 当前页已加载满
                        {
                            return;
                        }
                        else
                        {
                            threadReply.Replies.RemoveAll(r => r.PageNo == pageNo);
                        }
                    }
                }
                else
                {
                    if (pageNo == 1 && threadReply.Replies.Count > 0)
                    {
                        threadReply.Replies.Clear();
                    }
                }
            }
            else
            {
                threadReply = new ReplyPageModel { ThreadId = threadId, Replies = new List<ReplyItemModel>() };
                _replyData.Add(threadReply);
            }

            // 先移除 IsLast 为 True 的标记项，确保此标记项永远只出现在最后面
            threadReply.Replies.RemoveAll(r => r.IsLast);

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/viewthread.php?tid={0}&page={1}&ordertype={2}&_={3}", threadId, pageNo, string.Empty, DateTime.Now.Ticks.ToString("x"));
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            // 最先读取提醒数据
            var promptContentNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("promptcontent"));
            PromptService.GetPromptData(promptContentNode);

            // 读取最大页码
            var pagesNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("pages"));
            _maxPageNo = CommonService.GetMaxPageNo(pagesNode);

            var data = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("id", "").Equals("postlist"))?.ChildNodes;
            if (data == null)
            {
                return;
            }

            // 读取所属版块之名称及ID
            int forumId = 0;
            string forumName = string.Empty;
            var navNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("id", "").Equals("nav"));
            if (navNode != null)
            {
                var forumLinkNode = navNode.ChildNodes[3];
                forumId = Convert.ToInt32(forumLinkNode.Attributes[0].Value.Substring("forumdisplay.php?fid=".Length).Split('&')[0]);
                forumName = forumLinkNode.InnerText.Trim();
            }

            int i = threadReply.Replies.Count();
            int j = 1;
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

                // 判断当前用户是否已被屏蔽，是则跳过
                if (_myRoamingSettings.BlockUsers.Any(u => u.UserId == authorUserId && u.ForumId == forumId))
                {
                    j++;
                    continue;
                }

                var floorPostInfoNode = postContentNode.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("class", "").StartsWith("postinfo")); // div
                var floorLinkNode = floorPostInfoNode.ChildNodes[1].ChildNodes[0]; // a
                int postId = Convert.ToInt32(floorLinkNode.Attributes["id"].Value.Replace("postnum", string.Empty));
                var floorNumNode = floorLinkNode.ChildNodes[0]; // em
                int floor = Convert.ToInt32(floorNumNode.InnerText);
                if (floor == 1)
                {
                    threadAuthorUserId = authorUserId;
                    threadAuthorUsername = authorUsername;
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
                        threadTitle = WebUtility.HtmlDecode(threadTitle);
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
                string xamlContent = string.Empty;
                int imageCount = 0;
                int inAppLinkCount = 0;
                var contentNode = postContentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && (n.GetAttributeValue("class", "").Equals("t_msgfontfix") || n.GetAttributeValue("class", "").Equals("specialmsg")));
                if (contentNode != null)
                {
                    // 用于回复引用
                    textContent = contentNode.InnerText.Trim();
                    textContent = new Regex("\r\n").Replace(textContent, "↵");
                    textContent = new Regex("\r").Replace(textContent, "↵");
                    textContent = new Regex("\n").Replace(textContent, "↵");
                    textContent = new Regex(@"↵{1,}").Replace(textContent, "\r\n");
                    textContent = textContent.Replace("&nbsp;", "  ");

                    // 转换HTML为XAML
                    var ary = Html.HtmlToXaml.ConvertPost(postId, threadId, forumId, forumName, contentNode.InnerHtml.Trim(), _floorNoDic, ref InAppLinkUrlDic);
                    xamlContent = ary[0];
                    inAppLinkCount = Convert.ToInt32(ary[1]);
                }
                else
                {
                    xamlContent = $@"<StackPanel xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><RichTextBlock><Paragraph>作者被禁止或删除&#160;内容自动屏蔽</Paragraph></RichTextBlock></StackPanel>";
                }

                var reply = new ReplyItemModel(i, j, floor, postId, pageNo, forumId, forumName, threadId, threadTitle, threadAuthorUserId, authorUserId, authorUsername, textContent, xamlContent, postTime, imageCount, false, inAppLinkCount);
                threadReply.Replies.Add(reply);

                if (!_floorNoDic.ContainsKey(postId))
                {
                    _floorNoDic.Add(postId, new string[] { authorUserId.ToString(), floor.ToString() });
                }

                i++;
                j++;
            }

            // 如果本次有加载到数据，则为数据列表的末尾添加一项“载入已完成”的标记项
            // 方便在加载完成时显示“---完---”
            // 注意在下一页开始加载前移除此标记项
            if (j > 1)
            {
                var lastItem = threadReply.Replies.Last();
                var flag = new ReplyItemModel(lastItem.Index + 1, lastItem.Index2, -1, -1, lastItem.PageNo, -1, string.Empty, lastItem.ThreadId, string.Empty, -1, -1, string.Empty, string.Empty, string.Empty, string.Empty, -1, false, 0);
                flag.IsLast = true;
                threadReply.Replies.Add(flag);
            }

            // 加入历史记录
            ApplicationView.GetForCurrentView().Title = $"{CommonService.ReplaceEmojiLabel(threadTitle)} - {CommonService.ReplaceEmojiLabel(forumName)}";
            _threadHistoryListBoxViewModel.Add(new ThreadItemModelBase { ThreadId = threadId, Title = threadTitle, ForumId = forumId, ForumName = forumName, AuthorUserId = threadAuthorUserId, AuthorUsername = threadAuthorUsername });
        }

        async Task<string> LoadTopReplyDataAsync(int threadId, CancellationTokenSource cts)
        {
            string threadTitle = string.Empty;
            int threadAuthorUserId = 0;
            string threadAuthorUsername = string.Empty;

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/viewthread.php?tid={0}&page=1", threadId);
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            var data = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("id", "").Equals("postlist"))?.ChildNodes;
            if (data == null)
            {
                return null;
            }

            var item = data[0];
            var mainTable = item.Descendants().FirstOrDefault(n => n.Name.Equals("table") && n.GetAttributeValue("summary", "").StartsWith("pid") && n.GetAttributeValue("id", "").Equals(n.GetAttributeValue("summary", "")));
            var postAuthorNode = mainTable // table
                    .ChildNodes[1] // tr
                    .ChildNodes[1]; // td.postauthor

            var postContentNode = mainTable // table
                    .ChildNodes[1] // tr
                    .ChildNodes[3]; // td.postcontent

            int authorUserId = 0;
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
                threadAuthorUserId = authorUserId;
                threadAuthorUsername = authorNode.InnerText.Trim();
            }

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
                threadTitle = WebUtility.HtmlDecode(threadTitle);
            }

            return $"{threadAuthorUserId},{threadAuthorUsername},{threadTitle}";
        }

        public async Task<int> LoadMoreReplyItemsAsync(int threadId, int pageNo, Action beforeLoad, Action<int, int> afterLoad)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadReplyDataAsync(threadId, pageNo, cts);
            if (afterLoad != null) afterLoad(threadId, pageNo);

            return _replyData.FirstOrDefault(t => t.ThreadId == threadId).Replies.Count;
        }

        public static ReplyItemModel GetReplyItemByIndex(int threadId, int index)
        {
            return _replyData.FirstOrDefault(t => t.ThreadId == threadId).Replies[index];
        }

        public ICollectionView GetViewForReplyPageByThreadId(int startPageNo, int threadId, Action beforeLoad, Action<int, int> afterLoad)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<ReplyItemModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await LoadMoreReplyItemsAsync(threadId, pageNo, beforeLoad, afterLoad);
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

        public ICollectionView GetViewForRedirectToPostByThreadId(int startPageNo, int threadId, int floorIndex, Action beforeLoad, Action<int, int> afterLoad, Action<int> listViewScroll)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<ReplyItemModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await LoadMoreReplyItemsAsync(threadId, pageNo, beforeLoad, afterLoad);
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
        public async Task<int[]> LoadReplyDataForRedirectToSpecifiedPostAsync(int targetPostId, CancellationTokenSource cts)
        {
            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/redirect.php?goto=findpost&pid={0}", targetPostId);
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            // 获取当前 thread id
            var postReplyLink = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("span") && n.GetAttributeValue("id", "").Equals("post_reply")).ChildNodes[0];
            string linkUrl = postReplyLink.Attributes[0].Value.Trim();
            string threadIdStr = linkUrl.Replace(";tid=", "$").Split('$')[1].Split('&')[0];
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
            var forumControlNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("forumcontrol s_clear"));
            var pagesNode = forumControlNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("pages"));
            if (pagesNode != null)
            {
                var nodeList = pagesNode.Descendants().Where(n => n.Name.Equals("a") || n.Name.Equals("strong")).ToList();
                nodeList.RemoveAll(n => n.InnerText.Equals("下一页"));
                string lastPageNodeValue = nodeList.Last().InnerText.Replace("... ", string.Empty);
                _maxPageNo = Convert.ToInt32(lastPageNodeValue);

                var currentPageNode = nodeList.FirstOrDefault(n => n.Name.Equals("strong"));
                if (currentPageNode != null)
                {
                    pageNo = Convert.ToInt32(currentPageNode.InnerText);
                }
            }

            int threadAuthorUserId = 0;
            string threadAuthorUsername = string.Empty;
            string threadTitle = string.Empty;

            #region 如果第一项没有贴子及作者数据，则加载第一页读取贴子标题及作者数据
            if (pageNo > 1)
            {
                var item = threadReply.Replies.FirstOrDefault();
                if (item == null || item.ThreadAuthorUserId == 0 || string.IsNullOrEmpty(item.ThreadTitle))
                {

                    string val = await LoadTopReplyDataAsync(threadId, cts); 
                    string[] ary = val.Split(',');
                    threadAuthorUserId = Convert.ToInt32(ary[0]);
                    threadAuthorUsername = ary[1];
                    threadTitle = ary[2];
                }
                else
                {
                    threadAuthorUserId = item.ThreadAuthorUserId;
                    threadAuthorUsername = item.AuthorUsername;
                    threadTitle = item.ThreadTitle;
                }
            }
            #endregion

            // 读取所属版块之名称及ID
            int forumId = 0;
            string forumName = string.Empty;
            var navNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("id", "").Equals("nav"));
            if (navNode != null)
            {
                var forumLinkNode = navNode.ChildNodes[3];
                forumId = Convert.ToInt32(forumLinkNode.Attributes[0].Value.Substring("forumdisplay.php?fid=".Length).Split('&')[0]);
                forumName = forumLinkNode.InnerText.Trim();
            }

            var data = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("id", "").Equals("postlist")).ChildNodes;
            int i = 0;
            int j = 1;
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

                // 判断当前用户是否已被屏蔽，是则跳过
                if (_myRoamingSettings.BlockUsers.Any(u => u.UserId == authorUserId && u.ForumId == forumId))
                {
                    j++;
                    continue;
                }

                var floorPostInfoNode = postContentNode.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("class", "").StartsWith("postinfo")); // div
                var floorLinkNode = floorPostInfoNode.ChildNodes[1].ChildNodes[0]; // a
                int postId = Convert.ToInt32(floorLinkNode.Attributes["id"].Value.Replace("postnum", string.Empty));
                var floorNumNode = floorLinkNode.ChildNodes[0]; // em
                int floor = Convert.ToInt32(floorNumNode.InnerText);
                if (floor == 1)
                {
                    threadAuthorUserId = authorUserId;
                    threadAuthorUsername = authorUsername;
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
                        threadTitle = WebUtility.HtmlDecode(threadTitle);
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
                string xamlContent = string.Empty;
                int imageCount = 0;
                int inAppLinkCount = 0;
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

                    // 转换HTML为XAML
                    var ary = Html.HtmlToXaml.ConvertPost(postId, threadId, forumId, forumName, contentNode.InnerHtml.Trim(), _floorNoDic, ref InAppLinkUrlDic);
                    xamlContent = ary[0];
                    inAppLinkCount = Convert.ToInt32(ary[1]);
                }
                else
                {
                    xamlContent = $@"<StackPanel xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><RichTextBlock><Paragraph>作者被禁止或删除&#160;内容自动屏蔽</Paragraph></RichTextBlock></StackPanel>";
                }

                var reply = new ReplyItemModel(i, j, floor, postId, pageNo, forumId, forumName, threadId, threadTitle, threadAuthorUserId, authorUserId, authorUsername, textContent, xamlContent, postTime, imageCount, targetPostId == postId, inAppLinkCount);
                threadReply.Replies.Add(reply);

                if (!_floorNoDic.ContainsKey(postId))
                {
                    _floorNoDic.Add(postId, new string[] { authorUserId.ToString(), floor.ToString() });
                }

                i++;
                j++;
            }

            // 如果本次有加载到数据，则为数据列表的末尾添加一项“载入已完成”的标记项
            // 方便在加载完成时显示“---完---”
            // 注意在下一页开始加载前移除此标记项
            if (j > 1)
            {
                var lastItem = threadReply.Replies.Last();
                var flag = new ReplyItemModel(lastItem.Index + 1, lastItem.Index2, -1, -1, lastItem.PageNo, -1, string.Empty, lastItem.ThreadId, string.Empty, -1, -1, string.Empty, string.Empty, string.Empty, string.Empty, -1, false, 0);
                flag.IsLast = true;
                threadReply.Replies.Add(flag);
            }

            // 加入历史记录
            ApplicationView.GetForCurrentView().Title = $"{CommonService.ReplaceEmojiLabel(threadTitle)} - {CommonService.ReplaceEmojiLabel(forumName)}";
            _threadHistoryListBoxViewModel.Add(new ThreadItemModelBase { ThreadId = threadId, Title = threadTitle, ForumId = forumId, ForumName = forumName, AuthorUserId = threadAuthorUserId, AuthorUsername = threadAuthorUsername });

            int index = threadReply.Replies.FirstOrDefault(r => r.PostId == targetPostId).Index;
            return new int[] { pageNo, index, threadId };
        }

        /// <summary>
        /// 用于“刷新到尾页”等功能
        /// 直接打开指定主题的最后一页加载数据并返回最后一个回复的Index等数据，以便直接跳到此回复
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        public async Task<int[]> LoadReplyDataForRedirectToLastPostAsync(int threadId, CancellationTokenSource cts)
        {
            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/viewthread.php?tid={0}&page=9999999&_={1}", threadId, DateTime.Now.Ticks.ToString("x"));
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            // 先清空本贴的回复数据，以便重新加载
            _replyData.RemoveAll(r => r.ThreadId == threadId);
            var threadReply = new ReplyPageModel { ThreadId = threadId, Replies = new List<ReplyItemModel>() };
            _replyData.Add(threadReply);

            // 获取当前页码，及最大页码
            int pageNo = 1;
            var forumControlNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("forumcontrol s_clear"));
            var pagesNode = forumControlNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("pages"));
            if (pagesNode != null)
            {
                var nodeList = pagesNode.Descendants().Where(n => n.Name.Equals("a") || n.Name.Equals("strong")).ToList();
                nodeList.RemoveAll(n => n.InnerText.Equals("下一页"));
                string lastPageNodeValue = nodeList.Last().InnerText.Replace("... ", string.Empty);
                _maxPageNo = Convert.ToInt32(lastPageNodeValue);

                var currentPageNode = nodeList.FirstOrDefault(n => n.Name.Equals("strong"));
                if (currentPageNode != null)
                {
                    pageNo = Convert.ToInt32(currentPageNode.InnerText);
                }
            }

            int threadAuthorUserId = 0;
            string threadAuthorUsername = string.Empty;
            string threadTitle = string.Empty;

            #region 如果第一项没有贴子及作者数据，则加载第一页读取贴子标题及作者数据
            if (pageNo > 1)
            {
                var item = threadReply.Replies.FirstOrDefault();
                if (item == null || item.ThreadAuthorUserId == 0 || string.IsNullOrEmpty(item.ThreadTitle))
                {

                    string val = await LoadTopReplyDataAsync(threadId, cts);
                    string[] ary = val.Split(',');
                    threadAuthorUserId = Convert.ToInt32(ary[0]);
                    threadAuthorUsername = ary[1];
                    threadTitle = ary[2];
                }
                else
                {
                    threadAuthorUserId = item.ThreadAuthorUserId;
                    threadAuthorUsername = item.AuthorUsername;
                    threadTitle = item.ThreadTitle;
                }
            }
            #endregion

            // 读取所属版块之名称及ID
            int forumId = 0;
            string forumName = string.Empty;
            var navNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("id", "").Equals("nav"));
            if (navNode != null)
            {
                var forumLinkNode = navNode.ChildNodes[3];
                forumId = Convert.ToInt32(forumLinkNode.Attributes[0].Value.Substring("forumdisplay.php?fid=".Length).Split('&')[0]);
                forumName = forumLinkNode.InnerText.Trim();
            }

            var data = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("id", "").Equals("postlist")).ChildNodes;
            int i = 0;
            int j = 1;
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

                // 判断当前用户是否已被屏蔽，是则跳过
                if (_myRoamingSettings.BlockUsers.Any(u => u.UserId == authorUserId && u.ForumId == forumId))
                {
                    j++;
                    continue;
                }

                var floorPostInfoNode = postContentNode.ChildNodes.FirstOrDefault(n => n.GetAttributeValue("class", "").StartsWith("postinfo")); // div
                var floorLinkNode = floorPostInfoNode.ChildNodes[1].ChildNodes[0]; // a
                int postId = Convert.ToInt32(floorLinkNode.Attributes["id"].Value.Replace("postnum", string.Empty));
                var floorNumNode = floorLinkNode.ChildNodes[0]; // em
                int floor = Convert.ToInt32(floorNumNode.InnerText);
                if (floor == 1)
                {
                    threadAuthorUserId = authorUserId;
                    threadAuthorUsername = authorUsername;
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
                        threadTitle = WebUtility.HtmlDecode(threadTitle);
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
                string xamlContent = string.Empty;
                int imageCount = 0;
                int inAppLinkCount = 0;
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

                    // 转换HTML为XAML
                    var ary = Html.HtmlToXaml.ConvertPost(postId, threadId, forumId, forumName, contentNode.InnerHtml.Trim(), _floorNoDic, ref InAppLinkUrlDic);
                    xamlContent = ary[0];
                    inAppLinkCount = Convert.ToInt32(ary[1]);
                }
                else
                {
                    xamlContent = $@"<StackPanel xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><RichTextBlock><Paragraph>作者被禁止或删除&#160;内容自动屏蔽</Paragraph></RichTextBlock></StackPanel>";
                }

                var reply = new ReplyItemModel(i, j, floor, postId, pageNo, forumId, forumName, threadId, threadTitle, threadAuthorUserId, authorUserId, authorUsername, textContent, xamlContent, postTime, imageCount, false, inAppLinkCount);
                threadReply.Replies.Add(reply);

                if (!_floorNoDic.ContainsKey(postId))
                {
                    _floorNoDic.Add(postId, new string[] { authorUserId.ToString(), floor.ToString() });
                }

                i++;
                j++;
            }

            // 如果本次有加载到数据，则为数据列表的末尾添加一项“载入已完成”的标记项
            // 方便在加载完成时显示“---完---”
            // 注意在下一页开始加载前移除此标记项
            if (i > 0)
            {
                var lastItem = threadReply.Replies.Last();
                var flag = new ReplyItemModel(lastItem.Index + 1, lastItem.Index2, -1, -1, lastItem.PageNo, -1, string.Empty, lastItem.ThreadId, string.Empty, -1, -1, string.Empty, string.Empty, string.Empty, string.Empty, -1, false, 0);
                flag.IsLast = true;
                threadReply.Replies.Add(flag);
            }

            // 加入历史记录
            ApplicationView.GetForCurrentView().Title = $"{CommonService.ReplaceEmojiLabel(threadTitle)} - {CommonService.ReplaceEmojiLabel(forumName)}";
            _threadHistoryListBoxViewModel.Add(new ThreadItemModelBase { ThreadId = threadId, Title = threadTitle, ForumId = forumId, ForumName = forumName, AuthorUserId = threadAuthorUserId, AuthorUsername = threadAuthorUsername });

            int index = threadReply.Replies.Last().Index;
            return new int[] { pageNo, index, threadId };
        }

        public int GetReplyMaxPageNo()
        {
            return _maxPageNo;
        }

        public int GetReplyMinPageNoInLoadedData(int threadId)
        {
            var data = _replyData.FirstOrDefault(d => d.ThreadId == threadId);
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

        public static bool CanShowButtonForLoadPrevReplyPage(int threadId)
        {
            var data = _replyData.FirstOrDefault(d => d.ThreadId == threadId);
            if (data != null && data.Replies != null && data.Replies.Count > 0)
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
            await LoadReplyDataForRedirectToSpecifiedPostAsync(postId, cts);
            return _replyData.FirstOrDefault(d => d.ThreadId == threadId).Replies.FirstOrDefault(r => r.PostId == postId);
        }

        public string GetThreadTitle(int threadId)
        {
            var data = _replyData.FirstOrDefault(d => d.ThreadId == threadId);
            if (data != null)
            {
                return data.Replies[0].ThreadTitle;
            }

            return string.Empty;
        }
    }
}
