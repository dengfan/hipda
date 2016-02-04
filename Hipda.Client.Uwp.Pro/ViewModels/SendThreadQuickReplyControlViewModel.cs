using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class SendThreadQuickReplyControlViewModel : NotificationObject
    {
        int _threadId;
        Action<int, int, string> _beforeUpload;
        Action<string> _insertFileCodeIntoContentTextBox;
        Action<int> _afterUpload;
        Action<string> _sentFailded;
        Action<string> _sentSuccess;

        private string _content;

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                this.RaisePropertyChanged("Content");
            }
        }


        public DelegateCommand AddAttachFilesCommand { get; set; }

        public DelegateCommand SendCommand { get; set; }

        static List<string> _fileNameList = new List<string>();
        static List<string> _fileCodeList = new List<string>();

        public SendThreadQuickReplyControlViewModel(CancellationTokenSource cts, int threadId, Action<int, int, string> beforeUpload, Action<string> insertFileCodeIntoContentTextBox, Action<int> afterUpload, Action<string> sentFailded, Action<string> sentSuccess)
        {
            _threadId = threadId;
            _beforeUpload = beforeUpload;
            _insertFileCodeIntoContentTextBox = insertFileCodeIntoContentTextBox;
            _afterUpload = afterUpload;
            _sentSuccess = sentSuccess;
            _sentFailded = sentFailded;

            AddAttachFilesCommand = new DelegateCommand();
            AddAttachFilesCommand.ExecuteAction = async (p) =>
            {
                var data = await SendService.UploadFiles(cts, _beforeUpload, _afterUpload);
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
                }
            };

            SendCommand = new DelegateCommand();
            SendCommand.ExecuteAction = async (p) =>
            {
                if (string.IsNullOrEmpty(Content))
                {
                    _sentFailded("请填写内容！");
                    return;
                }

                bool flag = await SendService.SendThreadReply(cts, Content, _fileNameList, _threadId);
                if (flag)
                {
                    _fileNameList.Clear();
                    _fileCodeList.Clear();

                    Content = string.Empty;

                    // 提示发贴成功
                    if (_sentSuccess != null)
                    {
                        _sentSuccess(string.Empty);
                    }
                }
                else
                {
                    // 提示发贴不成功
                    if (_sentFailded != null)
                    {
                        _sentFailded("对不起，发布请求失败，请稍后再试！");
                    }
                }
            };
        }
    }
}
