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
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ThreadAndReplyPage : Page
    {
        ThreadItemModelBase _lastSelectedItem;
        ThreadHistoryListViewViewModel _vmForThreadHistory = new ThreadHistoryListViewViewModel();

        public ThreadAndReplyPage()
        {
            this.InitializeComponent();
            this.SizeChanged += ThreadAndReplyPage_SizeChanged;
            RightSideWrap.DataContext = _vmForThreadHistory;
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

            //_threadAndReplyViewModel.AddToReadHistory(threadId);

            // 如果加载到了第一页，则移除回复列表页的“加载上一页”的按钮，无论其有没有显示
            if (pageNo == 1)
            {
                ReplyListView.HeaderTemplate = null;
            }

            //bool isShown = _threadAndReplyViewModel.CheckIsShowButtonForLoadPrevReplyPage(threadId);
            //if (isShown && Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop"))
            //{
            //    // 只在桌面环境及最小页码不是1才显示此按钮
            //    ReplyListView.HeaderTemplate = Resources["ReplyListViewHeaderTemplate"] as DataTemplate;
            //}

            // 最宽屏模式下，自动滚到最底部
            if (RightSideColumn.ActualWidth > 0)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    int count = ThreadHistoryListView.Items.Count;
                    if (count > 0)
                    {
                        ThreadHistoryListView.ScrollIntoView(ThreadHistoryListView.Items[count - 1], ScrollIntoViewAlignment.Leading);
                    }
                });
            }
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
            RightWrap.DataContext = new ReplyListViewViewModel(cts, threadId, 0, ReplyListView, RightBeforeLoaded, RightAfterLoaded);
        }

        public void OpenReplyPageByThreadId(int postId, int threadId)
        {
            var cts = new CancellationTokenSource();
            RightWrap.DataContext = new ReplyListViewForSpecifiedPostViewModel(cts, postId, threadId, 0, ReplyListView, RightBeforeLoaded, RightAfterLoaded, ReplyListViewScrollForSpecifiedPost);
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
                    LeftWrap.DataContext = new ThreadListViewViewModel(1, fid, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
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
                else if (param.Contains(",")) // 表示要加载指定的回复列表页，从窄视图变宽后导航而来
                {
                    string[] p = param.Split(',');
                    int threadId = Convert.ToInt32(p[0]);
                    int threadAuthorUserId = Convert.ToInt32(p[1]);

                    OpenReplyPageByThreadId(threadId);
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
                string p = string.Empty;
                switch (_lastSelectedItem.ThreadType)
                {
                    case ThreadDataType.MyThreads:
                        var itemForMyThreads = _lastSelectedItem as ThreadItemForMyThreadsModel;
                        p = string.Format("{0},{1}", itemForMyThreads.ThreadId, AccountService.UserId);
                        break;
                    case ThreadDataType.MyPosts:
                        var itemForMyPosts = _lastSelectedItem as ThreadItemForMyPostsModel;
                        p = string.Format("{0},{1}", itemForMyPosts.ThreadId, 0);
                        break;
                    case ThreadDataType.MyFavorites:
                        var itemForMyFavorites = _lastSelectedItem as ThreadItemForMyFavoritesModel;
                        p = string.Format("{0},{1}", itemForMyFavorites.ThreadId, 0);
                        break;
                    case ThreadDataType.SearchTitle:
                        var itemForSearchTitle = _lastSelectedItem as ThreadItemForSearchTitleModel;
                        p = string.Format("{0},{1}", itemForSearchTitle.ThreadId, 0);
                        break;
                    case ThreadDataType.SearchFullText:
                        var itemForSearchFullText = _lastSelectedItem as ThreadItemForSearchFullTextModel;
                        p = string.Format("{0},{1}", itemForSearchFullText.ThreadId, 0);
                        break;
                    default:
                        var item = _lastSelectedItem as ThreadItemModel;
                        p = string.Format("{0},{1}", item.ThreadId, item.AuthorUserId);
                        break;
                }

                Frame.Navigate(typeof(ReplyListPage), p, new SuppressNavigationTransitionInfo());
            }

            EntranceNavigationTransitionInfo.SetIsTargetElement(LeftListView, isNarrow);
        }

        private void LeftListView_ItemClick(object sender, ItemClickEventArgs e) 
        {
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
                        string p = string.Format("{0},{1}", itemForMyThreads.ThreadId, AccountService.UserId);
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
                        string p = string.Format("{0},{1}", itemForMyPosts.ThreadId, 0);
                        Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                    }
                    else
                    {
                        OpenReplyPageByThreadId(itemForMyPosts.PostId, itemForMyPosts.ThreadId);
                    }
                    break;
                case ThreadDataType.MyFavorites:
                    var itemForMyFavorites = (ThreadItemForMyFavoritesModel)selectedItem;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = string.Format("{0},{1}", itemForMyFavorites.ThreadId, 0);
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
                        string p = string.Format("{0},{1}", itemForSearchTitle.ThreadId, 0);
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
                        string p = string.Format("{0},{1}", itemForSearchFullText.ThreadId, 0);
                        Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                    }
                    else
                    {
                        OpenReplyPageByThreadId(itemForSearchFullText.ThreadId);
                    }
                    break;
                default:
                    var item = (ThreadItemModel)selectedItem;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = string.Format("{0},{1}", item.ThreadId, item.AuthorUserId);
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
                        var vm = LeftWrap.DataContext as ThreadListViewViewModel;
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

            if (RightWrap.DataContext.GetType().Equals(typeof(ReplyListViewViewModel)))
            {
                var vm = RightWrap.DataContext as ReplyListViewViewModel;
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

        //private async void openThreadInNewView_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    var uri = new Uri("hipda:tid=" + MainPage.PopupThreadId);
        //    await Launcher.LaunchUriAsync(uri, new LauncherOptions { TreatAsUntrusted = false });
        //}

        private void openUserInfoDialogButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var mainPage = Common.FindParent<Page>(Frame) as MainPage;
            mainPage.OpenUserInfoDialog();
        }

        private void openUserMessageDialogButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var mainPage = Common.FindParent<Page>(Frame) as MainPage;
            mainPage.OpenUserMessageDialog();
        }

        private void PostDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var data = sender.DataContext as ReplyItemModel;
            PostReplyTextBox.Text = data.TextStr;
        }

        private void ReadListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as ThreadItemModelBase;
            OpenReplyPageByThreadId(data.ThreadId);

            LeftListView.SelectedItem = null;
        }

        //private void ReplyListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        //{
        //    ReplyItemModel item = args.Item as ReplyItemModel;
        //    Debug.WriteLine(item.FloorNo);
        //}
    }
}
