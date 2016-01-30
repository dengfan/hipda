using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ThreadItemForSearchFullTextModel : ThreadItemModelBase
    {
        public ThreadItemForSearchFullTextModel(int index, int postId, string searchResultSummaryHtml, string forumName, int pageNo, string titleHtml, string replyCount, string viewCount, string authorUsername, int authorUserId, string lastReplyTime)
        {
            this.Index = index;
            this.PostId = postId;
            this.SearchResultSummaryHtml = searchResultSummaryHtml;
            this.ForumName = forumName;
            this.PageNo = pageNo;
            this.Title = titleHtml;
            this.ReplyCount = replyCount;
            this.ViewCount = viewCount;
            this.AuthorUsername = authorUsername;
            this.AuthorUserId = authorUserId;
            this.LastReplyTime = lastReplyTime;
            this.ThreadType = ThreadDataType.SearchFullText;
        }

        public int Index { get; private set; }

        public int PostId { get; private set; }

        public string SearchResultSummaryHtml { get; private set; }

        public string ForumName { get; private set; }

        public int PageNo { get; private set; }

        public string ReplyCount { get; private set; }

        public string ViewCount { get; private set; }

        public string AuthorUsername { get; private set; }

        public int AuthorUserId { get; private set; }

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
                return LastReplyTime;
            }
        }

        public object TitleControl
        {
            get
            {
                string xaml = Html.HtmlToXaml.ConvertSearchResultSummary(Title, ForumName, SearchResultSummaryHtml, ViewInfo);
                return XamlReader.Load(xaml);
            }
        }
    }
}
