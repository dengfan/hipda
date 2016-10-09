using GalaSoft.MvvmLight;
using Hipda.Client.Services;
using System;

namespace Hipda.Client.ViewModels
{
    public class QuoteDetailPageViewModel : ViewModelBase
    {
        ReplyListService _ds;
        int _postId;
        int _threadId;

        private bool _isProgressRingActive = true;

        public bool IsProgressRingActive
        {
            get { return _isProgressRingActive; }
            set
            {
                Set(ref _isProgressRingActive, value);
            }
        }

        private string _floorNoStr;

        public string FloorNoStr
        {
            get { return _floorNoStr; }
            set
            {
                Set(ref _floorNoStr, value);
            }
        }

        private Uri _avatarUri;

        public Uri AvatarUri
        {
            get { return _avatarUri; }
            set
            {
                Set(ref _avatarUri, value);
            }
        }

        private string _authorUsername;

        public string AuthorUsername
        {
            get { return _authorUsername; }
            set
            {
                Set(ref _authorUsername, value);
            }
        }

        private string _authorCreateTime;

        public string AuthorCreateTime
        {
            get { return _authorCreateTime; }
            set
            {
                Set(ref _authorCreateTime, value);
            }
        }

        private object _xamlContent;

        public object XamlContent
        {
            get { return _xamlContent; }
            set
            {
                Set(ref _xamlContent, value);
            }
        }

        public QuoteDetailPageViewModel(int postId, int threadId)
        {
            _ds = new ReplyListService();
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

            IsProgressRingActive = false;
            FloorNoStr = replyItem.FloorNoStr;
            AvatarUri = replyItem.AvatarUri;
            AuthorUsername = replyItem.AuthorUsername;
            AuthorCreateTime = replyItem.AuthorCreateTime;
            XamlContent = replyItem.XamlContent;
        }
    }
}
