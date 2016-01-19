using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
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
        MyPosts,

        /// <summary>
        /// 我的收藏
        /// </summary>
        MyFavorites,

        /// <summary>
        /// 标题搜索
        /// </summary>
        SearchTitle,

        /// <summary>
        /// 全文搜索
        /// </summary>
        SearchFullText,

        /// <summary>
        /// 通知
        /// </summary>
        Notice
    }
}
