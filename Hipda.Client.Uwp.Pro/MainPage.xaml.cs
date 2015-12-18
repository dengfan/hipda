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
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace Hipda.Client.Uwp.Pro
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        

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
            CloseView.Begin();
        }

        private void btnMore_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (MainSplitView.DisplayMode == SplitViewDisplayMode.Overlay || MainSplitView.DisplayMode == SplitViewDisplayMode.CompactOverlay)
            {
                // 以免 pane 挡住 full sebject panel
                MainSplitView.IsPaneOpen = false;
            }

            this.FindName(nameof(FullSebjectPanel));
            OpenView.Begin();
        }

        private void FullSebjectPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "fid=2");
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "fid=14");
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "fid=57");
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "item=threads");
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "item=posts");
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
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
                SearchButton1Submit();
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            if (Page1.RequestedTheme == ElementTheme.Light)
            {
                Page1.RequestedTheme = ElementTheme.Dark;

                var c = Colors.Black;
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

                var c = Colors.White;
                titleBar.BackgroundColor = c;
                titleBar.InactiveBackgroundColor = c;
                titleBar.ForegroundColor = Colors.Black;
                titleBar.ButtonBackgroundColor = c;
                titleBar.ButtonInactiveBackgroundColor = c;
                titleBar.ButtonForegroundColor = Colors.Black;
                titleBar.ButtonHoverBackgroundColor = Colors.LightGray;
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

        private void SearchButton1Submit()
        {
            string paramFormat = "search={0},{1},{2},{3},1";

            string searchKeyword = Uri.EscapeUriString(KeywordTextBox.Text.Trim().Replace(",", " "));
            string searchAuthor = Uri.EscapeUriString(AuthorTextBox.Text.Trim().Replace(",", " "));
            int searchType = SearchTypeComboBox.SelectedIndex;
            int searchTimeSpan = SearchTimeSpanComboBox.SelectedIndex;

            string param = string.Format(paramFormat, searchKeyword, searchAuthor, searchType, searchTimeSpan);
            AppFrame.Navigate(typeof(ThreadAndReplyPage), param);
        }

        private void SearchButton1_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SearchButton1Submit();
        }

        private void SearchButton2_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string paramFormat = "search={0},{1},{2},{3},2";

            string searchKeyword = Uri.EscapeUriString(KeywordTextBox.Text.Trim().Replace(",", " "));
            string searchAuthor = Uri.EscapeUriString(AuthorTextBox.Text.Trim().Replace(",", " "));
            int searchType = SearchTypeComboBox.SelectedIndex;
            int searchTimeSpan = SearchTimeSpanComboBox.SelectedIndex;

            string param = string.Format(paramFormat, searchKeyword, searchAuthor, searchType, searchTimeSpan);
            AppFrame.Navigate(typeof(ThreadAndReplyPage), param);
        }
    }
}
