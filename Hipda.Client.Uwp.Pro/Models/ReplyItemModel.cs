using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ReplyItemModel
    {
        public ReplyItemModel(int index, int floorNo, string postId, int pageNo, int threadId, string threadTitle, string authorUserId, string authorUsername, string textContent, string htmlContent, string xamlConent, int imageCount, string authorCreateTime)
        {
            this.Index = index;
            this.FloorNo = floorNo;
            this.PostId = postId;
            this.PageNo = pageNo;
            this.ThreadId = threadId;
            this.ThreadTitle = threadTitle;
            this.AuthorUserId = authorUserId;
            this.AuthorUsername = authorUsername;
            this.TextContent = textContent;
            this.HtmlContent = htmlContent;
            this.XamlContent = xamlConent;
            this.ImageCount = imageCount;
            this.AuthorCreateTime = authorCreateTime;
        }

        public int Index { get; private set; }
        public int FloorNo { get; private set; }
        public string PostId { get; private set; }
        public int PageNo { get; private set; }
        public int ThreadId { get; private set; }
        public string ThreadTitle { get; private set; }
        public string AuthorUsername { get; private set; }

        public string AuthorUserId { get; private set; }

        public string TextContent { get; private set; }

        public string HtmlContent { get; private set; }

        public string XamlContent { get; set; }

        public int ImageCount { get; set; }

        public string AuthorCreateTime { get; private set; }

        public bool HasThreadTitle
        {
            get
            {
                return FloorNo == 1;
            }
        }

        public override string ToString()
        {
            return this.HtmlContent;
        }
        public string AvatarUrl
        {
            get
            {
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

        public string FloorNumStr
        {
            get
            {
                return string.Format("{0}#", FloorNo);
            }
        }
    }
}
