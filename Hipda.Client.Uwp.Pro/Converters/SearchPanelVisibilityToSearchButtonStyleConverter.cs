using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.Converters
{
    public class SearchPanelVisibilityToSearchButtonStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Visibility val = (Visibility)value;
            if (val == Visibility.Visible)
            {
                return App.Current.Resources["SearchButtonSelectedStyle"];
            }

            return App.Current.Resources["SearchButtonUnselectedStyle"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
