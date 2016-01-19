using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    /// <summary>
    /// 抽象出来的原因是方便记录浏览历史
    /// </summary>
    public class ThreadItemModelBase
    {
        public ThreadDataType ThreadType { get; set; }

        public int ThreadId { get; set; }

        public string Title { get; set; }
    }
}
