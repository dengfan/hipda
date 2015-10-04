﻿using Hipda.Client.Uwp.Pro.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
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
        public Frame AppFrame { get { return this.MainFrame; } }

        public MainPage()
        {
            this.InitializeComponent();

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView().BackgroundOpacity = 1;
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView().BackgroundColor = ((SolidColorBrush)Resources["SystemControlBackgroundAccentBrush"]).Color;
                Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ForegroundColor = Colors.White;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string param = string.Format("fid={0}", e.Parameter);
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

        private void btnNotice_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "fid=2");
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.Navigate(typeof(ThreadAndReplyPage), "fid=6");
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
    }
}
