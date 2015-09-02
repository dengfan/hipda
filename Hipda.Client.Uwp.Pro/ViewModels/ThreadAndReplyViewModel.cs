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
        private ICollectionView _threadItemCollection;

        public ICollectionView ThreadItemCollection
        {
            get { return _threadItemCollection; }
            set
            {
                _threadItemCollection = value;
                this.RaisePropertyChanged("ItemCollection");
            }
        }

        private void LoadThreadPageList(Action showProgressBar, Action hideProgressBar)
        {
            var ds = new DataService();
            var cv = ds.GetViewForThreadPage(14, showProgressBar, hideProgressBar);
            if (cv != null)
            {
                ThreadItemCollection = cv;
            }
        }

        public ThreadAndReplyViewModel(Action showProgressBar, Action hideProgressBar)
        {
            LoadThreadPageList(showProgressBar, hideProgressBar);
        }
    }
}
