using hipda.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace hipda.Settings
{
    public static class ImageCountSettings
    {
        private static string keyNameOfImageCountSettingsContainer = "ImageCountSettingsContainer";
        private static string keyNameOfDefaultImageCountId = "DefaultImageCountModeId";
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static int ImageCountSetting
        {
            set
            {
                ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfImageCountSettingsContainer, ApplicationDataCreateDisposition.Always);
                var settingsContainer = localSettings.Containers[keyNameOfImageCountSettingsContainer];
                settingsContainer.Values[keyNameOfDefaultImageCountId] = value;
            }
            get
            {
                ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfImageCountSettingsContainer, ApplicationDataCreateDisposition.Always);
                var settingsContainer = localSettings.Containers[keyNameOfImageCountSettingsContainer];
                if (settingsContainer.Values.ContainsKey(keyNameOfDefaultImageCountId))
                {
                    return Convert.ToInt16(settingsContainer.Values[keyNameOfDefaultImageCountId]);
                }

                return 5;
            }
        }
    }
}
