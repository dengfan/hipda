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

namespace HipdaUwpLite.Data
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
        int _stickCount = 0; // 置顶贴的数量，用于在判断是否翻页时，要加上置顶贴的数量，否则可能不会自动加载下一页的数据

        int triggerLoadedCount = 0;

        public GeneratorIncrementalLoadingClass(int pageSize, Func<int, Task<int>> loadMore, Func<int, T> generator)
        {
            _pageSize = pageSize;
            _loadMore = loadMore;
            _generator = generator;
        }

        /// <summary>
        /// 数据懒加载
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="count">每次触发懒加载时，将要新加入的数量</param>
        /// <returns></returns>
        protected async override Task<IList<object>> LoadMoreItemsOverrideAsync(System.Threading.CancellationToken c, uint count)
        {
            uint toGenerate = 0; // 本次要显示的数量

            if ((_generatedCount > 1 && count == 1) || // 当只请求加载一条数据，并且 _generatedCount > 1 时 才允许刷新
                (triggerLoadedCount == 1 && _generatedCount == 1 && _loadedDataMaxCount == 1 && count == 1)) // 也需要允许 “只有一楼而没有任何回复的贴子” 可以刷新
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
                if (toGenerate > 30)
                {
                    toGenerate = 15;
                }
            }
            else // 正常的滑动翻页流程，非点击刷新按钮走的流程
            {
                if (count == 1)
                {
                    triggerLoadedCount++;
                }

                // 判断是否需要加载下一页
                uint total = _generatedCount + count + (uint)_stickCount;
                if (total < _pageSize)
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

                        // 获取置顶贴的数量
                        if (pageNo == 1 && currentDataMaxCount < _pageSize)
                        {
                            _stickCount = _pageSize - currentDataMaxCount;
                        }
                    }
                }
                else if (total >= _pageSize)
                {
                    if (total % _pageSize == 0)
                    {
                        int pageNo = (int)Math.Ceiling(Convert.ToDecimal(total) / Convert.ToDecimal(_pageSize));
                        if (pageNo == _prevPageNo) // 表示正常的上划分页加载
                        {
                            pageNo++;

                            int currentDataMaxCount = await _loadMore(pageNo);
                            if (currentDataMaxCount > _loadedDataMaxCount) // 有新数据加入
                            {
                                _prevPageNo = pageNo;
                                _loadedDataMaxCount = currentDataMaxCount;
                            }

                            // 获取置顶贴的数量
                            if (pageNo == 1 && currentDataMaxCount < _pageSize)
                            {
                                _stickCount = _pageSize - currentDataMaxCount;
                            }
                        }
                    }
                    else
                    {
                        int pageNo = (int)Math.Ceiling(Convert.ToDecimal(total) / Convert.ToDecimal(_pageSize));
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
