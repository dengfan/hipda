using hipda.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace hipda.Settings
{
    public static class LayoutModeSettings
    {
        private static string keyNameOfLayoutModeSettingsContainer = "LayoutModeSettingsContainer";
        private static string keyNameOfDefaultLayoutModeId = "DefaultLayoutModeModeId";
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static void SetLayoutModeId(int argLayoutModeId)
        {
            ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfLayoutModeSettingsContainer, ApplicationDataCreateDisposition.Always);
            var settingsContainer = localSettings.Containers[keyNameOfLayoutModeSettingsContainer];
            settingsContainer.Values[keyNameOfDefaultLayoutModeId] = argLayoutModeId;
        }

        public static int GetLayoutModeId
        {
            get
            {
                ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfLayoutModeSettingsContainer, ApplicationDataCreateDisposition.Always);
                var settingsContainer = localSettings.Containers[keyNameOfLayoutModeSettingsContainer];
                if (settingsContainer.Values.ContainsKey(keyNameOfDefaultLayoutModeId))
                {
                    return Convert.ToInt16(settingsContainer.Values[keyNameOfDefaultLayoutModeId]);
                }

                return 0;
            }
        }
    }
}
