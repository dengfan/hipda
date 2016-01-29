using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.ViewModels;
using Hipda.Http;
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
    public class DataServiceForDefault
    {
        static RoamingSettingsDependencyObject _myRoamingSettings = ((RoamingSettingsDependencyObject)App.Current.Resources["MyRoamingSettings"]);
        static List<ThreadItemModel> _threadData = new List<ThreadItemModel>();
        static HttpHandle _httpClient = HttpHandle.GetInstance();
        static int _pageSize = 75;
        int _maxPageNo = 1;

        async Task LoadThreadDataAsync(int forumId, int pageNo, CancellationTokenSource cts)
        {
            int count = _threadData.Count(t => t.ForumId == forumId && t.PageNo == pageNo);
            if (count == _pageSize)
            {
                return;
            }
            else
            {
                _threadData.RemoveAll(t => t.ForumId == forumId && t.PageNo == pageNo);
            }

            if (pageNo == 1)
            {
                // 第一次打开或刷新时，先请求一下，看看是否有新私信
                // 这是论坛的一种机制，否则APP过了一段时间之后不能从HTML中解析私人消息的数量
                await _httpClient.GetAsync(string.Format("http://www.hi-pda.com/forum/pm.php?checknewpm={0}&inajax=1&ajaxtarget=myprompt_check", DateTime.Now.Ticks.ToString("x")), cts);
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
            var promptContentNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("promptcontent"));
            DataService.GetPromptData(promptContentNode);

            // 读取主内容
            var dataTable = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("table") && n.GetAttributeValue("class", "").Equals("datatable"));
            if (dataTable == null)
            {
                _maxPageNo = -1; // 找不到目标数据，一般为得到的是提示登录的页面的HTML，故在此将最大页标记为-1，以便增量加载不再继续
                return;
            }

            // 读取所属版块之名称
            string forumName = string.Empty;
            var titleNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("title"));
            if (titleNode != null)
            {
                string[] tary = titleNode.InnerText.Trim().Split('-');
                forumName = tary[0].Trim();
            }

            // 读取最大页码
            var pagesNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("pages"));
            _maxPageNo = DataService.GetMaxPageNo(pagesNode);

            if (pageNo > _maxPageNo)
            {
                return;
            }

            // 如果置顶贴数过多，只取非置顶贴的话，第一页数据项过少，会导致不会自动触发加载下一页数据
            var tbodies = dataTable.ChildNodes.Where(n => n.GetAttributeValue("id", "").Contains("stickthread_") || n.GetAttributeValue("id", "").Contains("normalthread_"));
            if (tbodies == null)
            {
                return;
            }

            int i = _threadData.Count(t => t.ForumId == forumId);
            foreach (var item in tbodies)
            {
                var tr = item.ChildNodes[1];
                var th = tr.ChildNodes[5];
                var span = th.ChildNodes.FirstOrDefault(n => n.Name.Equals("span") && n.GetAttributeValue("id", "").StartsWith("thread_"));
                var a = span.ChildNodes[0];
                var tdAuthor = tr.ChildNodes[7];
                var tdNums = tr.ChildNodes[9];
                var tdLastPost = tr.ChildNodes[11];

                int threadId = Convert.ToInt32(span.Attributes[0].Value.Substring("thread_".Length));
                string title = a.InnerText.Trim();

                // 判断当前主题是否已被屏蔽，是则跳过
                if (_myRoamingSettings.BlockThreads.Any(t => t.ThreadId == threadId))
                {
                    continue;
                }

                var authorName = string.Empty;
                int authorUserId = 0;
                var authorNameNode = tdAuthor.ChildNodes[1]; // cite 此节点有出“匿名”的可能
                var authorNameLink = authorNameNode.ChildNodes.FirstOrDefault(n => n.Name.Equals("a"));
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

                // 判断当前版块下的当前用户是否已被屏蔽，是则跳过
                if (_myRoamingSettings.BlockUsers.Any(u => u.UserId == authorUserId && u.ForumId == forumId))
                {
                    continue;
                }

                bool isTop = item.GetAttributeValue("id", "").StartsWith("stickthread_");

                // 根据“是否显示置顶贴”的设置值来决定是否跳过当前主题
                if (!_myRoamingSettings.CanShowTopThread && isTop)
                {
                    continue;
                }

                int attachType = -1;
                var attachIconNode = th.ChildNodes.FirstOrDefault(n => n.Name.Equals("img") && n.GetAttributeValue("class", "").Equals("attach"));
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

                var threadItem = new ThreadItemModel(i, forumId, forumName, threadId, pageNo, title, attachType, replyNum, viewNum, isTop, authorName, authorUserId, authorCreateTime, lastPostAuthorName, lastPostTime, AccountService.UserId == authorUserId);
                _threadData.Add(threadItem);

                i++;
            }
        }

        async Task<int> GetMoreThreadItemsAsync(int forumId, int pageNo, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadThreadDataAsync(forumId, pageNo, cts);
            if (_threadData.Count == 0 && noDataNotice != null) noDataNotice();
            if (afterLoad != null) afterLoad();

            return _threadData.Count(t => t.ForumId == forumId);
        }

        ThreadItemModel GetOneThread(int forumId, int index)
        {
            return _threadData.FirstOrDefault(t => t.ForumId == forumId && t.Index == index);
        }

        public ICollectionView GetViewForThreadItems(int startPageNo, int forumId, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<ThreadItemModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await GetMoreThreadItemsAsync(forumId, pageNo, beforeLoad, afterLoad, noDataNotice);
                },
                (index) =>
                {
                    // 从静态类中返回需要显示出来的数据
                    return GetOneThread(forumId, index);
                },
                () =>
                {
                    return GetThreadMaxPageNo();
                });

            return cvs.View;
        }

        public int GetThreadMaxPageNo()
        {
            return _maxPageNo;
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
