using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.Services
{
    public static class Common
    {
        public static async void PostEmailForConvertReplyContent(int threadId, int floorNo)
        {
            string errorReport = string.Format("http://www.hi-pda.com/forum/viewthread.php?tid={0} 楼层{1}内容解析出错。", threadId, floorNo);
            Uri uri = new Uri("mailto:appxking@outlook.com?subject=发送出错信息给开发者，以帮助开发者更好的解决问题&body=" + errorReport, UriKind.Absolute);
            await Launcher.LaunchUriAsync(uri);
        }

        public static T FindParent<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);
            if (parent == null) return null;
            var parentT = parent as T;
            return parentT ?? FindParent<T>(parent);
        }
    }
}
