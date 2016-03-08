using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.ViewModels;
using Hipda.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.Services
{
    public class SearchFullTextService
    {
        static List<ThreadItemForSearchFullTextModel> _threadDataForSearchFullText = new List<ThreadItemForSearchFullTextModel>();
        static HttpHandle _httpClient = HttpHandle.GetInstance();
        static int _pageSize = 50;
        int _threadMaxPageNoForSearchFullText = 1;

        async Task LoadThreadDataForSearchFullTextAsync(string searchKeyword, string searchAuthor, int searchTimeSpan, int searchForumSpan, int pageNo, CancellationTokenSource cts)
        {
            int count = _threadDataForSearchFullText.Count(t => t.PageNo == pageNo);
            if (count == _pageSize)
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

            string searchForumSpanStr = searchForumSpan == -1 ? "all" : searchForumSpan.ToString();

            string url = string.Format("http://www.hi-pda.com/forum/search.php?srchtype={2}&srchtxt={0}&searchsubmit=%CB%D1%CB%F7&st=on&srchuname={1}&srchfilter=all&srchfrom={3}&before=&orderby={5}&ascdesc=desc&srchfid%5B0%5D={4}&page={6}&_={7}",
                _httpClient.GetEncoding(searchKeyword), _httpClient.GetEncoding(searchAuthor), "fulltext", searchTimeSpanStr, searchForumSpanStr, "lastpost", pageNo, DateTime.Now.Ticks.ToString("x"));
            string htmlContent = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            // 最先读取提醒数据
            var promptContentNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("promptcontent"));
            PromptService.GetPromptData(promptContentNode);

            // 读取主内容
            var dataTable = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("datatable"));
            if (dataTable == null || dataTable.InnerText.Trim().Equals("对不起，没有找到匹配结果。"))
            {
                return;
            }

            // 读取最大页码
            var pagesNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("pages"));
            _threadMaxPageNoForSearchFullText = DataService.GetMaxPageNo(pagesNode);

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
                string titleHtml = string.Empty;
                int postId = 0;
                string summaryHtml = string.Empty;
                string forumName = string.Empty;
                string replyCount = string.Empty;
                string viewCount = string.Empty;
                string authorUsername = string.Empty;
                int authorUserId = 0;
                string lastReplyTime = string.Empty;

                try
                {
                    var div = r.ChildNodes[0].ChildNodes[0];
                    var div1 = div.ChildNodes[1];
                    var div2 = div.ChildNodes[3];
                    var div3 = div.ChildNodes[5];

                    var titleNode = div1.ChildNodes[2];
                    string postIdStr = titleNode.Attributes[0].Value.Substring("gotopost.php?pid=".Length).Split('&')[0];
                    if (!string.IsNullOrEmpty(postIdStr))
                    {
                        postId = Convert.ToInt32(postIdStr);
                        titleHtml = titleNode.InnerHtml;
                        summaryHtml = div2.InnerHtml;
                        forumName = div3.ChildNodes[1].ChildNodes[1].InnerText.Trim();
                        var authorNode = div3.ChildNodes[3];
                        authorUsername = authorNode.InnerText.Replace("作者: ", string.Empty).Trim();
                        authorUserId = Convert.ToInt32(authorNode.ChildNodes[1].Attributes[0].Value.Substring("space.php?uid=".Length).Split('&')[0]);
                        viewCount = div3.ChildNodes[5].InnerText.Trim().Replace("查看: ", string.Empty).Trim();
                        replyCount = div3.ChildNodes[7].InnerText.Trim().Replace("回复: ", string.Empty).Trim();
                        lastReplyTime = div3.ChildNodes[9].InnerText.Trim().Replace("最后发表: ", string.Empty).Trim();
                    }

                    var threadItem = new ThreadItemForSearchFullTextModel(i, postId, summaryHtml, forumName, pageNo, titleHtml, replyCount, viewCount, authorUsername, authorUserId, lastReplyTime);
                    _threadDataForSearchFullText.Add(threadItem);

                    i++;
                }
                catch (Exception ex)
                {
                    string errorDetails = "k: {0};p: {1};i: {2};t: {3};d: {4}";
                    errorDetails = string.Format(errorDetails, searchKeyword, pageNo, i, titleHtml, ex.Message);
                    Common.PostErrorEmailToDeveloper("全文搜索解析出现异常", errorDetails);
                }
            }
        }

        async Task<int> GetMoreThreadItemsForSearchFullTextAsync(string searchKeyword, string searchAuthor, int searchTimeSpan, int searchForumSpan, int pageNo, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadThreadDataForSearchFullTextAsync(searchKeyword, searchAuthor, searchTimeSpan, searchForumSpan, pageNo, cts);
            if (_threadDataForSearchFullText.Count == 0 && noDataNotice != null) noDataNotice();
            if (afterLoad != null) afterLoad();

            return _threadDataForSearchFullText.Count;
        }

        ThreadItemForSearchFullTextModel GetOneThreadItemForSearchFullText(int index)
        {
            return _threadDataForSearchFullText.FirstOrDefault(t => t.Index == index);
        }

        public ICollectionView GetViewForThreadPageForSearchFullText(int startPageNo, string searchKeyword, string searchAuthor, int searchTimeSpan, int searchForumSpan, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<ThreadItemForSearchFullTextModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await GetMoreThreadItemsForSearchFullTextAsync(searchKeyword, searchAuthor, searchTimeSpan, searchForumSpan, pageNo, beforeLoad, afterLoad, noDataNotice);
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
