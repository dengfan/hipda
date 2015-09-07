using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Hipda.Client.Uwp.Pro.Views
{
    public class ReplyListItemSelector : DataTemplateSelector
    {
        public DataTemplate LeftTemplate { get; set; }
        public DataTemplate RightTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore
            (object item, DependencyObject container)
        {
            ReplyItemModel data = (ReplyItemModel)item;

            if (data.IsTopicStarter)
            {
                return LeftTemplate;
            }
            else
            {
                return RightTemplate;
            }
        }

    }
}