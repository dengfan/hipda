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
    public class UserMessageListItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserMessageLeftItemTemplate { get; set; }
        public DataTemplate UserMessageRightItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore (object item, DependencyObject container)
        {
            UserMessageItemModel data = (UserMessageItemModel)item;

            if (data.UserId == 0)
            {
                return UserMessageRightItemTemplate;
            }

            return UserMessageLeftItemTemplate;
        }

    }
}