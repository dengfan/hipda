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
    /// <summary>
    /// 查看指定的POST之回复列表之视图模型
    /// 根据 post id 及 thread id 加载
    /// </summary>
    public class ReplyListViewForSpecifiedPostViewModel : NotificationObject
    {
        int _startPageNo = 1;
        int _postId;
        int _threadId;
        ListView _replyListView;
        Action _beforeLoad;
        Action<int, int> _afterLoad;
        Action<int> _listViewScroll;
        DataServiceForReply _ds;

        public DelegateCommand RefreshReplyCommand { get; set; }

        public DelegateCommand LoadPrevPageDataCommand { get; set; }

        public DelegateCommand LoadLastPageDataCommand { get; set; }


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

        public ReplyListViewForSpecifiedPostViewModel(CancellationTokenSource cts, int postId, ListView replyListView, Action beforeLoad, Action<int, int> afterLoad, Action<int> listViewScroll)
        {
            _postId = postId;
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _listViewScroll = listViewScroll;
            _ds = new DataServiceForReply();

            RefreshReplyCommand = new DelegateCommand();
            RefreshReplyCommand.ExecuteAction = (p) =>
            {
                _ds.ClearReplyData(_threadId);
                LoadData(_startPageNo);
            };

            LoadPrevPageDataCommand = new DelegateCommand();
            LoadPrevPageDataCommand.ExecuteAction = (p) =>
            {
                if (_startPageNo > 1)
                {
                    _ds.ClearReplyData(_threadId);
                    LoadData(_startPageNo - 1);
                }
            };

            LoadLastPageDataCommand = new DelegateCommand();
            LoadLastPageDataCommand.ExecuteAction = (p) =>
            {
                _ds.ClearReplyData(_threadId);
                LoadData(_ds.GetReplyMaxPageNo());
            };

            FirstLoad(cts);
        }

        async void FirstLoad(CancellationTokenSource cts)
        {
            // 先载入第一个转跳到的页面的数据，并得到页码之后即可进入正常流程
            int[] data = await _ds.LoadReplyDataForRedirectReplyPageAsync(_postId, cts);
            if (data != null)
            {
                int pageNo = data[0];
                int index = data[1];
                _threadId = data[2];
                _ds.SetScrollState(false);
                var cv = _ds.GetViewForRedirectReplyPageByThreadId(pageNo, _threadId, index, _beforeLoad, _afterLoad, _listViewScroll, null);
                if (cv != null)
                {
                    ReplyItemCollection = cv;
                    _startPageNo = pageNo;
                }
            }
        }

        void LoadData(int pageNo)
        {
            var cv = _ds.GetViewForReplyPageByThreadId(pageNo, _threadId, _beforeLoad, _afterLoad, null);
            if (cv != null)
            {
                _startPageNo = pageNo;
                ReplyItemCollection = cv;
            }
        }

        public void LoadPrevPageData()
        {
            if (_startPageNo > 1)
            {
                _ds.ClearReplyData(_threadId);
                LoadData(_startPageNo - 1);
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
