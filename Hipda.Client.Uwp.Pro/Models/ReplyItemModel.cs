using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ReplyItemModel
    {
        public ReplyItemModel(int index, int floorNo, int postId, int pageNo, int threadId, string threadTitle, int threadAuthorUserId, int authorUserId, string authorUsername, string textContent, string htmlContent, string xamlConent, string authorCreateTime, int imageCount, bool isHighLight)
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
            this.AuthorCreateTime = authorCreateTime;
            this.ImageCount = imageCount;
            this.IsHighLight = isHighLight;
        }

        public int Index { get; private set; }
        public int FloorNo { get; private set; }
        public int PostId { get; private set; }
        public int PageNo { get; private set; }
        public int ThreadId { get; private set; }
        public string ThreadTitle { get; set; }
        public int ThreadAuthorUserId { get; set; }
        public string AuthorUsername { get; private set; }

        public int AuthorUserId { get; private set; }

        public string TextStr { get; private set; }

        public string HtmlStr { get; private set; }

        public string XamlStr { get; set; }

        public string AuthorCreateTime { get; private set; }

        public int ImageCount { get; set; }

        public bool IsHighLight { get; private set; }

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
                try
                {
                    return XamlReader.Load(XamlStr);
                }
                catch
                {
                    string errorDetails = string.Format("http://www.hi-pda.com/forum/viewthread.php?tid={0} 楼层{1}内容解析出错。\r\n{2}", ThreadId, FloorNo, XamlStr);
                    Common.PostErrorEmailToDeveloper("回复内容解析出现异常", errorDetails);

                    string text = Regex.Replace(TextStr, @"[^a-zA-Z\d\u4e00-\u9fa5]", " ");
                    XamlStr = string.Format("<RichTextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph>{0}</Paragraph></RichTextBlock>", text);
                    return XamlReader.Load(XamlStr);
                }
            }
        }

        public Uri AvatarUri
        {
            get
            {
                return Common.GetAvatarUriByUserId(AuthorUserId);
            }
        }
    }
}
