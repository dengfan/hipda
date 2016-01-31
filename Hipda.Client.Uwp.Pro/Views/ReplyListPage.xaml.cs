using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Hipda.Client.Uwp.Pro.Views
{
    /// <summary>
    /// 回复列表页
    /// 用于窄视图
    /// </summary>
    public sealed partial class ReplyListPage : Page
    {
        #region 两个关键依赖属性，每次打开此页，必须且只能使用此俩参数之一
        /// <summary>
        /// 有的主题是使用 ThreadId 参数加载回复列表页
        /// </summary>
        public int ThreadId
        {
            get { return (int)GetValue(ThreadIdProperty); }
            set { SetValue(ThreadIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThreadId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThreadIdProperty =
            DependencyProperty.Register("ThreadId", typeof(int), typeof(ReplyListPage), new PropertyMetadata(0, new PropertyChangedCallback(OnThreadIdChanged)));

        private static void OnThreadIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue > 0)
            {
                var instance = d as ReplyListPage;
                instance.PostId = 0;
            }
        }


        /// <summary>
        /// 有的主题是使用 PostId 参数加载回复列表页
        /// </summary>
        public int PostId
        {
            get { return (int)GetValue(PostIdProperty); }
            set { SetValue(PostIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PostId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PostIdProperty =
            DependencyProperty.Register("PostId", typeof(int), typeof(ReplyListPage), new PropertyMetadata(0, new PropertyChangedCallback(OnPostIdChanged)));

        private static void OnPostIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue > 0)
            {
                var instance = d as ReplyListPage;
                instance.ThreadId = 0;
            }
        }
        #endregion


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

        #region 打开回复列表页必须使用以下两种方法之一
        public void OpenReplyPageByThreadId()
        {
            var cts = new CancellationTokenSource();
            RightWrap.DataContext = new ReplyListViewForDefaultViewModel(cts, ThreadId, ReplyListView, BeforeLoaded, AfterLoaded);

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
                    $"tid={ThreadId}",
                    masterPageEntry.NavigationTransitionInfo
                    );
                backStack.Add(modifiedEntry);
            }
            #endregion
        }

        public void OpenReplyPageByPostId()
        {
            var cts = new CancellationTokenSource();
            RightWrap.DataContext = new ReplyListViewForSpecifiedPostViewModel(cts, PostId, ReplyListView, BeforeLoaded, AfterLoaded, ReplyListViewScrollForSpecifiedPost);

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
                    $"pid={PostId}",
                    masterPageEntry.NavigationTransitionInfo
                    );
                backStack.Add(modifiedEntry);
            }
            #endregion
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
                ThreadId = Convert.ToInt32(param.Substring("tid=".Length));

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
                        $"tid={ThreadId}",
                        masterPageEntry.NavigationTransitionInfo
                        );
                    backStack.Add(modifiedEntry);
                }
                #endregion
            }
            else if (param.StartsWith("pid="))
            {
                PostId = Convert.ToInt32(param.Substring("pid=".Length));

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
                        $"pid={PostId}",
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
                if (PostId > 0)
                {
                    DataContext = new ReplyListViewForSpecifiedPostViewModel(cts, PostId, ReplyListView, BeforeLoaded, AfterLoaded, ReplyListViewScrollForSpecifiedPost);
                }
                else if (ThreadId > 0)
                {
                    DataContext = new ReplyListViewForDefaultViewModel(cts, ThreadId, ReplyListView, BeforeLoaded, AfterLoaded);
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
            if (PostId > 0)
            {
                var vm = (ReplyListViewForSpecifiedPostViewModel)DataContext;
                vm.LoadPrevPageData();
            }
            else if (ThreadId > 0)
            {
                var vm = (ReplyListViewForDefaultViewModel)DataContext;
                vm.LoadPrevPageData();
            }
        }
    }
}
