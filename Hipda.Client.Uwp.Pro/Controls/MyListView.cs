using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Hipda.Client.Uwp.Pro.Controls
{
    public class MyListView : ListView
    {


        public IList<object> SelectedUserMessageListItems
        {
            get { return (IList<object>)GetValue(SelectedUserMessageListItemsProperty); }
            set { SetValue(SelectedUserMessageListItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedUserMessageListItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedUserMessageListItemsProperty =
            DependencyProperty.Register("SelectedUserMessageListItems", typeof(IList<object>), typeof(MyListView), new PropertyMetadata(null));


    }
}
