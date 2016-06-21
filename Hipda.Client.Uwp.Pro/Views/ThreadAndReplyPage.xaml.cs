using Hipda.Client.Uwp.Pro.Controls;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Hipda.Client.Uwp.Pro.Views
{
    public sealed partial class ThreadAndReplyPage : Page
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
            DependencyProperty.Register("ThreadId", typeof(int), typeof(ThreadAndReplyPage), new PropertyMetadata(1802310, new PropertyChangedCallback(OnThreadIdChanged)));

        private static void OnThreadIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue > 0)
            {
                var instance = d as ThreadAndReplyPage;
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
            DependencyProperty.Register("PostId", typeof(int), typeof(ThreadAndReplyPage), new PropertyMetadata(0, new PropertyChangedCallback(OnPostIdChanged)));

        private static void OnPostIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue > 0)
            {
                var instance = d as ThreadAndReplyPage;
                instance.ThreadId = 0;
            }
        }
        #endregion


        public int Countdown
        {
            get { return (int)GetValue(CountdownProperty); }
            set { SetValue(CountdownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Countdown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountdownProperty =
            DependencyProperty.Register("Countdown", typeof(int), typeof(ThreadAndReplyPage), new PropertyMetadata(0));


        MainPage _mainPage;

        public ThreadAndReplyPage()
        {
            this.InitializeComponent();

            this.SizeChanged += (s, e) =>
            {
                string userInteractionType = Windows.UI.ViewManagement.UIViewSettings.GetForCurrentView().UserInteractionMode.ToString();
                if (userInteractionType.Equals("Touch"))
                {
                    InkButton.Width = 80;
                    InkButton.Height = 40;
                    FaceButton.Width = 80;
                    FaceButton.Height = 40;
                    FileButton.Width = 80;
                    FileButton.Height = 40;
                    SendButton.Height = 36;
                    ShortcutKeyButton.Height = 36;
                }
                else if (userInteractionType.Equals("Mouse"))
                {
                    InkButton.Width = 32;
                    InkButton.Height = 32;
                    FaceButton.Width = 32;
                    FaceButton.Height = 32;
                    FileButton.Width = 32;
                    FileButton.Height = 32;
                    SendButton.Height = 28;
                    ShortcutKeyButton.Height = 28;
                }

                ContentTextBox.MaxHeight = this.ActualHeight / 2;
            };

            var frame = (Frame)Window.Current.Content;
            _mainPage = (MainPage)frame.Content;
            var countdownBinding = new Binding { Path = new PropertyPath("Countdown"), Source = _mainPage };
            this.SetBinding(ThreadAndReplyPage.CountdownProperty, countdownBinding);
        }

        #region 委托事件
        private void LeftBeforeLoaded()
        {
            leftProgress.IsActive = true;
            leftProgress.Visibility = Visibility.Visible;
            ReplyRefreshToFirstPageButton.IsEnabled = false;
            ReplyRefreshToLastPageButton.IsEnabled = false;
        }

        private void LeftAfterLoaded()
        {
            leftProgress.IsActive = false;
            leftProgress.Visibility = Visibility.Collapsed;
            ReplyRefreshToFirstPageButton.IsEnabled = true;
            ReplyRefreshToLastPageButton.IsEnabled = true;
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
            ReplyRefreshToFirstPageButton.IsEnabled = false;
            ReplyRefreshToLastPageButton.IsEnabled = false;
        }

        private void RightAfterLoaded(int threadId, int pageNo)
        {
            ThreadId = threadId;

            rightProgress.IsActive = false;
            rightProgress.Visibility = Visibility.Collapsed;
            ReplyRefreshToFirstPageButton.IsEnabled = true;
            ReplyRefreshToLastPageButton.IsEnabled = true;

            bool isShown = ReplyListService.CanShowButtonForLoadPrevReplyPage(threadId);
            if (isShown)
            {
                ReplyListView.HeaderTemplate = (DataTemplate)App.Current.Resources["ReplyListViewHeaderTemplate"];
            }
            else
            {
                ReplyListView.HeaderTemplate = null;
            }

            var cts = new CancellationTokenSource();
            var vm = new SendThreadQuickReplyControlViewModel(cts, ThreadId, BeforeUpload, InsertFileCodeIntoContextTextBox, AfterUpload, SentFailed, SentSuccess);
            QuickReplyPanel.DataContext = vm;
        }

        async void ReplyListViewScrollForSpecifiedPost(int index)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var vmType = ReplyListView.DataContext.GetType();
                if (vmType.Equals(typeof(ReplyListViewForSpecifiedPostViewModel)))
                {
                    var vm = (ReplyListViewForSpecifiedPostViewModel)ReplyListView.DataContext;
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
                }
                else if (vmType.Equals(typeof(ReplyListViewForDefaultViewModel)))
                {
                    var vm = (ReplyListViewForDefaultViewModel)ReplyListView.DataContext;
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
                }
            });
        }

        void OpenCreateThreadPanel()
        {
            _mainPage?.OpenSendNewThreadPanel();
        }
        #endregion

        #region 打开回复列表页必须使用以下两种方法之一
        /// <summary>
        /// 通过 ThreadId 打开回复列表页，打开后从1楼开始显示
        /// </summary>
        public void OpenReplyPageByThreadId()
        {
            if (AdaptiveStates.CurrentState == NarrowState)
            {
                string p = $"tid={ThreadId}";
                Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
            }
            else
            {
                var cts = new CancellationTokenSource();
                var vm = new ReplyListViewForDefaultViewModel(cts, ThreadId, ReplyListView, RightBeforeLoaded, RightAfterLoaded, ReplyListViewScrollForSpecifiedPost);
                RightWrap.DataContext = vm;
            }
        }

        /// <summary>
        /// 通过 PostId 打开回复列表页，并且打开后跳到相应的回复楼层
        /// </summary>
        public void OpenReplyPageByPostId()
        {
            if (AdaptiveStates.CurrentState == NarrowState)
            {
                string p = $"pid={PostId}";
                Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
            }
            else
            {
                var cts = new CancellationTokenSource();
                RightWrap.DataContext = new ReplyListViewForSpecifiedPostViewModel(cts, PostId, ReplyListView, RightBeforeLoaded, RightAfterLoaded, ReplyListViewScrollForSpecifiedPost);
            }
        }
        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            OpenReplyPageByThreadId();
            leftNoDataNoticePanel.Visibility = Visibility.Collapsed;

            if (e.Parameter != null)
            {
                string param = e.Parameter.ToString();
                if (param.StartsWith("fid=")) // 表示要加载指定的贴子列表页
                {
                    int fid = Convert.ToInt32(param.Substring("fid=".Length));
                    LeftWrap.DataContext = new DefaultThreadListViewViewModel(1, fid, LeftListView, LeftCommandBar, OpenCreateThreadPanel, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                }
                else if (param.StartsWith("item="))
                {
                    string threadType = param.Substring("item=".Length);
                    switch (threadType)
                    {
                        case "threads":
                            LeftWrap.DataContext = new MyThreadsThreadListViewViewModel(1, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                            break;
                        case "posts":
                            LeftWrap.DataContext = new MyPostsThreadListViewViewModel(1, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                            break;
                        case "favorites":
                            LeftWrap.DataContext = new MyFavoritesThreadListViewViewModel(1, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                            break;
                        case "notice":
                            LeftWrap.DataContext = new NoticeThreadListViewViewModel(LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
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
                        LeftWrap.DataContext = new SearchTitleThreadListViewViewModel(1, searchKeyowrd, searchAuthor, searchType, searchTimeSpan, searchForumSpan, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                    }
                    else
                    {
                        LeftWrap.DataContext = new SearchFullTextThreadListViewViewModel(1, searchKeyowrd, searchAuthor, searchType, searchTimeSpan, searchForumSpan, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                    }
                }
                else if (param.Contains("tid=")) // 表示要加载指定的回复列表页，从窄视图变宽后导航而来
                {
                    ThreadId = Convert.ToInt32(param.Substring("tid=".Length));
                    OpenReplyPageByThreadId();
                }
                else if (param.Contains("pid=")) // 表示要加载指定的回复列表页，从窄视图变宽后导航而来
                {
                    PostId = Convert.ToInt32(param.Substring("pid=".Length));
                    OpenReplyPageByPostId();
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
            if (isNarrow && oldState == DefaultState) // 从宽视图进入窄视图
            {
                if (PostId > 0)
                {
                    Frame.Navigate(typeof(ReplyListPage), $"pid={PostId}");
                }
                else if (ThreadId > 0)
                {
                    Frame.Navigate(typeof(ReplyListPage), $"tid={ThreadId}");
                }
            }
        }

        private void LeftListView_ItemClick(object sender, ItemClickEventArgs e) 
        {
            // 如果进入了选择模式，则不打开主题
            if (LeftListView.SelectionMode == ListViewSelectionMode.Multiple)
            {
                return;
            }

            var selectedItem = (ThreadItemModelBase)e.ClickedItem;
            if (selectedItem == null)
            {
                return;
            }

            ThreadId = selectedItem.ThreadId;
            PostId = selectedItem.PostId;

            if (PostId > 0)
            {
                if (AdaptiveStates.CurrentState == NarrowState)
                {
                    string p = $"pid={PostId}";
                    Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                }
                else
                {
                    OpenReplyPageByPostId();
                }
            }
            else if (ThreadId > 0)
            {
                if (AdaptiveStates.CurrentState == NarrowState)
                {
                    string p = $"tid={ThreadId}";
                    Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                }
                else
                {
                    OpenReplyPageByThreadId();
                }
            }
        }

        #region 加载上一页
        private void leftPr_RefreshInvoked(DependencyObject sender, object args)
        {
            if (LeftWrap.DataContext == null)
            {
                return;
            }

            var vmType = LeftWrap.DataContext.GetType();
            if (vmType.Equals(typeof(DefaultThreadListViewViewModel)))
            {
                var vm = LeftWrap.DataContext as DefaultThreadListViewViewModel;
                vm.LoadPrevPageData();
            }
            else if (vmType.Equals(typeof(MyThreadsThreadListViewViewModel)))
            {
                var vm = LeftWrap.DataContext as MyThreadsThreadListViewViewModel;
                vm.LoadPrevPageData();
            }
            else if (vmType.Equals(typeof(MyPostsThreadListViewViewModel)))
            {
                var vm = LeftWrap.DataContext as MyPostsThreadListViewViewModel;
                vm.LoadPrevPageData();
            }
            else if (vmType.Equals(typeof(MyFavoritesThreadListViewViewModel)))
            {
                var vm = LeftWrap.DataContext as MyFavoritesThreadListViewViewModel;
                vm.LoadPrevPageData();
            }
        }

        private void rightPr_RefreshInvoked(DependencyObject sender, object args)
        {
            if (RightWrap.DataContext == null)
            {
                return;
            }

            if (RightWrap.DataContext.GetType().Equals(typeof(ReplyListViewForDefaultViewModel)))
            {
                var vm = RightWrap.DataContext as ReplyListViewForDefaultViewModel;
                vm.LoadPrevPageData();
            }
            else if (RightWrap.DataContext.GetType().Equals(typeof(ReplyListViewForSpecifiedPostViewModel)))
            {
                var vm = RightWrap.DataContext as ReplyListViewForSpecifiedPostViewModel;
                vm.LoadPrevPageData();
            }
        }
        #endregion

        

        private void PostDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var data = sender.DataContext as ReplyItemModel;
            //PostReplyTextBox.Text = data.TextStr;
        }

        #region 快速回贴
        int _autoRemoveTipTimerCount;
        DispatcherTimer _autoRemoveTipTimer;

        public void SendMessageTimerSetup()
        {
            _autoRemoveTipTimerCount = 3;
            _autoRemoveTipTimer = new DispatcherTimer();
            _autoRemoveTipTimer.Tick += AutoRemoveTipTimer_Tick;
            _autoRemoveTipTimer.Interval = new TimeSpan(0, 0, 1);
            _autoRemoveTipTimer.Start();
        }

        private void AutoRemoveTipTimer_Tick(object sender, object e)
        {
            if (_autoRemoveTipTimerCount == 0)
            {
                _autoRemoveTipTimer.Stop();
                return;
            }

            _autoRemoveTipTimerCount--;
        }

        void BeforeUpload(int fileIndex, int fileCount, string fileName)
        {
            _mainPage?.ShowTipsBarWhenUpload($"{fileName} 上载中 {fileIndex}/{fileCount}");
            SendMessageTimerSetup();
        }

        void InsertFileCodeIntoContextTextBox(string fileCode)
        {
            int occurences = 0;
            string originalContent = ContentTextBox.Text;

            for (var i = 0; i < ContentTextBox.SelectionStart + occurences; i++)
            {
                if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                    occurences++;
            }

            int cursorPosition = ContentTextBox.SelectionStart + occurences;
            ContentTextBox.Text = ContentTextBox.Text.Insert(cursorPosition, fileCode);
            ContentTextBox.SelectionStart = cursorPosition + fileCode.Length;
            ContentTextBox.Focus(FocusState.Programmatic);
        }

        void AfterUpload(int fileCount)
        {
            _mainPage?.ShowTipsBar($"文件上传已完成，共上传 {fileCount} 个文件。", false);
            SendMessageTimerSetup();
        }

        void SentFailed(string errorText)
        {
            _mainPage?.ShowTipsBar(errorText, true);
            SendMessageTimerSetup();
        }

        void SentSuccess(string title)
        {
            // 回贴成功后，刷新贴子到底部
            var vmType = RightWrap.DataContext.GetType();
            if (vmType.Equals(typeof(ReplyListViewForDefaultViewModel)))
            {
                var vm = (ReplyListViewForDefaultViewModel)RightWrap.DataContext;
                vm.LoadLastPageDataCommand.Execute(null);
            }
            else if (vmType.Equals(typeof(ReplyListViewForSpecifiedPostViewModel)))
            {
                var vm = (ReplyListViewForSpecifiedPostViewModel)RightWrap.DataContext;
                vm.LoadLastPageDataCommand.Execute(null);
            }

            // 开始倒计时
            _mainPage?.SendMessageTimerSetup();
        }


        void EmojiGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = (EmojiItemModel)e.ClickedItem;
            if (data == null)
            {
                return;
            }

            string faceText = data.Label;

            int occurences = 0;
            string originalContent = ContentTextBox.Text;

            for (var i = 0; i < ContentTextBox.SelectionStart + occurences; i++)
            {
                if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                    occurences++;
            }

            int cursorPosition = ContentTextBox.SelectionStart + occurences;
            ContentTextBox.Text = ContentTextBox.Text.Insert(cursorPosition, faceText);
            ContentTextBox.SelectionStart = cursorPosition + faceText.Length;
            ContentTextBox.Focus(FocusState.Pointer);
        }

        void FaceGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = (FaceItemModel)e.ClickedItem;
            if (data == null)
            {
                return;
            }

            string faceText = data.Text;

            int occurences = 0;
            string originalContent = ContentTextBox.Text;

            for (var i = 0; i < ContentTextBox.SelectionStart + occurences; i++)
            {
                if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                    occurences++;
            }

            int cursorPosition = ContentTextBox.SelectionStart + occurences;
            ContentTextBox.Text = ContentTextBox.Text.Insert(cursorPosition, faceText);
            ContentTextBox.SelectionStart = cursorPosition + faceText.Length;
            ContentTextBox.Focus(FocusState.Pointer);
        }

        private void QuickReplyPanel_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "拖放到此处即可上传文件";
        }

        private async void QuickReplyPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var files = await e.DataView.GetStorageItemsAsync();
                var cts = new CancellationTokenSource();
                var vm = ((FrameworkElement)sender).DataContext as SendThreadQuickReplyControlViewModel;
                vm.UploadMultipleFiles(cts, files, BeforeUpload, AfterUpload);
            }
        }

        private async void ContentTextBox_Paste(object sender, TextControlPasteEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as SendThreadQuickReplyControlViewModel;

            var cts = new CancellationTokenSource();
            var dpv = Clipboard.GetContent();
            if (dpv.Contains(StandardDataFormats.Bitmap))
            {
                e.Handled = true;
                var file = await dpv.GetBitmapAsync();
                vm.UploadSingleFile(cts, file, BeforeUpload, AfterUpload);
            }

            if (dpv.Contains(StandardDataFormats.StorageItems))
            {
                e.Handled = true;
                var files = await dpv.GetStorageItemsAsync();
                vm.UploadMultipleFiles(cts, files, BeforeUpload, AfterUpload);
            }
        }

        #endregion
    }
}
