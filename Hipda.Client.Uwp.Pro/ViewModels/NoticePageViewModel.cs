using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class NoticePageViewModel : NotificationObject
    {
        DataService _ds;

        private List<NoticeItemModel> _noticeData;

        public List<NoticeItemModel> NoticeData
        {
            get { return _noticeData; }
            set
            {
                _noticeData = value;
                this.RaisePropertyChanged("NoticeData");
            }
        }

        public NoticePageViewModel()
        {
            _ds = new DataService();

            GetData();
        }

        async void GetData()
        {
            NoticeData = await _ds.GetNoticeData();
        }
    }
}
