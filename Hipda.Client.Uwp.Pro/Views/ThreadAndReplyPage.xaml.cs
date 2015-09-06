using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Hipda.Client.Uwp.Pro.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ThreadAndReplyPage : Page
    {
        private ThreadAndReplyViewModel _threadAndReplyViewModel;

        public ThreadAndReplyPage()
        {
            this.InitializeComponent();

            _threadAndReplyViewModel = new ThreadAndReplyViewModel(
                ThreadListView,
                () => {
                    leftProgress.IsActive = true;
                    leftProgress.Visibility = Visibility.Visible;
                }, 
                () => {
                    leftProgress.IsActive = false;
                    leftProgress.Visibility = Visibility.Collapsed;
                });

            DataContext = _threadAndReplyViewModel;
        }

        #region 后退事件
        private void OnBackRequested()
        {
            if (RightWrap.DataContext != null)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                LeftColumn.Width = new GridLength(1, GridUnitType.Star);
                RightColumn.Width = new GridLength(0);
            }
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            e.Handled = true;
            OnBackRequested();
        }

        private void ThreadListPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            OnBackRequested();
        }
        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // 初次载入，
            if (AdaptiveStates.CurrentState == NarrowState)
            {
                LeftColumn.Width = new GridLength(1, GridUnitType.Star); 
                RightColumn.Width = new GridLength(0);
            }

            #region 注册后退按钮事件
            SystemNavigationManager.GetForCurrentView().BackRequested += ThreadListPage_BackRequested;

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed; ;
            }
            #endregion


        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            #region 注销后退按钮事件
            SystemNavigationManager.GetForCurrentView().BackRequested -= ThreadListPage_BackRequested; ;

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed; ;
            }
            #endregion
        }

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            UpdateForVisualState(e.NewState, e.OldState);
        }

        private void UpdateForVisualState(VisualState newState, VisualState oldState = null)
        {
            var isNarrow = newState == NarrowState;
            if (isNarrow && oldState == DefaultState)
            {
                if (RightWrap.DataContext != null)
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                    LeftColumn.Width = new GridLength(0);
                    RightColumn.Width = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    LeftColumn.Width = new GridLength(1, GridUnitType.Star);
                    RightColumn.Width = new GridLength(0);
                }
            }
            else
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
        }

        private void ThreadListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            RightWrap.DataContext = null;

            if (AdaptiveStates.CurrentState == NarrowState)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                LeftColumn.Width = new GridLength(0);
                RightColumn.Width = new GridLength(1, GridUnitType.Star);
            }

            ThreadItemViewModel data = e.ClickedItem as ThreadItemViewModel;
            data.SelectThreadItem(
                ReplyListView,
                () => {
                    rightProgress.IsActive = true;
                    rightProgress.Visibility = Visibility.Visible;
                },
                () => {
                    rightProgress.IsActive = false;
                    rightProgress.Visibility = Visibility.Collapsed;
                });

            RightWrap.DataContext = data;
            ReplyListView.ItemsSource = data.ReplyItemCollection;
        }

        #region 增量更新
        void ReplyListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.Handled = true;

            if (args.Phase != 0)
            {
                throw new Exception("Not in phase 0.");
            }

            ReplyItemModel reply = (ReplyItemModel)args.Item;

            Grid layoutGrid = (Grid)args.ItemContainer.ContentTemplateRoot;

            Border avatarImageBorder = (Border)layoutGrid.FindName("avatarImageBorder");
            TextBlock threadTitleTextBlock = (TextBlock)layoutGrid.FindName("threadTitleTextBlock");
            Run ownerNameTextBlockRun = (Run)layoutGrid.FindName("ownerNameTextBlock");
            Run createTimeTextBlockRun = (Run)layoutGrid.FindName("createTimeTextBlock");
            Button menuButton = (Button)layoutGrid.FindName("menuButton");
            ContentControl replyContent = (ContentControl)layoutGrid.FindName("replyContent");
            Button showMoreButton = (Button)layoutGrid.FindName("showMoreButton");
            MenuFlyoutItem modifyMenuFlyoutItem = (MenuFlyoutItem)layoutGrid.FindName("modifyMenuFlyoutItem");

            if (reply.FloorNo == 1 && !string.IsNullOrEmpty(reply.ThreadTitle))
            {
                threadTitleTextBlock.Text = reply.ThreadTitle;
                threadTitleTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                threadTitleTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            ownerNameTextBlockRun.Text = reply.AuthorUsername;
            createTimeTextBlockRun.Text = reply.AuthorCreateTime;
            menuButton.DataContext = reply;

            try
            {
                replyContent.Content = XamlReader.Load(reply.XamlContent);
            }
            catch
            {
                // 用于过滤掉无意义符号
                string regexForTitle = @"[^a-zA-Z\d\u4e00-\u9fa5]";
                string text = Regex.Replace(reply.TextContent, regexForTitle, "~");
                string xaml = string.Format(@"<RichTextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><Paragraph>{0}</Paragraph></RichTextBlock>", text);
                replyContent.Content = XamlReader.Load(xaml);
            }

            // 如果楼层内容的作者是当前账号，则显示编辑按钮
            //if (reply.AuthorUserId == DataSource.UserId)
            //{
            //    modifyMenuFlyoutItem.IsEnabled = true;
            //}
            //else
            //{
            //    modifyMenuFlyoutItem.IsEnabled = false;
            //}

            //if (reply.ImageCount > DataSource.MaxImageCount)
            //{
            //    showMoreButton.DataContext = reply;
            //    showMoreButton.Content = string.Format("查看本楼层完整原始内容 ({0}P)", reply.ImageCount);
            //    showMoreButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //}
            //else
            //{
            //    showMoreButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //}

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

            ReplyItemModel reply = (ReplyItemModel)args.Item;
            SelectorItem itemContainer = (SelectorItem)args.ItemContainer;
            Grid layoutGrid = (Grid)itemContainer.ContentTemplateRoot;

            Border avatarImageBorder = (Border)layoutGrid.FindName("avatarImageBorder");

            var imageBrush = new ImageBrush
            {
                ImageSource = new BitmapImage
                {
                    DecodePixelWidth = 40, // natural px width of image source
                    UriSource = new Uri(reply.AvatarUrl)
                }
            };
            imageBrush.ImageFailed += (sender2, e2) =>
            {
                var imageBrush2 = sender2 as ImageBrush;
                imageBrush2.ImageSource = new BitmapImage
                {
                    DecodePixelWidth = 40, // natural px width of image source
                    UriSource = new Uri("ms-appx:///Assets/Faces/no_face.png")
                };
            };
            imageBrush.Stretch = Stretch.UniformToFill;
            avatarImageBorder.Background = imageBrush;
        }
        #endregion
    }
}
