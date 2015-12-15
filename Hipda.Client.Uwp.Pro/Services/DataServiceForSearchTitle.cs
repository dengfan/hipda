using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.ViewModels;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.Services
{
    public partial class DataService
    {
        static List<ThreadItemForSearchTitleModel> _threadDataForSearchTitle = new List<ThreadItemForSearchTitleModel>();
        int _threadMaxPageNoForSearchTitle = 1;

        async Task LoadThreadDataForSearchTitleAsync(string searchKeyword, string searchAuthor, int searchType, int searchTimeSpan, int searchForumSpan, int pageNo, CancellationTokenSource cts)
        {
            int count = _threadDataForSearchTitle.Count(t => t.PageNo == pageNo);
            if (count == _threadPageSize)
            {
                return;
            }
            else
            {
                _threadDataForSearchTitle.RemoveAll(t => t.PageNo == pageNo);
            }

            // 读取数据
            string searchTypeStr = searchType == 0 ? "title" : "fulltext";
            string searchTimeSpanStr = string.Empty;
            switch (searchTimeSpan)
            {
                case 1:
                    searchTimeSpanStr = "604800"; // 一周
                    break;
                case 2:
                    searchTimeSpanStr = "2592000"; // 一个月
                    break;
                case 3:
                    searchTimeSpanStr = "7776000"; // 三个月
                    break;
                case 4:
                    searchTimeSpanStr = "31536000"; // 一年
                    break;
                default:
                    searchTimeSpanStr = "0"; // 全部时间
                    break;
            }
            string searchForumSpanStr = "all";

            string url = string.Format("http://www.hi-pda.com/forum/search.php?srchtype={2}&srchtxt={0}&searchsubmit=%CB%D1%CB%F7&st=on&srchuname={1}&srchfilter=all&srchfrom={3}&before=&orderby={5}&ascdesc=desc&srchfid%5B0%5D={4}&page={6}&_={7}",
                searchKeyword, searchAuthor, searchTypeStr, searchTimeSpanStr, searchForumSpanStr, "lastpost", pageNo, DateTime.Now.Ticks.ToString("x"));
            string htmlContent = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            var dataTable = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("datatable"));
            if (dataTable == null || dataTable.InnerText.Trim().Equals("对不起，没有找到匹配结果。"))
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
                _threadMaxPageNoForSearchTitle = Convert.ToInt32(lastPageNodeValue);
            }

            if (pageNo > _threadMaxPageNoForSearchTitle)
            {
                return;
            }

            var tbodies = dataTable.Descendants().Where(n => n.Name.Equals("tbody"));
            if (tbodies == null)
            {
                return;
            }

            int i = _threadDataForSearchTitle.Count;
            foreach (var item in tbodies)
            {
                var tr = item.ChildNodes[1];
                var th = tr.ChildNodes[5];
                var a = th.ChildNodes[3];
                var tdAuthor = tr.ChildNodes[9];
                var tdNums = tr.ChildNodes[11];
                var tdLastPost = tr.ChildNodes[13];

                string threadIdStr = a.Attributes[0].Value.Substring("viewthread.php?tid=".Length);
                threadIdStr = threadIdStr.Split('&')[0];
                int threadId = Convert.ToInt32(threadIdStr);

                string title = a.InnerHtml;

                // 替换搜索关键字，用于高亮关键字
                MatchCollection matchsForSearchKeywords = new Regex(@"<em style=""color:red;"">([^>#]*)</em>").Matches(title);
                if (matchsForSearchKeywords != null && matchsForSearchKeywords.Count > 0)
                {
                    for (int j = 0; j < matchsForSearchKeywords.Count; j++)
                    {
                        var m = matchsForSearchKeywords[j];

                        string placeHolder = m.Groups[0].Value; // 要被替换的元素
                        string k = m.Groups[1].Value;

                        string linkXaml = string.Format(@"  “{0}”  ", k);
                        title = title.Replace(placeHolder, linkXaml);
                    }
                }

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

                var forumNameNode = item.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("forum"));
                string forumName = forumNameNode.InnerText.Trim();

                var authorUsername = string.Empty;
                int authorUserId = 0;
                var authorNameNode = tdAuthor.ChildNodes[1]; // cite 此节点有出“匿名”的可能
                var authorNameLink = authorNameNode.Descendants().FirstOrDefault(n => n.Name.Equals("a"));
                if (authorNameLink == null)
                {
                    authorUsername = authorNameNode.InnerText.Trim();
                }
                else
                {
                    authorUsername = authorNameLink.InnerText;
                    string authorUserIdStr = authorNameLink.Attributes[0].Value.Substring("space.php?uid=".Length);
                    authorUserIdStr = authorUserIdStr.Split('&')[0];
                    authorUserId = Convert.ToInt32(authorUserIdStr);
                }

                var authorCreateTime = tdAuthor.ChildNodes[3].InnerText;

                string[] nums = tdNums.InnerText.Split('/');
                var replyCount = nums[0].Trim();
                var viewCount = nums[1].Trim();

                string lastReplyUsername = "匿名";
                string lastReplyTime = string.Empty;
                string[] lastPostInfo = tdLastPost.InnerText.Trim().Replace("\n", "@").Split('@');
                if (lastPostInfo.Length == 2)
                {
                    lastReplyUsername = lastPostInfo[0];
                    lastReplyTime = lastPostInfo[1]
                        .Replace(string.Format("{0}-{1}-{2} ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), string.Empty)
                        .Replace(string.Format("{0}-", DateTime.Now.Year), string.Empty);
                }

                var threadItem = new ThreadItemForSearchTitleModel(i, forumName, threadId, pageNo, title, attachType, replyCount, viewCount, authorUsername, authorUserId, authorCreateTime, lastReplyUsername, lastReplyTime);
                _threadDataForSearchTitle.Add(threadItem);

                i++;
            }
        }

        async Task<int> GetMoreThreadItemsForSearchTitleAsync(string searchKeyword, string searchAuthor, int searchType, int searchTimeSpan, int searchForumSpan, int pageNo, Action beforeLoad, Action afterLoad)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadThreadDataForSearchTitleAsync(searchKeyword, searchAuthor, searchType, searchTimeSpan, searchForumSpan, pageNo, cts);
            if (afterLoad != null) afterLoad();

            return _threadDataForSearchTitle.Count;
        }

        ThreadItemForSearchTitleViewModel GetOneThreadItemForSearchTitle(int index)
        {
            var threadItem = _threadDataForSearchTitle.FirstOrDefault(t => t.Index == index);
            if (threadItem == null)
            {
                return null;
            }

            var vm = new ThreadItemForSearchTitleViewModel(threadItem);
            vm.StatusColorStyle = GetReadStatusStyle(threadItem.ThreadId);

            return vm;
        }

        public ICollectionView GetViewForThreadPageForSearchTitle(int startPageNo, string searchKeyword, string searchAuthor, int searchType, int searchTimeSpan, int searchForumSpan, Action beforeLoad, Action afterLoad)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<ThreadItemForSearchTitleViewModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await GetMoreThreadItemsForSearchTitleAsync(searchKeyword, searchAuthor, searchType, searchTimeSpan, searchForumSpan, pageNo, beforeLoad, afterLoad);
                },
                (index) =>
                {
                    // 从静态类中返回需要显示出来的数据
                    return GetOneThreadItemForSearchTitle(index);
                },
                () =>
                {
                    return GetThreadMaxPageNoForSearchTitle();
                });

            return cvs.View;
        }

        public int GetThreadMaxPageNoForSearchTitle()
        {
            return _threadMaxPageNoForSearchTitle;
        }

        public void ClearThreadDataForSearchTitle()
        {
            _threadDataForSearchTitle.Clear();
        }

        public ThreadItemForSearchTitleModel GetThreadItemForSearchTitle(int threadId)
        {
            return _threadDataForSearchTitle.FirstOrDefault(t => t.ThreadId == threadId);
        }
    }
}
