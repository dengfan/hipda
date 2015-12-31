using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public enum NoticeType
    {
        /// <summary>
        /// 被引用或回复
        /// </summary>
        QuoteOrReply,

        /// <summary>
        /// 关注的主题被回复
        /// </summary>
        Thread,

        /// <summary>
        /// 被加为好友
        /// </summary>
        Buddy
    }

    public class NoticeItemForThreadModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public int ThreadId { get; set; }
        public string ThreadTitle { get; set; }
        public int PostId { get; set; }
        public string ActionTime { get; set; }
    }

    public class NoticeItemForQuoteModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public int ThreadId { get; set; }
        public string ThreadTitle { get; set; }
        public string ActionTime { get; set; }
        public string OriginalContent { get; set; }
        public string ActionContent { get; set; }
    }

    public class NoticeItemForReplyModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public int ThreadId { get; set; }
        public string ThreadTitle { get; set; }
        public string ActionTime { get; set; }
        public string ActionContent { get; set; }
    }

    /// <summary>
    /// http://www.hi-pda.com/forum/my.php?from=notice&item=buddylist&newbuddyid=759715&buddysubmit=yes
    /// </summary>
    public class NoticeItemForBuddyModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string ActionTime { get; set; }
        public int NewBuddyId { get; set; }
    }

    public class NoticeItemModel
    {
        public NoticeItemModel(NoticeType noticeType, string username, string actionTime, string[] actionInfo)
        {
            this.NoticeType = noticeType;
            Username = username;
            ActionTime = actionTime;
            ActionInfo = actionInfo;
        }

        public NoticeType NoticeType { get; set; }
        public string Username { get; set; }
        public string ActionTime { get; set; }
        public string[] ActionInfo { get; set; }
    }
}
