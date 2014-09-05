using hipda.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace hipda.Settings
{
    public static class ThemeSettings
    {
        private static string keyNameOfThemeSettingsContainer = "ThemeSettingsContainer";
        private static string keyNameOfDefaultThemeId = "DefaultThemeId";
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static void SetThemeId(int argThemeId)
        {
            ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfThemeSettingsContainer, ApplicationDataCreateDisposition.Always);
            var themeSettingsContainer = localSettings.Containers[keyNameOfThemeSettingsContainer];
            themeSettingsContainer.Values[keyNameOfDefaultThemeId] = argThemeId;
        }

        public static int GetThemeId
        {
            get
            {
                ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfThemeSettingsContainer, ApplicationDataCreateDisposition.Always);
                var themeSettingsContainer = localSettings.Containers[keyNameOfThemeSettingsContainer];
                return Convert.ToInt16(themeSettingsContainer.Values[keyNameOfDefaultThemeId]);
            }
        }
    }
}
