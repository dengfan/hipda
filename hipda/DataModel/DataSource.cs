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

namespace hipda.Data
{
    public class Reply
    {
        public Reply(int index, string ownerId, string ownerName, string content, string createTime)
        {
            this.Index = index;
            this.OwnerId = ownerId;
            this.OwnerName = ownerName;
            this.Content = content;
            this.CreateTime = createTime;
        }

        public int Index { get; private set; }
        public string OwnerName { get; private set; }

        public string OwnerId { get; private set; }

        public string Content { get; private set; }

        public string CreateTime { get; private set; }

        public override string ToString()
        {
            return this.Content;
        }
    }

    public class Thread
    {
        public Thread(int index, string id, string title, string replyAndViewInfo, string ownerName, string ownerId, string createTime, string lastReplyTime)
        {
            this.Index = index;
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
        public Forum(string id, string name, string todayCount, string info)
        {
            this.Id = id;
            this.Name = name;
            this.TodayCount = todayCount;
            this.Info = info;
            this.Threads = new ObservableCollection<Thread>();
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Info { get; private set; }
        public string TodayCount { get; private set; }

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

        public static async Task<IEnumerable<Forum>> GetForumsAsync()
        {
            await _dataSource.LoadThreadDataAsync();

            return _dataSource.Forums;
        }

        public static async Task<Forum> GetForumAsync(string uniqueId)
        {
            await _dataSource.LoadThreadDataAsync();

            // 对于小型数据集可接受简单线性搜索
            var matches = _dataSource.Forums.Where((f) => f.Id.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<Thread> GetThreadAsync(string uniqueId)
        {
            await _dataSource.LoadThreadDataAsync();

            // 对于小型数据集可接受简单线性搜索
            var matches = _dataSource.Forums.SelectMany(forum => forum.Threads).Where((thread) => thread.Id.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private async Task LoadThreadDataAsync()
        {
            if (this._forums.Count != 0) return;

            int pageNo = 1;
            string forumId = "2";

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
            var data = doc.DocumentNode.ChildNodes[2].ChildNodes[3].ChildNodes[10].ChildNodes[1].ChildNodes[1].ChildNodes[9].ChildNodes[7].ChildNodes;

            // 这个参数的目的是为了过滤掉首页长期置顶的贴子后，就不再判断每个节点是否是置顶贴，以节省性能
            bool isNormalThread = false;
            
            Forum forum = new Forum(forumId, "Discovery", string.Empty, string.Empty);
            int i = 0;
            foreach (var item in data)
            {
                if (isNormalThread == false && pageNo == 1)
                {
                    if (item.Attributes.Count() == 1 && item.Attributes[0].Value.Contains("normalthread_"))
                    {
                        var tr = item.ChildNodes[1];
                        var tdSubject = tr.ChildNodes[5];
                        var tdSubjectSpan = tdSubject.ChildNodes[3];
                        var tdSubjectSpanA = tdSubjectSpan.ChildNodes[0];
                        //var tdAuthor = tr.ChildNodes[7];
                        //var tdNums = tr.ChildNodes[9];
                        //var tdLastPost = tr.ChildNodes[11];

                        string id = tdSubjectSpan.Attributes[0].Value.Substring("thread_".Length);
                        string title = tdSubjectSpanA.InnerText;

                        Thread thread = new Thread(i, id, title, "1", "2", "3", "4", "5");

                        forum.Threads.Add(thread);
                        isNormalThread = true;
                    }
                }
                else
                {
                    var tr = item.ChildNodes[1];
                    var tdSubject = tr.ChildNodes[5];
                    var tdSubjectSpan = tdSubject.ChildNodes[3];
                    var tdSubjectSpanA = tdSubjectSpan.ChildNodes[0];
                    //var tdAuthor = tr.ChildNodes[7];
                    //var tdNums = tr.ChildNodes[9];
                    //var tdLastPost = tr.ChildNodes[11];

                    string id = tdSubjectSpan.Attributes[0].Value.Substring("thread_".Length);
                    string title = tdSubjectSpanA.InnerText;

                    Thread thread = new Thread(i, id, title, "1", "2", "3", "4", "5");

                    forum.Threads.Add(thread);
                    isNormalThread = true;
                }

                i++;
            }

            this.Forums.Add(forum);
        }


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
                    .Descendants().Where(n => n.GetAttributeValue("id", "").Contains("forum"));

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
        }

        // 读取指定贴子的回复列表数据
        public static async Task<Thread> LoadReplyDataAsync(string threadId)
        {
            HttpClient httpClient = new HttpClient();
            Helpers.CreateHttpClient(ref httpClient);

            CancellationTokenSource cts = new CancellationTokenSource();

            int pageNo = 1;

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/viewthread.php?tid={0}&page={1}", threadId, pageNo);
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url)).AsTask(cts.Token);
            response.Content.Headers.ContentType.CharSet = "GBK";

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            Thread thread = new Thread(1, threadId, "fadsf", "", "", "", "", "");

            // 载入HTML
            doc.LoadHtml(await response.Content.ReadAsStringAsync().AsTask(cts.Token));
            var data = doc.DocumentNode
                .ChildNodes[2] // html
                .ChildNodes[3] // body
                .ChildNodes[14] // div#wrap
                .ChildNodes[5] // div#postlist
                .ChildNodes;

            int i = 0;
            foreach (var item in data)
            {
                string ownerId = string.Empty;
                string ownerName = string.Empty;
                string postTime = string.Empty;
                try
                {
                    var authorNode = item.ChildNodes[0] // table
                    .ChildNodes[1] // tr
                    .ChildNodes[1] // td
                    .ChildNodes[1] // div
                    .ChildNodes[1]; // a

                    ownerId = authorNode.Attributes[1].Value.Substring("space.php?uid=".Length);
                    ownerName = authorNode.InnerText;
                }
                catch
                {
                    
                    throw new Exception("author node");
                }

                

                string content = string.Empty;
                if (i == 0)
                {
                    try
                    {
                        var postTimeNode = item.ChildNodes[0] // table
                        .ChildNodes[1] // tr
                        .ChildNodes[3] // td
                        .ChildNodes[3] // div.postinfo
                        .ChildNodes[3] // div.posterinfo
                        .ChildNodes[3] // div.authorinfo
                        .ChildNodes[1]; // em

                        postTime = postTimeNode.InnerText;
                    }
                    catch
                    {

                        throw new Exception("post time node");
                    }

                    try
                    {
                        content = item.ChildNodes[0] // table
                        .ChildNodes[1] // tr
                        .ChildNodes[3] // td.postcontent
                        .ChildNodes[5] // div.defaultpost
                        .ChildNodes[5] // div.postmessage
                        .ChildNodes[3] // div.t_msgfontfix
                        .ChildNodes[1] // table
                        .ChildNodes[0] // tr
                        .ChildNodes[0] // td
                        .InnerText;
                    }
                    catch
                    {
                        
                        throw new Exception("main post");
                    }
                }
                else
                {
                    try
                    {
                        var postTimeNode = item.ChildNodes[0] // table
                        .ChildNodes[1] // tr
                        .ChildNodes[3] // td
                        .ChildNodes[1] // div.postinfo
                        .ChildNodes[3] // div.posterinfo
                        .ChildNodes[3] // div.authorinfo
                        .ChildNodes[1]; // em

                        postTime = postTimeNode.InnerText;
                    }
                    catch
                    {

                        throw new Exception("post time node");
                    }

                    try
                    {
                        content = item.ChildNodes[0] // table
                        .ChildNodes[1] // tr
                        .ChildNodes[3] // td.postcontent
                        .ChildNodes[3] // div.defaultpost
                        .ChildNodes[5] // div.postmessage
                        .ChildNodes[1] // div.t_msgfontfix
                        .ChildNodes[1] // table
                        .ChildNodes[0] // tr
                        .ChildNodes[0] // td
                        .InnerText;
                    }
                    catch
                    {
                        
                        throw new Exception("others post");
                    }
                }

                Reply reply = new Reply(i, ownerId, ownerName, content, postTime);
                thread.Replies.Add(reply);
                
                i++;
            }

            return thread;
        }
    }
}
