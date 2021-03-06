﻿using Hipda.Client.Models;
using Hipda.Client.ViewModels;
using Hipda.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Services
{
    public class MyPostsService
    {
        static List<ThreadItemForMyPostsModel> _threadDataForMyPosts = new List<ThreadItemForMyPostsModel>();
        static HttpHandle _httpClient = HttpHandle.GetInstance();
        static int _pageSize = 75;
        int _maxPageNo = 1;

        async Task LoadThreadDataForMyPostsAsync(int pageNo, CancellationTokenSource cts)
        {
            int count = _threadDataForMyPosts.Count(t => t.PageNo == pageNo);
            if (count == _pageSize)
            {
                return;
            }
            else
            {
                _threadDataForMyPosts.RemoveAll(t => t.PageNo == pageNo);
            }

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/my.php?item=posts&page={0}&_={1}", pageNo, DateTime.Now.Ticks.ToString("x"));
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
            if (dataTable == null)
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

            int i = _threadDataForMyPosts.Count;
            for (int j = 0; j < rows.Count - 1; j += 2)
            {
                var tr = rows[j];
                var tr2 = rows[j + 1];

                var th = tr.Descendants().FirstOrDefault(n => n.Name.Equals("th"));
                var a = th.Descendants().FirstOrDefault(n => n.Name.Equals("a"));
                string threadName = a.InnerText.Trim();
                string[] pramaStr = a.GetAttributeValue("href", "").Substring("redirect.php?goto=findpost&amp;pid=".Length).Replace("&amp;ptid=", ",").Split(',');
                int threadId = Convert.ToInt32(pramaStr[1]);
                int postId = Convert.ToInt32(pramaStr[0]);

                var th2 = tr2.Descendants().FirstOrDefault(n => n.Name.Equals("th"));
                string lastPostContent = th2.InnerText.Trim();
                lastPostContent = lastPostContent.Replace("\n", string.Empty);
                lastPostContent = lastPostContent.Replace("\r", string.Empty);

                var forumNameNode = tr.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("forum"));
                string forumName = forumNameNode.InnerText.Trim();

                var lastPostNode = tr.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("lastpost"));
                string lastPostTime = lastPostNode.InnerText.Trim();
                lastPostTime = lastPostTime
                        .Replace(string.Format("{0}-{1}-{2} ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), string.Empty)
                        .Replace(string.Format("{0}-", DateTime.Now.Year), string.Empty);

                var threadItem = new ThreadItemForMyPostsModel(i, forumName, threadId, postId, pageNo, threadName, lastPostContent, lastPostTime);
                _threadDataForMyPosts.Add(threadItem);

                i++;
            }
        }

        async Task<int> GetMoreThreadItemsForMyPostsAsync(int pageNo, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadThreadDataForMyPostsAsync(pageNo, cts);
            if (_threadDataForMyPosts.Count == 0 && noDataNotice != null) noDataNotice();
            if (afterLoad != null) afterLoad();

            return _threadDataForMyPosts.Count;
        }

        ThreadItemForMyPostsModel GetOneThreadItemForMyPosts(int index)
        {
            return _threadDataForMyPosts.FirstOrDefault(t => t.Index == index);
        }

        public ICollectionView GetViewForThreadPageForMyPosts(int startPageNo, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<ThreadItemForMyPostsModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await GetMoreThreadItemsForMyPostsAsync(pageNo, beforeLoad, afterLoad, noDataNotice);
                },
                (index) =>
                {
                    // 从静态类中返回需要显示出来的数据
                    return GetOneThreadItemForMyPosts(index);
                },
                () =>
                {
                    return GetThreadMaxPageNoForMyPosts();
                });

            return cvs.View;
        }

        public int GetThreadMaxPageNoForMyPosts()
        {
            return _maxPageNo;
        }

        public void ClearThreadDataForMyPosts()
        {
            _threadDataForMyPosts.Clear();
        }

        public ThreadItemForMyPostsModel GetThreadItemForMyPosts(int threadId)
        {
            return _threadDataForMyPosts.FirstOrDefault(t => t.ThreadId == threadId);
        }
    }
}
