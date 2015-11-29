﻿using System;
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
    public class ThreadItemForSearchViewModel : ThreadItemViewModelBase
    {
        private ListView _replyListView { get; set; }
        private Action _beforeLoad { get; set; }
        private Action<int> _afterLoad { get; set; }
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
            var cv = _ds.GetViewForReplyPage(pageNo, ThreadItem.ThreadId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ReplyItemCollection = cv;
            }
        }

        public ThreadItemForSearchViewModel(ThreadItemModel threadItem)
        {
            ThreadDataType = ThreadDataType.Default;
            ThreadItem = threadItem;
        }

        public ThreadItemForSearchViewModel(int pageNo, int threadId, ListView replyListView, Action beforeLoad, Action<int> afterLoad)
        {
            ThreadDataType = ThreadDataType.Default;
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            ThreadItem = _ds.GetThreadItem(threadId);

            RefreshReplyCommand = new DelegateCommand();
            RefreshReplyCommand.ExecuteAction = (p) =>
            {
                _ds.ClearReplyData(threadId);
                LoadData(1);
            };

            var cv = _ds.GetViewForReplyPage(pageNo, threadId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ReplyItemCollection = cv;
            }
        }

        public void SelectThreadItem(ListView replyListView, Action beforeLoad, Action<int> afterLoad)
        {
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            RefreshReplyCommand = new DelegateCommand();
            RefreshReplyCommand.ExecuteAction = (p) => {
                _ds.ClearReplyData(ThreadItem.ThreadId);
                LoadData(1);
            };

            LoadData(1);
        }

        public void SetRead()
        {
            StatusColor = (SolidColorBrush)App.Current.Resources["StatusColor2"];
        }

        public void RefreshReplyDataFromPrevPage()
        {
            // 先获取当前数据中已存在的最小页码
            int minPageNo = _ds.GetReplyMinPageNoInLoadedData(ThreadItem.ThreadId);
            int startPageNo = minPageNo > 1 ? minPageNo - 1 : 1;

            _ds.ClearReplyData(ThreadItem.ThreadId);
            LoadData(startPageNo);
        }
    }
}
