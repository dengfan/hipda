using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Hipda.Client.Uwp.Pro
{
    public sealed class MyFace : Control
    {
        private Grid _grid1;

        public MyFace()
        {
            this.DefaultStyleKey = typeof(MyFace);
        }



        public int UserId
        {
            get { return (int)GetValue(UserIdProperty); }
            set { SetValue(UserIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserIdProperty =
            DependencyProperty.Register("UserId", typeof(int), typeof(MyFace), new PropertyMetadata(0, new PropertyChangedCallback(OnUserIdChanged)));


        public int ThreadId
        {
            get { return (int)GetValue(ThreadIdProperty); }
            set { SetValue(ThreadIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThreadId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThreadIdProperty =
            DependencyProperty.Register("ThreadId", typeof(int), typeof(MyFace), new PropertyMetadata(0));




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

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _grid1 = GetTemplateChild("grid1") as Grid;
        }

        private static void OnUserIdChanged(DependencyObject d,DependencyPropertyChangedEventArgs e)
        {
            var instance = d as MyFace;

            if (instance._grid1 != null)
            {
                BitmapImage bi = new BitmapImage();
                bi.UriSource = GetAvatarUrl((int)e.NewValue);
                bi.DecodePixelWidth = 40;
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = bi;
                instance._grid1.Background = ib;
            }
        }


        protected async override void OnHolding(HoldingRoutedEventArgs e)
        {
            base.OnHolding(e);

            await new MessageDialog(ThreadId.ToString()).ShowAsync();
        }

        protected async override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            base.OnRightTapped(e);
            await new MessageDialog(ThreadId.ToString()).ShowAsync();
        }
    }
}
