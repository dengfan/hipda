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
    public class ThreadAndReplyViewModel : NotificationObject
    {

        private IEnumerable<ThreadItemModel> _threadItemList;

        public IEnumerable<ThreadItemModel> ThreadItemList
        {
            get { return _threadItemList; }
            set
            {
                _threadItemList = value;
                this.RaisePropertyChanged("ThreadItemList");
            }
        }

        private async Task LoadThreadPageList()
        {
            var ds = new DataService();
            var cts = new CancellationTokenSource();
            ThreadItemList = await ds.GetThreadPageListByForumId(14, 1, cts);
        }

        public ThreadAndReplyViewModel()
        {
            Initialize();
        }

        private async void Initialize()
        {
            await LoadThreadPageList();
        }
    }
}
