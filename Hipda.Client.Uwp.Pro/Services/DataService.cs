using Hipda.Client.Uwp.Pro.Models;
using Hipda.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Services
{
    public class DataService
    {
        private static HttpHandle httpClient = HttpHandle.getInstance();

        public static ObservableCollection<ForumDataModel> DataBase { get; private set; }

        /// <summary>
        /// 获取指定版区的贴子列表数据
        /// </summary>
        /// <param name="argForumId">版区ID</param>
        /// <returns>贴子列表</returns>
        public async Task<ThreadPageModel> GetThreadPageListByForumId(string forumId, int pageNo, CancellationTokenSource cts)
        {
            await GetThreadsDataAsync(forumId, pageNo, cts);

            var forum = DataBase.FirstOrDefault(t => t.ForumId.Equals(forumId));
            var threadPage = forum.ThreadData.FirstOrDefault(t => t.PageNo == pageNo);
            return threadPage;
        }

        private async Task GetThreadsDataAsync(string forumId, int pageNo, CancellationTokenSource cts)
        {
            // 如果数据库中不存在该版，则创建之
            var forum = DataBase.FirstOrDefault(t => t.ForumId.Equals(forumId));
            if (forum == null)
            {
                forum = new ForumDataModel();
                forum.ForumId = forumId;
                DataBase.Add(forum);
            }

            // 如果该页贴子已存在，且贴子数量为满页，则返回
            var threadPage = forum.ThreadData.FirstOrDefault(t => t.PageNo == pageNo);
            if (threadPage != null)
            {
                if (threadPage.ThreadItems.Count == 50)
                {
                    return;
                }
            }
            else
            {
                threadPage = new ThreadPageModel();
                threadPage.PageNo = pageNo;
                threadPage.ThreadItems = new ObservableCollection<ThreadItemModel>();
            }

            // 读取数据
            string ThreadListPageOrderBy = string.Empty;
            string url = string.Format("http://www.hi-pda.com/forum/forumdisplay.php?fid={0}&orderby={1}&page={2}&_={3}", forumId, ThreadListPageOrderBy, pageNo, DateTime.Now.Ticks.ToString("x"));
            string htmlContent = await httpClient.GetAsync(url, cts);

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
                var authorNameNode = tdAuthor.ChildNodes[1]; // cite 此节点有出“匿名”的可能
                var authorNameLink = authorNameNode.Descendants().FirstOrDefault(n => n.Name.Equals("a"));
                if (authorNameLink == null)
                {
                    authorName = authorNameNode.InnerText.Trim();
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

                ThreadItemModel threadItem = new ThreadItemModel(pageNo, forumId, id, title, attachType, replyNum, viewNum, authorName, authorId, authorCreateTime, lastPostAuthorName, lastPostTime);
                threadPage.ThreadItems.Add(threadItem);

                i++;
            }
        }
    }
}
