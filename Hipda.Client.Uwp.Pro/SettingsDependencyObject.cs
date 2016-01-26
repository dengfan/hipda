using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Hipda.Client.Uwp.Pro
{
    public class SettingsDependencyObject : DependencyObject
    {
        private int themeType;

        public int ThemeType
        {
            get { return themeType; }
            set
            {
                themeType = value;

                var frame = Window.Current.Content as Frame;
                var mp = frame.Content as MainPage;
                if (mp != null)
                {
                    var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                    if (themeType == 0)
                    {
                        mp.RequestedTheme = ElementTheme.Light;
                        titleBar.BackgroundColor = null;
                        titleBar.InactiveBackgroundColor = null;
                        titleBar.ForegroundColor = null;
                        titleBar.ButtonBackgroundColor = null;
                        titleBar.ButtonInactiveBackgroundColor = null;
                        titleBar.ButtonForegroundColor = null;
                        titleBar.ButtonHoverBackgroundColor = null;
                    }
                    else if (themeType == 1)
                    {
                        mp.RequestedTheme = ElementTheme.Dark;
                        Color c = Colors.Black;
                        titleBar.BackgroundColor = c;
                        titleBar.InactiveBackgroundColor = c;
                        titleBar.ForegroundColor = Colors.Silver;
                        titleBar.ButtonBackgroundColor = c;
                        titleBar.ButtonInactiveBackgroundColor = c;
                        titleBar.ButtonForegroundColor = Colors.Silver;
                        titleBar.ButtonHoverBackgroundColor = Colors.DimGray;
                    }
                }
            }
        }

        public double FontSize1
        {
            get { return (double)GetValue(MyFontSizeProperty); }
            set { SetValue(MyFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyFontSizeProperty =
            DependencyProperty.Register("FontSize1", typeof(double), typeof(SettingsDependencyObject), new PropertyMetadata(15));


        public double FontSize2
        {
            get { return (double)GetValue(MyFontSize2Property); }
            set { SetValue(MyFontSize2Property, value); }
        }

        // Using a DependencyProperty as the backing store for MyFontSize2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyFontSize2Property =
            DependencyProperty.Register("FontSize2", typeof(double), typeof(SettingsDependencyObject), new PropertyMetadata(12));


        public double LineHeight
        {
            get { return (double)GetValue(MyLineHeightProperty); }
            set { SetValue(MyLineHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyLineHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyLineHeightProperty =
            DependencyProperty.Register("LineHeight", typeof(double), typeof(SettingsDependencyObject), new PropertyMetadata(22));


        public double PictureOpacity
        {
            get { return (double)GetValue(PictureOpacityProperty); }
            set { SetValue(PictureOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PictureOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PictureOpacityProperty =
            DependencyProperty.Register("PictureOpacity", typeof(double), typeof(SettingsDependencyObject), new PropertyMetadata(1));



    }
}
