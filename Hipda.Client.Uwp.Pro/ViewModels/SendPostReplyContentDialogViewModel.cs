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
    public class SendPostReplyContentDialogViewModel
    {
        int _postAuthorUserId;
        string _postAuthorUsername;
        string _postSimpleContent;
        int _floorNo;
        int _postId;
        int _threadId;
        Action<int, int, string> _beforeUpload;
        Action<string> _insertFileCodeIntoContentTextBox;
        Action<int> _afterUpload;
        Action<string> _sentFailded;
        Action<string> _sentSuccess;

        string _noticeauthor;
        string _noticetrimstr;
        string _noticeauthormsg;

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


        public DelegateCommand AddAttachFilesCommand { get; set; }

        public DelegateCommand SendCommand { get; set; }

        static List<string> _fileNameList = new List<string>();
        static List<string> _fileCodeList = new List<string>();

        public SendPostReplyContentDialogViewModel(CancellationTokenSource cts, string replyType, int postAuthorUserId, string postAuthorUsername, string postSimpleContent, string postTime, int floorNo, int postId, int threadId, Action<int, int, string> beforeUpload, Action<string> insertFileCodeIntoContentTextBox, Action<int> afterUpload, Action<string> sentFailded, Action<string> sentSuccess)
        {
            _postAuthorUserId = postAuthorUserId;
            _postAuthorUsername = postAuthorUsername;
            _postSimpleContent = postSimpleContent;
            _floorNo = floorNo;
            _postId = postId;
            _threadId = threadId;
            _beforeUpload = beforeUpload;
            _insertFileCodeIntoContentTextBox = insertFileCodeIntoContentTextBox;
            _afterUpload = afterUpload;
            _sentSuccess = sentSuccess;
            _sentFailded = sentFailded;

            if (replyType.Equals("r"))
            {
                _noticeauthor = $"{replyType}|{_postAuthorUserId}|[i]{_postAuthorUsername}[/i]";
                _noticetrimstr = $"[b]回复 [url=http://www.hi-pda.com/forum/redirect.php?goto=findpost&pid={_postId}&ptid={_threadId}]{_floorNo}#[/url] [i]{_postAuthorUsername}[/i] [/b]\r\n \r\n    ";
            }
            else if (replyType.Equals("q"))
            {
                _noticeauthor = $"{replyType}|{_postAuthorUserId}|{_postAuthorUsername}";
                _noticetrimstr = $"[quote]{_postSimpleContent}\r\n[size=2][color=#999999]{_postAuthorUserId} 发表于 {postTime}[/color] [url=http://www.hi-pda.com/forum/redirect.php?goto=findpost&pid={_postId}&ptid={_threadId}][img]http://www.hi-pda.com/forum/images/common/back.gif[/img][/url][/size][/quote]\r\n    ";
            }
            _noticeauthormsg = _postSimpleContent;

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

                bool flag = await SendService.SendPostReplyAsync(cts, _noticeauthor, _noticetrimstr, _noticeauthormsg, Content, _fileNameList, _threadId);
                if (flag)
                {
                    _fileNameList.Clear();

                    Title = string.Empty;
                    Content = string.Empty;

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
                        _sentFailded("对不起，发布请求失败，请稍后再试！");
                    }
                }
            };
        }
    }
}
