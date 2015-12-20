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
    public class ThreadItemForSearchTitleViewModel : ThreadItemViewModelBase
    {
        private int _threadId { get; set; }
        private ListView _replyListView { get; set; }
        private Action _beforeLoad { get; set; }
        private Action<int, int> _afterLoad { get; set; }
        private DataService _ds { get; set; }

        public DelegateCommand RefreshReplyCommand { get; set; }
        public DelegateCommand PostReplyCommand { get; set; }

        public ThreadItemForSearchTitleModel ThreadItem { get; set; }

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
            var cv = _ds.GetViewForReplyPageByThreadId(pageNo, _threadId, ThreadItem.AuthorUserId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                StartPageNo = pageNo;
                ReplyItemCollection = cv;
            }
        }

        private void PostData(string postContent, int threadId)
        {

        }

        public ThreadItemForSearchTitleViewModel(ThreadItemForSearchTitleModel threadItem)
        {
            StartPageNo = 1;
            ThreadDataType = ThreadDataType.SearchTitle;
            _threadId = threadItem.ThreadId;
            ThreadItem = threadItem;
        }

        public ThreadItemForSearchTitleViewModel(int pageNo, int threadId, int threadAuthorUserId, ListView replyListView, TextBox postReplyTextBox, Action beforeLoad, Action<int, int> afterLoad)
        {
            StartPageNo = 1;
            ThreadDataType = ThreadDataType.SearchTitle;
            _threadId = threadId;
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            ThreadItem = _ds.GetThreadItemForSearchTitle(threadId);

            RefreshReplyCommand = new DelegateCommand();
            RefreshReplyCommand.ExecuteAction = (p) =>
            {
                _ds.ClearReplyData(threadId);
                LoadData(1);
            };

            PostReplyCommand = new DelegateCommand();
            PostReplyCommand.ExecuteAction = (p) => {
                string postContent = postReplyTextBox.Text.Trim();
                PostData(postContent, threadId);
            };

            var cv = _ds.GetViewForReplyPageByThreadId(pageNo, threadId, threadAuthorUserId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                ReplyItemCollection = cv;
            }
        }

        public void SelectThreadItem(ListView replyListView, TextBox postReplyTextBox, Action beforeLoad, Action<int, int> afterLoad)
        {
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            RefreshReplyCommand = new DelegateCommand();
            RefreshReplyCommand.ExecuteAction = (p) => {
                _ds.ClearReplyData(_threadId);
                LoadData(StartPageNo);
            };

            PostReplyCommand = new DelegateCommand();
            PostReplyCommand.ExecuteAction = (p) => {
                string postContent = postReplyTextBox.Text.Trim();
                PostData(postContent, _threadId);
            };

            LoadData(StartPageNo);
        }

        public void SetRead()
        {
            StatusColorStyle = (Style)App.Current.Resources["ReadColorStyle"];
        }

        public void RefreshReplyDataFromPrevPage()
        {
            if (StartPageNo > 1)
            {
                _ds.ClearReplyData(_threadId);
                LoadData(StartPageNo - 1);
            }
        }
    }
}
