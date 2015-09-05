using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadAndReplyViewModel : NotificationObject
    {
        private ListView _threadListView { get; set; }
        private ListView _replyListView { get; set; }
        private Action _beforeLoad { get; set; }
        private Action _afterLoad { get; set; }
        private DataService _ds { get; set; }

        public DelegateCommand RefreshCommand { get; set; }

        public ICollectionView ThreadItemCollection { get; set; }

        public ThreadAndReplyViewModel(ListView threadListView, ListView replyListView, Action beforeLoad, Action afterLoad)
        {
            _threadListView = threadListView;
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            RefreshCommand = new DelegateCommand();
            RefreshCommand.ExecuteAction = new Action<object>(RefreshExecute);

            ThreadItemCollection = _ds.GetViewForThreadPage(14, _beforeLoad, _afterLoad);
        }

        private async void RefreshExecute(object parameter)
        {
            _threadListView.ItemsSource = null;
            await _ds.RefreshThreadData(14, new CancellationTokenSource());
            _threadListView.ItemsSource = ThreadItemCollection;
        }
    }
}
