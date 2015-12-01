using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    /// <summary>
    /// 主题类别之枚举
    /// </summary>
    public enum ThreadDataType
    {
        /// <summary>
        /// 默认类别
        /// </summary>
        Default,

        /// <summary>
        /// 我的贴子类别
        /// </summary>
        MyThreads,

        /// <summary>
        /// 我的回复类别
        /// </summary>
        MyPosts
    }

    public class ThreadItemViewModelBase : NotificationObject
    {
        public ThreadDataType ThreadDataType { get; set; }

        /// <summary>
        /// 加载的起始页
        /// 如“我的回复”之回复列表页并不一定是从第一页开始加载的，所以需要用此属性来判断是否显示“加载上一页”的按钮。
        /// </summary>
        public int StartPageNo { get; set; }

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
