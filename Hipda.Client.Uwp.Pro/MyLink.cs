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

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Hipda.Client.Uwp.Pro
{
    public sealed class MyLink : Control
    {
        public MyLink()
        {
            this.DefaultStyleKey = typeof(MyLink);
        }



        public string ThreadIdStr
        {
            get { return (string)GetValue(ThreadIdStrProperty); }
            set { SetValue(ThreadIdStrProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThreadIdStr.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThreadIdStrProperty =
            DependencyProperty.Register("ThreadIdStr", typeof(string), typeof(MyLink), new PropertyMetadata(0));


        public string LinkContent
        {
            get { return (string)GetValue(LinkContentProperty); }
            set { SetValue(LinkContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LinkContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinkContentProperty =
            DependencyProperty.Register("LinkContent", typeof(string), typeof(MyLink), new PropertyMetadata(0));





        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Button button1 = GetTemplateChild("button1") as Button;

            button1.Click += async (s, e) => {
                await new MessageDialog(LinkContent).ShowAsync();
            };
        }
    }
}
