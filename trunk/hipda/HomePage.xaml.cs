using hipda.Common;
using hipda.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace hipda
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private const int maxHubSectionCount = 4;
        private const string FirstGroupName = "FirstGroup";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public HomePage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            ShowStatusBar();
        }

        private async void ShowStatusBar()
        {
            StatusBar statusBar = StatusBar.GetForCurrentView();
            statusBar.BackgroundColor = Colors.Purple;
            statusBar.BackgroundOpacity = 100;
            statusBar.ForegroundColor = Colors.White;
            await statusBar.ShowAsync();

            statusBar.ProgressIndicator.Text = string.Concat("Hi!PDA");
            await statusBar.ProgressIndicator.ShowAsync();
        }

        /// <summary>
        /// 获取与此 <see cref="Page"/> 关联的 <see cref="NavigationHelper"/>。
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// 获取此 <see cref="Page"/> 的视图模型。
        /// 可将其更改为强类型视图模型。
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
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
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: 创建适用于问题域的合适数据模型以替换示例数据
            var sampleDataGroup = await DataSource.GetForumGroupsAsync();
            cvsForumGroups.Source = sampleDataGroup;
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
            // TODO: 在此处保存页面的唯一状态。
        }

        private void ForumItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            //// 开启忙指示器
            //StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;

            //Forum forum = (Forum)e.ClickedItem;
            //string forumId = forum.Id;
            //string forumName = forum.Name;

            //var data = await DataSource.GetForumsAsync(forum, 1);
            //var pivotItem = new PivotItem
            //{
            //    Header = forumName.Length > 10 ? forumName.Substring(0, 10) + "..." : forumName,
            //    ContentTemplate = ThreadListTemplate,
            //    DataContext = data,
            //    Margin = new Thickness(0, 0, 0, 0)
            //};

            //// 限制 hubsection 的数量
            //if (Pivot.Items.Count > maxHubSectionCount)
            //{
            //    Pivot.Items.RemoveAt(maxHubSectionCount);
            //}

            //Pivot.Items.Insert(Pivot.SelectedIndex + 1, pivotItem);
            //Pivot.SelectedItem = pivotItem;

            //// 关闭忙指示器
            //StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;

            // 导航至相应的目标页，并
            // 通过将所需信息作为导航参数传入来配置新页
            Forum forum = (Forum)e.ClickedItem;
            if (!Frame.Navigate(typeof(PivotPage), forum))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
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
