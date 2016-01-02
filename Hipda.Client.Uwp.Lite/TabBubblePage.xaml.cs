using HipdaUwpLite.Common;
using HipdaUwpLite.Data;
using HipdaUwpLite.Settings;
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
using Windows.UI.Core;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Hipda.Http;
using System.Threading;

namespace Hipda.Client.Uwp.Lite
{
    public sealed partial class TabBubblePage : Page
    {
        HttpHandle httpClient = HttpHandle.GetInstance();

        // 最后发布消息时间，用于限制发布速度（30秒限制）
        private DateTime lastPostTime = DateTime.Now.AddSeconds(-31);

        // 用于限制允许显示标签页的总数量
        private int maxHubSectionCount = 6;

        // 用于过滤掉无意义符号
        private string regexForTitle = @"[^a-zA-Z\d\u4e00-\u9fa5]";

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
        /// 是回复，还是发贴
        /// </summary>
        private EnumPostType currentPostType = EnumPostType.Reply;

        /// <summary>
        /// 记录当前编辑信息的类型
        /// 是新增，还是修改
        /// </summary>
        private EnumEditType currentEditType = EnumEditType.Add;

        #region 主题样式
        private void ThemeClassic()
        {
            tabPage.RequestedTheme = ElementTheme.Light;

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = ((SolidColorBrush)this.Resources["SystemControlBackgroundAccentBrush"]).Color;
            }

            ResourceDictionary r = tabPage.Resources;
            ((SolidColorBrush)r["ItemBgColor"]).Color = Colors.White;
            ((SolidColorBrush)r["MainFontColor"]).Color = Colors.DimGray;
            ((SolidColorBrush)r["CommandBarBgColor"]).Color = Color.FromArgb(255, 219, 219, 219);
            ((SolidColorBrush)r["MaskBgColor"]).Color = Colors.LightGray;
            ((SolidColorBrush)r["PanelBgColor"]).Color = Colors.White;
            ((SolidColorBrush)r["Panel2BgColor"]).Color = Colors.WhiteSmoke;

            tabPage.Background = new SolidColorBrush(Color.FromArgb(255, 239, 239, 239));
            tabPageCommandBar.Background = tabPage.Resources["CommandBarBgColor"] as SolidColorBrush;
        }

        private void ThemeDark()
        {
            tabPage.RequestedTheme = ElementTheme.Dark;

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = Colors.Black;
            }
            
            ResourceDictionary r = tabPage.Resources;
            ((SolidColorBrush)r["ItemBgColor"]).Color = Color.FromArgb(255, 12, 12, 12);
            ((SolidColorBrush)r["MainFontColor"]).Color = Colors.DimGray;
            ((SolidColorBrush)r["CommandBarBgColor"]).Color = Color.FromArgb(255, 16, 16, 16);
            ((SolidColorBrush)r["MaskBgColor"]).Color = Color.FromArgb(255, 150, 150, 150);
            ((SolidColorBrush)r["PanelBgColor"]).Color = Colors.White;
            ((SolidColorBrush)r["Panel2BgColor"]).Color = Colors.WhiteSmoke;

            tabPage.Background = new SolidColorBrush(Colors.Black);
            tabPageCommandBar.Background = tabPage.Resources["CommandBarBgColor"] as SolidColorBrush;
        }

        private void ThemeBlueSky()
        {
            tabPage.RequestedTheme = ElementTheme.Light;

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = Color.FromArgb(255, 108, 151, 193);
            }

            ResourceDictionary r = tabPage.Resources;
            ((SolidColorBrush)r["ItemBgColor"]).Color = Color.FromArgb(255, 196, 229, 254);
            ((SolidColorBrush)r["MainFontColor"]).Color = Color.FromArgb(255, 102, 102, 102);
            ((SolidColorBrush)r["CommandBarBgColor"]).Color = Colors.WhiteSmoke;
            ((SolidColorBrush)r["MaskBgColor"]).Color = Color.FromArgb(255, 196, 229, 254);
            ((SolidColorBrush)r["PanelBgColor"]).Color = Colors.White;
            ((SolidColorBrush)r["Panel2BgColor"]).Color = Colors.WhiteSmoke;

            var bgImageBrush = new ImageBrush
            {
                ImageSource = new BitmapImage
                {
                    UriSource = new Uri("ms-appx:///Assets/BackgroundImages/BlueSky.jpg")
                }
            };
            tabPage.Background = bgImageBrush;
            tabPageCommandBar.Background = tabPage.Resources["CommandBarBgColor"] as SolidColorBrush;
        }

        private void ThemeStarSky()
        {
            tabPage.RequestedTheme = ElementTheme.Dark;

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = Color.FromArgb(255, 7, 18, 40);
            }
            
            ResourceDictionary r = tabPage.Resources;
            ((SolidColorBrush)r["ItemBgColor"]).Color = Color.FromArgb(255, 5, 17, 36);
            ((SolidColorBrush)r["MainFontColor"]).Color = Color.FromArgb(255, 102, 102, 102);
            ((SolidColorBrush)r["CommandBarBgColor"]).Color = Colors.Black;
            ((SolidColorBrush)r["MaskBgColor"]).Color = Color.FromArgb(255, 13, 79, 112);
            ((SolidColorBrush)r["PanelBgColor"]).Color = Colors.White;
            ((SolidColorBrush)r["Panel2BgColor"]).Color = Colors.WhiteSmoke;

            var bgImageBrush = new ImageBrush
            {
                ImageSource = new BitmapImage
                {
                    UriSource = new Uri("ms-appx:///Assets/BackgroundImages/StarSky.jpg")
                }
            };
            tabPage.Background = bgImageBrush;
            tabPageCommandBar.Background = tabPage.Resources["CommandBarBgColor"] as SolidColorBrush;
        }
        #endregion

        public TabBubblePage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Disabled;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            switch (App.ThemeId)
            {
                case 0:
                    ThemeClassic();
                    break;
                case 1:
                    ThemeDark();
                    break;
                case 2:
                    ThemeBlueSky();
                    break;
                case 3:
                    ThemeStarSky();
                    break;
            }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
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

            SystemNavigationManager.GetForCurrentView().BackRequested += TitleBackButton_BackRequested;
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);

            SystemNavigationManager.GetForCurrentView().BackRequested -= TitleBackButton_BackRequested;
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            }
        }

        #endregion

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            #region 同步开关按钮之开关状态
            sortForThreadListButton.IsChecked = SortForThreadSettings.GetSortType.Equals("dateline");
            reverseListButton.IsChecked = SortForReplySettings.GetSortType == 1;
            #endregion

            #region 读取当前账号的名称
            Account account = AccountSettings.GetDefault();
            accountName = account != null ? account.Username : "未登录";
            if (accountName.Length > 3)
            {
                accountName = string.Format("{0}*{1}", accountName.Substring(0, 2), accountName.Last());
            }
            #endregion

            #region 从墓碑恢复
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
            #endregion

            #region 正常打开
            else
            {
                if (e.NavigationParameter == null)
                {
                    CreateThreadListTab("14", "WIN版", false);
                    CreateReplyListTab("1427253", "反馈", false);
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
            #endregion

            #region 生成按账号发布信息之菜单
            var accList = AccountSettings.List;
            if (accList.Count > 1)
            {
                var mf = new MenuFlyout();
                foreach (var acc in accList)
                {
                    MenuFlyoutItem m = new MenuFlyoutItem();
                    m.Text = string.Format("使用 {0} 账号发布", acc.Username);
                    m.DataContext = new AccountForSend { AccountUsername = acc.Username, AccountKeyName = acc.Key };
                    m.Click += AccountForSend_Click;
                    mf.Items.Add(m);
                }
                sendButton.Flyout = mf;
            }
            else
            {
                sendButton.Click += async (s, args) => {
                    await Send();
                };
            }
            
            #endregion
        }

        private async void AccountForSend_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            AccountForSend data = item.DataContext as AccountForSend;
            string accUsername = data.AccountUsername;
            await AccountSettings.SetDefault(data.AccountKeyName);

            await Send();
        }

        private async Task Send()
        {
            uploadStateInfo.Text = "点击 + 号图标可批量上传任何文件。";
            uploadStateInfo2.Text = "点击 + 号图标可批量上传任何文件。";

            DateTime nowTime = DateTime.Now;
            TimeSpan ts = (nowTime - lastPostTime);
            if (ts.TotalSeconds <= 30)
            {
                await new MessageDialog(string.Format("您的发布请求不成功！\n您发布速度过快，请于{0:f1}秒后再发布。", 31 - ts.TotalSeconds), "注意").ShowAsync();
                return;
            }

            PivotItem pivotItem = (PivotItem)Pivot.SelectedItem;

            if (currentEditType == EnumEditType.Add)
            {
                #region 新增回复或话题
                if (currentPostType == EnumPostType.Reply)
                {
                    // 获取当前主贴ID
                    string threadId = pivotItem.GetValue(PivotItemTabIdProperty).ToString();

                    string message = postReplyContentTextBox.Text;
                    if (string.IsNullOrEmpty(message))
                    {
                        await new MessageDialog("您的发布请求不成功！\n内容不能为空。", "注意").ShowAsync();
                        return;
                    }

                    string messageTail = message.Trim() + "\n \n" + DataSource.MessageTail; // 客户端尾巴，两个换行符之间的空格用于避免换行符被合并为一个

                    var postData = new List<KeyValuePair<string, object>>();
                    postData.Add(new KeyValuePair<string, object>("formhash", DataSource.FormHash));
                    postData.Add(new KeyValuePair<string, object>("subject", string.Empty));
                    postData.Add(new KeyValuePair<string, object>("message", messageTail));
                    postData.Add(new KeyValuePair<string, object>("usesig", "1"));
                    postData.Add(new KeyValuePair<string, object>("noticeauthor", noticeauthor));
                    postData.Add(new KeyValuePair<string, object>("noticetrimstr", noticetrimstr));
                    postData.Add(new KeyValuePair<string, object>("noticeauthormsg", noticeauthormsg));

                    // 图片信息
                    foreach (var imageName in imageNameList)
                    {
                        postData.Add(new KeyValuePair<string, object>(string.Format("attachnew[{0}][description]", imageName), string.Empty));
                    }

                    // 发布请求
                    string url = string.Format("http://www.hi-pda.com/forum/post.php?action=reply&tid={0}&replysubmit=yes&infloat=yes&handlekey=fastpost&inajax=1", threadId);
                    var cts = new CancellationTokenSource();
                    string resultContent = await httpClient.PostAsync(url, postData, cts);
                    if (!resultContent.Contains("您的回复已经发布"))
                    {
                        await new MessageDialog("您的发布请求不成功！\n请检查是否已登录或网络连接是否正常。", "注意").ShowAsync();
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
                    await RefreshRepliesListPage(threadId, listView);
                }
                else if (currentPostType == EnumPostType.NewThread)
                {
                    // 获取当前版块ID
                    string forumId = pivotItem.GetValue(PivotItemTabIdProperty).ToString();

                    string subject = postNewTitleTextBox.Text;
                    if (string.IsNullOrEmpty(subject))
                    {
                        await new MessageDialog("您的发布请求不成功！\n标题不能为空。", "注意").ShowAsync();
                        return;
                    }

                    string message = postNewContentTextBox.Text;
                    if (string.IsNullOrEmpty(message))
                    {
                        await new MessageDialog("您的发布请求不成功！\n内容不能为空。", "注意").ShowAsync();
                        return;
                    }

                    var postData = new List<KeyValuePair<string, object>>();
                    postData.Add(new KeyValuePair<string, object>("formhash", DataSource.FormHash));
                    postData.Add(new KeyValuePair<string, object>("wysiwyg", "1"));
                    postData.Add(new KeyValuePair<string, object>("iconid", "0"));
                    postData.Add(new KeyValuePair<string, object>("subject", subject));
                    postData.Add(new KeyValuePair<string, object>("message", message.Trim() + "\n \n" + DataSource.MessageTail)); // 客户端尾巴，两个换行符之间的空格用于避免换行符被合并为一个
                    postData.Add(new KeyValuePair<string, object>("attention_add", "1"));
                    postData.Add(new KeyValuePair<string, object>("usesig", "1"));

                    // 图片信息
                    foreach (var imageName in imageNameList)
                    {
                        postData.Add(new KeyValuePair<string, object>(string.Format("attachnew[{0}][description]", imageName), string.Empty));
                    }

                    // 发布请求
                    string url = string.Format("http://www.hi-pda.com/forum/post.php?action=newthread&fid={0}&extra=&topicsubmit=yes", forumId);
                    var cts = new CancellationTokenSource();
                    string resultContent = await httpClient.PostAsync(url, postData, cts);
                    if (resultContent.Contains("对不起，您两次发表间隔少于"))
                    {
                        await new MessageDialog("您的发布请求不成功！\n可能是你连续发布过快，请稍候再试。", "注意").ShowAsync();
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
                #endregion
            }
            else if (currentEditType == EnumEditType.Modify)
            {
                #region 修改回复或话题
                if (currentPostType == EnumPostType.Reply)
                {
                    string threadId = DataSource.ContentForEdit.ThreadId;

                    string message = postReplyContentTextBox.Text;
                    if (string.IsNullOrEmpty(message))
                    {
                        await new MessageDialog("您的修改请求不成功！\n内容不能为空。", "注意").ShowAsync();
                        return;
                    }

                    var postData = new List<KeyValuePair<string, object>>();
                    postData.Add(new KeyValuePair<string, object>("formhash", DataSource.FormHash));
                    postData.Add(new KeyValuePair<string, object>("subject", string.Empty));
                    postData.Add(new KeyValuePair<string, object>("message", message.Trim() + "\n \n" + DataSource.MessageTail)); // 客户端尾巴，两个换行符之间的空格用于避免换行符被合并为一个
                    postData.Add(new KeyValuePair<string, object>("wysiwyg", "1"));
                    postData.Add(new KeyValuePair<string, object>("tid", threadId));
                    postData.Add(new KeyValuePair<string, object>("pid", DataSource.ContentForEdit.PostId));
                    postData.Add(new KeyValuePair<string, object>("iconid", "0"));

                    // 图片信息
                    foreach (var imageName in imageNameList)
                    {
                        postData.Add(new KeyValuePair<string, object>(string.Format("attachnew[{0}][description]", imageName), string.Empty));
                    }

                    // 发布请求
                    string url = "http://www.hi-pda.com/forum/post.php?action=edit&extra=&editsubmit=yes&mod=";
                    var cts = new CancellationTokenSource();
                    await httpClient.PostAsync(url, postData, cts);

                    HidePostReplyPanelAndButton();

                    postReplyContentTextBox.Text = string.Empty;

                    // 清空上载图片记录之集合
                    imageNameList.Clear();

                    // 刷新当前贴子回复列表
                    ListView listView = (ListView)pivotItem.FindName("repliesListView" + threadId);
                    await RefreshReplyListPage(listView, threadId);
                }
                else if (currentPostType == EnumPostType.NewThread)
                {
                    string threadId = DataSource.ContentForEdit.ThreadId;

                    string subject = postNewTitleTextBox.Text;
                    if (string.IsNullOrEmpty(subject))
                    {
                        await new MessageDialog("您的修改请求不成功！\n标题不能为空。", "注意").ShowAsync();
                        return;
                    }

                    string message = postNewContentTextBox.Text;
                    if (string.IsNullOrEmpty(message))
                    {
                        await new MessageDialog("您的修改请求不成功！\n内容不能为空。", "注意").ShowAsync();
                        return;
                    }

                    var postData = new List<KeyValuePair<string, object>>();
                    postData.Add(new KeyValuePair<string, object>("formhash", DataSource.FormHash));
                    postData.Add(new KeyValuePair<string, object>("subject", subject));
                    postData.Add(new KeyValuePair<string, object>("message", message.Trim() + "\n \n" + DataSource.MessageTail)); // 客户端尾巴，两个换行符之间的空格用于避免换行符被合并为一个
                    postData.Add(new KeyValuePair<string, object>("wysiwyg", "1"));
                    postData.Add(new KeyValuePair<string, object>("tid", threadId));
                    postData.Add(new KeyValuePair<string, object>("pid", DataSource.ContentForEdit.PostId));
                    postData.Add(new KeyValuePair<string, object>("iconid", "0"));

                    // 图片信息
                    foreach (var imageName in imageNameList)
                    {
                        postData.Add(new KeyValuePair<string, object>(string.Format("attachnew[{0}][description]", imageName), string.Empty));
                    }

                    // 发布请求
                    string url = "http://www.hi-pda.com/forum/post.php?action=edit&extra=&editsubmit=yes&mod=";
                    var cts = new CancellationTokenSource();
                    await httpClient.PostAsync(url, postData, cts);

                    HidePostNewModifyPanelAndButton();

                    postNewTitleTextBox.Text = string.Empty;
                    postNewContentTextBox.Text = string.Empty;

                    // 清空上载图片记录之集合
                    imageNameList.Clear();

                    // 使用新的tab标题
                    pivotItem.Header = Regex.Replace(subject, regexForTitle, string.Empty);

                    // 刷新当前贴子回复列表
                    ListView listView = (ListView)pivotItem.FindName("repliesListView" + threadId);
                    await RefreshReplyListPage(listView, threadId);
                }
                #endregion
            }

            lastPostTime = DateTime.Now;
        }

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

            TextBlock ownerInfoTextBlock = (TextBlock)layoutGrid.FindName("ownerInfoTextBlock");
            Run pictureIconTextBlockRun = (Run)layoutGrid.FindName("pictureIconTextBlockRun");
            Run pagerclipTextBlockRun = (Run)layoutGrid.FindName("pagerclipTextBlockRun");
            Run titleTextBlockRun = (Run)layoutGrid.FindName("titleTextBlockRun");
            Run numbersTextBlockRun = (Run)layoutGrid.FindName("numbersTextBlockRun");

            ownerInfoTextBlock.Opacity = 0;
            pictureIconTextBlockRun.Text = thread.AttachType == 1 ? "\uEB9F" : string.Empty;
            pagerclipTextBlockRun.Text = thread.AttachType == 2 ? "\uE16C" : string.Empty;

            titleTextBlockRun.Text = thread.Title;
            numbersTextBlockRun.Text = thread.Numbers;

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
            Run lastPostTextBlockRun = (Run)layoutGrid.FindName("lastPostTextBlockRun");

            ownerNameTextBlockRun.Text = thread.OwnerName;
            createTimeTextBlockRun.Text = thread.CreateTime;
            ownerInfoTextBlock.Opacity = 1;

            lastPostTextBlockRun.Text = thread.LastPostInfo;

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
                avatarImageBorder.DataContext = thread;

                var imageBrush = new ImageBrush
                {
                    ImageSource = new BitmapImage
                    {
                        DecodePixelWidth = 50, // natural px width of image source
                        UriSource = new Uri(thread.AvatarUrl)
                    }
                };
                imageBrush.ImageFailed += (sender2, e2) => {
                    var imageBrush2 = sender2 as ImageBrush;
                    imageBrush2.ImageSource = new BitmapImage
                    {
                        DecodePixelWidth = 60, // natural px width of image source
                        UriSource = new Uri("ms-appx:///Assets/Faces/no_face.png")
                    };
                };
                imageBrush.Stretch = Stretch.UniformToFill;
                avatarImageBorder.Background = imageBrush;
            }
        }
        #endregion

        /// <summary>
        /// 在贴子项上单击时调用
        /// </summary>
        private async void ThreadItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            Thread thread = (Thread)e.ClickedItem;
            string threadId = thread.Id;
            string threadFullTitle = thread.Title;
            string threadTitle = Regex.Replace(threadFullTitle, regexForTitle, string.Empty);
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

            // 如果是从纯文本模式切换到非纯文本模式，则重新转换HTML为XAML
            if (string.IsNullOrEmpty(reply.XamlContent) && !string.IsNullOrEmpty(reply.HtmlContent))
            {
                reply = DataSource.ReconvertReplyXamlContent(reply.Floor, reply.ThreadId);
            }

            Grid layoutGrid = (Grid)args.ItemContainer.ContentTemplateRoot;

            Border avatarImageBorder = (Border)layoutGrid.FindName("avatarImageBorder");
            TextBlock threadTitleTextBlock = (TextBlock)layoutGrid.FindName("threadTitleTextBlock");
            Run ownerNameTextBlockRun = (Run)layoutGrid.FindName("ownerNameTextBlock");
            Run createTimeTextBlockRun = (Run)layoutGrid.FindName("createTimeTextBlock");
            Button menuButton = (Button)layoutGrid.FindName("menuButton");
            ContentControl replyContent = (ContentControl)layoutGrid.FindName("replyContent");
            Button showMoreButton = (Button)layoutGrid.FindName("showMoreButton");
            MenuFlyoutItem modifyMenuFlyoutItem = (MenuFlyoutItem)layoutGrid.FindName("modifyMenuFlyoutItem");

            if (reply.Floor == 1 && !string.IsNullOrEmpty(reply.ThreadTitle))
            {
                threadTitleTextBlock.Text = reply.ThreadTitle;
                threadTitleTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                threadTitleTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            ownerNameTextBlockRun.Text = reply.OwnerName;
            createTimeTextBlockRun.Text = reply.CreateTime;
            menuButton.DataContext = reply;

            try
            {
                replyContent.Content = XamlReader.Load(reply.XamlContent);
            }
            catch
            {
                string text = Regex.Replace(reply.TextContent, regexForTitle, "~");
                string xaml = string.Format(@"<RichTextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><Paragraph>{0}</Paragraph></RichTextBlock>", text);
                replyContent.Content = XamlReader.Load(xaml);
            }

            // 如果楼层内容的作者是当前账号，则显示编辑按钮
            if (reply.OwnerId == DataSource.UserId)
            {
                modifyMenuFlyoutItem.IsEnabled = true;
            }
            else
            {
                modifyMenuFlyoutItem.IsEnabled = false;
            }
            
            if (reply.ImageCount > DataSource.MaxImageCount)
            {
                showMoreButton.DataContext = reply;
                showMoreButton.Content = string.Format("查看本楼层完整原始内容 ({0}P)", reply.ImageCount);
                showMoreButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                showMoreButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            args.RegisterUpdateCallback(ShowAuthorFace);
        }

        private void ShowAuthorFace(
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

            Border avatarImageBorder = (Border)layoutGrid.FindName("avatarImageBorder");

            var imageBrush = new ImageBrush
            {
                ImageSource = new BitmapImage
                {
                    DecodePixelWidth = 50, // natural px width of image source
                    UriSource = new Uri(reply.AvatarUrl)
                }
            };
            imageBrush.ImageFailed += (sender2, e2) =>
            {
                var imageBrush2 = sender2 as ImageBrush;
                imageBrush2.ImageSource = new BitmapImage
                {
                    DecodePixelWidth = 50, // natural px width of image source
                    UriSource = new Uri("ms-appx:///Assets/Faces/no_face.png")
                };
            };
            imageBrush.Stretch = Stretch.UniformToFill;
            avatarImageBorder.Background = imageBrush;
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
            ThreadData data = (ThreadData)item.DataContext;
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
            if (replyProgressBar.Visibility == Visibility.Collapsed)
            {
                listView.ItemsSource = null;

                await DataSource.RefreshThreadList(forumId);

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
        }

        private async Task SearchThreadListPage(string keywords, int searchType, ListView listView, string forumId)
        {
            if (replyProgressBar.Visibility == Visibility.Collapsed)
            {
                listView.ItemsSource = null;

                bool hasData = await DataSource.SearchThreadList(keywords, searchType, forumId);
                if (hasData == false)
                {
                    var dialog = new MessageDialog("对不起，没有找到匹配结果。", "版内搜索");
                    await dialog.ShowAsync();
                    await RefreshThreadListPage(listView, forumId);
                    return;
                }

                var cvs = new CollectionViewSource();
                cvs.Source = new GeneratorIncrementalLoadingClass<Thread>(DataSource.SearchPageSize, async pageNo =>
                {
                    // 加载分页数据，并写入静态类中
                    // 返回的是本次加载的数据量
                    return await DataSource.SearchLoadThreadsCountAsync(keywords, searchType, forumId, pageNo, () =>
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
        }

        /// <summary>
        /// 先清空当前贴子所有回复数据，再全部重新加载
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="threadId"></param>
        /// <returns></returns>
        private async Task RefreshReplyListPage(ListView listView, string threadId)
        {
            if (replyProgressBar.Visibility == Visibility.Collapsed)
            {
                replyProgressBar.Visibility = Visibility.Visible;

                listView.ItemsSource = null;
                await DataSource.RefreshReplyList(threadId);
                var cvs = CreateDataViewForReplyListPage(threadId);
                listView.ItemsSource = cvs.View;

                replyProgressBar.Visibility = Visibility.Collapsed;
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
                // 显示 贴子列表页底部按钮
                openPostNewPanelButton.Visibility =
                    refreshThreadsButton.Visibility =
                    sortForThreadListButton.Visibility = 
                    changeThemeButton.Visibility =
                    openDialogForSearch.Visibility =
                    openMyThreads.Visibility =
                    Windows.UI.Xaml.Visibility.Visible;

                // 隐藏 回复列表页底部按钮
                openPostReplyPanelButton.Visibility = 
                    refreshRepliesButton.Visibility = 
                    reverseListButton.Visibility = 
                    Windows.UI.Xaml.Visibility.Collapsed;

                // 隐藏 发布信息状态之底部按钮
                sendButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else // 回复列表页
            {
                // 显示 回复列表页底部按钮
                openPostReplyPanelButton.Visibility = 
                    refreshRepliesButton.Visibility = 
                    reverseListButton.Visibility = 
                    Windows.UI.Xaml.Visibility.Visible;

                // 隐藏 贴子列表页底部按钮
                openPostNewPanelButton.Visibility =
                    refreshThreadsButton.Visibility =
                    sortForThreadListButton.Visibility =
                    changeThemeButton.Visibility =
                    openDialogForSearch.Visibility =
                    openMyThreads.Visibility =
                    Windows.UI.Xaml.Visibility.Collapsed;

                // 隐藏 发布信息状态之底部按钮
                sendButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            #endregion

            #region 刷新 顶部状态栏
            StringBuilder navTextContainer = new StringBuilder();

            foreach (PivotItem pivotItem in Pivot.Items)
            {
                string text = pivotItem.Header.ToString();
                if (!string.IsNullOrEmpty(text))
                {
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
            }

            string navText = navTextContainer.ToString();
            if (!string.IsNullOrEmpty(navText))
            {
                navText = navText.Substring(0, navText.Length - 1);
            }

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar.GetForCurrentView().ProgressIndicator.Text = string.Format("{0} > {1}", accountName, navText);
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
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

            var cvs = CreateDataViewForThreadListPage(forumId);

            var listView = new ListView
            {
                Name = string.Format("threadsListView{0}", forumId),
                Padding = new Thickness(10, 0, 10, 0),
                ItemsSource = cvs.View,
                IsItemClickEnabled = true,
                ItemContainerStyle = ThreadItemStyle,
                ItemTemplate = ThreadListItemTemplate,
                IncrementalLoadingTrigger = IncrementalLoadingTrigger.Edge,
                SelectionMode = ListViewSelectionMode.None
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
            if (Pivot.Items.Count == maxHubSectionCount)
            {
                PivotItem lastItem = (PivotItem)Pivot.Items.Last();
                string tabType = lastItem.GetValue(PivotItemTabTypeProperty).ToString();
                if (tabType.Equals("1"))
                {
                    Pivot.Items.Remove(lastItem);
                }
            }
        }

        private CollectionViewSource CreateDataViewForThreadListPage(string forumId)
        {
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
            return cvs;
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

            // 在静态数据类中创建一个主贴容器，用来装载回复数据列表
            pivotItem.DataContext = DataSource.GetReply(threadId, threadTitle);

            var cvs = CreateDataViewForReplyListPage(threadId);

            var listView = new ListView
            {
                Name = string.Format("repliesListView{0}", threadId),
                Padding = new Thickness(10, 0, 10, 0),
                ItemsSource = cvs.View,
                IsItemClickEnabled = false,
                ItemTemplate = ReplyListItemTemplate,
                ItemContainerStyle = ReplyItemStyle,
                IncrementalLoadingTrigger = IncrementalLoadingTrigger.Edge,
                SelectionMode = ListViewSelectionMode.None
            };

            // 留点底边距
            listView.Footer = new StackPanel
            {
                Margin = new Thickness(0, 50, 0, 50)
            };

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

        private CollectionViewSource CreateDataViewForReplyListPage(string threadId)
        {
            var cvs = new CollectionViewSource();
            cvs.Source = new GeneratorIncrementalLoadingClass<Reply>(50, async pageNo =>
            {
                // 加载分页数据，并写入静态类中
                // 返回的是本次加载的数据量
                return await DataSource.GetLoadRepliesCountAsync(threadId, pageNo, () =>
                {
                    replyProgressBar.Visibility = Visibility.Visible;
                }, () =>
                {
                    replyProgressBar.Visibility = Visibility.Collapsed;
                });
            }, (index) =>
            {
                // 从静态类中返回需要显示出来的数据
                return DataSource.GetReplyByIndex(threadId, index);
            });
            return cvs;
        }
        #endregion

        

        #region 打开标签页菜单按钮
        private void openTabForApp_Click(object sender, RoutedEventArgs e)
        {
            CreateReplyListTab("1427253", "反馈", false);
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
            currentPostType = EnumPostType.Reply;
            currentEditType = EnumEditType.Add;
            ShowPostReplyPanelAndButton();

            postReplyContentTextBox.Focus(FocusState.Programmatic);
        }

        private void openPostNewPanelButton_Click(object sender, RoutedEventArgs e)
        {
            currentPostType = EnumPostType.NewThread;
            currentEditType = EnumEditType.Add;
            ShowPostNewPanelAndButton();

            postNewTitleTextBox.Focus(FocusState.Programmatic);
        }

        private void btnBack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (postNewPanel.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                NavigationHelper.IsCanGoBack = false;
                e.Handled = true;

                if (currentEditType == EnumEditType.Add)
                {
                    HidePostNewPanelAndButton();
                }
                else if (currentEditType == EnumEditType.Modify)
                {
                    HidePostNewModifyPanelAndButton();
                }
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
                if (Pivot.SelectedIndex > 0)
                {
                    NavigationHelper.IsCanGoBack = false;
                    e.Handled = true;
                    Pivot.SelectedIndex = 0;
                }
                else
                {
                    NavigationHelper.IsCanGoBack = true;
                }
            }

            // 清除针对回复的数据
            postReplyContentTextBox.Text = string.Empty;
            noticeauthor = noticetrimstr = noticeauthormsg = string.Empty;

            if (navigationHelper.IsCanGoBack)
            {
                e.Handled = true;
                if (navigationHelper.GoBackCommand.CanExecute(null))
                {
                    navigationHelper.GoBackCommand.Execute(null);
                }
            }
        }

        private void TitleBackButton_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (postNewPanel.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                NavigationHelper.IsCanGoBack = false;
                e.Handled = true;

                if (currentEditType == EnumEditType.Add)
                {
                    HidePostNewPanelAndButton();
                }
                else if (currentEditType == EnumEditType.Modify)
                {
                    HidePostNewModifyPanelAndButton();
                }
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
                if (Pivot.SelectedIndex > 0)
                {
                    NavigationHelper.IsCanGoBack = false;
                    e.Handled = true;
                    Pivot.SelectedIndex = 0;
                }
                else
                {
                    NavigationHelper.IsCanGoBack = true;
                }
            }

            // 清除针对回复的数据
            postReplyContentTextBox.Text = string.Empty;
            noticeauthor = noticetrimstr = noticeauthormsg = string.Empty;

            if (navigationHelper.IsCanGoBack)
            {
                e.Handled = true;
                if (navigationHelper.GoBackCommand.CanExecute(null))
                {
                    navigationHelper.GoBackCommand.Execute(null);
                }
            }
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (postNewPanel.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                NavigationHelper.IsCanGoBack = false;
                e.Handled = true;

                if (currentEditType == EnumEditType.Add)
                {
                    HidePostNewPanelAndButton();
                }
                else if (currentEditType == EnumEditType.Modify)
                {
                    HidePostNewModifyPanelAndButton();
                }
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
                if (Pivot.SelectedIndex > 0)
                {
                    NavigationHelper.IsCanGoBack = false;
                    e.Handled = true;
                    Pivot.SelectedIndex = 0;
                }
                else
                {
                    NavigationHelper.IsCanGoBack = true;
                }
            }

            // 清除针对回复的数据
            postReplyContentTextBox.Text = string.Empty;
            noticeauthor = noticetrimstr = noticeauthormsg = string.Empty;

            if (navigationHelper.IsCanGoBack)
            {
                e.Handled = true;
                if (navigationHelper.GoBackCommand.CanExecute(null))
                {
                    navigationHelper.GoBackCommand.Execute(null);
                }
            }
        }

        #region 显示或隐藏回复或修改回复之输入面板
        private void HidePostReplyPanelAndButton()
        {
            foreach (AppBarButton btn in tabPageCommandBar.SecondaryCommands)
            {
                btn.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            postReplyPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            sendButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            openPostReplyPanelButton.Visibility = 
                refreshRepliesButton.Visibility = 
                reverseListButton.Visibility = 
                Windows.UI.Xaml.Visibility.Visible;
        }

        private void ShowPostReplyPanelAndButton()
        {
            foreach (AppBarButton btn in tabPageCommandBar.SecondaryCommands)
            {
                btn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            postReplyPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            sendButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

            openPostReplyPanelButton.Visibility = 
                refreshRepliesButton.Visibility = 
                reverseListButton.Visibility = 
                Windows.UI.Xaml.Visibility.Collapsed;
        }
        #endregion

        #region 显示或隐藏发贴或修改发贴之输入面板
        private void HidePostNewPanelAndButton()
        {
            foreach (AppBarButton btn in tabPageCommandBar.SecondaryCommands)
            {
                btn.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            postNewPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            sendButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            openPostNewPanelButton.Visibility = 
                refreshThreadsButton.Visibility =
                sortForThreadListButton.Visibility =
                changeThemeButton.Visibility =
                openDialogForSearch.Visibility =
                openMyThreads.Visibility =
                Windows.UI.Xaml.Visibility.Visible;
        }

        private void ShowPostNewPanelAndButton()
        {
            foreach (AppBarButton btn in tabPageCommandBar.SecondaryCommands)
            {
                btn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            postNewPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            sendButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

            openPostNewPanelButton.Visibility = 
                refreshThreadsButton.Visibility =
                sortForThreadListButton.Visibility =
                changeThemeButton.Visibility =
                openDialogForSearch.Visibility =
                openMyThreads.Visibility =
                Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void HidePostNewModifyPanelAndButton()
        {
            foreach (AppBarButton btn in tabPageCommandBar.SecondaryCommands)
            {
                btn.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            postNewPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            sendButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            openPostReplyPanelButton.Visibility = 
                refreshRepliesButton.Visibility = 
                reverseListButton.Visibility = 
                Windows.UI.Xaml.Visibility.Visible;
        }

        private void ShowPostNewModifyPanelAndButton()
        {
            foreach (AppBarButton btn in tabPageCommandBar.SecondaryCommands)
            {
                btn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            postNewPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            sendButton.Visibility = Windows.UI.Xaml.Visibility.Visible;

            openPostReplyPanelButton.Visibility = 
                refreshRepliesButton.Visibility = 
                reverseListButton.Visibility = 
                Windows.UI.Xaml.Visibility.Collapsed;
        }
        #endregion

        private void replyReplyMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Reply data = (sender as MenuFlyoutItem).DataContext as Reply;
            string textContent = data.TextContent.Length > 100 ? data.TextContent.Substring(0, 97) + "..." : data.TextContent;
            textContent = textContent.Trim();

            noticeauthor = string.Format("r|{0}|[i]{1}[/i]", data.OwnerId, data.OwnerName);
            noticetrimstr = string.Format("[b]回复 {0}# [i]{1}[/i] [/b]\n\n", data.Floor, data.OwnerName);
            noticeauthormsg = textContent;

            postReplyContentTextBox.Text = noticetrimstr;

            currentPostType = EnumPostType.Reply;
            currentEditType = EnumEditType.Add;
            ShowPostReplyPanelAndButton();
        }

        private void refReplyMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Reply data = (sender as MenuFlyoutItem).DataContext as Reply;
            string textContent = data.TextContent.Length > 100 ? data.TextContent.Substring(0, 97) + "..." : data.TextContent;
            textContent = textContent.Trim();

            noticeauthor = string.Format("r|{0}|[i]{1}[/i]", data.OwnerId, data.OwnerName);
            noticetrimstr = string.Format("[quote]回复 {0}# {1}\n{2}[/quote]\n\n", data.Floor, data.OwnerName, textContent);
            noticeauthormsg = textContent;

            postReplyContentTextBox.Text = noticetrimstr;

            currentPostType = EnumPostType.Reply;
            currentEditType = EnumEditType.Add;
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

            // 移除任意连续30个的非文字字符
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

        private void addFaceButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string value = btn.Content.ToString();

            string imageStr = string.Empty;
            if (value.StartsWith(":") || value.EndsWith("[/img]"))
            {
                imageStr = string.Format(" {0} ", value);
            }
            else
            {
                imageStr = string.Format(@" {{:{0}:}} ", value);
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
                postReplyContentTextBox.Text = postReplyContentTextBox.Text.Insert(cursorPosition, imageStr);
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
                postNewContentTextBox.Text = postNewContentTextBox.Text.Insert(cursorPosition, imageStr);
            }
        }

        private void replyListItemGrid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

        private async void addPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            //string deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add("*");

            var files = await openPicker.PickMultipleFilesAsync();
            if (files != null)
            {
                sendButton.IsEnabled = false;

                int fileIndex = 1;
                foreach (var file in files)
                {
                    string fileName = file.Name;
                    uploadStateInfo.Text = string.Format("请稍候，正在为您上传第 {0}/{1} 个文件（{2}）。", fileIndex, files.Count, fileName);
                    uploadStateInfo2.Text = string.Format("请稍候，正在为您上传第 {0}/{1} 个文件（{2}）。", fileIndex, files.Count, fileName);
                    imageNameList.Add(fileName);
                    byte[] imageBuffer;

                    //if (deviceFamily.Equals("Windows.Mobile"))
                    //{
                    //    imageBuffer = await ImageHelper.LoadAsync(file);
                    //}
                    //else
                    //{
                    IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                    IBuffer buffer = new Windows.Storage.Streams.Buffer((uint)stream.Size);
                    buffer = await stream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);
                    imageBuffer = WindowsRuntimeBufferExtensions.ToArray(buffer, 0, (int)buffer.Length);
                    //}

                    var data = new Dictionary<string, object>();
                    data.Add("uid", DataSource.UserId);
                    data.Add("hash", DataSource.Hash);

                    var cts = new CancellationTokenSource();
                    string result = await httpClient.PostFileAsync("http://www.hi-pda.com/forum/misc.php?action=swfupload&operation=upload&simple=1&type=image", data, fileName, "image/jpg", "Filedata", imageBuffer, cts);
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

                        fileIndex++;
                    }
                }

                sendButton.IsEnabled = true;
                uploadStateInfo.Text = string.Format("文件上传已完成，共上传 {0} 个文件。", files.Count);
                uploadStateInfo2.Text = string.Format("文件上传已完成，共上传 {0} 个文件。", files.Count);
            }
        }

        private static int GeneratePostTime()
        {
            var startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var diff = DateTime.Now - startTime;
            return (int)Math.Floor(diff.TotalSeconds);
        }

        private async void modifyMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Reply data = (sender as MenuFlyoutItem).DataContext as Reply;

            // 请求贴子标题及内容原数据
            await DataSource.GetContentForEdit(data.ThreadId, data.PostId);

            currentEditType = EnumEditType.Modify;

            if (data.Floor == 1)
            {
                currentPostType = EnumPostType.NewThread;

                var contentForEdit = DataSource.ContentForEdit;
                if (contentForEdit != null)
                {
                    // 设置数据到输入框
                    postNewTitleTextBox.Text = contentForEdit.Subject;
                    postNewContentTextBox.Text = contentForEdit.Content;

                    ShowPostNewModifyPanelAndButton();
                }
            }
            else
            {
                currentPostType = EnumPostType.Reply;

                var contentForEdit = DataSource.ContentForEdit;
                if (contentForEdit != null)
                {
                    // 设置数据到输入框
                    postReplyContentTextBox.Text = contentForEdit.Content;

                    ShowPostReplyPanelAndButton();
                }
            }
        }

        private async void sortForThreadListButton_Click(object sender, RoutedEventArgs e)
        {
            SortForThreadSettings.Toggle();

            // 刷新当前主题列表
            PivotItem pivotItem = (PivotItem)Pivot.SelectedItem;
            string forumId = pivotItem.GetValue(PivotItemTabIdProperty).ToString();
            ListView listView = (ListView)pivotItem.FindName("threadsListView" + forumId);
            await RefreshThreadListPage(listView, forumId);
        }

        private async void reverseListButton_Click(object sender, RoutedEventArgs e)
        {
            SortForReplySettings.Toggle();

            // 刷新当前贴子回复列表
            PivotItem pivotItem = (PivotItem)Pivot.SelectedItem;
            string threadId = pivotItem.GetValue(PivotItemTabIdProperty).ToString();
            ListView listView = (ListView)pivotItem.FindName("repliesListView" + threadId);
            await RefreshReplyListPage(listView, threadId);
        }

        private void changeThemeButton_Click(object sender, RoutedEventArgs e)
        {
            PivotItem item = (PivotItem)Pivot.SelectedItem;
            ThreadData data = (ThreadData)item.DataContext;
            string tabType = item.GetValue(PivotItemTabTypeProperty).ToString();
            if (tabType.Equals("1"))
            {
                string tabId = data.ForumId;
                ListView listView = (ListView)item.FindName("threadsListView" + tabId);
                //var childs = VisualTreeHelper.GetChildrenCount(listView);
                //await new MessageDialog(childs.ToString()).ShowAsync(); 

                App.ThemeId++;
                if (App.ThemeId > 3)
                {
                    App.ThemeId = 0;
                }
                ThemeSettings.ThemeSetting = App.ThemeId;

                switch (App.ThemeId)
                {
                    case 0:
                        ThemeClassic();
                        break;
                    case 1:
                        ThemeDark();
                        break;
                    case 2:
                        ThemeBlueSky();
                        break;
                    case 3:
                        ThemeStarSky();
                        break;
                }
            }
        }

        private async void refreshRepliesButton_Click(object sender, RoutedEventArgs e)
        {
            PivotItem pivotItem = (PivotItem)Pivot.SelectedItem;
            string themeId = pivotItem.GetValue(PivotItemTabIdProperty).ToString();
            ListView listView = (ListView)pivotItem.FindName("repliesListView" + themeId);

            await RefreshRepliesListPage(themeId, listView);
        }

        /// <summary>
        /// 此方法包含两种刷新方式
        /// 一种是倒序清空刷新，即先清空当前贴子所有回复，再全部重新加载
        /// 一种是顺序增量刷新，即先清空最后一页的所有回复数据，再重新加载最后一页的数据
        /// </summary>
        /// <param name="themeId"></param>
        /// <param name="listView"></param>
        /// <returns></returns>
        private async Task RefreshRepliesListPage(string themeId, ListView listView)
        {
            replyProgressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            refreshRepliesButton.IsEnabled = false;

            #region 只在倒序看贴时允许全局刷新
            if (SortForReplySettings.GetSortType == 1)
            {
                // 刷新当前贴子回复列表
                await RefreshReplyListPage(listView, themeId);
            }
            #endregion
            #region 顺序看贴，只刷新最后一页
            else
            {
                ICollectionView view = (ICollectionView)listView.ItemsSource;
                await view.LoadMoreItemsAsync(1); // count = 1 表示是要刷新
                listView.ScrollIntoView(view.Last());
            }
            #endregion
            
            refreshRepliesButton.IsEnabled = true;
            replyProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async Task SearchThreadListPage(string keywords, int searchType)
        {
            openDialogForSearch.Flyout.Hide();

            // 在当前版区内进行搜索
            PivotItem pivotItem = (PivotItem)Pivot.SelectedItem;
            string forumId = pivotItem.GetValue(PivotItemTabIdProperty).ToString();
            ListView listView = (ListView)pivotItem.FindName("threadsListView" + forumId);
            await SearchThreadListPage(keywords, searchType, listView, forumId);
        }

        private async void btnSearchTitle_Click(object sender, RoutedEventArgs e)
        {
            string keywords = txtKeyword.Text.Trim();
            await SearchThreadListPage(keywords, 1);
        }

        private async void btnSearchAuthor_Click(object sender, RoutedEventArgs e)
        {
            string keywords = txtKeyword.Text.Trim();
            await SearchThreadListPage(keywords, 2);
        }

        private async void txtKeyword_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string keywords = txtKeyword.Text.Trim();
                await SearchThreadListPage(keywords, 1);
            }

        }

        private async void openMyThreads_Click(object sender, RoutedEventArgs e)
        {
            if (!accountName.Equals("未登录"))
            {
                var acc = AccountSettings.GetDefault();
                string accUsername = acc.Username;
                await SearchThreadListPage(accUsername, 2);
            }
            else
            {
                await new MessageDialog("请先登录您的账号！", "提示").ShowAsync();
            }
        }

        private async void menuItemViewHistory_Click(object sender, RoutedEventArgs e)
        {
            Thread datacontext = (e.OriginalSource as FrameworkElement).DataContext as Thread;
            await SearchThreadListPage(datacontext.OwnerName, 2);
        }

        private void avatarImageBorder_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            // If you need the clicked element:
            // Item whichOne = senderElement.DataContext as Item;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void avatarImageBorder_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            // If you need the clicked element:
            // Item whichOne = senderElement.DataContext as Item;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void btnBack_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Border b = sender as Border;
            b.Opacity = 1;
        }

        private void btnBack_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Border b = sender as Border;
            b.Opacity = 0.6;
        }
    }
}
