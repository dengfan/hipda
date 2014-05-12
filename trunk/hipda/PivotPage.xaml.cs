using hipda.Common;
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
        private const string regexForTitle = @"[^@.a-zA-Z0-9\u4e00-\u9fa5]";


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
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
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
            Forum forumParam = (Forum)e.NavigationParameter;

            string forumId = forumParam.Id;
            string forumTitle = forumParam.Name;
            switch (forumId)
            {
                case "6":
                    forumTitle = "BS版";
                    break;
                case "7":
                    forumTitle = "G版";
                    break;
                case "14":
                    forumTitle = "Win版";
                    break;
                case "2":
                    forumTitle = "地板";
                    break;
                case "59":
                    forumTitle = "E版";
                    break;
                default:
                    forumTitle = forumTitle.Substring(0, 1) + "版";
                    break;
            }

            var pivotItem = new PivotItem
            {
                Header = forumTitle,
                ContentTemplate = ThreadListTemplate,
                Margin = new Thickness(0, 0, 0, 0)
            };

            // 限制 hubsection 的数量
            if (Pivot.Items.Count > maxHubSectionCount)
            {
                Pivot.Items.RemoveAt(maxHubSectionCount);
            }

            int index = Pivot.Items.Count == 0 ? 0 : Pivot.Items.Count;
            Pivot.Items.Insert(0, pivotItem);
            Pivot.SelectedItem = pivotItem;

            pivotItem.DataContext = await DataSource.GetForumsAsync(forumParam, 1);
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

        /// <summary>
        /// 在单击应用程序栏按钮时将项添加到列表中。
        /// </summary>
        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //string groupName = FirstGroupName;
            //var group = this.DefaultViewModel[groupName] as SampleDataGroup;
            //var nextItemId = group.Items.Count + 1;
            //var newItem = new SampleDataItem(
            //    string.Format(CultureInfo.InvariantCulture, "Group-{0}-Item-{1}", this.pivot.SelectedIndex + 1, nextItemId),
            //    string.Format(CultureInfo.CurrentCulture, this.resourceLoader.GetString("NewItemTitle"), nextItemId),
            //    string.Empty,
            //    string.Empty,
            //    this.resourceLoader.GetString("NewItemDescription"),
            //    string.Empty);

            //group.Items.Add(newItem);

            //// 将新的项滚动到视图中。
            //var container = this.pivot.ContainerFromIndex(this.pivot.SelectedIndex) as ContentControl;
            //var listView = container.ContentTemplateRoot as ListView;
            //listView.ScrollIntoView(newItem, ScrollIntoViewAlignment.Leading);
        }

        /// <summary>
        /// 在贴子项上单击时调用
        /// </summary>
        private async void ThreadItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 开启忙指示器
            StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;

            Thread thread = (Thread)e.ClickedItem;
            string threadId = thread.Id;
            string threadTitle = thread.Title;
            threadTitle = Regex.Replace(threadTitle, regexForTitle, string.Empty);
            if (string.IsNullOrEmpty(threadTitle)) threadTitle = "标题";
            var pivotItem = new PivotItem
            {
                Header = GetFirstString(threadTitle, 16),
                ContentTemplate = ReplyListTemplate,
                Margin = new Thickness(0,0,0,0)
            };

            // 限制 hubsection 的数量
            if (Pivot.Items.Count > maxHubSectionCount)
            {
                Pivot.Items.RemoveAt(maxHubSectionCount);
            }

            Pivot.Items.Insert(Pivot.SelectedIndex + 1, pivotItem);
            Pivot.SelectedItem = pivotItem;

            pivotItem.DataContext = await DataSource.GetThreadAsync(thread.ForumId, thread, 1);
        }

        /// <summary>
        /// 在回复项上单击时调用
        /// </summary>
        private void ReplyItem_ItemClick(object sender, ItemClickEventArgs e)
        { 

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
