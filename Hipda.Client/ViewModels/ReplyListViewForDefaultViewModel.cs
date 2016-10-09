using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Hipda.Client.Services;
using System;
using System.Threading;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.ViewModels
{
    /// <summary>
    /// 回复列表之视图模型
    /// 根据 thread id 加载
    /// </summary>
    public class ReplyListViewForDefaultViewModel : ViewModelBase
    {
        int _startPageNo = 1;
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
        public ICommand CopyUrlCommand { get; set; }
        public ICommand OpenInBrowserCommand { get; set; }


        ICollectionView _replyItemCollection;

        public ICollectionView ReplyItemCollection
        {
            get { return _replyItemCollection; }
            set
            {
                Set(ref _replyItemCollection, value);
            }
        }

        public ReplyListViewForDefaultViewModel(CancellationTokenSource cts, int threadId, ListView replyListView, Action beforeLoad, Action<int, int> afterLoad, Action<int> listViewScroll)
        {
            _threadId = threadId;
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _listViewScroll = listViewScroll;
            _ds = new ReplyListService();

            AddToFavoritesCommand = new RelayCommand(async () =>
            {
                await SendService.SendAddToFavoritesActionAsync(cts, threadId, _ds.GetThreadTitle(threadId));
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
                LoadLastData(cts);
            });

            CopyUrlCommand = new RelayCommand(() =>
            {
                string url = $"http://www.hi-pda.com/forum/viewthread.php?tid={_threadId}";
                var dataPackage = new DataPackage();
                dataPackage.SetText(url);
                Clipboard.SetContent(dataPackage);
            });

            OpenInBrowserCommand = new RelayCommand(async () =>
            {
                var url = $"http://www.hi-pda.com/forum/viewthread.php?tid={_threadId}";
                Uri uri = new Uri(url, UriKind.Absolute);
                await Launcher.LaunchUriAsync(uri);
            });

            LoadData(_startPageNo);
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

        async void LoadLastData(CancellationTokenSource cts)
        {
            // 直接载入并定位到最后一个回复
            int[] data = await _ds.LoadReplyDataForRedirectToLastPostAsync(_threadId, cts);
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
