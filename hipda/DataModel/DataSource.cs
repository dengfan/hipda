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
        public Reply(int index, int pageNo, string threadId, string ownerId, string ownerName, string content, string createTime)
        {
            this.Index = index;
            this.PageNo = pageNo;
            this.ThreadId = threadId;
            this.OwnerId = ownerId;
            this.OwnerName = ownerName;
            this.Content = content;
            this.CreateTime = createTime;
        }

        
        public int Index { get; private set; }
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
    }

    public class ReplyPage
    {
        public ReplyPage(int pageNo)
        {
            this.PageNo = pageNo;
            this.Replies = new ObservableCollection<Reply>();
        }

        public int PageNo { get; private set; }
        public ObservableCollection<Reply> Replies { get; private set; }
    }

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
            this.ReplyPages = new ObservableCollection<ReplyPage>();
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
        public ObservableCollection<ReplyPage> ReplyPages { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public class Forum
    {
        public Forum(string id, string name, string todayCount, string info)
        {
            this.Id = id;
            this.Name = name;
            this.TodayQuantity = todayCount;
            this.Info = info;
            this.Threads = new ObservableCollection<Thread>();
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
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

                    string forumTodayQuantity = string.Empty;
                    if (h2.ChildNodes.Count == 2)
                    {
                        var em = h2.ChildNodes[1];
                        forumTodayQuantity = em.InnerText;
                    }

                    string forumInfo = p.InnerText;

                    forumGroup.Forums.Add(new Forum(forumId, forumName, forumTodayQuantity, forumInfo));
                }

                this.ForumGroups.Add(forumGroup);
            }

            // 关闭忙指示器
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = 0;
        }
        #endregion

        #region 读取指定版块下的所有贴子
        public static async Task<Forum> GetForumsAsync(Forum fourm, int pageNo)
        {
            await _dataSource.LoadForumThreadsDataAsync(fourm, pageNo);

            return _dataSource.Forums.Single(f => f.Id.Equals(fourm.Id));
        }

        private async Task LoadForumThreadsDataAsync(Forum forum, int pageNo)
        {
            // 如果数据已存在，则不加载
            if (this._forums.Count(f => f.Id.Equals(forum.Id) && f.Threads.Count(t => t.PageNo == pageNo) > 0) > 0)
            {
                return;
            }

            HttpClient httpClient = new HttpClient();
            Helpers.CreateHttpClient(ref httpClient);

            CancellationTokenSource cts = new CancellationTokenSource();

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/forumdisplay.php?fid={0}&page={1}", forum.Id, pageNo);
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url)).AsTask(cts.Token);
            response.Content.Headers.ContentType.CharSet = "GBK";

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(await response.Content.ReadAsStringAsync().AsTask(cts.Token));

            // 
            var dataTable = doc.DocumentNode.Descendants().Single(n => n.GetAttributeValue("class", "") == "datatable");
            var tbodies = dataTable.Descendants().Where(n => n.GetAttributeValue("id", "").StartsWith("normalthread_"));

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

                Thread thread = new Thread(i, pageNo, forum.Id, id, title, "1", "2", "3", "4", "5");

                forum.Threads.Add(thread);

                i++;
            }

            this.Forums.Add(forum);

            // 关闭忙指示器
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = 0;
        }
        #endregion

        #region 读取指定贴子下所有回复
        public static async Task<Thread> GetThreadAsync(string forumId, Thread thread, int pageNo)
        {
            await _dataSource.LoadReplyDataAsync(forumId, thread, pageNo);

            return _dataSource.Forums.Single(f => f.Id.Equals(forumId)).Threads.Single(t => t.Id == thread.Id);
        }

        // 读取指定贴子的回复列表数据
        public async Task LoadReplyDataAsync(string forumId, Thread thread, int pageNo)
        {
            // 如果数据已存在，则不读取
            Forum forum = this._forums.Single(f => f.Id.Equals(forumId));
            if (forum != null)
            {
                Thread threadData = forum.Threads.Single(t => t.Id.Equals(thread.Id));
                if (threadData != null)
                {
                    if (threadData.ReplyPages.SingleOrDefault(rp => rp.PageNo == pageNo) != null)
                    {
                        return;
                    }

                    this.Forums.Single(f => f.Id.Equals(forumId)).Threads.Single(t => t.Id.Equals(thread.Id)).ReplyPages.Add(new ReplyPage(pageNo));


                    HttpClient httpClient = new HttpClient();
                    Helpers.CreateHttpClient(ref httpClient);

                    CancellationTokenSource cts = new CancellationTokenSource();

                    // 读取数据
                    string url = string.Format("http://www.hi-pda.com/forum/viewthread.php?tid={0}&page={1}", thread.Id, pageNo);
                    HttpResponseMessage response = await httpClient.GetAsync(new Uri(url)).AsTask(cts.Token);
                    response.Content.Headers.ContentType.CharSet = "GBK";

                    // 实例化 HtmlAgilityPack.HtmlDocument 对象
                    HtmlDocument doc = new HtmlDocument();

                    // 载入HTML
                    doc.LoadHtml(await response.Content.ReadAsStringAsync().AsTask(cts.Token));

                    // 解析HTML
                    //var data = doc.DocumentNode
                    //    .ChildNodes[2] // html
                    //    .ChildNodes[3] // body
                    //    .ChildNodes[14] // div#wrap
                    //    .ChildNodes[5] // div#postlist
                    //    .ChildNodes;

                    var data = doc.DocumentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("id", "").Equals("postlist")).ChildNodes;
                    if (data != null)
                    {
                        int i = 0;
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
                            string postTime = string.Empty;
                            try
                            {
                                var authorNode = postAuthorNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").Equals("postinfo"));
                                if (authorNode != null)
                                {
                                    authorNode = authorNode.ChildNodes[1]; // a
                                    ownerId = authorNode.Attributes[1].Value.Substring("space.php?uid=".Length);
                                    ownerName = authorNode.InnerText;
                                }
                            }
                            catch
                            {

                                throw new Exception("author node");
                            }

                            try
                            {
                                var postTimeNode = postContentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("id", "").StartsWith("authorposton")); // em
                                if (postTimeNode != null)
                                {
                                    postTime = postTimeNode.InnerText;
                                }
                            }
                            catch
                            {

                                throw new Exception("post time node");
                            }

                            string content = string.Empty;
                            try
                            {
                                var contentNode = postContentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("id", "").StartsWith("postmessage_"));
                                if (contentNode != null)
                                {
                                    content = contentNode.InnerText;
                                }
                            }
                            catch
                            {

                                throw new Exception("main post");
                            }

                            Reply reply = new Reply(i, pageNo, thread.Id, ownerId, ownerName, content, postTime);

                            this.Forums.Single(f => f.Id.Equals(forumId)).Threads.Single(t => t.Id.Equals(thread.Id)).ReplyPages.Single(rp => rp.PageNo == pageNo).Replies.Add(reply);

                            i++;
                        }
                    }
                }
            }

            // 关闭忙指示器
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = 0;
        }
        #endregion
    }
}
