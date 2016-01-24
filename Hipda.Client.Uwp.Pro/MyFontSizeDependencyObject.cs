using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Hipda.Client.Uwp.Pro
{
    public class MyFontSizeDependencyObject : DependencyObject
    {


        public double MyFontSize
        {
            get { return (double)GetValue(MyFontSizeProperty); }
            set { SetValue(MyFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyFontSizeProperty =
            DependencyProperty.Register("MyFontSize", typeof(double), typeof(MyFontSizeDependencyObject), new PropertyMetadata(15));


    }
}
