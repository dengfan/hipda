using Hipda.Client.Uwp.Pro.Controls;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.ViewModels;
using Hipda.Client.Uwp.Pro.Views;
using System;
using System.IO;
using System.Linq;
using Windows.UI;
using Windows.UI.Core;
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
        public static int CurrentForumId = 2;
        public static string CurrentNavButtonName = "DiButton"; // 默认为地板

        public Frame AppFrame { get { return this.MainFrame; } }

        void ElementAdapter()
        {
            PromptAllBorder.Visibility = ActualWidth >= 1000 ? Visibility.Collapsed : Visibility.Visible;
            BottomButtonContentControl.ContentTemplate = MainSplitView.IsPaneOpen ? Resources["BottomHorizontalButtonTemplate"] as DataTemplate : Resources["BottomVerticalButtonTemplate"] as DataTemplate;
        }

        public MainPage()
        {
            this.InitializeComponent();

            TopNavButtonListBox.DataContext = NavButtonViewModel.GetInstance();
            BottomButtonContentControl.DataContext = PromptNumViewModel.GetInstance();

            this.SizeChanged += (s, e) =>
            {
                ElementAdapter();
            };
        }

        private void Page1_Loaded(object sender, RoutedEventArgs e)
        {
            ElementAdapter();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string param = e.Parameter.ToString();
            AppFrame.Navigate(typeof(ThreadAndReplyPage), param);
        }

        private void MainSplitViewToggle_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
            BottomButtonContentControl.ContentTemplate = MainSplitView.IsPaneOpen ? Resources["BottomHorizontalButtonTemplate"] as DataTemplate : Resources["BottomVerticalButtonTemplate"] as DataTemplate;
        }

        void ShowLeftSwipePanel()
        {
            FindName("MaskGrid");
            FindName("LeftSwipePanel");

            if (MainSplitView.DisplayMode == SplitViewDisplayMode.Overlay || MainSplitView.DisplayMode == SplitViewDisplayMode.CompactOverlay)
            {
                // 以免 pane 挡住 left swipe panel
                MainSplitView.IsPaneOpen = false;
                ElementAdapter();
            }
            
            MaskGrid.Visibility = Visibility.Visible;
            OpenView.Begin();
        }

        void CloseLeftSwipePanel()
        {
            FindName("MaskGrid");
            FindName("LeftSwipePanel");

            if (MaskGrid.Visibility == Visibility.Visible)
            {
                MaskGrid.Visibility = Visibility.Collapsed;
                CloseView.Begin();

                TopNavButtonListBox.SelectedItem = null;
            }
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
                ShowLeftSwipePanel();
            }
            else
            {
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

            CloseLeftSwipePanel();
        }

        private void MaskGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            CloseLeftSwipePanel();
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

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            if (Page1.RequestedTheme == ElementTheme.Light)
            {
                Page1.RequestedTheme = ElementTheme.Dark;

                Color c = Colors.Black;
                titleBar.BackgroundColor = c;
                titleBar.InactiveBackgroundColor = c;
                titleBar.ForegroundColor = Colors.White;
                titleBar.ButtonBackgroundColor = c;
                titleBar.ButtonInactiveBackgroundColor = c;
                titleBar.ButtonForegroundColor = Colors.White;
                titleBar.ButtonHoverBackgroundColor = Colors.DimGray;
            }
            else
            {
                Page1.RequestedTheme = ElementTheme.Light;

                titleBar.BackgroundColor = null;
                titleBar.InactiveBackgroundColor = null;
                titleBar.ForegroundColor = null;
                titleBar.ButtonBackgroundColor = null;
                titleBar.ButtonInactiveBackgroundColor = null;
                titleBar.ButtonForegroundColor = null;
                titleBar.ButtonHoverBackgroundColor = null;
            }
        }

        public async void ShowTipBar(string tipContent)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                TipTextBlock.Text = Uri.UnescapeDataString(tipContent);
                ShowTipBarAnimation.Begin();
            });
        }

        private void MainSplitView_PaneClosed(SplitView sender, object args)
        {
            SearchPanel.Visibility = Visibility.Collapsed;
            ElementAdapter();
        }

        private void SearchDefaultSubmit()
        {
            string paramFormat = "search={0},{1},{2},{3},1";

            string searchKeyword = Uri.EscapeUriString(KeywordTextBox.Text.Trim().Replace(",", " "));
            string searchAuthor = Uri.EscapeUriString(AuthorTextBox.Text.Trim().Replace(",", " "));
            int searchType = SearchTypeComboBox.SelectedIndex;
            int searchTimeSpan = SearchTimeSpanComboBox.SelectedIndex;

            string param = string.Format(paramFormat, searchKeyword, searchAuthor, searchType, searchTimeSpan);
            AppFrame.Navigate(typeof(ThreadAndReplyPage), param);
        }

        private void SearchButton1_Click(object sender, RoutedEventArgs e)
        {
            SearchDefaultSubmit();
        }

        private void SearchButton2_Click(object sender, RoutedEventArgs e)
        {
            string paramFormat = "search={0},{1},{2},{3},2";

            string searchKeyword = Uri.EscapeUriString(KeywordTextBox.Text.Trim().Replace(",", " "));
            string searchAuthor = Uri.EscapeUriString(AuthorTextBox.Text.Trim().Replace(",", " "));
            int searchType = SearchTypeComboBox.SelectedIndex;
            int searchTimeSpan = SearchTimeSpanComboBox.SelectedIndex;

            string param = string.Format(paramFormat, searchKeyword, searchAuthor, searchType, searchTimeSpan);
            AppFrame.Navigate(typeof(ThreadAndReplyPage), param);
        }

        private void NoticeButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "item=notice");
        }

        #region 坛友资料及短消息之弹窗
        public static int PopupUserId { get; set; }
        public static string PopupUsername { get; set; }
        public static int PopupThreadId { get; set; }

        bool _isDialogShown = false;

        public async void OpenUserInfoDialog()
        {
            if (PopupUserId == 0)
            {
                return;
            }

            FindName("UserDialog");
            UserDialog.DataContext = new UserInfoDialogViewModel(PopupUserId);
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
            UserDialog.DataContext = new UserMessageDialogViewModel(PopupUserId);
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
            var vm = umb.DataContext as UserMessageDialogViewModel;
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

            var vm = new PostDetailDialogViewModel(postId, threadId);
            if (vm == null)
            {
                return;
            }

            UserDialog.DataContext = vm;
            UserDialog.Title = "查看引用楼之详情";
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

            var vm = new UserMessageListDialogViewModel();
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
        #endregion

        private void userDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _isDialogShown = false;
            UserDialog.DataContext = null;
        }
    }
}
