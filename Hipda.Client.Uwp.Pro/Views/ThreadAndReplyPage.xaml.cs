using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Hipda.Client.Uwp.Pro.Views
{
    public sealed partial class ThreadAndReplyPage : Page
    {
        ThreadItemModelBase _lastSelectedItem;

        public ThreadAndReplyPage()
        {
            this.InitializeComponent();
            this.SizeChanged += ThreadAndReplyPage_SizeChanged;
        }

        private void ThreadAndReplyPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            string userInteractionType = Windows.UI.ViewManagement.UIViewSettings.GetForCurrentView().UserInteractionMode.ToString();
            if (userInteractionType.Equals("Touch"))
            {
                FaceButton.Width = 80;
                FaceButton.Height = 40;
                FileButton.Width = 80;
                FileButton.Height = 40;
                SendButton.Width = 80;
                SendButton.Height = 40;
            }
            else if (userInteractionType.Equals("Mouse"))
            {
                FaceButton.Width = 36;
                FaceButton.Height = 32;
                FileButton.Width = 36;
                FileButton.Height = 32;
                SendButton.Width = 80;
                SendButton.Height = 32;
            }

            PostReplyTextBox.MaxHeight = this.ActualHeight / 2;
        }

        #region 委托事件
        private void LeftBeforeLoaded()
        {
            leftProgress.IsActive = true;
            leftProgress.Visibility = Visibility.Visible;
            ReplyRefreshButton.IsEnabled = false;
            ReplyRefreshButton2.IsEnabled = false;
        }

        private void LeftAfterLoaded()
        {
            leftProgress.IsActive = false;
            leftProgress.Visibility = Visibility.Collapsed;
            ReplyRefreshButton.IsEnabled = true;
            ReplyRefreshButton2.IsEnabled = true;
        }

        private void LeftNoDataNotice()
        {
            LeftCommandBar.Visibility = Visibility.Collapsed;
            leftNoDataNoticePanel.Visibility = Visibility.Visible;
        }

        private void RightBeforeLoaded()
        {
            rightProgress.IsActive = true;
            rightProgress.Visibility = Visibility.Visible;
            ReplyRefreshButton.IsEnabled = false;
            ReplyRefreshButton2.IsEnabled = false;
            rightFooter.Visibility = Visibility.Collapsed;
        }

        private async void RightAfterLoaded(int threadId, int pageNo)
        {
            rightProgress.IsActive = false;
            rightProgress.Visibility = Visibility.Collapsed;
            ReplyRefreshButton.IsEnabled = true;
            ReplyRefreshButton2.IsEnabled = true;
            rightFooter.Visibility = Visibility.Visible;

            // 如果加载到了第一页，则移除回复列表页的“加载上一页”的按钮，无论其有没有显示
            if (pageNo == 1)
            {
                ReplyListView.HeaderTemplate = null;
            }

            bool isShown = DataServiceForReply.CanShowButtonForLoadPrevReplyPage(threadId);
            if (isShown && Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop"))
            {
                // 只在桌面环境及最小页码不是1才显示此按钮
                ReplyListView.HeaderTemplate = Resources["ReplyListViewHeaderTemplate"] as DataTemplate;
            }

            // 最宽屏模式下，自动滚到最底部
            //if (RightSideColumn.ActualWidth > 0)
            //{
            //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
            //        int count = ThreadHistoryListView.Items.Count;
            //        if (count > 0)
            //        {
            //            ThreadHistoryListView.ScrollIntoView(ThreadHistoryListView.Items[count - 1], ScrollIntoViewAlignment.Leading);
            //        }
            //    });
            //}
        }

        private async void ReplyListViewScrollForSpecifiedPost(int index)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var vm = ReplyListView.DataContext as ReplyListViewForSpecifiedPostViewModel;
                int count = ReplyListView.Items.Count;

                if (count > 0 && count <= index + 1)
                {
                    ReplyListView.ScrollIntoView(ReplyListView.Items[count - 1], ScrollIntoViewAlignment.Leading);
                }

                if (count > index + 1 && vm.GetScrollState() == false)
                {
                    ReplyListView.ScrollIntoView(ReplyListView.Items[index], ScrollIntoViewAlignment.Leading);
                    vm.SetScrollState(true);
                }
            });
        }
        #endregion

        #region 公开的方法，可用协议调用
        public void OpenReplyPageByThreadId(int threadId)
        {
            var cts = new CancellationTokenSource();
            RightWrap.DataContext = new ReplyListViewForDefaultViewModel(cts, threadId, ReplyListView, RightBeforeLoaded, RightAfterLoaded);
        }

        public void OpenReplyPageByPostId(int postId)
        {
            var cts = new CancellationTokenSource();
            RightWrap.DataContext = new ReplyListViewForSpecifiedPostViewModel(cts, postId, ReplyListView, RightBeforeLoaded, RightAfterLoaded, ReplyListViewScrollForSpecifiedPost);
        }
        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            leftNoDataNoticePanel.Visibility = Visibility.Collapsed;

            if (e.Parameter != null)
            {
                string param = e.Parameter.ToString();
                if (param.StartsWith("fid=")) // 表示要加载指定的贴子列表页
                {
                    int fid = Convert.ToInt32(param.Substring("fid=".Length));
                    LeftWrap.DataContext = new ThreadListViewForDefaultViewModel(1, fid, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                }
                else if (param.StartsWith("item="))
                {
                    string threadType = param.Substring("item=".Length);
                    switch (threadType)
                    {
                        case "threads":
                            LeftWrap.DataContext = new ThreadListViewForMyThreadsViewModel(1, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                            break;
                        case "posts":
                            LeftWrap.DataContext = new ThreadListViewForMyPostsViewModel(1, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                            break;
                        case "favorites":
                            LeftWrap.DataContext = new ThreadListViewForMyFavoritesViewModel(1, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                            break;
                        case "notice":
                            LeftWrap.DataContext = new ThreadListViewForNoticeViewModel(LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                            break;
                    }
                }
                else if (param.StartsWith("search="))
                {
                    string[] paramaters = param.Substring("search=".Length).Split(',');
                    string searchKeyowrd = Uri.UnescapeDataString(paramaters[0]);
                    string searchAuthor = Uri.UnescapeDataString(paramaters[1]);
                    int searchType = Convert.ToInt32(paramaters[2]);
                    int searchTimeSpan = Convert.ToInt32(paramaters[3]);
                    int searchForumSpan = Convert.ToInt32(paramaters[4]);

                    if (searchType == 0)
                    {
                        LeftWrap.DataContext = new ThreadListViewForSearchTitleViewModel(1, searchKeyowrd, searchAuthor, searchType, searchTimeSpan, searchForumSpan, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                    }
                    else
                    {
                        LeftWrap.DataContext = new ThreadListViewForSearchFullTextViewModel(1, searchKeyowrd, searchAuthor, searchType, searchTimeSpan, searchForumSpan, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                    }
                }
                else if (param.Contains("tid=")) // 表示要加载指定的回复列表页，从窄视图变宽后导航而来
                {
                    int threadId = Convert.ToInt32(param.Substring("tid=".Length));
                    OpenReplyPageByThreadId(threadId);
                }
                else if (param.Contains("pid=")) // 表示要加载指定的回复列表页，从窄视图变宽后导航而来
                {
                    int postId = Convert.ToInt32(param.Substring("pid=".Length));
                    OpenReplyPageByPostId(postId);
                }
            }

            UpdateForVisualState(AdaptiveStates.CurrentState);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            UpdateForVisualState(e.NewState, e.OldState);
        }

        private void UpdateForVisualState(VisualState newState, VisualState oldState = null)
        {
            var isNarrow = newState == NarrowState;
            if (isNarrow && oldState == DefaultState && _lastSelectedItem != null) // 如果是窄视图，则跳转到 reply list page 页面
            {
                int id = 0;
                switch (_lastSelectedItem.ThreadType)
                {
                    case ThreadDataType.MyThreads:
                        var itemForMyThreads = _lastSelectedItem as ThreadItemForMyThreadsModel;
                        id = itemForMyThreads.ThreadId;
                        Frame.Navigate(typeof(ReplyListPage), $"tid={id}", new SuppressNavigationTransitionInfo());
                        break;
                    case ThreadDataType.MyPosts:
                        var itemForMyPosts = _lastSelectedItem as ThreadItemForMyPostsModel;
                        id = itemForMyPosts.PostId;
                        Frame.Navigate(typeof(ReplyListPage), $"pid={id}", new SuppressNavigationTransitionInfo());
                        break;
                    case ThreadDataType.MyFavorites:
                        var itemForMyFavorites = _lastSelectedItem as ThreadItemForMyFavoritesModel;
                        id = itemForMyFavorites.ThreadId;
                        Frame.Navigate(typeof(ReplyListPage), $"tid={id}", new SuppressNavigationTransitionInfo());
                        break;
                    case ThreadDataType.SearchTitle:
                        var itemForSearchTitle = _lastSelectedItem as ThreadItemForSearchTitleModel;
                        id = itemForSearchTitle.ThreadId;
                        Frame.Navigate(typeof(ReplyListPage), $"tid={id}", new SuppressNavigationTransitionInfo());
                        break;
                    case ThreadDataType.SearchFullText:
                        var itemForSearchFullText = _lastSelectedItem as ThreadItemForSearchFullTextModel;
                        id = itemForSearchFullText.PostId;
                        Frame.Navigate(typeof(ReplyListPage), $"pid={id}", new SuppressNavigationTransitionInfo());
                        break;
                    default:
                        var item = _lastSelectedItem as ThreadItemModel;
                        id = item.ThreadId;
                        Frame.Navigate(typeof(ReplyListPage), $"tid={id}", new SuppressNavigationTransitionInfo());
                        break;
                }
            }
        }

        private void LeftListView_ItemClick(object sender, ItemClickEventArgs e) 
        {
            //ThreadHistoryListView.SelectedItem = null;

            // 如果进入了选择模式，则不打开主题
            if (LeftListView.SelectionMode == ListViewSelectionMode.Multiple)
            {
                return;
            }

            var selectedItem = e.ClickedItem;
            _lastSelectedItem = (ThreadItemModelBase)selectedItem;
            switch (_lastSelectedItem.ThreadType)
            {
                case ThreadDataType.MyThreads:
                    var itemForMyThreads = (ThreadItemForMyThreadsModel)selectedItem;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = $"tid={itemForMyThreads.ThreadId}";
                        Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                    }
                    else
                    {
                        OpenReplyPageByThreadId(itemForMyThreads.ThreadId);
                    }
                    break;
                case ThreadDataType.MyPosts:
                    var itemForMyPosts = (ThreadItemForMyPostsModel)selectedItem;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = $"pid={itemForMyPosts.PostId}";
                        Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                    }
                    else
                    {
                        OpenReplyPageByPostId(itemForMyPosts.PostId);
                    }
                    break;
                case ThreadDataType.MyFavorites:
                    var itemForMyFavorites = (ThreadItemForMyFavoritesModel)selectedItem;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = $"tid={itemForMyFavorites.ThreadId}";
                        Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                    }
                    else
                    {
                        OpenReplyPageByThreadId(itemForMyFavorites.ThreadId);
                    }
                    break;
                case ThreadDataType.SearchTitle:
                    var itemForSearchTitle = (ThreadItemForSearchTitleModel)selectedItem;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = $"tid={itemForSearchTitle.ThreadId}";
                        Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                    }
                    else
                    {
                        OpenReplyPageByThreadId(itemForSearchTitle.ThreadId);
                    }
                    break;
                case ThreadDataType.SearchFullText:
                    var itemForSearchFullText = (ThreadItemForSearchFullTextModel)selectedItem;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = $"pid={itemForSearchFullText.PostId}";
                        Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                    }
                    else
                    {
                        OpenReplyPageByPostId(itemForSearchFullText.PostId);
                    }
                    break;
                default:
                    var item = (ThreadItemModel)selectedItem;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = $"tid={item.ThreadId}";
                        Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                    }
                    else
                    {
                        OpenReplyPageByThreadId(item.ThreadId);
                    }
                    break;
            }
        }

        #region 加载上一页
        private void leftPr_RefreshInvoked(DependencyObject sender, object args)
        {
            if (_lastSelectedItem != null)
            {
                switch (_lastSelectedItem.ThreadType)
                {
                    case ThreadDataType.MyThreads:
                        var vmForMyThreads = LeftWrap.DataContext as ThreadListViewForMyThreadsViewModel;
                        vmForMyThreads.LoadPrevPageData();
                        break;
                    case ThreadDataType.MyPosts:
                        var vmForMyPosts = LeftWrap.DataContext as ThreadListViewForMyPostsViewModel;
                        vmForMyPosts.LoadPrevPageData();
                        break;
                    case ThreadDataType.MyFavorites:
                        var vmForMyFavorites = LeftWrap.DataContext as ThreadListViewForMyFavoritesViewModel;
                        vmForMyFavorites.LoadPrevPageData();
                        break;
                    default:
                        var vm = LeftWrap.DataContext as ThreadListViewForDefaultViewModel;
                        vm.LoadPrevPageData();
                        break;
                }
            }
        }

        private void rightPr_RefreshInvoked(DependencyObject sender, object args)
        {
            if (RightWrap.DataContext == null)
            {
                return;
            }

            if (RightWrap.DataContext.GetType().Equals(typeof(ReplyListViewForDefaultViewModel)))
            {
                var vm = RightWrap.DataContext as ReplyListViewForDefaultViewModel;
                vm.LoadPrevPageData();
            }
            else if (RightWrap.DataContext.GetType().Equals(typeof(ReplyListViewForSpecifiedPostViewModel)))
            {
                var vm = RightWrap.DataContext as ReplyListViewForSpecifiedPostViewModel;
                vm.LoadPrevPageData();
            }
        }

        //private void LoadPrevReplyPageButton_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    if (_lastSelectedItem != null)
        //    {
        //        // 根据回复列表页所属的主题类别来加载其下的回复列表的上一页
        //        var threadItemViewModelBase = _lastSelectedItem as ThreadItemViewModelBase;
        //        switch (threadItemViewModelBase.ThreadDataType)
        //        {
        //            case ThreadDataType.MyThreads:
        //                ((ThreadItemForMyThreadsViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
        //                break;
        //            case ThreadDataType.MyPosts:
        //                ((ThreadItemForMyPostsViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
        //                break;
        //            case ThreadDataType.MyFavorites:
        //                ((ThreadItemForMyFavoritesViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
        //                break;
        //            default:
        //                ((ThreadItemViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
        //                break;
        //        }
        //    }
        //}
        #endregion

        

        private void PostDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var data = sender.DataContext as ReplyItemModel;
            PostReplyTextBox.Text = data.TextStr;
        }

        

        //private void ReplyListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        //{
        //    ReplyItemModel item = args.Item as ReplyItemModel;
        //    Debug.WriteLine(item.FloorNo);
        //}
    }
}
