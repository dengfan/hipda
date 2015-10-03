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
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.Services
{
    public class DataService : IDataService
    {
        private static int _threadPageSize = 75;
        private static int _replyPageSize = 50;
        private static int _searchPageSize = 50;

        private static HttpHandle _httpClient = HttpHandle.getInstance();
        private static List<ThreadItemModel> _threadData = new List<ThreadItemModel>();
        private static int _threadMaxPageNo = 1;
        private static List<ReplyPageModel> _replyData = new List<ReplyPageModel>();
        private static int _replyMaxPageNo = 1;
        private static List<int> _read = new List<int>();

        #region thread
        private async Task LoadThreadDataAsync(int forumId, int pageNo, CancellationTokenSource cts)
        {
            int count = _threadData.Count(t => t.ForumId == forumId && t.PageNo == pageNo);
            if (count == _threadPageSize)
            {
                return;
            }
            else
            {
                _threadData.RemoveAll(t => t.ForumId == forumId && t.PageNo == pageNo);
            }

            // 读取数据
            string ThreadListPageOrderBy = string.Empty;
            string url = string.Format("http://www.hi-pda.com/forum/forumdisplay.php?fid={0}&orderby={1}&page={2}&_={3}", forumId, ThreadListPageOrderBy, pageNo, DateTime.Now.Ticks.ToString("x"));
            string htmlContent = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            var dataTable = doc.DocumentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").Equals("datatable"));
            if (dataTable == null)
            {
                return;
            }

            // 读取最大页码
            var pagesNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("pages"));
            if (pagesNode != null)
            {
                var nodeList = pagesNode.Descendants().Where(n => n.Name.Equals("a") || n.Name.Equals("strong")).ToList();
                nodeList.RemoveAll(n => n.InnerText.Equals("下一页"));
                string lastPageNodeValue = nodeList.Last().InnerText.Replace("... ", string.Empty);
                _threadMaxPageNo = Convert.ToInt32(lastPageNodeValue);
            }

            // 如果置顶贴数过多，只取非置顶贴的话，第一页数据项过少，会导致不会自动触发加载下一页数据
            var tbodies = dataTable.Descendants().Where(n => n.GetAttributeValue("id", "").Contains("stickthread_") || n.GetAttributeValue("id", "").Contains("normalthread_"));
            if (tbodies == null)
            {
                return;
            }

            int i = _threadData.Count(t => t.ForumId == forumId);
            foreach (var item in tbodies)
            {
                var tr = item.ChildNodes[1];
                var th = tr.ChildNodes[5];
                var span = th.Descendants().Single(n => n.GetAttributeValue("id", "").StartsWith("thread_"));
                var a = span.ChildNodes[0];
                var tdAuthor = tr.ChildNodes[7];
                var tdNums = tr.ChildNodes[9];
                var tdLastPost = tr.ChildNodes[11];

                int threadId = Convert.ToInt32(span.Attributes[0].Value.Substring("thread_".Length));
                string title = a.InnerText;

                int attachType = -1;
                var attachIconNode = th.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("attach"));
                if (attachIconNode != null)
                {
                    string attachString = attachIconNode.Attributes[1].Value;
                    if (attachString.Equals("图片附件"))
                    {
                        attachType = 1;
                    }

                    if (attachString.Equals("附件"))
                    {
                        attachType = 2;
                    }
                }

                var authorName = string.Empty;
                int authorUserId = 0;
                var authorNameNode = tdAuthor.ChildNodes[1]; // cite 此节点有出“匿名”的可能
                var authorNameLink = authorNameNode.Descendants().FirstOrDefault(n => n.Name.Equals("a"));
                if (authorNameLink == null)
                {
                    authorName = authorNameNode.InnerText.Trim();
                }
                else
                {
                    authorName = authorNameLink.InnerText;
                    string authorUserIdStr = authorNameLink.Attributes[0].Value.Substring("space.php?uid=".Length);
                    if (authorUserIdStr.Contains("&"))
                    {
                        authorUserId = Convert.ToInt32(authorUserIdStr.Split('&')[0]);
                    }
                    else
                    {
                        authorUserId = Convert.ToInt32(authorUserIdStr);
                    }
                }

                var authorCreateTime = tdAuthor.ChildNodes[3].InnerText;

                string[] nums = tdNums.InnerText.Split('/');
                var replyNum = nums[0].Trim();
                var viewNum = nums[1].Trim();

                string lastPostAuthorName = "匿名";
                string lastPostTime = string.Empty;
                string[] lastPostInfo = tdLastPost.InnerText.Trim().Replace("\n", "@").Split('@');
                if (lastPostInfo.Length == 2)
                {
                    lastPostAuthorName = lastPostInfo[0];
                    lastPostTime = lastPostInfo[1]
                        .Replace(string.Format("{0}-{1}-{2} ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), string.Empty)
                        .Replace(string.Format("{0}-", DateTime.Now.Year), string.Empty);
                }

                var threadItem = new ThreadItemModel(i, forumId, threadId, pageNo, title, attachType, replyNum, viewNum, authorName, authorUserId, authorCreateTime, lastPostAuthorName, lastPostTime);
                _threadData.Add(threadItem);

                i++;
            }
        }

        private async Task<int> LoadMoreThreadItemsAsync(int forumId, int pageNo, Action beforeLoad, Action afterLoad)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadThreadDataAsync(forumId, pageNo, cts);
            if (afterLoad != null) afterLoad();

            return _threadData.Count(t => t.ForumId == forumId);
        }

        /// <summary>
        /// 用于增量加载来控制要显示哪一项
        /// </summary>
        /// <param name="forumId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private ThreadItemViewModel GetThreadItemByIndex(int forumId, int index)
        {
            var threadItem = _threadData.FirstOrDefault(t => t.ForumId == forumId && t.Index == index);
            if (threadItem == null)
            {
                return null;
            }
            
            var threadItemViewModel = new ThreadItemViewModel(threadItem);
            threadItemViewModel.StatusColor = (SolidColorBrush)App.Current.Resources["SystemControlBackgroundAccentBrush"];
            if (_read.Contains(threadItem.ThreadId))
            {
                threadItemViewModel.StatusColor = new SolidColorBrush(Colors.White);
            }

            return threadItemViewModel;
        }

        public ICollectionView GetViewForThreadPage(int startPageNo, int forumId, Action beforeLoad, Action afterLoad)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass2<ThreadItemViewModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await LoadMoreThreadItemsAsync(forumId, pageNo, beforeLoad, afterLoad);
                },
                (index) =>
                {
                    // 从静态类中返回需要显示出来的数据
                    return GetThreadItemByIndex(forumId, index);
                },
                () =>
                {
                    return GetThreadMaxPageNo();
                });

            return cvs.View;
        }

        public int GetThreadMaxPageNo()
        {
            return _threadMaxPageNo;
        }

        public int GetThreadMinPageNoInLoadedData()
        {
            return _threadData.Min(t => t.PageNo);
        }

        public void ClearThreadData(int forumId)
        {
            _threadData.RemoveAll(t => t.ForumId == forumId);
        }

        public ThreadItemModel GetThreadItem(int threadId)
        {
            return _threadData.FirstOrDefault(t => t.ThreadId == threadId);
        }

        public void SetRead(int threadId)
        {
            if (!_read.Contains(threadId))
            {
                _read.Add(threadId);
            }
        }

        public bool IsRead(int threadId)
        {
            return _read.Contains(threadId);
        }
        #endregion

        #region reply
        private async Task LoadReplyDataAsync(int threadId, int threadAuthorUserId, int pageNo, CancellationTokenSource cts)
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
                var pagesNode = forumControlNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").Equals("pages"));
                if (pagesNode == null) // 没有超过两页
                {
                    return;
                }
                else
                {
                    var actualCurrentPageNode = pagesNode.Descendants().SingleOrDefault(n => n.NodeType == HtmlNodeType.Element && n.Name == "strong");
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

            var data = doc.DocumentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("id", "").Equals("postlist")).ChildNodes;
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
                var postAuthorNode = item.ChildNodes[0] // table
                        .ChildNodes[1] // tr
                        .ChildNodes[1]; // td.postauthor

                var postContentNode = item.ChildNodes[0] // table
                        .ChildNodes[1] // tr
                        .ChildNodes[3]; // td.postcontent

                int authorUserId = 0;
                string authorUsername = string.Empty;
                var authorNode = postAuthorNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").Equals("postinfo"));
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

                var floorPostInfoNode = postContentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").StartsWith("postinfo")); // div
                var floorLinkNode = floorPostInfoNode.ChildNodes[1].ChildNodes[0]; // a
                string postId = floorLinkNode.Attributes["id"].Value.Replace("postnum", string.Empty);
                var floorNumNode = floorLinkNode.ChildNodes[0]; // em
                int floor = Convert.ToInt32(floorNumNode.InnerText);
                string threadTitle = string.Empty;
                if (floor == 1)
                {
                    var threadTitleNode = postContentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("id", "").Equals("threadtitle"));
                    if (threadTitleNode != null)
                    {
                        threadTitle = threadTitleNode.InnerText.Trim();
                    }
                }

                string postTime = string.Empty;
                var postTimeNode = postContentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("id", "").StartsWith("authorposton")); // em
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
                var contentNode = postContentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").Equals("t_msgfontfix"));
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
                    xamlContent = Html.HtmlToXaml.Convert(htmlContent, 20, ref imageCount);
                }
                else
                {
                    xamlContent = @"<RichTextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><Paragraph>{0}</Paragraph></RichTextBlock>";
                    xamlContent = string.Format(xamlContent, @"作者被禁止或删除&#160;内容自动屏蔽");
                }

                ReplyItemModel reply = new ReplyItemModel(i, floor, postId, pageNo, threadId, threadTitle, threadAuthorUserId, authorUserId, authorUsername, textContent, htmlContent, xamlContent, imageCount, postTime);
                threadReply.Replies.Add(reply);

                i++;
            }
        }

        public async Task<int> LoadMoreReplyItemsAsync(int threadId, int threadAuthorUserId, int pageNo, Action beforeLoad, Action afterLoad)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadReplyDataAsync(threadId, threadAuthorUserId, pageNo, cts);
            if (afterLoad != null) afterLoad();

            return _replyData.Single(t => t.ThreadId == threadId).Replies.Count;
        }

        public static ReplyItemModel GetReplyItemByIndex(int threadId, int index)
        {
            return _replyData.Single(t => t.ThreadId == threadId).Replies[index];
        }

        public ICollectionView GetViewForReplyPage(int startPageNo, int threadId, int threadAuthorUserId, Action beforeLoad, Action afterLoad)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass2<ReplyItemModel>(
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
        #endregion
    }
}
