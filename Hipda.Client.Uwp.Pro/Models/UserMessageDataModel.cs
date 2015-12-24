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
        public string TextStr { get; set; }
        public string HtmlStr { get; set; }
        public string XamlStr { get; set; }
        public object XamlContent
        {
            get
            {
                try
                {
                    return XamlReader.Load(XamlStr) as FrameworkElement;
                }
                catch
                {
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

    public class UserMessageDataModel
    {
        public List<UserMessageItemModel> ListData;
        public int Total;
    }
}
