using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Hipda.Client.Uwp.Pro.Controls
{
    public sealed class MyLink : Control
    {
        public MyLink()
        {
            this.DefaultStyleKey = typeof(MyLink);
        }


        public int ThreadId
        {
            get { return (int)GetValue(ThreadIdProperty); }
            set { SetValue(ThreadIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThreadId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThreadIdProperty =
            DependencyProperty.Register("ThreadId", typeof(int), typeof(MyLink), new PropertyMetadata(0));


        public string LinkContent
        {
            get { return (string)GetValue(LinkContentProperty); }
            set { SetValue(LinkContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LinkContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinkContentProperty =
            DependencyProperty.Register("LinkContent", typeof(string), typeof(MyLink), new PropertyMetadata(0));



        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            base.OnTapped(e);

            var frame = Window.Current.Content as Frame;
            var mp = frame.Content as MainPage;
            if (mp != null)
            {
                mp.CloseUserDialog();

                var pageType = mp.AppFrame.Content.GetType();
                if (pageType.Equals(typeof(ThreadAndReplyPage)))
                {
                    var p = (ThreadAndReplyPage)mp.AppFrame.Content;
                    if (p != null)
                    {
                        p.ThreadId = ThreadId;
                        p.OpenReplyPageByThreadId();
                    }
                }
                else if (pageType.Equals(typeof(ReplyListPage)))
                {
                    var p = (ReplyListPage)mp.AppFrame.Content;
                    if (p != null)
                    {
                        p.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
                        p.ThreadId = ThreadId;
                        p.OpenReplyPageByThreadId();
                    }
                }
            }
        }
    }
}
