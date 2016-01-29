using Hipda.Client.Uwp.Pro.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Hipda.Client.Uwp.Pro.Services
{
    public class LocalSettingsService
    {
        static string _containerKey = "Common";
        static ApplicationDataContainer _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);
        static LocalSettingsDependencyObject _myLocalSettings = (LocalSettingsDependencyObject)App.Current.Resources["MyLocalSettings"];

        public int ThemeType
        {
            get
            {
                if (_container.Values["ThemeType"] == null)
                {
                    _container.Values["ThemeType"] = _myLocalSettings.ThemeType;
                }

                return (int)_container.Values["ThemeType"];
            }
            set
            {
                _myLocalSettings.ThemeType = value;
                _container.Values["ThemeType"] = value;
            }
        }

        public double FontSize1
        {
            get
            {
                if (_container.Values["FontSize1"] == null)
                {
                    _container.Values["FontSize1"] = _myLocalSettings.FontSize1;
                }

                return (double)_container.Values["FontSize1"];
            }
            set
            {
                _myLocalSettings.FontSize1 = value;
                _container.Values["FontSize1"] = value;
            }
        }

        public double FontSize2
        {
            get
            {
                if (_container.Values["FontSize2"] == null)
                {
                    _container.Values["FontSize2"] = _myLocalSettings.FontSize2;
                }

                return (double)_container.Values["FontSize2"];
            }
            set
            {
                _myLocalSettings.FontSize2 = value;
                _container.Values["FontSize2"] = value;
            }
        }

        public double LineHeight
        {
            get
            {
                if (_container.Values["LineHeight"] == null)
                {
                    _container.Values["LineHeight"] = _myLocalSettings.LineHeight;
                }

                return (double)_container.Values["LineHeight"];
            }
            set
            {
                _myLocalSettings.LineHeight = value;
                _container.Values["LineHeight"] = value;
            }
        }

        public double PictureOpacity
        {
            get
            {
                if (_container.Values["PictureOpacity"] == null)
                {
                    _container.Values["PictureOpacity"] = _myLocalSettings.PictureOpacity;
                }

                return (double)_container.Values["PictureOpacity"];
            }
            set
            {
                _myLocalSettings.PictureOpacity = value;
                _container.Values["PictureOpacity"] = value;
            }
        }

        public bool CanShowTopThread
        {
            get
            {
                if (_container.Values["CanShowTopThread"] == null)
                {
                    _container.Values["CanShowTopThread"] = _myLocalSettings.CanShowTopThread;
                }

                return (bool)_container.Values["CanShowTopThread"];
            }
            set
            {
                _myLocalSettings.CanShowTopThread = value;
                _container.Values["CanShowTopThread"] = value;
            }
        }

        public static void Save()
        {
            _container.Values["ThemeType"] = _myLocalSettings.ThemeType;
            _container.Values["FontSize1"] = _myLocalSettings.FontSize1;
            _container.Values["FontSize2"] = _myLocalSettings.FontSize2;
            _container.Values["LineHeight"] = _myLocalSettings.LineHeight;
            _container.Values["PictureOpacity"] = _myLocalSettings.PictureOpacity;
            _container.Values["CanShowTopThread"] = _myLocalSettings.CanShowTopThread;
        }
    }

    public static class RoamingSettingsService
    {
        static string _blockUsersContainerKey = "BlockUsers";
        static string _blockThreadsContainerKey = "BlockThreads";
        static ApplicationDataContainer _blockUsersContainer = ApplicationData.Current.RoamingSettings.CreateContainer(_blockUsersContainerKey, ApplicationDataCreateDisposition.Always);
        static ApplicationDataContainer _blockThreadsContainer = ApplicationData.Current.RoamingSettings.CreateContainer(_blockThreadsContainerKey, ApplicationDataCreateDisposition.Always);

        public static RoamingSettingsModel Read()
        {
            var data = new RoamingSettingsModel();

            var blockUserItems = _blockUsersContainer.Values;
            foreach (var item in blockUserItems)
            {
                string jsonStr = item.Value.ToString();
                var bu = JsonConvert.DeserializeObject<BlockUser>(jsonStr);
                data.BlockUsers.Add(bu);
            }

            var blockThreadItems = _blockThreadsContainer.Values;
            foreach (var item in blockThreadItems)
            {
                string jsonStr = item.Value.ToString();
                var bt = JsonConvert.DeserializeObject<BlockThread>(jsonStr);
                data.BlockThreads.Add(bt);
            }

            return data;
        }

        public static void Save()
        {
            var myRoamingSettings = (RoamingSettingsDependencyObject)App.Current.Resources["MyRoamingSettings"];
            if (myRoamingSettings == null)
            {
                return;
            }

            foreach (var item in myRoamingSettings.BlockUsers)
            {
                string jsonStr = JsonConvert.SerializeObject(item);
                _blockThreadsContainer.Values[item.UserId.ToString()] = jsonStr;
            }

            foreach (var item in myRoamingSettings.BlockThreads)
            {
                string jsonStr = JsonConvert.SerializeObject(item);
                _blockThreadsContainer.Values[item.ThreadId.ToString()] = jsonStr;
            }
        }
    }
}
