using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Converters
{
    public class ElementThemeToMaskBackgroundBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (ElementTheme)value;
            if (val == ElementTheme.Dark)
            {
                return new SolidColorBrush(Colors.Black);
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
