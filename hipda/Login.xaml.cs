using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace hipda
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Login : Page
    {
        private HttpHandle httpClient = HttpHandle.getInstance();

        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public Login()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            httpClient.setEncoding("gb2312");
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("username", username);
            postData.Add("password", password);

            string resultContent = await httpClient.HttpPost("http://www.hi-pda.com/forum/logging.php?action=login&loginsubmit=yes&inajax=1", postData);
            if (resultContent.Contains("欢迎") && !resultContent.Contains("错误") && !resultContent.Contains("失败"))
            {
                if (!Frame.Navigate(typeof(HomePage)))
                {
                    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                }
            }
            else
            {
                OutputField.Text = resultContent;
            }
        }
    }
}
