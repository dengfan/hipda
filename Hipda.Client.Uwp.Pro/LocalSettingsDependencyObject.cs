using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro
{
    public class LocalSettingsDependencyObject : DependencyObject
    {
        public int ThemeType
        {
            get { return (int)GetValue(ThemeTypeProperty); }
            set { SetValue(ThemeTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThemeType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThemeTypeProperty =
            DependencyProperty.Register("ThemeType", typeof(int), typeof(LocalSettingsDependencyObject), new PropertyMetadata(App.Current.RequestedTheme == ApplicationTheme.Light ? 0 : 1));


        public double FontSize1
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize1", typeof(double), typeof(LocalSettingsDependencyObject), new PropertyMetadata(15D, new PropertyChangedCallback(OnFontSize1Changed)));

        private static void OnFontSize1Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as LocalSettingsDependencyObject;
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
            DependencyProperty.Register("FontSize2", typeof(double), typeof(LocalSettingsDependencyObject), new PropertyMetadata(12D));


        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            set { SetValue(LineHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyLineHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineHeightProperty =
            DependencyProperty.Register("LineHeight", typeof(double), typeof(LocalSettingsDependencyObject), new PropertyMetadata(22D));


        public double PictureOpacityBak { get; set; } = 0.4D;
        public double PictureOpacity
        {
            get { return (double)GetValue(PictureOpacityProperty); }
            set { SetValue(PictureOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PictureOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PictureOpacityProperty =
            DependencyProperty.Register("PictureOpacity", typeof(double), typeof(LocalSettingsDependencyObject), new PropertyMetadata(0.4D, new PropertyChangedCallback(OnPictureOpacityChanged)));

        private static void OnPictureOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as LocalSettingsDependencyObject;
            double val = (double)e.NewValue;
            if (val < 1)
            {
                instance.PictureOpacityBak = val;
            }
        }


        public int FontContrastRatio
        {
            get { return (int)GetValue(FontContrastRatioProperty); }
            set { SetValue(FontContrastRatioProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontContrastRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontContrastRatioProperty =
            DependencyProperty.Register("FontContrastRatio", typeof(int), typeof(LocalSettingsDependencyObject), new PropertyMetadata(1));


    }
}
