using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ReplyItemModel
    {
        public ReplyItemModel(int index, int index2, int floorNo, int postId, int pageNo, int forumId, string forumName, int threadId, string threadTitle, int threadAuthorUserId, int authorUserId, string authorUsername, string textContent, string xamlConent, string authorCreateTime, int imageCount, bool isHighLight, int inAppLinkCount)
        {
            this.Index = index;
            this.Index2 = index;
            this.FloorNo = floorNo;
            this.PostId = postId;
            this.PageNo = pageNo;
            this.ForumId = forumId;
            this.ForumName = forumName;
            this.ThreadId = threadId;
            this.ThreadTitle = threadTitle;
            this.ThreadAuthorUserId = threadAuthorUserId;
            this.AuthorUserId = authorUserId;
            this.AuthorUsername = authorUsername;
            this.TextStr = textContent;
            this.XamlStr = xamlConent;
            this.AuthorCreateTime = authorCreateTime;
            this.ImageCount = imageCount;
            this.IsHighLight = isHighLight;
            this.IsLast = false;
            this.InAppLinkCount = inAppLinkCount;
        }

        public int Index { get; private set; }
        public int Index2 { get; private set; }
        public int FloorNo { get; private set; }
        public int PostId { get; private set; }
        public int PageNo { get; private set; }
        public int ForumId { get; private set; }
        public string ForumName { get; private set; }
        public int ThreadId { get; private set; }

        string _threadTitle;
        public string ThreadTitle
        {
            get
            {
                return CommonService.ReplaceEmojiLabel(_threadTitle);
            }
            set
            {
                _threadTitle = value;
            }
        }

        public int ThreadAuthorUserId { get; set; }
        public string AuthorUsername { get; private set; }

        public int AuthorUserId { get; private set; }

        public string TextStr { get; private set; }

        public string XamlStr { get; set; }

        public string AuthorCreateTime { get; private set; }

        public int ImageCount { get; set; }

        public bool IsHighLight { get; private set; }

        public bool IsLast { get; set; }

        public int InAppLinkCount { get; private set; }

        public bool HasThreadTitle
        {
            get
            {
                return FloorNo == 1;
            }
        }

        public bool IsTopicStarter
        {
            get
            {
                return ThreadAuthorUserId == AuthorUserId;
            }
        }

        public bool IsMine
        {
            get
            {
                return AuthorUserId == AccountService.UserId;
            }
        }

        public string FloorNoStr
        {
            get
            {
                return string.Format("{0}#", FloorNo);
            }
        }

        public StackPanel XamlContent
        {
            get
            {
                if (FloorNo == -1)
                {
                    // “---完---”项不作处理
                    return null;
                }

                try
                {
                    StackPanel sp = (StackPanel)XamlReader.Load(CommonService.ReplaceEmojiLabel(XamlStr));
                    for (int i = 1; i <= InAppLinkCount; i++)
                    {
                        var key = $"InAppLink_{ThreadId}_{PostId}_{i}";
                        var hyperLink = (Hyperlink)sp.FindName(key);
                        if (hyperLink != null)
                        {
                            hyperLink.Click += HyperLink_Click;
                        }
                    }
                    return sp;
                }
                catch
                {
                    string errorDetails = string.Format("http://www.hi-pda.com/forum/viewthread.php?tid={0} 楼层{1}内容解析出错。\r\n{2}", ThreadId, FloorNo, XamlStr);
                    CommonService.PostErrorEmailToDeveloper("回复内容解析出现异常", errorDetails);

                    string text = CommonService.ReplaceHexadecimalSymbols(TextStr);
                    XamlStr = string.Format("<StackPanel xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><RichTextBlock><Paragraph>{0}</Paragraph></RichTextBlock></StackPanel>", text);
                    return (StackPanel)XamlReader.Load(XamlStr);
                }
            }
        }

        private void HyperLink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            var key = sender.Name;
            if (ReplyListService.InAppLinkUrlDic.ContainsKey(key))
            {
                var val = ReplyListService.InAppLinkUrlDic[key];
                var pid = new Regex("pid=([0-9]+)").Match(val)?.Groups[1]?.Value;
                var tid = new Regex("tid=([0-9]+)").Match(val)?.Groups[1]?.Value;
                
                int postId = 0;
                if (pid != null)
                {
                    int.TryParse(pid, out postId);
                }

                int threadId = 0;
                if (tid != null)
                {
                    int.TryParse(tid, out threadId);
                }

                if (postId == 0 && threadId == 0)
                {
                    return;
                }

                var frame = Window.Current.Content as Frame;
                var mp = frame.Content as MainPage;
                if (mp != null)
                {
                    var pageType = mp.AppFrame.Content.GetType();
                    if (pageType.Equals(typeof(ThreadAndReplyPage)))
                    {
                        var p = (ThreadAndReplyPage)mp.AppFrame.Content;
                        if (p != null)
                        {
                            if (postId > 0)
                            {
                                p.PostId = postId;
                                p.OpenReplyPageByPostId();
                            }
                            else
                            {
                                p.ThreadId = Convert.ToInt32(tid);
                                p.OpenReplyPageByThreadId();
                            }
                        }
                    }
                    else if (pageType.Equals(typeof(ReplyListPage)))
                    {
                        var p = (ReplyListPage)mp.AppFrame.Content;
                        if (p != null)
                        {
                            p.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;

                            if (postId > 0)
                            {
                                p.PostId = postId;
                                p.OpenReplyPageByPostId();
                            }
                            else
                            {
                                p.ThreadId = Convert.ToInt32(tid);
                                p.OpenReplyPageByThreadId();
                            }
                        }
                    }
                }
            }
        }

        public Uri AvatarUri
        {
            get
            {
                return CommonService.GetSmallAvatarUriByUserId(AuthorUserId);
            }
        }
    }
}
