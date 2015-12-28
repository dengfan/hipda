using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace Hipda.Client.Uwp.Pro.Models
{
    /// <summary>
    /// 短消息列表项之数据模型
    /// </summary>
    public class UserMessageListItemModel
    {
        public UserMessageListItemModel(int index, int pageNo, int userId, string username, string lastMessageTime, string lastMessageText)
        {
            this.Index = index;
            this.PageNo = pageNo;
            this.UserId = userId;
            this.Username = username;
            this.LastMessageTime = lastMessageTime;
            this.LastMessageText = lastMessageText;
        }
        public int Index { get; set; }
        public int PageNo { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string LastMessageTime { get; set; }
        public string LastMessageText { get; set; }

        public Uri AvatarUri
        {
            get
            {
                return Common.GetAvatarUriByUserId(UserId);
            }
        }
    }

    /// <summary>
    /// 短消息项之数据模型
    /// </summary>
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
        public ObservableCollection<UserMessageItemModel> ListData;
        public int Total;
    }
}
