using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class UserMessageListDialogViewModel
    {
        DataService _ds;

        private ICollectionView _dataView;

        public ICollectionView DataView
        {
            get { return _dataView; }
            set
            {
                _dataView = value;
            }
        }

        public UserMessageListDialogViewModel()
        {
            _ds = new DataService();

            GetData();
        }

        void GetData()
        {
            DataView = _ds.GetViewForUserMessageList(1);
        }
    }
}
