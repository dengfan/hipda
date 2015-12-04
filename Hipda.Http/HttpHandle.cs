using DBCSCodePage;
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
        private Encoding gbk = null;
        private static HttpHandle instance = null;

        public HttpHandle()
        {
            gbk = DBCSEncoding.GetDBCSEncoding("gb2312");
        }

        public static HttpHandle getInstance()
        {
            if (instance == null)
            {
                instance = new HttpHandle();
            }

            return instance;
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

        public async Task<string> GetAsync(string url, CancellationTokenSource cts)
        {
            var result = string.Empty;
            //var cts = new CancellationTokenSource();

            try
            {
                using (var client = new HttpClient())
                {
                    // 在异步任务中加入进度监控
                    var response = await client.GetAsync(new Uri(url)).AsTask(cts.Token);
                    var buf = await response.Content.ReadAsBufferAsync();
                    byte[] bytes = WindowsRuntimeBufferExtensions.ToArray(buf, 0, (int)buf.Length);
                    result = gbk.GetString(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                cts.Cancel();
                //await new MessageDialog(ex.Message + "\n\n请尝试刷新或检查网络连接是否正常！", "GET请求失败").ShowAsync();
            }

            return result;
        }

        public async Task<string> PostAsync(string url, List<KeyValuePair<string, object>> toPost, CancellationTokenSource cts)
        {
            var result = string.Empty;
            string postData = GetQueryString(toPost);
            //var cts = new CancellationTokenSource();

            try
            {
                using (var client = new HttpClient())
                {
                    var httpContent = new HttpStringContent(postData, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                    httpContent.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";

                    var response = await client.PostAsync(new Uri(url), httpContent).AsTask(cts.Token);
                    var buf = await response.Content.ReadAsBufferAsync();
                    byte[] bytes = WindowsRuntimeBufferExtensions.ToArray(buf, 0, (int)buf.Length);
                    result = gbk.GetString(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                cts.Cancel();
                await new MessageDialog(ex.Message + "\n\n请尝试刷新或检查网络连接是否正常！", "POST请求失败").ShowAsync();
            }

            return result;
        }

        public async Task<string> PostFileAsync(string url, IDictionary<string, object> toPost, string filename, string filetype, string fieldname, byte[] buffer, CancellationTokenSource cts)
        {
            var result = string.Empty;
            //var cts = new CancellationTokenSource();

            try
            {
                using (var client = new HttpClient())
                {
                    string boundary = "---------------------" + DateTime.Now.Ticks.ToString("x");

                    var httpContent = new HttpMultipartFormDataContent(boundary);

                    foreach (var item in toPost)
                    {
                        httpContent.Add(new HttpStringContent(item.Value.ToString(), Windows.Storage.Streams.UnicodeEncoding.Utf8), item.Key);
                    }

                    var imageContent = new HttpBufferContent(WindowsRuntimeBufferExtensions.AsBuffer(buffer));
                    httpContent.Add(imageContent, fieldname, filename);

                    var response = await client.PostAsync(new Uri(url), httpContent).AsTask(cts.Token);
                    var buf = await response.Content.ReadAsBufferAsync();
                    byte[] bytes = WindowsRuntimeBufferExtensions.ToArray(buf, 0, (int)buf.Length);
                    result = gbk.GetString(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                cts.Cancel();
                await new MessageDialog(ex.Message + "\n\n请尝试刷新或检查网络连接是否正常！", "POSTFILE请求失败").ShowAsync();
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

        /// <summary>
        /// 将字符串由UTF8转为GB2312，再进行UrlEncode编码
        /// 用于POST中文字符数据
        /// </summary>
        /// <param name="str">要编码的字符串</param>
        /// <returns>编码后的字符串</returns>
        public string GetEncoding(string str)
        {
            Encoding ut = Encoding.UTF8;
            var gb = DBCSEncoding.GetDBCSEncoding("gb2312");
            byte[] utbyte = ut.GetBytes(str);
            byte[] gbbyte = Encoding.Convert(ut, gb, utbyte);
            byte[] enn = WebUtility.UrlEncodeToBytes(gbbyte, 0, gbbyte.Length);
            char[] gbChars = new char[ut.GetCharCount(enn, 0, enn.Length)];
            ut.GetChars(enn, 0, enn.Length, gbChars, 0);
            string result = new string(gbChars);
            return result;
        }
    }
}
