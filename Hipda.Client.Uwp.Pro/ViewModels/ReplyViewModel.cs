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
        private ListView _replyListView { get; set; }
        private Action _beforeLoad { get; set; }
        private Action _afterLoad { get; set; }
        private DataService _ds { get; set; }

        public DelegateCommand RefreshReplyCommand { get; set; }

        public ICollectionView ReplyItemCollection { get; set; }

        public ReplyViewModel(int threadId, int threadAuthorUserId, ListView replyListView, Action beforeLoad, Action afterLoad)
        {
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            RefreshReplyCommand = new DelegateCommand();
            RefreshReplyCommand.ExecuteAction = new Action<object>(RefreshReplyExecute);

            var cv = _ds.GetViewForReplyPage(threadId, threadAuthorUserId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ReplyItemCollection = cv;
            }
        }

        private async void RefreshReplyExecute(object parameter)
        {
            _replyListView.ItemsSource = null;
            await _ds.RefreshThreadData(14, new CancellationTokenSource());
            _replyListView.ItemsSource = ReplyItemCollection;
        }
    }
}
