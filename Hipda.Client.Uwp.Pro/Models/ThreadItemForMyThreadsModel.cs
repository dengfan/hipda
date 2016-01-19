using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ThreadItemForMyThreadsModel : ThreadItemModelBase
    {
        public ThreadItemForMyThreadsModel(int index, string forumName, int threadId, int pageNo, string title, string lastReplyUsername, string lastReplyTime)
        {
            this.Index = index;
            this.ForumName = forumName;
            this.ThreadId = threadId;
            this.PageNo = pageNo;
            this.Title = title;
            this.LastReplyUsername = lastReplyUsername;
            this.LastReplyTime = lastReplyTime;
            this.ThreadType = ThreadDataType.MyThreads;
        }

        public int Index { get; private set; }

        public string ForumName { get; private set; }

        public int PageNo { get; private set; }

        public string LastReplyUsername { get; private set; }

        public string LastReplyTime { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }

        public string LastReplyInfo
        {
            get
            {
                return string.Format("{0} {1}", LastReplyUsername, LastReplyTime);
            }
        }
    }
}
