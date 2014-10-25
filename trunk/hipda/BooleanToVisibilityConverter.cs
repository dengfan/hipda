using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace hipda
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool data = (bool)value;
            return data ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Windows.UI.Xaml.Visibility data = (Windows.UI.Xaml.Visibility)value;
            return data == Windows.UI.Xaml.Visibility.Visible;
        }
    }
}
