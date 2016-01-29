using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.Converters
{
    public class ThemeTypeToRequestedThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var _myLocalSettings = ((LocalSettingsDependencyObject)App.Current.Resources["MyLocalSettings"]);
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            int themeType = (int)value;
            if (themeType == 1)
            {
                Color c = Colors.Black;
                titleBar.BackgroundColor = c;
                titleBar.InactiveBackgroundColor = c;
                titleBar.ForegroundColor = Colors.Silver;
                titleBar.ButtonBackgroundColor = c;
                titleBar.ButtonInactiveBackgroundColor = c;
                titleBar.ButtonForegroundColor = Colors.Silver;
                titleBar.ButtonHoverBackgroundColor = Colors.DimGray;

                _myLocalSettings.PictureOpacity = new LocalSettingsService().PictureOpacity;

                return ElementTheme.Dark;
            }
            else
            {
                titleBar.BackgroundColor = null;
                titleBar.InactiveBackgroundColor = null;
                titleBar.ForegroundColor = null;
                titleBar.ButtonBackgroundColor = null;
                titleBar.ButtonInactiveBackgroundColor = null;
                titleBar.ButtonForegroundColor = null;
                titleBar.ButtonHoverBackgroundColor = null;

                _myLocalSettings.PictureOpacity = 1;

                return ElementTheme.Light;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
