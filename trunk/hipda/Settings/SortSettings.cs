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
        private static string keyNameOfSortTypeContainer = "SortSettingsContainer";
        private static string keyNameOfDefaultSort = "DefaultSort";
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static void Toggle()
        {
            ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfSortTypeContainer, ApplicationDataCreateDisposition.Always);
            var sortTypeContainer = localSettings.Containers[keyNameOfSortTypeContainer];
            if (!sortTypeContainer.Values.ContainsKey(keyNameOfDefaultSort))
            {
                sortTypeContainer.Values[keyNameOfDefaultSort] = DataSource.OrderType;
            }

            if (sortTypeContainer.Values[keyNameOfDefaultSort].ToString().Equals("1"))
            {
                DataSource.OrderType = 2;
            }
            else
            {
                DataSource.OrderType = 1;
            }

            sortTypeContainer.Values[keyNameOfDefaultSort] = DataSource.OrderType;
        }

        public static int GetSortType
        {
            get
            {
                ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfSortTypeContainer, ApplicationDataCreateDisposition.Always);
                var sortTypeContainer = localSettings.Containers[keyNameOfSortTypeContainer];
                if (!sortTypeContainer.Values.ContainsKey(keyNameOfDefaultSort))
                {
                    sortTypeContainer.Values[keyNameOfDefaultSort] = DataSource.OrderType;
                }

                return Convert.ToInt16(sortTypeContainer.Values[keyNameOfDefaultSort]);
            }
        }
    }
}
