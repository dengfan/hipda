using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Hipda.Client.Uwp.Pro
{
    public class ReplyListItemSelector : DataTemplateSelector
    {
        public DataTemplate TopTemplate { get; set; }
        public DataTemplate LeftTemplate { get; set; }
        public DataTemplate Left2Template { get; set; }
        public DataTemplate RightTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore (object item, DependencyObject container)
        {
            ReplyItemModel data = (ReplyItemModel)item;

            if (data.FloorNo == 1)
            {
                return TopTemplate;
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