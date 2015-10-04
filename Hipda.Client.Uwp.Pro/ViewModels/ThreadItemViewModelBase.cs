using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public enum ThreadDataType
    {
        Normal,

        MyThreads,

        MyPosts
    }

    public class ThreadItemViewModelBase : NotificationObject
    {
        public ThreadDataType ThreadDataType { get; set; }
    }
}
