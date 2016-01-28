using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Hipda.Client.Uwp.Pro
{
    public class ReplyListItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ReplyListTopItemTemplate { get; set; }
        public DataTemplate ReplyListLeftItemTemplate { get; set; }
        public DataTemplate ReplyListLeft2ItemTemplate { get; set; }
        public DataTemplate ReplyListRightItemTemplate { get; set; }
        public DataTemplate ReplyListHighLightItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore (object item, DependencyObject container)
        {
            ReplyItemModel data = (ReplyItemModel)item;

            // 如果当前用户已被屏蔽，则不赋予模板
            if (data.AuthorUserId == -1)
            {
                return null;
            }

            if (data.FloorNo == 1)
            {
                return ReplyListTopItemTemplate;
            }
            else if (data.IsHighLight)
            {
                return ReplyListHighLightItemTemplate;
            }
            else if (data.AuthorUserId == AccountService.UserId)
            {
                return ReplyListRightItemTemplate;
            }
            else if (data.IsTopicStarter)
            {
                return ReplyListLeft2ItemTemplate;
            }
            else
            {
                return ReplyListLeftItemTemplate;
            }
        }

    }
}