using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ThreadItemForSearchFullTextModel : ThreadItemModelBase
    {
        public ThreadItemForSearchFullTextModel(int index, string forumName, int threadId, int pageNo, string title, int attachFileType, string replyCount, string viewCount, string authorUsername, int authorUserId, string authorCreateTime, string lastReplyUsername, string lastReplyTime)
        {
            this.Index = index;
            this.ForumName = forumName;
            this.ThreadId = threadId;
            this.PageNo = pageNo;
            this.Title = title;
            this.AttachFileType = attachFileType;
            this.ReplyCount = replyCount;
            this.ViewCount = viewCount;
            this.AuthorUsername = authorUsername;
            this.AuthorUserId = authorUserId;
            this.AuthorCreateTime = authorCreateTime;
            this.LastReplyUsername = lastReplyUsername;
            this.LastReplyTime = lastReplyTime;
        }

        public int Index { get; private set; }

        public int ForumId { get; private set; }

        public string ForumName { get; private set; }

        public int PageNo { get; private set; }

        public string ReplyCount { get; private set; }

        public string ViewCount { get; private set; }

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
                return AttachFileType == 1 ? "\uEB9F" : string.Empty;
            }
        }

        public string FileFontIcon
        {
            get
            {
                return AttachFileType == 2 ? "\uE16C" : string.Empty;
            }
        }

        public Style ThreadItemStyle
        {
            get
            {
                return (Style)App.Current.Resources["NormalThreadItemStyle"];
            }
        }
    }
}
