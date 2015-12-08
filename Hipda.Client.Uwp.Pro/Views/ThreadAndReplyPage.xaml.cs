using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        #region 三个属性，用于接收点击头像的参数
        public int PopupUserId { get; set; }

        public string PopupUsername { get; set; }

        public int PopupThreadId { get; set; }
        #endregion

        /// <summary>
        /// 用于记录当前页的类型，如常规、我的贴子、我的回复等等类型
        /// 在打开相应版块时赋值
        /// </summary>
        ThreadDataType _threadDataType;

        object _lastSelectedItem;
        ThreadAndReplyViewModel _threadAndReplyViewModel;

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
            ThreadListView.SelectedItem = _lastSelectedItem;
        }

        #region 委托事件
        private void LeftBeforeLoaded()
        {
            leftProgress.IsActive = true;
            leftProgress.Visibility = Visibility.Visible;
            //ThreadRefreshButton.IsEnabled = false;
            ReplyRefreshButton.IsEnabled = false;
            ReplyRefreshButton2.IsEnabled = false;
        }

        private void LeftAfterLoaded()
        {
            leftProgress.IsActive = false;
            leftProgress.Visibility = Visibility.Collapsed;
            //ThreadRefreshButton.IsEnabled = true;
            ReplyRefreshButton.IsEnabled = true;
            ReplyRefreshButton2.IsEnabled = true;
        }

        private void RightBeforeLoaded()
        {
            rightProgress.IsActive = true;
            rightProgress.Visibility = Visibility.Visible;
            //ThreadRefreshButton.IsEnabled = false;
            ReplyRefreshButton.IsEnabled = false;
            ReplyRefreshButton2.IsEnabled = false;
            rightFooter.Visibility = Visibility.Collapsed;
        }

        private async void RightAfterLoaded(int threadId, int pageNo)
        {
            rightProgress.IsActive = false;
            rightProgress.Visibility = Visibility.Collapsed;
            //ThreadRefreshButton.IsEnabled = true;
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

        private bool _isShownForPostDialog = false;
        public async void ShowPostDetailByPostId(int postId, int threadId)
        {
            if (_isShownForPostDialog == false)
            {
                PostDialog.DataContext = await _threadAndReplyViewModel.GetPostDetail(postId, threadId);
                _isShownForPostDialog = true;
                await PostDialog.ShowAsync();
            }
            else
            {
                PostDialog.DataContext = await _threadAndReplyViewModel.GetPostDetail(postId, threadId);
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
                    _threadDataType = ThreadDataType.Default;
                    string fid = param.Substring(4);
                    _threadAndReplyViewModel = new ThreadAndReplyViewModel(1, fid, ThreadListView, ThreadCommandBar, LeftBeforeLoaded, LeftAfterLoaded);
                    DataContext = _threadAndReplyViewModel;
                }
                else if (param.StartsWith("item="))
                {
                    string threadType = param.Substring(5);
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

                    _threadAndReplyViewModel = new ThreadAndReplyViewModel(1, threadType, ThreadListView, ThreadCommandBar, LeftBeforeLoaded, LeftAfterLoaded);
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
                    default:
                        var item = _lastSelectedItem as ThreadItemViewModel;
                        p = string.Format("{0},{1}", item.ThreadItem.ThreadId, item.ThreadItem.AuthorUserId);
                        break;
                }

                UserDialogContentControl.Content = null;
                UserDialog.Hide();

                Frame.Navigate(typeof(ReplyListPage), p, new SuppressNavigationTransitionInfo());
            }

            EntranceNavigationTransitionInfo.SetIsTargetElement(ThreadListView, isNarrow);
        }

        private async void ThreadListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 如果进入了选择模式，则不打开主题
            if (ThreadListView.SelectionMode == ListViewSelectionMode.Multiple)
            {
                return;
            }

            if (e.AddedItems.Count == 0)
            {
                return;
            }

            var selectedItem = e.AddedItems[0];
            switch (_threadDataType)
            {
                case ThreadDataType.MyThreads:
                    var itemForMyThreads = (ThreadItemForMyThreadsViewModel)selectedItem;
                    itemForMyThreads.SetRead();
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
                    var itemForMyPosts = (ThreadItemForMyPostsViewModel)selectedItem;
                    itemForMyPosts.SetRead();

                    _lastSelectedItem = itemForMyPosts;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        string p = string.Format("{0},{1}", itemForMyPosts.ThreadItem.ThreadId, 0);
                        Frame.Navigate(typeof(ReplyListPage), p, new DrillInNavigationTransitionInfo());
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
                        Frame.Navigate(typeof(ReplyListPage), p, new DrillInNavigationTransitionInfo());
                    }
                    else
                    {
                        itemForMyFavorites.SelectThreadItem(ReplyListView, PostReplyTextBox, RightBeforeLoaded, RightAfterLoaded);
                        RightWrap.DataContext = itemForMyFavorites;
                    }
                    break;
                default:
                    var item = (ThreadItemViewModel)selectedItem;
                    item.SetRead();
                    _lastSelectedItem = item;

                    if (AdaptiveStates.CurrentState == NarrowState)
                    {
                        
                        string p = string.Format("{0},{1}", item.ThreadItem.ThreadId, item.ThreadItem.AuthorUserId);
                        Frame.Navigate(typeof(ReplyListPage), p, new DrillInNavigationTransitionInfo());
                    }
                    else
                    {
                        item.SelectThreadItem(ReplyListView, PostReplyTextBox, RightBeforeLoaded, RightAfterLoaded);
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


        private void SortingButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ReadListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as ThreadItemModelBase;
            OpenReplyPageByThreadId(data.ThreadId);

            ThreadListView.SelectedItem = null;
        }

        /// <summary>
        /// 头像之上下文菜单项之事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void openThreadInNewView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var options = new LauncherOptions();
            options.TreatAsUntrusted = false;
            await Launcher.LaunchUriAsync(new Uri("hipda:tid=" + PopupThreadId), options);
        }

        #region 坛友资料及短消息之弹窗
        Grid _postUserMessageForm; // 发短消息之输入元素之容器
        GridView _userMessageFaceGridView; // 发短消息之表情图标
        TextBox _userMessageTextBox; // 发短消息之文本框
        Button _userMessagePostButton; // 发短消息之按钮
        UserDialogType _userDialogType = 0;

        private async void UserDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (_userDialogType == UserDialogType.Info)
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
                grid.Padding = new Thickness(16, 0, 16, 16);
                grid.Children.Add(img);
                grid.Children.Add(richTextBlock);

                var sv = new ScrollViewer();
                sv.Content = grid;
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                UserDialogContentControl.Content = sv;

                sender.PrimaryButtonText = "短消息";
            }
            else if(_userDialogType == UserDialogType.Message)
            {
                await PrepareUserMessage(sender);
            }

            sender.PrimaryButtonClick += OpenOrRefreshUserMessageDialog;

            var containerBorder = Common.FindParent<Border>(UserDialogContentControl) as Border; // 最先找到border容器不包含我要找的目标元素
            containerBorder = Common.FindParent<Border>(containerBorder) as Border; // 这次找到的border容器才包含我要找的目标元素
            _postUserMessageForm = containerBorder.FindName("PostUserMessageForm") as Grid;
            _postUserMessageForm.DataContext = new FaceService();
            _userMessageFaceGridView = _postUserMessageForm.FindName("UserMessageFaceGridView") as GridView;
            _userMessageFaceGridView.ItemClick += UserMessageFaceGridView_ItemClick;
            _userMessageTextBox = _postUserMessageForm.FindName("UserMessageTextBox") as TextBox;
            _userMessagePostButton = _postUserMessageForm.FindName("UserMessagePostButton") as Button;
            _userMessagePostButton.Tapped += UserMessagePostButton_Tapped;
        }

        private void UserMessageFaceGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_userMessageTextBox == null)
            {
                return;
            }

            var data = e.ClickedItem as FaceItemModel;
            if (data == null)
            {
                return;
            }

            string faceText = data.Text;

            int occurences = 0;
            string originalContent = _userMessageTextBox.Text;

            for (var i = 0; i < _userMessageTextBox.SelectionStart + occurences; i++)
            {
                if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                    occurences++;
            }

            int cursorPosition = _userMessageTextBox.SelectionStart + occurences;
            _userMessageTextBox.Text = _userMessageTextBox.Text.Insert(cursorPosition, faceText);
            _userMessageTextBox.SelectionStart = cursorPosition + faceText.Length;
        }

        private void UserDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            sender.PrimaryButtonClick -= OpenOrRefreshUserMessageDialog;

            if (_userMessageFaceGridView != null)
            {
                _userMessageFaceGridView.ItemClick -= UserMessageFaceGridView_ItemClick;
            }

            if (_userMessagePostButton != null)
            {
                _userMessagePostButton.Tapped -= UserMessagePostButton_Tapped;
            }

            _userMessageTextBox = null;
            _userMessagePostButton = null;
            _userMessageFaceGridView = null;
            _postUserMessageForm = null;
        }

        private async void OpenOrRefreshUserMessageDialog(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            await PrepareUserMessage(sender);
        }

        private void ShowTipsForUserMessage(string tips)
        {
            UserDialogContentControl.Content = new TextBlock
            {
                Text = tips,
                Margin = new Thickness(16),
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }

        private async void openUserInfoDialog_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (PopupUserId == 0)
            {
                return;
            }

            _userDialogType = UserDialogType.Info;
            ShowTipsForUserMessage("请稍候，载入中。。。");

            UserDialog.Title = string.Format("查看 {0} 的详细资料", PopupUsername);
            UserDialog.SecondaryButtonText = "关闭";
            await UserDialog.ShowAsync();

            
        }

        private async void openUserMessageDialog_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (PopupUserId == 0)
            {
                return;
            }

            _userDialogType = UserDialogType.Message;
            await UserDialog.ShowAsync();
        }

        private async void UserMessagePostButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _userMessagePostButton.IsEnabled = false;

            string msg = _userMessageTextBox.Text.Trim();
            if (string.IsNullOrEmpty(msg))
            {
                ShowTipsForUserMessage("请先填写消息内容。");
                _userMessagePostButton.IsEnabled = true;
                return;
            }

            bool isOk = await _threadAndReplyViewModel.PostUserMessage(msg, PopupUserId);
            if (isOk) // 发送成功
            {
                _userMessageTextBox.Text = string.Empty;

                ShowTipsForUserMessage("已发送成功，载入中。。。");

                // 这里要延迟载入短消息数据，以免取到的还是旧数据
                DelayPrepareUserMessage(1001);
            }
            else
            {
                ShowTipsForUserMessage("对不起，提交失败，请稍后再试。");
            }

            _userMessagePostButton.IsEnabled = true;
        }

        private async void DelayPrepareUserMessage(int interval)
        {
            DateTime now = DateTime.Now;
            while (now.AddMilliseconds(interval) > DateTime.Now)
            {
                await PrepareUserMessage(UserDialog);
            }
            return;
        }

        private async Task PrepareUserMessage(ContentDialog sender)
        {
            sender.IsPrimaryButtonEnabled = false;
            sender.PrimaryButtonText = "刷新";

            if (PopupUserId == 0)
            {
                ShowTipsForUserMessage("对不起，参数错误！");
                sender.IsPrimaryButtonEnabled = true;
                return;
            }

            ShowTipsForUserMessage("请稍候，载入中。。。");

            sender.Title = string.Format("与 {0} 聊天", PopupUsername);
            sender.SecondaryButtonText = "关闭";

            var btnGetAll = new HyperlinkButton
            {
                Content = "点击查看所有消息",
                Visibility = Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            btnGetAll.Tapped += async (s, e) =>
            {
                ShowTipsForUserMessage("请稍候，载入中。。。");
                await LoadAndShowUserMessage(null);
            };

            await LoadAndShowUserMessage(btnGetAll);

            sender.IsPrimaryButtonEnabled = true;
        }

        private async Task LoadAndShowUserMessage(HyperlinkButton getAllButton)
        {
            int limitCount = 5; // 默认只载入最新5条短消息
            if (getAllButton == null)
            {
                limitCount = -1;
            }

            // 读取数据
            var data = await _threadAndReplyViewModel.GetUserMessageData(PopupUserId, limitCount);
            if (data == null || data.Count == 0)
            {
                ShowTipsForUserMessage("你们之间还未开始。。。");
            }
            else
            {
                if (getAllButton != null && data.Count == limitCount)
                {
                    getAllButton.Visibility = Visibility.Visible;
                }

                var lv = new ListView();
                lv.Padding = new Thickness(16, 0, 16, 16);
                lv.IsItemClickEnabled = false;
                lv.IsSwipeEnabled = false;
                lv.CanDrag = false;
                lv.SelectionMode = ListViewSelectionMode.None;
                lv.ShowsScrollingPlaceholders = false;
                lv.ItemContainerStyle = App.Current.Resources["ReplyItemContainerStyle"] as Style;
                lv.ItemTemplateSelector = App.Current.Resources["userMessageListItemTemplateSelector"] as DataTemplateSelector;
                lv.ItemsSource = data;
                if (getAllButton != null)
                {
                    lv.Header = getAllButton;
                }

                UserDialogContentControl.Content = lv;
            }
        }
        #endregion

        private void PostDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var data = sender.DataContext as ReplyItemModel;
            PostReplyTextBox.Text = data.TextStr;
        }

        private void PostDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            _isShownForPostDialog = false;
        }

        //private void ReplyListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        //{
        //    ReplyItemModel item = args.Item as ReplyItemModel;
        //    Debug.WriteLine(item.FloorNo);
        //}
    }

    public enum UserDialogType
    {
        Info,
        Message
    }
}
