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
    public class ThreadListItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ThreadListNullItemTemplate { get; set; }
        public DataTemplate ThreadListNormalItemTemplate { get; set; }
        public DataTemplate ThreadListMyThreadsItemTemplate { get; set; }
        public DataTemplate ThreadListMyPostsItemTemplate { get; set; }
        public DataTemplate ThreadListMyFavoritesItemTemplate { get; set; }
        public DataTemplate ThreadListSearchTitleItemTemplate { get; set; }
        public DataTemplate ThreadListSearchFullTextItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item == null)
            {
                return null;
            }

            var data = (ThreadItemModelBase)item;
            switch (data.ThreadType)
            {
                case ThreadDataType.MyThreads:
                    return ThreadListMyThreadsItemTemplate;
                case ThreadDataType.MyPosts:
                    return ThreadListMyPostsItemTemplate;
                case ThreadDataType.MyFavorites:
                    return ThreadListMyFavoritesItemTemplate;
                case ThreadDataType.SearchTitle:
                    return ThreadListSearchTitleItemTemplate;
                case ThreadDataType.SearchFullText:
                    return ThreadListSearchFullTextItemTemplate;
                default:
                    return ThreadListNormalItemTemplate;
            }
        }
    }
}
