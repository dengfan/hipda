using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Hipda.Client.Models;
using Hipda.Client.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Hipda.Client.ViewModels
{
    public class UserMessagePageViewModel : ViewModelBase
    {
        int limitCount = 3;

        UserMessageService _ds;

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
                Set(ref _tipText, value);
            }
        }


        private bool _isProgressRingActive = true;

        public bool IsProgressRingActive
        {
            get { return _isProgressRingActive; }
            set
            {
                Set(ref _isProgressRingActive, value);
            }
        }


        ObservableCollection<UserMessageItemModel> _listData = new ObservableCollection<UserMessageItemModel>();
        public ObservableCollection<UserMessageItemModel> ListData
        {
            get
            {
                return _listData;
            }
            set
            {
                Set(ref _listData, value);
            }
        }

        private Visibility _isShowLoadMoreButton = Visibility.Collapsed;

        public Visibility IsShowLoadMoreButton
        {
            get { return _isShowLoadMoreButton; }
            set
            {
                Set(ref _isShowLoadMoreButton, value);
            }
        }

        private string _newMessage;

        public string NewMessage
        {
            get { return _newMessage; }
            set
            {
                Set(ref _newMessage, value);
            }
        }


        public ICommand LoadMoreCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SubmitCommand { get; set; }

        public UserMessagePageViewModel(int userId)
        {
            _ds = new UserMessageService();
            _userId = userId;

            GetData(limitCount);

            LoadMoreCommand = new RelayCommand(LoadMoreExecute);

            RefreshCommand = new RelayCommand(RefreshExecute);

            SubmitCommand = new RelayCommand(SubmitExecute);
        }

        void LoadMoreExecute()
        {
            GetData(-1);
        }

        void RefreshExecute()
        {
            GetData(limitCount);
        }

        async void SubmitExecute()
        {
            if (string.IsNullOrEmpty(NewMessage))
            {
                return;
            }

            var data = await _ds.PostUserMessage(NewMessage, UserId);
            if (data != null)
            {
                ListData.Add(data);
                TipText = NewMessage = string.Empty;
            }
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
    }
}
