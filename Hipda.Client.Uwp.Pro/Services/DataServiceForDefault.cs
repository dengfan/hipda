using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.ViewModels;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.Services
{
    public partial class DataService
    {
        static List<ThreadItemModel> _threadData = new List<ThreadItemModel>();
        int _threadMaxPageNo = 1;

        async Task<bool> LoadThreadDataAsync(int forumId, int pageNo, CancellationTokenSource cts)
        {
            int count = _threadData.Count(t => t.ForumId == forumId && t.PageNo == pageNo);
            if (count == _threadPageSize)
            {
                return true;
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

            // 最先读取提醒数据
            var promptContentNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("promptcontent"));
            GetPromptData(promptContentNode);

            // 读取主内容
            var dataTable = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("datatable"));
            if (dataTable == null)
            {
                return false;
            }

            // 读取最大页码
            var pagesNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("pages"));
            _threadMaxPageNo = GetMaxPageNo(pagesNode);

            if (pageNo > _threadMaxPageNo)
            {
                return false;
            }

            // 如果置顶贴数过多，只取非置顶贴的话，第一页数据项过少，会导致不会自动触发加载下一页数据
            var tbodies = dataTable.Descendants().Where(n => n.GetAttributeValue("id", "").Contains("stickthread_") || n.GetAttributeValue("id", "").Contains("normalthread_"));
            if (tbodies == null)
            {
                return false;
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
                    lastPostAuthorName = lastPostInfo[0].Trim();
                    lastPostTime = lastPostInfo[1].Trim()
                        .Replace(string.Format("{0}-{1}-{2} ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), string.Empty)
                        .Replace(string.Format("{0}-", DateTime.Now.Year), string.Empty);
                }

                var threadItem = new ThreadItemModel(i, forumId, threadId, pageNo, title, attachType, replyNum, viewNum, isTop, authorName, authorUserId, authorCreateTime, lastPostAuthorName, lastPostTime, AccountService.UserId == authorUserId);
                _threadData.Add(threadItem);

                i++;
            }

            return true;
        }

        async Task<int> GetMoreThreadItemsAsync(int forumId, int pageNo, Action beforeLoad, Action afterLoad)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadThreadDataAsync(forumId, pageNo, cts);
            if (afterLoad != null) afterLoad();

            return _threadData.Count(t => t.ForumId == forumId);
        }

        ThreadItemViewModel GetOneThreadItem(int forumId, int index)
        {
            var threadItem = _threadData.FirstOrDefault(t => t.ForumId == forumId && t.Index == index);
            if (threadItem == null)
            {
                return null;
            }

            var vm = new ThreadItemViewModel(threadItem);
            vm.StatusColorStyle = GetReadStatusStyle(threadItem.ThreadId);

            return vm;
        }

        public async Task<ICollectionView> GetViewForThreadPage(int startPageNo, int forumId, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            // 预先加载一次，以判断是否有数据，不会浪费性能，因为如果本次载入了数据，后面就不用再载入了
            var hasData = await LoadThreadDataAsync(forumId, startPageNo, new CancellationTokenSource());
            if (hasData == false)
            {
                noDataNotice();
                return null;
            }

            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<ThreadItemViewModel>(
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

        public int GetThreadMaxPageNo()
        {
            return _threadMaxPageNo;
        }

        public void ClearThreadData(int forumId)
        {
            _threadData.RemoveAll(t => t.ForumId == forumId);
        }

        public ThreadItemModel GetThreadItem(int threadId)
        {
            return _threadData.FirstOrDefault(t => t.ThreadId == threadId);
        }
    }
}
