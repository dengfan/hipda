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
        int _threadId = 0; // 有的主题是使用 ThreadId 参数加载回复列表页
        int _postId = 0; // 有的主题是使用 PostId 参数加载回复列表页

        public ThreadAndReplyPage()
        {
            this.InitializeComponent();
            this.SizeChanged += ThreadAndReplyPage_SizeChanged;
        }

        private void ThreadAndReplyPage_SizeChanged(object sender, SizeChangedEventArgs e)
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
        }

        #region 委托事件
        private void LeftBeforeLoaded()
        {
            leftProgress.IsActive = true;
            leftProgress.Visibility = Visibility.Visible;
            ReplyRefreshButton.IsEnabled = false;
        }

        private void LeftAfterLoaded()
        {
            leftProgress.IsActive = false;
            leftProgress.Visibility = Visibility.Collapsed;
            ReplyRefreshButton.IsEnabled = true;
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
            ReplyRefreshButton.IsEnabled = false;
        }

        private void RightAfterLoaded(int threadId, int pageNo)
        {
            rightProgress.IsActive = false;
            rightProgress.Visibility = Visibility.Collapsed;
            ReplyRefreshButton.IsEnabled = true;

            bool isShown = DataServiceForReply.CanShowButtonForLoadPrevReplyPage(threadId);
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
        /// <summary>
        /// 通过 ThreadId 打开回复列表页，打开后从1楼开始显示
        /// </summary>
        /// <param name="threadId">主题ID</param>
        public void OpenReplyPageByThreadId(int threadId)
        {
            _threadId = threadId;
            var cts = new CancellationTokenSource();
            RightWrap.DataContext = new ReplyListViewForDefaultViewModel(cts, _threadId, ReplyListView, RightBeforeLoaded, RightAfterLoaded);
        }

        /// <summary>
        /// 通过 PostId 打开回复列表页，并且打开后跳到相应的回复楼层
        /// </summary>
        /// <param name="postId">回复ID</param>
        public void OpenReplyPageByPostId(int postId)
        {
            _postId = postId;
            var cts = new CancellationTokenSource();
            RightWrap.DataContext = new ReplyListViewForSpecifiedPostViewModel(cts, _postId, ReplyListView, RightBeforeLoaded, RightAfterLoaded, ReplyListViewScrollForSpecifiedPost);
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
                    int threadId = Convert.ToInt32(param.Substring("tid=".Length));
                    OpenReplyPageByThreadId(threadId);
                }
                else if (param.Contains("pid=")) // 表示要加载指定的回复列表页，从窄视图变宽后导航而来
                {
                    int postId = Convert.ToInt32(param.Substring("pid=".Length));
                    OpenReplyPageByPostId(postId);
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
                if (_postId > 0)
                {
                    Frame.Navigate(typeof(ReplyListPage), $"pid={_postId}", new SuppressNavigationTransitionInfo());
                    return;
                }

                if (_threadId > 0)
                {
                    Frame.Navigate(typeof(ReplyListPage), $"tid={_threadId}", new SuppressNavigationTransitionInfo());
                    return;
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
            _threadId = selectedItem.ThreadId;
            _postId = selectedItem.PostId;

            if (_postId > 0)
            {
                if (AdaptiveStates.CurrentState == NarrowState)
                {
                    string p = $"pid={_postId}";
                    Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                }
                else
                {
                    OpenReplyPageByPostId(_postId);
                }

                return;
            }

            if (_threadId > 0)
            {
                if (AdaptiveStates.CurrentState == NarrowState)
                {
                    string p = $"tid={_threadId}";
                    Frame.Navigate(typeof(ReplyListPage), p, new CommonNavigationTransitionInfo());
                }
                else
                {
                    OpenReplyPageByThreadId(_threadId);
                }

                return;
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
            PostReplyTextBox.Text = data.TextStr;
        }
    }
}
