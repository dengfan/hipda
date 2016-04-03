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

        private ObservableCollection<AttachFileItemModel> _attachFileList;

        public ObservableCollection<AttachFileItemModel> AttachFileList
        {
            get { return _attachFileList; }
            set
            {
                _attachFileList = value;
            }
        }

        //public DelegateCommand RemoveAttachFileCommand { get; set; }
        //public DelegateCommand InsertAttachFileCommand { get; set; }
        public DelegateCommand AddAttachFilesCommand { get; set; }

        public DelegateCommand SendCommand { get; set; }

        static List<string> _fileCodeList = new List<string>();
        static List<string> _fileAddList = new List<string>();
        static List<string> _fileRemoveList = new List<string>();

        public SendEditPostPageViewModel(CancellationTokenSource cts, PostEditDataModel editData, Action<int, int, string> beforeUpload, Action<string> insertFileCodeIntoContentTextBox, Action<int> afterUpload, Action<string> sentFailded, Action<string> sentSuccess)
        {
            ShowUnusedImage(cts);

            Title = editData.Title;
            Content = editData.Content;
            AttachFileList = editData.AttachFileList;
            _postId = editData.PostId;
            _threadId = editData.ThreadId;
            _beforeUpload = beforeUpload;
            _insertFileCodeIntoContentTextBox = insertFileCodeIntoContentTextBox;
            _afterUpload = afterUpload;
            _sentSuccess = sentSuccess;
            _sentFailded = sentFailded;

            //RemoveAttachFileCommand = new DelegateCommand();
            //RemoveAttachFileCommand.ExecuteAction = (p) =>
            //{
            //    int a = 1;
            //};

            //InsertAttachFileCommand = new DelegateCommand();
            //InsertAttachFileCommand.ExecuteAction = (p) =>
            //{
            //    int b = 1;
            //};

            AddAttachFilesCommand = new DelegateCommand();
            AddAttachFilesCommand.ExecuteAction = async (p) =>
            {
                var data = await SendService.UploadFileAsync(cts, _beforeUpload, _afterUpload);
                if (data[0] != null && data[0].Count > 0)
                {
                    _fileAddList.AddRange(data[0]);
                }
                if (data[1] != null && data[1].Count > 0)
                {
                    _fileCodeList.AddRange(data[1]);
                }

                if (_fileCodeList.Count > 0)
                {
                    string fileCodes = string.Join("\r\n", _fileCodeList).Trim();
                    _insertFileCodeIntoContentTextBox($"{fileCodes}\r\n");
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

                await SendService.SendEditPostAsync(cts, Title, Content, _fileAddList, _fileRemoveList, _postId, _threadId);

                _fileAddList.Clear();
                _fileRemoveList.Clear();

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
                _fileAddList.AddRange(data[0]);
            }
            if (data[1] != null && data[1].Count > 0)
            {
                _fileCodeList.AddRange(data[1]);
            }

            if (_fileCodeList.Count > 0)
            {
                string fileCodes = string.Join("\r\n", _fileCodeList).Trim();
                _insertFileCodeIntoContentTextBox($"{fileCodes}\r\n");
                _fileCodeList.Clear();

                ShowUnusedImage(cts);
            }
        }

        async void ShowUnusedImage(CancellationTokenSource cts)
        {
            var unusedImageAttachList = await SendService.LoadUnusedAttachFilesAsync(cts);
            if (unusedImageAttachList != null && unusedImageAttachList.Count > 0)
            {
                if (AttachFileList == null)
                {
                    AttachFileList = new ObservableCollection<AttachFileItemModel>();
                }

                var list = AttachFileList.ToList();
                foreach (var item in unusedImageAttachList)
                {
                    if (list.Count(i => i.Id.Equals(item.Id)) == 0)
                    {
                        AttachFileList.Add(item);
                    }
                }
            }
        }

        public void RemoveAttachFile(string id)
        {
            var item = AttachFileList.Single(f => f.Id.Equals(id));
            _fileRemoveList.Add(item.Id);
            AttachFileList.Remove(item);
        }

        public void InsertAttachFile(string id)
        {
            var item = AttachFileList.Single(f => f.Id.Equals(id));
            _insertFileCodeIntoContentTextBox($"[attachimg]{item.Id}[/attachimg]\r\n");
        }
    }
}
