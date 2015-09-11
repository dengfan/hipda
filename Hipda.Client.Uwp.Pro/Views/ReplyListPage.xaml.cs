using Hipda.Client.Uwp.Pro.ViewModels;
using System;
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
        private ReplyViewModel _replyViewModel;
        private int _threadId;
        private int _threadAuthorUserId;

        public ReplyListPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;

            string[] p = e.Parameter.ToString().Split(',');
            _threadId = Convert.ToInt32(p[0]);
            _threadAuthorUserId = Convert.ToInt32(p[1]);

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
                    string.Format("{0},{1}", _threadId, _threadAuthorUserId),
                    masterPageEntry.NavigationTransitionInfo
                    );
                backStack.Add(modifiedEntry);
            }
            #endregion
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= BackRequested;
        }

        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame.GoBack(new SuppressNavigationTransitionInfo());
        }

        void NavigateBackForWideState(bool useTransition)
        {
            // Evict this page from the cache as we may not need it again.
            NavigationCacheMode = NavigationCacheMode.Disabled;

            if (useTransition)
            {
                Frame.GoBack(new EntranceNavigationTransitionInfo());
            }
            else
            {
                Frame.GoBack(new SuppressNavigationTransitionInfo());
            }
        }

        private bool ShouldGoToWideState()
        {
            return Window.Current.Bounds.Width >= 720;
        }

        private void PageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (ShouldGoToWideState())
            {
                // We shouldn't see this page since we are in "wide master-detail" mode.
                // Play a transition as we are navigating from a separate page.
                NavigateBackForWideState(useTransition: true);
            }
            else
            {
                // Realize the main page content.
                FindName("RightWrap");

                _replyViewModel = new ReplyViewModel(
                    _threadId,
                    _threadAuthorUserId,
                    ReplyListView,
                    () => {
                        rightProgress.IsActive = true;
                        rightProgress.Visibility = Visibility.Visible;
                    },
                    () => {
                        rightProgress.IsActive = false;
                        rightProgress.Visibility = Visibility.Collapsed;
                    });
                DataContext = _replyViewModel;
                ReplyListView.ItemsSource = _replyViewModel.ReplyItemCollection;
                btnRefresh.Command = _replyViewModel.RefreshReplyCommand;
            }

            Window.Current.SizeChanged += Window_SizeChanged;
        }

        private void PageRoot_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Window_SizeChanged;
        }

        private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (ShouldGoToWideState())
            {
                // Make sure we are no longer listening to window change events.
                Window.Current.SizeChanged -= Window_SizeChanged;

                // We shouldn't see this page since we are in "wide master-detail" mode.
                NavigateBackForWideState(useTransition: false);
            }
        }
    }
}
