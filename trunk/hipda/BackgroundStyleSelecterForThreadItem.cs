using hipda.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace hipda
{
    public class BackgroundStyleSelecterForThreadItem : StyleSelector
    {
        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            //Thread listViewItem = (Thread)item;
            //SolidColorBrush background = ((listViewItem.Index % 2) == 0 ? new SolidColorBrush(Colors.AliceBlue) : new SolidColorBrush(Colors.WhiteSmoke));
            Style style = new Style(typeof(ListViewItem));
            //style.Setters.Add(new Setter(ListViewItem.BackgroundProperty, "White"));
            style.Setters.Add(new Setter(ListViewItem.HorizontalContentAlignmentProperty, "Stretch"));
            style.Setters.Add(new Setter(ListViewItem.MarginProperty, "10,0,10,15"));
            return style;
        }
    }
}
