using Hipda.Client.Services;
using Hipda.Client.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Hipda.Client.Controls
{
    public sealed class MyQuoteLink : Control
    {
        public MyQuoteLink()
        {
            this.DefaultStyleKey = typeof(MyQuoteLink);
        }


        public int UserId
        {
            get { return (int)GetValue(UserIdProperty); }
            set { SetValue(UserIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserIdProperty =
            DependencyProperty.Register("UserId", typeof(int), typeof(MyQuoteLink), new PropertyMetadata(0));


        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Username.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(MyQuoteLink), new PropertyMetadata(string.Empty));


        public int PostId
        {
            get { return (int)GetValue(PostIdProperty); }
            set { SetValue(PostIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PostId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PostIdProperty =
            DependencyProperty.Register("PostId", typeof(int), typeof(MyQuoteLink), new PropertyMetadata(0));



        public int ThreadId
        {
            get { return (int)GetValue(ThreadIdProperty); }
            set { SetValue(ThreadIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThreadId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThreadIdProperty =
            DependencyProperty.Register("ThreadId", typeof(int), typeof(MyQuoteLink), new PropertyMetadata(0));



        public string LinkContent
        {
            get { return (string)GetValue(LinkContentProperty); }
            set { SetValue(LinkContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LinkContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinkContentProperty =
            DependencyProperty.Register("LinkContent", typeof(string), typeof(MyQuoteLink), new PropertyMetadata(string.Empty));


        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            base.OnTapped(e);

            var frame = Window.Current.Content as Frame;
            var mp = frame.Content as MainPage;
            if (mp != null)
            {
                mp.OpenQuoteDetailPage(UserId, Username, PostId, ThreadId);
            }
        }
    }
}
