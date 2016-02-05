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
        DataService _ds;

        public DelegateCommand RefreshThreadCommand { get; set; }

        public ThreadListViewForNoticeViewModel(ListView leftListView, CommandBar leftCommandBar, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            _leftListView = leftListView;
            _leftListView.SelectionMode = ListViewSelectionMode.Single;
            _leftListView.ItemsSource = null;
            _leftListView.ItemTemplateSelector = (DataTemplateSelector)App.Current.Resources["NoticeListItemTemplateSelector"];
            _leftListView.ItemContainerStyle = (Style)App.Current.Resources["NoticeItemContainerStyle"];

            _leftCommandBar = leftCommandBar;
            _leftCommandBar.Visibility = Visibility.Visible;
            _leftCommandBar.PrimaryCommands.Clear();
            _leftCommandBar.SecondaryCommands.Clear();

            _ds = new DataService();

            LoadData();



            var btnRefreshForNotice = new AppBarButton { Icon = new SymbolIcon(Symbol.Refresh), Label = "刷新" };
            btnRefreshForNotice.Tapped += (s, e) => {

            };

            _leftCommandBar.PrimaryCommands.Add(btnRefreshForNotice);
        }

        async void LoadData()
        {
            var data = await _ds.GetNoticeData();
            if (data != null)
            {
                _leftListView.ItemsSource = data;
            }
        }
    }
}
