﻿using Newtonsoft.Json;
using Hipda.Client.Models;
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

namespace Hipda.Client.Services
{
    public static class SendService
    {
        static HttpHandle _httpClient = HttpHandle.GetInstance();
        public static string MessageTail = "\r\n \r\n[img=16,16]http://www.hi-pda.com/forum/attachments/day_140621/1406211752793e731a4fec8f7b.png[/img]";

        public static async Task SendEditPostAsync(CancellationTokenSource cts, string title, string content, List<string> fileNameList, List<string> fileRemoveList, int postId, int threadId)
        {
            title = CommonService.MyHtmlEncodeForSend(title);
            content = CommonService.ReplaceFaceLabel(content);
            content = CommonService.MyHtmlEncodeForSend(content);

            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("subject", title));
            postData.Add(new KeyValuePair<string, object>("message", $"{content.Trim()}{MessageTail}"));
            postData.Add(new KeyValuePair<string, object>("wysiwyg", "1"));
            postData.Add(new KeyValuePair<string, object>("usesig", "1"));
            postData.Add(new KeyValuePair<string, object>("tid", threadId));
            postData.Add(new KeyValuePair<string, object>("pid", postId));
            postData.Add(new KeyValuePair<string, object>("iconid", "0"));

            // 添加附件
            foreach (var fileId in fileNameList)
            {
                postData.Add(new KeyValuePair<string, object>(string.Format("attachnew[{0}][description]", fileId), string.Empty));
            }

            // 删除附件
            foreach (var fileId in fileRemoveList)
            {
                postData.Add(new KeyValuePair<string, object>("attachdel[]", fileId));
            }

            string url = string.Format("http://www.hi-pda.com/forum/post.php?action=edit&extra=&editsubmit=yes&mod=", threadId);
            await _httpClient.PostAsync(url, postData, cts);
        }

        

        public static async Task<List<AttachFileItemModel>> LoadUnusedAttachFilesAsync(CancellationTokenSource cts)
        {
            var data = new List<AttachFileItemModel>();

            var url = $"http://www.hi-pda.com/forum/ajax.php?action=imagelist&inajax=1";
            string htmlContent = await _httpClient.GetAsync(url, cts);
            var matchs = new Regex("<img src=\"([^\"]*)\" id=\"([^\"]*)\"").Matches(htmlContent);
            if (matchs != null && matchs.Count > 0)
            {
                for (int i = 0; i < matchs.Count; i++)
                {
                    var m = matchs[i];
                    string id = m.Groups[2].Value.Replace("image_", string.Empty);
                    string src = m.Groups[1].Value;
                    //src = $"http://www.hi-pda.com/forum/{src}";
                    data.Add(new AttachFileItemModel(0, id, src, false));
                }
            }

            url = $"http://www.hi-pda.com/forum/ajax.php?action=attachlist&inajax=1";
            htmlContent = await _httpClient.GetAsync(url, cts);
            matchs = new Regex("onclick=\"(insertAttachTag|insertAttachimgTag)\\('(\\d*)'\\)\" title=\"([^\"]*)\n上传日期").Matches(htmlContent);
            if (matchs != null && matchs.Count > 0)
            {
                for (int i = 0; i < matchs.Count; i++)
                {
                    var m = matchs[i];
                    string id = m.Groups[2].Value;
                    string fileName = m.Groups[3].Value.Trim();
                    data.Add(new AttachFileItemModel(1, id, fileName, false));
                }
            }

            return data;
        }

        public static async Task<bool> SendPostReplyAsync(CancellationTokenSource cts, string noticeauthor, string noticetrimstr, string noticeauthormsg, string content, List<string> fileNameList, int threadId)
        {
            content = CommonService.ReplaceFaceLabel(content);
            content = CommonService.MyHtmlEncodeForSend(content);

            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("wysiwyg", "1"));
            postData.Add(new KeyValuePair<string, object>("usesig", "1"));
            postData.Add(new KeyValuePair<string, object>("noticeauthor", "0"));
            postData.Add(new KeyValuePair<string, object>("noticetrimstr", "0"));
            postData.Add(new KeyValuePair<string, object>("noticeauthormsg", "0"));
            postData.Add(new KeyValuePair<string, object>("subject", string.Empty));
            postData.Add(new KeyValuePair<string, object>("message", $"{noticetrimstr}{content.Trim()}{MessageTail}"));

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
            title = CommonService.MyHtmlEncodeForSend(title);
            content = CommonService.ReplaceFaceLabel(content);
            content = CommonService.MyHtmlEncodeForSend(content);

            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("wysiwyg", "1"));
            postData.Add(new KeyValuePair<string, object>("usesig", "1"));
            postData.Add(new KeyValuePair<string, object>("iconid", "0"));
            postData.Add(new KeyValuePair<string, object>("subject", title));
            postData.Add(new KeyValuePair<string, object>("message", $"{content.Trim()}{MessageTail}"));
            postData.Add(new KeyValuePair<string, object>("attention_add", "1"));

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
            content = CommonService.ReplaceFaceLabel(content);
            content = CommonService.MyHtmlEncodeForSend(content);

            var postData = new List<KeyValuePair<string, object>>();
            postData.Add(new KeyValuePair<string, object>("formhash", AccountService.FormHash));
            postData.Add(new KeyValuePair<string, object>("subject", string.Empty));
            postData.Add(new KeyValuePair<string, object>("usesig", "1"));
            postData.Add(new KeyValuePair<string, object>("message", $"{content.Trim()}{MessageTail}"));

            // 图片信息
            foreach (var fileName in fileNameList)
            {
                postData.Add(new KeyValuePair<string, object>(string.Format("attachnew[{0}][description]", fileName), string.Empty));
            }

            string url = string.Format("http://www.hi-pda.com/forum/post.php?action=reply&tid={0}&replysubmit=yes&infloat=yes&handlekey=fastpost&inajax=1", threadId);
            string resultContent = await _httpClient.PostAsync(url, postData, cts);
            return resultContent.Contains("您的回复已经发布");
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


        #region 文件上传
        /// <summary>
        /// 用于上传从对话框中选择的文件，每次可上传多个文件
        /// </summary>
        /// <param name="cts"></param>
        /// <param name="beforeUpload"></param>
        /// <param name="afterUpload"></param>
        /// <returns></returns>
        public static async Task<List<string>[]> UploadFileAsync(CancellationTokenSource cts, Action<int, int, string> beforeUpload, Action<int> afterUpload)
        {
            var openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add("*");

            var files = await openPicker.PickMultipleFilesAsync();
            return await UploadFileAsync(cts, files, beforeUpload, afterUpload);
        }

        /// <summary>
        /// 用于上传拖拽的文件，每次可上传多个文件
        /// </summary>
        /// <param name="cts"></param>
        /// <param name="files"></param>
        /// <param name="beforeUpload"></param>
        /// <param name="afterUpload"></param>
        /// <returns></returns>
        public static async Task<List<string>[]> UploadFileAsync(CancellationTokenSource cts, IReadOnlyList<IStorageItem> files, 
            Action<int, int, string> beforeUpload, Action<int> afterUpload)
        {
            var fileNameList = new List<string>(); // 用于与其他文本内容一起POST
            var fileCodeList = new List<string>(); // 用于插入内容文本框

            if (files != null && files.Count > 0)
            {
                int fileIndex = 1;
                foreach (var file in files)
                {
                    string fileName = file.Name;

                    beforeUpload?.Invoke(fileIndex, files.Count, fileName);

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
                        string url = "http://www.hi-pda.com/forum/misc.php?action=swfupload&operation=upload&simple=1";
                        string result = await _httpClient.PostFileAsync(url, data, fileName, "Filedata", buffer, cts);
                        if (result.Contains("DISCUZUPLOAD|"))
                        {
                            string value = result.Split('|')[2];
                            value = $"[attachimg]{value}[/attachimg]";
                            fileCodeList.Add(value);

                            fileIndex++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                afterUpload?.Invoke(files.Count);
            }

            return new List<string>[] { fileNameList, fileCodeList };
        }

        /// <summary>
        /// 用于上传剪贴板中的图片，每次仅能上传一个图片
        /// </summary>
        /// <param name="cts"></param>
        /// <param name="file"></param>
        /// <param name="beforeUpload"></param>
        /// <param name="afterUpload"></param>
        /// <returns></returns>
        public static async Task<List<string>[]> UploadFileAsync(CancellationTokenSource cts, IRandomAccessStream stream, 
            Action<int, int, string> beforeUpload, Action<int> afterUpload)
        {
            var fileName = $"{DateTime.Now.Ticks.ToString("x")}.jpg";

            beforeUpload?.Invoke(1, 1, fileName);

            var fileNameList = new List<string>();
            var fileCodeList = new List<string>();

            IBuffer buffer = new Windows.Storage.Streams.Buffer((uint)stream.Size);
            buffer = await stream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);

            var data = new Dictionary<string, object>();
            data.Add("uid", AccountService.UserId);
            data.Add("hash", AccountService.Hash);

            try
            {
                string result = await _httpClient.PostFileAsync("http://www.hi-pda.com/forum/misc.php?action=swfupload&operation=upload&simple=1", data, fileName, "Filedata", buffer, cts);
                if (result.Contains("DISCUZUPLOAD|"))
                {
                    string value = result.Split('|')[2];
                    value = $"[attachimg]{value}[/attachimg]";
                    fileNameList.Add(fileName);
                    fileCodeList.Add(value);

                    afterUpload?.Invoke(1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return new List<string>[] { fileNameList, fileCodeList };
        }
        #endregion
    }
}
