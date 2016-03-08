using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class UserMessageHubPageViewModel : NotificationObject
    {
        UserMessageHubService _ds;

        public DelegateCommand RefreshCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }


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

        private IList<object> _selecteUserMessageListItems;

        public IList<object> SelectedUserMessageListItems
        {
            get { return _selecteUserMessageListItems; }
            set { _selecteUserMessageListItems = value; }
        }


        private ICollectionView _dataView;

        public ICollectionView DataView
        {
            get { return _dataView; }
            set
            {
                _dataView = value;
                this.RaisePropertyChanged("DataView");
            }
        }

        private void AfterLoaded()
        {
            IsProgressRingActive = false;
        }

        public UserMessageHubPageViewModel()
        {
            _ds = new UserMessageHubService();

            GetData();

            RefreshCommand = new DelegateCommand();
            RefreshCommand.ExecuteAction = (p) => {
                _ds.ClearUserMessageListData();
                DataView = _ds.GetViewForUserMessageList(1, AfterLoaded, null);
            };

            DeleteCommand = new DelegateCommand();
            DeleteCommand.ExecuteAction = async (p) => {
                if (SelectedUserMessageListItems == null)
                {
                    return;
                }

                List<int> selectedUserIds = new List<int>();
                foreach (UserMessageListItemModel item in SelectedUserMessageListItems)
                {
                    selectedUserIds.Add(item.UserId);
                }

                bool isOk = await _ds.DeleteUserMessageListItem(selectedUserIds);
                if (isOk)
                {
                    _ds.ClearUserMessageListData();
                    DataView = _ds.GetViewForUserMessageList(1, AfterLoaded, null);
                }
            };
        }

        void GetData()
        {
            DataView = _ds.GetViewForUserMessageList(1, AfterLoaded, null);
        }
    }
}
