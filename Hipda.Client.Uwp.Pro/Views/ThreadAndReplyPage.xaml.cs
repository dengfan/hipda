using Hipda.Client.Uwp.Pro.Controls;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Hipda.Client.Uwp.Pro.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ThreadAndReplyPage : Page
    {
        /// <summary>
        /// 用于记录当前页的类型，如常规、我的贴子、我的回复等等类型
        /// 在打开相应版块时赋值
        /// </summary>
        ThreadDataType _threadDataType;

        object _lastSelectedItem;
        ThreadAndReplyPageViewModel _threadAndReplyViewModel;

        public ThreadAndReplyPage()
        {
            this.InitializeComponent();

            this.SizeChanged += (s, e) =>
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
            };
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            LeftListView.SelectedItem = _lastSelectedItem;
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

            _threadAndReplyViewModel.AddToReadHistory(threadId);

            // 如果加载到了第一页，则移除回复列表页的“加载上一页”的按钮，无论其有没有显示
            if (pageNo == 1)
            {
                ReplyListView.HeaderTemplate = null;
            }

            bool isShown = _threadAndReplyViewModel.CheckIsShowButtonForLoadPrevReplyPage(threadId);
            if (isShown && Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop"))
            {
                // 只在桌面环境及最小页码不是1才显示此按钮
                ReplyListView.HeaderTemplate = Resources["ReplyListViewHeaderTemplate"] as DataTemplate;
            }

            // 最宽屏模式下，自动滚到最底部
            if (RightSideColumn.ActualWidth > 0)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    int count = ReadListView.Items.Count;
                    if (count > 0)
                    {
                        ReadListView.ScrollIntoView(ReadListView.Items[count - 1], ScrollIntoViewAlignment.Leading);
                    }
                });
            }
        }

        private async void ReplyListViewScroll(int index)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                switch (_threadDataType)
                {
                    case ThreadDataType.SearchFullText:
                        var itemForSearchFullText = _lastSelectedItem as ThreadItemForSearchFullTextViewModel;
                        int countForSearchFullText = ReplyListView.Items.Count;

                        if (countForSearchFullText > 0 && countForSearchFullText <= index + 1)
                        {
                            ReplyListView.ScrollIntoView(ReplyListView.Items[countForSearchFullText - 1], ScrollIntoViewAlignment.Leading);
                        }

                        if (countForSearchFullText > index + 1 && itemForSearchFullText.GetScrollState() == false)
                        {
                            ReplyListView.ScrollIntoView(ReplyListView.Items[index], ScrollIntoViewAlignment.Leading);
                            itemForSearchFullText.SetScrollState(true);
                        }
                        break;
                    default:
                        var itemForMyPosts = _lastSelectedItem as ThreadItemForMyPostsViewModel;
                        int countForMyPosts = ReplyListView.Items.Count;

                        if (countForMyPosts > 0 && countForMyPosts <= index + 1)
                        {
                            ReplyListView.ScrollIntoView(ReplyListView.Items[countForMyPosts - 1], ScrollIntoViewAlignment.Leading);
                        }

                        if (countForMyPosts > index + 1 && itemForMyPosts.GetScrollState() == false)
                        {
                            ReplyListView.ScrollIntoView(ReplyListView.Items[index], ScrollIntoViewAlignment.Leading);
                            itemForMyPosts.SetScrollState(true);
                        }
                        break;
                }
            });
        }

        private async void ReplyListViewScrollForSpecifiedPost(int index)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var vm = ReplyListView.DataContext as RightSpecifiedPostViewModel;
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

        #region 公开的方法，可用URI SCHEME方法调用
        public void OpenReplyPageByThreadId(int threadId)
        {
            var threadItem = _threadAndReplyViewModel.GetThreadItem(threadId);
            if (threadItem == null)
            {
                _threadAndReplyViewModel.ClearReplyData(threadId);
                var item = new ThreadItemViewModel(1, threadId, 0, ReplyListView, PostReplyTextBox, RightBeforeLoaded, RightAfterLoaded);
                RightWrap.DataContext = item;
            }
            else
            {
                var item = new ThreadItemViewModel(threadItem);
                item.SelectThreadItem(ReplyListView, PostReplyTextBox, RightBeforeLoaded, RightAfterLoaded);
                RightWrap.DataContext = item;
            }
        }

        public void OpenReplyPageByThreadId(int postId, int threadId)
        {
            _threadAndReplyViewModel.ClearReplyData(threadId);
            var cts = new CancellationTokenSource();
            var vm = new RightSpecifiedPostViewModel(cts, postId, threadId, 0, ReplyListView, RightBeforeLoaded, RightAfterLoaded, ReplyListViewScrollForSpecifiedPost);
            RightWrap.DataContext = vm;
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
                    _threadDataType = ThreadDataType.Default;
                    int fid = Convert.ToInt32(param.Substring("fid=".Length));
                    _threadAndReplyViewModel = new ThreadAndReplyPageViewModel(1, fid, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                    DataContext = _threadAndReplyViewModel;
                }
                else if (param.StartsWith("item="))
                {
                    string threadType = param.Substring("item=".Length);
                    if (threadType.Equals("threads"))
                    {
                        _threadDataType = ThreadDataType.MyThreads;
                    }
                    else if (threadType.Equals("posts"))
                    {
                        _threadDataType = ThreadDataType.MyPosts;
                    }
                    else if (threadType.Equals("favorites"))
                    {
                        _threadDataType = ThreadDataType.MyFavorites;
                    }
                    else if (threadType.Equals("notice"))
                    {
                        _threadDataType = ThreadDataType.Notice;
                    }

                    _threadAndReplyViewModel = new ThreadAndReplyPageViewModel(1, threadType, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                    DataContext = _threadAndReplyViewModel;
                }
                else if (param.StartsWith("search="))
                {
                    string[] paramaters = param.Substring("search=".Length).Split(',');
                    string searchKeyowrd = Uri.UnescapeDataString(paramaters[0]);
                    string searchAuthor = Uri.UnescapeDataString(paramaters[1]);
                    int searchType = Convert.ToInt32(paramaters[2]);
                    int searchTimeSpan = Convert.ToInt32(paramaters[3]);
                    int searchForumSpan = Convert.ToInt32(paramaters[4]);

                    _threadDataType = (searchType == 0) ? ThreadDataType.SearchTitle : ThreadDataType.SearchFullText;

                    _threadAndReplyViewModel = new ThreadAndReplyPageViewModel(1, searchKeyowrd, searchAuthor, searchType, searchTimeSpan, searchForumSpan, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                    DataContext = _threadAndReplyViewModel;
                }
                else if (param.Contains(",")) // 表示要加载指定的回复列表页，从窄视图变宽后导航而来
                {
                    string[] p = param.Split(',');
                    int threadId = Convert.ToInt32(p[0]);
                    int threadAuthorUserId = Convert.ToInt32(p[1]);

                    switch (_threadDataType)
                    {
                        case ThreadDataType.MyThreads:
                            var itemForMyThreads = new ThreadItemForMyThreadsViewModel(1, threadId, threadAuthorUserId, ReplyListView, RightBeforeLoaded, RightAfterLoaded);
                            _lastSelectedItem = itemForMyThreads;
                            RightWrap.DataContext = itemForMyThreads;
                            break;
                        case ThreadDataType.MyPosts:
                            var itemForMyPosts = new ThreadItemForMyPostsViewModel(1, threadId, threadAuthorUserId, ReplyListView, RightBeforeLoaded, RightAfterLoaded);
                            _lastSelectedItem = itemForMyPosts;
                            RightWrap.DataContext = itemForMyPosts;
                            break;
                        case ThreadDataType.SearchTitle:
                            break;
                        case ThreadDataType.SearchFullText:
                            break;
                        default:
                            var item = new ThreadItemViewModel(1, threadId, threadAuthorUserId, ReplyListView, PostReplyTextBox, RightBeforeLoaded, RightAfterLoaded);
                            _lastSelectedItem = item;
                            RightWrap.DataContext = item;
                            break;
                    }
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

                switch (_threadDataType)
                {
                    case ThreadDataType.MyThreads:
                        var itemForMyThreads = _lastSelectedItem as ThreadItemForMyThreadsViewModel;
                        p = string.Format("{0},{1}", itemForMyThreads.ThreadItem.ThreadId, AccountService.UserId);
                        break;
                    case ThreadDataType.MyPosts:
                        var itemForMyPosts = _lastSelectedItem as ThreadItemForMyPostsViewModel;
                        p = string.Format("{0},{1}", itemForMyPosts.ThreadItem.ThreadId, 0);
                        break;
                    case ThreadDataType.MyFavorites:
                        var itemForMyFavorites = _lastSelectedItem as ThreadItemForMyFavoritesViewModel;
                        p = string.Format("{0},{1}", itemForMyFavorites.ThreadItem.ThreadId, 0);
                        break;
                    case ThreadDataType.SearchTitle:
                        var itemForSearchTitle = _lastSelectedItem as ThreadItemForSearchTitleViewModel;
                        p = string.Format("{0},{1}", itemForSearchTitle.ThreadItem.ThreadId, 0);
                        break;
                    case ThreadDataType.SearchFullText:
                        var itemForSearchFullText = _lastSelectedItem as ThreadItemForSearchFullTextViewModel;
                        p = string.Format("{0},{1}", itemForSearchFullText.ThreadItem.ThreadId, 0);
                        break;
                    default:
                        var item = _lastSelectedItem as ThreadItemViewModel;
                        p = string.Format("{0},{1}", item.ThreadItem.ThreadId, item.ThreadItem.AuthorUserId);
                        break;
                }

                Frame.Navigate(typeof(ReplyListPage), p, new SuppressNavigationTransitionInfo());
            }

            EntranceNavigationTransitionInfo.SetIsTargetElement(LeftListView, isNarrow);
        }

        private async void LeftListView_ItemClick(object sender, ItemClickEventArgs e) 
        {
            // 如果进入了选择模式，则不打开主题
            if (LeftListView.SelectionMode == ListViewSelectionMode.Multiple)
            {
                return;
            }

            var selectedItem = e.ClickedItem;
            switch (_threadDataType)
            {
                case ThreadDataType.MyThreads:
                    var itemForMyThreads = (ThreadItemForMyThreadsViewModel)selectedItem;
                    itemForMyThreads.SetRead();
                    _lastSelectedItem = itemForMyThreads;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = string.Format("{0},{1}", itemForMyThreads.ThreadItem.ThreadId, AccountService.UserId);
                        Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                    }
                    else
                    {
                        itemForMyThreads.SelectThreadItem(ReplyListView, RightBeforeLoaded, RightAfterLoaded);
                        RightWrap.DataContext = itemForMyThreads;
                    }
                    break;
                case ThreadDataType.MyPosts:
                    var itemForMyPosts = (ThreadItemForMyPostsViewModel)selectedItem;
                    itemForMyPosts.SetRead();

                    _lastSelectedItem = itemForMyPosts;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = string.Format("{0},{1}", itemForMyPosts.ThreadItem.ThreadId, 0);
                        Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                    }
                    else
                    {
                        await itemForMyPosts.SelectThreadItem(ReplyListView, RightBeforeLoaded, RightAfterLoaded, ReplyListViewScroll);
                        RightWrap.DataContext = itemForMyPosts;
                    }
                    break;
                case ThreadDataType.MyFavorites:
                    var itemForMyFavorites = (ThreadItemForMyFavoritesViewModel)selectedItem;
                    itemForMyFavorites.SetRead();

                    _lastSelectedItem = itemForMyFavorites;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = string.Format("{0},{1}", itemForMyFavorites.ThreadItem.ThreadId, 0);
                        Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                    }
                    else
                    {
                        itemForMyFavorites.SelectThreadItem(ReplyListView, PostReplyTextBox, RightBeforeLoaded, RightAfterLoaded);
                        RightWrap.DataContext = itemForMyFavorites;
                    }
                    break;
                case ThreadDataType.SearchTitle:
                    var itemForSearchTitle = (ThreadItemForSearchTitleViewModel)selectedItem;
                    itemForSearchTitle.SetRead();

                    _lastSelectedItem = itemForSearchTitle;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = string.Format("{0},{1}", itemForSearchTitle.ThreadItem.ThreadId, 0);
                        Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                    }
                    else
                    {
                        itemForSearchTitle.SelectThreadItem(ReplyListView, PostReplyTextBox, RightBeforeLoaded, RightAfterLoaded);
                        RightWrap.DataContext = itemForSearchTitle;
                    }
                    break;
                case ThreadDataType.SearchFullText:
                    var itemForSearchFullText = (ThreadItemForSearchFullTextViewModel)selectedItem;
                    itemForSearchFullText.SetRead();

                    _lastSelectedItem = itemForSearchFullText;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = string.Format("{0},{1}", itemForSearchFullText.ThreadItem.ThreadId, 0);
                        Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                    }
                    else
                    {
                        await itemForSearchFullText.SelectThreadItem(ReplyListView, PostReplyTextBox, RightBeforeLoaded, RightAfterLoaded, ReplyListViewScroll);
                        RightWrap.DataContext = itemForSearchFullText;
                    }
                    break;
                default:
                    var item = (ThreadItemViewModel)selectedItem;
                    item.SetRead();
                    _lastSelectedItem = item;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        
                        string p = string.Format("{0},{1}", item.ThreadItem.ThreadId, item.ThreadItem.AuthorUserId);
                        Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                    }
                    else
                    {
                        item.SelectThreadItem(ReplyListView, PostReplyTextBox, RightBeforeLoaded, RightAfterLoaded);
                        RightWrap.DataContext = item;
                    }
                    break;
            }
        }

        #region 加载上一页
        private void leftPr_RefreshInvoked(DependencyObject sender, object args)
        {
            if (_lastSelectedItem != null)
            {
                switch (_threadDataType)
                {
                    case ThreadDataType.MyThreads:
                        _threadAndReplyViewModel.RefreshThreadDataForMyThreadsFromPrevPage();
                        break;
                    case ThreadDataType.MyPosts:
                        _threadAndReplyViewModel.RefreshThreadDataForMyPostsFromPrevPage();
                        break;
                    case ThreadDataType.MyFavorites:
                        _threadAndReplyViewModel.RefreshThreadDataForMyFavoritesFromPrevPage();
                        break;
                    default:
                        _threadAndReplyViewModel.RefreshThreadDataFromPrevPage();
                        break;
                }
            }
        }

        private void rightPr_RefreshInvoked(DependencyObject sender, object args)
        {
            if (_lastSelectedItem != null)
            {
                // 根据回复列表页所属的主题类别来加载其下的回复列表的上一页
                var threadItemViewModelBase = _lastSelectedItem as ThreadItemViewModelBase;
                switch (threadItemViewModelBase.ThreadDataType)
                {
                    case ThreadDataType.MyThreads:
                        ((ThreadItemForMyThreadsViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
                        break;
                    case ThreadDataType.MyPosts:
                        ((ThreadItemForMyPostsViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
                        break;
                    case ThreadDataType.MyFavorites:
                        ((ThreadItemForMyFavoritesViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
                        break;
                    default:
                        ((ThreadItemViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
                        break;
                }
            }
        }

        private void LoadPrevReplyPageButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_lastSelectedItem != null)
            {
                // 根据回复列表页所属的主题类别来加载其下的回复列表的上一页
                var threadItemViewModelBase = _lastSelectedItem as ThreadItemViewModelBase;
                switch (threadItemViewModelBase.ThreadDataType)
                {
                    case ThreadDataType.MyThreads:
                        ((ThreadItemForMyThreadsViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
                        break;
                    case ThreadDataType.MyPosts:
                        ((ThreadItemForMyPostsViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
                        break;
                    case ThreadDataType.MyFavorites:
                        ((ThreadItemForMyFavoritesViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
                        break;
                    default:
                        ((ThreadItemViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
                        break;
                }
            }
        }
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
