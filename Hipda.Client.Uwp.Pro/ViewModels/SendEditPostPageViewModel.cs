using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class SendEditPostPageViewModel : NotificationObject
    {
        int _postId;
        int _threadId;
        Action<int, int, string> _beforeUpload;
        Action<string> _insertFileCodeIntoContentTextBox;
        Action<int> _afterUpload;
        Action<string> _sentFailded;
        Action<string> _sentSuccess;

        private static string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                this.RaisePropertyChanged("Title");
            }
        }

        private static string _content;

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                this.RaisePropertyChanged("Content");
            }
        }

        private ObservableCollection<AttachFileItemModel> _attachImageList;

        public ObservableCollection<AttachFileItemModel> AttachImageList
        {
            get { return _attachImageList; }
            set
            {
                _attachImageList = value;
            }
        }

        public DelegateCommand AddAttachFilesCommand { get; set; }

        public DelegateCommand SendCommand { get; set; }

        static List<string> _fileNameList = new List<string>();
        static List<string> _fileCodeList = new List<string>();

        public SendEditPostPageViewModel(CancellationTokenSource cts, PostEditDataModel editData, Action<int, int, string> beforeUpload, Action<string> insertFileCodeIntoContentTextBox, Action<int> afterUpload, Action<string> sentFailded, Action<string> sentSuccess)
        {
            ShowUnusedImage(cts);

            Title = editData.Title;
            Content = editData.Content;
            AttachImageList = editData.AttachFileList;
            _postId = editData.PostId;
            _threadId = editData.ThreadId;
            _beforeUpload = beforeUpload;
            _insertFileCodeIntoContentTextBox = insertFileCodeIntoContentTextBox;
            _afterUpload = afterUpload;
            _sentSuccess = sentSuccess;
            _sentFailded = sentFailded;

            AddAttachFilesCommand = new DelegateCommand();
            AddAttachFilesCommand.ExecuteAction = async (p) =>
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

                    ShowUnusedImage(cts);
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

                await SendService.SendEditPostAsync(cts, Title, Content, _fileNameList, _postId, _threadId);

                _fileNameList.Clear();

                Title = string.Empty;
                Content = string.Empty;

                // 提示发贴成功
                if (_sentSuccess != null)
                {
                    _sentSuccess(Title);
                }
            };
        }

        public async void DragAndUploadFile(CancellationTokenSource cts, IReadOnlyList<IStorageItem> files, Action<int, int, string> beforeUpload, Action<int> afterUpload)
        {
            var data = await SendService.UploadFileAsync(cts, files, beforeUpload, afterUpload);
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

                ShowUnusedImage(cts);
            }
        }

        async void ShowUnusedImage(CancellationTokenSource cts)
        {
            var unusedImageAttachList = await SendService.LoadUnusedImageAttachListAsync(cts);
            if (unusedImageAttachList != null && unusedImageAttachList.Count > 0)
            {
                foreach (var item in unusedImageAttachList)
                {
                    if (!AttachImageList.Contains(item))
                    {
                        AttachImageList.Add(item);
                    }
                }
            }
        }
    }
}
