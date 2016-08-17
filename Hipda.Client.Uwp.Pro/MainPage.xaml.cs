using Hipda.Client.Uwp.Pro.Controls;
using Hipda.Client.Uwp.Pro.Converters;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.ViewModels;
using Hipda.Client.Uwp.Pro.Views;
using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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


        public int ForumId
        {
            get { return (int)GetValue(ForumIdProperty); }
            set { SetValue(ForumIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForumId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForumIdProperty =
            DependencyProperty.Register("ForumId", typeof(int), typeof(MainPage), new PropertyMetadata(0));


        public ulong ImageCacheDataSize
        {
            get { return (ulong)GetValue(ImageCacheDataSizeProperty); }
            set { SetValue(ImageCacheDataSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageCacheDataSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageCacheDataSizeProperty =
            DependencyProperty.Register("ImageCacheDataSize", typeof(ulong), typeof(MainPage), new PropertyMetadata(0UL));


        public string RemoveAdButtonContent
        {
            get { return (string)GetValue(RemoveAdButtonContentProperty); }
            set { SetValue(RemoveAdButtonContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RemoveAdButtonContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RemoveAdButtonContentProperty =
            DependencyProperty.Register("RemoveAdButtonContent", typeof(string), typeof(MainPage), new PropertyMetadata("--"));


        #region 发贴计时器


        public int Countdown
        {
            get { return (int)GetValue(CountdownProperty); }
            set { SetValue(CountdownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Countdown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountdownProperty =
            DependencyProperty.Register("Countdown", typeof(int), typeof(MainPage), new PropertyMetadata(0));


        public DispatcherTimer SendMessageTimer;

        public void SendMessageTimerSetup()
        {
            Countdown = 30;
            SendMessageTimer = new DispatcherTimer();
            SendMessageTimer.Tick += dispatcherTimer_Tick;
            SendMessageTimer.Interval = new TimeSpan(0, 0, 1);
            SendMessageTimer.Start();
        }

        void dispatcherTimer_Tick(object sender, object e)
        {
            if (Countdown == 0)
            {
                SendMessageTimer.Stop();
                return;
            }

            Countdown--;
        }
        #endregion

        MainPageViewModel _mainPageViewModel;

        #region 打开下级页面必须且只能使用以下三个方法
        public void OpenThreadByForumId()
        {
            AppFrame.Navigate(typeof(ThreadAndReplyPage), $"fid={ForumId}");
        }

        public void OpenThreadForItemType(string args)
        {
            AppFrame.Navigate(typeof(ThreadAndReplyPage), $"item={args}");
        }

        public void OpenThreadForSearch(string args)
        {
            AppFrame.Navigate(typeof(ThreadAndReplyPage), $"search={args}");
        }
        #endregion

        public Frame AppFrame { get { return this.MainFrame; } }

        public Action<List<string>[]> UploadInkContentDelegate;
        public Action<List<string>[]> PostInkContentDelegate;

        public ImageBrush UserAvatarImageBrush
        {
            get
            {
                var avatarBrush = new ImageBrush();
                avatarBrush.Stretch = Stretch.UniformToFill;
                avatarBrush.ImageSource = new BitmapImage { UriSource = CommonService.GetSmallAvatarUriByUserId(AccountService.UserId), DecodePixelWidth = 32 };
                avatarBrush.ImageFailed += (s, e) => { return; };
                return avatarBrush;
            }
        }

        public string FuzzyUsername
        {
            get
            {
                string username = AccountService.Username;
                int len = username.Length;
                if (len < 3)
                {
                    return $"{username.Substring(0, 1)}*";
                }
                else
                {
                    return $"{username.Substring(0, 1)}***{username.Substring(len - 1)}";
                }
            }
        }

        async void GetRemoveAdButtonContent()
        {
            try
            {
                // 读取内购信息
                var listing = await CurrentApp.LoadListingInformationAsync();
                var iap = listing.ProductListings.FirstOrDefault(p => p.Value.ProductId == "RemoveAd");
                RemoveAdButtonContent = $"【{iap.Value.Name} {iap.Value.FormattedPrice}】";
            }
            catch (Exception)
            {
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            GetRemoveAdButtonContent();

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
                if (CurrentApp.LicenseInformation.ProductLicenses["RemoveAd"].IsActive == false)
                {
                    FindName("AdWrap");

                    if (e.NewSize.Width >= 900)
                    {
                        AdWrap.Visibility = Visibility.Visible;
                        MainSplitView.Margin = new Thickness(0, 0, 160, 0);
                    }
                    else
                    {
                        AdWrap.Visibility = Visibility.Collapsed;
                        MainSplitView.Margin = new Thickness(0);
                    }
                }
            };

            MyInkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Pen;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string param = e.Parameter.ToString();
            if (param.StartsWith("fid="))
            {
                ForumId = Convert.ToInt32(param.Substring("fid=".Length));
                OpenThreadByForumId();
            }

            _mainPageViewModel.SelectedNavButton = _mainPageViewModel.NavButtons.FirstOrDefault(b => b.TypeValue.Equals(param));
        }

        

        private void MainSplitViewToggle_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        private void MainSplitView_PaneClosed(SplitView sender, object args)
        {
            SearchPanel.Visibility = Visibility.Collapsed;
        }

        

        

        private void TopNavButtonListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
            {
                return;
            }

            var data = e.AddedItems[0] as NavButtonItemModel;
            if (data.TypeValue.Equals("more"))
            {
                //ShowLeftSwipePanel();

                //LeftSwipeContentControl.ContentTemplate = (DataTemplate)Resources["ForumAllCategoryListViewDataTemplate"];
            }
            else
            {
                if (data.TypeValue.StartsWith("fid="))
                {
                    ForumId = Convert.ToInt32(data.TypeValue.Substring("fid=".Length));
                    OpenThreadByForumId();
                }
                else if (data.TypeValue.StartsWith("item="))
                {
                    OpenThreadForItemType(data.TypeValue.Replace("item=", string.Empty));
                }
                else if (data.TypeValue.StartsWith("search="))
                {
                    OpenThreadForSearch(data.TypeValue.Replace("search=", string.Empty));
                }
            }
        }

        private void TopNavButtonListBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // 如果事件源是来自“更多版块”按钮，则阻止事件继续向上冒泡并停止继续执行
            var item = CommonService.FindParent<ListBoxItem>(e.OriginalSource as FrameworkElement);
            if (item != null)
            {
                var data = item.DataContext as NavButtonItemModel;
                if (data.TypeValue.Equals("more"))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchPanel.Visibility == Visibility.Collapsed)
            {
                MainSplitView.IsPaneOpen = true;

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

        int _historyItemMaxIndex = 0;
        private void ThreadHistoryListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue == false && args.ItemIndex > _historyItemMaxIndex)
            {
                sender.SelectedItem = null;
                if (sender.Items.Count > 0)
                {
                    sender.ScrollIntoView(sender.Items.Last());
                    _historyItemMaxIndex = args.ItemIndex;
                }
            }
        }

        private void ThreadHistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = (ListView)sender;
            var selectedItem = (ThreadItemModelBase)lb.SelectedItem;
            if (selectedItem != null)
            {
                if (AppFrame.Content.GetType().Equals(typeof(ThreadAndReplyPage)))
                {
                    var page = (ThreadAndReplyPage)AppFrame.Content;
                    if (page != null)
                    {
                        page.ThreadId = selectedItem.ThreadId;
                        page.OpenReplyPageByThreadId();
                        ApplicationView.GetForCurrentView().Title = $"{selectedItem.Title} - {selectedItem.ForumName}";
                    }
                }
                else if (AppFrame.Content.GetType().Equals(typeof(ReplyListPage)))
                {
                    var page = (ReplyListPage)AppFrame.Content;
                    if (page != null)
                    {
                        page.NavigationCacheMode = NavigationCacheMode.Disabled;
                        page.ThreadId = selectedItem.ThreadId;
                        page.OpenReplyPageByThreadId();
                        ApplicationView.GetForCurrentView().Title = $"{selectedItem.Title} - {selectedItem.ForumName}";
                    }
                }
            }
        }

        private void ForumAllCategoryListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (ForumModel)e.ClickedItem;
            ForumId = item.Id;
            OpenThreadByForumId();
        }

        private void ShowHistoryRecordButton_Click(object sender, RoutedEventArgs e)
        {
            RightPivot.SelectedIndex = 0;
            if (SubSplitView.IsPaneOpen == false)
            {
                SubSplitView.IsPaneOpen = true;
            }
        }

        private async void ShowSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            RightPivot.SelectedIndex = 1;
            if (SubSplitView.IsPaneOpen == false)
            {
                SubSplitView.IsPaneOpen = true;
            }

            // 更新黑名单
            new RoamingSettingsService().ReadAndUpdate();

            // 统计缓存文件大小
            if (ImageCacheDataSize == 0)
            {
                var folder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("hipda", CreationCollisionOption.OpenIfExists);
                await GetDataSizeInFolder(folder);
            }
        }

        private void SearchDefaultSubmit()
        {
            string searchKeyword = Uri.EscapeUriString(KeywordTextBox.Text.Trim().Replace(",", " "));
            string searchAuthor = Uri.EscapeUriString(AuthorTextBox.Text.Trim().Replace(",", " "));
            int searchType = SearchTypeComboBox.SelectedIndex;
            int searchTimeSpan = SearchTimeSpanComboBox.SelectedIndex;

            string args = $"{searchKeyword},{searchAuthor},{searchType},{searchTimeSpan},-1";
            OpenThreadForSearch(args);

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

            string args = $"{searchKeyword},{searchAuthor},{searchType},{searchTimeSpan},{ForumId}";
            OpenThreadForSearch(args);

            TopNavButtonListBox.SelectedItem = null;
        }

        private void NoticeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenThreadForItemType("notice");
            TopNavButtonListBox.SelectedItem = null;
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
                    CommonService.SendToast(xml1);
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
                    new RoamingSettingsService().SaveBlockUsers();
                }

                CommonService.SendToast(xml2);
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
                    CommonService.SendToast(xml1);
                    return;
                }

                if (!_myRoamingSettings.BlockThreads.Any(t => t.ThreadId == PopupThreadId))
                {
                    _myRoamingSettings.BlockThreads.Add(new BlockThread { UserId = PopupUserId, Username = PopupUsername, ThreadId = PopupThreadId, ThreadTitle = PopupThreadTitle, ForumId = PopupForumId, ForumName = PopupForumName });
                    new RoamingSettingsService().SaveBlockThreads();
                }

                string xml2 = "<toast>" +
                    "<visual>" +
                        "<binding template='ToastGeneric'>" +
                            $"<text>恭喜您，已将《{PopupThreadTitle}》加入黑名单</text>" +
                            $"<text>刷新后生效</text>" +
                        "</binding>" +
                    "</visual>" +
                    "</toast>";

                CommonService.SendToast(xml2);
            }
        }

        private async void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (PopupThreadId == 0 || string.IsNullOrEmpty(PopupThreadTitle))
            {
                return;
            }

            var cts = new CancellationTokenSource();
            await SendService.SendAddToFavoritesActionAsync(cts, PopupThreadId, PopupThreadTitle);
        }

        private void OpenUserThread_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PopupUsername))
            {
                return;
            }

            OpenThreadForSearch($",{PopupUsername},0,0,-1");
            TopNavButtonListBox.SelectedItem = null;
        }

        private async void AddBuddy_Click(object sender, RoutedEventArgs e)
        {
            if (PopupUserId == 0 || string.IsNullOrEmpty(PopupUsername))
            {
                return;
            }

            var cts = new CancellationTokenSource();
            await SendService.SendAddBuddyActionAsync(cts, PopupUserId, PopupUsername);
        }
        #endregion

        #region 坛友资料及短消息之弹窗
        public static int PopupUserId { get; set; }
        public static string PopupUsername { get; set; }
        public static int PopupThreadId { get; set; }
        public static string PopupThreadTitle { get; set; }
        public static int PopupForumId { get; set; }
        public static string PopupForumName { get; set; }

        public void OpenUserInfoDialog()
        {
            if (PopupUserId == 0)
            {
                return;
            }

            OpenInputPanel(typeof(UserInfoPage), $"{PopupUserId},{PopupUsername}");
        }

        public void OpenUserMessageDialog()
        {
            if (PopupUserId == 0)
            {
                return;
            }

            OpenInputPanel(typeof(UserMessagePage), $"{PopupUserId},{PopupUsername}");
        }

        public void OpenQuoteDetailPage(int userId, string username, int postId, int threadId)
        {
            OpenInputPanel(typeof(QuoteDetailPage), $"{userId},{username},{postId},{threadId}");
        }

        public void OpenUserMessageHubPage()
        {
            OpenInputPanel(typeof(UserMessageHubPage), null);
        }

        private void openUserInfoDialogButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUserInfoDialog();
        }

        private void openUserMessageListButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUserMessageHubPage();
        }

        private void openUserMessageDialogButton_Click(object sender, RoutedEventArgs e)
        {
            OpenUserMessageDialog();
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

        #region 消息发送
        public void OpenSendNewThreadPanel()
        {
            OpenInputPanel(typeof(NewThreadPage), null);

            //FindName("UserDialog");

            //Action<string> sendSuccess = (title) =>
            //{
            //    // 开始倒计时
            //    SendMessageTimerSetup();

            //    // 发贴成功后，刷新主题列表
            //    if (AppFrame.Content.GetType().Equals(typeof(ThreadAndReplyPage)))
            //    {
            //        var page = (ThreadAndReplyPage)AppFrame.Content;
            //        var cmdBar = (CommandBar)page.FindName("LeftCommandBar");
            //        if (cmdBar.DataContext.GetType().Equals(typeof(DefaultThreadListViewViewModel)))
            //        {
            //            var vm = (DefaultThreadListViewViewModel)cmdBar.DataContext;
            //            vm.RefreshThreadCommand.Execute(null);
            //        }
            //    }
            //};

            //var sendControl = new SendControl(SendType.New, ForumId, sendSuccess);
            //var binding = new Binding { Path = new PropertyPath("Countdown"), Source = this };
            //sendControl.SetBinding(SendControl.CountdownProperty, binding);

            //var titleBinding = new Binding { Path = new PropertyPath("Countdown"), Source = this, Converter = new CountdownToCountdownLabelConverter(), ConverterParameter = "发表新话题" };
            //UserDialog.SetBinding(ContentDialog.TitleProperty, titleBinding);

            //UserDialog.ContentTemplate = null;
            //UserDialog.Content = sendControl;
        }

        //public void OpenSendReplyPostPanel(string replyType, int postAuthorUserId, string postAuthorUsername, string quoteSimpleContent, string postTime, int floorNo, int postId, int threadId)
        //{
        //    FindName("UserDialog");

        //    Action<string> sendSuccess = (title) =>
        //    {
        //        // 开始倒计时
        //        SendMessageTimerSetup();

        //        // 回贴成功后，刷新回复页到底部
        //        if (AppFrame.Content.GetType().Equals(typeof(ThreadAndReplyPage)))
        //        {
        //            var page = (ThreadAndReplyPage)AppFrame.Content;
        //            var cmdBar = (CommandBar)page.FindName("RightCommandBar");
        //            if (cmdBar.DataContext.GetType().Equals(typeof(ReplyListViewForDefaultViewModel)))
        //            {
        //                var vm = (ReplyListViewForDefaultViewModel)cmdBar.DataContext;
        //                vm.LoadLastPageDataCommand.Execute(null);
        //            }
        //            else if (cmdBar.DataContext.GetType().Equals(typeof(ReplyListViewForSpecifiedPostViewModel)))
        //            {
        //                var vm = (ReplyListViewForSpecifiedPostViewModel)cmdBar.DataContext;
        //                vm.LoadLastPageDataCommand.Execute(null);
        //            }
        //        }
        //        else if (AppFrame.Content.GetType().Equals(typeof(ReplyListPage)))
        //        {
        //            var page = (ReplyListPage)AppFrame.Content;
        //            var cmdBar = (CommandBar)page.FindName("RightCommandBar");
        //            if (cmdBar.DataContext.GetType().Equals(typeof(ReplyListViewForDefaultViewModel)))
        //            {
        //                var vm = (ReplyListViewForDefaultViewModel)cmdBar.DataContext;
        //                vm.LoadLastPageDataCommand.Execute(null);
        //            }
        //            else if (cmdBar.DataContext.GetType().Equals(typeof(ReplyListViewForSpecifiedPostViewModel)))
        //            {
        //                var vm = (ReplyListViewForSpecifiedPostViewModel)cmdBar.DataContext;
        //                vm.LoadLastPageDataCommand.Execute(null);
        //            }
        //        }
        //    };

        //    var sendControl = new SendControl(replyType, postAuthorUserId, postAuthorUsername, quoteSimpleContent, postTime, floorNo, postId, threadId, sendSuccess);
        //    var binding = new Binding { Path = new PropertyPath("Countdown"), Source = this };
        //    sendControl.SetBinding(SendControl.CountdownProperty, binding);

        //    string titleParameter = string.Empty;
        //    if (replyType.Equals("r"))
        //    {
        //        titleParameter = "回复";
        //    }
        //    else if (replyType.Equals("q"))
        //    {
        //        titleParameter = "引用";
        //    }

        //    var titleBinding = new Binding { Path = new PropertyPath("Countdown"), Source = this, Converter = new CountdownToCountdownLabelConverter(), ConverterParameter = titleParameter };
        //    UserDialog.SetBinding(ContentDialog.TitleProperty, titleBinding);

        //    UserDialog.ContentTemplate = null;
        //    UserDialog.Content = sendControl;
        //}

        public void OpenSendEditPostPanel(PostEditDataModel editData)
        {
            OpenInputPanel(typeof(EditPostPage), editData);

            //FindName("UserDialog");

            //Action<string> sendSuccess = (t) =>
            //{
            //    // 开始倒计时
            //    SendMessageTimerSetup();

            //    // 回贴成功后，刷新回复页到底部
            //    if (AppFrame.Content.GetType().Equals(typeof(ThreadAndReplyPage)))
            //    {
            //        var page = (ThreadAndReplyPage)AppFrame.Content;
            //        var cmdBar = (CommandBar)page.FindName("RightCommandBar");
            //        if (cmdBar.DataContext.GetType().Equals(typeof(ReplyListViewForDefaultViewModel)))
            //        {
            //            var vm = (ReplyListViewForDefaultViewModel)cmdBar.DataContext;
            //            vm.RefreshReplyCommand.Execute(null);
            //        }
            //        else if (cmdBar.DataContext.GetType().Equals(typeof(ReplyListViewForSpecifiedPostViewModel)))
            //        {
            //            var vm = (ReplyListViewForSpecifiedPostViewModel)cmdBar.DataContext;
            //            vm.RefreshReplyCommand.Execute(null);
            //        }
            //    }
            //    else if (AppFrame.Content.GetType().Equals(typeof(ReplyListPage)))
            //    {
            //        var page = (ReplyListPage)AppFrame.Content;
            //        var cmdBar = (CommandBar)page.FindName("RightCommandBar");
            //        if (cmdBar.DataContext.GetType().Equals(typeof(ReplyListViewForDefaultViewModel)))
            //        {
            //            var vm = (ReplyListViewForDefaultViewModel)cmdBar.DataContext;
            //            vm.RefreshReplyCommand.Execute(null);
            //        }
            //        else if (cmdBar.DataContext.GetType().Equals(typeof(ReplyListViewForSpecifiedPostViewModel)))
            //        {
            //            var vm = (ReplyListViewForSpecifiedPostViewModel)cmdBar.DataContext;
            //            vm.RefreshReplyCommand.Execute(null);
            //        }
            //    }
            //};

            //var sendControl = new SendControl(title, content, postId, threadId, sendSuccess);
            //var binding = new Binding { Path = new PropertyPath("Countdown"), Source = this };
            //sendControl.SetBinding(SendControl.CountdownProperty, binding);

            //var titleBinding = new Binding { Path = new PropertyPath("Countdown"), Source = this, Converter = new CountdownToCountdownLabelConverter(), ConverterParameter = "编辑" };
            //UserDialog.SetBinding(ContentDialog.TitleProperty, titleBinding);

            //UserDialog.ContentTemplate = null;
            //UserDialog.Content = sendControl;
        }
        #endregion


        private void AccountLogoutButton_Click(object sender, RoutedEventArgs e)
        {
            AccountService.ClearDefaultStatus();
            ((Frame)Window.Current.Content).Navigate(typeof(LoginPage));
        }

        private async void PurchaseRemoveAdButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var listing = await CurrentApp.LoadListingInformationAsync();
                var iap = listing.ProductListings.FirstOrDefault(p => p.Value.ProductId == "RemoveAd");
                var purchaseResult = await CurrentApp.RequestProductPurchaseAsync(iap.Value.ProductId);
                if (CurrentApp.LicenseInformation.ProductLicenses[iap.Value.ProductId].IsActive)
                {
                    CurrentApp.ReportProductFulfillment(iap.Value.ProductId);

                    AdWrap.Visibility = Visibility.Collapsed;
                    MainSplitView.Margin = new Thickness(0);
                }
            }
            catch (Exception)
            {
            }
        }

        #region 遮罩及输入面板
        public void SetTitleForInputPanel(string title)
        {
            InputPanelTitleTextBlock.Text = title;
        }

        private void OpenInputPanel(Type pageType, object parameters)
        {
            FindName("InputPanelMask");
            FindName("InputPanel");

            if (InputPanel.Visibility != Visibility.Visible)
            {
                InputPanel.Visibility = InputPanelMask.Visibility = Visibility.Visible;
                OpenInputPanelMaskAnimation.Begin();
            }

            InputPanelFrame.Navigate(pageType, parameters);
        }

        private void InputPanelFrame_Navigated(object sender, NavigationEventArgs e)
        {
            InputPanelBackButton.Visibility = InputPanelFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
            InputPanelForwardButton.Visibility = InputPanelFrame.CanGoForward ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CloseInputPanelMask_Tapped(object sender, TappedRoutedEventArgs e)
        {
            InputPanelDoubleAnimation.To = InputPanel.ActualHeight;
            InputPanelAnimation.Begin();
            InputPanelAnimation.Completed += (s2, e2) =>
            {
                CloseInputPanelMaskAnimation.Begin();
                InputPanel.Visibility = InputPanelMask.Visibility = Visibility.Collapsed;

                InputPanelAnimation.Stop();
                OpenInputPanelMaskAnimation.Stop();
                CloseInputPanelMaskAnimation.Stop();
            };

            SetTitleForInputPanel(string.Empty);
            InputPanelFrame.BackStack.Clear();
        }

        private void InputPanelFrameGoBack()
        {
            if (InputPanelFrame.CanGoBack)
            {
                InputPanelFrame.GoBack();
            }
        }

        private void InputPanelFrameGoForward()
        {
            if (InputPanelFrame.CanGoForward)
            {
                InputPanelFrame.GoForward();
            }
        }

        private void InputPanelBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            InputPanelFrameGoBack();
        }

        private void InputPanelForwardButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            InputPanelFrameGoForward();
        }

        #endregion

        #region Ink面板
        private async Task<List<string>[]> GetInkImage()
        {
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)MyInkCanvas.ActualWidth, (int)MyInkCanvas.ActualHeight, 96);

            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(Colors.White);
                ds.DrawInk(MyInkCanvas.InkPresenter.StrokeContainer.GetStrokes());
            }

            using (var stream = new InMemoryRandomAccessStream())
            {
                await renderTarget.SaveAsync(stream, CanvasBitmapFileFormat.Jpeg, 1f);

                var cts = new CancellationTokenSource();
                return await SendService.UploadFileAsync(cts, stream, 
                    (fileIndex, fileCount, fileName) => { ShowTipsBarWhenUpload($"{fileName} 上载中 {fileIndex}/{fileCount}"); }, 
                    (fileCount) => { ShowTipsBar($"文件上传已完成，共上传 {fileCount} 个文件。", false); });
            }
        }

        public void OpenInkPanel()
        {
            InkPanel.Visibility = Visibility.Visible;
        }

        public void CloseInkPanel()
        {
            InkPanel.Visibility = Visibility.Collapsed;
        }

        private void CloseInkPanelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseInkPanel();
        }

        private async void UploadInkContentButton_Click(object sender, RoutedEventArgs e)
        {
            var data = await GetInkImage();
            UploadInkContentDelegate(data);
            CloseInkPanel();
        }

        private async void PostInkContentButton_Click(object sender, RoutedEventArgs e)
        {
            var data = await GetInkImage();
            PostInkContentDelegate(data);
            CloseInkPanel();
        }
        #endregion

        #region 提示条
        private async void TipsBar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                ShowTipsBarAnimationHide.Begin();
            });
        }

        public async void ShowTipsBar(string tipsText, bool isAlert)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                var c1 = Color.FromArgb(255, 239, 80, 38); // 红色
                var c2 = Color.FromArgb(255, 126, 188, 67); // 绿色
                TipsBar.Background = new SolidColorBrush(isAlert ? c1 : c2);
                TipsBarTextBlock.Text = Uri.UnescapeDataString(tipsText);
                ShowTipsBarShortAnimation.Begin();
            });
        }

        public async void ShowTipsBarWhenUpload(string tipsText)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                var c = Color.FromArgb(255, 253, 184, 19); // 桔色
                TipsBar.Background = new SolidColorBrush(c);
                TipsBarTextBlock.Text = Uri.UnescapeDataString(tipsText);
                ShowTipsBarAnimationShow.Begin();
            });
        }

        private void SubSplitViewToggle_Click(object sender, RoutedEventArgs e)
        {
            SubSplitView.IsPaneOpen = !SubSplitView.IsPaneOpen;

            // 切换主题样式
            App.Current.Resources.MergedDictionaries.RemoveAt(1);
            App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("ms-resource:///Files/Themes/Red.xaml", UriKind.Absolute) });
            
            // 切换黑白场景以刷新主题
            var i = ThemeTypeComboBox.SelectedIndex;
            ThemeTypeComboBox.SelectedIndex = -1;
            ThemeTypeComboBox.SelectedIndex = i;
        }
        #endregion

        //private void xxx_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    ShowTipsBarAnimationShow.Begin();
        //}

        //private void yyy_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    ShowTipsBarAnimationHide.Begin();
        //}


    }
}
