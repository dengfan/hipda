using Hipda.Client.Models;
using Hipda.Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Hipda.Client
{
    public class AttachFileGridItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ImageItemTemplate { get; set; }
        public DataTemplate FileItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var b = (AttachFileItemModel)item;
            if (b.FileType == 0)
            {
                return ImageItemTemplate;
            }
            else if (b.FileType == 1)
            {
                return FileItemTemplate;
            }

            return null;
        }
    }
}
