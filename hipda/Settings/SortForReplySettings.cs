using HipdaUwpLite.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace HipdaUwpLite.Settings
{
    public static class SortForReplySettings
    {
        private static string keyNameOfSortTypeContainer = "SortForReplySettingsContainer";
        private static string keyNameOfDefaultSort = "DefaultSortForReply";
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static void Toggle()
        {
            ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfSortTypeContainer, ApplicationDataCreateDisposition.Always);
            var sortTypeContainer = localSettings.Containers[keyNameOfSortTypeContainer];
            if (!sortTypeContainer.Values.ContainsKey(keyNameOfDefaultSort))
            {
                sortTypeContainer.Values[keyNameOfDefaultSort] = DataSource.RelayListPageOrderType;
            }

            if (sortTypeContainer.Values[keyNameOfDefaultSort].ToString().Equals("1"))
            {
                DataSource.RelayListPageOrderType = 2;
            }
            else
            {
                DataSource.RelayListPageOrderType = 1;
            }

            sortTypeContainer.Values[keyNameOfDefaultSort] = DataSource.RelayListPageOrderType;
        }

        public static int GetSortType
        {
            get
            {
                ApplicationDataContainer container = localSettings.CreateContainer(keyNameOfSortTypeContainer, ApplicationDataCreateDisposition.Always);
                var sortTypeContainer = localSettings.Containers[keyNameOfSortTypeContainer];
                if (!sortTypeContainer.Values.ContainsKey(keyNameOfDefaultSort))
                {
                    sortTypeContainer.Values[keyNameOfDefaultSort] = DataSource.RelayListPageOrderType;
                }

                return Convert.ToInt16(sortTypeContainer.Values[keyNameOfDefaultSort]);
            }
        }
    }
}
