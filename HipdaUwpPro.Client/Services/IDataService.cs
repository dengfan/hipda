using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Services
{
    public interface IDataService
    {
        /// <summary>
        /// 获取指定版区的贴子列表数据
        /// </summary>
        /// <param name="argForumId">版区ID</param>
        /// <returns>贴子列表</returns>
        List<ThreadPageModel> GetThreadPageListByForumId(int argForumId);
    }
}
