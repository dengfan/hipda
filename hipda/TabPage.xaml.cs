using hipda.Common;
using hipda.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace hipda
{
    public sealed partial class TabPage : Page
    {
        HttpHandle httpClient = HttpHandle.getInstance();

        private const int maxHubSectionCount = 6;
        private const string regexForTitle = @"[^a-zA-Z\d\u4e00-\u9fa5]"; // 用于过滤掉无意义符号

        private readonly NavigationHelper navigationHelper;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private string accountName = "未登录";

        public static readonly DependencyProperty PivotItemTabTypeProperty = DependencyProperty.Register("TabType", typeof(String), typeof(PivotItem), null);
        public static readonly DependencyProperty PivotItemTabIdProperty = DependencyProperty.Register("TabId", typeof(String), typeof(PivotItem), null);

        private string noticeauthor = string.Empty;
        private string noticetrimstr = string.Empty;
        private string noticeauthormsg = string.Empty;
        
        public TabPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
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
                            CreateThreadListTab(tabId, tabTitle, true);
                        }
                        else
                        {
                            CreateReplyListTab(tabId, tabTitle, true);
                        }
                    }
                }

                int selectedIndex = Convert.ToInt16(e.PageState["PivotSelectedIndex"]);
                Pivot.SelectedIndex = selectedIndex;
            }
            else
            {
                if (e.NavigationParameter == null)
                {
                    CreateThreadListTab("14", "WIN版", false);
                    CreateReplyListTab("1427253", "特别鸣谢 + 关于", false);
                }
                else
                {
                    string dataStr = (string)e.NavigationParameter;
                    string[] dataAry = dataStr.Split(',');
                    string forumId = dataAry[0];
                    string forumName = dataAry[1];

                    CreateThreadListTab(forumId, forumName, false);
                }

                await RefreshElementStatus();
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

            CreateReplyListTab(threadId, threadTitle, false);

            await RefreshElementStatus();
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
            Button menuButton = (Button)layoutGrid.FindName("menuButton");
            TextBlock floorNumTextBlock = (TextBlock)layoutGrid.FindName("floorNumTextBlock");
            ContentControl replyContent = (ContentControl)layoutGrid.FindName("replyContent");

            avatarImageBorder.Opacity = 0;
            ownerNameTextBlock.Opacity = 0;
            createTimeTextBlock.Opacity = 0;
            menuButton.DataContext = reply;
            floorNumTextBlock.Opacity = 0;
            replyContent.Content = XamlReader.Load(reply.XamlContent);

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

        private async void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            PivotItem item = (PivotItem)Pivot.SelectedItem;
            ThreadItem data = (ThreadItem)item.DataContext;
            string tabType = item.GetValue(PivotItemTabTypeProperty).ToString();
            if (tabType.Equals("1"))
            {
                string tabId = data.ForumId;

                ListView listView = (ListView)item.FindName("threadsListView" + tabId);
                listView.ItemsSource = null;
                
                await DataSource.RefreshThread(tabId);

                var cvs = new CollectionViewSource();
                cvs.Source = new GeneratorIncrementalLoadingClass<Thread>(DataSource.ThreadPageSize, async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await DataSource.GetLoadThreadsCountAsync(tabId, pageNo, () =>
                    {
                        replyProgressBar.Visibility = Visibility.Visible;
                    }, () =>
                    {
                        replyProgressBar.Visibility = Visibility.Collapsed;
                    });
                }, (index) =>
                {
                    // 从静态类中返回需要显示出来的数据
                    return DataSource.GetThreadByIndex(tabId, index);
                });

                listView.ItemsSource = cvs.View;
            }
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await RefreshElementStatus();
        }

        private async Task RefreshElementStatus()
        {
            #region 刷新 底部按钮
            PivotItem item = (PivotItem)Pivot.SelectedItem;
            string tabType = item.GetValue(PivotItemTabTypeProperty).ToString();
            if (tabType.Equals("1")) // 主贴列表页
            {
                tabPageCommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
                refreshButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                replyButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                postButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else // 回复列表页
            {
                tabPageCommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
                refreshButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                replyButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                postButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            #endregion

            #region 刷新 顶部状态栏
            StringBuilder navTextContainer = new StringBuilder();

            foreach (PivotItem pivotItem in Pivot.Items)
            {
                string text = pivotItem.Header.ToString();
                if (!"|BS版|G版|Win版|地板|E版|".Contains(text))
                {
                    text = pivotItem.Header.ToString().Substring(0, 1);
                }

                if (Pivot.SelectedItem == pivotItem)
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
            #endregion
        }

        #region 创建 tab 页
        private void CreateThreadListTab(string forumId, string forumName, bool isResume)
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
                Name = "threadsListView" + forumId,
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

        private void CreateReplyListTab(string threadId, string threadTitle, bool isResume)
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
                Name = "repliesListView" + threadId,
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
        #endregion

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

        private void openTabForApp_Click(object sender, RoutedEventArgs e)
        {
            CreateReplyListTab("1427253", "特别鸣谢 + 关于", false);
        }

        private void openTabForDiscovery_Click(object sender, RoutedEventArgs e)
        {
            CreateThreadListTab("2", "地板", false);
        }

        private void openTabForBuyAndSell_Click(object sender, RoutedEventArgs e)
        {
            CreateThreadListTab("6", "BS 版", false);
        }

        private void openTabForEink_Click(object sender, RoutedEventArgs e)
        {
            CreateThreadListTab("59", "Eink 版", false);
        }

        private void replyButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPostButton();
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (popupGrid.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                NavigationHelper.IsCanGoBack = false;
                e.Handled = true;

                ShowReplyButton();
            }
            else
            {
                NavigationHelper.IsCanGoBack = true;
            }
        }

        private async void postButton_Click(object sender, RoutedEventArgs e)
        {
            //postPanelFadeIn.Begin();

            // 获取当前主贴ID
            PivotItem pivotItem = (PivotItem)Pivot.SelectedItem;
            string threadId = pivotItem.GetValue(PivotItemTabIdProperty).ToString();

            string message = postMessageTextBox.Text;

            var postData = new Dictionary<string, object>();
            postData.Add("noticeauthor", noticeauthor);
            postData.Add("noticetrimstr", noticetrimstr);
            postData.Add("noticeauthormsg", noticeauthormsg);

            postData.Add("formhash", DataSource.FormHash);
            postData.Add("subject", string.Empty);
            postData.Add("usesig", "1");

            // 客户端尾巴
            postData.Add("message", message + "\n\n[img=16,16]http://www.hi-pda.com/forum/attachments/day_140621/1406211752793e731a4fec8f7b.png[/img]");

            string resultContent = await httpClient.HttpPost("http://www.hi-pda.com/forum/post.php?action=reply&tid=" + threadId + "&replysubmit=yes&infloat=yes&handlekey=fastpost&inajax=1", postData);
            if (resultContent.Contains("您的回复已经发布"))
            {
                ShowReplyButton();

                // 刷新数据
                ListView listView = (ListView)pivotItem.FindName("repliesListView" + threadId);
                ICollectionView view = (ICollectionView)listView.ItemsSource;
                await view.LoadMoreItemsAsync(1); // count = 1 表示是要刷新

                postMessageTextBox.Text = string.Empty;
                noticeauthor = string.Empty;
                noticetrimstr = string.Empty;
                noticeauthormsg = string.Empty;
            }
            else
            {
                await new MessageDialog("您的发布请求已不成功！", "注意").ShowAsync();
            }
        }

        private void ShowReplyButton()
        {
            tabPageCommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
            popupGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            replyButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            postButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            openTabForDiscovery.Visibility = Windows.UI.Xaml.Visibility.Visible;
            openTabForBuyAndSell.Visibility = Windows.UI.Xaml.Visibility.Visible;
            openTabForEink.Visibility = Windows.UI.Xaml.Visibility.Visible;
            openTabForApp.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void ShowPostButton()
        {
            tabPageCommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
            popupGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            replyButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            postButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

            openTabForDiscovery.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            openTabForBuyAndSell.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            openTabForEink.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            openTabForApp.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void replyReplyMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Reply data = (sender as MenuFlyoutItem).DataContext as Reply;

            noticeauthor = string.Format("r|{0}|[i]{1}[/i]", data.OwnerId, data.OwnerName);
            noticetrimstr = string.Format("[b]回复 {0}# [i]{1}[/i] [/b]\n\n", data.Floor, data.OwnerName);
            noticeauthormsg = data.TextContent.Length > 100 ? data.TextContent.Substring(0, 95) + "..." : data.TextContent;

            postMessageTextBox.Text = noticetrimstr;

            popupGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            ShowPostButton();
        }

        private void refReplyMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Reply data = (sender as MenuFlyoutItem).DataContext as Reply;

            noticeauthor = string.Format("r|{0}|[i]{1}[/i]", data.OwnerId, data.OwnerName);

            noticetrimstr = data.TextContent.Length > 100 ? data.TextContent.Substring(0, 95) + "..." : data.TextContent;
            noticetrimstr = new Regex("\r\n").Replace(noticetrimstr, string.Empty);
            noticetrimstr = new Regex("\r").Replace(noticetrimstr, string.Empty);
            noticetrimstr = new Regex("\n").Replace(noticetrimstr, string.Empty);
            noticetrimstr = noticetrimstr.Replace("&nbsp;", string.Empty);
            noticetrimstr = string.Format("[quote]回复 {0}# {1}\n{2}[/quote]\n\n", data.Floor, data.OwnerName, noticetrimstr);

            noticeauthormsg = noticetrimstr;

            postMessageTextBox.Text = noticetrimstr;

            popupGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
            ShowPostButton();
        }

        //private async void addImageForPostButton_Click(object sender, RoutedEventArgs e)
        //{
        //    FileOpenPicker openPicker = new FileOpenPicker();
        //    openPicker.ViewMode = PickerViewMode.Thumbnail;
        //    openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        //    openPicker.FileTypeFilter.Add(".jpg");
        //    openPicker.FileTypeFilter.Add(".jpeg");
        //    openPicker.FileTypeFilter.Add(".png");

        //    StorageFile file = await openPicker.PickSingleFileAsync();
        //    if (file != null)
        //    {

        //    }
        //    else
        //    {

        //    }
        //}
    }
}
