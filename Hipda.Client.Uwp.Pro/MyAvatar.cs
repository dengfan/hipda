using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Hipda.Client.Uwp.Pro
{
    public sealed class MyAvatar : Control
    {
        private Border _border1;
        private Grid _grid1;

        public MyAvatar()
        {
            this.DefaultStyleKey = typeof(MyAvatar);
        }



        public int UserId
        {
            get { return (int)GetValue(UserIdProperty); }
            set { SetValue(UserIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserIdProperty =
            DependencyProperty.Register("UserId", typeof(int), typeof(MyAvatar), new PropertyMetadata(0, new PropertyChangedCallback(OnUserIdChanged)));


        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Username.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(MyAvatar), new PropertyMetadata(string.Empty));


        public int ThreadId
        {
            get { return (int)GetValue(ThreadIdProperty); }
            set { SetValue(ThreadIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThreadId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThreadIdProperty =
            DependencyProperty.Register("ThreadId", typeof(int), typeof(MyAvatar), new PropertyMetadata(0));


        public bool IsRightTappedEnable
        {
            get { return (bool)GetValue(IsRightTappedEnableProperty); }
            set { SetValue(IsRightTappedEnableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRightTappedEnable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRightTappedEnableProperty =
            DependencyProperty.Register("IsRightTappedEnable", typeof(bool), typeof(MyAvatar), new PropertyMetadata(true));


        public bool IsNotAssociateThreadId
        {
            get { return (bool)GetValue(IsNotAssociateThreadIdProperty); }
            set { SetValue(IsNotAssociateThreadIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsNotAssociateThreadId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsNotAssociateThreadIdProperty =
            DependencyProperty.Register("IsNotAssociateThreadId", typeof(bool), typeof(MyAvatar), new PropertyMetadata(false));


        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _border1 = GetTemplateChild("border1") as Border;
            _grid1 = GetTemplateChild("grid1") as Grid;
        }

        private static void OnUserIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as MyAvatar;
            int userId = (int)e.NewValue;

            if (instance._grid1 != null)
            {
                BitmapImage bi = new BitmapImage();
                bi.UriSource = GetAvatarUrl(userId);
                bi.DecodePixelWidth = 40;
                ImageBrush ib = new ImageBrush();
                ib.Stretch = Stretch.UniformToFill;
                ib.ImageSource = bi;
                ib.ImageFailed += (s, e2) => { return; };
                instance._grid1.Background = ib;
            }
        }

        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            base.OnRightTapped(e);

            if (IsRightTappedEnable)
            {
                var parentPage = Common.FindParent<ThreadAndReplyPage>(this);
                ThreadAndReplyPage.PopupUserId = UserId;
                ThreadAndReplyPage.PopupUsername = Username;
                ThreadAndReplyPage.PopupThreadId = ThreadId;
                var menu = parentPage.Resources["avatarContextMenu"] as MenuFlyout;
                menu.Items[4].Visibility = IsNotAssociateThreadId ? Visibility.Collapsed : Visibility.Visible;
                menu.ShowAt(this);
            }
        }

        public static Uri GetAvatarUrl(int userId)
        {
            int uid = userId;
            var s = new int[10];
            for (int i = 0; i < s.Length - 1; ++i)
            {
                s[i] = uid % 10;
                uid = (uid - s[i]) / 10;
            }
            return new Uri("http://www.hi-pda.com/forum/uc_server/data/avatar/" + s[8] + s[7] + s[6] + "/" + s[5] + s[4] + "/" + s[3] + s[2] + "/" + s[1] + s[0] + "_avatar_middle.jpg");
        }
    }
}
