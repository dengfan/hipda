using DBCSCodePage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;

namespace hipda
{
    class HttpHandle
    {
        private CookieContainer cookieJar = new CookieContainer();
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
                return cookieJar.Count > 0;
            }
        }

        public void ClearCookies()
        {
            cookieJar = new CookieContainer();
        }

        //public async Task<string> GetAsync(string url)
        //{
        //    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
        //    request.Method = "GET";
        //    request.CookieContainer = cookieJar;
        //    var response = (HttpWebResponse)(await request.GetResponseAsync());
        //    byte[] data = new byte[1024000];
        //    int length = 0;
        //    int cnt = 1;
        //    while (cnt > 0)
        //    {
        //        cnt = response.GetResponseStream().Read(data, length, 1024000 - length);
        //        length += cnt;
        //    }
        //    Encoding ut = Encoding.UTF8;
        //    Encoding encode = this.encode;
        //    Match m = Regex.Match(response.ContentType, @"charset=([\w\-]+)");
        //    string charset = m.Groups[1].ToString().ToLower();
        //    if (!string.IsNullOrEmpty(charset))
        //    {
        //        if (charset == "utf-8")
        //        {
        //            encode = null;
        //        }
        //        else if (charset == "gbk" || charset == "gb2312")
        //        {
        //            encode = gbk;
        //        }
        //    }
        //    if (encode != null)
        //    {
        //        byte[] utbyte = Encoding.Convert(encode, ut, data, 0, length);
        //        char[] utChars = new char[encode.GetCharCount(utbyte, 0, utbyte.Length)];
        //        ut.GetChars(utbyte, 0, utbyte.Length, utChars, 0);
        //        string res = new string(utChars);
        //        return res;
        //    }
        //    else
        //    {
        //        char[] utChars = new char[ut.GetCharCount(data, 0, length)];
        //        ut.GetChars(data, 0, length, utChars, 0);
        //        string res = new string(utChars);
        //        return res;
        //    }
        //}

        public async Task<string> GetAsync(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.CookieContainer = cookieJar;

            // TODO: 异常出现的需要重新登录的情况

            try
            {
                using (var response = (HttpWebResponse)(await request.GetResponseAsync()))
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var sr = new StreamReader(responseStream, DBCSEncoding.GetDBCSEncoding("gb2312")))
                        {
                            var result = sr.ReadToEnd();

                            // 有的时候，会出现重新登录的问题
                            if (result.Contains("未登录"))
                                return null;

                            return result;
                        }
                    }
                }
            }
            catch (WebException)
            {
                // 缺少有限的网络连接
                return null;
            }
        }

        /// <summary>
        /// 向服务器Post Data
        /// </summary>
        /// <param name="url">指定页面的地址</param>
        /// <param name="toPost">需要Post的数据</param>
        /// <returns>结果字符串</returns>
        public async Task<string> PostAsync(string url, IDictionary<string, object> toPost)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.CookieContainer = cookieJar;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            string postData = "";
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
            Encoding ut = Encoding.UTF8;
            byte[] byte1 = ut.GetBytes(postData);
            using (Stream newStream = await request.GetRequestStreamAsync())
            {
                await newStream.WriteAsync(byte1, 0, byte1.Length);
            }

            var response = (HttpWebResponse)(await request.GetResponseAsync());
            byte[] data = new byte[1024000];
            int length = 0;
            int cnt = 1;
            while (cnt > 0)
            {
                cnt = response.GetResponseStream().Read(data, length, 1024000 - length);
                length += cnt;
            }
            Encoding encode = this.encode;
            Match m = Regex.Match(response.ContentType, @"charset=([\w\-]+)");
            string charset = m.Groups[1].ToString().ToLower();
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

        /// <summary>
        /// 向服务器Post Data
        /// </summary>
        /// <param name="url">指定页面的地址</param>
        /// <param name="queryString">需要Post的数据</param>
        /// <returns>结果字符串</returns>
        public async Task<string> PostAsync(string url, string queryString)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.CookieContainer = cookieJar;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            Encoding ut = Encoding.UTF8;
            byte[] byte1 = ut.GetBytes(queryString);
            using (Stream newStream = await request.GetRequestStreamAsync())
            {
                await newStream.WriteAsync(byte1, 0, byte1.Length);
            }

            var response = (HttpWebResponse)(await request.GetResponseAsync());
            byte[] data = new byte[1024000];
            int length = 0;
            int cnt = 1;
            while (cnt > 0)
            {
                cnt = response.GetResponseStream().Read(data, length, 1024000 - length);
                length += cnt;
            }
            Encoding encode = this.encode;
            Match m = Regex.Match(response.ContentType, @"charset=([\w\-]+)");
            string charset = m.Groups[1].ToString().ToLower();
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

        public IAsyncOperation<string> HttpPostFile(string url, IDictionary<string, object> toPost, string filename, string filetype, string fieldname, byte[] buffer)
        {
            return Task.Run<string>(async () =>
            {
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                Encoding ascii = Encoding.UTF8;
                byte[] boundarybytes = ascii.GetBytes("\r\n--" + boundary + "\r\n");

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.CookieContainer = cookieJar;
                request.Method = "POST";
                request.ContentType = "multipart/form-data; boundary=" + boundary;
                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                using (Stream newStream = await request.GetRequestStreamAsync())
                {
                    foreach (KeyValuePair<string, object> kvp in toPost)
                    {
                        newStream.Write(boundarybytes, 0, boundarybytes.Length);
                        string formitem = string.Format(formdataTemplate, kvp.Key, kvp.Value.ToString());
                        byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                        newStream.Write(formitembytes, 0, formitembytes.Length);
                    }
                    newStream.Write(boundarybytes, 0, boundarybytes.Length);
                    string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                    string header = string.Format(headerTemplate, fieldname, filename, filetype);
                    byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                    newStream.Write(headerbytes, 0, headerbytes.Length);

                    // File binary array
                    newStream.Write(buffer, 0, buffer.Length);

                    // footer
                    byte[] trailer = ascii.GetBytes("\r\n--" + boundary + "--\r\n");
                    newStream.Write(trailer, 0, trailer.Length);
                }

                Task<WebResponse> taskresponse = request.GetResponseAsync();
                HttpWebResponse response = (HttpWebResponse)await taskresponse;
                byte[] data = new byte[1024000];
                int length = 0;
                int cnt = 1;
                while (cnt > 0)
                {
                    cnt = await response.GetResponseStream().ReadAsync(data, length, 1024000 - length);
                    length += cnt;
                }
                Encoding ut = Encoding.UTF8;
                Encoding encode = this.encode;
                Match m = Regex.Match(response.ContentType, @"charset=([\w\-]+)");
                string charset = m.Groups[1].ToString().ToLower();
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
            }).AsAsyncOperation();
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
