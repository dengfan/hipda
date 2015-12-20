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
    /// 全文搜索模块之视图模型
    /// 本模块是先只是提供 post id，待加载了数据之后才会获取到 thread id
    /// </summary>
    public class ThreadItemForSearchFullTextViewModel : ThreadItemViewModelBase
    {
        int _postId;
        ListView _replyListView;
        Action _beforeLoad;
        Action<int, int> _afterLoad;
        DataService _ds;

        public DelegateCommand RefreshReplyCommand { get; set; }
        public DelegateCommand PostReplyCommand { get; set; }

        public ThreadItemForSearchFullTextModel ThreadItem { get; set; }

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

        void LoadData(int pageNo)
        {
            var cv = _ds.GetViewForReplyPageByThreadId(pageNo, _postId, ThreadItem.AuthorUserId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                StartPageNo = pageNo;
                ReplyItemCollection = cv;
            }
        }

        void PostData(string postContent, int threadId)
        {

        }

        public ThreadItemForSearchFullTextViewModel(ThreadItemForSearchFullTextModel threadItem)
        {
            StartPageNo = 1;
            ThreadDataType = ThreadDataType.SearchFullText;
            _postId = threadItem.ThreadId;
            ThreadItem = threadItem;
        }

        public ThreadItemForSearchFullTextViewModel(int pageNo, int threadId, int threadAuthorUserId, ListView replyListView, TextBox postReplyTextBox, Action beforeLoad, Action<int, int> afterLoad)
        {
            StartPageNo = 1;
            ThreadDataType = ThreadDataType.SearchFullText;
            _postId = threadId;
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            ThreadItem = _ds.GetThreadItemForSearchFullText(threadId);

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
                _ds.ClearReplyData(_postId);
                LoadData(StartPageNo);
            };

            PostReplyCommand = new DelegateCommand();
            PostReplyCommand.ExecuteAction = (p) => {
                string postContent = postReplyTextBox.Text.Trim();
                PostData(postContent, _postId);
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
                _ds.ClearReplyData(_postId);
                LoadData(StartPageNo - 1);
            }
        }
    }
}
