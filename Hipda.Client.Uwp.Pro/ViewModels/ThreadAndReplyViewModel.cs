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
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    /// <summary>
    /// 主题及回复页之视图模型
    /// 共有三种
    /// 1. 按版块ID来初始化
    /// 2. 按主题类别来初始化，如我的贴子、我的回复
    /// 2. 按搜索条件来初始化
    /// </summary>
    public class ThreadAndReplyViewModel : NotificationObject
    {
        #region 仅用于按版块ID来初始化的情况
        /// <summary>
        /// 版块ID
        /// </summary>
        int _forumId;
        #endregion

        #region 仅用于按搜索条件来来初始化的情况
        string _searchKeyword;
        string _searchAuthor;
        int _searchType;
        int _searchTimeSpan;
        int _searchForumSpan;
        #endregion

        /// <summary>
        /// 起始页，只在LoadData方法中赋值
        /// 因为只有LoadData方法中的页码参数才算作起始页
        /// </summary>
        int _startPageNo;

        ListView _threadListView;
        CommandBar _threadCommandBar;
        Action _beforeLoad;
        Action _afterLoad;
        Action _noDataNotice;
        DataService _ds;

        public int ThreadMaxPageNo { get; set; }

        public DelegateCommand ClearHistoryCommand { get; set; }

        public ObservableCollection<ThreadItemModelBase> ReadData
        {
            get
            {
                return DataService.ReadHistoryData;
            }
        }

        #region 从指定页开始加载数据
        async void LoadData(int pageNo, int forumId)
        {
            var cv = await _ds.GetViewForThreadPage(pageNo, forumId, _beforeLoad, _afterLoad, _noDataNotice);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNo();
                _startPageNo = pageNo;
                _threadListView.ItemsSource = cv;
            }
        }

        async void LoadDataForMyThreads(int pageNo)
        {
            var cv = await _ds.GetViewForThreadPageForMyThreads(pageNo, _beforeLoad, _afterLoad, _noDataNotice);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNoForMyThreads();
                _startPageNo = pageNo;
                _threadListView.ItemsSource = cv;
            }
        }

        async void LoadDataForMyPosts(int pageNo)
        {
            var cv = await _ds.GetViewForThreadPageForMyPosts(pageNo, _beforeLoad, _afterLoad, _noDataNotice);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNoForMyPosts();
                _startPageNo = pageNo;
                _threadListView.ItemsSource = cv;
            }
        }

        async void LoadDataForMyFavorites(int pageNo)
        {
            var cv = await _ds.GetViewForThreadPageForMyFavorites(pageNo, _beforeLoad, _afterLoad, _noDataNotice);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNoForMyFavorites();
                _startPageNo = pageNo;
                _threadListView.ItemsSource = cv;
            }
        }

        async void LoadDataForSearchTitle(int pageNo)
        {
            var cv = await _ds.GetViewForThreadPageForSearchTitle(pageNo, _searchKeyword, _searchAuthor, _searchTimeSpan, _searchForumSpan, _beforeLoad, _afterLoad, _noDataNotice);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNoForSearchTitle();
                _startPageNo = pageNo;
                _threadListView.ItemsSource = cv;
            }
        }

        async void LoadDataForSearchFullText(int pageNo)
        {
            var cv = await _ds.GetViewForThreadPageForSearchFullText(pageNo, _searchKeyword, _searchAuthor, _searchTimeSpan, _searchForumSpan, _beforeLoad, _afterLoad, _noDataNotice);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNoForSearchFullText();
                _startPageNo = pageNo;
                _threadListView.ItemsSource = cv;
            }
        }
        #endregion

        #region 从开始页的上一页开始加载，用于下滑刷新
        public void RefreshThreadDataFromPrevPage()
        {
            if (_startPageNo > 1)
            {
                _ds.ClearThreadData(_forumId);
                LoadData(_startPageNo, _forumId);
            }
        }

        public void RefreshThreadDataForMyThreadsFromPrevPage()
        {
            if (_startPageNo > 1)
            {
                _ds.ClearThreadDataForMyThreads();
                LoadDataForMyThreads(_startPageNo);
            }
        }

        public void RefreshThreadDataForMyPostsFromPrevPage()
        {
            if (_startPageNo > 1)
            {
                _ds.ClearThreadDataForMyPosts();
                LoadDataForMyPosts(_startPageNo);
            }
        }

        public void RefreshThreadDataForMyFavoritesFromPrevPage()
        {
            if (_startPageNo > 1)
            {
                _ds.ClearThreadDataForMyFavorites();
                LoadDataForMyFavorites(_startPageNo);
            }
        }
        #endregion

        /// <summary>
        /// 按版块ID来初始化视图模型
        /// </summary>
        /// <param name="pageNo"></param>
        /// <param name="forumId"></param>
        /// <param name="threadListView"></param>
        /// <param name="threadCommandBar"></param>
        /// <param name="beforeLoad"></param>
        /// <param name="afterLoad"></param>
        public ThreadAndReplyViewModel(int pageNo, int forumId, ListView threadListView, CommandBar threadCommandBar, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            _forumId = forumId;
            _threadListView = threadListView;
            _threadListView.SelectionMode = ListViewSelectionMode.Single;
            _threadListView.ItemsSource = null;

            _threadCommandBar = threadCommandBar;
            _threadCommandBar.PrimaryCommands.Clear();
            _threadCommandBar.SecondaryCommands.Clear();

            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _noDataNotice = noDataNotice;
            _ds = new DataService();

            ClearHistoryCommand = new DelegateCommand();
            ClearHistoryCommand.ExecuteAction = (p) => {
                DataService.ReadHistoryData.Clear();
            };

            LoadData(pageNo, _forumId);

            var refreshThreadCommand = new DelegateCommand();
            refreshThreadCommand.ExecuteAction = (p) => {
                _ds.ClearThreadData(_forumId);
                LoadData(1, _forumId);
            };

            var btnAdd = new AppBarButton { Icon = new SymbolIcon(Symbol.Add), Label = "发表新贴" };
            var btnRefresh = new AppBarButton { Icon = new SymbolIcon(Symbol.Refresh), Label = "刷新" };
            btnRefresh.Command = refreshThreadCommand;
            var btnSort = new AppBarButton { Icon = new SymbolIcon(Symbol.Sort), Label = "按发布时间倒序排列" };
            _threadCommandBar.PrimaryCommands.Add(btnAdd);
            _threadCommandBar.PrimaryCommands.Add(btnRefresh);
            _threadCommandBar.PrimaryCommands.Add(btnSort);
        }

        /// <summary>
        /// 按主题类型（如我的主题、我的回复等）来初始化视图模型
        /// </summary>
        /// <param name="pageNo"></param>
        /// <param name="threadType"></param>
        /// <param name="threadListView"></param>
        /// <param name="threadCommandBar"></param>
        /// <param name="beforeLoad"></param>
        /// <param name="afterLoad"></param>
        public ThreadAndReplyViewModel(int pageNo, string threadType, ListView threadListView, CommandBar threadCommandBar, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            _threadListView = threadListView;
            _threadListView.SelectionMode = ListViewSelectionMode.Single;
            _threadListView.ItemsSource = null;

            _threadCommandBar = threadCommandBar;
            _threadCommandBar.PrimaryCommands.Clear();
            _threadCommandBar.SecondaryCommands.Clear();

            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _noDataNotice = noDataNotice;
            _ds = new DataService();

            ClearHistoryCommand = new DelegateCommand();
            ClearHistoryCommand.ExecuteAction = (p) => {
                DataService.ReadHistoryData.Clear();
            };

            switch (threadType)
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
                    btnDeleteSelected.Tapped += async (s, e) =>
                    {
                        var deleteThreads = _threadListView.SelectedItems;
                        if (deleteThreads != null)
                        {
                            var ids = new List<int>();
                            foreach (ThreadItemForMyFavoritesViewModel thread in deleteThreads)
                            {
                                ids.Add(thread.ThreadItem.ThreadId);
                            }
                            bool isOk = await _ds.DeleteThreadForMyFavoritesAsync(ids);
                            if (isOk)
                            {
                                _ds.ClearThreadDataForMyFavorites();
                                LoadDataForMyFavorites(1);

                                var options = new LauncherOptions();
                                options.TreatAsUntrusted = false;

                                string tipContent = Uri.EscapeUriString("操作成功！");
                                await Launcher.LaunchUriAsync(new Uri(string.Format("hipda:tip={0}", tipContent)), options);
                            }
                        }
                    };

                    var btnMultipleSelect = new AppBarToggleButton { Icon = new FontIcon { Glyph = "\uE762", FontFamily = new FontFamily("Segoe MDL2 Assets") }, Label = "选择", IsThreeState = false };
                    btnMultipleSelect.Tapped += (s, e) =>
                    {
                        var btn = s as AppBarToggleButton;
                        if (btn.IsChecked == true)
                        {
                            _threadListView.SelectionMode = ListViewSelectionMode.Multiple;
                            btnDeleteSelected.IsEnabled = true;
                        }
                        else
                        {
                            _threadListView.SelectionMode = ListViewSelectionMode.Single;
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
            }
        }

        /// <summary>
        /// 按搜索条件来初始化视图模型
        /// </summary>
        /// <param name="pageNo"></param>
        /// <param name="searchKeyword"></param>
        /// <param name="searchType"></param>
        /// <param name="searchTimeSpan"></param>
        /// <param name="searchForumSpan"></param>
        /// <param name="threadListView"></param>
        /// <param name="threadCommandBar"></param>
        /// <param name="beforeLoad"></param>
        /// <param name="afterLoad"></param>
        public ThreadAndReplyViewModel(int pageNo, string searchKeyword, string searchAuthor, int searchType, int searchTimeSpan, int searchForumSpan, ListView threadListView, CommandBar threadCommandBar, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            _searchKeyword = searchKeyword;
            _searchAuthor = searchAuthor;
            _searchType = searchType;
            _searchTimeSpan = searchTimeSpan;
            _searchForumSpan = searchForumSpan;

            _threadListView = threadListView;
            _threadListView.SelectionMode = ListViewSelectionMode.Single;
            _threadListView.ItemsSource = null;

            _threadCommandBar = threadCommandBar;
            _threadCommandBar.PrimaryCommands.Clear();
            _threadCommandBar.SecondaryCommands.Clear();

            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _noDataNotice = noDataNotice;
            _ds = new DataService();

            ClearHistoryCommand = new DelegateCommand();
            ClearHistoryCommand.ExecuteAction = (p) => {
                DataService.ReadHistoryData.Clear();
            };

            if (_searchType == 0) // 按标题搜索
            {
                // 先清除已搜索的数据
                _ds.ClearThreadDataForSearchTitle();
                LoadDataForSearchTitle(pageNo);

                var refreshThreadForSearchCommand = new DelegateCommand();
                refreshThreadForSearchCommand.ExecuteAction = (p) =>
                {
                    _ds.ClearThreadDataForSearchTitle();
                    LoadDataForSearchTitle(1);
                };
            }
            else // 全文搜索
            {
                // 先清除已搜索的数据
                _ds.ClearThreadDataForSearchFullText();
                LoadDataForSearchFullText(pageNo);

                var refreshThreadForSearchCommand = new DelegateCommand();
                refreshThreadForSearchCommand.ExecuteAction = (p) => {
                    _ds.ClearThreadDataForSearchFullText();
                    LoadDataForSearchFullText(1);
                };
            }
        }

        public ThreadItemModel GetThreadItem(int threadId)
        {
            return _ds.GetThreadItem(threadId);
        }

        string GetThreadTitle(int threadId)
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

        public async Task<ReplyItemModel> GetPostDetail(int postId, int threadId)
        {
            return await _ds.GetPostDetail(postId, threadId);
        }
    }
}
