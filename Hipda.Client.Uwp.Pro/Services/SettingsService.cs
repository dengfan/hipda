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
        static ApplicationDataContainer _container = ApplicationData.Current.LocalSettings.CreateContainer("Common", ApplicationDataCreateDisposition.Always);
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

                if (ThemeType == 0)
                {
                    // 在打开APP为light主题的情况下，主值为1时，不会联动更新副值，故需要在此先为副值赋于真实的值，以免使用初始值。
                    _myLocalSettings.PictureOpacityBak = (double)_container.Values["PictureOpacity"];

                    return 1D;
                }

                return (double)_container.Values["PictureOpacity"];
            }
            set
            {
                if (value < 1)
                {
                    _container.Values["PictureOpacity"] = value;
                }
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
                _container.Values["CanShowTopThread"] = value;
            }
        }

        public void ReadAndUpdate()
        {
            _myLocalSettings.ThemeType = ThemeType;
            _myLocalSettings.FontSize1 = FontSize1;
            _myLocalSettings.FontSize2 = FontSize2;
            _myLocalSettings.LineHeight = LineHeight;
            _myLocalSettings.PictureOpacity = PictureOpacity;
            _myLocalSettings.CanShowTopThread = CanShowTopThread;
        }

        public void Save()
        {
            ThemeType = _myLocalSettings.ThemeType;
            FontSize1 = _myLocalSettings.FontSize1;
            FontSize2 = _myLocalSettings.FontSize2;
            LineHeight = _myLocalSettings.LineHeight;
            PictureOpacity = _myLocalSettings.PictureOpacityBak;
            CanShowTopThread = _myLocalSettings.CanShowTopThread;
        }
    }

    public class RoamingSettingsService
    {
        static ApplicationDataContainer _container = ApplicationData.Current.RoamingSettings;
        static ApplicationDataContainer _blockUsersContainer = _container.CreateContainer("BlockUsers", ApplicationDataCreateDisposition.Always);
        static ApplicationDataContainer _blockThreadsContainer = _container.CreateContainer("BlockThreads", ApplicationDataCreateDisposition.Always);
        static RoamingSettingsDependencyObject _myRoamingSettings = (RoamingSettingsDependencyObject)App.Current.Resources["MyRoamingSettings"];

        public static void ReadAndUpdate()
        {
            var data = new RoamingSettingsModel();

            var blockUserItems = _blockUsersContainer.Values;
            foreach (var item in blockUserItems)
            {
                string jsonStr = item.Value.ToString();
                var bu = JsonConvert.DeserializeObject<BlockUser>(jsonStr);
                data.BlockUsers.Add(bu);
            }
            _myRoamingSettings.BlockUsers = data.BlockUsers;

            var blockThreadItems = _blockThreadsContainer.Values;
            foreach (var item in blockThreadItems)
            {
                string jsonStr = item.Value.ToString();
                var bt = JsonConvert.DeserializeObject<BlockThread>(jsonStr);
                data.BlockThreads.Add(bt);
            }
            _myRoamingSettings.BlockThreads = data.BlockThreads;
        }

        public static void Save()
        {
            foreach (var item in _myRoamingSettings.BlockUsers)
            {
                string jsonStr = JsonConvert.SerializeObject(item);
                _blockUsersContainer.Values[$"{item.UserId}@{item.ForumId}"] = jsonStr;
            }

            foreach (var item in _myRoamingSettings.BlockThreads)
            {
                string jsonStr = JsonConvert.SerializeObject(item);
                _blockThreadsContainer.Values[$"{item.ThreadId}"] = jsonStr;
            }
        }

        public static void UnblockUsers(List<string> UnblockUserKeys)
        {
            foreach (string key in UnblockUserKeys)
            {
                if (_blockUsersContainer.Values[key] != null)
                {
                    _blockUsersContainer.Values.Remove(key);
                }
            }

            ReadAndUpdate();
        }

        public static void UnblockThreads(List<string> UnblockThreadKeys)
        {
            foreach (string key in UnblockThreadKeys)
            {
                if (_blockThreadsContainer.Values[key] != null)
                {
                    _blockThreadsContainer.Values.Remove(key);
                }
            }

            ReadAndUpdate();
        }
    }
}
