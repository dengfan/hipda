using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
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
        #region 两个依赖属性，用于接收点击头像的参数
        public int PopupUserId
        {
            get { return (int)GetValue(PopupUserIdProperty); }
            set { SetValue(PopupUserIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PopupUserId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupUserIdProperty =
            DependencyProperty.Register("PopupUserId", typeof(int), typeof(ThreadAndReplyPage), new PropertyMetadata(0));


        public string PopupUsername
        {
            get { return (string)GetValue(PopupUsernameProperty); }
            set { SetValue(PopupUsernameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PopupUsername.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupUsernameProperty =
            DependencyProperty.Register("PopupUsername", typeof(string), typeof(ThreadAndReplyPage), new PropertyMetadata(string.Empty));


        public int PopupThreadId
        {
            get { return (int)GetValue(PopupThreadIdProperty); }
            set { SetValue(PopupThreadIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PopupThreadId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupThreadIdProperty =
            DependencyProperty.Register("PopupThreadId", typeof(int), typeof(ThreadAndReplyPage), new PropertyMetadata(0));
        #endregion

        private object _lastSelectedItem;
        private ThreadAndReplyViewModel _threadAndReplyViewModel;

        public ThreadAndReplyPage()
        {
            this.InitializeComponent();

            this.SizeChanged += (s, e) => {
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
            ThreadListView.SelectedItem = _lastSelectedItem;
        }

        #region 委托事件
        private void LeftBeforeLoaded()
        {
            leftProgress.IsActive = true;
            leftProgress.Visibility = Visibility.Visible;
            ThreadRefreshButton.IsEnabled = false;
            ReplyRefreshButton.IsEnabled = false;
            ReplyRefreshButton2.IsEnabled = false;
        }

        private void LeftAfterLoaded()
        {
            leftProgress.IsActive = false;
            leftProgress.Visibility = Visibility.Collapsed;
            ThreadRefreshButton.IsEnabled = true;
            ReplyRefreshButton.IsEnabled = true;
            ReplyRefreshButton2.IsEnabled = true;
        }

        private void RightBeforeLoaded()
        {
            rightProgress.IsActive = true;
            rightProgress.Visibility = Visibility.Visible;
            ThreadRefreshButton.IsEnabled = false;
            ReplyRefreshButton.IsEnabled = false;
            ReplyRefreshButton2.IsEnabled = false;
            rightFooter.Visibility = Visibility.Collapsed;
        }

        private async void RightAfterLoaded(int threadId)
        {
            rightProgress.IsActive = false;
            rightProgress.Visibility = Visibility.Collapsed;
            ThreadRefreshButton.IsEnabled = true;
            ReplyRefreshButton.IsEnabled = true;
            ReplyRefreshButton2.IsEnabled = true;
            rightFooter.Visibility = Visibility.Visible;

            _threadAndReplyViewModel.AddToReadHistory(threadId);

            // 最宽屏模式下，自动滚到最底部
            if (RightSideColumn.ActualWidth > 0)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    int count = ReadListView.Items.Count;
                    ReadListView.ScrollIntoView(ReadListView.Items[count - 1], ScrollIntoViewAlignment.Leading);
                });
            }
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

        private void OpenReplyPageByThreadId(int threadId)
        {
            var threadItem = _threadAndReplyViewModel.GetThreadItem(threadId);
            if (threadItem == null)
            {
                var item = new ThreadItemViewModel(1, threadId, 0, ReplyListView, PostReplyTextBox, RightBeforeLoaded, RightAfterLoaded, OpenReplyPageByThreadId);
                RightWrap.DataContext = item;
            }
            else
            {
                var item = new ThreadItemViewModel(threadItem);
                item.SelectThreadItem(ReplyListView, PostReplyTextBox, RightBeforeLoaded, RightAfterLoaded, OpenReplyPageByThreadId);
                RightWrap.DataContext = item;
            }
        }
        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                string param = e.Parameter.ToString();

                if (param.StartsWith("fid=")) // 表示要加载指定的贴子列表页
                {
                    int fid = Convert.ToInt32(param.Substring(4));
                    _threadAndReplyViewModel = new ThreadAndReplyViewModel(1, fid, ThreadListView, LeftBeforeLoaded, LeftAfterLoaded);
                    DataContext = _threadAndReplyViewModel;
                }
                else if (param.StartsWith("item="))
                {
                    string itemType = param.Substring(5);
                    if (itemType.Equals("threads"))
                    {
                        _threadAndReplyViewModel = new ThreadAndReplyViewModel(1, itemType, ThreadListView, LeftBeforeLoaded, LeftAfterLoaded);
                        DataContext = _threadAndReplyViewModel;
                    }
                    else if (itemType.Equals("posts"))
                    {
                        _threadAndReplyViewModel = new ThreadAndReplyViewModel(1, itemType, ThreadListView, LeftBeforeLoaded, LeftAfterLoaded);
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
                            var itemForMyThreads = new ThreadItemForMyThreadsViewModel(1, threadId, threadAuthorUserId, ReplyListView, RightBeforeLoaded, RightAfterLoaded, OpenReplyPageByThreadId);
                            _lastSelectedItem = itemForMyThreads;
                            RightWrap.DataContext = itemForMyThreads;
                            break;
                        case ThreadDataType.MyPosts:
                            var itemForMyPosts = new ThreadItemForMyPostsViewModel(1, threadId, threadAuthorUserId, ReplyListView, RightBeforeLoaded, RightAfterLoaded, OpenReplyPageByThreadId);
                            _lastSelectedItem = itemForMyPosts;
                            RightWrap.DataContext = itemForMyPosts;
                            break;
                        default:
                            var item = new ThreadItemViewModel(1, threadId, threadAuthorUserId, ReplyListView, PostReplyTextBox, RightBeforeLoaded, RightAfterLoaded, OpenReplyPageByThreadId);
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
                        itemForMyThreads.SelectThreadItem(ReplyListView, RightBeforeLoaded, RightAfterLoaded);
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
                        itemForMyPosts.SelectThreadItem(ReplyListView, RightBeforeLoaded, RightAfterLoaded, ReplyListViewScroll, OpenReplyPageByThreadId);
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
                        item.SelectThreadItem(ReplyListView, PostReplyTextBox, RightBeforeLoaded, RightAfterLoaded, OpenReplyPageByThreadId);
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
            if (_lastSelectedItem != null)
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
        }

        private void rightPr_RefreshInvoked(DependencyObject sender, object args)
        {
            if (_lastSelectedItem != null)
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
        }

        private void SortingButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ReadListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ThreadItemModelBase data = e.ClickedItem as ThreadItemModelBase;
            OpenReplyPageByThreadId(data.ThreadId);
        }

        private async void openUserDialog_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (PopupUserId == 0)
            {
                return;
            }

            var loading = new TextBlock
            {
                Text = "请稍候，载入中。。。"
            };

            UserDialogContentControl.Content = loading;
            UserDialog.Title = string.Format("查看 {0} 的详细资料", PopupUsername);
            UserDialog.PrimaryButtonText = "发短消息";
            UserDialog.PrimaryButtonClick += PostUserMessage;
            UserDialog.SecondaryButtonText = "关闭";

            await UserDialog.ShowAsync();
        }

        private async void PostUserMessage(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;

            if (PopupUserId == 0)
            {
                var textBlock = new TextBlock { Text = "对不起，参数错误！" };
                UserDialogContentControl.Content = textBlock;
                return;
            }
            else
            {
                var textBlock = new TextBlock { Text = "请稍候，载入中。。。" };
                UserDialogContentControl.Content = textBlock;
            }

            sender.Title = string.Format("与 {0} 聊天", PopupUsername);
            sender.PrimaryButtonText = "发送";
            sender.SecondaryButtonText = "关闭";

            var grid = new Grid();
            grid.HorizontalAlignment = HorizontalAlignment.Stretch;
            grid.RowDefinitions.Insert(0, new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Insert(1, new RowDefinition { Height = GridLength.Auto });

            var btnGetAll = new HyperlinkButton();
            btnGetAll.Content = "点击查看所有消息";
            btnGetAll.Visibility = Visibility.Collapsed;
            btnGetAll.HorizontalAlignment = HorizontalAlignment.Center;

            var lv = new ListView();
            lv.SetValue(Grid.RowProperty, 0);
            lv.IsItemClickEnabled = false;
            lv.IsSwipeEnabled = false;
            lv.CanDrag = false;
            lv.SelectionMode = ListViewSelectionMode.None;
            lv.ShowsScrollingPlaceholders = false;
            lv.ItemContainerStyle = Application.Current.Resources["ReplyItemContainerStyle"] as Style;
            lv.ItemTemplateSelector = Application.Current.Resources["userMessageListItemTemplateSelector"] as DataTemplateSelector;
            lv.Header = btnGetAll;

            var data = await _threadAndReplyViewModel.GetUserMessageData(PopupUserId);
            data.Reverse();
            if (data.Count > 5)
            {
                btnGetAll.Visibility = Visibility.Visible;
                data = data.Take(5).ToList();
            }
            lv.ItemsSource = data;

            var tb = new TextBox();
            tb.SetValue(Grid.RowProperty, 1);
            tb.PlaceholderText = "编辑短消息";

            grid.Children.Add(lv);
            grid.Children.Add(tb);
            UserDialogContentControl.Content = grid;
        }

        private async void UserDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            var bi = new BitmapImage();
            bi.UriSource = MyAvatar.GetAvatarUrl(PopupUserId);
            var img = new Image();
            img.Stretch = Stretch.None;
            img.Source = bi;
            img.VerticalAlignment = VerticalAlignment.Top;
            img.HorizontalAlignment = HorizontalAlignment.Right;

            string xaml = await _threadAndReplyViewModel.GetXamlForUserInfo(PopupUserId);
            var richTextBlock = XamlReader.Load(xaml) as RichTextBlock;

            var grid = new Grid();
            grid.Children.Add(img);
            grid.Children.Add(richTextBlock);
            UserDialogContentControl.Content = grid;
        }
    }
}
