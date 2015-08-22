using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadAndReplyViewModels : NotificationObject
    {

        private List<ThreadPageModel> threadPageList;

        public List<ThreadPageModel> ThreadPageList
        {
            get { return threadPageList; }
            set
            {
                threadPageList = value;
                this.RaisePropertyChanged("ThreadPageList");
            }
        }

        private void LoadThreadPageList()
        {

        }


    }
}
