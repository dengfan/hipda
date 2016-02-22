using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.Converters
{
    public class FontContrastRatioToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (int)value == 0 ? (SolidColorBrush)App.Current.Resources["SystemControlForegroundBaseMediumBrush"] : (SolidColorBrush)App.Current.Resources["SystemControlForegroundBaseHighBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
