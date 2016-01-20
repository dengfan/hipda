using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ContentDialogForUserInfoViewModel : NotificationObject
    {
        DataService _ds;

        int _userId;
        public int UserId
        {
            get
            {
                return _userId;
            }
        }

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

        Uri _avatarUri;
        public Uri AvatarUri
        {
            get
            {
                return _avatarUri;
            }
            set
            {
                _avatarUri = value;
                this.RaisePropertyChanged("AvatarUri");
            }
        }

        object _userInfoRichTextBlock;
        public object UserInfoRichTextBlock
        {
            get
            {
                return _userInfoRichTextBlock;
            }
            set
            {
                _userInfoRichTextBlock = value;
                this.RaisePropertyChanged("UserInfoRichTextBlock");
            }
        }

        public ContentDialogForUserInfoViewModel(int userId)
        {
            _ds = new DataService();
            _userId = userId;
            
            GetUserInfoRichTextBlock();
        }

        async void GetUserInfoRichTextBlock()
        {
            string xaml = await _ds.GetXamlForUserInfo(_userId);
            TipText = string.Empty;
            UserInfoRichTextBlock = XamlReader.Load(xaml);
            AvatarUri = Common.GetBigAvatarUriByUserId(_userId);
        }
    }
}
