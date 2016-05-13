using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
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
                string quoteSimpleContent = data.TextStr.Length > 300 ? data.TextStr.Substring(0, 290) + " ..." : data.TextStr;
                //mp.OpenSendReplyPostPanel("r", data.AuthorUserId, data.AuthorUsername, quoteSimpleContent, data.AuthorCreateTime, data.FloorNo, data.PostId, data.ThreadId);
            }
        }

        private void OpenQuotePostPanel(object sender, RoutedEventArgs e)
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
                string quoteSimpleContent = data.TextStr.Length > 300 ? data.TextStr.Substring(0, 290) + " ..." : data.TextStr;
                //mp.OpenSendReplyPostPanel("q", data.AuthorUserId, data.AuthorUsername, quoteSimpleContent, data.AuthorCreateTime, data.FloorNo, data.PostId, data.ThreadId);
            }
        }

        private async void OpenEditPostPanel(object sender, RoutedEventArgs e)
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
                var cts = new CancellationTokenSource();
                var editData = await ReplyListService.LoadContentForEditAsync(cts, data.PostId, data.ThreadId); // 先载入要修改的贴子的内容
                mp.OpenSendEditPostPanel(editData);
            }
        }

        private async void OpenInBrowser(object sender, RoutedEventArgs e)
        {
            var data = (ReplyItemModel)((MenuFlyoutItem)e.OriginalSource).DataContext;
            if (data == null)
            {
                return;
            }

            var uriStr = $"http://www.hi-pda.com/forum/viewthread.php?tid={data.ThreadId}&rpid={data.PostId}&fav=yes#pid{data.PostId}";
            Uri uri = new Uri(uriStr, UriKind.Absolute);
            await Launcher.LaunchUriAsync(uri);
        }

        private void CopyUrl(object sender, RoutedEventArgs e)
        {
            var data = (ReplyItemModel)((MenuFlyoutItem)e.OriginalSource).DataContext;
            if (data == null)
            {
                return;
            }

            var url = $"http://www.hi-pda.com/forum/viewthread.php?tid={data.ThreadId}&rpid={data.PostId}&fav=yes#pid{data.PostId}";
            var dataPackage = new DataPackage();
            dataPackage.SetText(url);
            Clipboard.SetContent(dataPackage);
        }
    }
}
