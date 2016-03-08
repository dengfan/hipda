using Hipda.Client.Uwp.Pro.Services;
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

        /// <summary>
        /// 有的主题是使用 ThreadId 参数加载回复列表页
        /// </summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// 有的主题是使用 PostId 参数加载回复列表页
        /// </summary>
        public int PostId { get; set; }

        private string _title;
        public string Title
        {
            get
            {
                return CommonService.ReplaceEmojiLabel(_title);
            }
            set
            {
                _title = value;
            }
        }

        public int ForumId { get; set; }

        public string ForumName { get; set; }

        public int AuthorUserId { get; set; }

        public string AuthorUsername { get; set; }
    }
}
