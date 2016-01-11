using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Services
{
    public class GeneratorIncrementalLoadingClass<T> : IncrementalLoadingBase
    {
        Func<int, Task<int>> _getMore;
        Func<int, T> _getOne;
        Func<int> _getMaxPageNo;
        int _pageNo;
        int _loadedCount = 0; // 已载入的项的数量
        uint _showedCount = 0; // 已显示的项的数量
        bool _isFirstLoad = true;

        public GeneratorIncrementalLoadingClass(int pageNo, Func<int, Task<int>> getMore, Func<int, T> getOne, Func<int> getMaxPageNo)
        {
            _pageNo = pageNo;
            _getMore = getMore;
            _getOne = getOne;
            _getMaxPageNo = getMaxPageNo;
        }

        protected override bool HasMoreItemsOverride()
        {
            if (_isFirstLoad)
            {
                return true;
            }

            int maxPage = _getMaxPageNo();
            if (_pageNo == maxPage)
            {
                return _showedCount < _loadedCount;
            }

            return _pageNo < maxPage;
        }

        protected async override Task<IList<object>> LoadMoreItemsOverrideAsync(CancellationToken c, uint count)
        {
            if (_loadedCount == 0) // 第一次请求
            {
                _loadedCount = await _getMore(_pageNo);
            }
            else // 正常的滑动翻页开始啦
            {
                // 首先要判断是否需要载入下一页
                if (_showedCount + count >= _loadedCount) // 需要载入下一页
                {
                    _pageNo += 1;
                    _loadedCount = await _getMore(_pageNo);
                }
            }

            uint readyShowCount = count;
            if (_showedCount + readyShowCount > _loadedCount)
            {
                readyShowCount = (uint)_loadedCount - _showedCount;
            }

            // This code simply generates
            var values = from j in Enumerable.Range((int)_showedCount, (int)readyShowCount)
                         select (object)_getOne(j);

            _showedCount += readyShowCount;

            _isFirstLoad = false;
            return values.ToArray();
        }
    }
}
