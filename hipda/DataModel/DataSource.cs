using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Html;

namespace hipda.Data
{
    #region 实体类
    public class ContentForEdit
    {
        public ContentForEdit(string subject, string content, string postId, string threadId)
        {
            this.Subject = subject;
            this.Content = content;
            this.PostId = postId;
            this.ThreadId = threadId;
        }

        public string Subject { get; private set; }

        public string Content { get; private set; }

        public string PostId { get; private set; }

        public string ThreadId { get; private set; }
    }

    public class Reply
    {
        public Reply(int index, int floor, string postId, int pageNo, string threadId, string ownerId, string ownerName, string textContent, string htmlContent, string xamlConent, int imageCount, string createTime)
        {
            this.Index = index;
            this.Floor = floor;
            this.PostId = postId;
            this.PageNo = pageNo;
            this.ThreadId = threadId;
            this.OwnerId = ownerId;
            this.OwnerName = ownerName;
            this.TextContent = textContent;
            this.HtmlContent = htmlContent;
            this.XamlContent = xamlConent;
            this.ImageCount = imageCount;
            this.CreateTime = createTime;
        }

        public int Index { get; private set; }
        public int Floor { get; private set; }
        public string PostId { get; private set; }
        public int PageNo { get; private set; }
        public string ThreadId { get; private set; }
        public string OwnerName { get; private set; }

        public string OwnerId { get; private set; }

        public string TextContent { get; private set; }

        public string HtmlContent { get; private set; }

        public string XamlContent { get; private set; }

        public int ImageCount { get; private set; }

        public string CreateTime { get; private set; }

        public override string ToString()
        {
            return this.HtmlContent;
        }
        public string AvatarUrl
        {
            get
            {
                int uid = Convert.ToInt32(OwnerId);
                var s = new int[10];
                for (int i = 0; i < s.Length - 1; ++i)
                {
                    s[i] = uid % 10;
                    uid = (uid - s[i]) / 10;
                }
                return "http://www.hi-pda.com/forum/uc_server/data/avatar/" + s[8] + s[7] + s[6] + "/" + s[5] + s[4] + "/" + s[3] + s[2] + "/" + s[1] + s[0] + "_avatar_middle.jpg";
            }
        }

        public string FloorNumStr
        {
            get
            {
                return string.Format("{0}#", Floor);
            }
        }
    }

    public class ReplyData
    {
        public ReplyData(string threadId, string threadName)
        {
            this.ThreadId = threadId;
            this.ThreadName = threadName;
            this.Replies = new ObservableCollection<Reply>();
        }
        public string ThreadId { get; private set; }
        public string ThreadName { get; private set; }
        public IList<Reply> Replies { get; private set; }
    }

    public class Thread
    {
        public Thread(int index, int pageNo, string forumId, string id, string title, int attachType, string replyNum, string viewNum, string ownerName, string ownerId, string createTime, string lastPostAuthorName, string lastPostTime)
        {
            this.Index = index;
            this.PageNo = pageNo;
            this.ForumId = forumId;
            this.Id = id;
            this.Title = title;
            this.AttachType = attachType;
            this.ReplyNum = replyNum;
            this.ViewNum = viewNum;
            this.OwnerName = ownerName;
            this.OwnerId = ownerId;
            this.CreateTime = createTime;
            this.LastPostAuthorName = lastPostAuthorName;
            this.LastPostTime = lastPostTime;
        }

        public int Index { get; private set; }
        public int PageNo { get; private set; }
        public string ForumId { get; private set; }
        public string Id { get; private set; }
        public string Title { get; private set; }
        public int AttachType { get; private set; }
        public string ReplyNum { get; private set; }
        public string ViewNum { get; private set; }
        public string OwnerName { get; private set; }
        public string OwnerId { get; private set; }
        public string CreateTime { get; private set; }
        public string LastPostAuthorName { get; private set; }
        public string LastPostTime { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }

        public string AvatarUrl
        {
            get
            {
                if (string.IsNullOrEmpty(OwnerId))
                {
                    return string.Empty;
                }

                int uid = Convert.ToInt32(OwnerId);
                var s = new int[10];
                for (int i = 0; i < s.Length - 1; ++i)
                {
                    s[i] = uid % 10;
                    uid = (uid - s[i]) / 10;
                }
                return "http://www.hi-pda.com/forum/uc_server/data/avatar/" + s[8] + s[7] + s[6] + "/" + s[5] + s[4] + "/" + s[3] + s[2] + "/" + s[1] + s[0] + "_avatar_middle.jpg";
            }
        }

        public string Numbers
        {
            get
            {
                return string.Format("({0}/{1})", ReplyNum, ViewNum);
            }
        }

        public string LastPostInfo
        {
            get
            {
                return string.Format("{0} {1}", LastPostAuthorName, LastPostTime);
            }
        }
    }

    public class ThreadData
    {
        public ThreadData(string forumId, string forumName)
        {
            this.ForumId = forumId;
            this.ForumName = forumName;
            this.Threads = new ObservableCollection<Thread>();
        }
        public string ForumId { get; private set; }

        public string ForumName { get; private set; }

        public IList<Thread> Threads { get; private set; }
    }

    public class Forum
    {
        public Forum(string id, string name, string alias, string todayCount, string info)
        {
            this.Id = id;
            this.Name = name;
            this.Alias = alias;
            this.TodayQuantity = todayCount;
            this.Info = info;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Alias { get; private set; }
        public string Info { get; private set; }
        public string TodayQuantity { get; private set; }

        public override string ToString()
        {
            return this.Name;
        }
 
    }

    public class ForumGroup
    {
        public ForumGroup(string name)
        {
            this.Name = name;
            this.Forums = new ObservableCollection<Forum>();
        }

        public string Name { get; private set; }
        public ObservableCollection<Forum> Forums { get; private set; }
    }
    #endregion

    public sealed class DataSource
    {
        public static int ThreadPageSize { get { return 75; } }

        public static int ReplyPageSize { get { return 50; } }

        /// <summary>
        /// 每个楼层默认显示的图片数据
        /// 用于节省流量
        /// </summary>
        public static int MaxImageCount { get { return 6; } }

        public static int OrderType = 2;

        public static string MessageTail { get { return "[img=16,16]http://www.hi-pda.com/forum/attachments/day_140621/1406211752793e731a4fec8f7b.png[/img]"; } }

        private static DataSource _dataSource = new DataSource();

        private static HttpHandle httpClient = HttpHandle.getInstance();

        /// <summary>
        /// 用于发布信息的 formhash 值
        /// </summary>
        private string _formHash = string.Empty;
        public static string FormHash
        {
            get { return _dataSource._formHash; }
        }

        /// <summary>
        /// 用户ID，用于上传图片
        /// </summary>
        private string _userId = string.Empty;
        public static string UserId
        {
            get { return _dataSource._userId; }
        }

        /// <summary>
        /// 用于上载图片所需的 hash 值
        /// </summary>
        private string _hash = string.Empty;
        public static string Hash
        {
            get { return _dataSource._hash; }
        }

        private ObservableCollection<ForumGroup> _forumGroups = new ObservableCollection<ForumGroup>();
        public ObservableCollection<ForumGroup> ForumGroups
        {
            get { return this._forumGroups; }
        }

        private ObservableCollection<ThreadData> _threadList = new ObservableCollection<ThreadData>();
        public ObservableCollection<ThreadData> ThreadList
        {
            get { return this._threadList; }
        }

        private List<ReplyData> _replyList = new List<ReplyData>();
        public List<ReplyData> ReplyList
        {
            get { return this._replyList; }
        }

        private ContentForEdit _contentForEdit = null;
        public static ContentForEdit ContentForEdit
        {
            get { return _dataSource._contentForEdit; }
        }

        #region 读取论坛所有版块数据
        public static async Task<IEnumerable<ForumGroup>> GetForumGroupsAsync()
        {
            await _dataSource.LoadForumGroupDataAsync();

            return _dataSource.ForumGroups;
        }

        // 读取所以版区列表数据
        public async Task LoadForumGroupDataAsync()
        {
            if (this._forumGroups.Count > 0)
            {
                this.ForumGroups.Clear();
            }

            // 读取数据
            string url = "http://www.hi-pda.com/forum/index.php?r=" + DateTime.Now.Second;
            string htmlContent = await httpClient.HttpGet(url);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);
            var data = doc.DocumentNode;

            var content = data.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").Equals("content"));
            if (content == null)
            {
                return;
            }

            var mainBoxes = content.Descendants().Where(n => n.GetAttributeValue("class", "").Equals("mainbox list") && !n.GetAttributeValue("id", "").Equals("online"));
            if (mainBoxes == null)
            {
                return;
            }

            foreach (var mainBox in mainBoxes)
            {
                string forumGroupName = mainBox.ChildNodes[3].ChildNodes[0].InnerText;
                ForumGroup forumGroup = new ForumGroup(forumGroupName);

                var tbodies = mainBox.ChildNodes[5] // table
                    .Descendants().Where(n => n.GetAttributeValue("id", "").StartsWith("forum"));

                if (tbodies == null)
                {
                    return;
                }

                foreach (var tbody in tbodies)
                {
                    var divLeft = tbody
                        .ChildNodes[1] // tr
                        .ChildNodes[1] // th
                        .ChildNodes[1]; // div.left

                    var h2 = divLeft.ChildNodes[1]; // h2
                    var p = divLeft.ChildNodes[3]; // p

                    var a = h2.ChildNodes[0];
                    string forumId = a.Attributes[0].Value.Substring("forumdisplay.php?fid=".Length);
                    if (forumId.Contains("&"))
                    {
                        forumId = forumId.Split('&')[0];
                    }

                    string forumName = a.InnerText;
                    string forumAlias = forumName;
                    switch (forumId)
                    {
                        case "6":
                            forumAlias = "BS版";
                            break;
                        case "7":
                            forumAlias = "G版";
                            break;
                        case "14":
                            forumAlias = "Win版";
                            break;
                        case "2":
                            forumAlias = "地板";
                            break;
                        case "59":
                            forumAlias = "E版";
                            break;
                        default:
                            forumAlias = string.Format("{0}版", forumAlias.Substring(0, 1));
                            break;
                    }

                    string forumTodayQuantity = string.Empty;
                    if (h2.ChildNodes.Count == 2)
                    {
                        var em = h2.ChildNodes[1];
                        forumTodayQuantity = em.InnerText;
                    }

                    string forumInfo = p.InnerText;

                    forumGroup.Forums.Add(new Forum(forumId, forumName, forumAlias, forumTodayQuantity, forumInfo));
                }

                this.ForumGroups.Add(forumGroup);
            }
        }
        #endregion

        #region 读取指定版块下的所有贴子
        public static ThreadData GetThread(string forumId, string forumName)
        {
            var count = _dataSource.ThreadList.Count(t => t.ForumId.Equals(forumId));
            if (count == 0)
            {
                _dataSource.ThreadList.Add(new ThreadData(forumId, forumName));
            }

            return _dataSource.ThreadList.Single(t => t.ForumId.Equals(forumId));
        }

        public static async Task RefreshThreadList(string forumId)
        {
            var threadData = _dataSource.ThreadList.FirstOrDefault(t => t.ForumId.Equals(forumId));
            if (threadData != null)
            {
                threadData.Threads.Clear();
                await _dataSource.LoadThreadsDataAsync(forumId, 1);
            }
        }

        public static async Task<int> GetLoadThreadsCountAsync(string forumId, int pageNo, Action showProgressBar, Action hideProgressBar)
        {
            showProgressBar();
            await _dataSource.LoadThreadsDataAsync(forumId, pageNo);
            hideProgressBar();

            return _dataSource.ThreadList.Single(f => f.ForumId.Equals(forumId)).Threads.Count;
        }

        /// <summary>
        /// 用于增量加载来控制要显示哪几项
        /// </summary>
        /// <param name="forumId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Thread GetThreadByIndex(string forumId, int index)
        {
            return _dataSource.ThreadList.Single(f => f.ForumId.Equals(forumId)).Threads[index];
        }

        private async Task LoadThreadsDataAsync(string forumId, int pageNo)
        {
            // 载入过的页面不再载入
            var forum = _dataSource.ThreadList.FirstOrDefault(t => t.ForumId.Equals(forumId));
            if (forum == null)
            {
                return;
            }

            int threadCount = forum.Threads.Count(t => t.PageNo == pageNo);
            if (threadCount > 1)
            {
                return;
            }

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/forumdisplay.php?fid={0}&page={1}&r={2}", forumId, pageNo, DateTime.Now.ToString("HHmmss"));
            string htmlContent = await httpClient.HttpGet(url);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            var dataTable = doc.DocumentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").Equals("datatable"));
            if (dataTable == null)
            {
                return;
            }

            // 如果置顶贴数过多，只取非置顶贴的话，第一页数据项过少，会导致不会自动触发加载下一页数据
            var tbodies = dataTable.Descendants().Where(n => n.GetAttributeValue("id", "").StartsWith("normalthread_"));
            if (tbodies == null)
            {
                return;
            }

            int i = 0;
            foreach (var item in tbodies)
            {
                var tr = item.ChildNodes[1];
                var th = tr.ChildNodes[5];
                var span = th.Descendants().Single(n => n.GetAttributeValue("id", "").StartsWith("thread_"));
                var a = span.ChildNodes[0];
                var tdAuthor = tr.ChildNodes[7];
                var tdNums = tr.ChildNodes[9];
                var tdLastPost = tr.ChildNodes[11];

                string id = span.Attributes[0].Value.Substring("thread_".Length);
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
                var authorId = string.Empty;
                var authorNameNode = tdAuthor.ChildNodes[1]; // cite
                var authorNameLink = authorNameNode.Descendants().FirstOrDefault(n => n.Name.Equals("a"));
                if (authorNameLink == null)
                {
                    authorName = authorNameNode.InnerText;
                }
                else
                {
                    authorName = authorNameLink.InnerText;
                    authorId = authorNameLink.Attributes[0].Value.Substring("space.php?uid=".Length);
                    if (authorId.Contains("&"))
                    {
                        authorId = authorId.Split('&')[0];
                    }
                }

                var authorCreateTime = tdAuthor.ChildNodes[3].InnerText;

                var replyNum = tdNums.ChildNodes[0].InnerText;
                var viewNum = tdNums.ChildNodes[2].InnerText;

                var lastPostAuthorName = tdLastPost.ChildNodes[1].ChildNodes[0].InnerText;
                var lastPostTime = tdLastPost.ChildNodes[3].ChildNodes[0].InnerText
                    .Replace(string.Format("{0}-{1}-{2} ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), string.Empty)
                    .Replace(string.Format("{0}-", DateTime.Now.Year), string.Empty);

                Thread thread = new Thread(i, pageNo, forumId, id, title, attachType, replyNum, viewNum, authorName, authorId, authorCreateTime, lastPostAuthorName, lastPostTime);
                forum.Threads.Add(thread);

                i++;
            }
        }
        #endregion

        #region 读取指定贴子下所有回复
        public static ReplyData GetReply(string threadId, string threadName)
        {
            var count = _dataSource.ReplyList.Count(t => t.ThreadId.Equals(threadId));
            if (count == 0)
            {
                _dataSource.ReplyList.Add(new ReplyData(threadId, threadName));
            }

            return _dataSource.ReplyList.Single(t => t.ThreadId.Equals(threadId));
        }

        public static async Task RefreshReplyList(string threadId)
        {
            var replyData = _dataSource.ReplyList.FirstOrDefault(r => r.ThreadId.Equals(threadId));
            if (replyData != null)
            {
                replyData.Replies.Clear();
                await _dataSource.LoadRepliesDataAsync(threadId, 1);
            }
        }

        public static async Task<int> GetLoadRepliesCountAsync(string threadId, int pageNo, Action showProgressBar, Action hideProgressBar)
        {
            showProgressBar();
            await _dataSource.LoadRepliesDataAsync(threadId, pageNo);
            hideProgressBar();

            return _dataSource.ReplyList.First(t => t.ThreadId == threadId).Replies.Count;
        }

        /// <summary>
        /// 用于增量加载来控制要显示哪几项
        /// </summary>
        /// <param name="forumId"></param>
        /// <param name="threadId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Reply GetReplyByIndex(string threadId, int index)
        {
            return _dataSource.ReplyList.Single(r => r.ThreadId.Equals(threadId)).Replies[index];
        }

        // 读取指定贴子的回复列表数据
        private async Task LoadRepliesDataAsync(string threadId, int pageNo)
        {
            // 移除过旧的数量，以释放内存空间
            //if (_dataSource.ReplyList.Count > 7)
            //{
            //    _dataSource.ReplyList[0] = null;
            //    _dataSource.ReplyList.RemoveAt(0);
            //}

            #region 如果数据已存在，则不读取，以便节省流量
            // 载入过的页面不再载入
            var thread = _dataSource.ReplyList.FirstOrDefault(t => t.ThreadId.Equals(threadId));
            if (thread == null)
            {
                return;
            }

            //// 现改为，只要当前页数据存在就不重新加载，让用户自己点击最底下的刷新按钮来刷新
            //// 此举可提高响应速度
            //int count = thread.Replies.Count(o => o.PageNo == pageNo);
            //if (count > 0)
            //{
            //    return;
            //}

            // 如果页面已存在，并且已载满数据，则不重新从网站拉取数据，以便节省流量， 
            // 但最后一页（如果数据少于一页，那么第一页就是最后一页）由于随时可能会有新回复，所以对于最后一页的处理方式是先清除再重新加载
            int count = thread.Replies.Count(o => o.PageNo == pageNo);
            if (count > 0)
            {
                if (count >= ReplyPageSize) // 满页的不再加载，以便节省流量
                {
                    return;
                }

                // 再判断未满页的
                // 第一页或最后一页的回复数量不足一页，表示此页随时可能有新回复，故删除
                var lastPageData = thread.Replies.Where(r => r.PageNo == pageNo).ToList();
                foreach (var item in lastPageData)
                {
                    thread.Replies.Remove(thread.Replies.Single(r => r.Floor == item.Floor));
                }
            }
            #endregion

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/viewthread.php?tid={0}&page={1}&ordertype={2}&" + DateTime.Now.Second, threadId, pageNo, OrderType);
            string htmlStr = await httpClient.HttpGet(url);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            #region 先判断页码是否已超过最大页码，以免造成重复加载
            if (pageNo > 1)
            {
                var forumControlNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("forumcontrol s_clear"));
                var pagesNode = forumControlNode.ChildNodes[1] // table
                    .ChildNodes[1] // tr
                    .ChildNodes[3] // td
                    .Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").Equals("pages"));
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

            int i = thread.Replies.Count;
            foreach (var item in data)
            {
                var postAuthorNode = item.ChildNodes[0] // table
                        .ChildNodes[1] // tr
                        .ChildNodes[1]; // td.postauthor

                var postContentNode = item.ChildNodes[0] // table
                        .ChildNodes[1] // tr
                        .ChildNodes[3]; // td.postcontent

                string authorId = string.Empty;
                string ownerName = string.Empty;
                var authorNode = postAuthorNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").Equals("postinfo"));
                if (authorNode != null)
                {
                    authorNode = authorNode.ChildNodes[1]; // a
                    authorId = authorNode.Attributes[1].Value.Substring("space.php?uid=".Length);
                    if (authorId.Contains("&"))
                    {
                        authorId = authorId.Split('&')[0];
                    }
                    ownerName = authorNode.InnerText;
                }

                var floorPostInfoNode = postContentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").StartsWith("postinfo")); // div
                var floorLinkNode = floorPostInfoNode.ChildNodes[1].ChildNodes[0]; // a
                string postId = floorLinkNode.Attributes["id"].Value.Replace("postnum", string.Empty);
                var floorNumNode = floorLinkNode.ChildNodes[0]; // em
                int floor = Convert.ToInt32(floorNumNode.InnerText);

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
                    textContent = new Regex("\r\n").Replace(textContent, string.Empty);
                    textContent = new Regex("\r").Replace(textContent, string.Empty);
                    textContent = new Regex("\n").Replace(textContent, string.Empty);
                    textContent = new Regex(@"[\s]{2}").Replace(textContent, " ");
                    textContent = textContent.Replace("&nbsp;", string.Empty);
                    textContent = textContent.Length > 100 ? textContent.Substring(0, 97) + "..." : textContent;
                    
                    // 用于显示原始内容
                    htmlContent = xamlContent = contentNode.InnerHtml;

                    // 用于正常显示
                    xamlContent = HtmlToXaml(xamlContent.Trim(), ref imageCount);
                }
                else
                {
                    xamlContent = @"作者被禁止或删除&#160;内容自动屏蔽";
                }
                
                xamlContent = new Regex("<[^<]*>").Replace(xamlContent, string.Empty); // 移除所有HTML标签
                xamlContent = new Regex("\r\n").Replace(xamlContent, string.Empty); // 忽略源换行
                xamlContent = new Regex("\r").Replace(xamlContent, string.Empty); // 忽略源换行
                xamlContent = new Regex("\n").Replace(xamlContent, string.Empty); // 忽略源换行
                xamlContent = new Regex(@"↵{1,}").Replace(xamlContent, "↵"); // 将多个换行符合并成一个
                xamlContent = new Regex(@"^↵").Replace(xamlContent, string.Empty); // 移除行首的换行符
                xamlContent = new Regex(@"↵$").Replace(xamlContent, string.Empty); // 移除行末的换行符
                xamlContent = xamlContent.Replace("↵", "[LineBreak/]"); // 解析换行符
                xamlContent = xamlContent.Replace("[", "<");
                xamlContent = xamlContent.Replace("]", ">");
                xamlContent = string.Format(@"<RichTextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><Paragraph>{0}</Paragraph></RichTextBlock>", xamlContent);

                Reply reply = new Reply(i, floor, postId, pageNo, threadId, authorId, ownerName, textContent, htmlContent, xamlContent, imageCount, postTime);
                thread.Replies.Add(reply);

                i++;
            }
        }

        private static string HtmlToXaml(string htmlContent, ref int imageCount)
        {
            var content = new StringBuilder(htmlContent);
            content.EnsureCapacity(htmlContent.Length * 2);

            content = content.Replace("[", "&#8968;");
            content = content.Replace("]", "&#8971;");
            content = content.Replace("&nbsp;", " ");
            content = content.Replace("↵", "&#8629;");

            // 移除无用的的日期信息 
            MatchCollection matchsForInvalidHtml1 = new Regex(@"<div class=""t_smallfont"">[\d\s-:]*</div>").Matches(content.ToString());
            if (matchsForInvalidHtml1 != null && matchsForInvalidHtml1.Count > 0)
            {
                for (int i = 0; i < matchsForInvalidHtml1.Count; i++)
                {
                    var m = matchsForInvalidHtml1[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    content = content.Replace(placeHolder, string.Empty);
                }
            }

            // 移除无用的下载提示信息 
            MatchCollection matchsForInvalidHtml2 = new Regex(@"<strong>下载</strong></a>[()\d\sKB.]*<br[^>]*>").Matches(content.ToString());
            if (matchsForInvalidHtml2 != null && matchsForInvalidHtml2.Count > 0)
            {
                for (int i = 0; i < matchsForInvalidHtml2.Count; i++)
                {
                    var m = matchsForInvalidHtml2[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    content = content.Replace(placeHolder, string.Empty);
                }
            }

            // 替换链接
            MatchCollection matchsForLink = new Regex(@"<a\s+href=""([^""]*)""[^>]*>([^>#]*)</a>").Matches(content.ToString());
            if (matchsForLink != null && matchsForLink.Count > 0)
            {
                for (int i = 0; i < matchsForLink.Count; i++)
                {
                    var m = matchsForLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string linkUrl = m.Groups[1].Value;
                    string linkContent = m.Groups[2].Value;

                    if (!linkUrl.StartsWith("http"))
                    {
                        linkUrl = string.Format("http://www.hi-pda.com/forum/{0}", linkUrl);
                    }
                    string linkXaml = string.Format(@"[Hyperlink NavigateUri=""{0}"" Foreground=""DodgerBlue""]{1}[/Hyperlink]", linkUrl, linkContent);
                    content = content.Replace(placeHolder, linkXaml);
                }
            }

            content = content.Replace(@"<div class=""postattachlist"">", "↵");
            content = content.Replace("<br/>", "↵"); // ↵符号表示换行符
            content = content.Replace("<br />", "↵");
            content = content.Replace("<br>", "↵");

            // 替换引用文字标签
            content = content.Replace("<blockquote>", @"↵[Span Foreground=""Gray""]");
            content = content.Replace("</blockquote>", "[/Span]");

            // 移除无意义图片HTML
            content = content.Replace(@"src=""images/default/attachimg.gif""", string.Empty);
            content = content.Replace(@"src=""http://www.hi-pda.com/forum/images/default/attachimg.gif""", string.Empty);

            // 将HTML字符串转换为RichTextBlock XAML字符串
            // 替换上载图片
            MatchCollection matchsForImage1 = new Regex(@"<img[^>]*file=""([^""]*)""\swidth=""([\d]*)""[^>]*>").Matches(content.ToString());
            if (matchsForImage1 != null && matchsForImage1.Count > 0)
            {
                for (int i = 0; i < matchsForImage1.Count; i++)
                {
                    imageCount++;

                    string placeHolderLabel = matchsForImage1[i].Groups[0].Value; // 要被替换的元素
                    string imgUrl = matchsForImage1[i].Groups[1].Value; // 图片URL
                    int width = Convert.ToInt16(matchsForImage1[i].Groups[2].Value); // 图片宽度

                    string imgXaml = @"[InlineUIContainer][Image Stretch=""None""][Image.Source][BitmapImage UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";

                    if (imageCount <= MaxImageCount)
                    {
                        if (width > 300)
                        {
                            imgXaml = @"↵[InlineUIContainer][Image Stretch=""Uniform"" Width=""360""][Image.Source][BitmapImage DecodePixelWidth=""360"" UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]↵";
                        }

                        imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;
                        imgXaml = string.Format(imgXaml, imgUrl);
                    }
                    else
                    {
                        imgXaml = @"[Span Foreground=""Gray""]- 为节省流量，图片{0}已被智能忽略 -[/Span]";
                        imgXaml = string.Format(imgXaml, imageCount);
                    }
                    
                    content = content.Replace(placeHolderLabel, imgXaml);
                }
            }

            // 替换图片，已知图片宽度，主要是用编辑器引用网络图片，及少量带宽度的直接复制到编辑器里的图片
            MatchCollection matchsForImage2 = new Regex(@"<img\swidth=""([\d]*)""[^>]*src=""([^""]*)""[^>]*>").Matches(content.ToString());
            if (matchsForImage2 != null && matchsForImage2.Count > 0)
            {
                for (int i = 0; i < matchsForImage2.Count; i++)
                {
                    imageCount++;

                    var m = matchsForImage2[i];
                    string placeHolderLabel = m.Groups[0].Value; // 要被替换的元素
                    int width = Convert.ToInt16(matchsForImage2[i].Groups[1].Value); // 图片宽度
                    string imgUrl = m.Groups[2].Value; // 图片URL
                    string imgXaml = string.Empty;

                    if (imageCount <= MaxImageCount)
                    {
                        imgXaml = @"[InlineUIContainer][Image Stretch=""None""][Image.Source][BitmapImage UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";
                        if (width > 300)
                        {
                            imgXaml = @"↵[InlineUIContainer][Image Stretch=""Uniform"" Width=""360""][Image.Source][BitmapImage DecodePixelWidth=""360"" UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]↵";
                        }

                        if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;
                        imgXaml = string.Format(imgXaml, imgUrl);
                    }
                    else
                    {
                        imgXaml = @"[Span Foreground=""Gray""]- 为节省流量，图片{0}已被智能忽略 -[/Span]";
                        imgXaml = string.Format(imgXaml, imageCount);
                    }
                    
                    content = content.Replace(placeHolderLabel, imgXaml);
                }
            }

            // 替换图片，未知图片宽度，来自网站自带的一些小ICON及表情图标 和 直接复制到编辑器里的图片
            MatchCollection matchsForImage3 = new Regex(@"<img[^>]*src=""([^""]*)""[^>]*>").Matches(content.ToString());
            if (matchsForImage3 != null && matchsForImage3.Count > 0)
            {
                for (int i = 0; i < matchsForImage3.Count; i++)
                {
                    var m = matchsForImage3[i];
                    string placeHolderLabel = m.Groups[0].Value; // 要被替换的元素
                    string imgUrl = m.Groups[1].Value; // 图片URL
                    //string imgXaml = @"[InlineUIContainer][Image Stretch=""None"" MaxWidth=""400""][Image.Source][BitmapImage UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";
                    string imgXaml = string.Empty;

                    if (imgUrl.EndsWith("/back.gif") || imgUrl.StartsWith("images/smilies/") || imgUrl.Contains("images/attachicons")) // 论坛自带
                    {
                        imgXaml = @"[InlineUIContainer][Image Stretch=""None""][Image.Source][BitmapImage UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";

                        if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;
                        imgXaml = string.Format(imgXaml, imgUrl);
                    }
                    else
                    {
                        imageCount++;

                        if (imageCount <= MaxImageCount)
                        {
                            imgXaml = @"↵[InlineUIContainer][Image Stretch=""Uniform"" Width=""360""][Image.Source][BitmapImage DecodePixelWidth=""360"" UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]↵";

                            if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;
                            imgXaml = string.Format(imgXaml, imgUrl);
                        }
                        else
                        {
                            imgXaml = @"[Span Foreground=""Gray""]- 为节省流量，图片{0}已被智能忽略 -[/Span]";
                            imgXaml = string.Format(imgXaml, imageCount);
                        }
                    }

                    content = content.Replace(placeHolderLabel, imgXaml);
                }
            }

            return content.ToString();
        }
        #endregion

        #region 获取 formhash 用于提交数据，用于切换账号时调用
        public static async Task GetHashAndUserId()
        {
            await _dataSource.LoadHashAndUserId();
        }

        private async Task LoadHashAndUserId()
        {
            string url = "http://www.hi-pda.com/forum/post.php?action=newthread&fid=2&r=" + DateTime.Now.Second;
            string htmlContent = await httpClient.HttpGet(url);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            // 读取发布文字信息所需要的 hash 值
            var postNode = doc.DocumentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").Equals("content editorcontent"));
            if (postNode != null)
            {
                var formHashInputNode = postNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("name", "").Equals("formhash"));
                if (formHashInputNode != null)
                {
                    this._formHash = formHashInputNode.Attributes[3].Value.ToString();
                }
            }

            // 读取 上载图片所需的 uid 和 hash 值
            var imgAttachNode = doc.DocumentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("id", "").Equals("imgattachbtnhidden"));
            if (imgAttachNode != null)
            {
                var userIdNode = imgAttachNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("name", "").Equals("uid"));
                if (userIdNode != null)
                {
                    this._userId = userIdNode.Attributes[2].Value;
                }

                var hashNode = imgAttachNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("name", "").Equals("hash"));
                if (hashNode != null)
                {
                    this._hash = hashNode.Attributes[2].Value;
                }
            }
        }
        #endregion

        #region 读取指定贴子标题、内容及回复数据，用于修改贴子或回复
        public static async Task GetContentForEdit(string threadId, string postId)
        {
            await _dataSource.LoadContentForEdit(threadId, postId);
        }

        private async Task LoadContentForEdit(string threadId, string postId)
        {
            string url = string.Format("http://www.hi-pda.com/forum/post.php?action=edit&tid={0}&pid={1}&page=1&r=" + DateTime.Now.ToString("HHmmss"), threadId, postId);
            string htmlContent = await httpClient.HttpGet(url);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            // 标题
            string subject = string.Empty;
            var subjectInput = doc.DocumentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("prompt", "").Equals("post_subject"));
            if (subjectInput != null)
            {
                subject = subjectInput.Attributes["value"].Value;
            }

            // 内容
            string content = string.Empty;
            var contentTextArea = doc.DocumentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("prompt", "").Equals("post_message"));
            if (contentTextArea != null)
            {
                content = contentTextArea.InnerText.Replace(MessageTail, string.Empty).TrimEnd();
            }

            this._contentForEdit = new ContentForEdit(subject, content, postId, threadId);
        }
        #endregion
    }
}