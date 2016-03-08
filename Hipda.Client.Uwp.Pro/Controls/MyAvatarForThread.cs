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


        public int MyWidth
        {
            get { return (int)GetValue(MyWidthProperty); }
            set { SetValue(MyWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyWidthProperty =
            DependencyProperty.Register("MyWidth", typeof(int), typeof(MyAvatarForThread), new PropertyMetadata(40));


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


        public string ThreadTitle
        {
            get { return (string)GetValue(ThreadTitleProperty); }
            set { SetValue(ThreadTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThreadName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThreadTitleProperty =
            DependencyProperty.Register("ThreadTitle", typeof(string), typeof(MyAvatarForThread), new PropertyMetadata(string.Empty));


        public int ForumId
        {
            get { return (int)GetValue(ForumIdProperty); }
            set { SetValue(ForumIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForumId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForumIdProperty =
            DependencyProperty.Register("ForumId", typeof(int), typeof(MyAvatarForThread), new PropertyMetadata(0));


        public string ForumName
        {
            get { return (string)GetValue(ForumNameProperty); }
            set { SetValue(ForumNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForumName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForumNameProperty =
            DependencyProperty.Register("ForumName", typeof(string), typeof(MyAvatarForThread), new PropertyMetadata(string.Empty));


        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _border1 = GetTemplateChild("border1") as Border;
            _border2 = GetTemplateChild("border2") as Border;

            if (UserId > 0)
            {
                GetAvatarImageBrush();
            }
        }

        void GetAvatarImageBrush()
        {
            if (_border1 == null || _border2 == null)
            {
                return;
            }

            double cornerRadius = (double)MyWidth / 2;

            _border1.Width = MyWidth;
            _border1.Height = MyWidth;
            _border1.CornerRadius = new CornerRadius(cornerRadius);

            _border2.Width = MyWidth;
            _border2.Height = MyWidth;
            _border2.CornerRadius = new CornerRadius(cornerRadius);

            var bi = new BitmapImage();
            bi.UriSource = CommonService.GetSmallAvatarUriByUserId(UserId);
            bi.DecodePixelWidth = MyWidth;

            var ib = new ImageBrush();
            ib.Stretch = Stretch.UniformToFill;
            ib.ImageSource = bi;
            ib.ImageFailed += (s, e2) => { return; };
            _border2.Background = ib;
        }

        private static void OnUserIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as MyAvatarForThread;
            int userId = (int)e.NewValue;

            if (userId > 0)
            {
                instance.GetAvatarImageBrush();
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
                MainPage.PopupThreadTitle = ThreadTitle;
                MainPage.PopupForumId = ForumId;
                MainPage.PopupForumName = ForumName;
                var menu = mp.Resources["AvatarContextMenuForThread"] as MenuFlyout;
                menu.ShowAt(this);
            }
        }
    }
}
