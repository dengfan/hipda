using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace hipda
{
    public static class ThemeHelper
    {
        public static void Dark1()
        {
            (App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color = Colors.White;
            (App.Current.Resources["AppBarBackgroundThemeBrush"] as SolidColorBrush).Color = Color.FromArgb(255, 36, 36, 36);
            (App.Current.Resources["AppBarBorderThemeBrush"] as SolidColorBrush).Color = Colors.Gainsboro;
            ResourceDictionary r = Application.Current.Resources;
            ((SolidColorBrush)r["PageBgColor"]).Color = Color.FromArgb(255, 29, 29, 29);
            ((SolidColorBrush)r["ItemBgColor"]).Color = Color.FromArgb(255, 38, 38, 38);
        }

        public static void Light1()
        {
            (App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush).Color = Colors.Black;
            (App.Current.Resources["AppBarBackgroundThemeBrush"] as SolidColorBrush).Color = Colors.Silver;
            (App.Current.Resources["AppBarBorderThemeBrush"] as SolidColorBrush).Color = Colors.Red;
            ResourceDictionary r = Application.Current.Resources;
            ((SolidColorBrush)r["PageBgColor"]).Color = Color.FromArgb(255, 243, 243, 243);
            ((SolidColorBrush)r["ItemBgColor"]).Color = Colors.White;
            
        }
    }
}
