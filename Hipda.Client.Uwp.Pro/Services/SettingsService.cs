using Hipda.Client.Uwp.Pro.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Hipda.Client.Uwp.Pro.Services
{
    public static class SettingsService
    {
        static string _containerKey = "HIPDA";
        static string _dataKey = "SettingsData";
        static ApplicationDataContainer _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);

        public static SettingsModel Read()
        {
            if (_container.Values.ContainsKey(_dataKey))
            {
                string jsonStr = _container.Values[_dataKey].ToString();
                return JsonConvert.DeserializeObject<SettingsModel>(jsonStr);
            }

            return new SettingsModel();
        }

        public static void Save(SettingsModel settingsData)
        {
            string jsonStr = JsonConvert.SerializeObject(settingsData);
            _container.Values[_dataKey] = jsonStr;
        }
    }
}
