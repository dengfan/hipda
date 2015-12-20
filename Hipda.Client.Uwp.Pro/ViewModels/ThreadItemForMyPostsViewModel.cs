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
        int _threadId;
        ListView _replyListView;
        Action _beforeLoad;
        Action<int, int> _afterLoad;
        DataService _ds;

        public DelegateCommand RefreshReplyCommand { get; set; }

        public ThreadItemForMyPostsModel ThreadItem { get; set; }

        ICollectionView _replyItemCollection;

        public ICollectionView ReplyItemCollection
        {
            get { return _replyItemCollection; }
            set
            {
                _replyItemCollection = value;
                this.RaisePropertyChanged("ReplyItemCollection");
            }
        }

        void LoadData(int pageNo)
        {
            var cv = _ds.GetViewForReplyPageByThreadId(pageNo, _threadId, AccountService.UserId, _beforeLoad, _afterLoad);
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

            var cv = _ds.GetViewForReplyPageByThreadId(pageNo, threadId, threadAuthorUserId, _beforeLoad, _afterLoad);
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
            int[] data = await _ds.LoadReplyDataForRedirectReplyPageAsync(ThreadItem.PostId, cts);
            if (data != null)
            {
                int pageNo = data[0];
                int index = data[1];
                _threadId = data[2];
                _ds.SetScrollState(false);
                var cv = _ds.GetViewForRedirectReplyPageByThreadId(pageNo, _threadId, 0, index, _beforeLoad, _afterLoad, listViewScroll);
                if (cv != null)
                {
                    ReplyItemCollection = cv;
                    StartPageNo = pageNo;
                }
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
