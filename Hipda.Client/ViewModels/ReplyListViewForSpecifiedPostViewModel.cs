using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Hipda.Client.Services;
using System;
using System.Threading;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.ViewModels
{
    /// <summary>
    /// 查看指定的POST之回复列表之视图模型
    /// 根据 post id 及 thread id 加载
    /// </summary>
    public class ReplyListViewForSpecifiedPostViewModel : ViewModelBase
    {
        int _startPageNo = 1;
        int _postId;
        int _threadId;
        ListView _replyListView;
        Action _beforeLoad;
        Action<int, int> _afterLoad;
        Action<int> _listViewScroll;
        ReplyListService _ds;

        public ICommand AddToFavoritesCommand { get; set; }
        public ICommand RefreshReplyCommand { get; set; }
        public ICommand LoadPrevPageDataCommand { get; set; }
        public ICommand LoadLastPageDataCommand { get; set; }


        ICollectionView _replyItemCollection;

        public ICollectionView ReplyItemCollection
        {
            get { return _replyItemCollection; }
            set
            {
                Set(ref _replyItemCollection, value);
            }
        }

        public ReplyListViewForSpecifiedPostViewModel(CancellationTokenSource cts, int postId, ListView replyListView, Action beforeLoad, Action<int, int> afterLoad, Action<int> listViewScroll)
        {
            _postId = postId;
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _listViewScroll = listViewScroll;
            _ds = new ReplyListService();

            AddToFavoritesCommand = new RelayCommand(async () =>
            {
                await SendService.SendAddToFavoritesActionAsync(cts, _threadId, _ds.GetThreadTitle(_threadId));
            });

            RefreshReplyCommand = new RelayCommand(() =>
            {
                _ds.ClearReplyData(_threadId);
                LoadData(1);
            });

            LoadPrevPageDataCommand = new RelayCommand(() =>
            {
                if (_startPageNo > 1)
                {
                    _ds.ClearReplyData(_threadId);
                    LoadData(_startPageNo - 1);
                }
            });

            LoadLastPageDataCommand = new RelayCommand(() =>
            {
                _ds.ClearReplyData(_threadId);
                LoadData(_ds.GetReplyMaxPageNo());
            });

            FirstLoad(cts);
        }

        async void FirstLoad(CancellationTokenSource cts)
        {
            // 先载入第一个转跳到的页面的数据，并得到页码之后即可进入正常流程
            int[] data = await _ds.LoadReplyDataForRedirectToSpecifiedPostAsync(_postId, cts);
            if (data != null)
            {
                int pageNo = data[0];
                int index = data[1];
                _threadId = data[2];
                _ds.SetScrollState(false);

                var cv = _ds.GetViewForRedirectToPostByThreadId(pageNo, _threadId, index, _beforeLoad, _afterLoad, _listViewScroll);
                if (cv != null)
                {
                    ReplyItemCollection = cv;
                    _startPageNo = pageNo;
                }
            }
        }

        void LoadData(int pageNo)
        {
            var cv = _ds.GetViewForReplyPageByThreadId(pageNo, _threadId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                _startPageNo = pageNo;
                ReplyItemCollection = cv;
            }
        }

        public void LoadPrevPageData()
        {
            if (_startPageNo > 1)
            {
                _ds.ClearReplyData(_threadId);
                LoadData(_startPageNo - 1);
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
