using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ThreadItemForMyPostsModel
    {
        public ThreadItemForMyPostsModel(int index, int forumId, string forumName, int threadId, int pageNo, string title, string lastReplyUsername, string lastReplyTime)
        {
            this.Index = index;
            this.ForumId = forumId;
            this.ForumName = forumName;
            this.ThreadId = threadId;
            this.PageNo = pageNo;
            this.Title = title;
            this.LastReplyUsername = lastReplyUsername;
            this.LastReplyTime = lastReplyTime;
        }

        public int Index { get; private set; }

        public int ForumId { get; private set; }

        public string ForumName { get; private set; }

        public int ThreadId { get; private set; }

        public int PageNo { get; private set; }

        public string Title { get; private set; }

        public string LastReplyUsername { get; private set; }

        public string LastReplyTime { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }
}
