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
    public class DataService : IDataService
    {
        private static int _threadPageSize = 75;
        private static int _replyPageSize = 50;
        private static int _searchPageSize = 50;

        private static HttpHandle _httpClient = HttpHandle.getInstance();

        #region thread
        public static ObservableCollection<ThreadItemModelBase> ReadHistoryData = new ObservableCollection<ThreadItemModelBase>();
        private static List<ThreadItemModel> _threadData = new List<ThreadItemModel>();
        private static List<ThreadItemForMyThreadsModel> _threadDataForMyThreads = new List<ThreadItemForMyThreadsModel>();
        private static List<ThreadItemForMyPostsModel> _threadDataForMyPosts = new List<ThreadItemForMyPostsModel>();
        private int _threadMaxPageNo = 1;
        private int _threadMaxPageNoForMyThreads = 1;
        private int _threadMaxPageNoForMyPosts = 1;

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

                bool isTop = item.GetAttributeValue("id", "").StartsWith("stickthread_");

                int threadId = Convert.ToInt32(span.Attributes[0].Value.Substring("thread_".Length));
                string title = a.InnerText.Trim();

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

                var threadItem = new ThreadItemModel(i, forumId, threadId, pageNo, title, attachType, replyNum, viewNum, isTop, authorName, authorUserId, authorCreateTime, lastPostAuthorName, lastPostTime);
                _threadData.Add(threadItem);

                i++;
            }
        }

        private async Task LoadThreadDataForMyThreadsAsync(int pageNo, CancellationTokenSource cts)
        {
            int count = _threadDataForMyThreads.Count(t => t.PageNo == pageNo);
            if (count == _threadPageSize)
            {
                return;
            }
            else
            {
                _threadDataForMyThreads.RemoveAll(t => t.PageNo == pageNo);
            }

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/my.php?item=threads&page={0}&_={1}", pageNo, DateTime.Now.Ticks.ToString("x"));
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
                _threadMaxPageNoForMyThreads = Convert.ToInt32(lastPageNodeValue);
            }

            // 如果置顶贴数过多，只取非置顶贴的话，第一页数据项过少，会导致不会自动触发加载下一页数据
            var rows = dataTable.ChildNodes[3].Descendants().Where(n => n.Name.Equals("tr"));
            if (rows == null)
            {
                return;
            }

            int i = _threadDataForMyThreads.Count;
            foreach (var item in rows)
            {
                var th = item.Descendants().FirstOrDefault(n => n.Name.Equals("th"));
                var a = th.Descendants().FirstOrDefault(n => n.Name.Equals("a"));
                string threadName = a.InnerText.Trim();
                int threadId = Convert.ToInt32(a.GetAttributeValue("href", "").Substring("viewthread.php?tid=".Length));

                var forumNameNode = item.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("forum"));
                string forumName = forumNameNode.InnerText.Trim();

                var lastPostNode = item.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("lastpost"));
                string lastPostAuthorName = "匿名";
                string lastPostTime = string.Empty;
                string[] lastPostInfo = lastPostNode.InnerText.Trim().Replace("\n", "@").Split('@');
                if (lastPostInfo.Length == 2)
                {
                    lastPostAuthorName = lastPostInfo[0];
                    lastPostTime = lastPostInfo[1]
                        .Replace(string.Format("{0}-{1}-{2} ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), string.Empty)
                        .Replace(string.Format("{0}-", DateTime.Now.Year), string.Empty);
                }

                var threadItem = new ThreadItemForMyThreadsModel(i, forumName, threadId, pageNo, threadName, lastPostAuthorName, lastPostTime);
                _threadDataForMyThreads.Add(threadItem);

                i++;
            }
        }

        private async Task LoadThreadDataForMyPostsAsync(int pageNo, CancellationTokenSource cts)
        {
            int count = _threadDataForMyPosts.Count(t => t.PageNo == pageNo);
            if (count == _threadPageSize)
            {
                return;
            }
            else
            {
                _threadDataForMyPosts.RemoveAll(t => t.PageNo == pageNo);
            }

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/my.php?item=posts&page={0}&_={1}", pageNo, DateTime.Now.Ticks.ToString("x"));
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
                _threadMaxPageNoForMyPosts = Convert.ToInt32(lastPageNodeValue);
            }

            // 如果置顶贴数过多，只取非置顶贴的话，第一页数据项过少，会导致不会自动触发加载下一页数据
            var rows = dataTable.ChildNodes[3].Descendants().Where(n => n.Name.Equals("tr")).ToList();
            if (rows == null)
            {
                return;
            }

            int i = _threadDataForMyPosts.Count;
            for (int j = 0; j < _threadPageSize * 2; j += 2)
            {
                var tr = rows[j];
                var tr2 = rows[j + 1];

                var th = tr.Descendants().FirstOrDefault(n => n.Name.Equals("th"));
                var a = th.Descendants().FirstOrDefault(n => n.Name.Equals("a"));
                string threadName = a.InnerText.Trim();
                string[] pramaStr = a.GetAttributeValue("href", "").Substring("redirect.php?goto=findpost&amp;pid=".Length).Replace("&amp;ptid=", ",").Split(',');
                int threadId = Convert.ToInt32(pramaStr[1]);
                int postId = Convert.ToInt32(pramaStr[0]);

                var th2 = tr2.Descendants().FirstOrDefault(n => n.Name.Equals("th"));
                string lastPostContent = th2.InnerText.Trim();

                var forumNameNode = tr.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("forum"));
                string forumName = forumNameNode.InnerText.Trim();

                var lastPostNode = tr.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("lastpost"));
                string lastPostTime = lastPostNode.InnerText.Trim();
                lastPostTime = lastPostTime
                        .Replace(string.Format("{0}-{1}-{2} ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), string.Empty)
                        .Replace(string.Format("{0}-", DateTime.Now.Year), string.Empty);

                var threadItem = new ThreadItemForMyPostsModel(i, forumName, threadId, postId, pageNo, threadName, lastPostContent, lastPostTime);
                _threadDataForMyPosts.Add(threadItem);

                i++;
            }
        }


        private async Task<int> GetMoreThreadItemsAsync(int forumId, int pageNo, Action beforeLoad, Action afterLoad)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadThreadDataAsync(forumId, pageNo, cts);
            if (afterLoad != null) afterLoad();

            return _threadData.Count(t => t.ForumId == forumId);
        }

        private ThreadItemViewModel GetOneThreadItem(int forumId, int index)
        {
            var threadItem = _threadData.FirstOrDefault(t => t.ForumId == forumId && t.Index == index);
            if (threadItem == null)
            {
                return null;
            }
            
            var threadItemViewModel = new ThreadItemViewModel(threadItem);
            threadItemViewModel.StatusColorStyle = (Style)App.Current.Resources["UnReadColorStyle"];
            if (IsRead(threadItem.ThreadId))
            {
                threadItemViewModel.StatusColorStyle = (Style)App.Current.Resources["ReadColorStyle"];
            }

            return threadItemViewModel;
        }

        private async Task<int> GetMoreThreadItemsForMyThreadsAsync(int pageNo, Action beforeLoad, Action afterLoad)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadThreadDataForMyThreadsAsync(pageNo, cts);
            if (afterLoad != null) afterLoad();

            return _threadDataForMyThreads.Count;
        }

        private ThreadItemForMyThreadsViewModel GetOneThreadItemForMyThreads(int index)
        {
            var threadItem = _threadDataForMyThreads.FirstOrDefault(t => t.Index == index);
            if (threadItem == null)
            {
                return null;
            }

            return new ThreadItemForMyThreadsViewModel(threadItem);
        }

        private async Task<int> GetMoreThreadItemsForMyPostsAsync(int pageNo, Action beforeLoad, Action afterLoad)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadThreadDataForMyPostsAsync(pageNo, cts);
            if (afterLoad != null) afterLoad();

            return _threadDataForMyPosts.Count;
        }

        private ThreadItemForMyPostsViewModel GetOneThreadItemForMyPosts(int index)
        {
            var threadItem = _threadDataForMyPosts.FirstOrDefault(t => t.Index == index);
            if (threadItem == null)
            {
                return null;
            }

            return new ThreadItemForMyPostsViewModel(threadItem);
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
                    return await GetMoreThreadItemsAsync(forumId, pageNo, beforeLoad, afterLoad);
                },
                (index) =>
                {
                    // 从静态类中返回需要显示出来的数据
                    return GetOneThreadItem(forumId, index);
                },
                () =>
                {
                    return GetThreadMaxPageNo();
                });

            return cvs.View;
        }

        public ICollectionView GetViewForThreadPageForMyThreads(int startPageNo, Action beforeLoad, Action afterLoad)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass2<ThreadItemForMyThreadsViewModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await GetMoreThreadItemsForMyThreadsAsync(pageNo, beforeLoad, afterLoad);
                },
                (index) =>
                {
                    // 从静态类中返回需要显示出来的数据
                    return GetOneThreadItemForMyThreads(index);
                },
                () =>
                {
                    return GetThreadMaxPageNoForMyThreads();
                });

            return cvs.View;
        }

        public ICollectionView GetViewForThreadPageForMyPosts(int startPageNo, Action beforeLoad, Action afterLoad)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass2<ThreadItemForMyPostsViewModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await GetMoreThreadItemsForMyPostsAsync(pageNo, beforeLoad, afterLoad);
                },
                (index) =>
                {
                    // 从静态类中返回需要显示出来的数据
                    return GetOneThreadItemForMyPosts(index);
                },
                () =>
                {
                    return GetThreadMaxPageNoForMyPosts();
                });

            return cvs.View;
        }


        public int GetThreadMaxPageNo()
        {
            return _threadMaxPageNo;
        }

        public int GetThreadMaxPageNoForMyThreads()
        {
            return _threadMaxPageNoForMyThreads;
        }

        public int GetThreadMaxPageNoForMyPosts()
        {
            return _threadMaxPageNoForMyPosts;
        }


        public int GetThreadMinPageNoInLoadedData()
        {
            return _threadData.Min(t => t.PageNo);
        }

        public int GetThreadMinPageNoForMyThreadsInLoadedData()
        {
            return _threadDataForMyThreads.Min(t => t.PageNo);
        }

        public int GetThreadMinPageNoForMyPostsInLoadedData()
        {
            return _threadDataForMyPosts.Min(t => t.PageNo);
        }


        public void ClearThreadData(int forumId)
        {
            _threadData.RemoveAll(t => t.ForumId == forumId);
        }

        public void ClearThreadDataForMyThreads()
        {
            _threadDataForMyThreads.Clear();
        }

        public void ClearThreadDataForMyPosts()
        {
            _threadDataForMyPosts.Clear();
        }


        public ThreadItemModel GetThreadItem(int threadId)
        {
            return _threadData.FirstOrDefault(t => t.ThreadId == threadId);
        }

        public ThreadItemForMyThreadsModel GetThreadItemForMyThreads(int threadId)
        {
            return _threadDataForMyThreads.FirstOrDefault(t => t.ThreadId == threadId);
        }

        public ThreadItemForMyPostsModel GetThreadItemForMyPosts(int threadId)
        {
            return _threadDataForMyPosts.FirstOrDefault(t => t.ThreadId == threadId);
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
        private static List<ReplyPageModel> _replyData = new List<ReplyPageModel>();
        private int _replyMaxPageNo = 1;
        private bool _isScrollCompleted = false;

        private async Task LoadReplyDataAsync(int threadId, int threadAuthorUserId, int pageNo, Action<int> linkClickEvent, CancellationTokenSource cts)
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
                int postId = Convert.ToInt32(floorLinkNode.Attributes["id"].Value.Replace("postnum", string.Empty));
                var floorNumNode = floorLinkNode.ChildNodes[0]; // em
                int floor = Convert.ToInt32(floorNumNode.InnerText);
                string threadTitle = string.Empty;
                if (floor == 1)
                {
                    var threadTitleNode = postContentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("id", "").Equals("threadtitle"));
                    if (threadTitleNode != null)
                    {
                        threadTitle = threadTitleNode.ChildNodes[1].InnerText.Trim();
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
                int linkCount = 0;
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
                    xamlContent = Html.HtmlToXaml.ConvertPost(threadId, htmlContent, 20, ref imageCount, ref linkCount);
                }
                else
                {
                    xamlContent = @"<RichTextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><Paragraph>{0}</Paragraph></RichTextBlock>";
                    xamlContent = string.Format(xamlContent, @"作者被禁止或删除&#160;内容自动屏蔽");
                }

                ReplyItemModel reply = new ReplyItemModel(i, floor, postId, pageNo, threadId, threadTitle, threadAuthorUserId, authorUserId, authorUsername, textContent, htmlContent, xamlContent, postTime, imageCount, linkCount, linkClickEvent);
                threadReply.Replies.Add(reply);

                i++;
            }
        }

        public async Task<int> LoadMoreReplyItemsAsync(int threadId, int threadAuthorUserId, int pageNo, Action beforeLoad, Action<int> afterLoad, Action<int> linkClickEvent)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadReplyDataAsync(threadId, threadAuthorUserId, pageNo, linkClickEvent, cts);
            if (afterLoad != null) afterLoad(threadId);

            return _replyData.Single(t => t.ThreadId == threadId).Replies.Count;
        }

        public static ReplyItemModel GetReplyItemByIndex(int threadId, int index)
        {
            return _replyData.Single(t => t.ThreadId == threadId).Replies[index];
        }

        public ICollectionView GetViewForReplyPage(int startPageNo, int threadId, int threadAuthorUserId, Action beforeLoad, Action<int> afterLoad, Action<int> linkClickEvent)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass2<ReplyItemModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await LoadMoreReplyItemsAsync(threadId, threadAuthorUserId, pageNo, beforeLoad, afterLoad, linkClickEvent);
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

        public ICollectionView GetViewForReplyPage(int startPageNo, int threadId, int threadAuthorUserId, int floorIndex, Action beforeLoad, Action<int> afterLoad, Action<int> listViewScroll, Action<int> linkClickEvent)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass2<ReplyItemModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await LoadMoreReplyItemsAsync(threadId, threadAuthorUserId, pageNo, beforeLoad, afterLoad, linkClickEvent);
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

        public async Task<int[]> LoadReplyDataForRedirectPageAsync(int threadId, int targetPostId, Action<int> linkClickEvent, CancellationTokenSource cts)
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

            var data = doc.DocumentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("id", "").Equals("postlist")).ChildNodes;

            // 获取当前页码，及最大页码
            int pageNo = 1;
            var forumControlNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("forumcontrol s_clear"));
            var pagesNode = forumControlNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").Equals("pages"));
            if (pagesNode != null)
            {
                var nodeList = pagesNode.Descendants().Where(n => n.Name.Equals("a") || n.Name.Equals("strong")).ToList();
                nodeList.RemoveAll(n => n.InnerText.Equals("下一页"));
                string lastPageNodeValue = nodeList.Last().InnerText.Replace("... ", string.Empty);
                _replyMaxPageNo = Convert.ToInt32(lastPageNodeValue);

                var currentPageNode = nodeList.SingleOrDefault(n => n.Name.Equals("strong"));
                if (currentPageNode != null)
                {
                    pageNo = Convert.ToInt32(currentPageNode.InnerText);
                }
            }

            int i = 0;
            foreach (var item in data)
            {
                var tableRowNode = item.ChildNodes[0].ChildNodes[1]; // tr

                var postAuthorNode = tableRowNode.ChildNodes[1]; // td.postauthor
                if (string.IsNullOrEmpty(postAuthorNode.InnerText))
                {
                    tableRowNode = item.ChildNodes[0].ChildNodes[3]; // tr
                    postAuthorNode = tableRowNode.ChildNodes[1]; // td.postauthor
                }

                var postContentNode = tableRowNode.ChildNodes[3]; // td.postcontent

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
                int postId = Convert.ToInt32(floorLinkNode.Attributes["id"].Value.Replace("postnum", string.Empty));
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
                int linkCount = 0;
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
                    xamlContent = Html.HtmlToXaml.ConvertPost(threadId, htmlContent, 20, ref imageCount, ref linkCount);
                }
                else
                {
                    xamlContent = @"<RichTextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><Paragraph>{0}</Paragraph></RichTextBlock>";
                    xamlContent = string.Format(xamlContent, @"作者被禁止或删除&#160;内容自动屏蔽");
                }

                ReplyItemModel reply = new ReplyItemModel(i, floor, postId, pageNo, threadId, threadTitle, 0, authorUserId, authorUsername, textContent, htmlContent, xamlContent, postTime, imageCount, linkCount, linkClickEvent);
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
        #endregion

        #region user
        private static Dictionary<int, string> _userInfoData = new Dictionary<int, string>();

        private async Task LoadUserDataAsync(int userId, CancellationTokenSource cts)
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

            var node = doc.DocumentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("id", "").Equals("profilecontent"));
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

        private async Task<List<UserMessageItemModel>> LoadUserMessageDataAsync(int userId, int limitCount, CancellationTokenSource cts)
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
                        string str = userInfoNode.InnerText.Trim().Replace("&nbsp;", string.Empty).Replace(Environment.NewLine, "$");
                        string[] strAry = str.Split('$');
                        string username = strAry[0];
                        string time = strAry[1];
                        string date = time.Split(' ')[0];
                        
                        string textStr = messageNode.InnerText;
                        string htmlStr = messageNode.InnerHtml;
                        int linkCount = 0;
                        string xamlStr = Html.HtmlToXaml.ConvertUserMessage(htmlStr, ref linkCount);

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
            var postData = new Dictionary<string, object>();
            postData.Add("formhash", AccountService.FormHash);
            postData.Add("handlekey", "pmreply");
            postData.Add("lastdaterange", DateTime.Now.ToString("yyyy-MM-dd"));
            postData.Add("message", FaceService.FaceReplace(message));

            string url = string.Format("http://www.hi-pda.com/forum/pm.php?action=send&uid={0}&pmsubmit=yes&_={1}", userId, DateTime.Now.Ticks.ToString("x"));
            var cts = new CancellationTokenSource();
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            if (resultContent.StartsWith(@"<?xml version=""1.0"" encoding=""gbk""?><root><![CDATA[<li id=""pm_") && resultContent.Contains(@"images/default/notice_newpm.gif"))
            {
                return true;
            }

            return false;
        }
        #endregion

    }
}
