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
    /// 回复列表之视图模型
    /// 根据 thread id 加载
    /// </summary>
    public class ReplyListViewViewModel : NotificationObject
    {
        int _startPageNo = 1;
        int _threadId;
        ListView _replyListView;
        Action _beforeLoad;
        Action<int, int> _afterLoad;
        DataService _ds;

        public DelegateCommand RefreshReplyCommand { get; set; }

        public DelegateCommand LoadPrevPageDataCommand { get; set; }


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

        public ReplyListViewViewModel(CancellationTokenSource cts, int threadId, int threadAuthorUserId, ListView replyListView, Action beforeLoad, Action<int, int> afterLoad)
        {
            _threadId = threadId;
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

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

            LoadData(_startPageNo);
        }

        void LoadData(int pageNo)
        {
            var cv = _ds.GetViewForReplyPageByThreadId(pageNo, _threadId, 0, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                _startPageNo = pageNo;
                ReplyItemCollection = cv;
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

        public void RefreshReplyDataFromPrevPage()
        {
            if (_startPageNo > 1)
            {
                _ds.ClearReplyData(_threadId);
                LoadData(_startPageNo - 1);
            }
        }
    }
}
