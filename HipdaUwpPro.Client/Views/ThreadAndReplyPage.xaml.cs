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
        public List<ThreadAndReplyViewModels> ThreadListProp
        {
            get { return (List<ThreadAndReplyViewModels>)GetValue(ThreadListPropProperty); }
            set { SetValue(ThreadListPropProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThreadListProp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThreadListPropProperty =
            DependencyProperty.Register("ThreadListProp", typeof(List<ThreadAndReplyViewModels>), typeof(ThreadAndReplyPage), new PropertyMetadata(null));

        public ThreadAndReplyPage()
        {
            this.InitializeComponent();
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
            ThreadListProp = GetThreadList();
            ThreadListView.ItemsSource = ThreadListProp;

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
            var data = (ThreadAndReplyViewModels)e.ClickedItem;
            data.GetReplyList();
            RightWrap.DataContext = data;

            if (AdaptiveStates.CurrentState == NarrowState)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                LeftColumn.Width = new GridLength(0);
                RightColumn.Width = new GridLength(1, GridUnitType.Star);
            }
        }

        private List<ThreadAndReplyViewModels> GetThreadList()
        {
            var threadList = new List<ThreadAndReplyViewModels>();
            for (int i = 0; i < 50; i++)
            {
                threadList.Add(new ThreadItem { Id = i, Name = "功率" + i, Title = "法灯电动车工，夺不地工地城示灯影jfis不煤类时是革夺地且，夺城工在地肝苦不困kdfd不裁夺地夺萘在地载。" + i });
            }

            return threadList;
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
