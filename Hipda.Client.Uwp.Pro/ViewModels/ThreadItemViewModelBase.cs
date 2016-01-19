using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadItemViewModelBase : NotificationObject
    {
        public ThreadDataType ThreadDataType { get; set; }

        /// <summary>
        /// 加载的起始页
        /// 如“我的回复”之回复列表页并不一定是从第一页开始加载的，所以需要用此属性来判断是否显示“加载上一页”的按钮。
        /// </summary>
        public int StartPageNo { get; set; } = 1;

        private Style _statusColorStyle;

        public Style StatusColorStyle
        {
            get { return _statusColorStyle; }
            set
            {
                _statusColorStyle = value;
                this.RaisePropertyChanged("StatusColorStyle");
            }
        }
    }
}
