using System;
using Windows.UI.Xaml;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ThreadItemModel : ThreadItemModelBase
    {
        public ThreadItemModel(int index, int forumId, int threadId, int pageNo, string title, int attachFileType, string replyCount, string viewCount, bool isTop, string authorUsername, int authorUserId, string authorCreateTime, string lastReplyUsername, string lastReplyTime)
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
                return AttachFileType == 1 ? "\uE187" : string.Empty;
            }
        }

        public string FileFontIcon
        {
            get
            {
                return AttachFileType == 2 ? "\uE16C" : string.Empty;
            }
        }

        public string TopFontIcon
        {
            get
            {
                return IsTop ? "\uE11C" : string.Empty;
            }
        }

        public Style ThreadItemStyle
        {
            get
            {
                return IsTop ? (Style)Application.Current.Resources["TopThreadItemStyle"] : (Style)Application.Current.Resources["NormalThreadItemStyle"];
            }
        }
    }
}
