using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class NewThreadViewModel
    {
        Action<int, int, string> _beforeUpload;
        Action<string> _insertFileCodeIntoContentTextBox;
        Action<int> _afterUpload;
        Action _sentFailded;
        Action<string> _sentSuccess;


        private string _title;

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private string _content;

        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }


        public DelegateCommand AddAttachFilesCommand { get; set; }

        public DelegateCommand SendCommand { get; set; }

        static List<string> _imaegNameList = new List<string>();

        public NewThreadViewModel(int forumId, Action<int, int, string> beforeUpload, Action<string> insertFileCodeIntoContentTextBox, Action<int> afterUpload, Action sentFailded, Action<string> sentSuccess)
        {
            _beforeUpload = beforeUpload;
            _insertFileCodeIntoContentTextBox = insertFileCodeIntoContentTextBox;
            _afterUpload = afterUpload;
            _sentSuccess = sentSuccess;
            _sentFailded = sentFailded;

            AddAttachFilesCommand = new DelegateCommand();
            AddAttachFilesCommand.ExecuteAction = async (p) => 
            {
                var cts = new CancellationTokenSource();
                var data = await PostMessageService.UploadFiles(cts, _beforeUpload, _insertFileCodeIntoContentTextBox, _afterUpload);
                if (data != null && data.Count > 0)
                {
                    _imaegNameList.AddRange(data);
                }
            };

            SendCommand = new DelegateCommand();
            SendCommand.ExecuteAction = async (p) =>
            {
                var cts = new CancellationTokenSource();
                bool flag = await PostMessageService.PostNewThread(cts, Title, Content, _imaegNameList, forumId);
                if (flag)
                {
                    _imaegNameList.Clear();

                    // 提示发贴成功
                    if (_sentSuccess != null)
                    {
                        _sentSuccess(Title);
                    }
                }
                else
                {
                    // 提示发贴不成功
                    if (_sentFailded != null)
                    {
                        _sentFailded();
                    }
                }
            };
        }
    }
}
