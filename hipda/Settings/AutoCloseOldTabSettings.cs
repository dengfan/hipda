using hipda.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace hipda.Settings
{
    public static class AutoCloseOldTabSettings
    {
        private static string keyNameOfAutoCloseOldTabSettingsContainer = "AutoCloseOldTabSettingsContainer";
        private static string keyNameOfDefaultAutoCloseOldTabId = "DefaultAutoCloseOldTabType";
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static void SetValue(int argAutoCloseOldTabType)
        {
            ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfAutoCloseOldTabSettingsContainer, ApplicationDataCreateDisposition.Always);
            var settingsContainer = localSettings.Containers[keyNameOfAutoCloseOldTabSettingsContainer];
            settingsContainer.Values[keyNameOfDefaultAutoCloseOldTabId] = argAutoCloseOldTabType;
        }

        public static int GetValue
        {
            get
            {
                ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfAutoCloseOldTabSettingsContainer, ApplicationDataCreateDisposition.Always);
                var settingsContainer = localSettings.Containers[keyNameOfAutoCloseOldTabSettingsContainer];
                if (settingsContainer.Values.ContainsKey(keyNameOfDefaultAutoCloseOldTabId))
                {
                    return Convert.ToInt16(settingsContainer.Values[keyNameOfDefaultAutoCloseOldTabId]);
                }

                return 0;
            }
        }
    }
}
