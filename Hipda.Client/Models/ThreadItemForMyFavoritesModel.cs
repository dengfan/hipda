﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Models
{
    public class ThreadItemForMyFavoritesModel : ThreadItemModelBase
    {
        public ThreadItemForMyFavoritesModel(int index, string forumName, int threadId, int pageNo, string title, int replyCount, string lastReplyUsername, string lastReplyTime)
        {
            this.Index = index;
            this.ForumName = forumName;
            this.ThreadId = threadId;
            this.PageNo = pageNo;
            this.Title = title;
            this.ReplyCount = replyCount;
            this.LastReplyUsername = lastReplyUsername;
            this.LastReplyTime = lastReplyTime;
            this.ThreadType = ThreadDataType.MyFavorites;
        }

        public int Index { get; private set; }

        public int PageNo { get; private set; }

        public int ReplyCount { get; private set; }

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
