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
    public class NavButtonListItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NavButtonBySearchItemTemplate { get; set; }
        public DataTemplate NavButtonByForumIdItemTemplate { get; set; }
        public DataTemplate NavButtonByMyItemItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore (object item, DependencyObject container)
        {
            var data = (NavButtonItemModel)item;

            if (data.TypeValue.StartsWith("item="))
            {
                return NavButtonByMyItemItemTemplate;
            }
            else if (data.TypeValue.StartsWith("fid="))
            {
                return NavButtonByForumIdItemTemplate;
            }
            return NavButtonBySearchItemTemplate;
        }

    }
}