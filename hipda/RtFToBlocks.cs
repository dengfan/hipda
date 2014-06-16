using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using System.Threading.Tasks;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Controls;

namespace hipda
{
    public class RtFToBlocks : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string rtfText = (string)value;
            object blocksObj = XamlReader.Load(rtfText);
            return (RichTextBlock)blocksObj;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
}
