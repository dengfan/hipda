using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Hipda.Client.Uwp.Pro
{
    public class ThreadListItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TopTemplate { get; set; }
        public DataTemplate PageTemplate { get; set; }
        public DataTemplate NormalTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            ThreadItemViewModel data = (ThreadItemViewModel)item;

            //if (data.ThreadItem.Index % 75 == 0)
            //{
            //    return PageTemplate;
            //}
            //else
            //{
                return NormalTemplate;
            //}
        }
    }
}
