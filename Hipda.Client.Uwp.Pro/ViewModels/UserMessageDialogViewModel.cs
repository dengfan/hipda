using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class UserMessageDialogViewModel : NotificationObject
    {
        DataService _ds;

        int _userId;
        public int UserId
        {
            get
            {
                return _userId;
            }
        }

        ObservableCollection<UserMessageItemModel> _listData;
        public ObservableCollection<UserMessageItemModel> ListData
        {
            get
            {
                return _listData;
            }
            set
            {
                _listData = value;
                this.RaisePropertyChanged("ListData");
            }
        }

        private Visibility _isShowLoadMoreButton = Visibility.Collapsed;

        public Visibility IsShowLoadMoreButton
        {
            get { return _isShowLoadMoreButton; }
            set
            {
                _isShowLoadMoreButton = value;
                this.RaisePropertyChanged("IsShowLoadMoreButton");
            }
        }

        public UserMessageDialogViewModel(int userId)
        {
            _ds = new DataService();
            _userId = userId;

            GetData();

            LoadMoreCommand = new DelegateCommand();
            LoadMoreCommand.ExecuteAction = new Action<object>(LoadMoreExecute);

            RefreshCommand = new DelegateCommand();
            RefreshCommand.ExecuteAction = new Action<object>(RefreshExecute);
        }

        public DelegateCommand LoadMoreCommand { get; set; }
        public DelegateCommand RefreshCommand { get; set; }

        async void LoadMoreExecute(object parameter)
        {
            var data = await _ds.GetUserMessageData(_userId, -1);
            ListData = data.ListData;
            IsShowLoadMoreButton = data.Total > 5 ? Visibility.Collapsed : Visibility.Visible;
        }

        void RefreshExecute(object parameter)
        {
            GetData();
        }

        async void GetData()
        {
            var data = await _ds.GetUserMessageData(_userId, 5);
            ListData = data.ListData;
            IsShowLoadMoreButton = data.Total > 5 ? Visibility.Visible : Visibility.Collapsed;
        }

        public async void PostUserMessage(string message, int userId)
        {
            var data = await _ds.PostUserMessage(message, userId);
            if (data != null)
            {
                ListData.Add(data);
            }
        }
    }
}
