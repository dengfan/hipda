using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Converters
{
    public class ByteToMegaByteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ulong val = (ulong)value;
            if (val >= 1073741824) // 如果大于1G
            {
                double g = (double)val / 1024 / 1024 / 1024;
                return string.Format("{0:f2}G", g);
            }
            else
            {
                double m = (double)val / 1024 / 1024;
                return string.Format("{0:f2}M", m);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
