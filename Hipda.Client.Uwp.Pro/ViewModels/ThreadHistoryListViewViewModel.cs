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
    public class ThreadHistoryListViewViewModel
    {
        public DelegateCommand ClearHistoryCommand { get; set; }

        public ObservableCollection<ThreadItemModelBase> ThreadHistoryData
        {
            get
            {
                return DataService.ThreadHistoryData;
            }
        }

        public ThreadHistoryListViewViewModel()
        {
            ClearHistoryCommand = new DelegateCommand();
            ClearHistoryCommand.ExecuteAction = (p) => {
                DataService.ThreadHistoryData.Clear();
            };
        }
    }
}
