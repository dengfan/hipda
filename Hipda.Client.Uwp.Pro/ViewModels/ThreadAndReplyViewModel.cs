using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadAndReplyViewModel : NotificationObject
    {
        public ICollectionView ThreadItemCollection { get; set; }

        private void LoadThreadPageList(Action beforeLoad, Action afterLoad)
        {
            var ds = new DataService();
            var cv = ds.GetViewForThreadPage(14, beforeLoad, afterLoad);
            if (cv != null)
            {
                ThreadItemCollection = cv;
            }
        }

        public ThreadAndReplyViewModel(Action beforeLoad, Action afterLoad)
        {
            LoadThreadPageList(beforeLoad, afterLoad);
        }
    }
}
