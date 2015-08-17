using HipdaUwpLite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace HipdaUwpLite.Settings
{
    public static class ThemeSettings
    {
        private static string keyNameOfThemeSettingsContainer = "ThemeSettingsContainer";
        private static string keyNameOfDefaultThemeId = "DefaultThemeId";
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static int ThemeSetting
        {
            set
            {
                ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfThemeSettingsContainer, ApplicationDataCreateDisposition.Always);
                var themeSettingsContainer = localSettings.Containers[keyNameOfThemeSettingsContainer];
                themeSettingsContainer.Values[keyNameOfDefaultThemeId] = value;
            }
            get
            {
                ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfThemeSettingsContainer, ApplicationDataCreateDisposition.Always);
                var themeSettingsContainer = localSettings.Containers[keyNameOfThemeSettingsContainer];
                return Convert.ToInt16(themeSettingsContainer.Values[keyNameOfDefaultThemeId]);
            }
        }
    }
}
