using DBCSCodePage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using System.Net.Http;
using System.Net.Http.Headers;

namespace hipda
{
    class HttpHandle
    {
        private CookieContainer cookieContainer = new CookieContainer();
        private Encoding encode = null;
        private Encoding gbk = null;
        private static HttpHandle instance = null;

        public static HttpHandle getInstance()
        {
            if (instance == null)
            {
                instance = new HttpHandle();
                instance.setEncoding("gb2312");
            }

            return instance;
        }

        public HttpHandle()
        {
            gbk = DBCSEncoding.GetDBCSEncoding("gb2312");
        }

        public void setEncoding(string code)
        {
            if (code == "gbk" || code == "gb2312") encode = DBCSEncoding.GetDBCSEncoding("gb2312");
            else encode = null;
        }

        public bool HasCookies
        {
            get
            {
                return cookieContainer.Count > 0;
            }
        }

        public void ClearCookies()
        {
            cookieContainer = new CookieContainer();
        }
        
        public async Task<string> GetAsync(string url)
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer = cookieContainer;
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip;
            }

            // Create a New HttpClient object.
            using (var client = new HttpClient(handler))
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions 
                try
                {
                    HttpResponseMessage responseMessage = await client.GetAsync(url);
                    Stream responseStream = await responseMessage.Content.ReadAsStreamAsync();

                    byte[] data = new byte[1024000];
                    int length = 0;
                    int cnt = 1;
                    while (cnt > 0)
                    {
                        cnt = responseStream.Read(data, length, 1024000 - length);
                        length += cnt;
                    }
                    Encoding ut = Encoding.UTF8;
                    Encoding encode = this.encode;
                    string charset = responseMessage.Content.Headers.ContentType.CharSet;
                    if (!string.IsNullOrEmpty(charset))
                    {
                        charset = charset.ToLower();
                        if (charset == "utf-8")
                        {
                            encode = null;
                        }
                        else if (charset == "gbk" || charset == "gb2312")
                        {
                            encode = gbk;
                        }
                    }
                    if (encode != null)
                    {
                        byte[] utbyte = Encoding.Convert(encode, ut, data, 0, length);
                        char[] utChars = new char[encode.GetCharCount(utbyte, 0, utbyte.Length)];
                        ut.GetChars(utbyte, 0, utbyte.Length, utChars, 0);
                        string res = new string(utChars);
                        return res;
                    }
                    else
                    {
                        char[] utChars = new char[ut.GetCharCount(data, 0, length)];
                        ut.GetChars(data, 0, length, utChars, 0);
                        string res = new string(utChars);
                        return res;
                    }
                }
                catch (HttpRequestException e)
                {

                }

                return string.Empty;
            }
        }

        public async Task<string> PostAsync(string url, IDictionary<string, object> toPost)
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer = cookieContainer;
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip;
            }

            // Create a New HttpClient object.
            using (var client = new HttpClient(handler))
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions 
                try
                {
                    string postData = string.Empty;
                    bool first = true;
                    foreach (KeyValuePair<string, object> kvp in toPost)
                    {
                        if (first)
                        {
                            first = false;
                            postData += GetEncoding(kvp.Key) + "=" + GetEncoding(kvp.Value.ToString());
                        }
                        else
                        {
                            postData += "&" + GetEncoding(kvp.Key) + "=" + GetEncoding(kvp.Value.ToString());
                        }
                    }
                    HttpContent httpContent = new StringContent(postData, Encoding.UTF8);

                    //var dic = new Dictionary<string, string>();
                    //foreach (var item in toPost)
                    //{
                    //    dic.Add(item.Key, item.Value.ToString());
                    //}
                    //HttpContent httpContent = new FormUrlEncodedContent(dic);

                    httpContent.Headers.ContentType.CharSet = "utf-8";
                    httpContent.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";
                    HttpResponseMessage responseMessage = await client.PostAsync(url, httpContent);
                    Stream responseStream = await responseMessage.Content.ReadAsStreamAsync();

                    byte[] data = new byte[1024000];
                    int length = 0;
                    int cnt = 1;
                    while (cnt > 0)
                    {
                        cnt = responseStream.Read(data, length, 1024000 - length);
                        length += cnt;
                    }
                    Encoding ut = Encoding.UTF8;
                    Encoding encode = this.encode;
                    string charset = responseMessage.Content.Headers.ContentType.CharSet;
                    if (!string.IsNullOrEmpty(charset))
                    {
                        if (charset == "utf-8")
                        {
                            encode = null;
                        }
                        else if (charset == "gbk" || charset == "gb2312")
                        {
                            encode = gbk;
                        }
                    }

                    if (encode != null)
                    {
                        byte[] utbyte = Encoding.Convert(encode, ut, data, 0, length);
                        char[] utChars = new char[encode.GetCharCount(utbyte, 0, utbyte.Length)];
                        ut.GetChars(utbyte, 0, utbyte.Length, utChars, 0);
                        return new string(utChars);
                    }
                    else
                    {
                        char[] utChars = new char[ut.GetCharCount(data, 0, length)];
                        ut.GetChars(data, 0, length, utChars, 0);
                        return new string(utChars);
                    }
                }
                catch (HttpRequestException e)
                {

                }

                return string.Empty;
            }
        }

        //public IAsyncOperation<string> HttpPostFile(string url, IDictionary<string, object> toPost, string filename, string filetype, string fieldname, byte[] buffer)
        //{
        //    return Task.Run<string>(async () =>
        //    {
        //        string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
        //        Encoding ascii = Encoding.UTF8;
        //        byte[] boundarybytes = ascii.GetBytes("\r\n--" + boundary + "\r\n");

        //        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
        //        request.CookieContainer = cookieContainer;
        //        request.Method = "POST";
        //        request.ContentType = "multipart/form-data; boundary=" + boundary;
        //        string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
        //        using (Stream newStream = await request.GetRequestStreamAsync())
        //        {
        //            foreach (KeyValuePair<string, object> kvp in toPost)
        //            {
        //                newStream.Write(boundarybytes, 0, boundarybytes.Length);
        //                string formitem = string.Format(formdataTemplate, kvp.Key, kvp.Value.ToString());
        //                byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
        //                newStream.Write(formitembytes, 0, formitembytes.Length);
        //            }
        //            newStream.Write(boundarybytes, 0, boundarybytes.Length);
        //            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
        //            string header = string.Format(headerTemplate, fieldname, filename, filetype);
        //            byte[] headerbytes = Encoding.UTF8.GetBytes(header);
        //            newStream.Write(headerbytes, 0, headerbytes.Length);

        //            // File binary array
        //            newStream.Write(buffer, 0, buffer.Length);

        //            // footer
        //            byte[] trailer = ascii.GetBytes("\r\n--" + boundary + "--\r\n");
        //            newStream.Write(trailer, 0, trailer.Length);
        //        }

        //        Task<WebResponse> taskresponse = request.GetResponseAsync();
        //        HttpWebResponse response = (HttpWebResponse)await taskresponse;
        //        byte[] data = new byte[1024000];
        //        int length = 0;
        //        int cnt = 1;
        //        while (cnt > 0)
        //        {
        //            cnt = await response.GetResponseStream().ReadAsync(data, length, 1024000 - length);
        //            length += cnt;
        //        }
        //        Encoding ut = Encoding.UTF8;
        //        Encoding encode = this.encode;
        //        Match m = Regex.Match(response.ContentType, @"charset=([\w\-]+)");
        //        string charset = m.Groups[1].ToString().ToLower();
        //        if (!string.IsNullOrEmpty(charset))
        //        {
        //            if (charset == "utf-8")
        //            {
        //                encode = null;
        //            }
        //            else if (charset == "gbk" || charset == "gb2312")
        //            {
        //                encode = gbk;
        //            }
        //        }
        //        if (encode != null)
        //        {
        //            byte[] utbyte = Encoding.Convert(encode, ut, data, 0, length);
        //            char[] utChars = new char[encode.GetCharCount(utbyte, 0, utbyte.Length)];
        //            ut.GetChars(utbyte, 0, utbyte.Length, utChars, 0);
        //            string res = new string(utChars);
        //            return res;
        //        }
        //        else
        //        {
        //            char[] utChars = new char[ut.GetCharCount(data, 0, length)];
        //            ut.GetChars(data, 0, length, utChars, 0);
        //            string res = new string(utChars);
        //            return res;
        //        }
        //    }).AsAsyncOperation();
        //}

        public async Task<string> HttpPostFile(string url, IDictionary<string, object> toPost, string filename, string filetype, string fieldname, byte[] buffer)
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer = cookieContainer;
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip;
            }

            // Create a New HttpClient object.
            using (var client = new HttpClient(handler))
            {
                // Call asynchronous network methods in a try/catch block to handle exceptions 
                try
                {
                    string boundary = "---------------------" + DateTime.Now.Ticks.ToString("x");

                    var httpContent = new MultipartFormDataContent(boundary);

                    foreach (var item in toPost)
                    {
                        httpContent.Add(new StringContent(item.Value.ToString(), Encoding.UTF8), item.Key);
                    }

                    var imageContent = new ByteArrayContent(buffer);
                    httpContent.Add(imageContent, fieldname, filename);

                    HttpResponseMessage responseMessage = await client.PostAsync(url, httpContent);
                    Stream responseStream = await responseMessage.Content.ReadAsStreamAsync();

                    byte[] data = new byte[1024000];
                    int length = 0;
                    int cnt = 1;
                    while (cnt > 0)
                    {
                        cnt = responseStream.Read(data, length, 1024000 - length);
                        length += cnt;
                    }
                    Encoding ut = Encoding.UTF8;
                    Encoding encode = this.encode;
                    string charset = responseMessage.Content.Headers.ContentType.CharSet;
                    if (!string.IsNullOrEmpty(charset))
                    {
                        if (charset == "utf-8")
                        {
                            encode = null;
                        }
                        else if (charset == "gbk" || charset == "gb2312")
                        {
                            encode = gbk;
                        }
                    }

                    if (encode != null)
                    {
                        byte[] utbyte = Encoding.Convert(encode, ut, data, 0, length);
                        char[] utChars = new char[encode.GetCharCount(utbyte, 0, utbyte.Length)];
                        ut.GetChars(utbyte, 0, utbyte.Length, utChars, 0);
                        return new string(utChars);
                    }
                    else
                    {
                        char[] utChars = new char[ut.GetCharCount(data, 0, length)];
                        ut.GetChars(data, 0, length, utChars, 0);
                        return new string(utChars);
                    }
                }
                catch (HttpRequestException e)
                {

                }

                return string.Empty;
            }
        }

        public string GetEncoding(string wgg)
        {
            if (encode != null)
            {
                Encoding ut = Encoding.UTF8;
                byte[] utbyte = ut.GetBytes(wgg);
                byte[] gbbyte = Encoding.Convert(ut, encode, utbyte);
                byte[] enn = WebUtility.UrlEncodeToBytes(gbbyte, 0, gbbyte.Length);
                char[] gbChars = new char[ut.GetCharCount(enn, 0, enn.Length)];
                ut.GetChars(enn, 0, enn.Length, gbChars, 0);
                string res = new string(gbChars);
                return res;
            }
            else
            {
                return WebUtility.UrlEncode(wgg);
            }
        }
    }
}
