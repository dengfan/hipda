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

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadItemViewModel : NotificationObject
    {
        private ListView _replyListView { get; set; }
        private Action _beforeLoad { get; set; }
        private Action _afterLoad { get; set; }
        private DataService _ds { get; set; }

        public DelegateCommand RefreshRelplyCommand { get; set; }

        public ThreadItemModel ThreadItem { get; set; }

        public ICollectionView ReplyItemCollection { get; set; }

        private void LoadData()
        {
            var cv = _ds.GetViewForReplyPage(ThreadItem.ThreadId, ThreadItem.AuthorUserId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ReplyItemCollection = cv;
            }
        }

        public ThreadItemViewModel(ThreadItemModel threadItem)
        {
            ThreadItem = threadItem;
        }

        public ThreadItemViewModel(int threadId, int threadAuthorUserId, ListView replyListView, Action beforeLoad, Action afterLoad)
        {
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            ThreadItem = _ds.GetThreadItem(threadId);

            RefreshRelplyCommand = new DelegateCommand();
            RefreshRelplyCommand.ExecuteAction = new Action<object>(RefreshReplyExecute);

            var cv = _ds.GetViewForReplyPage(threadId, threadAuthorUserId, _beforeLoad, _afterLoad);
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

            RefreshRelplyCommand = new DelegateCommand();
            RefreshRelplyCommand.ExecuteAction = new Action<object>(RefreshReplyExecute);

            LoadData();
        }

        private void RefreshReplyExecute(object parameter)
        {
            _replyListView.ItemsSource = null;
            _ds.ClearReplyData(ThreadItem.ThreadId);

            LoadData();
            _replyListView.ItemsSource = ReplyItemCollection;
        }
    }
}
