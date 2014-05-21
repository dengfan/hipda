using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Diagnostics;
namespace hipda
{
    public static class HttpExtensions
    {
        public static Task<WebResponse> GetResponseAsync(this HttpWebRequest request)
        {
            var taskComplete = new TaskCompletionSource<WebResponse>();
            request.BeginGetResponse(asyncResponse =>
            {
                try
                {
                    HttpWebRequest responseRequest = (HttpWebRequest)asyncResponse.AsyncState;
                    WebResponse someResponse = responseRequest.EndGetResponse(asyncResponse);
                    taskComplete.TrySetResult(someResponse);
                }
                catch (WebException webExc)
                {
                    Debug.WriteLine(webExc.Message);
                    WebResponse failedResponse = webExc.Response;
                    taskComplete.TrySetResult(failedResponse);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }, request);
            return taskComplete.Task;
        }
        public static Task<Stream> GetRequestStreamAsync(this HttpWebRequest request)
        {
            var taskComplete = new TaskCompletionSource<Stream>();
            request.BeginGetRequestStream(asyncResponse =>
            {
                try
                {
                    HttpWebRequest responseRequest = (HttpWebRequest)asyncResponse.AsyncState;
                    Stream someStream = responseRequest.EndGetRequestStream(asyncResponse);
                    taskComplete.TrySetResult(someStream);
                }
                catch (WebException webExc)
                {
                    taskComplete.SetException(webExc);
                }
            }, request);
            return taskComplete.Task;
        }
    }

    public static class HttpMethod
    {
        public static string Head { get { return "HEAD"; } }
        public static string Post { get { return "POST"; } }
        public static string Put { get { return "PUT"; } }
        public static string Get { get { return "GET"; } }
        public static string Delete { get { return "DELETE"; } }
        public static string Trace { get { return "TRACE"; } }
        public static string Options { get { return "OPTIONS"; } }
        public static string Connect { get { return "CONNECT"; } }
        public static string Patch { get { return "PATCH"; } }
    }
}
