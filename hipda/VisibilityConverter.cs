using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using System.Threading.Tasks;

namespace hipda
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Boolean)
            {
                if ((bool)value == false)
                {
                    return Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
}
