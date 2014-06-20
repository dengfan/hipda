﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

    public class ReplyItem
    {
        public ReplyItem(string threadId, string threadName)
        {
            this.ThreadId = threadId;
            this.ThreadName = threadName;
            this.Replies = new ObservableCollection<Reply>();
        }
        public string ThreadId { get; private set; }
        public string ThreadName { get; private set; }
        public ObservableCollection<Reply> Replies { get; private set; }
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

    public class ThreadItem
    {
        public ThreadItem(string forumId, string forumName)
        {
            this.ForumId = forumId;
            this.ForumName = forumName;
            this.Threads = new ObservableCollection<Thread>();
        }
        public string ForumId { get; private set; }

        public string ForumName { get; private set; }

        public ObservableCollection<Thread> Threads { get; private set; }
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

    public sealed class DataSource
    {
        public static int ThreadPageSize { get { return 75; } }
        public static int ReplyPageSize { get { return 50; } }

        private static DataSource _dataSource = new DataSource();

        private static HttpHandle httpClient = HttpHandle.getInstance();

        private ObservableCollection<ForumGroup> _forumGroups = new ObservableCollection<ForumGroup>();
        public ObservableCollection<ForumGroup> ForumGroups
        {
            get { return this._forumGroups; }
        }

        private ObservableCollection<ThreadItem> _threadList = new ObservableCollection<ThreadItem>();
        public ObservableCollection<ThreadItem> ThreadList
        {
            get { return this._threadList; }
        }

        private List<ReplyItem> _replyList = new List<ReplyItem>();
        public List<ReplyItem> ReplyList
        {
            get { return this._replyList; }
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
        public static ThreadItem GetThread(string forumId, string forumName)
        {
            var count = _dataSource.ThreadList.Count(t => t.ForumId.Equals(forumId));
            if (count == 0)
            {
                _dataSource.ThreadList.Add(new ThreadItem(forumId, forumName));
            }

            return _dataSource.ThreadList.Single(t => t.ForumId.Equals(forumId));
        }

        public static async Task RefreshThread(string forumId)
        {
            var threadItem = _dataSource.ThreadList.FirstOrDefault(t => t.ForumId.Equals(forumId));
            if (threadItem != null)
            {
                threadItem.Threads.Clear();
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
            return _dataSource.ThreadList.Single(f => f.ForumId.Equals(forumId)).Threads.ElementAt(index);
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
        public static ReplyItem GetReply(string threadId, string threadName)
        {
            var count = _dataSource.ReplyList.Count(t => t.ThreadId.Equals(threadId));
            if (count == 0)
            {
                _dataSource.ReplyList.Add(new ReplyItem(threadId, threadName));
            }

            return _dataSource.ReplyList.Single(t => t.ThreadId.Equals(threadId));
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
            return _dataSource.ReplyList.Single(r => r.ThreadId.Equals(threadId)).Replies.OrderBy(r => r.Floor).ElementAt(index);
        }

        // 读取指定贴子的回复列表数据
        private async Task LoadRepliesDataAsync(string threadId, int pageNo)
        {
            #region 如果数据已存在，则不读取，以便节省流量
            // 载入过的页面不再载入
            var thread = _dataSource.ReplyList.FirstOrDefault(t => t.ThreadId.Equals(threadId));
            if (thread == null)
            {
                return;
            }

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
            string url = string.Format("http://www.hi-pda.com/forum/viewthread.php?tid={0}&page={1}&" + DateTime.Now.Second, threadId, pageNo);
            string htmlContent = await httpClient.HttpGet(url);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

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
                        .Replace(string.Format("{0}-{1}-{2} ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), string.Empty)
                        .Replace(string.Format("{0}-", DateTime.Now.Year), string.Empty);
                }

                string content = string.Empty;
                var contentNode = postContentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("class", "").Equals("t_msgfontfix"));
                if (contentNode != null)
                {
                    contentNode = contentNode.Descendants().SingleOrDefault(n => n.GetAttributeValue("id", "").StartsWith("postmessage_"));
                    content = contentNode.InnerHtml;

                    content = content.Replace("[", "［");
                    content = content.Replace("]", "］");
                    content = content.Replace("&nbsp;", "&#160;");

                    // 替换链接
                    MatchCollection matchsForLink = new Regex(@"<a\s+href=""([^""]*)""[^>]*>([^>]*)</a>").Matches(content);
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
                            string linkXaml = string.Format(@"[Hyperlink NavigateUri=""{0}"" Foreground=""Blue""]{1}[/Hyperlink]", linkUrl, linkContent);
                            content = content.Replace(placeHolder, linkXaml);
                        }
                    }

                    content = content.Replace("<br/>", "[LineBreak/]");
                    content = content.Replace("<br />", "[LineBreak/]");
                    content = content.Replace("<br>", "[LineBreak/]");

                    // 替换引用 
                    content = content.Replace("<blockquote>", @"[LineBreak/][Span Foreground=""DimGray""]");
                    content = content.Replace("</blockquote>", "[/Span]");

                    // 移除无意义图片HTML
                    content = content.Replace(@"<img src=""images/default/attachimg.gif"" border=""0"">", string.Empty);

                    // 将HTML字符串转换为RichTextBlock XAML字符串
                    // 替换上载图片
                    MatchCollection matchsForImage1 = new Regex(@"<img\b[^>]*?\bfile[\s]*=[\s]*[""']?[\s]*([^""'>]*)[^>]*?/?[\s]*>").Matches(content);
                    if (matchsForImage1 != null && matchsForImage1.Count > 0)
                    {
                        for (int i = 0; i < matchsForImage1.Count; i++)
                        {
                            string placeHolderLabel = matchsForImage1[i].Groups[0].Value; // 要被替换的元素
                            string imgUrl = matchsForImage1[i].Groups[1].Value; // 图片URL
                            string imgXaml = string.Format(@"[InlineUIContainer][Image Stretch=""None""][Image.Source][BitmapImage UriSource=""http://www.hi-pda.com/forum/{0}"" /][/Image.Source][/Image][/InlineUIContainer]", imgUrl); 
                            if (imgUrl.StartsWith("attachments/"))
                            {
                                imgXaml = string.Format(@"[InlineUIContainer][Image Stretch=""Uniform"" Width=""320""][Image.Source][BitmapImage DecodePixelWidth=""320"" UriSource=""http://www.hi-pda.com/forum/{0}"" /][/Image.Source][/Image][/InlineUIContainer]", imgUrl); 
                            }
                            content = content.Replace(placeHolderLabel, imgXaml);
                        }
                    }

                    // 替换网络图片
                    MatchCollection matchsForImage2 = new Regex(@"<img\b[^>]*?\bsrc[\s]*=[\s]*[""']?[\s]*([^""'>]*)[^>]*?/?[\s]*>").Matches(content);
                    if (matchsForImage2 != null && matchsForImage2.Count > 0)
                    {
                        for (int i = 0; i < matchsForImage2.Count; i++)
                        {
                            var m = matchsForImage2[i];
                            string imgXaml = string.Empty;
                            string placeHolderLabel = m.Groups[0].Value; // 要被替换的元素
                            string imgUrl = m.Groups[1].Value; // 图片URL
                            
                            if (imgUrl.EndsWith("/back.gif") || imgUrl.StartsWith("images/smilies/"))
                            {
                                imgXaml = @"[InlineUIContainer][Image Stretch=""None""][Image.Source][BitmapImage UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";
                            }
                            else
                            {
                                imgXaml = @"[InlineUIContainer][Image Stretch=""Uniform"" Width=""320""][Image.Source][BitmapImage DecodePixelWidth=""320"" UriSource=""{0}"" /][/Image.Source][/Image][/InlineUIContainer]";
                            }

                            if (!imgUrl.StartsWith("http"))
                            {
                                imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;
                            }

                            imgXaml = string.Format(imgXaml, imgUrl);
                            content = content.Replace(placeHolderLabel, imgXaml);
                        }
                    }
                }
                else
                {
                    content = @"作者被禁止或删除&#160;内容自动屏蔽";
                }

                content = string.Format(@"[RichTextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Margin=""0,7,0,0""][Paragraph FontSize=""18""]{0}[/Paragraph][/RichTextBlock]", content);
                content = new Regex("<[^<]*>").Replace(content, string.Empty);
                content = new Regex("\r\n").Replace(content, string.Empty);
                content = new Regex("\r").Replace(content, string.Empty);
                content = new Regex("\n").Replace(content, string.Empty);
                content = content.Replace("[", "<");
                content = content.Replace("]", ">");

                Reply reply = new Reply(floor, pageNo, threadId, authorId, ownerName, content, postTime);
                thread.Replies.Add(reply);
            }
        }
        #endregion
    }
}