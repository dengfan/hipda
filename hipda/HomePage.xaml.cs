using hipda.Common;
using hipda.Data;
using hipda.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace hipda
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private const int maxHubSectionCount = 4;
        private const string FirstGroupName = "FirstGroup";

        private readonly NavigationHelper navigationHelper;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private HttpHandle httpClient = HttpHandle.getInstance();

        public HomePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// 获取与此 <see cref="Page"/> 关联的 <see cref="NavigationHelper"/>。
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private async void Refresh()
        {
            // 刷新版块列表
            replyProgressBar.Visibility = Visibility.Visible;
            cvsForumGroups.Source = await DataSource.GetForumGroupsAsync();
            replyProgressBar.Visibility = Visibility.Collapsed;

            // 刷新顶部状态栏
            Account account = AccountSettings.GetDefault();
            string accountName = account != null ? account.Username : "未登录";
            if (accountName.Length > 3)
            {
                accountName = string.Format("{0}*{1}", accountName.Substring(0, 2), accountName.Last());
            }

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.Text = string.Format("Hi!PDA > {0}", accountName);
                await statusBar.ProgressIndicator.ShowAsync();
            }
        }

        /// <summary>
        /// 使用在导航过程中传递的内容填充页。在从以前的会话
        /// 重新创建页时，也会提供任何已保存状态。
        /// </summary>
        /// <param name="sender">
        /// 事件的来源；通常为 <see cref="NavigationHelper"/>。
        /// </param>
        /// <param name="e">事件数据，其中既提供在最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, Object)"/> 的导航参数，又提供
        /// 此页在以前会话期间保留的状态的
        /// 的字典。首次访问页面时，该状态将为 null。</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            accountList.ItemsSource = AccountSettings.List;
            layoutModeComboBox.SelectedIndex = LayoutModeSettings.LayoutModeSetting;
            imageCountComboBox.SelectedIndex = ImageCountSettings.ImageCountSetting;

            Refresh();
        }

        /// <summary>
        /// 保留与此页关联的状态，以防挂起应用程序或
        /// 从导航缓存中放弃此页。值必须符合序列化
        /// <see cref="SuspensionManager.SessionState"/> 的序列化要求。
        /// </summary>
        /// <param name="sender">事件的来源；通常为 <see cref="NavigationHelper"/>。</param>
        ///<param name="e">提供要使用可序列化状态填充的空字典
        ///的事件数据。</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: 在此处保存页面的唯一状态。
        }

        private void ForumItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            Forum forum = (Forum)e.ClickedItem;
            string data = string.Format("{0},{1}", forum.Id, forum.Alias);
            switch (LayoutModeSettings.LayoutModeSetting)
            {
                case 1:
                    if (!Frame.Navigate(typeof(TabPlainPage), data))
                    {
                        throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                    }
                    break;
                case 2:
                    if (!Frame.Navigate(typeof(TabBubblePage), data))
                    {
                        throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                    }
                    break;
                default:
                    if (!Frame.Navigate(typeof(TabClassicPage), data))
                    {
                        throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                    }
                    break;
            }
        }

        private void openTabForApp_Click(object sender, RoutedEventArgs e)
        {
            switch (LayoutModeSettings.LayoutModeSetting)
            {
                case 1:
                    if (!Frame.Navigate(typeof(TabPlainPage)))
                    {
                        throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                    }
                    break;
                case 2:
                    if (!Frame.Navigate(typeof(TabBubblePage)))
                    {
                        throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                    }
                    break;
                default:
                    if (!Frame.Navigate(typeof(TabClassicPage)))
                    {
                        throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                    }
                    break;
            }
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            StackPanel loginPanel = (StackPanel)accountList.FindName("addAccountPanel");
            TextBox usernameTextBox = ((TextBox)loginPanel.FindName("usernameTextBox"));
            PasswordBox passwordBox = ((PasswordBox)loginPanel.FindName("passwordTextBox"));
            ComboBox questionComboBox = ((ComboBox)loginPanel.FindName("questionComboBox"));
            TextBox answerTextBox = ((TextBox)loginPanel.FindName("answerTextBox"));

            string username = usernameTextBox.Text.Trim().ToLower();
            string password = passwordBox.Password.Trim();
            int question = questionComboBox.SelectedIndex;
            string answer = answerTextBox.Text.Trim();

            // 清除当前的登录cookie
            httpClient.ClearCookies();

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                bool isOk = await AccountSettings.LoginAndAdd(username, password, question, answer, true);
                if (isOk)
                {
                    Refresh();
                }
                else
                {
                    await new MessageDialog(DataSource.LoginMessage, "账号登录失败").ShowAsync();
                }
            }

            usernameTextBox.Text = string.Empty;
            passwordBox.Password = string.Empty;
            questionComboBox.SelectedIndex = 0;
            answerTextBox.Text = string.Empty;
            answerTextBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        #region NavigationHelper 注册

        /// <summary>
        /// 此部分中提供的方法只是用于使
        /// NavigationHelper 可响应页面的导航方法。
        /// <para>
        /// 应将页面特有的逻辑放入用于
        /// <see cref="NavigationHelper.LoadState"/>
        /// 和 <see cref="NavigationHelper.SaveState"/> 的事件处理程序中。
        /// 除了在会话期间保留的页面状态之外
        /// LoadState 方法中还提供导航参数。
        /// </para>
        /// </summary>
        /// <param name="e">提供导航方法数据和
        /// 无法取消导航请求的事件处理程序。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void accountItem_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

        private async void setDefaultItem_Click(object sender, RoutedEventArgs e)
        {
            // 清除登录cookies
            httpClient.ClearCookies();

            Account data = (sender as MenuFlyoutItem).DataContext as Account;
            string keyName = data.Key;
            await AccountSettings.SetDefault(keyName);

            Refresh();
        }

        private async void deleteItem_Click(object sender, RoutedEventArgs e)
        {
            // 清除登录cookies
            httpClient.ClearCookies();

            Account data = (sender as MenuFlyoutItem).DataContext as Account;
            string keyName = data.Key;
            await AccountSettings.Delete(keyName);

            Refresh();
        }

        private void questionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            answerTextBox.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (((ComboBox)sender).SelectedIndex == 0)
            {
                answerTextBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private async void marketplaceReviewAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }

        private void layoutModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LayoutModeSettings.LayoutModeSetting = ((ComboBox)sender).SelectedIndex;
        }

        private void imageCountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ImageCountSettings.ImageCountSetting = ((ComboBox)sender).SelectedIndex;
        }

        
    }
}
