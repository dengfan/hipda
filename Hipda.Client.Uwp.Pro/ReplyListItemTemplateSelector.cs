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
        public DataTemplate TopTemplate { get; set; }
        public DataTemplate LeftTemplate { get; set; }
        public DataTemplate Left2Template { get; set; }
        public DataTemplate RightTemplate { get; set; }
        public DataTemplate HighLightTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore (object item, DependencyObject container)
        {
            ReplyItemModel data = (ReplyItemModel)item;

            if (data.FloorNo == 1)
            {
                return TopTemplate;
            }
            else if (data.IsHighLight)
            {
                return HighLightTemplate;
            }
            else if (data.AuthorUserId == AccountService.UserId)
            {
                return RightTemplate;
            }
            else if (data.IsTopicStarter)
            {
                return Left2Template;
            }
            else
            {
                return LeftTemplate;
            }
        }

    }
}