using Hipda.Client.Uwp.Pro.Services;
using Hipda.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Hipda.Client.Uwp.Pro
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            HttpHandle httpClient = HttpHandle.getInstance();
            string username = usernameTextBox.Text.Trim().ToLower();
            string password = passwordBox.Password.Trim();
            int question = questionComboBox.SelectedIndex;
            string answer = answerTextBox.Text.Trim();

            // 清除当前的登录cookie
            httpClient.ClearCookies();

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var accountService = new AccountService();
                bool isOk = await accountService.LoginAndSave(username, password, question, answer, true);
                if (isOk)
                {
                    int fid = 2;
                    Frame.Navigate(typeof(MainPage), fid);
                }
                else
                {
                    await new MessageDialog(accountService.LoginResultMessage, "登录失败").ShowAsync();
                }
            }
            else
            {
                await new MessageDialog("请输入账号密码！", "温馨提示").ShowAsync();
            }
        }
    }
}
