using Hipda.Client.Uwp.Pro.Controls;
using Hipda.Client.Uwp.Pro.Models;
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
    public sealed partial class UserMessageHubPage : Page
    {
        public UserMessageHubPage()
        {
            this.InitializeComponent();
            this.DataContext = new UserMessageHubPageViewModel();
        }

        private void UserMessageListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (UserMessageListView.SelectionMode == ListViewSelectionMode.Multiple)
            {
                return;
            }

            var data = (UserMessageListItemModel)e.ClickedItem;
            if (data != null)
            {
                Frame.Navigate(typeof(UserMessagePage), data.UserId);

                var frame = (Frame)Window.Current.Content;
                if (frame != null)
                {
                    var mainPage = (MainPage)frame.Content;
                    if (mainPage != null)
                    {
                        mainPage.SetTitleForInputPanel($"与 {data.Username} 聊天");
                    }
                }
            }
        }

        private void MultipleSelectButton_Checked(object sender, RoutedEventArgs e)
        {
            UserMessageListView.SelectionMode = ListViewSelectionMode.Multiple;
        }

        private void MultipleSelectButton_Unchecked(object sender, RoutedEventArgs e)
        {
            UserMessageListView.SelectionMode = ListViewSelectionMode.Single;
        }
    }
}
