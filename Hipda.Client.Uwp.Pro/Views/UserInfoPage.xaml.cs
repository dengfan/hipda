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
    public sealed partial class UserInfoPage : Page
    {
        int _userId = 0;
        string _username = string.Empty;

        public UserInfoPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var ary = e.Parameter.ToString().Split(',');
            _userId = Convert.ToInt32(ary[0]);
            _username = ary[1];

            SetTitle($"{_username} 的个人资料");
            this.DataContext = new UserInfoPageViewModel(_userId);
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

        private void OpenPostUserMessagePanelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserInfoPage), $"{_userId},{_username}");
        }
    }
}
