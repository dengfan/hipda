using HipdaUwpLite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace HipdaUwpLite.Settings
{
    public static class SortForThreadSettings
    {
        private static string keyNameOfSortTypeContainer = "SortForThreadSettingsContainer";
        private static string keyNameOfDefaultSort = "DefaultSortForThread";
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static void Toggle()
        {
            ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfSortTypeContainer, ApplicationDataCreateDisposition.Always);
            var sortTypeContainer = localSettings.Containers[keyNameOfSortTypeContainer];
            if (!sortTypeContainer.Values.ContainsKey(keyNameOfDefaultSort))
            {
                sortTypeContainer.Values[keyNameOfDefaultSort] = DataSource.ThreadListPageOrderBy;
            }

            if (sortTypeContainer.Values[keyNameOfDefaultSort].ToString().ToLower().Equals("dateline"))
            {
                DataSource.ThreadListPageOrderBy = string.Empty;
            }
            else
            {
                DataSource.ThreadListPageOrderBy = "dateline";
            }

            sortTypeContainer.Values[keyNameOfDefaultSort] = DataSource.ThreadListPageOrderBy;
        }

        public static string GetSortType
        {
            get
            {
                ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfSortTypeContainer, ApplicationDataCreateDisposition.Always);
                var sortTypeContainer = localSettings.Containers[keyNameOfSortTypeContainer];
                if (!sortTypeContainer.Values.ContainsKey(keyNameOfDefaultSort))
                {
                    sortTypeContainer.Values[keyNameOfDefaultSort] = DataSource.ThreadListPageOrderBy;
                }

                return sortTypeContainer.Values[keyNameOfDefaultSort].ToString();
            }
        }
    }
}
