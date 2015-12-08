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
using Windows.UI;
using Windows.UI.Xaml;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadItemForMyFavoritesViewModel : ThreadItemViewModelBase
    {
        private int _threadId { get; set; }
        private ListView _replyListView { get; set; }
        private TextBox _postReplyTextBox { get; set; }
        private Action _beforeLoad { get; set; }
        private Action<int, int> _afterLoad { get; set; }
        private DataService _ds { get; set; }

        public DelegateCommand RefreshReplyCommand { get; set; }
        public DelegateCommand PostReplyCommand { get; set; }

        public ThreadItemForMyFavoritesModel ThreadItem { get; set; }

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

        public List<FaceItemModel> FaceData
        {
            get
            {
                return FaceService.FaceData;
            }
        }

        private void LoadData(int pageNo)
        {
            var cv = _ds.GetViewForReplyPage(pageNo, _threadId, 0, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                StartPageNo = pageNo;
                ReplyItemCollection = cv;
            }
        }

        private void PostData(string postContent, int threadId)
        {

        }

        public ThreadItemForMyFavoritesViewModel(ThreadItemForMyFavoritesModel threadItem)
        {
            StartPageNo = 1;
            ThreadDataType = ThreadDataType.MyFavorites;
            _threadId = threadItem.ThreadId;
            ThreadItem = threadItem;
        }

        public ThreadItemForMyFavoritesViewModel(int pageNo, int threadId, ListView replyListView, TextBox postReplyTextBox, Action beforeLoad, Action<int, int> afterLoad)
        {
            StartPageNo = 1;
            ThreadDataType = ThreadDataType.MyFavorites;
            _threadId = threadId;
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            ThreadItem = _ds.GetThreadItemForMyFavorites(threadId);

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

            var cv = _ds.GetViewForReplyPage(pageNo, threadId, 0, _beforeLoad, _afterLoad);
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
