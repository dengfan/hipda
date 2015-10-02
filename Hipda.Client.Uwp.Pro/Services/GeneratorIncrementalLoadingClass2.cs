using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Services
{
    public class GeneratorIncrementalLoadingClass2<T> : IncrementalLoadingBase
    {
        Func<int, Task<int>> _getMore;
        Func<int, T> _getOne;
        int _firstPageNo;
        int _loadedCount = 0; // 已载入的项的数量
        uint _showedCount = 0; // 已显示的项的数量

        public GeneratorIncrementalLoadingClass2(int firstPageNo, Func<int, Task<int>> getMore, Func<int, T> getOne)
        {
            _firstPageNo = firstPageNo;
            _getMore = getMore;
            _getOne = getOne;
        }

        protected override bool HasMoreItemsOverride()
        {
            return true;
        }

        protected async override Task<IList<object>> LoadMoreItemsOverrideAsync(CancellationToken c, uint count)
        {
            if (_loadedCount == 0) // 第一次请求
            {
                _loadedCount = await _getMore(_firstPageNo);
            }
            else // 正常的滑动翻页开始啦
            {
                // 首先要判断是否需要载入下一页
                if (_showedCount + count >= _loadedCount) // 需要载入下一页
                {
                    _firstPageNo += 1;
                    _loadedCount = await _getMore(_firstPageNo);
                }
            }

            // This code simply generates
            var values = from j in Enumerable.Range((int)_showedCount, (int)count)
                         select (object)_getOne(j);

            _showedCount += count;

            return values.ToArray();
        }
    }
}
