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
        private ThreadItemViewModel _lastSelectedItem;
        private ThreadAndReplyViewModel _threadAndReplyViewModel;

        public ThreadAndReplyPage()
        {
            this.InitializeComponent();
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
                string param = e.Parameter.ToString();

                if (param.StartsWith("fid=")) // 表示要加载指定的贴子列表页
                {
                    int fid = Convert.ToInt32(param.Substring(4));

                    _threadAndReplyViewModel = new ThreadAndReplyViewModel(
                        1,
                        fid,
                        ThreadListView,
                        () => {
                            leftProgress.IsActive = true;
                            leftProgress.Visibility = Visibility.Visible;
                            ThreadRefreshButton.IsEnabled = false;
                            ReplyRefreshButton.IsEnabled = false;
                        },
                        () => {
                            leftProgress.IsActive = false;
                            leftProgress.Visibility = Visibility.Collapsed;
                            ThreadRefreshButton.IsEnabled = true;
                            ReplyRefreshButton.IsEnabled = true;
                        });

                    DataContext = _threadAndReplyViewModel;
                }
                else if (param.StartsWith("item="))
                {
                    string itemType = param.Substring(5);
                    if (itemType.Equals("threads"))
                    {
                        _threadAndReplyViewModel = new ThreadAndReplyViewModel(
                        1,
                        itemType,
                        ThreadListView,
                        () => {
                            leftProgress.IsActive = true;
                            leftProgress.Visibility = Visibility.Visible;
                            ThreadRefreshButton.IsEnabled = false;
                            ReplyRefreshButton.IsEnabled = false;
                        },
                        () => {
                            leftProgress.IsActive = false;
                            leftProgress.Visibility = Visibility.Collapsed;
                            ThreadRefreshButton.IsEnabled = true;
                            ReplyRefreshButton.IsEnabled = true;
                        });

                        DataContext = _threadAndReplyViewModel;
                    }
                    else if (itemType.Equals("posts"))
                    {
                        _threadAndReplyViewModel = new ThreadAndReplyViewModel(
                        1,
                        itemType,
                        ThreadListView,
                        () => {
                            leftProgress.IsActive = true;
                            leftProgress.Visibility = Visibility.Visible;
                            ThreadRefreshButton.IsEnabled = false;
                            ReplyRefreshButton.IsEnabled = false;
                        },
                        () => {
                            leftProgress.IsActive = false;
                            leftProgress.Visibility = Visibility.Collapsed;
                            ThreadRefreshButton.IsEnabled = true;
                            ReplyRefreshButton.IsEnabled = true;
                        });

                        DataContext = _threadAndReplyViewModel;
                    }
                }
                else if (param.Contains(",")) // 表示要加载指定的回复列表页
                {
                    string[] p = param.Split(',');
                    int threadId = Convert.ToInt32(p[0]);
                    int threadAuthorUserId = Convert.ToInt32(p[1]);

                    RightWrap.DataContext = null;

                    _lastSelectedItem = new ThreadItemViewModel(1, threadId, threadAuthorUserId, ReplyListView,
                        () =>
                        {
                            rightProgress.IsActive = true;
                            rightProgress.Visibility = Visibility.Visible;
                            ThreadRefreshButton.IsEnabled = false;
                            ReplyRefreshButton.IsEnabled = false;
                        },
                        () =>
                        {
                            rightProgress.IsActive = false;
                            rightProgress.Visibility = Visibility.Collapsed;
                            ThreadRefreshButton.IsEnabled = true;
                            ReplyRefreshButton.IsEnabled = true;
                        });

                    RightWrap.DataContext = _lastSelectedItem;
                    ReplyListView.ItemsSource = _lastSelectedItem.ReplyItemCollection;
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
                string p = string.Format("{0},{1}", _lastSelectedItem.ThreadItem.ThreadId, _lastSelectedItem.ThreadItem.AuthorUserId);
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
                    break;
                case ThreadDataType.MyPosts:
                    break;
                default:
                    _lastSelectedItem = (ThreadItemViewModel)e.ClickedItem;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        _lastSelectedItem.SetRead();
                        string p = string.Format("{0},{1}", _lastSelectedItem.ThreadItem.ThreadId, _lastSelectedItem.ThreadItem.AuthorUserId);
                        Frame.Navigate(typeof(ReplyListPage), p, new DrillInNavigationTransitionInfo());
                    }
                    else
                    {
                        RightWrap.DataContext = null;

                        _lastSelectedItem.SetRead();
                        _lastSelectedItem.SelectThreadItem(
                            ReplyListView,
                            () => {
                                rightProgress.IsActive = true;
                                rightProgress.Visibility = Visibility.Visible;
                                ThreadRefreshButton.IsEnabled = false;
                                ReplyRefreshButton.IsEnabled = false;
                            },
                            () => {
                                rightProgress.IsActive = false;
                                rightProgress.Visibility = Visibility.Collapsed;
                                ThreadRefreshButton.IsEnabled = true;
                                ReplyRefreshButton.IsEnabled = true;
                            });

                        RightWrap.DataContext = _lastSelectedItem;
                        ReplyListView.ItemsSource = _lastSelectedItem.ReplyItemCollection;
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
            _threadAndReplyViewModel.RefreshThreadDataFromPrevPage();
        }

        private void rightPr_RefreshInvoked(DependencyObject sender, object args)
        {
            _lastSelectedItem.RefreshReplyDataFromPrevPage();
        }
    }
}
