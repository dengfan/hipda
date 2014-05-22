using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.Web.Http;
using Windows.Data.Xml;
using HtmlAgilityPack;
using Windows.UI.ViewManagement;

namespace hipda.Data
{
    public class Reply
    {
        public Reply(int floor, int pageNo, string threadId, string ownerId, string ownerName, string content, string createTime)
        {
            this.Floor = floor;
            this.PageNo = pageNo;
            this.ThreadId = threadId;
            this.OwnerId = ownerId;
            this.OwnerName = ownerName;
            this.Content = content;
            this.CreateTime = createTime;
        }

        
        public int Floor { get; private set; }
        public int PageNo { get; private set; }
        public string ThreadId { get; private set; }
        public string OwnerName { get; private set; }

        public string OwnerId { get; private set; }

        public string Content { get; private set; }

        public string CreateTime { get; private set; }

        public override string ToString()
        {
            return this.Content;
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

    //public class ReplyPage
    //{
    //    public ReplyPage(int pageNo)
    //    {
    //        this.PageNo = pageNo;
    //        this.Replies = new ObservableCollection<Reply>();
    //    }

    //    public int PageNo { get; private set; }
    //    public ObservableCollection<Reply> Replies { get; private set; }
    //}

    public class Thread
    {
        public Thread(int index, int pageNo, string forumId, string id, string title, string replyAndViewInfo, string ownerName, string ownerId, string createTime, string lastReplyTime)
        {
            this.Index = index;
            this.PageNo = pageNo;
            this.ForumId = forumId;
            this.Id = id;
            this.Title = title;
            this.ReplyAndViewInfo = replyAndViewInfo;
            this.OwnerName = ownerName;
            this.OwnerId = ownerId;
            this.CreateTime = createTime;
            this.LastReplyTime = lastReplyTime;
            this.Replies = new ObservableCollection<Reply>();
        }

        public int Index { get; private set; }
        public int PageNo { get; private set; }
        public string ForumId { get; private set; }
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string ReplyAndViewInfo { get; private set; }
        public string OwnerName { get; private set; }
        public string OwnerId { get; private set; }
        public string CreateTime { get; private set; }
        public string LastReplyTime { get; private set; }
        public ObservableCollection<Reply> Replies { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
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
            this.Threads = new ObservableCollection<Thread>();
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Alias { get; private set; }
        public string Info { get; private set; }
        public string TodayQuantity { get; private set; }

        public ObservableCollection<Thread> Threads { get; private set; }

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

    public sealed class DataSource
    {
        //private static int threadsPageSize = 75;
        private static int repliesPageSize = 50;
        private static DataSource _dataSource = new DataSource();

        private ObservableCollection<ForumGroup> _forumGroups = new ObservableCollection<ForumGroup>();
        public ObservableCollection<ForumGroup> ForumGroups
        {
            get { return this._forumGroups; }
        }

        private ObservableCollection<Forum> _forums = new ObservableCollection<Forum>();
        public ObservableCollection<Forum> Forums
        {
            get { return this._forums; }
        }

        #region 读取论坛所有版板数据
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
                return;
            }

            HttpClient httpClient = new HttpClient();
            Helpers.CreateHttpClient(ref httpClient);

            CancellationTokenSource cts = new CancellationTokenSource();

            // 读取数据
            string url = "http://www.hi-pda.com/forum/index.php";
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url)).AsTask(cts.Token);
            response.Content.Headers.ContentType.CharSet = "GBK";

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(await response.Content.ReadAsStringAsync().AsTask(cts.Token));
            var data = doc.DocumentNode;

            var content = data.ChildNodes[2] // html
                .ChildNodes[3] // body
                .ChildNodes[10] // div#wrap
                .ChildNodes[1] // div.main
                .ChildNodes[0]; // div.content 登录后有23个子节点

            var mainBoxes = content.Descendants().Where(n => n.GetAttributeValue("class", "") == "mainbox list" && n.GetAttributeValue("id", "") != "online");
            foreach (var mainBox in mainBoxes)
            {
                string forumGroupName = mainBox.ChildNodes[3].ChildNodes[0].InnerText;
                ForumGroup forumGroup = new ForumGroup(forumGroupName);

                var tbodies = mainBox.ChildNodes[5] // table
                    .Descendants().Where(n => n.GetAttributeValue("id", "").StartsWith("forum"));

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
        public static void RemoveForum(string forumId)
        {
            var forumData = _dataSource.Forums.SingleOrDefault(f => f.Id.Equals(forumId));
            if (forumData != null)
            {
                _dataSource.Forums.Remove(forumData);
            }
        }

        public static async Task<Forum> GetForum(string forumId, string forumName)
        {
            var count = _dataSource.Forums.Count(f => f.Id.Equals(forumId));
            if (count == 0)
            {
                _dataSource.Forums.Add(new Forum(forumId, forumName, string.Empty, string.Empty, string.Empty));
            }

            // 先加载第一页的数据，以提高响应流畅度
            await _dataSource.LoadThreadsDataAsync(forumId, 1);

            return _dataSource.Forums.SingleOrDefault(f => f.Id.Equals(forumId));
        }

        public static async Task<int> GetLoadThreadsCountAsync(string forumId, int pageNo)
        {
            await _dataSource.LoadThreadsDataAsync(forumId, pageNo);

            return _dataSource.Forums.Single(f => f.Id.Equals(forumId)).Threads.Count;
        }
        public static Thread GetThreadByIndex(string forumId, int index)
        {
            return _dataSource.Forums.Single(f => f.Id.Equals(forumId)).Threads.ElementAt(index);
        }

        private async Task LoadThreadsDataAsync(string forumId, int pageNo)
        {
            Forum forum = this._forums.SingleOrDefault(f => f.Id.Equals(forumId));
            if (forum == null)
            {
                return;
            }

            // 载入过的页面不再载入
            int count = forum.Threads.Count(t => t.PageNo == pageNo);
            if (count > 0)
            {
                return;
            }

            HttpClient httpClient = new HttpClient();
            Helpers.CreateHttpClient(ref httpClient);

            CancellationTokenSource cts = new CancellationTokenSource();

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/forumdisplay.php?fid={0}&page={1}", forumId, pageNo);
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url)).AsTask(cts.Token);
            response.Content.Headers.ContentType.CharSet = "GBK";

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(await response.Content.ReadAsStringAsync().AsTask(cts.Token));

            var dataTable = doc.DocumentNode.Descendants().Single(n => n.GetAttributeValue("class", "") == "datatable");
            var tbodies = dataTable.Descendants().Where(n => n.GetAttributeValue("id", "").StartsWith("normalthread_") || n.GetAttributeValue("id", "").StartsWith("stickthread_"));

            int i = 0;
            foreach (var item in tbodies)
            {
                var tr = item.ChildNodes[1];
                var th = tr.ChildNodes[5];
                var span = th.Descendants().Single(n => n.GetAttributeValue("id", "").StartsWith("thread_"));
                var a = span.ChildNodes[0];
                //var tdAuthor = tr.ChildNodes[7];
                //var tdNums = tr.ChildNodes[9];
                //var tdLastPost = tr.ChildNodes[11];

                string id = span.Attributes[0].Value.Substring("thread_".Length);
                string title = a.InnerText;

                Thread thread = new Thread(i, pageNo, forumId, id, title, "1", "2", "3", "4", "5");

                //this.Forums.Single(f => f.Id.Equals(forumId)).Threads.Add(thread);
                forum.Threads.Add(thread);

                i++;
            }
        }
        #endregion

        #region 读取指定贴子下所有回复
        public static async void RefrashReply(string forumId, string threadId)
        {
            int maxPageNo = _dataSource.Forums.Single(f => f.Id.Equals(forumId)).Threads.Single(t => t.Id == threadId).Replies.Max(r => r.PageNo);
            await _dataSource.LoadRepliesDataAsync(forumId, threadId, maxPageNo);
        }

        public static async Task<Thread> GetThread(string forumId, string threadId)
        {
            // 先加载第一页的数据，以提高响应流畅度
            await _dataSource.LoadRepliesDataAsync(forumId, threadId, 1);

            return _dataSource.Forums.Single(f => f.Id.Equals(forumId)).Threads.Single(t => t.Id == threadId);
        }

        public static async Task<int> GetLoadRepliesCountAsync(string forumId, string threadId, int pageNo, Action showProgressBar, Action hideProgressBar)
        {
            showProgressBar();
            await _dataSource.LoadRepliesDataAsync(forumId, threadId, pageNo);
            hideProgressBar();

            return _dataSource.Forums.Single(f => f.Id.Equals(forumId)).Threads.Single(t => t.Id == threadId).Replies.Count;
        }

        public static Reply GetReplyByIndex(string forumId, string threadId, int index)
        {
            return _dataSource.Forums.Single(f => f.Id.Equals(forumId)).Threads.Single(t => t.Id == threadId).Replies.OrderBy(r => r.Floor).ElementAt(index);
        }

        // 读取指定贴子的回复列表数据
        private async Task LoadRepliesDataAsync(string forumId, string threadId, int pageNo)
        {
            #region 如果数据已存在，则不读取，以便节省流量
            Forum forum = this._forums.SingleOrDefault(f => f.Id.Equals(forumId));
            if (forum == null)
            {
                return;
            }

            Thread threadData = forum.Threads.SingleOrDefault(t => t.Id.Equals(threadId));
            if (threadData == null)
            {
                return;
            }

            // 如果页面已存在，并且已载满数据，则不重新从网站拉取数据，以便节省流量， 
            // 但最后一页（如果数据少于一页，那么第一页就是最后一页）由于随时可能会有新回复，所以对于最后一页的处理方式是先清除再重新加载
            int count = threadData.Replies.Count(r => r.PageNo == pageNo);
            if (count > 0)
            {
                // 由于第一页已经预加载了，所以这里不再加载第一页的数据
                if (pageNo == 1)
                {
                    return;
                }

                if (count >= repliesPageSize) // 满页的不再加载，以便节省流量
                {
                    return;
                }

                // 再判断未满页的
                // 第一页或最后一页的回复数量不足一页，表示此页随时可能有新回复，故删除
                var lastPageData = threadData.Replies.Where(r => r.PageNo == pageNo).ToList();
                foreach (var item in lastPageData)
                {
                    threadData.Replies.Remove(threadData.Replies.Single(r => r.Floor == item.Floor));
                }
            }
            #endregion

            HttpClient httpClient = new HttpClient();
            Helpers.CreateHttpClient(ref httpClient);

            var cts = new CancellationTokenSource();

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/viewthread.php?tid={0}&page={1}", threadId, pageNo);
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url)).AsTask(cts.Token);
            response.Content.Headers.ContentType.CharSet = "GBK";

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(await response.Content.ReadAsStringAsync().AsTask(cts.Token));

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
            if (data != null)
            {
                foreach (var item in data)
                {
                    var postAuthorNode = item.ChildNodes[0] // table
                            .ChildNodes[1] // tr
                            .ChildNodes[1]; // td.postauthor

                    var postContentNode = item.ChildNodes[0] // table
                            .ChildNodes[1] // tr
                            .ChildNodes[3]; // td.postcontent

                    string ownerId = string.Empty;
                    string ownerName = string.Empty;
                    var authorNode = postAuthorNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").Equals("postinfo"));
                    if (authorNode != null)
                    {
                        authorNode = authorNode.ChildNodes[1]; // a
                        ownerId = authorNode.Attributes[1].Value.Substring("space.php?uid=".Length);
                        ownerName = authorNode.InnerText;
                    }
                    
                    var floorNode = postContentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").StartsWith("postinfo")) // div
                        .ChildNodes[1] // strong
                        .ChildNodes[0] // a
                        .ChildNodes[0]; // em
                    int floor = Convert.ToInt32(floorNode.InnerText);

                    string postTime = string.Empty;
                    var postTimeNode = postContentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("id", "").StartsWith("authorposton")); // em
                    if (postTimeNode != null)
                    {
                        postTime = postTimeNode.InnerText
                            .Replace("发表于 ", string.Empty)
                            .Replace(string.Format("{0}-", DateTime.Now.Year), string.Empty);
                    }

                    string content = string.Empty;
                    var contentNode = postContentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("id", "").StartsWith("postmessage_"));
                    if (contentNode != null)
                    {
                        content = contentNode.InnerText;
                    }

                    Reply reply = new Reply(floor, pageNo, threadId, ownerId, ownerName, content, postTime);

                    threadData.Replies.Add(reply);
                }
            }
        }
        #endregion
    }
}
