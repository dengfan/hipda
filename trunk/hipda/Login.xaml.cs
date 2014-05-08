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
        private HttpClient httpClient;
        private CancellationTokenSource cts;

        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public Login()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Helpers.CreateHttpClient(ref httpClient);
            cts = new CancellationTokenSource();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            Dictionary<string, string> postDataDic = new Dictionary<string, string>();
            postDataDic.Add("username", username);
            postDataDic.Add("password", password);

            HttpFormUrlEncodedContent postData = new HttpFormUrlEncodedContent(postDataDic);

            HttpResponseMessage response = await httpClient.PostAsync(new Uri("http://www.hi-pda.com/forum/logging.php?action=login&loginsubmit=yes&inajax=1"), postData).AsTask(cts.Token);
            string resultContent = await response.Content.ReadAsStringAsync().AsTask(cts.Token);
            if (resultContent.Contains("欢迎") && !resultContent.Contains("错误") && !resultContent.Contains("失败"))
            {
                if (!Frame.Navigate(typeof(PivotPage)))
                {
                    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                }
            }
            else
            {
                await Helpers.DisplayTextResultAsync(response, OutputField, cts.Token);
            }
            //await Helpers.DisplayTextResultAsync(response, OutputField, cts.Token);

            //HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            
            //if (response.Headers.ContainsKey("Set-Cookie"))
            //{
            //    string cookieStr = response.Headers["Set-Cookie"];

            //    string[] cookieStrAry = cookieStr.Split(';');
            //    foreach (string cookiestr in cookieStrAry)
            //    {
            //        OutputField.Text += cookiestr;
            //    }
            //}
            
        }
    }
}
