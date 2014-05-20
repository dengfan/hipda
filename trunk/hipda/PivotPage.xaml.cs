﻿using hipda.Common;
using hipda.Data;
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text.RegularExpressions;

// “透视应用程序”模板在 http://go.microsoft.com/fwlink/?LinkID=391641 上有介绍

namespace hipda
{
    public sealed partial class PivotPage : Page
    {
        private const int maxHubSectionCount = 5;
        private const string regexForTitle = @"[^@.a-zA-Z0-9\u4e00-\u9fa5]"; // 用于过滤掉无意义符号

        public static string GetFirstString(string stringToSub, int length)
        {
            Regex regex = new Regex(@"[\u4e00-\u9fa5]+");
            char[] stringChar = stringToSub.ToCharArray();
            StringBuilder sb = new StringBuilder();
            int nLength = 0;

            for (int i = 0; i < stringChar.Length; i++)
            {
                if (regex.IsMatch((stringChar[i]).ToString()))
                {
                    nLength += 2;
                }

                else
                {
                    nLength = nLength + 1;
                }

                if (nLength <= length)
                {
                    sb.Append(stringChar[i]);
                }

                else
                {
                    break;
                }
            }

            if (sb.ToString() != stringToSub)
            {
                sb.Append("...");
            }

            return sb.ToString();
        }

        private readonly NavigationHelper navigationHelper;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public PivotPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// 获取与此 <see cref="Page"/> 关联的 <see cref="NavigationHelper"/>。
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// 使用在导航过程中传递的内容填充页。在从以前的会话
        /// 重新创建页时，也会提供任何已保存状态。
        /// </summary>
        /// <param name="sender">
        /// 事件的来源；通常为 <see cref="NavigationHelper"/>。
        /// </param>
        /// <param name="e">事件数据，其中既提供在最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, Object)"/> 的导航参数，又提供
        /// 此页在以前会话期间保留的状态的
        /// 的字典。首次访问页面时，该状态将为 null。</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            string dataStr = (string)e.NavigationParameter;
            string[] data = dataStr.Split(',');
            string forumId = data[0];
            string forumName = data[1];

            var pivotItem = new PivotItem
            {
                Header = forumName,
                Margin = new Thickness(0, 0, 0, 0)
            };

            // 限制 pivot item 的数量
            if (Pivot.Items.Count > maxHubSectionCount)
            {
                Pivot.Items.RemoveAt(maxHubSectionCount);
            }

            //int itemIndex = Pivot.Items.Count == 0 ? 0 : Pivot.Items.Count;
            Pivot.Items.Insert(0, pivotItem);
            Pivot.SelectedItem = pivotItem;

            // 在静态数据类中创建一个版块容器，用来装载主贴数据列表
            pivotItem.DataContext = DataSource.GetForum(forumId, forumName);

            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<Thread>(75, async pageNo =>
            {
                // 加载分页数据，并写入静态类中
                // 返回的是本次加载的数据量
                return await DataSource.GetLoadThreadsCountAsync(forumId, pageNo);
            }, (index) =>
            {
                // 从静态类中返回需要显示出来的数据
                return DataSource.GetThreadByIndex(forumId, index);
            });

            var listView = new ListView
            {
                ItemsSource = cvs.View,
                IsItemClickEnabled = true,
                //ContinuumNavigationTransitionInfo.ExitElementContainer="True",
                ItemTemplate = ThreadListItemTemplate,
                //GroupStyleSelector = new ListGroupStyleSelectorFroReply(),
                //ItemContainerStyleSelector = new BackgroundStyleSelecterForThreadItem(),
                IncrementalLoadingTrigger = IncrementalLoadingTrigger.Edge,
                DataFetchSize = 4, // 每次预提数据的5屏
                IncrementalLoadingThreshold = 2, // 每滚动三屏就触发预提数据
            };

            listView.ItemClick += ThreadItem_ItemClick;
            pivotItem.Content = listView;
        }

        /// <summary>
        /// 保留与此页关联的状态，以防挂起应用程序或
        /// 从导航缓存中放弃此页。值必须符合序列化
        /// <see cref="SuspensionManager.SessionState"/> 的序列化要求。
        /// </summary>
        /// <param name="sender">事件的来源；通常为 <see cref="NavigationHelper"/>。</param>
        ///<param name="e">提供要使用可序列化状态填充的空字典
        ///的事件数据。</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        /// <summary>
        /// 在单击应用程序栏按钮时将项添加到列表中。
        /// </summary>
        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// 在贴子项上单击时调用
        /// </summary>
        private void ThreadItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            Thread thread = (Thread)e.ClickedItem;
            string threadId = thread.Id;
            string threadTitle = thread.Title;
            threadTitle = Regex.Replace(threadTitle, regexForTitle, string.Empty);
            if (string.IsNullOrEmpty(threadTitle)) threadTitle = "无标题";

            var pivotItem = new PivotItem
            {
                Header = GetFirstString(threadTitle, 16),
                Margin = new Thickness(0,0,0,0)
            };

            // 限制 pivot item 的数量
            if (Pivot.Items.Count > maxHubSectionCount)
            {
                Pivot.Items.RemoveAt(maxHubSectionCount);
            }

            Pivot.Items.Insert(Pivot.SelectedIndex + 1, pivotItem);
            Pivot.SelectedItem = pivotItem;

            // 在静态数据类中创建一个主贴容器，用来装载回复数据列表
            pivotItem.DataContext = DataSource.GetThread(thread.ForumId, thread.Id);
            
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<Reply>(50, async pageNo =>
            {
                // 加载分页数据，并写入静态类中
                // 返回的是本次加载的数据量
                return await DataSource.GetLoadRepliesCountAsync(thread.ForumId, thread.Id, pageNo);
            }, (index) =>
            {
                // 从静态类中返回需要显示出来的数据
                return DataSource.GetReplyByIndex(thread.ForumId, thread.Id, index);
            });

            var listView = new ListView
            {
                ItemsSource = cvs.View,
                IsItemClickEnabled = false,
                //ContinuumNavigationTransitionInfo.ExitElementContainer="True",
                ItemTemplate = ReplyListItemTemplate,
                //GroupStyleSelector = new ListGroupStyleSelectorFroReply(),
                ItemContainerStyleSelector = new BackgroundStyleSelecterForReplyItem(),
                IncrementalLoadingTrigger = IncrementalLoadingTrigger.Edge,
                DataFetchSize = 4, // 每次预提数据的5屏
                IncrementalLoadingThreshold = 2, // 每滚动三屏就触发预提数据
            };
            pivotItem.Content = listView;
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StringBuilder navTextContainer = new StringBuilder();

            foreach (PivotItem item in Pivot.Items)
            {
                string text = item.Header.ToString();
                if (!"|BS版|G版|Win版|地板|E版|".Contains(text))
                { 
                    text = item.Header.ToString().Substring(0, 1);
                }

                if (Pivot.SelectedItem == item)
                {
                    navTextContainer.Append(string.Format("@{0} -", text));
                }
                else
                {
                    navTextContainer.Append(string.Format(" {0} -", text));
                }
            }

            string navText = navTextContainer.ToString();
            navText = navText.Substring(0, navText.Length - 1);

            StatusBar.GetForCurrentView().ProgressIndicator.Text = string.Concat("Hi!PDA ", navText);
            await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
        }

        #region NavigationHelper 注册

        /// <summary>
        /// 此部分中提供的方法只是用于使
        /// NavigationHelper 可响应页面的导航方法。
        /// <para>
        /// 应将页面特有的逻辑放入用于
        /// <see cref="NavigationHelper.LoadState"/>
        /// 和 <see cref="NavigationHelper.SaveState"/> 的事件处理程序中。
        /// 除了在会话期间保留的页面状态之外
        /// LoadState 方法中还提供导航参数。
        /// </para>
        /// </summary>
        /// <param name="e">提供导航方法数据和
        /// 无法取消导航请求的事件处理程序。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
