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
        bool _isShowFullForumPanel = false;

        public static int CurrentForumId = 2;
        public static string CurrentNavButtonName = "DiButton"; // 默认为地板

        public Frame AppFrame { get { return this.MainFrame; } }

        public MainPage()
        {
            this.InitializeComponent();
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
        }

        private void pageGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (_isShowFullForumPanel)
            {
                _isShowFullForumPanel = false;
                CloseView.Begin();
            }
        }

        private void btnMore_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (MainSplitView.DisplayMode == SplitViewDisplayMode.Overlay || MainSplitView.DisplayMode == SplitViewDisplayMode.CompactOverlay)
            {
                // 以免 pane 挡住 full sebject panel
                MainSplitView.IsPaneOpen = false;
            }

            FindName("FullSebjectPanel");
            _isShowFullForumPanel = true;
            OpenView.Begin();
        }

        private void FullSebjectPanel_Tapped(object sender, TappedRoutedEventArgs e) 
        {
            e.Handled = true;
        }

        private async void SetSelected(Button button)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                var btn1 = this.FindName(CurrentNavButtonName) as Button;
                if (btn1 != null)
                {
                    btn1.Style = this.Resources["NavButtonNormalStyle"] as Style;
                }

                button.Style = this.Resources["NavButtonSelectedStyle"] as Style;
                CurrentNavButtonName = button.Name;
            });
        }

        private void DiButton_Click(object sender, RoutedEventArgs e)
        {
            SetSelected(sender as Button);
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "fid=2");
        }

        private void BsButton_Click(object sender, RoutedEventArgs e)
        {
            SetSelected(sender as Button);
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "fid=14");
        }

        private void EiButton_Click(object sender, RoutedEventArgs e)
        {
            SetSelected(sender as Button);
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "fid=57");
        }

        private void MyThreadsButton_Click(object sender, RoutedEventArgs e)
        {
            SetSelected(sender as Button);
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "item=threads");
        }

        private void MyPostsButton_Click(object sender, RoutedEventArgs e)
        {
            SetSelected(sender as Button);
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "item=posts");
        }

        private void MyFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            SetSelected(sender as Button);
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "item=favorites");
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
        }

        private void SearchDefaultSubmit()
        {
            var btn = this.FindName("SearchButton") as Button;
            SetSelected(btn);

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
            
            var btn = this.FindName("SearchButton") as Button;
            SetSelected(btn);

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
            SetSelected(sender as Button);
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
