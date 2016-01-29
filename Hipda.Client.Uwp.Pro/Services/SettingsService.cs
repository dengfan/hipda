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
    public static class LocalSettingsService
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

        public static void Save()
        {
            var mySettings = (SettingsDependencyObject)App.Current.Resources["MySettings"];

            var data = new SettingsModel
            {
                ThemeType = mySettings.ThemeType,
                FontSize1 = mySettings.FontSize1,
                FontSize2 = mySettings.FontSize2,
                LineHeight = mySettings.LineHeight,
                PictureOpacity = mySettings.PictureOpacityBak,
                CanShowTopThread = mySettings.CanShowTopThread,
                BlockUsers = mySettings.BlockUsers,
                BlockThreads = mySettings.BlockThreads
            };

            string jsonStr = JsonConvert.SerializeObject(data);
            _container.Values[_dataKey] = jsonStr;
        }
    }
}
