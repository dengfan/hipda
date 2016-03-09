using Hipda.Client.Uwp.Pro.ViewModels;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Hipda.Client.Uwp.Pro.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class QuoteDetailPage : Page
    {
        int _userId = 0;
        string _username = string.Empty;
        int _postId = 0;
        int _threadId = 0;

        public QuoteDetailPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string[] p = e.Parameter.ToString().Split(',');
            _userId = Convert.ToInt32(p[0]);
            _username = p[1];
            _postId = Convert.ToInt32(p[2]);
            _threadId = Convert.ToInt32(p[3]);

            SetTitle("查看引用楼");
            this.DataContext = new QuoteDetailPageViewModel(_postId, _threadId);
        }

        private void SetTitle(string title)
        {
            var frame = (Frame)Window.Current.Content;
            if (frame != null)
            {
                var mainPage = (MainPage)frame.Content;
                if (mainPage != null)
                {
                    mainPage.SetTitleForInputPanel(title);
                }
            }
        }

        private void UserInfoButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserInfoPage), $"{_userId},{_username}");
        }

        private void UserMessageButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserMessagePage), $"{_userId},{_username}");
        }
    }
}
