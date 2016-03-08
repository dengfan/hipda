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
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class UserInfoPageViewModel : NotificationObject
    {
        UserInfoService _ds;

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

        ImageBrush _avatarBrush;
        public ImageBrush AvatarBrush
        {
            get
            {
                return _avatarBrush;
            }
            set
            {
                _avatarBrush = value;
                this.RaisePropertyChanged("AvatarBrush");
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

        public UserInfoPageViewModel(int userId)
        {
            _ds = new UserInfoService();
            _userId = userId;
            
            GetUserInfoRichTextBlock();
        }

        async void GetUserInfoRichTextBlock()
        {
            string xaml = await _ds.GetXamlForUserInfo(_userId);
            TipText = string.Empty;
            UserInfoRichTextBlock = XamlReader.Load(xaml);

            BitmapImage bi = new BitmapImage();
            bi.UriSource = Common.GetBigAvatarUriByUserId(_userId);
            bi.DecodePixelWidth = 160;
            ImageBrush ib = new ImageBrush();
            ib.Stretch = Stretch.UniformToFill;
            ib.ImageSource = bi;
            ib.ImageFailed += (s, e) => { return; };
            AvatarBrush = ib;
        }
    }
}
