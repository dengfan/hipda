﻿using hipda.Data;
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
    public class BackgroundStyleSelecterForReplyItem : StyleSelector
    {
        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            //Reply listViewItem = (Reply)item;
            //SolidColorBrush background = ((listViewItem.Floor % 2) == 0 ? new SolidColorBrush(Colors.WhiteSmoke) : new SolidColorBrush(Colors.White));
            Style style = new Style(typeof(ListViewItem));
            //style.Setters.Add(new Setter(ListViewItem.HorizontalContentAlignmentProperty, "Stretch"));
            style.Setters.Add(new Setter(ListViewItem.MarginProperty, "0,0,0,15"));

            ControlTemplate listViewItem1 = (ControlTemplate)App.Current.Resources["ListViewItem1"];
            style.Setters.Add(new Setter(ListViewItem.TemplateProperty, listViewItem1));
            return style;
        }
    }
}
