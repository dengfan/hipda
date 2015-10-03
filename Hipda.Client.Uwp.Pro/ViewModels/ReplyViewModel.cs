using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ReplyViewModel : NotificationObject
    {
        private int _threadId;
        private int _threadAuthorUserId;
        private ListView _replyListView { get; set; }
        private Action _beforeLoad { get; set; }
        private Action _afterLoad { get; set; }
        private DataService _ds { get; set; }

        public DelegateCommand RefreshReplyCommand { get; set; }

        public ICollectionView ReplyItemCollection { get; set; }

        private void LoadData(int pageNo)
        {
            var cv = _ds.GetViewForReplyPage(pageNo, _threadId, _threadAuthorUserId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ReplyItemCollection = cv;
            }
        }

        public ReplyViewModel(int pageNo, int threadId, int threadAuthorUserId, ListView replyListView, Action beforeLoad, Action afterLoad)
        {
            _threadId = threadId;
            _threadAuthorUserId = threadAuthorUserId;
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            RefreshReplyCommand = new DelegateCommand();
            RefreshReplyCommand.ExecuteAction = new Action<object>(RefreshReplyExecute);

            LoadData(pageNo);
        }

        private void RefreshReplyExecute(object parameter)
        {
            _replyListView.ItemsSource = null;
            _ds.ClearReplyData(_threadId);

            LoadData(1);
            _replyListView.ItemsSource = ReplyItemCollection;
        }

        public void RefreshReplyDataFromPrevPage()
        {
            // 先获取当前数据中已存在的最小页码
            int minPageNo = _ds.GetReplyMinPageNoInLoadedData(_threadId);
            int startPageNo = minPageNo > 1 ? minPageNo - 1 : 1;

            _replyListView.ItemsSource = null;
            _ds.ClearReplyData(_threadId);

            LoadData(1);
            _replyListView.ItemsSource = ReplyItemCollection;
        }
    }
}
