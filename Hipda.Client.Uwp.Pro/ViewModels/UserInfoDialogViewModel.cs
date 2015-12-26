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
    public class UserInfoDialogViewModel : NotificationObject
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

        private string _tipText;

        public string TipText
        {
            get { return _tipText; }
            set
            {
                _tipText = value;
                this.RaisePropertyChanged("TipText");
            }
        }

        public Uri AvatarUri
        {
            get
            {
                return MyAvatar.GetAvatarUrl(_userId);
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

        public UserInfoDialogViewModel(int userId)
        {
            _ds = new DataService();
            _userId = userId;
            TipText = "请稍候，载入中。。。";
            GetUserInfoRichTextBlock();
        }

        async void GetUserInfoRichTextBlock()
        {
            string xaml = await _ds.GetXamlForUserInfo(_userId);
            UserInfoRichTextBlock = XamlReader.Load(xaml);
            TipText = string.Empty;
        }
    }
}
