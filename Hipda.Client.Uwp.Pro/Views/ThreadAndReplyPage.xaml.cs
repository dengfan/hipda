using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Hipda.Client.Uwp.Pro.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ThreadAndReplyPage : Page
    {
        private object _lastSelectedItem;
        private ThreadAndReplyViewModel _threadAndReplyViewModel;

        public ThreadAndReplyPage()
        {
            this.InitializeComponent();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadListView.SelectedItem = _lastSelectedItem;
        }

        private void LeftBeforeLoad()
        {
            leftProgress.IsActive = true;
            leftProgress.Visibility = Visibility.Visible;
            ThreadRefreshButton.IsEnabled = false;
            ReplyRefreshButton.IsEnabled = false;
        }

        private void LeftAfterLoad()
        {
            leftProgress.IsActive = false;
            leftProgress.Visibility = Visibility.Collapsed;
            ThreadRefreshButton.IsEnabled = true;
            ReplyRefreshButton.IsEnabled = true;
        }

        private void RightBeforeLoad()
        {
            rightProgress.IsActive = true;
            rightProgress.Visibility = Visibility.Visible;
            ThreadRefreshButton.IsEnabled = false;
            ReplyRefreshButton.IsEnabled = false;
        }

        private void RightAfterLoad()
        {
            rightProgress.IsActive = false;
            rightProgress.Visibility = Visibility.Collapsed;
            ThreadRefreshButton.IsEnabled = true;
            ReplyRefreshButton.IsEnabled = true;
        }

        private async void ReplyListViewScroll(int index)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                var item = _lastSelectedItem as ThreadItemForMyPostsViewModel;
                int count = ReplyListView.Items.Count;

                if (count > 0 && count <= index + 1)
                {
                    ReplyListView.ScrollIntoView(ReplyListView.Items[count - 1], ScrollIntoViewAlignment.Leading);
                }

                if (count > index + 1 && item.GetScrollState() == false)
                {
                    ReplyListView.ScrollIntoView(ReplyListView.Items[index], ScrollIntoViewAlignment.Leading);
                    item.SetScrollState(true);
                }
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                string param = e.Parameter.ToString();

                if (param.StartsWith("fid=")) // 表示要加载指定的贴子列表页
                {
                    int fid = Convert.ToInt32(param.Substring(4));
                    _threadAndReplyViewModel = new ThreadAndReplyViewModel(1, fid, ThreadListView, LeftBeforeLoad, LeftAfterLoad);
                    DataContext = _threadAndReplyViewModel;
                }
                else if (param.StartsWith("item="))
                {
                    string itemType = param.Substring(5);
                    if (itemType.Equals("threads"))
                    {
                        _threadAndReplyViewModel = new ThreadAndReplyViewModel(1, itemType, ThreadListView, LeftBeforeLoad, LeftAfterLoad);
                        DataContext = _threadAndReplyViewModel;
                    }
                    else if (itemType.Equals("posts"))
                    {
                        _threadAndReplyViewModel = new ThreadAndReplyViewModel(1, itemType, ThreadListView, LeftBeforeLoad, LeftAfterLoad);
                        DataContext = _threadAndReplyViewModel;
                    }
                }
                else if (param.Contains(",")) // 表示要加载指定的回复列表页
                {
                    string[] p = param.Split(',');
                    int threadId = Convert.ToInt32(p[0]);
                    int threadAuthorUserId = Convert.ToInt32(p[1]);

                    ThreadItemViewModelBase itemBase = _lastSelectedItem as ThreadItemViewModelBase;
                    switch (itemBase.ThreadDataType)
                    {
                        case ThreadDataType.MyThreads:
                            var itemForMyThreads = new ThreadItemForMyThreadsViewModel(1, threadId, threadAuthorUserId, ReplyListView, RightBeforeLoad, RightAfterLoad);
                            _lastSelectedItem = itemForMyThreads;
                            RightWrap.DataContext = itemForMyThreads;
                            break;
                        case ThreadDataType.MyPosts:
                            var itemForMyPosts = new ThreadItemForMyPostsViewModel(1, threadId, threadAuthorUserId, ReplyListView, RightBeforeLoad, RightAfterLoad);
                            _lastSelectedItem = itemForMyPosts;
                            RightWrap.DataContext = itemForMyPosts;
                            break;
                        default:
                            var item = new ThreadItemViewModel(1, threadId, threadAuthorUserId, ReplyListView, RightBeforeLoad, RightAfterLoad);
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
            ThreadItemViewModelBase itemBase = _lastSelectedItem as ThreadItemViewModelBase;

            var isNarrow = newState == NarrowState;
            if (isNarrow && oldState == DefaultState && _lastSelectedItem != null) // 如果是窄视图，则跳转到 reply list page 页面
            {
                string p = string.Empty;

                switch (itemBase.ThreadDataType)
                {
                    case ThreadDataType.MyThreads:
                        var itemForMyThreads = _lastSelectedItem as ThreadItemForMyThreadsViewModel;
                        p = string.Format("{0},{1}", itemForMyThreads.ThreadItem.ThreadId, AccountService.UserId);
                        break;
                    case ThreadDataType.MyPosts:
                        var itemForMyPosts = _lastSelectedItem as ThreadItemForMyPostsViewModel;
                        p = string.Format("{0},{1}", itemForMyPosts.ThreadItem.ThreadId, AccountService.UserId);
                        break;
                    default:
                        var item = _lastSelectedItem as ThreadItemViewModel;
                        p = string.Format("{0},{1}", item.ThreadItem.ThreadId, item.ThreadItem.AuthorUserId);
                        break;
                }

                Frame.Navigate(typeof(ReplyListPage), p, new SuppressNavigationTransitionInfo());
            }

            EntranceNavigationTransitionInfo.SetIsTargetElement(ThreadListView, isNarrow);
        }

        private void ThreadListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var b = e.ClickedItem as ThreadItemViewModelBase;
            switch (b.ThreadDataType)
            {
                case ThreadDataType.MyThreads:
                    var itemForMyThreads = (ThreadItemForMyThreadsViewModel)e.ClickedItem;
                    _lastSelectedItem = itemForMyThreads;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = string.Format("{0},{1}", itemForMyThreads.ThreadItem.ThreadId, AccountService.UserId);
                        Frame.Navigate(typeof(ReplyListPage), p, new DrillInNavigationTransitionInfo());
                    }
                    else
                    {
                        itemForMyThreads.SelectThreadItem(ReplyListView, RightBeforeLoad, RightAfterLoad);
                        RightWrap.DataContext = itemForMyThreads;
                    }
                    break;
                case ThreadDataType.MyPosts:
                    var itemForMyPosts = (ThreadItemForMyPostsViewModel)e.ClickedItem;
                    _lastSelectedItem = itemForMyPosts;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = string.Format("{0},{1}", itemForMyPosts.ThreadItem.ThreadId, AccountService.UserId);
                        Frame.Navigate(typeof(ReplyListPage), p, new DrillInNavigationTransitionInfo());
                    }
                    else
                    {
                        itemForMyPosts.SelectThreadItem(ReplyListView, RightBeforeLoad, RightAfterLoad, ReplyListViewScroll);
                        RightWrap.DataContext = itemForMyPosts;
                    }
                    break;
                default:
                    var item = (ThreadItemViewModel)e.ClickedItem;
                    _lastSelectedItem = item;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        item.SetRead();
                        string p = string.Format("{0},{1}", item.ThreadItem.ThreadId, item.ThreadItem.AuthorUserId);
                        Frame.Navigate(typeof(ReplyListPage), p, new DrillInNavigationTransitionInfo());
                    }
                    else
                    {
                        item.SetRead();
                        item.SelectThreadItem(ReplyListView, RightBeforeLoad, RightAfterLoad);
                        RightWrap.DataContext = item;
                    }
                    break;
            }
        }

        private void ThreadItem_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            // If you need the clicked element:
            // Item whichOne = senderElement.DataContext as Item;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void leftPr_RefreshInvoked(DependencyObject sender, object args)
        {
            ThreadItemViewModelBase itemBase = _lastSelectedItem as ThreadItemViewModelBase;
            switch (itemBase.ThreadDataType)
            {
                case ThreadDataType.MyThreads:
                    _threadAndReplyViewModel.RefreshThreadDataForMyThreadsFromPrevPage();
                    break;
                case ThreadDataType.MyPosts:
                    _threadAndReplyViewModel.RefreshThreadDataForMyPostsFromPrevPage();
                    break;
                default:
                    _threadAndReplyViewModel.RefreshThreadDataFromPrevPage();
                    break;
            }
        }

        private void rightPr_RefreshInvoked(DependencyObject sender, object args)
        {
            ThreadItemViewModelBase itemBase = _lastSelectedItem as ThreadItemViewModelBase;
            switch (itemBase.ThreadDataType)
            {
                case ThreadDataType.MyThreads:
                    ((ThreadItemForMyThreadsViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
                    break;
                case ThreadDataType.MyPosts:
                    ((ThreadItemForMyPostsViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
                    break;
                default:
                    ((ThreadItemViewModel)_lastSelectedItem).RefreshReplyDataFromPrevPage();
                    break;
            }
        }

        private void SortingButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.RequestedTheme == ApplicationTheme.Light)
            {
                App.Current.RequestedTheme = ApplicationTheme.Dark;
            }
            else
            {
                App.Current.RequestedTheme = ApplicationTheme.Light;
            }
        }
    }
}
