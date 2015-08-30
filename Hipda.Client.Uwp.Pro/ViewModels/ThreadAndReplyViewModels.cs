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

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadAndReplyViewModels : NotificationObject
    {

        private ObservableCollection<ThreadItemModel> threadPageList;

        public ObservableCollection<ThreadItemModel> ThreadPageList
        {
            get { return threadPageList; }
            set
            {
                threadPageList = value;
                this.RaisePropertyChanged("ThreadPageList");
            }
        }

        private async Task LoadThreadPageList()
        {
            var ds = new DataService();
            var cts = new CancellationTokenSource();
            await ds.GetThreadPageListByForumId("2", 1, cts);
        }


    }
}
