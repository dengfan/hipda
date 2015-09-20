using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class AccountItemViewModel : NotificationObject
    {
        public AccountItemModel AccountItem { get; set; }

        private bool _isDefault;

        public bool IsDefault
        {
            get { return _isDefault; }
            set
            {
                _isDefault = value;
                this.RaisePropertyChanged("IsDefault");
            }
        }

    }
}
