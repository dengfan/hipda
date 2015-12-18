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
        static List<ThreadItemForSearchFullTextModel> _threadDataForSearchFullText = new List<ThreadItemForSearchFullTextModel>();
        int _threadMaxPageNoForSearchFullText = 1;

        async Task LoadThreadDataForSearchFullTextAsync(string searchKeyword, string searchAuthor, int searchTimeSpan, int searchForumSpan, int pageNo, CancellationTokenSource cts)
        {
            int count = _threadDataForSearchFullText.Count(t => t.PageNo == pageNo);
            if (count == _threadPageSize)
            {
                return;
            }
            else
            {
                _threadDataForSearchFullText.RemoveAll(t => t.PageNo == pageNo);
            }

            // 读取数据
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

            string searchForumSpanStr = searchForumSpan == 1 ? "all" : searchForumSpan.ToString();

            string url = string.Format("http://www.hi-pda.com/forum/search.php?srchtype={2}&srchtxt={0}&searchsubmit=%CB%D1%CB%F7&st=on&srchuname={1}&srchfilter=all&srchfrom={3}&before=&orderby={5}&ascdesc=desc&srchfid%5B0%5D={4}&page={6}&_={7}",
                _httpClient.GetEncoding(searchKeyword), _httpClient.GetEncoding(searchAuthor), "fulltext", searchTimeSpanStr, searchForumSpanStr, "lastpost", pageNo, DateTime.Now.Ticks.ToString("x"));
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
                _threadMaxPageNoForSearchFullText = Convert.ToInt32(lastPageNodeValue);
            }

            if (pageNo > _threadMaxPageNoForSearchFullText)
            {
                return;
            }

            var rows = dataTable.Descendants().Where(n => n.Name.Equals("tr"));
            if (rows == null)
            {
                return;
            }

            int i = _threadDataForSearchFullText.Count;
            foreach (var r in rows)
            {
                var div = r.ChildNodes[0].ChildNodes[0];
                var div1 = div.ChildNodes[1];
                var div2 = div.ChildNodes[3];
                var div3 = div.ChildNodes[5];

                var titleNode = div1.ChildNodes[2];
                int postId = Convert.ToInt32(titleNode.Attributes[0].Value.Substring("gotopost.php?pid=".Length).Split('&')[0]);
                string titleHtml = titleNode.InnerHtml;
                string summaryHtml = div2.InnerHtml;
                string forumName = div3.ChildNodes[1].ChildNodes[1].InnerText.Trim();
                var authorNode = div3.ChildNodes[3];
                string authorUsername = authorNode.InnerText.Replace("作者: ", string.Empty).Trim();
                int authorUserId = Convert.ToInt32(authorNode.ChildNodes[1].Attributes[0].Value.Substring("space.php?uid=".Length).Split('&')[0]);
                string viewCount = div3.ChildNodes[5].InnerText.Trim().Replace("查看: ", string.Empty).Trim();
                string replyCount = div3.ChildNodes[7].InnerText.Trim().Replace("回复: ", string.Empty).Trim();
                string lastReplyTime = div3.ChildNodes[9].InnerText.Trim().Replace("最后发表: ", string.Empty).Trim();

                var threadItem = new ThreadItemForSearchFullTextModel(i, postId, summaryHtml, forumName, pageNo, titleHtml, replyCount, viewCount, authorUsername, authorUserId, lastReplyTime);
                _threadDataForSearchFullText.Add(threadItem);

                i++;
            }
        }

        async Task<int> GetMoreThreadItemsForSearchFullTextAsync(string searchKeyword, string searchAuthor, int searchTimeSpan, int searchForumSpan, int pageNo, Action beforeLoad, Action afterLoad)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadThreadDataForSearchFullTextAsync(searchKeyword, searchAuthor, searchTimeSpan, searchForumSpan, pageNo, cts);
            if (afterLoad != null) afterLoad();

            return _threadDataForSearchFullText.Count;
        }

        ThreadItemForSearchFullTextViewModel GetOneThreadItemForSearchFullText(int index)
        {
            var threadItem = _threadDataForSearchFullText.FirstOrDefault(t => t.Index == index);
            if (threadItem == null)
            {
                return null;
            }

            var vm = new ThreadItemForSearchFullTextViewModel(threadItem);
            vm.StatusColorStyle = GetReadStatusStyle(threadItem.ThreadId);

            return vm;
        }

        public ICollectionView GetViewForThreadPageForSearchFullText(int startPageNo, string searchKeyword, string searchAuthor, int searchTimeSpan, int searchForumSpan, Action beforeLoad, Action afterLoad)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<ThreadItemForSearchFullTextViewModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await GetMoreThreadItemsForSearchFullTextAsync(searchKeyword, searchAuthor, searchTimeSpan, searchForumSpan, pageNo, beforeLoad, afterLoad);
                },
                (index) =>
                {
                    // 从静态类中返回需要显示出来的数据
                    return GetOneThreadItemForSearchFullText(index);
                },
                () =>
                {
                    return GetThreadMaxPageNoForSearchFullText();
                });

            return cvs.View;
        }

        public int GetThreadMaxPageNoForSearchFullText()
        {
            return _threadMaxPageNoForSearchFullText;
        }

        public void ClearThreadDataForSearchFullText()
        {
            _threadDataForSearchFullText.Clear();
        }

        public ThreadItemForSearchFullTextModel GetThreadItemForSearchFullText(int threadId)
        {
            return _threadDataForSearchFullText.FirstOrDefault(t => t.ThreadId == threadId);
        }
    }
}
