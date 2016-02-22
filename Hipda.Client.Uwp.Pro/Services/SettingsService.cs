using Hipda.Client.Uwp.Pro.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.Services
{
    public class LocalSettingsService
    {
        static ApplicationDataContainer _commonContainer = ApplicationData.Current.LocalSettings.CreateContainer("Common", ApplicationDataCreateDisposition.Always);
        static LocalSettingsDependencyObject _myLocalSettings = (LocalSettingsDependencyObject)App.Current.Resources["MyLocalSettings"];

        public int ThemeType
        {
            get
            {
                if (_commonContainer.Values["ThemeType"] == null)
                {
                    _commonContainer.Values["ThemeType"] = _myLocalSettings.ThemeType;
                }

                return (int)_commonContainer.Values["ThemeType"];
            }
            set
            {
                _commonContainer.Values["ThemeType"] = value;
            }
        }

        public double FontSize1
        {
            get
            {
                if (_commonContainer.Values["FontSize1"] == null)
                {
                    _commonContainer.Values["FontSize1"] = _myLocalSettings.FontSize1;
                }

                return (double)_commonContainer.Values["FontSize1"];
            }
            set
            {
                _commonContainer.Values["FontSize1"] = value;
            }
        }

        public double FontSize2
        {
            get
            {
                if (_commonContainer.Values["FontSize2"] == null)
                {
                    _commonContainer.Values["FontSize2"] = _myLocalSettings.FontSize2;
                }

                return (double)_commonContainer.Values["FontSize2"];
            }
            set
            {
                _commonContainer.Values["FontSize2"] = value;
            }
        }

        public double LineHeight
        {
            get
            {
                if (_commonContainer.Values["LineHeight"] == null)
                {
                    _commonContainer.Values["LineHeight"] = _myLocalSettings.LineHeight;
                }

                return (double)_commonContainer.Values["LineHeight"];
            }
            set
            {
                _commonContainer.Values["LineHeight"] = value;
            }
        }

        public double PictureOpacity
        {
            get
            {
                if (_commonContainer.Values["PictureOpacity"] == null)
                {
                    _commonContainer.Values["PictureOpacity"] = _myLocalSettings.PictureOpacity;
                }

                if (ThemeType == 0)
                {
                    // 在打开APP为light主题的情况下，主值为1时，不会联动更新副值，故需要在此先为副值赋于真实的值，以免使用初始值。
                    _myLocalSettings.PictureOpacityBak = (double)_commonContainer.Values["PictureOpacity"];

                    return 1D;
                }

                return (double)_commonContainer.Values["PictureOpacity"];
            }
            set
            {
                if (value < 1)
                {
                    _commonContainer.Values["PictureOpacity"] = value;
                }
            }
        }

        public int FontContrastRatio
        {
            get
            {
                if (_commonContainer.Values["FontContrastRatio"] == null)
                {
                    _commonContainer.Values["FontContrastRatio"] = _myLocalSettings.FontContrastRatio;
                }

                return (int)_commonContainer.Values["FontContrastRatio"];
            }
            set
            {
                _commonContainer.Values["FontContrastRatio"] = value;
            }
        }

        public void ReadAndUpdate()
        {
            _myLocalSettings.ThemeType = ThemeType;
            _myLocalSettings.FontSize1 = FontSize1;
            _myLocalSettings.FontSize2 = FontSize2;
            _myLocalSettings.LineHeight = LineHeight;
            _myLocalSettings.PictureOpacity = PictureOpacity;
            _myLocalSettings.FontContrastRatio = FontContrastRatio;
        }

        public void Save()
        {
            ThemeType = _myLocalSettings.ThemeType;
            FontSize1 = _myLocalSettings.FontSize1;
            FontSize2 = _myLocalSettings.FontSize2;
            LineHeight = _myLocalSettings.LineHeight;
            PictureOpacity = _myLocalSettings.PictureOpacityBak;
            FontContrastRatio = _myLocalSettings.FontContrastRatio;
        }
    }

    public class RoamingSettingsService
    {
        static ApplicationDataContainer _container = ApplicationData.Current.RoamingSettings;
        static ApplicationDataContainer _commonContainer = _container.CreateContainer("Common", ApplicationDataCreateDisposition.Always);
        static ApplicationDataContainer _blockUsersContainer = _container.CreateContainer("BlockUsers", ApplicationDataCreateDisposition.Always);
        static ApplicationDataContainer _blockThreadsContainer = _container.CreateContainer("BlockThreads", ApplicationDataCreateDisposition.Always);
        static RoamingSettingsDependencyObject _myRoamingSettings = (RoamingSettingsDependencyObject)App.Current.Resources["MyRoamingSettings"];

        public bool CanShowTopThread
        {
            get
            {
                if (_commonContainer.Values["CanShowTopThread"] == null)
                {
                    _commonContainer.Values["CanShowTopThread"] = _myRoamingSettings.CanShowTopThread;
                }

                return (bool)_commonContainer.Values["CanShowTopThread"];
            }
            set
            {
                _commonContainer.Values["CanShowTopThread"] = value;
            }
        }

        public void ReadAndUpdate()
        {
            var data = new RoamingSettingsModel();

            _myRoamingSettings.CanShowTopThread = CanShowTopThread;

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

        public void Save()
        {
            CanShowTopThread = _myRoamingSettings.CanShowTopThread;
        }

        public void SaveBlockUsers()
        {
            foreach (var item in _myRoamingSettings.BlockUsers)
            {
                string jsonStr = JsonConvert.SerializeObject(item);
                _blockUsersContainer.Values[$"{item.UserId}@{item.ForumId}"] = jsonStr;
            }
        }

        public void SaveBlockThreads()
        {
            foreach (var item in _myRoamingSettings.BlockThreads)
            {
                string jsonStr = JsonConvert.SerializeObject(item);
                _blockThreadsContainer.Values[$"{item.ThreadId}"] = jsonStr;
            }
        }

        public void UnblockUsers(List<string> UnblockUserKeys)
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

        public void UnblockThreads(List<string> UnblockThreadKeys)
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


        public static bool? GetOrderByDateline(int forumId)
        {
            string key = $"{forumId}IsOrderByDateline";

            if (_commonContainer.Values[key] == null)
            {
                return false;
            }

            return (bool)_commonContainer.Values[key];
        }

        public static void SetOrderByDateline(int forumId, bool isOrderByDateline)
        {
            string key = $"{forumId}IsOrderByDateline";

            if (_commonContainer.Values[key] == null)
            {
                _commonContainer.Values.Add(key, isOrderByDateline);
            }
            else
            {
                _commonContainer.Values[key] = isOrderByDateline;
            }
        }
    }
}
