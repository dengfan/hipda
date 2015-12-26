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
using Windows.UI.Xaml.Controls;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class UserMessageDialogViewModel : NotificationObject
    {
        int limitCount = 5;

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

            GetData(limitCount);

            LoadMoreCommand = new DelegateCommand();
            LoadMoreCommand.ExecuteAction = new Action<object>(LoadMoreExecute);

            RefreshCommand = new DelegateCommand();
            RefreshCommand.ExecuteAction = new Action<object>(RefreshExecute);
        }

        public DelegateCommand LoadMoreCommand { get; set; }
        public DelegateCommand RefreshCommand { get; set; }

        void LoadMoreExecute(object parameter)
        {
            GetData(-1);
        }

        void RefreshExecute(object parameter)
        {
            GetData(limitCount);
        }

        async void GetData(int count)
        {
            var data = await _ds.GetUserMessageData(_userId, count);
            ListData = data.ListData;
            IsShowLoadMoreButton = data.Total > count ? Visibility.Visible : Visibility.Collapsed;
        }

        public async Task<bool> PostUserMessage(string message, int userId)
        {
            var data = await _ds.PostUserMessage(message, userId);
            if (data != null)
            {
                ListData.Add(data);
                return true;
            }

            return false;
        }
    }
}
