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
        private Grid _grid1;
        private MenuFlyoutItem _btn1;
        private MenuFlyoutItem _btn2;
        private MenuFlyoutItem _btn3;
        private MenuFlyoutItem _btn4;

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


        public int ThreadId
        {
            get { return (int)GetValue(ThreadIdProperty); }
            set { SetValue(ThreadIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThreadId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThreadIdProperty =
            DependencyProperty.Register("ThreadId", typeof(int), typeof(MyAvatar), new PropertyMetadata(0, new PropertyChangedCallback(OnThreadIdChanged)));


        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _grid1 = GetTemplateChild("grid1") as Grid;
            _btn1 = GetTemplateChild("btn1") as MenuFlyoutItem;
            _btn1.Text = "查看详细资料";
            _btn1.Tapped += async (s, e) => { await new UserInfoDialog().ShowAsync(); };
            _btn2 = GetTemplateChild("btn2") as MenuFlyoutItem;
            _btn2.Text = "查看发贴记录";
            _btn3 = GetTemplateChild("btn3") as MenuFlyoutItem;
            _btn3.Text = "屏蔽此人所有主题及回复";
            _btn4 = GetTemplateChild("btn4") as MenuFlyoutItem;
            _btn4.Text = "屏蔽此主题";
        }

        private static void OnUserIdChanged(DependencyObject d,DependencyPropertyChangedEventArgs e)
        {
            var instance = d as MyAvatar;
            int userId = (int)e.NewValue;

            if (instance._grid1 != null)
            {
                BitmapImage bi = new BitmapImage();
                bi.UriSource = GetAvatarUrl(userId);
                bi.DecodePixelWidth = 40;
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = bi;
                ib.ImageFailed += (s, e2) => { return; };
                instance._grid1.Background = ib;
            }
        }

        private static void OnThreadIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as MyAvatar;
            int threadId = (int)e.NewValue;

            if (instance._btn4 != null && threadId > 0)
            {
                instance._btn4.Visibility = Visibility.Visible;
            }
        }

        protected override void OnHolding(HoldingRoutedEventArgs e)
        {
            base.OnHolding(e);
            var border1 = GetTemplateChild("border1") as Border;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(border1);
            flyoutBase.ShowAt(border1);
        }

        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            base.OnRightTapped(e);
            var border1 = GetTemplateChild("border1") as Border;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(border1);
            flyoutBase.ShowAt(border1);
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
