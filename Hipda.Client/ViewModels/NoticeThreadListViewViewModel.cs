using Hipda.Client.Commands;
using Hipda.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.ViewModels
{
    public class NoticeThreadListViewViewModel
    {
        ListView _leftListView;
        CommandBar _leftCommandBar;
        NoticeService _ds;

        public DelegateCommand RefreshThreadCommand { get; set; }

        public NoticeThreadListViewViewModel(ListView leftListView, CommandBar leftCommandBar, Action beforeLoad, Action afterLoad, Action noDataNotice)
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

            _ds = new NoticeService();

            LoadData();

            RefreshThreadCommand = new DelegateCommand();
            RefreshThreadCommand.ExecuteAction = (p) =>
            {
                LoadData();
            };

            var RefreshButton = new AppBarButton { Icon = new SymbolIcon(Symbol.Refresh), Label = "刷新" };
            RefreshButton.Command = RefreshThreadCommand;

            _leftCommandBar.PrimaryCommands.Add(RefreshButton);
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
