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
        public DataTemplate ReplyListLastItemTemplate { get; set; }
        public DataTemplate ReplyListTopItemTemplate { get; set; }
        public DataTemplate ReplyListLeftItemTemplate { get; set; }
        public DataTemplate ReplyListLeft2ItemTemplate { get; set; }
        public DataTemplate ReplyListRightItemTemplate { get; set; }
        public DataTemplate ReplyListHighLightItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore (object item, DependencyObject container)
        {
            if (item == null)
            {
                return null;
            }

            var data = (ReplyItemModel)item;
            if (data.IsLast)
            {
                return ReplyListLastItemTemplate;
            }
            else if (data.FloorNo == 1)
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