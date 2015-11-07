﻿using Hipda.Client.Uwp.Pro.Commands;
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
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

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

        public DelegateCommand ClearHistoryCommand { get; set; }

        public int ThreadMaxPageNo { get; private set; }

        public List<FaceItem> FaceData
        {
            get
            {
                return FaceService.FaceData;
            }
        }

        public ObservableCollection<ThreadItemModelBase> ReadData
        {
            get
            {
                return DataService.ReadHistoryData;
            }
        }

        #region 用于主题列表控件增量加载
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
        #endregion

        private void LoadData(int pageNo)
        {
            var cv = _ds.GetViewForThreadPage(pageNo, _forumId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ThreadItemCollection = cv;
            }

            ThreadMaxPageNo = _ds.GetThreadMaxPageNo();
        }

        private void LoadDataForMyThreads(int pageNo)
        {
            var cv = _ds.GetViewForThreadPageForMyThreads(pageNo, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNo();
                ThreadItemCollection = cv;
            }
        }

        private void LoadDataForMyPosts(int pageNo)
        {
            var cv = _ds.GetViewForThreadPageForMyPosts(pageNo, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNo();
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
                _ds.ClearThreadData(_forumId);
                LoadData(1);
            };

            ClearHistoryCommand = new DelegateCommand();
            ClearHistoryCommand.ExecuteAction = (p) => {
                DataService.ReadHistoryData.Clear();
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
                    _ds.ClearThreadDataForMyThreads();
                    LoadDataForMyThreads(1);
                };

                LoadDataForMyThreads(pageNo);
            }
            else if (itemType.Equals("posts"))
            {
                RefreshThreadCommand = new DelegateCommand();
                RefreshThreadCommand.ExecuteAction = (p) => {
                    _ds.ClearThreadDataForMyPosts();
                    LoadDataForMyPosts(1);
                };

                LoadDataForMyPosts(pageNo);
            }
        }

        #region 从上一页开始加载
        public void RefreshThreadDataFromPrevPage()
        {
            // 先获取当前数据中已存在的最小页码
            int minPageNo = _ds.GetThreadMinPageNoInLoadedData();
            int startPageNo = minPageNo > 1 ? minPageNo - 1 : 1;

            _ds.ClearThreadData(_forumId);
            LoadData(startPageNo);
        }

        public void RefreshThreadDataForMyThreadsFromPrevPage()
        {
            // 先获取当前数据中已存在的最小页码
            int minPageNo = _ds.GetThreadMinPageNoForMyThreadsInLoadedData();
            int startPageNo = minPageNo > 1 ? minPageNo - 1 : 1;

            _ds.ClearThreadDataForMyThreads();
            LoadDataForMyThreads(startPageNo);
        }

        public void RefreshThreadDataForMyPostsFromPrevPage()
        {
            // 先获取当前数据中已存在的最小页码
            int minPageNo = _ds.GetThreadMinPageNoForMyPostsInLoadedData();
            int startPageNo = minPageNo > 1 ? minPageNo - 1 : 1;

            _ds.ClearThreadDataForMyPosts();
            LoadDataForMyPosts(startPageNo);
        }
        #endregion

        public ThreadItemModel GetThreadItem(int threadId)
        {
            return _ds.GetThreadItem(threadId);
        }

        private string GetThreadTitle(int threadId)
        {
            string title = _ds.GetThreadTitleFromReplyData(threadId);
            if (string.IsNullOrEmpty(title))
            {
                title = _ds.GetThreadTitleFromThreadData(threadId);
            }

            return title;
        }

        public void AddToReadHistory(int threadId)
        {
            var ti = DataService.ReadHistoryData.FirstOrDefault(t => t.ThreadId == threadId);
            if (ti != null)
            {
                DataService.ReadHistoryData.Remove(ti);
            }

            string threadTitle = GetThreadTitle(threadId);
            if (string.IsNullOrEmpty(threadTitle))
            {
                return;
            }

            DataService.ReadHistoryData.Add(new ThreadItemModelBase
            {
                Title = threadTitle,
                ThreadId = threadId
            });

            ApplicationView.GetForCurrentView().Title = threadTitle;
        }
    }
}
