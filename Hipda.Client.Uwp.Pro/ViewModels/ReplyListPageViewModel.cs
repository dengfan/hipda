using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ReplyListPageViewModel : NotificationObject
    {
        /// <summary>
        /// 加载的起始页
        /// 如“我的回复”之回复列表页并不一定是从第一页开始加载的，所以需要用此属性来判断是否显示“加载上一页”的按钮。
        /// </summary>
        public int StartPageNo { get; set; } = 1;

        private int _threadId;
        private int _threadAuthorUserId;
        private ListView _replyListView { get; set; }
        private Action _beforeLoad { get; set; }
        private Action<int, int> _afterLoad { get; set; }
        private DataService _ds { get; set; }

        public DelegateCommand RefreshReplyCommand { get; set; }

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

        public List<FaceItemModel> FaceData
        {
            get
            {
                return FaceService.FaceData;
            }
        }

        void LoadData(int pageNo)
        {
            var cv = _ds.GetViewForReplyPageByThreadId(pageNo, _threadId, _threadAuthorUserId, _beforeLoad, _afterLoad);
            if (cv != null)
            {
                StartPageNo = pageNo;
                ReplyItemCollection = cv;
            }
        }

        public ReplyListPageViewModel(int pageNo, int threadId, int threadAuthorUserId, ListView replyListView, Action beforeLoad, Action<int, int> afterLoad)
        {
            _threadId = threadId;
            _threadAuthorUserId = threadAuthorUserId;
            _replyListView = replyListView;
            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _ds = new DataService();

            RefreshReplyCommand = new DelegateCommand();
            RefreshReplyCommand.ExecuteAction = (p) =>
            {
                _ds.ClearReplyData(_threadId);
                LoadData(1);
            };

            LoadData(pageNo);
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
