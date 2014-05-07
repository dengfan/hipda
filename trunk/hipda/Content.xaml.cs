using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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
    public sealed partial class Content : Page
    {
        private HttpClient httpClient;
        private CancellationTokenSource cts;

        public Content()
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

            Load();
        }

        protected async void Load()
        {
            string url = "http://www.hi-pda.com/forum/forumdisplay.php?fid=2&page=1";
            HttpResponseMessage response = await httpClient.GetAsync(new Uri(url)).AsTask(cts.Token);
            string resultContent = await response.Content.ReadAsStringAsync().AsTask(cts.Token);

            // 实例化HtmlAgilityPack.HtmlDocument对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(resultContent);

            //根据HTML节点NODE的ID获取节点
            HtmlNode navNode = doc.GetElementbyId("post_list");

            doc.DocumentNode.Descendants("a");

            await Helpers.DisplayTextResultAsync(response, OutputField, cts.Token);
        }
    }
}
