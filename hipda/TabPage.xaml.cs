using hipda.Common;
using hipda.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.Activation;

namespace hipda
{
    public enum EnumPostType
    {
        Reply,
        NewThread
    }

    public sealed partial class TabPage : Page, IFileOpenPickerContinuable
    {
        HttpHandle httpClient = HttpHandle.getInstance();

        private const int maxHubSectionCount = 6;
        private const string regexForTitle = @"[^a-zA-Z\d\u4e00-\u9fa5]"; // 用于过滤掉无意义符号

        private readonly NavigationHelper navigationHelper;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private string accountName = "未登录";

        public static readonly DependencyProperty PivotItemTabTypeProperty = DependencyProperty.Register("TabType", typeof(String), typeof(PivotItem), null);
        public static readonly DependencyProperty PivotItemTabIdProperty = DependencyProperty.Register("TabId", typeof(String), typeof(PivotItem), null);

        /// <summary>
        /// 用于回复给某人的参数
        /// </summary>
        private string noticeauthor = string.Empty;
        private string noticetrimstr = string.Empty;
        private string noticeauthormsg = string.Empty;

        /// <summary>
        /// 回复或发贴所上载的图片集合
        /// </summary>
        private List<string> imageNameList = new List<string>();

        /// <summary>
        /// 记录当前发布信息的类型
        /// </summary>
        private EnumPostType currentPostType = EnumPostType.Reply;

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

            if (e.PageState != null && e.PageState.Count > 0) // 从墓碑恢复
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
            else // 正常打开
            {
                if (e.NavigationParameter == null)
                {
                    CreateThreadListTab("14", "WIN版", false);
                    CreateReplyListTab("1427253", "特别感谢 + 关于 + 反馈", false);
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
            Run pictureIconTextBlockRun = (Run)layoutGrid.FindName("pictureIconTextBlockRun");
            Run pagerclipTextBlockRun = (Run)layoutGrid.FindName("pagerclipTextBlockRun");
            Run titleTextBlockRun = (Run)layoutGrid.FindName("titleTextBlockRun");
            Run numbersTextBlockRun = (Run)layoutGrid.FindName("numbersTextBlockRun");
            TextBlock lastPostTextBlock = (TextBlock)layoutGrid.FindName("lastPostTextBlock");

            avatarImageBorder.Opacity = 0;
            ownerInfoTextBlock.Opacity = 0;
            if (thread.AttachType == 1) pictureIconTextBlockRun.Text = "\uE187";
            if (thread.AttachType == 2) pagerclipTextBlockRun.Text = "\uE16C";
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
                        UriSource = new Uri(thread.AvatarUrl)
                    }
                };
                imageBrush.Stretch = Stretch.UniformToFill;

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
            Button showMoreButton = (Button)layoutGrid.FindName("showMoreButton");
            
            avatarImageBorder.Opacity = 0;
            ownerNameTextBlock.Opacity = 0;
            createTimeTextBlock.Opacity = 0;
            menuButton.DataContext = reply;
            floorNumTextBlock.Opacity = 0;

            replyContent.Content = XamlReader.Load(reply.XamlContent);

            if (reply.ImageCount > DataSource.MaxImageCount)
            {
                showMoreButton.DataContext = reply;
                showMoreButton.Content = string.Format("查看本楼层完整原始内容 ({0}P)", reply.ImageCount);
                showMoreButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

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
                    UriSource = new Uri(reply.AvatarUrl)
                }
            };
            imageBrush.Stretch = Stretch.UniformToFill;

            avatarImageBorder.Background = imageBrush;
            avatarImageBorder.Opacity = 1;
        }
        #endregion

        private void viewFullContentButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Reply data = (Reply)btn.DataContext;

            string html = HtmlFormat(data.HtmlContent);
            webView.NavigateToString(html);
            floorOriginalContentPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private async void refreshThreadsButton_Click(object sender, RoutedEventArgs e)
        {
            PivotItem item = (PivotItem)Pivot.SelectedItem;
            ThreadItem data = (ThreadItem)item.DataContext;
            string tabType = item.GetValue(PivotItemTabTypeProperty).ToString();
            if (tabType.Equals("1"))
            {
                string tabId = data.ForumId;
                ListView listView = (ListView)item.FindName("threadsListView" + tabId);

                await RefreshThreadListPage(listView, tabId);
            }
        }

        private async Task RefreshThreadListPage(ListView listView, string forumId)
        {
            listView.ItemsSource = null;

            await DataSource.RefreshThread(forumId);

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

            listView.ItemsSource = cvs.View;
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

                refreshThreadsButton.Visibility = openPostNewPanelButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                openPostReplyPanelButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                postButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else // 回复列表页
            {
                tabPageCommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;

                refreshThreadsButton.Visibility = openPostNewPanelButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                openPostReplyPanelButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
            // 先检查要打开的版块是否已存在于 tabs 中
            // 由于要确保位置位于第一位，所以如果已存在，则删除之然后再创建，以确保位置正确
            if (Pivot.Items.Count > 0)
            {
                var tabs = Pivot.Items;
                foreach (PivotItem tab in tabs)
                {
                    if (tab.Header.ToString().Equals(forumName))
                    {
                        if (Pivot.Items[0] == tab)
                        {
                            Pivot.SelectedIndex = 0;
                            return;
                        }
                        else
                        {
                            Pivot.Items.Remove(tab);
                        }
                    }
                }
            }

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
            // 先检查要打开的版块是否已存在于 tabs 中
            // 由于要确保位置位于所属板块的后面，所以如果已存在，则删除之然后再创建，以确保位置正确
            if (Pivot.Items.Count > 0)
            {
                var tabs = Pivot.Items;
                foreach (PivotItem tab in tabs)
                {
                    if (tab.Header.ToString().Equals(threadTitle))
                    {
                        Pivot.Items.Remove(tab);
                    }
                }
            }

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
            refreshIcon.Stretch = Stretch.Uniform;

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

        #region 打开标签页菜单按钮
        private void openTabForApp_Click(object sender, RoutedEventArgs e)
        {
            CreateReplyListTab("1427253", "特别感谢 + 关于 + 反馈", false);
        }

        private async void openTabForDiscovery_Click(object sender, RoutedEventArgs e)
        {
            if (!accountName.Equals("未登录"))
            {
                CreateThreadListTab("2", "地板", false);
            }
            else
            {
                await new MessageDialog("您必须先登录，才允许访问地板！", "提示").ShowAsync();
            }
        }

        private async void openTabForBuyAndSell_Click(object sender, RoutedEventArgs e)
        {
            if (!accountName.Equals("未登录"))
            {
                CreateThreadListTab("6", "BS版", false);
            }
            else
            {
                await new MessageDialog("您必须先登录，才允许访问BS版！", "提示").ShowAsync();
            }
        }

        private async void openTabForEink_Click(object sender, RoutedEventArgs e)
        {
            if (!accountName.Equals("未登录"))
            {
                CreateThreadListTab("59", "E版", false);
            }
            else
            {
                await new MessageDialog("您必须先登录，才允许访问Eink版！", "提示").ShowAsync();
            }
        }
        #endregion

        private void openPostReplyPanelButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPostReplyPanelAndButton();
        }

        private void openPostNewPanelButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPostNewPanelAndButton();
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (postNewPanel.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                NavigationHelper.IsCanGoBack = false;
                e.Handled = true;

                HidePostNewPanelAndButton();
            }
            else if (postReplyPanel.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                NavigationHelper.IsCanGoBack = false;
                e.Handled = true;

                HidePostReplyPanelAndButton();
            }
            else if (floorOriginalContentPanel.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                NavigationHelper.IsCanGoBack = false;
                e.Handled = true;

                floorOriginalContentPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                
                // 清空内容
                webView.NavigateToString(string.Empty);
            }
            else
            {
                NavigationHelper.IsCanGoBack = true;
            }

            // 清除针对回复的数据
            postReplyContentTextBox.Text = string.Empty;
            noticeauthor = noticetrimstr = noticeauthormsg = string.Empty;
        }

        private async void postButton_Click(object sender, RoutedEventArgs e)
        {
            PivotItem pivotItem = (PivotItem)Pivot.SelectedItem;

            if (currentPostType == EnumPostType.Reply)
            {
                // 获取当前主贴ID
                string threadId = pivotItem.GetValue(PivotItemTabIdProperty).ToString();

                string message = postReplyContentTextBox.Text;
                if (string.IsNullOrEmpty(message))
                {
                    await new MessageDialog("您的发布请求已不成功！\n内容不能为空。", "注意").ShowAsync();
                    return;
                }

                var postData = new Dictionary<string, object>();
                postData.Add("noticeauthor", noticeauthor);
                postData.Add("noticetrimstr", noticetrimstr);
                postData.Add("noticeauthormsg", noticeauthormsg);

                postData.Add("formhash", DataSource.FormHash);
                postData.Add("subject", string.Empty);
                postData.Add("usesig", "1");

                foreach (var imageName in imageNameList)
                {
                    postData.Add(string.Format("attachnew[{0}][description]", imageName), string.Empty);
                }

                // 客户端尾巴
                postData.Add("message", message + "\n\n[img=16,16]http://www.hi-pda.com/forum/attachments/day_140621/1406211752793e731a4fec8f7b.png[/img]");

                // 发布请求
                string resultContent = await httpClient.HttpPost("http://www.hi-pda.com/forum/post.php?action=reply&tid=" + threadId + "&replysubmit=yes&infloat=yes&handlekey=fastpost&inajax=1", postData);
                if (!resultContent.Contains("您的回复已经发布"))
                {
                    await new MessageDialog("您的发布请求已不成功！\n可能是你连续发布过快，请稍候再试。", "注意").ShowAsync();
                    return;
                }

                HidePostReplyPanelAndButton();

                postReplyContentTextBox.Text = string.Empty;
                noticeauthor = string.Empty;
                noticetrimstr = string.Empty;
                noticeauthormsg = string.Empty;

                // 清空上载图片记录之集合
                imageNameList.Clear();

                // 刷新数据
                ListView listView = (ListView)pivotItem.FindName("repliesListView" + threadId);
                ICollectionView view = (ICollectionView)listView.ItemsSource;

                // count = 1 表示是要刷新
                await view.LoadMoreItemsAsync(1);
            }
            else if (currentPostType == EnumPostType.NewThread)
            {
                // 获取当前版块ID
                string forumId = pivotItem.GetValue(PivotItemTabIdProperty).ToString();

                string subject = postNewTitleTextBox.Text;
                if (string.IsNullOrEmpty(subject))
                {
                    await new MessageDialog("您的发布请求已不成功！\n标题不能为空。", "注意").ShowAsync();
                    return;
                }

                string message = postNewContentTextBox.Text;
                if (string.IsNullOrEmpty(message))
                {
                    await new MessageDialog("您的发布请求已不成功！\n内容不能为空。", "注意").ShowAsync();
                    return;
                }

                var postData = new Dictionary<string, object>();
                postData.Add("formhash", DataSource.FormHash);
                postData.Add("wysiwyg", "1");
                postData.Add("iconid", string.Empty);
                postData.Add("subject", subject);
                postData.Add("tags", string.Empty);
                postData.Add("attention_add", "1");
                postData.Add("usesig", "1");

                foreach (var imageName in imageNameList)
                {
                    postData.Add(string.Format("attachnew[{0}][description]", imageName), string.Empty);
                }

                // 客户端尾巴
                postData.Add("message", message + "\n\n[img=16,16]http://www.hi-pda.com/forum/attachments/day_140621/1406211752793e731a4fec8f7b.png[/img]");

                // 发布请求
                string resultContent = await httpClient.HttpPost("http://www.hi-pda.com/forum/post.php?action=newthread&fid=" + forumId + "&extra=&topicsubmit=yes", postData);
                if (resultContent.Contains("对不起，您两次发表间隔少于"))
                {
                    await new MessageDialog("您的发布请求已不成功！\n可能是你连续发布过快，请稍候再试。", "注意").ShowAsync();
                    return;
                }

                HidePostNewPanelAndButton();

                postNewTitleTextBox.Text = string.Empty;
                postNewContentTextBox.Text = string.Empty;

                // 清空上载图片记录之集合
                imageNameList.Clear();

                // 刷新数据
                ListView listView = (ListView)pivotItem.FindName("threadsListView" + forumId);
                await RefreshThreadListPage(listView, forumId);
            }
        }

        private void HidePostReplyPanelAndButton()
        {
            postReplyPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            tabPageCommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;

            foreach (AppBarButton btn in tabPageCommandBar.SecondaryCommands)
            {
                btn.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            openPostReplyPanelButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            postButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void ShowPostReplyPanelAndButton()
        {
            postReplyPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            currentPostType = EnumPostType.Reply;

            tabPageCommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;

            foreach (AppBarButton btn in tabPageCommandBar.SecondaryCommands)
            {
                btn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            openPostReplyPanelButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            postButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void HidePostNewPanelAndButton()
        {
            postNewPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            foreach (AppBarButton btn in tabPageCommandBar.SecondaryCommands)
            {
                btn.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            refreshThreadsButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            openPostNewPanelButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            postButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void ShowPostNewPanelAndButton()
        {
            postNewPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            currentPostType = EnumPostType.NewThread;

            foreach (AppBarButton btn in tabPageCommandBar.SecondaryCommands)
            {
                btn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            refreshThreadsButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            openPostNewPanelButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            postButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void replyReplyMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Reply data = (sender as MenuFlyoutItem).DataContext as Reply;

            noticeauthor = string.Format("r|{0}|[i]{1}[/i]", data.OwnerId, data.OwnerName);
            noticetrimstr = string.Format("[b]回复 {0}# [i]{1}[/i] [/b]\n\n", data.Floor, data.OwnerName);
            noticeauthormsg = data.TextContent;

            postReplyContentTextBox.Text = noticetrimstr;

            ShowPostReplyPanelAndButton();
        }

        private void refReplyMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Reply data = (sender as MenuFlyoutItem).DataContext as Reply;

            noticeauthor = string.Format("r|{0}|[i]{1}[/i]", data.OwnerId, data.OwnerName);
            noticetrimstr = string.Format("[quote]回复 {0}# {1}\n{2}[/quote]\n\n", data.Floor, data.OwnerName, data.TextContent);
            noticeauthormsg = data.TextContent;

            postReplyContentTextBox.Text = noticetrimstr;

            ShowPostReplyPanelAndButton();
        }

        private void viewFullContentMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Reply data = (sender as MenuFlyoutItem).DataContext as Reply;

            string html = HtmlFormat(data.HtmlContent);
            webView.NavigateToString(html);
            floorOriginalContentPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private static string HtmlFormat(string htmlStr)
        {
            // 整理HTML
            string html = htmlStr
                .Replace(@"<img src=""images/default/attachimg.gif"" border=""0"">", string.Empty)
                .Replace(@" src=""images/common/none.gif""", string.Empty)
                .Replace(@" file=""attachments/", @" src=""http://www.hi-pda.com/forum/attachments/")
                .Replace(@" src=""images/smilies/", @" src=""http://www.hi-pda.com/forum/images/smilies/")
                .Replace(@" src=""images/attachicons/", @" src=""http://www.hi-pda.com/forum/images/attachicons/");

            // 移除任意连续50个的非文字字符
            MatchCollection matchsForInvalidHtml2 = new Regex(@"\p{P}{30}").Matches(html);
            if (matchsForInvalidHtml2 != null && matchsForInvalidHtml2.Count > 0)
            {
                for (int i = 0; i < matchsForInvalidHtml2.Count; i++)
                {
                    var m = matchsForInvalidHtml2[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    html = html.Replace(placeHolder, string.Empty);
                }
            }

            html = string.Format(@"<html><head><style>table{{width:100%;}}blockquote{{margin:20px;width:100%;}}img{{display:block;max-width:800px !important;}}</style></head><body style=""padding:20px 0;white-space:normal;word-break:break-all;word-wrap:break-word;"">{0}</body></html>", html);
            return html;
        }

        private async void closeTab_Click(object sender, RoutedEventArgs e)
        {
            // 获取当前tab页
            if (Pivot.Items.Count > 1)
            {
                PivotItem pivotItem = (PivotItem)Pivot.SelectedItem;
                Pivot.Items.Remove(pivotItem);
            }
            else
            {
                await new MessageDialog("您不能删除最后一个标签页！", "提示").ShowAsync();
            }
        }

        private void addFaceButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string value = btn.Content.ToString();

            string message = string.Empty;
            if (value.StartsWith(":"))
            {
                message = string.Format(" {0} ", value);
            }
            else
            {
                message = string.Format(@" {{:{0}:}} ", value);
            }

            int occurences = 0;
            string originalContent = string.Empty;

            if (currentPostType == EnumPostType.Reply)
            {
                originalContent = postReplyContentTextBox.Text;

                for (var i = 0; i < postReplyContentTextBox.SelectionStart + occurences; i++)
                {
                    if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                        occurences++;
                }

                int cursorPosition = postReplyContentTextBox.SelectionStart + occurences;
                postReplyContentTextBox.Text = postReplyContentTextBox.Text.Insert(cursorPosition, message);
            }
            else if (currentPostType == EnumPostType.NewThread)
            {
                originalContent = postNewContentTextBox.Text;

                for (var i = 0; i < postNewContentTextBox.SelectionStart + occurences; i++)
                {
                    if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                        occurences++;
                }

                int cursorPosition = postNewContentTextBox.SelectionStart + occurences;
                postNewContentTextBox.Text = postNewContentTextBox.Text.Insert(cursorPosition, message);
            }
        }

        private void replyMenu_Opening(object sender, object e)
        {
            tabPageCommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
        }

        private void replyListItemGrid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

        private void addPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            // Launch file open picker and caller app is suspended and may be terminated if required
            openPicker.PickSingleFileAndContinue();
        }

        /// <summary>
        /// Handle the returned files from file picker
        /// This method is triggered by ContinuationManager based on ActivationKind
        /// </summary>
        /// <param name="args">File open picker continuation activation argment. It cantains the list of files user selected with file open picker </param>
        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files.Count > 0)
            {
                StorageFile file = args.Files[0];
                if (file != null)
                {
                    imageNameList.Add(file.Name);
                    byte[] image = await ImageHelper.LoadAsync(file);

                    var data = new Dictionary<string, object>();
                    data.Add("uid", DataSource.UserId);
                    data.Add("hash", DataSource.Hash);

                    string result = await httpClient.HttpPostFile("http://www.hi-pda.com/forum/misc.php?action=swfupload&operation=upload&simple=1&type=image", data, file.Name, "image/jpg", "Filedata", image);
                    if (result.Contains("DISCUZUPLOAD|"))
                    {
                        string value = result.Split('|')[2];
                        value = string.Format("[attachimg]{0}[/attachimg]", value);

                        int occurences = 0;
                        string originalContent = string.Empty;

                        if (currentPostType == EnumPostType.Reply)
                        {
                            originalContent = postReplyContentTextBox.Text;

                            for (var i = 0; i < postReplyContentTextBox.SelectionStart + occurences; i++)
                            {
                                if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                                    occurences++;
                            }

                            int cursorPosition = postReplyContentTextBox.SelectionStart + occurences;
                            postReplyContentTextBox.Text = postReplyContentTextBox.Text.Insert(cursorPosition, value);
                        }
                        else if (currentPostType == EnumPostType.NewThread)
                        {
                            originalContent = postNewContentTextBox.Text;

                            for (var i = 0; i < postNewContentTextBox.SelectionStart + occurences; i++)
                            {
                                if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                                    occurences++;
                            }

                            int cursorPosition = postNewContentTextBox.SelectionStart + occurences;
                            postNewContentTextBox.Text = postNewContentTextBox.Text.Insert(cursorPosition, value);
                        }
                    }
                }
            }
        }

        private static int GeneratePostTime()
        {
            var startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var diff = DateTime.Now - startTime;
            return (int)Math.Floor(diff.TotalSeconds);
        }
    }
}
