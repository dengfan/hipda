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
        private static string sortTypeContainerKeyName = "sortTypeContainer";
        private static string sortTypeDataKeyName = "sortTypeData";
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static void Toggle()
        {
            ApplicationDataContainer container = localSettings.CreateContainer(sortTypeContainerKeyName, ApplicationDataCreateDisposition.Always);
            var sortTypeContainer = localSettings.Containers[sortTypeContainerKeyName];
            if (!sortTypeContainer.Values.ContainsKey(sortTypeDataKeyName))
            {
                sortTypeContainer.Values[sortTypeDataKeyName] = DataSource.OrderType;
            }

            if (sortTypeContainer.Values[sortTypeDataKeyName].ToString().Equals("1"))
            {
                DataSource.OrderType = 2;
            }
            else
            {
                DataSource.OrderType = 1;
            }

            sortTypeContainer.Values[sortTypeDataKeyName] = DataSource.OrderType;
        }

        public static int GetSortType
        {
            get
            {
                ApplicationDataContainer container = localSettings.CreateContainer(sortTypeContainerKeyName, ApplicationDataCreateDisposition.Always);
                var sortTypeContainer = localSettings.Containers[sortTypeContainerKeyName];
                if (!sortTypeContainer.Values.ContainsKey(sortTypeDataKeyName))
                {
                    sortTypeContainer.Values[sortTypeDataKeyName] = DataSource.OrderType;
                }

                return Convert.ToInt16(sortTypeContainer.Values[sortTypeDataKeyName]);
            }
        }
    }
}
