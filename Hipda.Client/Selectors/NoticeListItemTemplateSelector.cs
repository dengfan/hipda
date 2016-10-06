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
    public class NoticeListItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NoticeQuoteOrReplyItemTemplate { get; set; }
        public DataTemplate NoticeThreadItemTemplate { get; set; }
        public DataTemplate NoticeBuddyItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var b = (NoticeItemModel)item;
            switch (b.NoticeType)
            {
                case NoticeType.QuoteOrReply:
                    return NoticeQuoteOrReplyItemTemplate;
                case NoticeType.Thread:
                    return NoticeThreadItemTemplate;
                case NoticeType.Buddy:
                    return NoticeBuddyItemTemplate;
                default:
                    return NoticeQuoteOrReplyItemTemplate;
            }
        }
    }
}
