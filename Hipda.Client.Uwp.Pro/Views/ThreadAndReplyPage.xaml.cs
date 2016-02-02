using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using System.Linq;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
            DependencyProperty.Register("ThreadId", typeof(int), typeof(ThreadAndReplyPage), new PropertyMetadata(0, new PropertyChangedCallback(OnThreadIdChanged)));

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

        public ThreadAndReplyPage()
        {
            this.InitializeComponent();
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
                RightWrap.DataContext = new ReplyListViewForDefaultViewModel(cts, ThreadId, ReplyListView, RightBeforeLoaded, RightAfterLoaded, ReplyListViewScrollForSpecifiedPost);
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

            leftNoDataNoticePanel.Visibility = Visibility.Collapsed;

            if (e.Parameter != null)
            {
                string param = e.Parameter.ToString();
                if (param.StartsWith("fid=")) // 表示要加载指定的贴子列表页
                {
                    int fid = Convert.ToInt32(param.Substring("fid=".Length));
                    LeftWrap.DataContext = new ThreadListViewForDefaultViewModel(1, fid, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                }
                else if (param.StartsWith("item="))
                {
                    string threadType = param.Substring("item=".Length);
                    switch (threadType)
                    {
                        case "threads":
                            LeftWrap.DataContext = new ThreadListViewForMyThreadsViewModel(1, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                            break;
                        case "posts":
                            LeftWrap.DataContext = new ThreadListViewForMyPostsViewModel(1, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                            break;
                        case "favorites":
                            LeftWrap.DataContext = new ThreadListViewForMyFavoritesViewModel(1, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                            break;
                        case "notice":
                            LeftWrap.DataContext = new ThreadListViewForNoticeViewModel(LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
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
                        LeftWrap.DataContext = new ThreadListViewForSearchTitleViewModel(1, searchKeyowrd, searchAuthor, searchType, searchTimeSpan, searchForumSpan, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
                    }
                    else
                    {
                        LeftWrap.DataContext = new ThreadListViewForSearchFullTextViewModel(1, searchKeyowrd, searchAuthor, searchType, searchTimeSpan, searchForumSpan, LeftListView, LeftCommandBar, LeftBeforeLoaded, LeftAfterLoaded, LeftNoDataNotice);
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
            if (vmType.Equals(typeof(ThreadListViewForDefaultViewModel)))
            {
                var vm = LeftWrap.DataContext as ThreadListViewForDefaultViewModel;
                vm.LoadPrevPageData();
            }
            else if (vmType.Equals(typeof(ThreadListViewForMyThreadsViewModel)))
            {
                var vm = LeftWrap.DataContext as ThreadListViewForMyThreadsViewModel;
                vm.LoadPrevPageData();
            }
            else if (vmType.Equals(typeof(ThreadListViewForMyPostsViewModel)))
            {
                var vm = LeftWrap.DataContext as ThreadListViewForMyPostsViewModel;
                vm.LoadPrevPageData();
            }
            else if (vmType.Equals(typeof(ThreadListViewForMyFavoritesViewModel)))
            {
                var vm = LeftWrap.DataContext as ThreadListViewForMyFavoritesViewModel;
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
    }
}
