using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Hipda.Client.Uwp.Pro.Controls
{
    public sealed partial class PostMenuControl : UserControl
    {
        public PostMenuControl()
        {
            this.InitializeComponent();
        }

        private void OpenReplyPostPanel(object sender, RoutedEventArgs e)
        {
            var data = (ReplyItemModel)((MenuFlyoutItem)e.OriginalSource).DataContext;
            if (data == null)
            {
                return;
            }

            var frame = Window.Current.Content as Frame;
            var mp = frame.Content as MainPage;
            if (mp != null)
            {
                string postSimpleContent = data.TextStr.Length > 300 ? data.TextStr.Substring(0, 290) + "..." : data.TextStr;
                mp.OpenReplyPostPanel(data.AuthorUserId, data.AuthorUsername, postSimpleContent, data.FloorNo, data.PostId, data.ThreadId);
            }
        }
    }
}
