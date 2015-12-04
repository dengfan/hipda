using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadAndReplyViewModel : NotificationObject
    {
        private int _forumId { get; set; }
        private ListView _threadListView { get; set; }
        private CommandBar _threadCommandBar { get; set; }
        private Action _beforeLoad { get; set; }
        private Action _afterLoad { get; set; }
        private DataService _ds { get; set; }

        public DelegateCommand ClearHistoryCommand { get; set; }

        public int ThreadMaxPageNo { get; private set; }

        public ObservableCollection<ThreadItemModelBase> ReadData
        {
            get
            {
                return DataService.ReadHistoryData;
            }
        }

        private void LoadData(int pageNo)
        {
            var cv = _ds.GetViewForThreadPage(pageNo, _forumId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNo();
                _threadListView.ItemsSource = cv;
            }
        }

        private void LoadDataForMyThreads(int pageNo)
        {
            var cv = _ds.GetViewForThreadPageForMyThreads(pageNo, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNo();
                _threadListView.ItemsSource = cv;
            }
        }

        private void LoadDataForMyPosts(int pageNo)
        {
            var cv = _ds.GetViewForThreadPageForMyPosts(pageNo, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNo();
                _threadListView.ItemsSource = cv;
            }
        }

        private void LoadDataForMyFavorites(int pageNo)
        {
            var cv = _ds.GetViewForThreadPageForMyFavorites(pageNo, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNo();
                _threadListView.ItemsSource = cv;
            }
        }

        public ThreadAndReplyViewModel(int pageNo, string threadTypeOrForumId, ListView threadListView, CommandBar threadCommandBar, Action beforeLoad, Action afterLoad)
        {
            _threadListView = threadListView;
            _threadListView.SelectionMode = ListViewSelectionMode.Single;
            _threadListView.ItemsSource = null;

            _threadCommandBar = threadCommandBar;
            _threadCommandBar.PrimaryCommands.Clear();
            _threadCommandBar.SecondaryCommands.Clear();

            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            ClearHistoryCommand = new DelegateCommand();
            ClearHistoryCommand.ExecuteAction = (p) => {
                DataService.ReadHistoryData.Clear();
            };

            switch (threadTypeOrForumId)
            {
                case "threads":
                    LoadDataForMyThreads(pageNo);

                    var refreshThreadForThreadsCommand = new DelegateCommand();
                    refreshThreadForThreadsCommand.ExecuteAction = (p) => {
                        _ds.ClearThreadDataForMyThreads();
                        LoadDataForMyThreads(1);
                    };
                    break;
                case "posts":
                    LoadDataForMyPosts(pageNo);

                    var refreshThreadForPostsCommand = new DelegateCommand();
                    refreshThreadForPostsCommand.ExecuteAction = (p) => {
                        _ds.ClearThreadDataForMyPosts();
                        LoadDataForMyPosts(1);
                    };
                    break;
                case "favorites":
                    LoadDataForMyFavorites(pageNo);

                    var btnDeleteSelected = new AppBarButton { Icon = new SymbolIcon(Symbol.Delete), Label = "删除", IsEnabled = false };
                    btnDeleteSelected.Tapped += (s, e) =>
                    {

                    };

                    var btnMultipleSelect = new AppBarToggleButton { Icon = new FontIcon { Glyph = "\uE179", FontFamily = new FontFamily("Segoe MDL2 Assets") }, Label = "进入选择模式", IsThreeState = false };
                    btnMultipleSelect.Tapped += (s, e) =>
                    {
                        var btn = s as AppBarToggleButton;
                        if (btn.IsChecked == true)
                        {
                            _threadListView.SelectionMode = ListViewSelectionMode.Multiple;
                            btnMultipleSelect.Label = "退出选择模式";
                            btnDeleteSelected.IsEnabled = true;
                        }
                        else
                        {
                            _threadListView.SelectionMode = ListViewSelectionMode.Single;
                            btnMultipleSelect.Label = "进入选择模式";
                            btnDeleteSelected.IsEnabled = false;
                        }
                    };

                    var btnRefreshForFavorites = new AppBarButton { Icon = new SymbolIcon(Symbol.Refresh), Label = "刷新" };
                    btnRefreshForFavorites.Tapped += (s, e) => {
                        _threadListView.SelectionMode = ListViewSelectionMode.Single;
                        btnMultipleSelect.IsChecked = false;
                        btnDeleteSelected.IsEnabled = false;

                        _ds.ClearThreadDataForMyFavorites();
                        LoadDataForMyFavorites(1);
                    };

                    _threadCommandBar.PrimaryCommands.Add(btnRefreshForFavorites);
                    _threadCommandBar.PrimaryCommands.Add(btnMultipleSelect);
                    _threadCommandBar.PrimaryCommands.Add(btnDeleteSelected);
                    break;
                default:
                    _forumId = Convert.ToInt32(threadTypeOrForumId);

                    LoadData(pageNo);

                    var refreshThreadCommand = new DelegateCommand();
                    refreshThreadCommand.ExecuteAction = (p) => {
                        _ds.ClearThreadData(_forumId);
                        LoadData(1);
                    };

                    var btnAdd = new AppBarButton { Icon = new SymbolIcon(Symbol.Add), Label = "发表新贴" };
                    var btnRefresh = new AppBarButton { Icon = new SymbolIcon(Symbol.Refresh), Label = "刷新" };
                    btnRefresh.Command = refreshThreadCommand;
                    var btnSort = new AppBarButton { Icon = new SymbolIcon(Symbol.Sort), Label = "按发布时间倒序排列" };
                    _threadCommandBar.PrimaryCommands.Add(btnAdd);
                    _threadCommandBar.PrimaryCommands.Add(btnRefresh);
                    _threadCommandBar.PrimaryCommands.Add(btnSort);
                    break;
            }
        }

        #region 从上一页开始加载
        public void RefreshThreadDataFromPrevPage()
        {
            // 先获取当前数据中已存在的最小页码
            int minPageNo = _ds.GetThreadMinPageNoInLoadedData();
            int startPageNo = minPageNo > 1 ? minPageNo - 1 : 1;

            _ds.ClearThreadData(_forumId);
            LoadData(startPageNo);
        }

        public void RefreshThreadDataForMyThreadsFromPrevPage()
        {
            // 先获取当前数据中已存在的最小页码
            int minPageNo = _ds.GetThreadMinPageNoForMyThreadsInLoadedData();
            int startPageNo = minPageNo > 1 ? minPageNo - 1 : 1;

            _ds.ClearThreadDataForMyThreads();
            LoadDataForMyThreads(startPageNo);
        }

        public void RefreshThreadDataForMyPostsFromPrevPage()
        {
            // 先获取当前数据中已存在的最小页码
            int minPageNo = _ds.GetThreadMinPageNoForMyPostsInLoadedData();
            int startPageNo = minPageNo > 1 ? minPageNo - 1 : 1;

            _ds.ClearThreadDataForMyPosts();
            LoadDataForMyPosts(startPageNo);
        }

        public void RefreshThreadDataForMyFavoritesFromPrevPage()
        {
            // 先获取当前数据中已存在的最小页码
            int minPageNo = _ds.GetThreadMinPageNoForMyFavoritesInLoadedData();
            int startPageNo = minPageNo > 1 ? minPageNo - 1 : 1;

            _ds.ClearThreadDataForMyFavorites();
            LoadDataForMyFavorites(startPageNo);
        }
        #endregion

        public ThreadItemModel GetThreadItem(int threadId)
        {
            return _ds.GetThreadItem(threadId);
        }

        private string GetThreadTitle(int threadId)
        {
            string title = _ds.GetThreadTitleFromReplyData(threadId);
            if (string.IsNullOrEmpty(title))
            {
                title = _ds.GetThreadTitleFromThreadData(threadId);
            }

            return title;
        }

        public bool CheckIsShowButtonForLoadPrevReplyPage(int threadId)
        {
            return _ds.CheckIsShowButtonForLoadPrevReplyPage(threadId);
        }

        public void AddToReadHistory(int threadId)
        {
            var ti = DataService.ReadHistoryData.FirstOrDefault(t => t.ThreadId == threadId);
            if (ti != null)
            {
                DataService.ReadHistoryData.Remove(ti);
            }

            string threadTitle = GetThreadTitle(threadId);
            if (string.IsNullOrEmpty(threadTitle))
            {
                return;
            }

            DataService.ReadHistoryData.Add(new ThreadItemModelBase
            {
                Title = threadTitle,
                ThreadId = threadId
            });

            ApplicationView.GetForCurrentView().Title = threadTitle;
        }

        public void ClearReplyData(int threadId)
        {
            _ds.ClearReplyData(threadId);
        }

        public async Task<string> GetXamlForUserInfo(int userId)
        {
            return await _ds.GetXamlForUserInfo(userId);
        }

        public async Task<List<UserMessageItemModel>> GetUserMessageData(int userId, int limitCount)
        {
            return await _ds.GetUserMessageData(userId, limitCount);
        }

        public async Task<bool> PostUserMessage(string message, int userId)
        {
            return await _ds.PostUserMessage(message, userId);
        }
    }
}
