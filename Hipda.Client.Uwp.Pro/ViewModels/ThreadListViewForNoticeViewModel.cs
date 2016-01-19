using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadListViewForNoticeViewModel
    {
        ListView _leftListView;
        CommandBar _leftCommandBar;
        Action _beforeLoad;
        Action _afterLoad;
        Action _noDataNotice;
        DataService _ds;

        public int ThreadMaxPageNo { get; set; }

        public ThreadListViewForNoticeViewModel(ListView leftListView, CommandBar leftCommandBar, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            _leftListView = leftListView;
            _leftListView.SelectionMode = ListViewSelectionMode.Single;
            _leftListView.ItemsSource = null;
            _leftListView.ItemTemplateSelector = App.Current.Resources["threadListItemTemplateSelector"] as DataTemplateSelector;
            _leftListView.ItemContainerStyle = App.Current.Resources["ThreadItemContainerStyle"] as Style;

            _leftCommandBar = leftCommandBar;
            _leftCommandBar.Visibility = Visibility.Visible;
            _leftCommandBar.PrimaryCommands.Clear();
            _leftCommandBar.SecondaryCommands.Clear();

            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _noDataNotice = noDataNotice;
            _ds = new DataService();

            _leftListView.ItemTemplateSelector = App.Current.Resources["noticeListItemTemplateSelector"] as DataTemplateSelector;
            _leftListView.ItemContainerStyle = App.Current.Resources["NoticeItemContainerStyle"] as Style;

            var vm = new NoticePageViewModel();
            var b = new Binding { Source = vm, Path = new PropertyPath("NoticeData") };
            _leftListView.DataContext = vm;
            _leftListView.SetBinding(ListView.ItemsSourceProperty, b);

            var btnRefreshForNotice = new AppBarButton { Icon = new SymbolIcon(Symbol.Refresh), Label = "刷新" };
            btnRefreshForNotice.Tapped += (s, e) => {

            };

            _leftCommandBar.PrimaryCommands.Add(btnRefreshForNotice);
        }
    }
}
