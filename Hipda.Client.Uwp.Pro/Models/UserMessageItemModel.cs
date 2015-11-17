using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class UserMessageItemModel
    {
        public bool IsRead { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public int LinkCount { get; set; }
        public Action<int> LinkClickEvent { get; set; }
        public string TextStr { get; set; }
        public string HtmlStr { get; set; }
        public string XamlStr { get; set; }
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
                    //string errorReport = string.Format("http://www.hi-pda.com/forum/viewthread.php?tid={0} 楼层{1}内容解析出错。", ThreadId, FloorNo);
                    //Uri uri = new Uri("mailto:appxking@outlook.com?subject=发送出错信息给开发者，以帮助开发者更好的解决问题&body=" + errorReport, UriKind.Absolute);
                    //Launcher.LaunchUriAsync(uri);

                    string text = Regex.Replace(TextStr, @"[^a-zA-Z\d\u4e00-\u9fa5]", " ");
                    XamlStr = string.Format("<RichTextBlock xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph>{0}</Paragraph></RichTextBlock>", text);
                    return XamlReader.Load(XamlStr);
                }
            }
        }

        public string IsReadInfo
        {
            get
            {
                return IsRead ? string.Empty : " 对方未读";
            }
        }
    }
}
