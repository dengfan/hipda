using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadHistoryListBoxViewModel : NotificationObject
    {
        public DelegateCommand ClearHistoryCommand { get; set; }

        ObservableCollection<ThreadItemModelBase> _threadHistoryData = new ObservableCollection<ThreadItemModelBase>();

        public ObservableCollection<ThreadItemModelBase> ThreadHistoryData
        {
            get
            {
                return _threadHistoryData;
            }
            set
            {
                _threadHistoryData = value;
            }
        }

        public ThreadHistoryListBoxViewModel()
        {
            ClearHistoryCommand = new DelegateCommand();
            ClearHistoryCommand.ExecuteAction = (p) => {
                _threadHistoryData.Clear();
            };
        }

        public void Add(ThreadItemModelBase item)
        {
            if (!_threadHistoryData.Any(t => t.ThreadId == item.ThreadId))
            {
                _threadHistoryData.Add(item);
                this.RaisePropertyChanged("LastItemIndex");
            }
        }

        public int LastItemIndex
        {
            get
            {
                if (ThreadHistoryData.Count > 0)
                {
                    return ThreadHistoryData.Count - 1;
                }

                return -1;
            }
        }
    }
}
