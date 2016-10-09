using GalaSoft.MvvmLight.Command;
using Hipda.Client.Services;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Hipda.Client.ViewModels
{
    public class NoticeThreadListViewViewModel
    {
        ListView _leftListView;
        CommandBar _leftCommandBar;
        NoticeService _ds;

        public ICommand RefreshThreadCommand { get; set; }

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

            RefreshThreadCommand = new RelayCommand(() =>
            {
                LoadData();
            });

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
