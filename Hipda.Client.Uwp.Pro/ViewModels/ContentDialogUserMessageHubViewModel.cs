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
    public class ContentDialogUserMessageHubViewModel : NotificationObject
    {
        DataService _ds;

        public DelegateCommand IntoMultiSelectionModeCommand { get; set; }
        public DelegateCommand RefreshCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }

        private string _tipText = "请稍候，载入中。。。";

        public string TipText
        {
            get { return _tipText; }
            set { _tipText = value;
                this.RaisePropertyChanged("TipText");
            }
        }

        ListViewSelectionMode _selectionMode = ListViewSelectionMode.Single;

        public ListViewSelectionMode SelectionMode
        {
            get { return _selectionMode; }
            set
            {
                _selectionMode = value;
                this.RaisePropertyChanged("SelectionMode");
            }
        }

        private bool _isButtonEnable = false;

        public bool IsButtonEnable
        {
            get { return _isButtonEnable; }
            set
            {
                _isButtonEnable = value;
                this.RaisePropertyChanged("IsButtonEnable");
            }
        }

        private bool _isDeleteButtonEnable = false;

        public bool IsDeleteButtonEnable
        {
            get { return _isDeleteButtonEnable; }
            set
            {
                _isDeleteButtonEnable = value;
                this.RaisePropertyChanged("IsDeleteButtonEnable");
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
            TipText = string.Empty;
            IsButtonEnable = true;
        }

        public ContentDialogUserMessageHubViewModel()
        {
            _ds = new DataService();

            GetData();

            RefreshCommand = new DelegateCommand();
            RefreshCommand.ExecuteAction = (p) => {
                _ds.ClearUserMessageListData();
                DataView = _ds.GetViewForUserMessageList(1, AfterLoaded);
            };

            IntoMultiSelectionModeCommand = new DelegateCommand();
            IntoMultiSelectionModeCommand.ExecuteAction = (p) => {
                SelectionMode = _selectionMode == ListViewSelectionMode.Multiple ? ListViewSelectionMode.Single : ListViewSelectionMode.Multiple;
                IsDeleteButtonEnable = SelectionMode == ListViewSelectionMode.Multiple;
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
                    DataView = _ds.GetViewForUserMessageList(1, AfterLoaded);
                }
            };
        }

        void GetData()
        {
            DataView = _ds.GetViewForUserMessageList(1, AfterLoaded);
        }
    }
}
