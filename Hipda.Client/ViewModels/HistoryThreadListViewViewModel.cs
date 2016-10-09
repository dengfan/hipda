using GalaSoft.MvvmLight.Command;
using Hipda.Client.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Hipda.Client.ViewModels
{
    public class HistoryThreadListViewViewModel
    {
        public ICommand ClearHistoryCommand { get; set; }

        public ICommand RemoveSelectedCommand { get; set; }

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
            ClearHistoryCommand = new RelayCommand(() =>
            {
                _threadHistoryData.Clear();
            });

            RemoveSelectedCommand = new RelayCommand(() =>
            {
                if (SelectedOne != null)
                {
                    _threadHistoryData.Remove(SelectedOne);
                }
            });
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
