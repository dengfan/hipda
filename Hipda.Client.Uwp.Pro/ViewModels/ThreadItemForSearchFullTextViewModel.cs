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
        int _threadId;
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
            var cv = _ds.GetViewForReplyPageByThreadId(pageNo, _threadId, ThreadItem.AuthorUserId, _beforeLoad, _afterLoad);
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
            ThreadDataType = ThreadDataType.SearchFullText;
            ThreadItem = threadItem;
        }

        public ThreadItemForSearchFullTextViewModel(int pageNo, int threadId, int threadAuthorUserId, ListView replyListView, TextBox postReplyTextBox, Action beforeLoad, Action<int, int> afterLoad)
        {
            ThreadDataType = ThreadDataType.SearchFullText;
            _threadId = threadId;
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

        public async Task SelectThreadItem(ListView replyListView, TextBox postReplyTextBox, Action beforeLoad, Action<int, int> afterLoad, Action<int> listViewScroll)
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

            // 先载入第一个转跳到的页面的数据，并得到页码之后即可进入正常流程
            var cts = new CancellationTokenSource();
            int[] data = await _ds.LoadReplyDataForRedirectReplyPageAsync(ThreadItem.PostId, cts);
            if (data != null)
            {
                int pageNo = data[0];
                int index = data[1];
                _threadId = data[2];
                _ds.SetScrollState(false);
                var cv = _ds.GetViewForRedirectReplyPageByThreadId(pageNo, _threadId, 0, index, _beforeLoad, _afterLoad, listViewScroll);
                if (cv != null)
                {
                    ReplyItemCollection = cv;
                    StartPageNo = pageNo;
                }
            }
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

        public bool GetScrollState()
        {
            return _ds.GetScrollState();
        }

        public void SetScrollState(bool isCompleted)
        {
            _ds.SetScrollState(isCompleted);
        }
    }
}
