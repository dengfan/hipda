using Hipda.Client.Uwp.Pro.Controls;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.ViewModels;
using Hipda.Client.Uwp.Pro.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace Hipda.Client.Uwp.Pro
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static LocalSettingsDependencyObject _myLocalSettings = (LocalSettingsDependencyObject)App.Current.Resources["MyLocalSettings"];
        static RoamingSettingsDependencyObject _myRoamingSettings = (RoamingSettingsDependencyObject)App.Current.Resources["MyRoamingSettings"];

        public ulong ImageCacheDataSize
        {
            get { return (ulong)GetValue(ImageCacheDataSizeProperty); }
            set { SetValue(ImageCacheDataSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageCacheDataSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageCacheDataSizeProperty =
            DependencyProperty.Register("ImageCacheDataSize", typeof(ulong), typeof(MainPage), new PropertyMetadata(0UL));

        MainPageViewModel _mainPageViewModel;

        public Frame AppFrame { get { return this.MainFrame; } }

        void ElementAdapter()
        {
            PromptAllBorder.Visibility = ActualWidth >= 1000 ? Visibility.Collapsed : Visibility.Visible;
            BottomButtonPanel.Orientation = MainSplitView.IsPaneOpen ? Orientation.Horizontal : Orientation.Vertical;
        }

        public MainPage()
        {
            this.InitializeComponent();

            if (_myLocalSettings.ThemeType == 0)
            {
                this.RequestedTheme = ElementTheme.Light;
            }
            else if (_myLocalSettings.ThemeType == 1)
            {
                this.RequestedTheme = ElementTheme.Dark;
            }

            _mainPageViewModel = MainPageViewModel.GetInstance();
            DataContext = _mainPageViewModel;

            this.SizeChanged += (s, e) =>
            {
                ElementAdapter();
            };

            this.Loaded += (s, e) =>
            {
                ElementAdapter();
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string param = e.Parameter.ToString();
            AppFrame.Navigate(typeof(ThreadAndReplyPage), param);

            _mainPageViewModel.SelectedNavButton = _mainPageViewModel.NavButtons.FirstOrDefault(b => b.TypeValue.Equals(param));
        }

        public async void ShowTipBar(string tipContent)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                TipTextBlock.Text = Uri.UnescapeDataString(tipContent);
                ShowTipBarAnimation.Begin();
            });
        }

        private void MainSplitViewToggle_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
            ElementAdapter();
        }

        private void MainSplitView_PaneClosed(SplitView sender, object args)
        {
            SearchPanel.Visibility = Visibility.Collapsed;
            ElementAdapter();
        }

        void ShowLeftSwipePanel()
        {
            FindName("MaskGrid");
            FindName("LeftSwipePanel");
            FindName("RightSwipePanel");

            if (MainSplitView.DisplayMode == SplitViewDisplayMode.Overlay || MainSplitView.DisplayMode == SplitViewDisplayMode.CompactOverlay)
            {
                // 以免 pane 挡住 left swipe panel
                MainSplitView.IsPaneOpen = false;
                ElementAdapter();
            }
            
            MaskGrid.Visibility = Visibility.Visible;
            OpenMaskAnimation.Begin();
            OpenLeftSwipePanelAnimation.Begin();
            CloseRightSwipePanelAnimation.Begin();
        }

        void HideLeftSwipePanel()
        {
            CloseMaskAnimation.Begin();
            CloseLeftSwipePanelAnimation.Begin();
            MaskGrid.Visibility = Visibility.Collapsed;

            var selectedItem = (NavButtonItemModel)TopNavButtonListBox.SelectedItem;
            if (selectedItem != null && selectedItem.TypeValue.Equals("more"))
            {
                TopNavButtonListBox.SelectedItem = null;
            }
        }

        void ShowRightSwipePanel()
        {
            FindName("MaskGrid");
            FindName("LeftSwipePanel");
            FindName("RightSwipePanel");

            if (MainSplitView.DisplayMode == SplitViewDisplayMode.Overlay || MainSplitView.DisplayMode == SplitViewDisplayMode.CompactOverlay)
            {
                // 以免 pane 挡住 left swipe panel
                MainSplitView.IsPaneOpen = false;
                ElementAdapter();
            }

            MaskGrid.Visibility = Visibility.Visible;

            OpenMaskAnimation.Begin();
            OpenRightSwipePanelAnimation.Begin();

            CloseLeftSwipePanelAnimation.Begin();
            TopNavButtonListBox.SelectedItem = null;
        }

        void HideRightSwipePanel()
        {
            CloseMaskAnimation.Begin();

            if (AdaptiveStates.CurrentState == Min1200 || AdaptiveStates.CurrentState == Min1400 || AdaptiveStates.CurrentState == Min1600)
            {
                // 宽视图（已显示历史记录）下，直接隐藏，无过渡
                CloseRightSwipePanelAnimation2.Begin();
            }
            else
            {
                CloseRightSwipePanelAnimation.Begin();
            }
            

            MaskGrid.Visibility = Visibility.Collapsed;

            // 清除值，以便每次打开时都重新计算
            ImageCacheDataSize = 0;

            // 保存本地设置
            new LocalSettingsService().Save();

            // 保存漫游设置
            new RoamingSettingsService().Save();
        }

        string _prevAccessForumIdStr = "2";
        private void TopNavButtonListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
            {
                return;
            }

            var data = e.AddedItems[0] as NavButtonItemModel;
            if (data.TypeValue.Equals("more"))
            {
                ShowLeftSwipePanel();
            }
            else
            {
                if (data.TypeValue.StartsWith("fid="))
                {
                    _prevAccessForumIdStr = data.TypeValue.Substring("fid=".Length);
                }
                AppFrame.Navigate(typeof(ThreadAndReplyPage), data.TypeValue);
            }
        }

        private void TopNavButtonListBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 如果事件源是来自“更多版块”按钮，则阻止事件继续向上冒泡并停止继续执行
            var item = Common.FindParent<ListBoxItem>(e.OriginalSource as FrameworkElement);
            if (item != null)
            {
                var data = item.DataContext as NavButtonItemModel;
                if (data.TypeValue.Equals("more"))
                {
                    e.Handled = true;
                    return;
                }
            }

            HideLeftSwipePanel();
            HideRightSwipePanel();
        }

        private void MaskGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            HideLeftSwipePanel();
            HideRightSwipePanel();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchPanel.Visibility == Visibility.Collapsed)
            {
                MainSplitView.IsPaneOpen = true;
                ElementAdapter();

                SearchPanel.Visibility = Visibility.Visible;
                KeywordTextBox.Focus(FocusState.Programmatic);
            }
            else
            {
                SearchPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void KeywordTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SearchDefaultSubmit();
            }
        }

        private void ShowHistoryRecordButton_Click(object sender, RoutedEventArgs e)
        {
            FindName("RightSwipePanel");
            RightSwipeContentControl.ContentTemplate = Resources["HistoryRecordContentControl"] as DataTemplate;

            ShowRightSwipePanel();
        }

        private async void ShowSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            FindName("RightSwipePanel");
            RightSwipeContentControl.ContentTemplate = Resources["SettingsContentControl"] as DataTemplate;

            ShowRightSwipePanel();

            // 更新黑名单
            new RoamingSettingsService().ReadAndUpdate();

            // 统计缓存文件大小
            var folder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("hipda", CreationCollisionOption.OpenIfExists);
            await GetDataSizeInFolder(folder);
        }

        private void SearchDefaultSubmit()
        {
            string searchKeyword = Uri.EscapeUriString(KeywordTextBox.Text.Trim().Replace(",", " "));
            string searchAuthor = Uri.EscapeUriString(AuthorTextBox.Text.Trim().Replace(",", " "));
            int searchType = SearchTypeComboBox.SelectedIndex;
            int searchTimeSpan = SearchTimeSpanComboBox.SelectedIndex;

            string param = $"search={searchKeyword},{searchAuthor},{searchType},{searchTimeSpan},-1";
            AppFrame.Navigate(typeof(ThreadAndReplyPage), param);

            TopNavButtonListBox.SelectedItem = null;
        }

        private void SearchButton1_Click(object sender, RoutedEventArgs e)
        {
            SearchDefaultSubmit();
        }

        private void SearchButton2_Click(object sender, RoutedEventArgs e)
        {
            string searchKeyword = Uri.EscapeUriString(KeywordTextBox.Text.Trim().Replace(",", " "));
            string searchAuthor = Uri.EscapeUriString(AuthorTextBox.Text.Trim().Replace(",", " "));
            int searchType = SearchTypeComboBox.SelectedIndex;
            int searchTimeSpan = SearchTimeSpanComboBox.SelectedIndex;

            string param = $"search={searchKeyword},{searchAuthor},{searchType},{searchTimeSpan},{_prevAccessForumIdStr}";
            AppFrame.Navigate(typeof(ThreadAndReplyPage), param);

            TopNavButtonListBox.SelectedItem = null;
        }

        private void NoticeButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "item=notice");
        }

        void SendToast(string toastXml)
        {
            toastXml = Common.ReplaceHexadecimalSymbols(toastXml);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(toastXml);

            // 创建通知实例
            var notification = new ToastNotification(xmlDoc);

            // 显示通知
            var tn = ToastNotificationManager.CreateToastNotifier();
            tn.Show(notification);
        }

        #region 头像上下文菜单
        //private async void openThreadInNewView_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    var uri = new Uri("hipda:tid=" + MainPage.PopupThreadId);
        //    await Launcher.LaunchUriAsync(uri, new LauncherOptions { TreatAsUntrusted = false });
        //}

        private void OpenUserInfoDialogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenUserInfoDialog();
        }

        private void OpenUserMessageDialogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenUserMessageDialog();
        }

        private void BlockUserMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PopupUserId > 0 && !string.IsNullOrEmpty(PopupUsername))
            {
                if (PopupUserId == AccountService.UserId)
                {
                    string xml1 = "<toast>" +
                        "<visual>" +
                            "<binding template='ToastGeneric'>" +
                                "<text>对不起，您不能将自己列入屏蔽名单</text>" +
                            "</binding>" +
                        "</visual>" +
                        "</toast>";
                    SendToast(xml1);
                    return;
                }

                string xml2 = "<toast>" +
                    "<visual>" +
                        "<binding template='ToastGeneric'>" +
                            $"<text>恭喜您，已将 {PopupUsername} 加入黑名单</text>" +
                            $"<text>刷新后生效</text>" +
                        "</binding>" +
                    "</visual>" +
                    "</toast>";

                if (!_myRoamingSettings.BlockUsers.Any(u => u.UserId == PopupUserId && u.ForumId == PopupForumId))
                {
                    _myRoamingSettings.BlockUsers.Add(new BlockUser { UserId = PopupUserId, Username = PopupUsername, ForumId = PopupForumId, ForumName = PopupForumName });
                    new RoamingSettingsService().Save();
                }

                SendToast(xml2);
            }
        }

        private void BlockThreadMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (PopupUserId > 0 && PopupThreadId > 0 && !string.IsNullOrEmpty(PopupThreadTitle))
            {
                if (PopupUserId == AccountService.UserId)
                {
                    string xml1 = "<toast>" +
                        "<visual>" +
                            "<binding template='ToastGeneric'>" +
                                "<text>对不起，您不能将自己的主题列入屏蔽列表</text>" +
                            "</binding>" +
                        "</visual>" +
                        "</toast>";
                    SendToast(xml1);
                    return;
                }

                if (!_myRoamingSettings.BlockThreads.Any(t => t.ThreadId == PopupThreadId))
                {
                    _myRoamingSettings.BlockThreads.Add(new BlockThread { UserId = PopupUserId, Username = PopupUsername, ThreadId = PopupThreadId, ThreadTitle = PopupThreadTitle, ForumId = PopupForumId, ForumName = PopupForumName });
                    new RoamingSettingsService().Save();
                }

                string xml2 = "<toast>" +
                    "<visual>" +
                        "<binding template='ToastGeneric'>" +
                            $"<text>恭喜您，已将《{PopupThreadTitle}》加入黑名单</text>" +
                            $"<text>刷新后生效</text>" +
                        "</binding>" +
                    "</visual>" +
                    "</toast>";

                SendToast(xml2);
            }
        }
        #endregion

        #region 坛友资料及短消息之弹窗
        public static int PopupUserId { get; set; }
        public static string PopupUsername { get; set; }
        public static int PopupThreadId { get; set; }
        public static string PopupThreadTitle { get; set; }
        public static int PopupForumId { get; set; }
        public static string PopupForumName { get; set; }

        bool _isDialogShown = false;

        public async void OpenUserInfoDialog()
        {
            if (PopupUserId == 0)
            {
                return;
            }

            FindName("UserDialog");
            UserDialog.DataContext = new ContentDialogForUserInfoViewModel(PopupUserId);
            UserDialog.Title = string.Format("查看 {0} 的详细资料", PopupUsername);
            UserDialog.ContentTemplate = this.Resources["UserInfoDialogContentTemplate"] as DataTemplate;

            if (_isDialogShown == false)
            {
                _isDialogShown = true;
                await UserDialog.ShowAsync();
            }
        }

        public async void OpenUserMessageDialog()
        {
            if (PopupUserId == 0)
            {
                return;
            }

            FindName("UserDialog");
            UserDialog.DataContext = new ContentDialogForUserMessageViewModel(PopupUserId);
            UserDialog.Title = string.Format("与 {0} 聊天", PopupUsername);
            UserDialog.ContentTemplate = this.Resources["UserMessageDialogContentTemplate"] as DataTemplate;

            if (_isDialogShown == false)
            {
                _isDialogShown = true;
                await UserDialog.ShowAsync();
            }
        }

        private async void UserMessageBox_Submit(object sender, EventArgs e)
        {
            UserMessageBox umb = sender as UserMessageBox;
            TextBox tb = umb.FindName("UserMessageTextBox") as TextBox;
            var msg = tb.Text.Trim();
            var vm = umb.DataContext as ContentDialogForUserMessageViewModel;
            bool isOk = await vm.PostUserMessage(msg, umb.UserId);
            tb.Text = string.Empty;

            // 发送完成后跳到列表底部
            var listView = Common.FindParent<Grid>(umb).Children[0] as ListView;
            if (listView.Items.Count > 0)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    listView.ScrollIntoView(listView.Items.Last());
                });
            }
        }

        public async void OpenPostDetailDialog(int postId, int threadId)
        {
            FindName("UserDialog");

            var vm = new ContentDialogForPostDetailViewModel(postId, threadId);
            if (vm == null)
            {
                return;
            }

            UserDialog.DataContext = vm;
            UserDialog.Title = "查看引用楼";
            UserDialog.ContentTemplate = this.Resources["PostDetailDialogContentTemplate"] as DataTemplate;

            if (_isDialogShown == false)
            {
                _isDialogShown = true;
                await UserDialog.ShowAsync();
            }
        }

        public async void OpenUserMessageListDialog()
        {
            FindName("UserDialog");

            var vm = new ContentDialogForUserMessageHubViewModel();
            if (vm == null)
            {
                return;
            }

            UserDialog.DataContext = vm;
            UserDialog.Title = "短消息";
            UserDialog.ContentTemplate = this.Resources["UserMessageListDialogContentTemplate"] as DataTemplate;

            if (_isDialogShown == false)
            {
                _isDialogShown = true;
                await UserDialog.ShowAsync();
            }
        }

        private void openUserInfoDialogButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUserInfoDialog();
        }

        private void openUserMessageListButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUserMessageListDialog();
        }

        private void openUserMessageDialogButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUserMessageDialog();
        }

        private void userMessageListListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = (MyListViewForUserMessageList)sender;
            listView.SelectedUserMessageListItems = listView.SelectedItems;

            if (listView.SelectionMode == ListViewSelectionMode.Single && e.AddedItems.Count == 1)
            {
                var data = e.AddedItems[0] as UserMessageListItemModel;
                if (data == null)
                {
                    return;
                }

                PopupUserId = data.UserId;
                PopupUsername = data.Username;
                OpenUserMessageDialog();
            }
        }

        public void CloseUserDialog()
        {
            if (UserDialog != null)
            {
                _isDialogShown = false;
                UserDialog.Hide();
                UserDialog.DataContext = null;
            }

        }

        private void userDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            CloseUserDialog();
            ElementAdapter();
        }

        private void UserDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            CloseUserDialog();
        }
        #endregion

        #region 设置面板相关程序
        private async void OpenImageFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("hipda", CreationCollisionOption.OpenIfExists);
            if (folder != null)
            {
                await Launcher.LaunchFolderAsync(folder);
            }
            else
            {
                await new MessageDialog("暂无缓存文件！", "温馨提示").ShowAsync();
            }
        }

        private async Task GetDataSizeInFolder(StorageFolder folder)
        {
            var files = await folder.GetFilesAsync();
            foreach (var file in files)
            {
                var bp = await file.GetBasicPropertiesAsync();
                ImageCacheDataSize += bp.Size;
            }

            var folders = await folder.GetFoldersAsync();
            foreach (var f in folders)
            {
                await GetDataSizeInFolder(f);
            }
        }

        private async void ClearImageCacheData(object sender, RoutedEventArgs e)
        {
            var md = new MessageDialog("您确定要清除图片缓存数据吗？", "温馨提示");
            md.Commands.Add(new UICommand { Id = 0, Label = "确定", Invoked = new UICommandInvokedHandler(CommandInvokedHandler) });
            md.Commands.Add(new UICommand { Id = 1, Label = "取消" });
            md.DefaultCommandIndex = 0;
            md.CancelCommandIndex = 1;
            await md.ShowAsync();
        }

        private async void CommandInvokedHandler(IUICommand command)
        {
            var folder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("hipda", CreationCollisionOption.OpenIfExists);
            await folder.DeleteAsync();
            ImageCacheDataSize = 0;
            await GetDataSizeInFolder(folder);
        }

        static List<string> _unblockUserKeys = new List<string>();

        private void BlockUsersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = new List<string>();
            var lb = (ListBox)sender;
            if (lb != null)
            {
                foreach (BlockUser item in lb.SelectedItems)
                {
                    list.Add($"{item.UserId}@{item.ForumId}");
                }
            }

            _unblockUserKeys = list;
        }

        private void UnblockUsers(object sender, RoutedEventArgs e)
        {
            new RoamingSettingsService().UnblockUsers(_unblockUserKeys);
            _unblockUserKeys.Clear();
        }

        static List<string> _unblockThreadKeys = new List<string>();

        private void BlockThreadsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = new List<string>();
            var lb = (ListBox)sender;
            if (lb != null)
            {
                foreach (BlockThread item in lb.SelectedItems)
                {
                    list.Add($"{item.ThreadId}");
                }
            }

            _unblockThreadKeys = list;
        }

        private void UnblockThreads(object sender, RoutedEventArgs e)
        {
            new RoamingSettingsService().UnblockThreads(_unblockThreadKeys);
            _unblockThreadKeys.Clear();
        }
        #endregion

        private void ThreadHistoryListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var lb = (ListBox)sender;
            lb.Items.VectorChanged += (s, args) =>
            {
                lb.SelectedItem = null;
                if (lb.Items.Count > 0)
                {
                    lb.ScrollIntoView(lb.Items.Last());
                }
            };
        }
    }
}
