using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.Converters
{
    public class CountdownToSendDialogTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = (int)value;
            if (val > 0)
            {
                return $"{parameter.ToString()} （倒计时{val}秒）";
            }

            return parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
