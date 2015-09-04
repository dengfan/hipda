using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hipda.Client.Uwp.Pro.Models;
using Windows.UI.Xaml.Data;
using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Services;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadItemViewModel : NotificationObject
    {
        public ThreadItemModel ThreadItem { get; set; }

        public ICollectionView ReplyItemCollection { get; set; }

        public void SelectThreadItem(Action beforeLoad, Action afterLoad)
        {
            var ds = new DataService();
            var cv = ds.GetViewForReplyPage(ThreadItem.ThreadId, beforeLoad, afterLoad);
            if (cv != null)
            {
                ReplyItemCollection = cv;
            }
        }
    }
}
