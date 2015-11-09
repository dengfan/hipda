using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ReplyItemModel
    {
        public ReplyItemModel(int index, int floorNo, int postId, int pageNo, int threadId, string threadTitle, int threadAuthorUserId, int authorUserId, string authorUsername, string textContent, string htmlContent, string xamlConent, string authorCreateTime, int imageCount, int linkCount, Action<int> linkClickEvent)
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
            this.LinkCount = linkCount;
            this.LinkClickEvent = linkClickEvent;
        }

        public int Index { get; private set; }
        public int FloorNo { get; private set; }
        public int PostId { get; private set; }
        public int PageNo { get; private set; }
        public int ThreadId { get; private set; }
        public string ThreadTitle { get; private set; }
        public int ThreadAuthorUserId { get; private set; }
        public string AuthorUsername { get; private set; }

        public int AuthorUserId { get; private set; }

        public string TextStr { get; private set; }

        public string HtmlStr { get; private set; }

        public string XamlStr { get; set; }

        public string AuthorCreateTime { get; private set; }

        public int ImageCount { get; set; }

        public int LinkCount { get; set; }

        public Action<int> LinkClickEvent { get; set; }

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
                    var element = XamlReader.Load(XamlStr) as FrameworkElement;
                    for (int i = 0; i < LinkCount; i++)
                    {
                        var myLink = element.FindName(string.Format("MyLink_{0}", i)) as MyLink;
                        if (myLink != null && LinkClickEvent != null)
                        {
                            myLink.MyLinkClick = LinkClickEvent;
                        }
                    }

                    return element;
                }
                catch
                {
                    string errorReport = string.Format("http://www.hi-pda.com/forum/viewthread.php?tid={0} 楼层{1}内容解析出错。", ThreadId, FloorNo);
                    Uri uri = new Uri("mailto:appxking@outlook.com?subject=发送出错信息给开发者，以帮助开发者更好的解决问题&body=" + errorReport, UriKind.Absolute);
                    Launcher.LaunchUriAsync(uri);

                    string text = Regex.Replace(TextStr, @"[^a-zA-Z\d\u4e00-\u9fa5]", " ");
                    XamlStr = string.Format("<RichTextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph>{0}</Paragraph></RichTextBlock>", text);
                    return XamlReader.Load(XamlStr);
                }
            }
        }
    }
}
