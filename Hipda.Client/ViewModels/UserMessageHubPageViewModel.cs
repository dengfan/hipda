using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Hipda.Client.Models;
using Hipda.Client.Services;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.ViewModels
{
    public class UserMessageHubPageViewModel : ViewModelBase
    {
        UserMessageHubService _ds;

        public ICommand RefreshCommand { get; set; }
        public ICommand DeleteCommand { get; set; }


        private bool _isProgressRingActive = true;

        public bool IsProgressRingActive
        {
            get { return _isProgressRingActive; }
            set
            {
                Set(ref _isProgressRingActive, value);
            }
        }

        private ICollectionView _dataView;

        public ICollectionView DataView
        {
            get { return _dataView; }
            set
            {
                Set(ref _dataView, value);
            }
        }

        private void AfterLoaded()
        {
            IsProgressRingActive = false;
        }

        public UserMessageHubPageViewModel(ListView listView)
        {
            _ds = new UserMessageHubService();

            GetData();

            RefreshCommand = new RelayCommand(() =>
            {
                _ds.ClearUserMessageListData();
                DataView = _ds.GetViewForUserMessageList(1, AfterLoaded, null);
            });

            DeleteCommand = new RelayCommand(async () =>
            {
                if (listView.SelectedItems == null)
                {
                    return;
                }

                List<int> selectedUserIds = new List<int>();
                foreach (UserMessageListItemModel item in listView.SelectedItems)
                {
                    selectedUserIds.Add(item.UserId);
                }

                bool isOk = await _ds.DeleteUserMessageListItem(selectedUserIds);
                if (isOk)
                {
                    _ds.ClearUserMessageListData();
                    DataView = _ds.GetViewForUserMessageList(1, AfterLoaded, null);
                }
            });
        }

        void GetData()
        {
            DataView = _ds.GetViewForUserMessageList(1, AfterLoaded, null);
        }
    }
}
