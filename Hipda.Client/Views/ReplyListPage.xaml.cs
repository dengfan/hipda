﻿using Hipda.Client.Models;
using Hipda.Client.ViewModels;
using System;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Hipda.Client.Views
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
            ReplyRefreshToFirstPageButton.IsEnabled = false;
            ReplyRefreshToLastPageButton.IsEnabled = false;
        }

        void AfterLoaded(int threadId, int pageNo)
        {
            rightProgress.IsActive = false;
            rightProgress.Visibility = Visibility.Collapsed;
            ReplyRefreshToFirstPageButton.IsEnabled = true;
            ReplyRefreshToLastPageButton.IsEnabled = true;

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
        #endregion

        #region 打开回复列表页必须使用以下两种方法之一
        public void OpenReplyPageByThreadId()
        {
            var cts = new CancellationTokenSource();
            RightWrap.DataContext = new ReplyListViewForDefaultViewModel(cts, ThreadId, ReplyListView, BeforeLoaded, AfterLoaded, ReplyListViewScrollForSpecifiedPost);

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

        public int Countdown
        {
            get { return (int)GetValue(CountdownProperty); }
            set { SetValue(CountdownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Countdown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountdownProperty =
            DependencyProperty.Register("Countdown", typeof(int), typeof(ReplyListPage), new PropertyMetadata(0));


        MainPage _mainPage;

        public ReplyListPage()
        {
            InitializeComponent();

            var frame = (Frame)Window.Current.Content;
            _mainPage = (MainPage)frame.Content;
            var countdownBinding = new Binding { Path = new PropertyPath("Countdown"), Source = _mainPage };
            SetBinding(CountdownProperty, countdownBinding);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

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

            // Register for hardware and software back request from the system
            SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.BackRequested += DetailPage_BackRequested;
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
            systemNavigationManager.BackRequested -= DetailPage_BackRequested;
            systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void OnBackRequested()
        {
            // Page above us will be our master view.
            // Make sure we are using the "drill out" animation in this transition.

            Frame.GoBack(new DrillInNavigationTransitionInfo());
        }

        void DetailPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            // Mark event as handled so we don't get bounced out of the app.
            e.Handled = true;

            OnBackRequested();
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
                var cts = new CancellationTokenSource();
                if (PostId > 0)
                {
                    DataContext = new ReplyListViewForSpecifiedPostViewModel(cts, PostId, ReplyListView, BeforeLoaded, AfterLoaded, ReplyListViewScrollForSpecifiedPost);
                }
                else if (ThreadId > 0)
                {
                    DataContext = new ReplyListViewForDefaultViewModel(cts, ThreadId, ReplyListView, BeforeLoaded, AfterLoaded, ReplyListViewScrollForSpecifiedPost);
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

        #endregion

        private void FaceButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FacePanel.Visibility = Visibility.Visible;
            FilePanel.Visibility = Visibility.Collapsed;
        }

        private void FileButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FacePanel.Visibility = Visibility.Collapsed;
            FilePanel.Visibility = Visibility.Visible;
        }

        private void HideReplyBottomPanel(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FacePanel.Visibility = Visibility.Collapsed;
            FilePanel.Visibility = Visibility.Collapsed;
        }

        private void QuickReplyPanel_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // 阻止 HideReplyBottomPanel 事件向下传递
            e.Handled = true;
        }

        private void ReplyListView_PullProgressChanged(object sender, Controls.RefreshProgressEventArgs e)
        {
            if (e.IsRefreshable)
            {
                if (e.PullProgress == 1)
                {
                    // Progress = 1.0 means that the refresh has been triggered.
                    if (SpinnerStoryboard.GetCurrentState() == Windows.UI.Xaml.Media.Animation.ClockState.Stopped)
                    {
                        SpinnerStoryboard.Begin();
                    }
                }
                else if (SpinnerStoryboard.GetCurrentState() != Windows.UI.Xaml.Media.Animation.ClockState.Stopped)
                {
                    SpinnerStoryboard.Stop();
                }
                else
                {
                    // Turn the indicator by an amount proportional to the pull progress.
                    SpinnerTransform.Angle = e.PullProgress * 360;
                }
            }
        }

        private void ReplyListView_RefreshRequested(object sender, Controls.RefreshRequestedEventArgs e)
        {
            if (PostId > 0)
            {
                var vm = DataContext as ReplyListViewForSpecifiedPostViewModel;
                vm.LoadPrevPageData();
            }
            else if (ThreadId > 0)
            {
                var vm = DataContext as ReplyListViewForDefaultViewModel;
                vm.LoadPrevPageData();
            }
        }
    }
}
