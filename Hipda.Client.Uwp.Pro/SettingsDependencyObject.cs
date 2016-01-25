using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Hipda.Client.Uwp.Pro
{
    public class SettingsDependencyObject : DependencyObject
    {
        public int ThemeType { get; set; }

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
