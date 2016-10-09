using GalaSoft.MvvmLight;
using Hipda.Client.Services;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Hipda.Client.ViewModels
{
    public class UserInfoPageViewModel : ViewModelBase
    {
        UserInfoService _ds;
        int _userId;

        private bool _isProgressRingActive = true;

        public bool IsProgressRingActive
        {
            get { return _isProgressRingActive; }
            set
            {
                Set(ref _isProgressRingActive, value);
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
                Set(ref _avatarBrush, value);
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
                Set(ref _userInfoRichTextBlock, value);
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
            UserInfoRichTextBlock = XamlReader.Load(xaml);
            IsProgressRingActive = false;

            BitmapImage bi = new BitmapImage();
            bi.UriSource = CommonService.GetBigAvatarUriByUserId(_userId);
            bi.DecodePixelWidth = 160;
            ImageBrush ib = new ImageBrush();
            ib.Stretch = Stretch.UniformToFill;
            ib.ImageSource = bi;
            ib.ImageFailed += (s, e) => { return; };
            AvatarBrush = ib;
        }
    }
}
