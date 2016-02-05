using Hipda.Http;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace Hipda.Client.Uwp.Pro.Services
{
    public static class SendService
    {
        static HttpHandle _httpClient = HttpHandle.GetInstance();
        static string _messageTail = "\r\n \r\n[img=16,16]http://www.hi-pda.com/forum/attachments/day_140621/1406211752793e731a4fec8f7b.png[/img]";

        public static async Task SendEditPost(CancellationTokenSource cts, string title, string content, List<string> fileNameList, int postId, int threadId)
        {
            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("subject", title));
            postData.Add(new KeyValuePair<string, object>("message", $"{content}{_messageTail}"));
            postData.Add(new KeyValuePair<string, object>("wysiwyg", "1"));
            postData.Add(new KeyValuePair<string, object>("tid", threadId));
            postData.Add(new KeyValuePair<string, object>("pid", postId));
            postData.Add(new KeyValuePair<string, object>("iconid", "0"));

            // 图片信息
            foreach (var fileName in fileNameList)
            {
                postData.Add(new KeyValuePair<string, object>(string.Format("attachnew[{0}][description]", fileName), string.Empty));
            }

            string url = string.Format("http://www.hi-pda.com/forum/post.php?action=edit&extra=&editsubmit=yes&mod=", threadId);
            await _httpClient.PostAsync(url, postData, cts);
        }

        public static async Task<string[]> LoadContentForEditAsync(CancellationTokenSource cts, int postId, int threadId)
        {
            string url = $"http://www.hi-pda.com/forum/post.php?action=edit&tid={threadId}&pid={postId}&_={DateTime.Now.Ticks.ToString("x")}";
            string htmlContent = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            // 标题
            string subject = string.Empty;
            var subjectInput = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("input") && n.GetAttributeValue("prompt", "").Equals("post_subject"));
            if (subjectInput != null)
            {
                subject = subjectInput.Attributes["value"].Value;
            }

            // 内容
            string content = string.Empty;
            var contentTextArea = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("textarea") && n.GetAttributeValue("prompt", "").Equals("post_message"));
            if (contentTextArea != null)
            {
                content = contentTextArea.InnerText.Replace(_messageTail, string.Empty).TrimEnd();
            }

            return new string[] { subject, content };
        }

        public static async Task<bool> SendPostReply(CancellationTokenSource cts, string noticeauthor, string noticetrimstr, string noticeauthormsg, string content, List<string> fileNameList, int threadId)
        {
            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("wysiwyg", "1"));
            postData.Add(new KeyValuePair<string, object>("noticeauthor", "0"));
            postData.Add(new KeyValuePair<string, object>("noticetrimstr", "0"));
            postData.Add(new KeyValuePair<string, object>("noticeauthormsg", "0"));
            postData.Add(new KeyValuePair<string, object>("subject", string.Empty));
            postData.Add(new KeyValuePair<string, object>("message", $"{noticetrimstr}{content.Trim()}{_messageTail}"));

            // 图片信息
            foreach (var fileName in fileNameList)
            {
                postData.Add(new KeyValuePair<string, object>(string.Format("attachnew[{0}][description]", fileName), string.Empty));
            }

            string url = string.Format("http://www.hi-pda.com/forum/post.php?action=reply&tid={0}&replysubmit=yes&inajax=1", threadId);
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            return resultContent.Contains("您的回复已经发布");
        }

        public static async Task<bool> SendNewThread(CancellationTokenSource cts, string title, string content, List<string> fileNameList, int forumId)
        {
            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("wysiwyg", "1"));
            postData.Add(new KeyValuePair<string, object>("iconid", "0"));
            postData.Add(new KeyValuePair<string, object>("subject", title));
            postData.Add(new KeyValuePair<string, object>("message", $"{content.Trim()}{_messageTail}"));
            postData.Add(new KeyValuePair<string, object>("attention_add", "1"));
            postData.Add(new KeyValuePair<string, object>("usesig", "1"));

            // 图片信息
            foreach (var fileName in fileNameList)
            {
                postData.Add(new KeyValuePair<string, object>(string.Format("attachnew[{0}][description]", fileName), string.Empty));
            }

            string url = string.Format("http://www.hi-pda.com/forum/post.php?action=newthread&fid={0}&extra=&topicsubmit=yes", forumId);
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            return !resultContent.Contains("对不起，您两次发表间隔少于");
        }

        public static async Task<bool> SendThreadReply(CancellationTokenSource cts, string content, List<string> fileNameList, int threadId)
        {
            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("subject", string.Empty));
            postData.Add(new KeyValuePair<string, object>("usesig", "0"));
            postData.Add(new KeyValuePair<string, object>("message", $"{content.Trim()}{_messageTail}"));

            // 图片信息
            foreach (var fileName in fileNameList)
            {
                postData.Add(new KeyValuePair<string, object>(string.Format("attachnew[{0}][description]", fileName), string.Empty));
            }

            string url = string.Format("http://www.hi-pda.com/forum/post.php?action=reply&tid={0}&replysubmit=yes&infloat=yes&handlekey=fastpost&inajax=1", threadId);
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            return resultContent.Contains("您的回复已经发布");
        }

        public static async Task<List<string>[]> UploadFiles(CancellationTokenSource cts, Action<int, int, string> beforeUpload, Action<int> afterUpload)
        {
            var fileNameList = new List<string>();
            var fileCodeList = new List<string>();

            var openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add("*");

            var files = await openPicker.PickMultipleFilesAsync();
            if (files != null)
            {
                int fileIndex = 1;
                foreach (var file in files)
                {
                    string fileName = file.Name;

                    if (beforeUpload != null)
                    {
                        beforeUpload(fileIndex, files.Count, fileName);
                    }
                    
                    fileNameList.Add(fileName);

                    byte[] imageBuffer;

                    //if (deviceFamily.Equals("Windows.Mobile"))
                    //{
                    //    imageBuffer = await ImageHelper.LoadAsync(file);
                    //}
                    //else
                    //{
                    IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                    IBuffer buffer = new Windows.Storage.Streams.Buffer((uint)stream.Size);
                    buffer = await stream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);
                    imageBuffer = WindowsRuntimeBufferExtensions.ToArray(buffer, 0, (int)buffer.Length);
                    //}

                    var data = new Dictionary<string, object>();
                    data.Add("uid", AccountService.UserId);
                    data.Add("hash", AccountService.Hash);

                    try
                    {
                        string result = await _httpClient.PostFileAsync("http://www.hi-pda.com/forum/misc.php?action=swfupload&operation=upload&simple=1&type=image", data, fileName, "image/jpg", "Filedata", imageBuffer, cts);
                        if (result.Contains("DISCUZUPLOAD|"))
                        {
                            string value = result.Split('|')[2];
                            value = string.Format("[attachimg]{0}[/attachimg]", value);
                            fileCodeList.Add(value);

                            fileIndex++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                if (afterUpload != null)
                {
                    afterUpload(files.Count);
                }
            }

            return new List<string>[] { fileNameList, fileCodeList };
        }

        public static async Task SendAddToFavoritesAction(CancellationTokenSource cts, int threadId, string threadTitle)
        {
            string url = $"http://www.hi-pda.com/forum/my.php?item=favorites&tid={threadId}&inajax=1";
            await _httpClient.GetAsync(url, cts);

            string xml = "<toast>" +
                        "<visual>" +
                            "<binding template='ToastGeneric'>" +
                                $"<text>恭喜您，主题《{threadTitle}》</text>" +
                                "<text>已收藏成功</text>" +
                            "</binding>" +
                        "</visual>" +
                        "</toast>";
            Common.SendToast(xml);
        }

        public static async Task SendAddBuddyAction(CancellationTokenSource cts, int userId, string username)
        {
            string url = $"http://www.hi-pda.com/forum/my.php?item=buddylist&newbuddyid={userId}&buddysubmit=yes&inajax=1";
            await _httpClient.GetAsync(url, cts);

            string xml = "<toast>" +
                        "<visual>" +
                            "<binding template='ToastGeneric'>" +
                                $"<text>恭喜您，已添加 {username} 为好友</text>" +
                                $"<text>已操作成功</text>" +
                            "</binding>" +
                        "</visual>" +
                        "</toast>";
            Common.SendToast(xml);
        }

    }
}
