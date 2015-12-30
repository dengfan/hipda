using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.Views;
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
            set
            {
                _tipText = value;
                this.RaisePropertyChanged("TipText");
            }
        }

        private string _floorNoStr;

        public string FloorNoStr
        {
            get { return _floorNoStr; }
            set
            {
                _floorNoStr = value;
                this.RaisePropertyChanged("FloorNoStr");
            }
        }

        private Uri _avatarUri;

        public Uri AvatarUri
        {
            get { return _avatarUri; }
            set
            {
                _avatarUri = value;
                this.RaisePropertyChanged("AvatarUri");
            }
        }

        private string _authorUsername;

        public string AuthorUsername
        {
            get { return _authorUsername; }
            set
            {
                _authorUsername = value;
                this.RaisePropertyChanged("AuthorUsername");
            }
        }

        private string _authorCreateTime;

        public string AuthorCreateTime
        {
            get { return _authorCreateTime; }
            set
            {
                _authorCreateTime = value;
                this.RaisePropertyChanged("AuthorCreateTime");
            }
        }

        private object _xamlContent;

        public object XamlContent
        {
            get { return _xamlContent; }
            set
            {
                _xamlContent = value;
                this.RaisePropertyChanged("XamlContent");
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
            var replyItem = await _ds.GetPostDetail(_postId, _threadId);
            if (replyItem == null)
            {
                return;
            }

            MainPage.PopupUserId = replyItem.AuthorUserId;
            MainPage.PopupUsername = replyItem.AuthorUsername;
            MainPage.PopupThreadId = replyItem.ThreadId;

            TipText = string.Empty;
            FloorNoStr = replyItem.FloorNoStr;
            AvatarUri = replyItem.AvatarUri;
            AuthorUsername = replyItem.AuthorUsername;
            AuthorCreateTime = replyItem.AuthorCreateTime;
            XamlContent = replyItem.XamlContent;
        }
    }
}
