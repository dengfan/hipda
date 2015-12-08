using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hipda.Client.Uwp.Pro.Models;
using Windows.UI.Xaml.Data;
using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Services;
using Windows.UI.Xaml.Controls;
using System.Threading;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadItemForMyPostsViewModel : ThreadItemViewModelBase
    {
        private int _threadId { get; set; }
        private ListView _replyListView { get; set; }
        private Action _beforeLoad { get; set; }
        private Action<int, int> _afterLoad { get; set; }
        private DataService _ds { get; set; }

        public DelegateCommand RefreshReplyCommand { get; set; }

        public ThreadItemForMyPostsModel ThreadItem { get; set; }

        private ICollectionView _replyItemCollection;

        public ICollectionView ReplyItemCollection
        {
            get { return _replyItemCollection; }
            set
            {
                _replyItemCollection = value;
                this.RaisePropertyChanged("ReplyItemCollection");
            }
        }

        private void LoadData(int pageNo)
        {
            var cv = _ds.GetViewForReplyPage(pageNo, _threadId, AccountService.UserId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                StartPageNo = pageNo;
                ReplyItemCollection = cv;
            }
        }

        public ThreadItemForMyPostsViewModel(ThreadItemForMyPostsModel threadItem)
        {
            StartPageNo = 1;
            ThreadDataType = ThreadDataType.MyPosts;
            _threadId = threadItem.ThreadId;
            ThreadItem = threadItem;
        }

        public ThreadItemForMyPostsViewModel(int pageNo, int threadId, int threadAuthorUserId, ListView replyListView, Action beforeLoad, Action<int, int> afterLoad)
        {
            StartPageNo = 1;
            ThreadDataType = ThreadDataType.MyPosts;
            _threadId = threadId;
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            ThreadItem = _ds.GetThreadItemForMyPosts(threadId);

            RefreshReplyCommand = new DelegateCommand();
            RefreshReplyCommand.ExecuteAction = (p) =>
            {
                _ds.ClearReplyData(threadId);
                LoadData(StartPageNo);
            };

            var cv = _ds.GetViewForReplyPage(pageNo, threadId, threadAuthorUserId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ReplyItemCollection = cv;
            }
        }

        public async Task SelectThreadItem(ListView replyListView, Action beforeLoad, Action<int, int> afterLoad, Action<int> listViewScroll)
        {
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            RefreshReplyCommand = new DelegateCommand();
            RefreshReplyCommand.ExecuteAction = (p) =>
            {
                _ds.ClearReplyData(_threadId);
                LoadData(StartPageNo);
            };

            // 先载入第一个转跳到的页面的数据，并得到页码之后即可进入正常流程
            var cts = new CancellationTokenSource();
            int[] data = await _ds.LoadReplyDataForRedirectPageAsync(_threadId, ThreadItem.PostId, cts);
            int pageNo = data[0];
            int index = data[1];
            _ds.SetScrollState(false);
            var cv = _ds.GetViewForReplyPage(pageNo, _threadId, 0, index, _beforeLoad, _afterLoad, listViewScroll);
            if (cv != null)
            {
                ReplyItemCollection = cv;
                StartPageNo = pageNo;
            }
        }

        public void SetRead()
        {
            StatusColorStyle = (Style)App.Current.Resources["ReadColorStyle"];
        }

        public void RefreshReplyDataFromPrevPage()
        {
            if (StartPageNo > 1)
            {
                _ds.ClearReplyData(_threadId);
                LoadData(StartPageNo - 1);
            }
        }

        public bool GetScrollState()
        {
            return _ds.GetScrollState();
        }

        public void SetScrollState(bool isCompleted)
        {
            _ds.SetScrollState(isCompleted);
        }
    }
}
