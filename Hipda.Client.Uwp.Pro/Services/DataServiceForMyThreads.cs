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
    public class DataServiceForMyThreads
    {
        static List<ThreadItemForMyThreadsModel> _threadDataForMyThreads = new List<ThreadItemForMyThreadsModel>();
        static HttpHandle _httpClient = HttpHandle.GetInstance();
        static int _pageSize = 75;
        int _threadMaxPageNoForMyThreads = 1;

        async Task LoadThreadDataForMyThreadsAsync(int pageNo, CancellationTokenSource cts)
        {
            int count = _threadDataForMyThreads.Count(t => t.PageNo == pageNo);
            if (count == _pageSize)
            {
                return;
            }
            else
            {
                _threadDataForMyThreads.RemoveAll(t => t.PageNo == pageNo);
            }

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/my.php?item=threads&page={0}&_={1}", pageNo, DateTime.Now.Ticks.ToString("x"));
            string htmlContent = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            // 最先读取提醒数据
            var promptContentNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("div") && n.GetAttributeValue("class", "").Equals("promptcontent"));
            DataService.GetPromptData(promptContentNode);

            // 读取主内容
            var dataTable = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("datatable"));
            if (dataTable == null)
            {
                return;
            }

            // 读取最大页码
            var pagesNode = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("pages"));
            _threadMaxPageNoForMyThreads = DataService.GetMaxPageNo(pagesNode);

            if (pageNo > _threadMaxPageNoForMyThreads)
            {
                return;
            }

            var rows = dataTable.ChildNodes[3].Descendants().Where(n => n.Name.Equals("tr"));
            if (rows == null)
            {
                return;
            }

            int i = _threadDataForMyThreads.Count;
            foreach (var item in rows)
            {
                var th = item.Descendants().FirstOrDefault(n => n.Name.Equals("th"));
                var a = th.Descendants().FirstOrDefault(n => n.Name.Equals("a"));
                string threadName = a.InnerText.Trim();
                int threadId = Convert.ToInt32(a.GetAttributeValue("href", "").Substring("viewthread.php?tid=".Length));

                var forumNameNode = item.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("forum"));
                string forumName = forumNameNode.InnerText.Trim();

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

                var threadItem = new ThreadItemForMyThreadsModel(i, forumName, threadId, pageNo, threadName, lastPostAuthorName, lastPostTime);
                _threadDataForMyThreads.Add(threadItem);

                i++;
            }
        }

        async Task<int> GetMoreThreadItemsForMyThreadsAsync(int pageNo, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadThreadDataForMyThreadsAsync(pageNo, cts);
            if (_threadDataForMyThreads.Count == 0 && noDataNotice != null) noDataNotice();
            if (afterLoad != null) afterLoad();

            return _threadDataForMyThreads.Count;
        }

        ThreadItemForMyThreadsModel GetOneThreadItemForMyThreads(int index)
        {
            return _threadDataForMyThreads.FirstOrDefault(t => t.Index == index);
        }

        public ICollectionView GetViewForThreadPageForMyThreads(int startPageNo, Action beforeLoad, Action afterLoad, Action noDataNotice, Action loadAllFinish)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<ThreadItemForMyThreadsModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await GetMoreThreadItemsForMyThreadsAsync(pageNo, beforeLoad, afterLoad, noDataNotice);
                },
                (index) =>
                {
                    // 从静态类中返回需要显示出来的数据
                    return GetOneThreadItemForMyThreads(index);
                },
                () =>
                {
                    return GetThreadMaxPageNoForMyThreads();
                },
                loadAllFinish);

            return cvs.View;
        }

        public int GetThreadMaxPageNoForMyThreads()
        {
            return _threadMaxPageNoForMyThreads;
        }

        public void ClearThreadDataForMyThreads()
        {
            _threadDataForMyThreads.Clear();
        }

        public ThreadItemForMyThreadsModel GetThreadItemForMyThreads(int threadId)
        {
            return _threadDataForMyThreads.FirstOrDefault(t => t.ThreadId == threadId);
        }
    }
}
