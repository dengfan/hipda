using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Hipda.Client.Uwp.Pro
{
    public class MyTextDependencyObject : DependencyObject
    {


        public double MyFontSize1
        {
            get { return (double)GetValue(MyFontSizeProperty); }
            set { SetValue(MyFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyFontSizeProperty =
            DependencyProperty.Register("MyFontSize1", typeof(double), typeof(MyTextDependencyObject), new PropertyMetadata(15));


        public double MyFontSize2
        {
            get { return (double)GetValue(MyFontSize2Property); }
            set { SetValue(MyFontSize2Property, value); }
        }

        // Using a DependencyProperty as the backing store for MyFontSize2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyFontSize2Property =
            DependencyProperty.Register("MyFontSize2", typeof(double), typeof(MyTextDependencyObject), new PropertyMetadata(12));


        public double MyLineHeight
        {
            get { return (double)GetValue(MyLineHeightProperty); }
            set { SetValue(MyLineHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyLineHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyLineHeightProperty =
            DependencyProperty.Register("MyLineHeight", typeof(double), typeof(MyTextDependencyObject), new PropertyMetadata(22));


    }
}
