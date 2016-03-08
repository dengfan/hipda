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
    public class MyFavoritesService
    {
        static List<ThreadItemForMyFavoritesModel> _threadDataForMyFavorites = new List<ThreadItemForMyFavoritesModel>();
        static HttpHandle _httpClient = HttpHandle.GetInstance();
        static int _pageSize = 75;
        int _maxPageNo = 1;

        async Task LoadThreadDataForMyFavoritesAsync(int pageNo, CancellationTokenSource cts)
        {
            int count = _threadDataForMyFavorites.Count(t => t.PageNo == pageNo);
            if (count == _pageSize)
            {
                return;
            }
            else
            {
                _threadDataForMyFavorites.RemoveAll(t => t.PageNo == pageNo);
            }

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/my.php?item=favorites&type=thread&page={0}&_={1}", pageNo, DateTime.Now.Ticks.ToString("x"));
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
            if (dataTable == null || dataTable.InnerText.Trim().Equals("暂无数据"))
            {
                return;
            }

            // 读取最大页码
            var pagesNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("pages"));
            _maxPageNo = CommonService.GetMaxPageNo(pagesNode);

            if (pageNo > _maxPageNo)
            {
                return;
            }

            var rows = dataTable.ChildNodes[3].Descendants().Where(n => n.Name.Equals("tr")).ToList();
            if (rows == null)
            {
                return;
            }

            // 移除最后一行，注：最后一行是批量删除的按钮
            rows.RemoveAt(rows.Count - 1);

            int i = _threadDataForMyFavorites.Count;
            foreach (var item in rows)
            {
                var th = item.Descendants().FirstOrDefault(n => n.Name.Equals("th"));
                var a = th.Descendants().FirstOrDefault(n => n.Name.Equals("a"));
                string threadName = a.InnerText.Trim();
                string hrefStr = a.GetAttributeValue("href", "").Substring("viewthread.php?tid=".Length);
                hrefStr = hrefStr.Split('&')[0];
                int threadId = Convert.ToInt32(hrefStr);

                var forumNameNode = item.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("forum"));
                string forumName = forumNameNode.InnerText.Trim();

                var replyCountNode = item.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("nums"));
                int replyCount = Convert.ToInt32(replyCountNode.InnerText);

                var lastPostNode = item.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("lastpost"));
                string lastPostAuthorName = "匿名";
                string lastPostTime = string.Empty;
                string[] lastPostInfo = lastPostNode.InnerText.Trim().Replace("\n", "@").Split('@');
                if (lastPostInfo.Length == 2)
                {
                    lastPostAuthorName = lastPostInfo[0].Trim();
                    lastPostTime = lastPostInfo[1].Trim()
                        .Replace(string.Format("{0}-{1}-{2} ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), string.Empty)
                        .Replace(string.Format("{0}-", DateTime.Now.Year), string.Empty);
                }

                var threadItem = new ThreadItemForMyFavoritesModel(i, forumName, threadId, pageNo, threadName, replyCount, lastPostAuthorName, lastPostTime);
                _threadDataForMyFavorites.Add(threadItem);

                i++;
            }
        }

        async Task<int> GetMoreThreadItemsForMyFavoritesAsync(int pageNo, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadThreadDataForMyFavoritesAsync(pageNo, cts);
            if (_threadDataForMyFavorites.Count == 0 && noDataNotice != null) noDataNotice();
            if (afterLoad != null) afterLoad();

            return _threadDataForMyFavorites.Count;
        }

        ThreadItemForMyFavoritesModel GetOneThreadItemForMyFavorites(int index)
        {
            return _threadDataForMyFavorites.FirstOrDefault(t => t.Index == index);
        }

        public ICollectionView GetViewForThreadPageForMyFavorites(int startPageNo, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<ThreadItemForMyFavoritesModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await GetMoreThreadItemsForMyFavoritesAsync(pageNo, beforeLoad, afterLoad, noDataNotice);
                },
                (index) =>
                {
                    // 从静态类中返回需要显示出来的数据
                    return GetOneThreadItemForMyFavorites(index);
                },
                () =>
                {
                    return GetThreadMaxPageNoForMyFavorites();
                });

            return cvs.View;
        }

        public int GetThreadMaxPageNoForMyFavorites()
        {
            return _maxPageNo;
        }

        public void ClearThreadDataForMyFavorites()
        {
            _threadDataForMyFavorites.Clear();
        }

        public ThreadItemForMyFavoritesModel GetThreadItemForMyFavorites(int threadId)
        {
            return _threadDataForMyFavorites.FirstOrDefault(t => t.ThreadId == threadId);
        }

        public async Task<bool> DeleteThreadForMyFavoritesAsync(List<int> deleteThreadIds, CancellationTokenSource cts)
        {
            if (deleteThreadIds.Count == 0)
            {
                return false;
            }

            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("favsubmit", "true"));
            foreach (var tid in deleteThreadIds)
            {
                postData.Add(new KeyValuePair<string, object>("delete[]", tid));
            }

            string url = string.Format("http://www.hi-pda.com/forum/my.php?item=favorites&type=thread&_={0}", DateTime.Now.Ticks.ToString("x"));
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            return resultContent.Contains("收藏夹已成功更新，现在将转入更新后的收藏夹。");
        }

        public async Task<bool> DeleteThreadForMyFavorites(List<int> deleteThreadIds)
        {
            var cts = new CancellationTokenSource();
            return await DeleteThreadForMyFavoritesAsync(deleteThreadIds, cts);
        }
    }
}
