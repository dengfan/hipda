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

namespace Hipda.Client.Uwp.Pro.Controls
{
    public sealed class MyAvatarForThread : Control
    {
        private Border _border1;
        private Border _border2;

        public MyAvatarForThread()
        {
            this.DefaultStyleKey = typeof(MyAvatarForThread);
        }


        public int UserId
        {
            get { return (int)GetValue(UserIdProperty); }
            set { SetValue(UserIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserIdProperty =
            DependencyProperty.Register("UserId", typeof(int), typeof(MyAvatarForThread), new PropertyMetadata(0, new PropertyChangedCallback(OnUserIdChanged)));


        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Username.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(MyAvatarForThread), new PropertyMetadata(string.Empty));


        public int ThreadId
        {
            get { return (int)GetValue(ThreadIdProperty); }
            set { SetValue(ThreadIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThreadId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThreadIdProperty =
            DependencyProperty.Register("ThreadId", typeof(int), typeof(MyAvatarForThread), new PropertyMetadata(0));


        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _border1 = GetTemplateChild("border1") as Border;
            _border2 = GetTemplateChild("border2") as Border;
        }

        private static void OnUserIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as MyAvatarForThread;
            int userId = (int)e.NewValue;

            if (instance._border2 != null)
            {
                BitmapImage bi = new BitmapImage();
                bi.UriSource = Common.GetSmallAvatarUriByUserId(userId);
                bi.DecodePixelWidth = 40;
                ImageBrush ib = new ImageBrush();
                ib.Stretch = Stretch.UniformToFill;
                ib.ImageSource = bi;
                ib.ImageFailed += (s, e2) => { return; };
                instance._border2.Background = ib;
            }
        }

        protected override void OnHolding(HoldingRoutedEventArgs e)
        {
            base.OnHolding(e);
            Show();
        }

        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            base.OnRightTapped(e);
            Show();
        }

        void Show()
        {
            var frame = Window.Current.Content as Frame;
            var mp = frame.Content as MainPage;
            if (mp != null)
            {
                MainPage.PopupUserId = UserId;
                MainPage.PopupUsername = Username;
                MainPage.PopupThreadId = ThreadId;
                var menu = mp.Resources["AvatarContextMenuForThread"] as MenuFlyout;
                menu.ShowAt(this);
            }
        }
    }
}
