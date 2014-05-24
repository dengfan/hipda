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
        Func<int, Task<int>> _loadMore;
        Func<int, T> _generator;
        int _pageSize;
        uint _generatedCount = 0; // 已加载并显示的数量
        int _loadedDataMaxCount = 0; // 记录已经载入数据总量，用于与刚载入的数量总量进行对比，如果数据量没有变大，则上一次的页码保持不变
        int _prevPageNo = 0; // 记录上次加载的页码，以免重复加载

        public GeneratorIncrementalLoadingClass(int pageSize, Func<int, Task<int>> loadMore, Func<int, T> generator)
        {
            _pageSize = pageSize;
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
            uint toGenerate = 0; // 本次要显示的数量

            // 表示通过刷新按钮刷新最后一页数据
            // 由于增量加载一启动就会默认先显示一条数据，所以这里必须是 _generatedCount > 1 && count == 1 同时满足才表示是刷新
            if (_generatedCount > 1 && count == 1)
            {
                uint total = _generatedCount + count;
                if (total <= _pageSize)
                {
                    int pageNo = 1;
                    _loadedDataMaxCount = await _loadMore(pageNo);
                }
                else if (total > _pageSize)
                {
                    int pageNo = (int)Math.Ceiling(Convert.ToDecimal(_generatedCount + count) / Convert.ToDecimal(_pageSize));
                    _loadedDataMaxCount = await _loadMore(pageNo);
                }

                // 触发刷新后，有多少新数据全显示出来
                toGenerate = (uint)_loadedDataMaxCount - _generatedCount;
            }
            else
            {
                uint total = _generatedCount + count;
                if (total <= _pageSize)
                {
                    int pageNo = 1;
                    if (pageNo - _prevPageNo == 1) // 避免重复加载
                    {
                        int currentDataMaxCount = await _loadMore(pageNo);
                        if (currentDataMaxCount > _loadedDataMaxCount) // 有新数据加入
                        {
                            _prevPageNo = pageNo;
                            _loadedDataMaxCount = currentDataMaxCount;
                        }
                        
                    }
                }
                else if (total > _pageSize)
                {
                    int pageNo = (int)Math.Ceiling(Convert.ToDecimal(_generatedCount + count) / Convert.ToDecimal(_pageSize));
                    if (pageNo - _prevPageNo == 1) // 表示正常的上划分页加载
                    {
                        // Wait for load 
                        int currentDataMaxCount = await _loadMore(pageNo);
                        if (currentDataMaxCount > _loadedDataMaxCount) // 有新数据加入
                        {
                            _prevPageNo = pageNo;
                            _loadedDataMaxCount = currentDataMaxCount;
                        }
                        
                    }
                    else if (pageNo - _prevPageNo > 1)
                    {
                        for (int i = _prevPageNo; i < pageNo - _prevPageNo; i++)
                        {
                            // Wait for load 
                            int currentDataMaxCount = await _loadMore(i);
                            if (currentDataMaxCount > _loadedDataMaxCount)
                            {
                                _prevPageNo = i;
                                _loadedDataMaxCount = currentDataMaxCount;
                            }
                        }
                    }
                }

                // 触发加载下一页后，只加载要加载的数量
                toGenerate = System.Math.Min(count, (uint)_loadedDataMaxCount - _generatedCount);
            }

            // This code simply generates
            var values = from j in Enumerable.Range((int)_generatedCount, (int)toGenerate)
                         select (object)_generator(j);

            _generatedCount += toGenerate;

            return values.ToArray();
        }

        protected override bool HasMoreItemsOverride()
        {
            if (_loadedDataMaxCount == 0)
            {
                return true;
            }

            return _generatedCount < _loadedDataMaxCount;
        }
    }
}
