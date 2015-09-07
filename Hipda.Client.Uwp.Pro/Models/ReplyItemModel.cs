using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Markup;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ReplyItemModel
    {
        public ReplyItemModel(int index, int floorNo, string postId, int pageNo, int threadId, string threadTitle, int threadAuthorUserId, int authorUserId, string authorUsername, string textContent, string htmlContent, string xamlConent, int imageCount, string authorCreateTime)
        {
            this.Index = index;
            this.FloorNo = floorNo;
            this.PostId = postId;
            this.PageNo = pageNo;
            this.ThreadId = threadId;
            this.ThreadTitle = threadTitle;
            this.ThreadAuthorUserId = threadAuthorUserId;
            this.AuthorUserId = authorUserId;
            this.AuthorUsername = authorUsername;
            this.TextStr = textContent;
            this.HtmlStr = htmlContent;
            this.XamlStr = xamlConent;
            this.ImageCount = imageCount;
            this.AuthorCreateTime = authorCreateTime;
        }

        public int Index { get; private set; }
        public int FloorNo { get; private set; }
        public string PostId { get; private set; }
        public int PageNo { get; private set; }
        public int ThreadId { get; private set; }
        public string ThreadTitle { get; private set; }
        public int ThreadAuthorUserId { get; private set; }
        public string AuthorUsername { get; private set; }

        public int AuthorUserId { get; private set; }

        public string TextStr { get; private set; }

        public string HtmlStr { get; private set; }

        public string XamlStr { get; set; }

        public int ImageCount { get; set; }

        public string AuthorCreateTime { get; private set; }

        public bool HasThreadTitle
        {
            get
            {
                return FloorNo == 1;
            }
        }

        public bool IsTopicStarter
        {
            get
            {
                return ThreadAuthorUserId == AuthorUserId;
            }
        }

        public override string ToString()
        {
            return this.HtmlStr;
        }
        public Uri AvatarUrl
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
                return new Uri("http://www.hi-pda.com/forum/uc_server/data/avatar/" + s[8] + s[7] + s[6] + "/" + s[5] + s[4] + "/" + s[3] + s[2] + "/" + s[1] + s[0] + "_avatar_middle.jpg");
            }
        }

        public string FloorNoStr
        {
            get
            {
                return string.Format("{0}#", FloorNo);
            }
        }

        public object XamlContent
        {
            get
            {
                return XamlReader.Load(XamlStr);
            }
        }
    }
}
