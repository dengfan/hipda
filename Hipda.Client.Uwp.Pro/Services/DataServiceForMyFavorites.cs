﻿using Hipda.Client.Uwp.Pro.Models;
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
        static List<ThreadItemForMyFavoritesModel> _threadDataForMyFavorites = new List<ThreadItemForMyFavoritesModel>();
        int _threadMaxPageNoForMyFavorites = 1;

        async Task LoadThreadDataForMyFavoritesAsync(int pageNo, CancellationTokenSource cts)
        {
            int count = _threadDataForMyFavorites.Count(t => t.PageNo == pageNo);
            if (count == _threadPageSize)
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

            var dataTable = doc.DocumentNode.Descendants().FirstOrDefault(n => n.GetAttributeValue("class", "").Equals("datatable"));
            if (dataTable == null || dataTable.InnerText.Trim().Equals("暂无数据"))
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
                _threadMaxPageNoForMyFavorites = Convert.ToInt32(lastPageNodeValue);
            }

            if (pageNo > _threadMaxPageNoForMyFavorites)
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

        async Task<int> GetMoreThreadItemsForMyFavoritesAsync(int pageNo, Action beforeLoad, Action afterLoad)
        {
            if (beforeLoad != null) beforeLoad();
            var cts = new CancellationTokenSource();
            await LoadThreadDataForMyFavoritesAsync(pageNo, cts);
            if (afterLoad != null) afterLoad();

            return _threadDataForMyFavorites.Count;
        }

        ThreadItemForMyFavoritesViewModel GetOneThreadItemForMyFavorites(int index)
        {
            var threadItem = _threadDataForMyFavorites.FirstOrDefault(t => t.Index == index);
            if (threadItem == null)
            {
                return null;
            }

            var vm = new ThreadItemForMyFavoritesViewModel(threadItem);
            vm.StatusColorStyle = GetReadStatusStyle(threadItem.ThreadId);
            return vm;
        }

        public ICollectionView GetViewForThreadPageForMyFavorites(int startPageNo, Action beforeLoad, Action afterLoad)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<ThreadItemForMyFavoritesViewModel>(
                startPageNo,
                async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await GetMoreThreadItemsForMyFavoritesAsync(pageNo, beforeLoad, afterLoad);
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
            return _threadMaxPageNoForMyFavorites;
        }

        public void ClearThreadDataForMyFavorites()
        {
            _threadDataForMyFavorites.Clear();
        }

        public ThreadItemForMyFavoritesModel GetThreadItemForMyFavorites(int threadId)
        {
            return _threadDataForMyFavorites.FirstOrDefault(t => t.ThreadId == threadId);
        }
    }
}