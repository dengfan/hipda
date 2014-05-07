using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.Web.Http;

namespace hipda.Data
{
    public class Reply
    {
        public Reply(string id, string ownerName, string ownerId, string content, string createTime)
        {
            this.Id = id;
            this.OwnerName = ownerName;
            this.OwnerId = ownerId;
            this.Content = content;
            this.CreateTime = createTime;
        }

        public string Id { get; private set; }

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
        public Thread(string id, string title, string replyAndViewInfo, string ownerName, string ownerId, string createTime, string lastReplyTime)
        {
            this.Id = id;
            this.Title = title;
            this.ReplyAndViewInfo = replyAndViewInfo;
            this.OwnerName = ownerName;
            this.OwnerId = ownerId;
            this.CreateTime = createTime;
            this.LastReplyTime = lastReplyTime;
            //this.Replys = new ObservableCollection<Reply>();
        }

        public string Id { get; private set; }
        public string Title { get; private set; }
        public string ReplyAndViewInfo { get; private set; }
        public string OwnerName { get; private set; }
        public string OwnerId { get; private set; }
        public string CreateTime { get; private set; }
        public string LastReplyTime { get; private set; }
        //public ObservableCollection<Reply> Replys { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    public class Forum
    {
        public Forum(string id, string name)
        {
            this.Id = id;
            this.Name = name;
            this.Threads = new ObservableCollection<Thread>();
        }

        public string Id { get; private set; }
        public string Name { get; private set; }

        public ObservableCollection<Thread> Threads { get; private set; }

        public override string ToString()
        {
            return this.Name;
        }
 
    }

    public sealed class DataSource
    {
        private static DataSource _dataSource = new DataSource();

        private ObservableCollection<Forum> _forums = new ObservableCollection<Forum>();
        public ObservableCollection<Forum> Forums
        {
            get { return this._forums; }
        }

        public static async Task<IEnumerable<Forum>> GetForumsAsync()
        {
            await _dataSource.LoadDataAsync();

            return _dataSource.Forums;
        }

        public static async Task<Forum> GetForumAsync(string uniqueId)
        {
            await _dataSource.LoadDataAsync();

            // 对于小型数据集可接受简单线性搜索
            var matches = _dataSource.Forums.Where((f) => f.Id.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<Thread> GetThreadAsync(string uniqueId)
        {
            await _dataSource.LoadDataAsync();

            // 对于小型数据集可接受简单线性搜索
            var matches = _dataSource.Forums.SelectMany(forum => forum.Threads).Where((thread) => thread.Id.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private async Task LoadDataAsync()
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
            
            Forum forum = new Forum(forumId, "Discovery");
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

                        Thread thread = new Thread(id, title, "1", "2", "3", "4", "5");

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

                    Thread thread = new Thread(id, title, "1", "2", "3", "4", "5");

                    forum.Threads.Add(thread);
                    isNormalThread = true;
                }
            }

            this.Forums.Add(forum);
        }
    }
}
