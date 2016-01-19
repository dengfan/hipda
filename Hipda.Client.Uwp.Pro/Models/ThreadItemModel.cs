using System;
using Windows.UI.Xaml;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ThreadItemModel : ThreadItemModelBase
    {
        public ThreadItemModel(int index, int forumId, int threadId, int pageNo, string title, int attachFileType, string replyCount, string viewCount, bool isTop, string authorUsername, int authorUserId, string authorCreateTime, string lastReplyUsername, string lastReplyTime, bool isMine)
        {
            this.Index = index;
            this.ForumId = forumId;
            this.ThreadId = threadId;
            this.PageNo = pageNo;
            this.Title = title;
            this.AttachFileType = attachFileType;
            this.ReplyCount = replyCount;
            this.ViewCount = viewCount;
            this.IsTop = isTop;
            this.AuthorUsername = authorUsername;
            this.AuthorUserId = authorUserId;
            this.AuthorCreateTime = authorCreateTime;
            this.LastReplyUsername = lastReplyUsername;
            this.LastReplyTime = lastReplyTime;
            this.IsMine = isMine;
            this.ThreadType = ThreadDataType.Default;
        }

        public int Index { get; private set; }

        public int ForumId { get; private set; }

        public int PageNo { get; private set; }

        public string ReplyCount { get; private set; }

        public string ViewCount { get; private set; }

        public bool IsTop { get; private set; }

        public int AttachFileType { get; private set; }

        public string AuthorUsername { get; private set; }

        public int AuthorUserId { get; private set; }

        public string AuthorCreateTime { get; private set; }

        public string LastReplyUsername { get; private set; }

        public string LastReplyTime { get; private set; }

        public bool IsMine { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }

        public string ViewInfo
        {
            get
            {
                return string.Format("({0}/{1})", ReplyCount, ViewCount);
            }
        }

        public string LastReplyInfo
        {
            get
            {
                return string.Format("{0} {1}", LastReplyUsername, LastReplyTime);
            }
        }

        public string ImageFontIcon
        {
            get
            {
                return AttachFileType == 1 ? "\uD83C\uDF04" : string.Empty;
            }
        }

        public string FileFontIcon
        {
            get
            {
                return AttachFileType == 2 ? "\uD83D\uDCCE" : string.Empty;
            }
        }

        public string TopFontIcon
        {
            get
            {
                return IsTop ? "\uD83D\uDD1D" : string.Empty;
            }
        }

        public Style ThreadItemStyle
        {
            get
            {
                if (IsTop)
                {
                    return (Style)App.Current.Resources["TopThreadItemStyle"];
                }
                else if (IsMine)
                {
                    return (Style)App.Current.Resources["MineThreadItemStyle"];
                }
                else
                {
                    return (Style)App.Current.Resources["NormalThreadItemStyle"];
                }
            }
        }
    }
}
