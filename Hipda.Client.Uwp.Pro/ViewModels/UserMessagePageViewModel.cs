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
    public class UserMessagePageViewModel : NotificationObject
    {
        int limitCount = 3;

        DataService _ds;

        int _userId;
        public int UserId
        {
            get
            {
                return _userId;
            }
        }


        private string _tipText = string.Empty;

        public string TipText
        {
            get { return _tipText; }
            set
            {
                _tipText = value;
                this.RaisePropertyChanged("TipText");
            }
        }


        private bool _isProgressRingActive = true;

        public bool IsProgressRingActive
        {
            get { return _isProgressRingActive; }
            set
            {
                _isProgressRingActive = value;
                this.RaisePropertyChanged("IsProgressRingActive");
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

        public UserMessagePageViewModel(int userId)
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
            if (data.Total == 0)
            {
                TipText = "你们之间还没有开始。。。";
            }
            else
            {
                TipText = string.Empty;
                ListData = data.ListData;
                if (count == -1)
                {
                    IsShowLoadMoreButton = Visibility.Collapsed;
                }
                else
                {
                    IsShowLoadMoreButton = data.Total > count ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            IsProgressRingActive = false;
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
