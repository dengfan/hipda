using hipda.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace hipda.Settings
{
    public static class SortSettings
    {
        private static string sortTypeContainerKeyName = "SortSettingsContainer";
        private static string defaultSortKeyName = "DefaultSort";
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static void Toggle()
        {
            ApplicationDataContainer container = localSettings.CreateContainer(sortTypeContainerKeyName, ApplicationDataCreateDisposition.Always);
            var sortTypeContainer = localSettings.Containers[sortTypeContainerKeyName];
            if (!sortTypeContainer.Values.ContainsKey(defaultSortKeyName))
            {
                sortTypeContainer.Values[defaultSortKeyName] = DataSource.OrderType;
            }

            if (sortTypeContainer.Values[defaultSortKeyName].ToString().Equals("1"))
            {
                DataSource.OrderType = 2;
            }
            else
            {
                DataSource.OrderType = 1;
            }

            sortTypeContainer.Values[defaultSortKeyName] = DataSource.OrderType;
        }

        public static int GetSortType
        {
            get
            {
                ApplicationDataContainer container = localSettings.CreateContainer(sortTypeContainerKeyName, ApplicationDataCreateDisposition.Always);
                var sortTypeContainer = localSettings.Containers[sortTypeContainerKeyName];
                if (!sortTypeContainer.Values.ContainsKey(defaultSortKeyName))
                {
                    sortTypeContainer.Values[defaultSortKeyName] = DataSource.OrderType;
                }

                return Convert.ToInt16(sortTypeContainer.Values[defaultSortKeyName]);
            }
        }
    }
}
