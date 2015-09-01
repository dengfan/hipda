using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Hipda.Client.Uwp.Pro.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ThreadAndReplyPage : Page
    {
        public ThreadAndReplyViewModel ThreadAndReplyViewModel
        {
            get { return _threadAndReplyViewModel; }
        }
        private readonly ThreadAndReplyViewModel _threadAndReplyViewModel = new ThreadAndReplyViewModel();

        public ThreadAndReplyPage()
        {
            this.InitializeComponent();

            DataContext = ThreadAndReplyViewModel;
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

        private void ThreadListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //var data = (ThreadAndReplyViewModels)e.ClickedItem;
            //RightWrap.DataContext = data;

            //if (AdaptiveStates.CurrentState == NarrowState)
            //{
            //    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            //    LeftColumn.Width = new GridLength(0);
            //    RightColumn.Width = new GridLength(1, GridUnitType.Star);
            //}
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
    }
}
