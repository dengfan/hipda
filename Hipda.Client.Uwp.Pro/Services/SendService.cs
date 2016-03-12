﻿using Newtonsoft.Json;
using Hipda.Client.Uwp.Pro.Models;
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
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Hipda.Client.Uwp.Pro.Services
{
    public static class SendService
    {
        static HttpHandle _httpClient = HttpHandle.GetInstance();
        static string _messageTail = "\r\n \r\n[img=16,16]http://www.hi-pda.com/forum/attachments/day_140621/1406211752793e731a4fec8f7b.png[/img]";

        public static async Task SendEditPostAsync(CancellationTokenSource cts, string title, string content, List<string> fileNameList, int postId, int threadId)
        {
            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("subject", title));
            postData.Add(new KeyValuePair<string, object>("message", $"{CommonService.ReplaceFaceLabel(content)}{_messageTail}"));
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

        public static async Task<List<AttachImageItemModel>> LoadUnusedImageAttachListAsync(CancellationTokenSource cts)
        {
            var url = $"http://www.hi-pda.com/forum/ajax.php?action=imagelist&inajax=1&ajaxtarget=imgattachlist";
            string htmlContent = await _httpClient.GetAsync(url, cts);

            var data = new List<AttachImageItemModel>();
            var matchs = new Regex("<img src=\"([^\"]*)\" id=\"([^\"]*)\"").Matches(htmlContent);
            if (matchs != null && matchs.Count > 0)
            {
                for (int i = 0; i < matchs.Count; i++)
                {
                    var m = matchs[i];
                    string src = $"http://www.hi-pda.com/forum/{m.Groups[1].Value}";
                    string id = m.Groups[2].Value.Replace("image_", string.Empty);
                    data.Add(new AttachImageItemModel(id, src));
                }
            }

            return data;
        }

        public static async Task<PostEditDataModel> LoadContentForEditAsync(CancellationTokenSource cts, int postId, int threadId)
        {
            string url = $"http://www.hi-pda.com/forum/post.php?action=edit&tid={threadId}&pid={postId}&_={DateTime.Now.Ticks.ToString("x")}";
            string htmlContent = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlContent);

            // 标题
            string title = string.Empty;
            var subjectInput = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("input") && n.GetAttributeValue("prompt", string.Empty).Equals("post_subject"));
            if (subjectInput != null)
            {
                title = subjectInput.Attributes["value"].Value;
            }

            // 内容
            string content = string.Empty;
            var contentTextArea = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("textarea") && n.GetAttributeValue("prompt", string.Empty).Equals("post_message"));
            if (contentTextArea != null)
            {
                content = contentTextArea.InnerText.Replace(_messageTail, string.Empty).TrimEnd();
            }

            // 图片附件
            var attachImageList = new ObservableCollection<AttachImageItemModel>();
            var imgList = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("table") && n.GetAttributeValue("class", string.Empty).Equals("imglist"));
            if (imgList != null)
            {
                var images = imgList.Descendants().Where(n => n.Name.Equals("img") && n.GetAttributeValue("src", string.Empty).StartsWith("attachments/day_") && n.GetAttributeValue("id", string.Empty).StartsWith("image_"));
                if (images != null)
                {
                    foreach (var img in images)
                    {
                        string id = img.GetAttributeValue("id", string.Empty).Replace("image_", string.Empty);
                        string src = img.GetAttributeValue("src", string.Empty);
                        attachImageList.Add(new AttachImageItemModel(id, $"http://www.hi-pda.com/forum/{src}"));
                    }
                }
            }

            return new PostEditDataModel(postId, threadId, title, content, attachImageList);
        }

        public static async Task<bool> SendPostReplyAsync(CancellationTokenSource cts, string noticeauthor, string noticetrimstr, string noticeauthormsg, string content, List<string> fileNameList, int threadId)
        {
            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("wysiwyg", "1"));
            postData.Add(new KeyValuePair<string, object>("noticeauthor", "0"));
            postData.Add(new KeyValuePair<string, object>("noticetrimstr", "0"));
            postData.Add(new KeyValuePair<string, object>("noticeauthormsg", "0"));
            postData.Add(new KeyValuePair<string, object>("subject", string.Empty));
            postData.Add(new KeyValuePair<string, object>("message", $"{noticetrimstr}{CommonService.ReplaceFaceLabel(content.Trim())}{_messageTail}"));

            // 图片信息
            foreach (var fileName in fileNameList)
            {
                postData.Add(new KeyValuePair<string, object>(string.Format("attachnew[{0}][description]", fileName), string.Empty));
            }

            string url = string.Format("http://www.hi-pda.com/forum/post.php?action=reply&tid={0}&replysubmit=yes&inajax=1", threadId);
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            return resultContent.Contains("您的回复已经发布");
        }

        public static async Task<bool> SendNewThreadAsync(CancellationTokenSource cts, string title, string content, List<string> fileNameList, int forumId)
        {
            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("wysiwyg", "1"));
            postData.Add(new KeyValuePair<string, object>("iconid", "0"));
            postData.Add(new KeyValuePair<string, object>("subject", title));
            postData.Add(new KeyValuePair<string, object>("message", $"{CommonService.ReplaceFaceLabel(content.Trim())}{_messageTail}"));
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

        public static async Task<bool> SendThreadReplyAsync(CancellationTokenSource cts, string content, List<string> fileNameList, int threadId)
        {
            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("subject", string.Empty));
            postData.Add(new KeyValuePair<string, object>("usesig", "0"));
            postData.Add(new KeyValuePair<string, object>("message", $"{CommonService.ReplaceFaceLabel(content.Trim())}{_messageTail}"));

            // 图片信息
            foreach (var fileName in fileNameList)
            {
                postData.Add(new KeyValuePair<string, object>(string.Format("attachnew[{0}][description]", fileName), string.Empty));
            }

            string url = string.Format("http://www.hi-pda.com/forum/post.php?action=reply&tid={0}&replysubmit=yes&infloat=yes&handlekey=fastpost&inajax=1", threadId);
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            return resultContent.Contains("您的回复已经发布");
        }

        public static async Task<List<string>[]> UploadFileAsync(CancellationTokenSource cts, Action<int, int, string> beforeUpload, Action<int> afterUpload)
        {
            var openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add("*");

            var files = await openPicker.PickMultipleFilesAsync();
            return await UploadFileAsync(cts, files, beforeUpload, afterUpload);
        }

        public static async Task<List<string>[]> UploadFileAsync(CancellationTokenSource cts, IReadOnlyList<IStorageItem> files, Action<int, int, string> beforeUpload, Action<int> afterUpload)
        {
            var fileNameList = new List<string>();
            var fileCodeList = new List<string>();

            if (files != null && files.Count > 0)
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

                    //byte[] imageBuffer;

                    //if (deviceFamily.Equals("Windows.Mobile"))
                    //{
                    //    imageBuffer = await ImageHelper.LoadAsync(file);
                    //}
                    //else
                    //{
                    IRandomAccessStream stream = await ((StorageFile)file).OpenAsync(FileAccessMode.Read);
                    IBuffer buffer = new Windows.Storage.Streams.Buffer((uint)stream.Size);
                    buffer = await stream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);
                    //}

                    var data = new Dictionary<string, object>();
                    data.Add("uid", AccountService.UserId);
                    data.Add("hash", AccountService.Hash);

                    try
                    {
                        string result = await _httpClient.PostFileAsync("http://www.hi-pda.com/forum/misc.php?action=swfupload&operation=upload&simple=1&type=image", data, fileName, "Filedata", buffer, cts);
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

        public static async Task SendAddToFavoritesActionAsync(CancellationTokenSource cts, int threadId, string threadTitle)
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
            CommonService.SendToast(xml);
        }

        public static async Task SendAddBuddyActionAsync(CancellationTokenSource cts, int userId, string username)
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
            CommonService.SendToast(xml);
        }

        public static List<string[]> GetCategory(int forumId)
        {
            switch (forumId)
            {
                case 2:
                    return new List<string[]>
                    {
                        new string[] { "聚会", "9" },
                        new string[] { "汽车", "33" },
                        new string[] { "大杂烩", "38" },
                        new string[] { "助学", "40" },
                        new string[] { "Discovery", "56" },
                        new string[] { "投资", "57" },
                        new string[] { "职场", "58" },
                        new string[] { "文艺", "65" },
                        new string[] { "版喃", "66" },
                        new string[] { "显摆", "67" },
                        new string[] { "晒物劝败", "79" },
                        new string[] { "装修", "81" },
                        new string[] { "YY", "39" },
                        new string[] { "站务", "19" },
                    };
                case 6:
                    return new List<string[]>
                    {
                        new string[] { "手机", "1" },
                        new string[] { "掌上电脑", "2" },
                        new string[] { "笔记本电脑", "3" },
                        new string[] { "无线产品", "4" },
                        new string[] { "数码相机、摄像机", "5" },
                        new string[] { "MP3随身听", "6" },
                        new string[] { "各类配件", "7" },
                        new string[] { "其他好玩的", "8" },
                        new string[] { "站务", "19" },
                    };
                case 59:
                    return new List<string[]>
                    {
                        new string[] { "Kindle", "68" },
                        new string[] { "SONY", "69" },
                        new string[] { "国产", "70" },
                        new string[] { "资源", "72" },
                        new string[] { "综合", "73" },
                        new string[] { "交流", "75" },
                        new string[] { "Nook", "77" },
                        new string[] { "Kobo", "80" },
                        new string[] { "求助", "18" },
                        new string[] { "站务", "19" },
                    };
                default:
                    return null;
            }
        }
    }
}
