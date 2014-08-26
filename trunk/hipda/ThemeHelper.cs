﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace hipda
{
    public static class ThemeHelper
    {
        public static void Night()
        {
 
        }

        public static void Light1()
        {
            //(App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color = Colors.Black;

            ResourceDictionary r = Application.Current.Resources;
            ((SolidColorBrush)r["PageBgColor"]).Color = Color.FromArgb(255, 243, 243, 243);
            ((SolidColorBrush)r["ItemBgColor"]).Color = Colors.White;
            //((SolidColorBrush)r["CommandBarBgColor"]).Color = Colors.Gainsboro;
            //((SolidColorBrush)r["CommandFontColor"]).Color = Colors.Black;

            //StatusBar.GetForCurrentView().BackgroundColor = Colors.Purple;
        }

        public static void Light2()
        {
            //(App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color = Colors.Black;

            ResourceDictionary r = Application.Current.Resources;
            ((SolidColorBrush)r["PageBgColor"]).Color = Color.FromArgb(255, 243, 243, 243);
            ((SolidColorBrush)r["ItemBgColor"]).Color = Colors.White;
            //((SolidColorBrush)r["CommandBarBgColor"]).Color = Colors.Gainsboro;
            //((SolidColorBrush)r["CommandFontColor"]).Color = Colors.Black;

            //StatusBar.GetForCurrentView().BackgroundColor = Colors.Purple;
        }

        public static void Dark1()
        {
            //(App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color = Colors.White;

            ResourceDictionary r = Application.Current.Resources;
            ((SolidColorBrush)r["PageBgColor"]).Color = Color.FromArgb(255, 29, 29, 29);
            ((SolidColorBrush)r["ItemBgColor"]).Color = Color.FromArgb(255, 38, 38, 38);
            //((SolidColorBrush)r["CommandBarBgColor"]).Color = Color.FromArgb(255, 32, 32, 32);
            //((SolidColorBrush)r["CommandFontColor"]).Color = Colors.DimGray;

            //StatusBar.GetForCurrentView().BackgroundColor = Color.FromArgb(255, 94, 0, 94);
        }
    }
}
