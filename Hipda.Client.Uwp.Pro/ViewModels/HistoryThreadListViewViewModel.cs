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
    public class HistoryThreadListViewViewModel
    {
        public DelegateCommand ClearHistoryCommand { get; set; }

        public DelegateCommand RemoveSelectedCommand { get; set; }

        private ObservableCollection<ThreadItemModelBase> _threadHistoryData = new ObservableCollection<ThreadItemModelBase>();

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

        private ThreadItemModelBase _selectedOne;

        public ThreadItemModelBase SelectedOne
        {
            get { return _selectedOne; }
            set { _selectedOne = value; }
        }

        public HistoryThreadListViewViewModel()
        {
            ClearHistoryCommand = new DelegateCommand();
            ClearHistoryCommand.ExecuteAction = (p) => {
                _threadHistoryData.Clear();
            };

            RemoveSelectedCommand = new DelegateCommand();
            RemoveSelectedCommand.ExecuteAction = (p) => {
                if (SelectedOne != null)
                {
                    _threadHistoryData.Remove(SelectedOne);
                }
            };
        }

        public void Add(ThreadItemModelBase item)
        {
            if (!_threadHistoryData.Any(t => t.ThreadId == item.ThreadId))
            {
                _threadHistoryData.Add(item);
            }
        }
    }
}
