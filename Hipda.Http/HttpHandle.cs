using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Hipda.Http
{
    public class HttpHandle
    {
        Encoding _gbk = null;
        private static readonly HttpHandle _instance = new HttpHandle();

        public HttpHandle()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _gbk = Encoding.GetEncoding("GBK");
        }

        public static HttpHandle GetInstance()
        {
            return _instance;
        }

        public void ClearCookies()
        {
            var filter = new HttpBaseProtocolFilter();
            var cookieCollection = filter.CookieManager.GetCookies(new Uri("http://www.hi-pda.com"));
            foreach (var item in cookieCollection)
            {
                filter.CookieManager.DeleteCookie(item);
            }
        }

        async void ShowError(string errorText, string errorTitle)
        {
            await new MessageDialog(errorText, errorTitle).ShowAsync();
        }

        public async Task<string> GetAsync(string url, CancellationTokenSource cts)
        {
            var result = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    // 在异步任务中加入进度监控
                    var response = await client.GetAsync(new Uri(url)).AsTask(cts.Token);
                    var buf = await response.Content.ReadAsBufferAsync();
                    result = _gbk.GetString(buf.ToArray());
                }
            }
            catch (Exception ex)
            {
                cts.Cancel();
                cts.Dispose();

                string err = $"请检查网络连接是否正常。\r\n{ex.Message}";
                ShowError(err, "GetAsync 请求失败");
            }

            return result;
        }

        public async Task<string> PostAsync(string url, List<KeyValuePair<string, object>> toPost, CancellationTokenSource cts)
        {
            var result = string.Empty;
            string postData = GetQueryString(toPost);

            try
            {
                using (var client = new HttpClient())
                {
                    var httpContent = new HttpStringContent(postData, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                    httpContent.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";

                    var response = await client.PostAsync(new Uri(url), httpContent).AsTask(cts.Token);
                    var buf = await response.Content.ReadAsBufferAsync();
                    result = _gbk.GetString(buf.ToArray());
                }
            }
            catch (Exception ex)
            {
                cts.Cancel();
                cts.Dispose();

                string err = $"请检查网络连接是否正常。\r\n{ex.Message}";
                ShowError(err, "PostAsync 请求失败");
            }

            return result;
        }

        public async Task<string> PostFileAsync(string url, IDictionary<string, object> toPost, string filename, string fieldname, IBuffer buffer, CancellationTokenSource cts)
        {
            var result = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    string boundary = $"---------------------{DateTime.Now.Ticks.ToString("x")}";
                    var httpContent = new HttpMultipartFormDataContent(boundary);

                    foreach (var item in toPost)
                    {
                        httpContent.Add(new HttpStringContent(item.Value.ToString(), Windows.Storage.Streams.UnicodeEncoding.Utf8), item.Key);
                    }

                    var imageContent = new HttpBufferContent(buffer);
                    httpContent.Add(imageContent, fieldname, EncodeToIso(filename));

                    var response = await client.PostAsync(new Uri(url), httpContent).AsTask(cts.Token);
                    var buf = await response.Content.ReadAsBufferAsync();
                    result = _gbk.GetString(buf.ToArray());
                }
            }
            catch (Exception ex)
            {
                cts.Cancel();
                cts.Dispose();

                string err = $"请检查网络连接是否正常。\r\n{ex.Message}";
                ShowError(err, "PostFileAsync 请求失败");
            }
            
            return result;
        }

        /// <summary>
        /// 将字典数据转换为QueryString
        /// </summary>
        /// <param name="toPost">字典数据</param>
        /// <returns>QueryString</returns>
        public string GetQueryString(List<KeyValuePair<string, object>> toPost)
        {
            string queryStr = string.Empty;

            bool first = true;
            foreach (var item in toPost)
            {
                if (first)
                {
                    first = false;
                    queryStr += GetEncoding(item.Key.Trim()) + "=" + GetEncoding(item.Value.ToString().Trim());
                }
                else
                {
                    queryStr += "&" + GetEncoding(item.Key.Trim()) + "=" + GetEncoding(item.Value.ToString().Trim());
                }
            }

            return queryStr;
        }

        public string GetEncoding(string str)
        {
            byte[] b = _gbk.GetBytes(str);
            b = WebUtility.UrlEncodeToBytes(b, 0, b.Length);
            return _gbk.GetString(b);
        }

        private string EncodeToIso(string str)
        {
            byte[] b = _gbk.GetBytes(str);
            return Encoding.GetEncoding("ISO-8859-1").GetString(b);
        }
    }
}