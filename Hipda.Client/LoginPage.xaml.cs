using Hipda.Client.Services;
using Hipda.Http;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Hipda.Client
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        static HttpHandle _httpClient = HttpHandle.GetInstance();

        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameTextBox.Text.Trim().ToLower();
            string password = passwordBox.Password.Trim();
            int question = questionComboBox.SelectedIndex;
            string answer = answerTextBox.Text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                await new MessageDialog("请输入账号！", "温馨提示").ShowAsync();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                await new MessageDialog("请输入密码！", "温馨提示").ShowAsync();
                return;
            }

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                // 清除当前的登录cookie
                _httpClient.ClearCookies();

                var accountService = new AccountService();
                bool isOk = await accountService.LoginAsync(username, password, question, answer);
                if (isOk)
                {
                    Frame.Navigate(typeof(MainPage), $"fid=2");
                }
            }
        }
    }
}
