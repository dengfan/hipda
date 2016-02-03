using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    /// <summary>
    /// 发送信息之类别
    /// </summary>
    public enum SendType
    {
        /// <summary>
        /// 发贴
        /// </summary>
        New,

        /// <summary>
        /// 改贴
        /// </summary>
        Edit,

        /// <summary>
        /// 回贴
        /// </summary>
        Reply
    }
}
