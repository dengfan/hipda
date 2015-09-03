using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ThreadItemModel
    {
        public ThreadItemModel(int index, int forumId, int threadId, int pageNo, string title, int attachFileType, string replyCount, string viewCount, string authorUsername, string authorUserId, string authorCreateTime, string lastReplyUsername, string lastReplyTime)
        {
            this.Index = index;
            this.ForumId = forumId;
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
            this.Replies = new List<ReplyItemModel>();
        }

        public int Index { get; private set; }

        public int ForumId { get; private set; }

        public int ThreadId { get; private set; }

        public int PageNo { get; private set; }

        public string Title { get; private set; }

        public string ReplyCount { get; private set; }

        public string ViewCount { get; private set; }

        public int AttachFileType { get; private set; }

        public string AuthorUsername { get; private set; }

        public string AuthorUserId { get; private set; }

        public string AuthorCreateTime { get; private set; }

        public string LastReplyUsername { get; private set; }

        public string LastReplyTime { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }

        public string AvatarUrl
        {
            get
            {
                if (string.IsNullOrEmpty(AuthorUserId))
                {
                    return string.Empty;
                }

                int uid = Convert.ToInt32(AuthorUserId);
                var s = new int[10];
                for (int i = 0; i < s.Length - 1; ++i)
                {
                    s[i] = uid % 10;
                    uid = (uid - s[i]) / 10;
                }
                return "http://www.hi-pda.com/forum/uc_server/data/avatar/" + s[8] + s[7] + s[6] + "/" + s[5] + s[4] + "/" + s[3] + s[2] + "/" + s[1] + s[0] + "_avatar_middle.jpg";
            }
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

        public List<ReplyItemModel> Replies { get; set; }
    }
}
