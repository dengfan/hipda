using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Converters
{
    class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b1 = (SolidColorBrush)App.Current.Resources["SystemControlBackgroundAccentBrush"];
            var b2 = (SolidColorBrush)App.Current.Resources["SystemControlHighlightListAccentMediumBrush"];
            return (bool)value ?  b1 : b2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
