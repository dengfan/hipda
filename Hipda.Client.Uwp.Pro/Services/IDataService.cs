using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Services
{
    public interface IDataService
    {
        /// <summary>
        /// 获取指定版区的贴子列表数据
        /// </summary>
        /// <param name="argForumId">版区ID</param>
        /// <param name="pageNo">页码</param>
        /// <param name="cts">CancellationTokenSource</param>
        /// <returns>贴子列表</returns>
        Task<IEnumerable<ThreadItemModel>> GetThreadPageListByForumId(int forumId, int pageNo, CancellationTokenSource cts);
    }
}
