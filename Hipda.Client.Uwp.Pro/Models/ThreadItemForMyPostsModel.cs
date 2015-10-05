using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ThreadItemForMyPostsModel
    {
        public ThreadItemForMyPostsModel(int index, string forumName, int threadId, int pageNo, string title, string lastReplyContent, string lastReplyTime)
        {
            this.Index = index;
            this.ForumName = forumName;
            this.ThreadId = threadId;
            this.PageNo = pageNo;
            this.Title = title;
            this.LastReplyContent = lastReplyContent;
            this.LastReplyTime = lastReplyTime;
        }

        public int Index { get; private set; }

        public string ForumName { get; private set; }

        public int ThreadId { get; private set; }

        public int PageNo { get; private set; }

        public string Title { get; private set; }

        public string LastReplyContent { get; private set; }

        public string LastReplyTime { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }

        public string LastReplyInfo
        {
            get
            {
                return LastReplyTime;
            }
        }
    }
}
