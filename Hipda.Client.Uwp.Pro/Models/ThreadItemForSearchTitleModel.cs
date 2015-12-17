using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ThreadItemForSearchTitleModel : ThreadItemModelBase
    {
        public ThreadItemForSearchTitleModel(int index, string searchKeyword, string forumName, int threadId, int pageNo, string title, int attachFileType, string replyCount, string viewCount, string authorUsername, int authorUserId, string authorCreateTime, string lastReplyUsername, string lastReplyTime)
        {
            this.Index = index;
            this.SearchKeyword = searchKeyword;
            this.ForumName = forumName;
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
        }

        public int Index { get; private set; }

        public string SearchKeyword { get; private set; }

        public int ForumId { get; private set; }

        public string ForumName { get; private set; }

        public int PageNo { get; private set; }

        public string ReplyCount { get; private set; }

        public string ViewCount { get; private set; }

        public int AttachFileType { get; private set; }

        public string AuthorUsername { get; private set; }

        public int AuthorUserId { get; private set; }

        public string AuthorCreateTime { get; private set; }

        public string LastReplyUsername { get; private set; }

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
                return string.Format("{0} {1}", LastReplyUsername, LastReplyTime);
            }
        }

        public string ImageFontIcon
        {
            get
            {
                return AttachFileType == 1 ? "\uEB9F" : string.Empty;
            }
        }

        public string FileFontIcon
        {
            get
            {
                return AttachFileType == 2 ? "\uE16C" : string.Empty;
            }
        }

        public TextBlock TitleControl
        {
            get
            {
                string title = Title;
                string xaml = @"<TextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Foreground=""{{ThemeResource SystemControlForegroundBaseMediumBrush}}"" TextWrapping=""Wrap""><Run FontFamily=""Segoe MDL2 Assets"" Foreground=""OrangeRed"" Text=""{1}"" /><Run FontFamily=""Segoe MDL2 Assets"" Foreground=""DeepSkyBlue"" Text=""{2}"" /> {0} <Run Text=""{3}"" Foreground=""{{ThemeResource SystemControlBackgroundAccentBrush}}"" /></TextBlock>";

                MatchCollection matchsForSearchKeywords = new Regex(@"<em style=""color:red;"">([^>#]*)</em>").Matches(title);
                if (matchsForSearchKeywords != null && matchsForSearchKeywords.Count > 0)
                {
                    for (int j = 0; j < matchsForSearchKeywords.Count; j++)
                    {
                        var m = matchsForSearchKeywords[j];

                        string placeHolder = m.Groups[0].Value; // 要被替换的元素
                        string k = m.Groups[1].Value;

                        string linkXaml = string.Format(@"<Run Foreground=""Red"">{0}</Run>", k);
                        title = title.Replace(placeHolder, linkXaml);
                    }
                }

                xaml = string.Format(xaml, title, ImageFontIcon, FileFontIcon, ViewInfo);
                return XamlReader.Load(xaml) as TextBlock;
            }
        }
    }
}
