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

                        PictureOpacity = 1;
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

                        PictureOpacity = PictureOpacityBak;
                    }
                }
            }
        }

        public double FontSize1
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize1", typeof(double), typeof(SettingsDependencyObject), new PropertyMetadata(15d, new PropertyChangedCallback(OnFontSize1Changed)));

        private static void OnFontSize1Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as SettingsDependencyObject;
            double fs1 = (double)e.NewValue;
            instance.FontSize2 = fs1 > 15 ? 14 : 12;
        }

        public double FontSize2
        {
            get { return (double)GetValue(FontSize2Property); }
            set { SetValue(FontSize2Property, value); }
        }

        // Using a DependencyProperty as the backing store for MyFontSize2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontSize2Property =
            DependencyProperty.Register("FontSize2", typeof(double), typeof(SettingsDependencyObject), new PropertyMetadata(12d));


        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            set { SetValue(LineHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyLineHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineHeightProperty =
            DependencyProperty.Register("LineHeight", typeof(double), typeof(SettingsDependencyObject), new PropertyMetadata(22d));


        public double PictureOpacityBak { get; set; }
        public double PictureOpacity
        {
            get { return (double)GetValue(PictureOpacityProperty); }
            set { SetValue(PictureOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PictureOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PictureOpacityProperty =
            DependencyProperty.Register("PictureOpacity", typeof(double), typeof(SettingsDependencyObject), new PropertyMetadata(1d, new PropertyChangedCallback(OnPictureOpacityChanged)));

        private static void OnPictureOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as SettingsDependencyObject;
            double val = (double)e.NewValue;
            if (val < 1)
            {
                instance.PictureOpacityBak = val;
            }
        }
    }
}
