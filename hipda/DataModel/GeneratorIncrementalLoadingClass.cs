//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hipda.Data
{
    // This class implements IncrementalLoadingBase. 
    // To create your own Infinite List, you can create a class like this one that doesn't have 'generator' or 'maxcount', 
    //  and instead downloads items from a live data source in LoadMoreItemsOverrideAsync.
    public class GeneratorIncrementalLoadingClass<T>: IncrementalLoadingBase
    {
        public GeneratorIncrementalLoadingClass(Func<int, Task<int>> loadMore, Func<int, T> generator)
        {
            _loadMore = loadMore;
            _generator = generator;
        }  

        /// <summary>
        /// 数据懒加载
        /// </summary>
        /// <param name="c"></param>
        /// <param name="count">每次触发懒加载时，将要新加入的数量</param>
        /// <returns></returns>
        protected async override Task<IList<object>> LoadMoreItemsOverrideAsync(System.Threading.CancellationToken c, uint count)
        {
            // 按条件加载分页数据
            if (_generatedCount == 0)
            {
                int pageNo = 1;

                // Wait for load
                _currentDataMaxCount = await _loadMore(pageNo);
                prevPageNo = pageNo;
            }
            else
            {
                uint total = _generatedCount + count;
                if (total >= pageSize)
                {
                    int pageNo = (int)Math.Ceiling(Convert.ToDecimal(_generatedCount + count) / Convert.ToDecimal(pageSize));
                    if (pageNo - prevPageNo == 1)
                    {
                        // Wait for load 
                        _currentDataMaxCount = await _loadMore(pageNo);
                        prevPageNo = pageNo;
                    }
                    else if (pageNo - prevPageNo > 1)
                    {
                        for (int i = prevPageNo; i < pageNo - prevPageNo; i++)
                        {
                            // Wait for load 
                            _currentDataMaxCount = await _loadMore(i);
                        }
                        prevPageNo = pageNo;
                    }
                }
            }

            // 本次要显示的数量
            uint toGenerate = System.Math.Min(count, (uint)_currentDataMaxCount - _generatedCount);

            // This code simply generates
            var values = from j in Enumerable.Range((int)_generatedCount, (int)toGenerate)
                         select (object)_generator(j);

            _generatedCount += toGenerate;

            return values.ToArray();
        }

        protected override bool HasMoreItemsOverride()
        {
            if (_currentDataMaxCount == 0)
            {
                return true;
            }

            return _generatedCount < _currentDataMaxCount;
        }

        #region State

        Func<int, Task<int>> _loadMore;
        Func<int, T> _generator;
        uint _generatedCount = 0; // 已加载并显示的数量
        int _currentDataMaxCount = 0; // 当前数据总量

        int prevPageNo = 1;
        int pageSize = 50;

        #endregion 
    }
}
