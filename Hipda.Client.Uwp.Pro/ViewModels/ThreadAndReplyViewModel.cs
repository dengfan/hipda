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

        //private IEnumerable<ThreadItemModel> _threadItemList;

        //public IEnumerable<ThreadItemModel> ThreadItemList
        //{
        //    get { return _threadItemList; }
        //    set
        //    {
        //        _threadItemList = value;
        //        this.RaisePropertyChanged("ThreadItemList");
        //    }
        //}

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


        private void LoadThreadPageList()
        {
            var ds = new DataService();
            var cts = new CancellationTokenSource();
            var cvs = ds.GetViewForThreadPage(14);
            if (cvs != null)
            {
                ThreadItemCollection = cvs.View;
            }
        }

        public ThreadAndReplyViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            LoadThreadPageList();
        }
    }
}
