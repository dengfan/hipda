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
        public DataTemplate NormalTemplate { get; set; }
        public DataTemplate NormalTemplate2 { get; set; }

        public DataTemplate MyThreadsTemplate { get; set; }
        public DataTemplate MyPostsTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            ThreadItemViewModelBase b = (ThreadItemViewModelBase)item;
            switch (b.ThreadDataType)
            {
                case ThreadDataType.MyThreads:
                    //var data2 = (ThreadItemViewModel)item;
                    return MyThreadsTemplate;
                case ThreadDataType.MyPosts:
                    //var data3 = (ThreadItemViewModel)item;
                    return MyPostsTemplate;
                default:
                    return NormalTemplate;
            }
        }
    }
}
