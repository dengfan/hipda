using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Hipda.Client.Uwp.Lite
{
    public class ListGroupStyleSelector : GroupStyleSelector
    {
        protected override GroupStyle SelectGroupStyleCore(object group, uint level)
        {
            return (GroupStyle)App.Current.Resources["listViewGroupStyle"];
        }
    }

}
