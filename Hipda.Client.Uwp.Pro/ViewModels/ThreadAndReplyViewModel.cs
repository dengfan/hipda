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
        private int _forumId { get; set; }
        private ListView _threadListView { get; set; }
        private Action _beforeLoad { get; set; }
        private Action _afterLoad { get; set; }
        private DataService _ds { get; set; }

        public DelegateCommand RefreshThreadCommand { get; set; }

        private ICollectionView _threadItemCollection;

        public ICollectionView ThreadItemCollection
        {
            get { return _threadItemCollection; }
            set
            {
                _threadItemCollection = value;
                this.RaisePropertyChanged("ThreadItemCollection");
            }
        }

        private void LoadData(int pageNo)
        {
            var cv = _ds.GetViewForThreadPage(pageNo, _forumId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ThreadItemCollection = cv;
            }
        }

        private void LoadDataForMyThreads(int pageNo)
        {
            var cv = _ds.GetViewForThreadPageForMyThreads(pageNo, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ThreadItemCollection = cv;
            }
        }

        public ThreadAndReplyViewModel(int pageNo, int forumId, ListView threadListView, Action beforeLoad, Action afterLoad)
        {
            _forumId = forumId;
            _threadListView = threadListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            RefreshThreadCommand = new DelegateCommand();
            RefreshThreadCommand.ExecuteAction = (p) => {
                _threadListView.ItemsSource = null;
                _ds.ClearThreadData(_forumId);

                LoadData(1);
                _threadListView.ItemsSource = ThreadItemCollection;
            };

            LoadData(pageNo);
        }

        public ThreadAndReplyViewModel(int pageNo, string itemType, ListView threadListView, Action beforeLoad, Action afterLoad)
        {
            _threadListView = threadListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            if (itemType.Equals("threads"))
            {
                RefreshThreadCommand = new DelegateCommand();
                RefreshThreadCommand.ExecuteAction = (p) => {

                };

                LoadDataForMyThreads(pageNo);
            }
            
        }

        public void RefreshThreadDataFromPrevPage()
        {
            // 先获取当前数据中已存在的最小页码
            int minPageNo = _ds.GetThreadMinPageNoInLoadedData();
            int startPageNo = minPageNo > 1 ? minPageNo - 1 : 1;

            _threadListView.ItemsSource = null;
            _ds.ClearThreadData(_forumId);

            LoadData(startPageNo);
            _threadListView.ItemsSource = ThreadItemCollection;
        }
    }
}
