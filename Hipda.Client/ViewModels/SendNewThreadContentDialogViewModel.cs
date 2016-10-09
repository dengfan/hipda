using GalaSoft.MvvmLight.Command;
using Hipda.Client.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;

namespace Hipda.Client.ViewModels
{
    public class SendNewThreadContentDialogViewModel
    {
        Action<int, int, string> _beforeUpload;
        Action<string> _insertFileCodeIntoContentTextBox;
        Action<int> _afterUpload;
        Action<string> _sentFailded;
        Action<string> _sentSuccess;


        private static string _title;

        public static string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private static string _content;

        public static string Content
        {
            get { return _content; }
            set { _content = value; }
        }


        public ICommand AddAttachFilesCommand { get; set; }

        public ICommand SendCommand { get; set; }

        static List<string> _fileNameList = new List<string>();
        static List<string> _fileCodeList = new List<string>();

        public SendNewThreadContentDialogViewModel(CancellationTokenSource cts, int forumId,
            Action<int, int, string> beforeUpload, Action<string> insertFileCodeIntoContentTextBox, Action<int> afterUpload,
            Action<string> sentFailded, Action<string> sentSuccess)
        {
            _beforeUpload = beforeUpload;
            _insertFileCodeIntoContentTextBox = insertFileCodeIntoContentTextBox;
            _afterUpload = afterUpload;
            _sentSuccess = sentSuccess;
            _sentFailded = sentFailded;

            AddAttachFilesCommand = new RelayCommand(async () =>
            {
                var data = await SendService.UploadFileAsync(cts, _beforeUpload, _afterUpload);
                if (data[0] != null && data[0].Count > 0)
                {
                    _fileNameList.AddRange(data[0]);
                }
                if (data[1] != null && data[1].Count > 0)
                {
                    _fileCodeList.AddRange(data[1]);
                }

                if (_fileCodeList.Count > 0)
                {
                    string fileCodes = string.Join("\r\n", _fileCodeList);
                    _insertFileCodeIntoContentTextBox($"\r\n{fileCodes}\r\n");
                    _fileCodeList.Clear();
                }
            });

            SendCommand = new RelayCommand(async () =>
            {
                if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Content))
                {
                    _sentFailded("请将标题及内容填写完整！");
                    return;
                }

                bool flag = await SendService.SendNewThreadAsync(cts, Title, Content, _fileNameList, forumId);
                if (flag)
                {
                    _fileNameList.Clear();

                    Title = string.Empty;
                    Content = string.Empty;

                    // 提示发贴成功
                    _sentSuccess?.Invoke(Title);
                }
                else
                {
                    // 提示发贴不成功
                    _sentFailded?.Invoke("对不起，发布请求失败，请稍后再试！");
                }
            });
        }
    }
}
