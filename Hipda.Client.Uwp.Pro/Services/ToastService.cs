using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Hipda.Client.Uwp.Pro.Services
{
    public static class ToastService
    {
        public static List<string> GetNoticeToastTempData()
        {
            string _containerKey = "HIPDA";
            string _dataKey = "NoticeToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);
            if (_container.Values.ContainsKey(_dataKey))
            {
                return _container.Values[_dataKey].ToString().Split(',').ToList();
            }

            return null;
        }

        public static void ClearNoticeToastTempData()
        {
            string _containerKey = "HIPDA";
            string _dataKey = "NoticeToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);
            _container.Values.Remove(_dataKey);

            UpdateBadge();
        }

        public static void ClearPmToastTempData(int userId)
        {
            string _containerKey = "HIPDA";
            string _dataKey = "PmToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);
            string value = _container.Values[_dataKey]?.ToString();
            if (!string.IsNullOrEmpty(value))
            {
                var list = value.Split(',').ToList();
                list.RemoveAll(u => u.Equals(userId.ToString()));
                _container.Values[_dataKey] = string.Join(",", list);

                UpdateBadge();
            }
        }

        public static int GetNoticeCountFromNoticeToastTempData()
        {
            string _containerKey = "HIPDA";
            string _dataKey = "NoticeToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);
            if (_container.Values.ContainsKey(_dataKey))
            {
                return _container.Values[_dataKey].ToString().Split(',').ToList().Count(i => i.Length > 0);
            }

            return 0;
        }

        public static int GetPmCountFromPmToastTempData()
        {
            string _containerKey = "HIPDA";
            string _dataKey = "PmToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);
            if (_container.Values.ContainsKey(_dataKey))
            {
                return _container.Values[_dataKey].ToString().Split(',').ToList().Count(i => i.Length > 0);
            }

            return 0;
        }

        public static async void UpdateBadge(int count)
        {
            await Task.Run(() => {
                Debug.WriteLine("更新 badge 数量 开始");
                XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
                XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
                badgeElement.SetAttribute("value", count.ToString());
                BadgeNotification badgeNotification = new BadgeNotification(badgeXml);
                BadgeUpdater badgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();
                badgeUpdater.Update(badgeNotification);
                Debug.WriteLine(string.Format("更新 badge 数量 {0} 结束", count));
            });
        }

        public static void UpdateBadge()
        {
            Debug.WriteLine("更新 badge 数量 开始");
            int count = GetNoticeCountFromNoticeToastTempData();
            count += GetPmCountFromPmToastTempData();

            UpdateBadge(count);
        }
    }
}
