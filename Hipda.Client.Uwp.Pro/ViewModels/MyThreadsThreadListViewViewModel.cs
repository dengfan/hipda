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
    public class MyThreadsThreadListViewViewModel
    {
        int _startPageNo = 1;
        ListView _leftListView;
        CommandBar _leftCommandBar;
        Action _beforeLoad;
        Action _afterLoad;
        Action _noDataNotice;
        MyThreadsService _ds;

        public int ThreadMaxPageNo { get; set; }

        public DelegateCommand RefreshThreadCommand { get; set; }

        public MyThreadsThreadListViewViewModel(int pageNo, ListView leftListView, CommandBar leftCommandBar, Action beforeLoad, Action afterLoad, Action noDataNotice)
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
            _ds = new MyThreadsService();

            LoadDataForMyThreads(pageNo);

            var RefreshThreadCommand = new DelegateCommand();
            RefreshThreadCommand.ExecuteAction = (p) => {
                _ds.ClearThreadDataForMyThreads();
                LoadDataForMyThreads(1);
            };

            var btnRefresh = new AppBarButton { Icon = new FontIcon { Glyph = "\uE895" }, Label = "刷新" };
            btnRefresh.Command = RefreshThreadCommand;
            _leftCommandBar.PrimaryCommands.Add(btnRefresh);
        }

        void LoadDataForMyThreads(int pageNo)
        {
            var cv = _ds.GetViewForThreadPageForMyThreads(pageNo, _beforeLoad, _afterLoad, _noDataNotice);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNoForMyThreads();
                _startPageNo = pageNo;
                _leftListView.ItemsSource = cv;
            }
        }

        public void LoadPrevPageData()
        {
            if (_startPageNo > 1)
            {
                _ds.ClearThreadDataForMyThreads();
                LoadDataForMyThreads(_startPageNo);
            }
        }
    }
}
