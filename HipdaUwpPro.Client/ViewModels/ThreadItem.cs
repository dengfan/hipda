using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HipdaUwpPro.Client.ViewModels
{
    public class ThreadItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ThreadItem(int index, int pageNo, string forumId, string id, string title, int attachType, string replyNum, string viewNum, string ownerName, string ownerId, string createTime, string lastPostAuthorName, string lastPostTime)
        {
            this.Index = index;
            this.PageNo = pageNo;
            this.ForumId = forumId;
            this.Id = id;
            this.Title = title;
            this.AttachType = attachType;
            this.ReplyNum = replyNum;
            this.ViewNum = viewNum;
            this.OwnerName = ownerName;
            this.OwnerId = ownerId;
            this.CreateTime = createTime;
            this.LastPostAuthorName = lastPostAuthorName;
            this.LastPostTime = lastPostTime;
        }

        public int Index { get; private set; }
        public int PageNo { get; private set; }
        public string ForumId { get; private set; }
        public string Id { get; private set; } 
        public string Title { get; private set; }
        public int AttachType { get; private set; }
        public string ReplyNum { get; private set; }
        public string ViewNum { get; private set; }
        public string OwnerName { get; private set; }
        public string OwnerId { get; private set; }
        public string CreateTime { get; private set; }
        public string LastPostAuthorName { get; private set; }
        public string LastPostTime { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }

        public string AvatarUrl
        {
            get
            {
                if (string.IsNullOrEmpty(OwnerId))
                {
                    return string.Empty;
                }

                int uid = Convert.ToInt32(OwnerId);
                var s = new int[10];
                for (int i = 0; i < s.Length - 1; ++i)
                {
                    s[i] = uid % 10;
                    uid = (uid - s[i]) / 10;
                }
                return "http://www.hi-pda.com/forum/uc_server/data/avatar/" + s[8] + s[7] + s[6] + "/" + s[5] + s[4] + "/" + s[3] + s[2] + "/" + s[1] + s[0] + "_avatar_middle.jpg";
            }
        }

        public string Numbers
        {
            get
            {
                return string.Format("({0}/{1})", ReplyNum, ViewNum);
            }
        }

        public string LastPostInfo
        {
            get
            {
                return string.Format("{0} {1}", LastPostAuthorName, LastPostTime);
            }
        }
    }
}
