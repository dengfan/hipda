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

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadItemViewModel : NotificationObject
    {
        private ListView _replyListView { get; set; }
        private Action _beforeLoad { get; set; }
        private Action _afterLoad { get; set; }
        private DataService _ds { get; set; }

        private Brush _statusColor;

        public Brush StatusColor
        {
            get { return _statusColor; }
            set
            {
                _statusColor = value;
                this.RaisePropertyChanged("StatusColor");
            }
        }

        public DelegateCommand RefreshReplyCommand { get; set; }

        public ThreadItemModel ThreadItem { get; set; }

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
            var cv = _ds.GetViewForReplyPage(pageNo, ThreadItem.ThreadId, ThreadItem.AuthorUserId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ReplyItemCollection = cv;
            }
        }

        public ThreadItemViewModel(ThreadItemModel threadItem)
        {
            ThreadItem = threadItem;
        }

        public ThreadItemViewModel(int pageNo, int threadId, int threadAuthorUserId, ListView replyListView, Action beforeLoad, Action afterLoad)
        {
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            ThreadItem = _ds.GetThreadItem(threadId);

            RefreshReplyCommand = new DelegateCommand();
            RefreshReplyCommand.ExecuteAction = new Action<object>(RefreshReplyExecute);

            var cv = _ds.GetViewForReplyPage(pageNo, threadId, threadAuthorUserId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ReplyItemCollection = cv;
            }
        }

        public void SelectThreadItem(ListView replyListView, Action beforeLoad, Action afterLoad)
        {
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            RefreshReplyCommand = new DelegateCommand();
            RefreshReplyCommand.ExecuteAction = new Action<object>(RefreshReplyExecute);

            LoadData(1);
        }

        public void SetRead()
        {
            _ds = new DataService();
            _ds.SetRead(ThreadItem.ThreadId);
            StatusColor = new SolidColorBrush(Colors.White);
        }

        private void RefreshReplyExecute(object parameter)
        {
            _replyListView.ItemsSource = null;
            _ds.ClearReplyData(ThreadItem.ThreadId);

            LoadData(1);
            _replyListView.ItemsSource = ReplyItemCollection;
        }

        public void RefreshReplyDataFromPrevPage()
        {
            // 先获取当前数据中已存在的最小页码
            int minPageNo = _ds.GetReplyMinPageNoInLoadedData(ThreadItem.ThreadId);
            int startPageNo = minPageNo > 1 ? minPageNo - 1 : 1;

            _replyListView.ItemsSource = null;
            _ds.ClearReplyData(ThreadItem.ThreadId);

            LoadData(startPageNo);
            _replyListView.ItemsSource = ReplyItemCollection;
        }
    }
}
