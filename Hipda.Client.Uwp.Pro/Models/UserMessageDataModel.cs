using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;

namespace Hipda.Client.Uwp.Pro.Models
{
    /// <summary>
    /// 短消息列表项之数据模型
    /// </summary>
    public class UserMessageListItemModel
    {
        public UserMessageListItemModel(int index, bool isNew, int pageNo, int userId, string username, string lastMessageTime, string lastMessageText)
        {
            this.Index = index;
            this.IsNew = isNew;
            this.PageNo = pageNo;
            this.UserId = userId;
            this.Username = username;
            this.LastMessageTime = lastMessageTime;
            this.LastMessageText = lastMessageText;
        }
        public int Index { get; set; }
        public bool IsNew { get; set; }
        public int PageNo { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string LastMessageTime { get; set; }

        private string _lastMessageText;
        public string LastMessageText
        {
            get
            {
                return CommonService.ReplaceEmojiLabel(_lastMessageText);
            }
            set
            {
                _lastMessageText = value;
            }
        }

        public Uri AvatarUri
        {
            get
            {
                return CommonService.GetMiddleAvatarUriByUserId(UserId);
            }
        }

        public string NewLabel
        {
            get
            {
                return IsNew ? "NEW" : string.Empty;
            }
        }
    }

    /// <summary>
    /// 短消息项之数据模型
    /// </summary>
    public class UserMessageItemModel
    {
        public UserMessageItemModel(bool isRead, int userId, string username, string date, string time, string xamlStr, int inAppLinkCount)
        {
            this.IsRead = isRead;
            this.UserId = userId;
            this.Username = username;
            this.Date = date;
            this.Time = time;
            this.XamlStr = xamlStr;
            this.InAppLinkCount = inAppLinkCount;
        }

        public bool IsRead { get; private set; }
        public int UserId { get; private set; }
        public string Username { get; private set; }
        public string Date { get; private set; }
        public string Time { get; private set; }
        public string XamlStr { get; private set; }
        public int InAppLinkCount { get; private set; }
        public RichTextBlock XamlContent
        {
            get
            {
                var rtb = (RichTextBlock)XamlReader.Load(CommonService.ReplaceEmojiLabel(XamlStr));
                for (int i = 1; i <= InAppLinkCount; i++)
                {
                    var key = $"InAppLink_{Username}_{Time}_{i}";
                    var hyperLink = (Hyperlink)rtb.FindName(key);
                    if (hyperLink != null)
                    {
                        hyperLink.Click += ReplyItemModel.InAppLink_Click;
                    }
                }
                return rtb;
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
