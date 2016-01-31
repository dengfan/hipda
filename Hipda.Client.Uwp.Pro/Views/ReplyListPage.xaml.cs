using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Hipda.Client.Uwp.Pro.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ReplyListPage : Page
    {
        int _threadId;
        int _postId;

        #region 委托事件
        void BeforeLoaded()
        {
            rightProgress.IsActive = true;
            rightProgress.Visibility = Visibility.Visible;
            ReplyRefreshButton.IsEnabled = false;
        }

        void AfterLoaded(int threadId, int pageNo)
        {
            rightProgress.IsActive = false;
            rightProgress.Visibility = Visibility.Collapsed;
            ReplyRefreshButton.IsEnabled = true;
        }

        async void ReplyListViewScrollForSpecifiedPost(int index)
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
            RightWrap.DataContext = new ReplyListViewForDefaultViewModel(cts, threadId, ReplyListView, BeforeLoaded, AfterLoaded);

            #region 避免在窄视图下拖宽窗口时返回到主页时还是显示旧缓存
            var backStack = Frame.BackStack;
            var backStackCount = backStack.Count;

            if (backStackCount > 0)
            {
                var masterPageEntry = backStack[backStackCount - 1];
                backStack.RemoveAt(backStackCount - 1);

                // Doctor the navigation parameter for the master page so it
                // will show the correct item in the side-by-side view.
                var modifiedEntry = new PageStackEntry(
                    masterPageEntry.SourcePageType,
                    $"tid={threadId}",
                    masterPageEntry.NavigationTransitionInfo
                    );
                backStack.Add(modifiedEntry);
            }
            #endregion
        }

        public void OpenReplyPageByPostId(int postId)
        {
            var cts = new CancellationTokenSource();
            RightWrap.DataContext = new ReplyListViewForSpecifiedPostViewModel(cts, postId, ReplyListView, BeforeLoaded, AfterLoaded, ReplyListViewScrollForSpecifiedPost);
        }
        #endregion

        public ReplyListPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Frame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;
            }

            string param = e.Parameter.ToString();
            if (param.StartsWith("tid="))
            {
                _threadId = Convert.ToInt32(param.Substring("tid=".Length));

                #region 避免在窄视图下拖宽窗口时返回到主页时还是显示旧缓存
                var backStack = Frame.BackStack;
                var backStackCount = backStack.Count;

                if (backStackCount > 0)
                {
                    var masterPageEntry = backStack[backStackCount - 1];
                    backStack.RemoveAt(backStackCount - 1);

                    // Doctor the navigation parameter for the master page so it
                    // will show the correct item in the side-by-side view.
                    var modifiedEntry = new PageStackEntry(
                        masterPageEntry.SourcePageType,
                        $"tid={_threadId}",
                        masterPageEntry.NavigationTransitionInfo
                        );
                    backStack.Add(modifiedEntry);
                }
                #endregion
            }
            else if (param.StartsWith("pid="))
            {
                _postId = Convert.ToInt32(param.Substring("pid=".Length));

                #region 避免在窄视图下拖宽窗口时返回到主页时还是显示旧缓存
                var backStack = Frame.BackStack;
                var backStackCount = backStack.Count;

                if (backStackCount > 0)
                {
                    var masterPageEntry = backStack[backStackCount - 1];
                    backStack.RemoveAt(backStackCount - 1);

                    // Doctor the navigation parameter for the master page so it
                    // will show the correct item in the side-by-side view.
                    var modifiedEntry = new PageStackEntry(
                        masterPageEntry.SourcePageType,
                        $"pid={_postId}",
                        masterPageEntry.NavigationTransitionInfo
                        );
                    backStack.Add(modifiedEntry);
                }
                #endregion
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (Frame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                SystemNavigationManager.GetForCurrentView().BackRequested -= BackRequested;
            }
        }

        void BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame.GoBack(new CommonNavigationTransitionInfo());
            e.Handled = true;
        }

        void NavigateBackForWideState(bool useTransition)
        {
            // Evict this page from the cache as we may not need it again.
            NavigationCacheMode = NavigationCacheMode.Disabled;

            if (useTransition)
            {
                Frame.GoBack(new SlideNavigationTransitionInfo());
            }
            else
            {
                Frame.GoBack(new SuppressNavigationTransitionInfo());
            }
        }

        bool ShouldGoToWideState()
        {
            return Window.Current.Bounds.Width >= 720;
        }

        void PageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (ShouldGoToWideState())
            {
                // We shouldn't see this page since we are in "wide master-detail" mode.
                // Play a transition as we are navigating from a separate page.
                NavigateBackForWideState(useTransition: true);
            }
            else
            {
                FindName("RightWrap");

                var cts = new CancellationTokenSource();
                if (_threadId > 0)
                {
                    DataContext = new ReplyListViewForDefaultViewModel(cts, _threadId, ReplyListView, BeforeLoaded, AfterLoaded);
                }
                else if (_postId > 0)
                {
                    DataContext = new ReplyListViewForSpecifiedPostViewModel(cts, _postId, ReplyListView, BeforeLoaded, AfterLoaded, ReplyListViewScrollForSpecifiedPost);
                }
            }

            Window.Current.SizeChanged += Window_SizeChanged;
        }

        void PageRoot_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Window_SizeChanged;
        }

        void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (ShouldGoToWideState())
            {
                // Make sure we are no longer listening to window change events.
                Window.Current.SizeChanged -= Window_SizeChanged;

                // We shouldn't see this page since we are in "wide master-detail" mode.
                NavigateBackForWideState(useTransition: false);
            }
        }

        void rightPr_RefreshInvoked(DependencyObject sender, object args)
        {
            if (_threadId > 0)
            {
                var vm = (ReplyListViewForDefaultViewModel)DataContext;
                vm.LoadPrevPageData();
            }
            else if (_postId > 0)
            {
                var vm = (ReplyListViewForSpecifiedPostViewModel)DataContext;
                vm.LoadPrevPageData();
            }
        }
    }
}
