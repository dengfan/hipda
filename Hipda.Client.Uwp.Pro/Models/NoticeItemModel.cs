using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.Models
{
    public enum NoticeType
    {
        /// <summary>
        /// 被引用或回复
        /// </summary>
        QuoteOrReply,

        /// <summary>
        /// 关注的主题被回复
        /// </summary>
        Thread,

        /// <summary>
        /// 被加为好友
        /// </summary>
        Buddy
    }

    public class NoticeItemModel
    {
        public NoticeItemModel(NoticeType noticeType, bool isNew, string username, string actionTime, string[] actionInfo)
        {
            NoticeType = noticeType;
            IsNew = isNew;
            Username = username;
            ActionTime = actionTime;
            ActionInfo = actionInfo;
        }

        public NoticeType NoticeType { get; set; }
        public bool IsNew { get; set; }
        public string Username { get; set; }
        public string ActionTime { get; set; }
        public string[] ActionInfo { get; set; }

        public SolidColorBrush StatusColorBrush
        {
            get
            {
                return IsNew ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Transparent);
            }
        }
    }
}
