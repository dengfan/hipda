﻿using hipda.Common;
using hipda.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace hipda
{
    public sealed partial class PivotPage : Page
    {
        HttpHandle httpClient = HttpHandle.getInstance();

        private const int maxHubSectionCount = 6;
        private const string regexForTitle = @"[^@.a-zA-Z0-9\u4e00-\u9fa5]"; // 用于过滤掉无意义符号

        private readonly NavigationHelper navigationHelper;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private string accountName = "未登录";

        public static readonly DependencyProperty PivotItemTabTypeProperty = DependencyProperty.Register("TabType", typeof(String), typeof(PivotItem), null);
        public static readonly DependencyProperty PivotItemTabIdProperty = DependencyProperty.Register("TabId", typeof(String), typeof(PivotItem), null);
        
        public PivotPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            #region 读取当前账号的名称
            Account account = AccountHelper.GetDefault();
            accountName = account != null ? account.Username : "未登录";
            if (accountName.Length > 3)
            {
                accountName = string.Format("{0}*{1}", accountName.Substring(0, 2), accountName.Last());
            }
            #endregion

            if (e.PageState != null && e.PageState.Count > 0)
            {
                string tabStr = e.PageState["PivotItems"].ToString();
                tabStr = tabStr.Substring(0, tabStr.Length - 1);
                if (!string.IsNullOrEmpty(tabStr))
                {
                    string[] tabAry = tabStr.Split(';');
                    foreach (var item in tabAry.Reverse())
                    {
                        string[] tabAttrAry = item.Split(',');
                        string tabId = tabAttrAry[0];
                        string tabTitle = tabAttrAry[1];
                        string tabType = tabAttrAry[2];

                        if (tabType.Equals("1"))
                        {
                            CreateForumTab(tabId, tabTitle, true);
                        }
                        else
                        {
                            CreateThreadTab(tabId, tabTitle, true);
                        }
                    }
                }

                int selectedIndex = Convert.ToInt16(e.PageState["PivotSelectedIndex"]);
                Pivot.SelectedIndex = selectedIndex;
            }
            else
            {
                string dataStr = (string)e.NavigationParameter;
                string[] dataAry = dataStr.Split(',');
                string forumId = dataAry[0];
                string forumName = dataAry[1];

                CreateForumTab(forumId, forumName, false);

                await RefreshStatusBar();
            }
        }

        private void CreateForumTab(string forumId, string forumName, bool isResume)
        {
            var pivotItem = new PivotItem
            {
                Header = forumName,
                Margin = new Thickness(0, 0, 0, 0)
            };
            pivotItem.SetValue(PivotItemTabTypeProperty, "1");
            pivotItem.SetValue(PivotItemTabIdProperty, forumId);

            if (isResume)
            {
                Pivot.Items.Insert(0, pivotItem);
            }
            else
            {
                Pivot.Items.Insert(0, pivotItem);
            }
            Pivot.SelectedItem = pivotItem;

            // 在静态数据类中创建一个版块容器，用来装载主贴数据列表
            pivotItem.DataContext = DataSource.GetThread(forumId, forumName);

            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<Thread>(DataSource.ThreadPageSize, async pageNo =>
            {
                // 加载分页数据，并写入静态类中
                // 返回的是本次加载的数据量
                return await DataSource.GetLoadThreadsCountAsync(forumId, pageNo, () =>
                {
                    replyProgressBar.Visibility = Visibility.Visible;
                }, () =>
                {
                    replyProgressBar.Visibility = Visibility.Collapsed;
                });
            }, (index) =>
            {
                // 从静态类中返回需要显示出来的数据
                return DataSource.GetThreadByIndex(forumId, index);
            });

            var listView = new ListView
            {
                ItemsSource = cvs.View,
                IsItemClickEnabled = true,
                ItemTemplate = ThreadListItemTemplate,
                ItemContainerStyleSelector = new BackgroundStyleSelecterForThreadItem(),
                IncrementalLoadingTrigger = IncrementalLoadingTrigger.Edge
            };

            ContinuumNavigationTransitionInfo.SetExitElementContainer(listView, true);
            listView.ItemClick += ThreadItem_ItemClick;
            listView.ContainerContentChanging += ThemeListView_ContainerContentChanging;
            pivotItem.Content = listView;

            // 限制 pivot item 的数量
            if (Pivot.Items.Count > maxHubSectionCount)
            {
                PivotItem item = (PivotItem)Pivot.Items.Last();
                Pivot.Items.Remove(item);
            }

            // 由于在最后一个thread tab 打开一个 reply tab，会因超过 tab 数量而被删除，导致用户看不到
            // 所以如果发现最后一个 tab 是 thread tab，则删除之
            if (Pivot.Items.Count == 6)
            {
                PivotItem lastItem = (PivotItem)Pivot.Items.Last();
                string tabType = lastItem.GetValue(PivotItemTabTypeProperty).ToString();
                if (tabType.Equals("1"))
                {
                    Pivot.Items.Remove(lastItem);
                }
            }
        }

        #region 增量更新
        private void ThemeListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.Handled = true;

            if (args.Phase != 0)
            {
                throw new Exception("Not in phase 0.");
            }

            Thread thread = (Thread)args.Item;

            Grid layoutGrid = (Grid)args.ItemContainer.ContentTemplateRoot;

            Border avatarImageBorder = (Border)layoutGrid.FindName("avatarImageBorder");
            TextBlock ownerInfoTextBlock = (TextBlock)layoutGrid.FindName("ownerInfoTextBlock");
            Run titleTextBlockRun = (Run)layoutGrid.FindName("titleTextBlockRun");
            Run numbersTextBlockRun = (Run)layoutGrid.FindName("numbersTextBlockRun");
            TextBlock lastPostTextBlock = (TextBlock)layoutGrid.FindName("lastPostTextBlock");

            avatarImageBorder.Opacity = 0;
            ownerInfoTextBlock.Opacity = 0;
            titleTextBlockRun.Text = thread.Title;
            numbersTextBlockRun.Text = thread.Numbers;
            lastPostTextBlock.Opacity = 0;

            args.RegisterUpdateCallback(ShowThreadAuthor);
        }

        private void ShowThreadAuthor(
                ListViewBase sender,
                ContainerContentChangingEventArgs args)
        {
            if (args.Phase != 1)
            {
                throw new Exception("Not in phase 1.");
            }

            Thread thread = (Thread)args.Item;
            SelectorItem itemContainer = (SelectorItem)args.ItemContainer;
            Grid layoutGrid = (Grid)itemContainer.ContentTemplateRoot;

            TextBlock ownerInfoTextBlock = (TextBlock)layoutGrid.FindName("ownerInfoTextBlock");
            Run ownerNameTextBlockRun = (Run)layoutGrid.FindName("ownerNameTextBlockRun");
            Run createTimeTextBlockRun = (Run)layoutGrid.FindName("createTimeTextBlockRun");
            TextBlock lastPostTextBlock = (TextBlock)layoutGrid.FindName("lastPostTextBlock");

            ownerNameTextBlockRun.Text = thread.OwnerName;
            createTimeTextBlockRun.Text = thread.CreateTime;
            ownerInfoTextBlock.Opacity = 1;

            lastPostTextBlock.Text = thread.LastPostInfo;
            lastPostTextBlock.Opacity = 1;

            args.RegisterUpdateCallback(ShowThreadAuthorFace);
        }

        private void ShowThreadAuthorFace(
                ListViewBase sender,
                ContainerContentChangingEventArgs args)
        {
            if (args.Phase != 2)
            {
                throw new Exception("Not in phase 2.");
            }

            Thread thread = (Thread)args.Item;
            if (!string.IsNullOrEmpty(thread.AvatarUrl))
            {
                SelectorItem itemContainer = (SelectorItem)args.ItemContainer;
                Grid layoutGrid = (Grid)itemContainer.ContentTemplateRoot;

                Border avatarImageBorder = (Border)layoutGrid.FindName("avatarImageBorder");

                var imageBrush = new ImageBrush()
                {
                    ImageSource = new BitmapImage()
                    {
                        DecodePixelWidth = 60, // natural px width of image source
                        DecodePixelHeight = 60, // natural px width of image source
                        UriSource = new Uri(thread.AvatarUrl)
                    }
                };

                avatarImageBorder.Background = imageBrush;
                avatarImageBorder.Opacity = 1;
            }
        }
        #endregion

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // 获取当前 pivot item 用于从墓碑状态恢复
            StringBuilder tabStr = new StringBuilder();
            int i = 0;
            int selectedIndex = 0;
            foreach (PivotItem item in Pivot.Items)
            {
                string tabType = item.GetValue(PivotItemTabTypeProperty).ToString();
                string tabId = item.GetValue(PivotItemTabIdProperty).ToString();
                string tabTitle = item.Header.ToString();
                tabStr.Append(string.Format("{0},{1},{2};", tabId, tabTitle, tabType));
                if (Pivot.SelectedItem.Equals(item))
                {
                    selectedIndex = i;
                }
                i++;
            }

            e.PageState["PivotItems"] = tabStr.ToString();
            e.PageState["PivotSelectedIndex"] = selectedIndex;
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
        private async void ThreadItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            Thread thread = (Thread)e.ClickedItem;
            string threadId = thread.Id;
            string threadTitle = thread.Title;
            threadTitle = Regex.Replace(threadTitle, regexForTitle, string.Empty);
            threadTitle = threadTitle.Length > 7 ? threadTitle.Substring(0, 7) : threadTitle;
            if (string.IsNullOrEmpty(threadTitle)) threadTitle = "无标题";

            CreateThreadTab(threadId, threadTitle, false);

            await RefreshStatusBar();
        }

        private void CreateThreadTab(string threadId, string threadTitle, bool isResume)
        {
            var pivotItem = new PivotItem
            {
                Header = threadTitle,
                Margin = new Thickness(0, 0, 0, 0)
            };
            pivotItem.SetValue(PivotItemTabTypeProperty, "2");
            pivotItem.SetValue(PivotItemTabIdProperty, threadId);

            if (isResume)
            {
                Pivot.Items.Insert(0, pivotItem);
            }
            else
            {
                Pivot.Items.Insert(Pivot.SelectedIndex + 1, pivotItem);
            }
            
            Pivot.SelectedItem = pivotItem;

            Button refreshButton = new Button
            {
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 0, 100),
                Background = new SolidColorBrush(Colors.Green),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 60
            };

            // 在静态数据类中创建一个主贴容器，用来装载回复数据列表
            pivotItem.DataContext = DataSource.GetReply(threadId, threadTitle);

            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<Reply>(50, async pageNo =>
            {
                // 加载分页数据，并写入静态类中
                // 返回的是本次加载的数据量
                return await DataSource.GetLoadRepliesCountAsync(threadId, pageNo, () =>
                {
                    refreshButton.IsEnabled = false;
                    replyProgressBar.Visibility = Visibility.Visible;
                }, () =>
                {
                    refreshButton.IsEnabled = true;
                    replyProgressBar.Visibility = Visibility.Collapsed;
                });
            }, (index) =>
            {
                // 从静态类中返回需要显示出来的数据
                return DataSource.GetReplyByIndex(threadId, index);
            });

            var listView = new ListView
            {
                Padding = new Thickness(10, 0, 10, 0),
                ItemsSource = cvs.View,
                IsItemClickEnabled = false,
                ItemTemplate = ReplyListItemTemplate,
                ItemContainerStyleSelector = new BackgroundStyleSelecterForReplyItem(),
                IncrementalLoadingTrigger = IncrementalLoadingTrigger.Edge
            };

            #region 底部刷新按钮
            Image refreshIcon = new Image
            {
                Source = new BitmapImage
                {
                    UriSource = new Uri("ms-appx:///Assets/Icons/appbar.refresh.png")
                }
            };

            refreshButton.Content = refreshIcon;

            refreshButton.Tapped += async (s2, e2) =>
            {
                Button btn = (Button)s2;
                ICollectionView view = (ICollectionView)listView.ItemsSource;
                await view.LoadMoreItemsAsync(1); // count = 1 表示是要刷新
            };

            listView.Footer = refreshButton;
            #endregion

            listView.ContainerContentChanging += ReplyListView_ContainerContentChanging;
            pivotItem.Content = listView;

            // 限制 pivot item 的数量
            if (Pivot.Items.Count > maxHubSectionCount)
            {
                PivotItem item = (PivotItem)Pivot.Items.Last();
                Pivot.Items.Remove(item);
            }

            // 由于在最后一个thread tab 打开一个 reply tab，会因超过 tab 数量而被删除，导致用户看不到
            // 所以如果发现最后一个 tab 是 thread tab，则删除之
            if (Pivot.Items.Count == 6)
            {
                PivotItem lastItem = (PivotItem)Pivot.Items.Last();
                string tabType = lastItem.GetValue(PivotItemTabTypeProperty).ToString();
                if (tabType.Equals("1"))
                {
                    Pivot.Items.Remove(lastItem);
                }
            }
        }

        #region 增量更新
        void ReplyListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.Handled = true;

            if (args.Phase != 0)
            {
                throw new Exception("Not in phase 0.");
            }

            Reply reply = (Reply)args.Item;

            Grid layoutGrid = (Grid)args.ItemContainer.ContentTemplateRoot;

            Border avatarImageBorder = (Border)layoutGrid.FindName("avatarImageBorder");
            TextBlock ownerNameTextBlock = (TextBlock)layoutGrid.FindName("ownerNameTextBlock");
            TextBlock createTimeTextBlock = (TextBlock)layoutGrid.FindName("createTimeTextBlock");
            TextBlock floorNumTextBlock = (TextBlock)layoutGrid.FindName("floorNumTextBlock");
            TextBlock replyContentTextBlock = (TextBlock)layoutGrid.FindName("replyContentTextBlock");

            avatarImageBorder.Opacity = 0;
            ownerNameTextBlock.Opacity = 0;
            createTimeTextBlock.Opacity = 0;
            floorNumTextBlock.Opacity = 0;
            replyContentTextBlock.Text = reply.Content;

            args.RegisterUpdateCallback(ShowAuthor);
        }

        private void ShowAuthor(
                ListViewBase sender,
                ContainerContentChangingEventArgs args)
        {
            if (args.Phase != 1)
            {
                throw new Exception("Not in phase 1.");
            }

            Reply reply = (Reply)args.Item;
            SelectorItem itemContainer = (SelectorItem)args.ItemContainer;
            Grid layoutGrid = (Grid)itemContainer.ContentTemplateRoot;

            TextBlock ownerNameTextBlock = (TextBlock)layoutGrid.FindName("ownerNameTextBlock");
            TextBlock createTimeTextBlock = (TextBlock)layoutGrid.FindName("createTimeTextBlock");
            TextBlock floorNumTextBlock = (TextBlock)layoutGrid.FindName("floorNumTextBlock");

            ownerNameTextBlock.Text = reply.OwnerName;
            ownerNameTextBlock.Opacity = 1;

            createTimeTextBlock.Text = reply.CreateTime;
            createTimeTextBlock.Opacity = 1;

            floorNumTextBlock.Text = reply.FloorNumStr;
            floorNumTextBlock.Opacity = 1;

            args.RegisterUpdateCallback(ShowAuthorFace);
        }

        private void ShowAuthorFace(
                ListViewBase sender,
                ContainerContentChangingEventArgs args)
        {
            if (args.Phase != 2)
            {
                throw new Exception("Not in phase 2.");
            }

            Reply reply = (Reply)args.Item;
            SelectorItem itemContainer = (SelectorItem)args.ItemContainer;
            Grid layoutGrid = (Grid)itemContainer.ContentTemplateRoot;

            Border avatarImageBorder = (Border)layoutGrid.FindName("avatarImageBorder");

            var imageBrush = new ImageBrush()
            {
                ImageSource = new BitmapImage()
                {
                    DecodePixelWidth = 60, // natural px width of image source
                    DecodePixelHeight = 60, // natural px width of image source
                    UriSource = new Uri(reply.AvatarUrl)
                }
            };

            avatarImageBorder.Background = imageBrush;
            avatarImageBorder.Opacity = 1;
        }
        #endregion

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await RefreshStatusBar();
        }

        private async Task RefreshStatusBar()
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
                    navTextContainer.Append(string.Format("{0}●-", text));
                }
                else
                {
                    navTextContainer.Append(string.Format("{0}-", text));
                }
            }

            string navText = navTextContainer.ToString();
            navText = navText.Substring(0, navText.Length - 1);

            StatusBar.GetForCurrentView().ProgressIndicator.Text = string.Format("{0} > {1}", accountName, navText);
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
