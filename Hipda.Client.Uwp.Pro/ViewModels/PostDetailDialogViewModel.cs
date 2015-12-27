using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class PostDetailDialogViewModel : NotificationObject
    {
        DataService _ds;
        int _postId;
        int _threadId;

        private string _tipText = "请稍候，载入中。。。";

        public string TipText
        {
            get { return _tipText; }
            set { _tipText = value; }
        }

        private ReplyItemModel _replyItem;

        public ReplyItemModel ReplyItem
        {
            get { return _replyItem; }
            set
            {
                _replyItem = value;
                this.RaisePropertyChanged("ReplyItem");
            }
        }


        public PostDetailDialogViewModel(int postId, int threadId)
        {
            _ds = new DataService();
            _postId = postId;
            _threadId = threadId;

            GetData();
        }

        async void GetData()
        {
            TipText = string.Empty;
            ReplyItem = await _ds.GetPostDetail(_postId, _threadId);
        }

    }
}
