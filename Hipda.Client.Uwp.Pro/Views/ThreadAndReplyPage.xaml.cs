using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
        private ThreadItemViewModel _lastSelectedItem;
        private ThreadAndReplyViewModel _threadAndReplyViewModel;

        public ThreadAndReplyPage()
        {
            this.InitializeComponent();

            _threadAndReplyViewModel = new ThreadAndReplyViewModel(
                ThreadListView,
                () => {
                    leftProgress.IsActive = true;
                    leftProgress.Visibility = Visibility.Visible;
                }, 
                () => {
                    leftProgress.IsActive = false;
                    leftProgress.Visibility = Visibility.Collapsed;
                });

            DataContext = _threadAndReplyViewModel;
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadListView.SelectedItem = _lastSelectedItem;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                RightWrap.DataContext = null;

                string[] p = e.Parameter.ToString().Split(',');
                int threadId = Convert.ToInt32(p[0]);
                int threadAuthorUserId = Convert.ToInt32(p[1]);

                _lastSelectedItem = new ThreadItemViewModel(threadId, threadAuthorUserId, ReplyListView, 
                    () =>
                    {
                        rightProgress.IsActive = true;
                        rightProgress.Visibility = Visibility.Visible;
                    },
                    () =>
                    {
                        rightProgress.IsActive = false;
                        rightProgress.Visibility = Visibility.Collapsed;
                    });

                RightWrap.DataContext = _lastSelectedItem;
                ReplyListView.ItemsSource = _lastSelectedItem.ReplyItemCollection;
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
                string p = string.Format("{0},{1}", _lastSelectedItem.ThreadItem.ThreadId, _lastSelectedItem.ThreadItem.AuthorUserId);
                Frame.Navigate(typeof(ReplyListPage), p, new SuppressNavigationTransitionInfo());
            }

            EntranceNavigationTransitionInfo.SetIsTargetElement(ThreadListView, isNarrow);
        }

        private void ThreadListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (ThreadItemViewModel)e.ClickedItem;
            _lastSelectedItem = clickedItem;

            if (AdaptiveStates.CurrentState == NarrowState)
            {
                string p = string.Format("{0},{1}", clickedItem.ThreadItem.ThreadId, clickedItem.ThreadItem.AuthorUserId);
                Frame.Navigate(typeof(ReplyListPage), p, new DrillInNavigationTransitionInfo());
            }
            else
            {
                RightWrap.DataContext = null;

                _lastSelectedItem = e.ClickedItem as ThreadItemViewModel;
                _lastSelectedItem.SelectThreadItem(
                    ReplyListView,
                    () => {
                        rightProgress.IsActive = true;
                        rightProgress.Visibility = Visibility.Visible;
                    },
                    () => {
                        rightProgress.IsActive = false;
                        rightProgress.Visibility = Visibility.Collapsed;
                    });

                RightWrap.DataContext = _lastSelectedItem;
                ReplyListView.ItemsSource = _lastSelectedItem.ReplyItemCollection;
            }

            //var listItem = sender as ListView;
            //var itemStatusBar = listItem.FindName("ItemStatusBar") as Border;
            //itemStatusBar.Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}
