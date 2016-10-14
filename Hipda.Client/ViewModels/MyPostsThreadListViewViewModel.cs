using Hipda.Client.Commands;
using Hipda.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Hipda.Client.ViewModels
{
    public class MyPostsThreadListViewViewModel
    {
        int _startPageNo = 1;
        ListView _leftListView;
        CommandBar _leftCommandBar;
        Action _beforeLoad;
        Action _afterLoad;
        Action _noDataNotice;
        MyPostsService _ds;

        public int ThreadMaxPageNo { get; set; }

        public DelegateCommand RefreshThreadCommand { get; set; }

        public MyPostsThreadListViewViewModel(int pageNo, ListView leftListView, CommandBar leftCommandBar, Action beforeLoad, Action afterLoad, Action noDataNotice)
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
            _ds = new MyPostsService();

            LoadDataForMyPosts(pageNo);

            var RefreshThreadCommand = new DelegateCommand();
            RefreshThreadCommand.ExecuteAction = (p) => {
                _ds.ClearThreadDataForMyPosts();
                LoadDataForMyPosts(1);
            };

            var btnRefresh = new AppBarButton { Icon = new FontIcon { Glyph = "\uE895" }, Label = "刷新" };
            btnRefresh.Command = RefreshThreadCommand;
            _leftCommandBar.PrimaryCommands.Add(btnRefresh);
        }

        void LoadDataForMyPosts(int pageNo)
        {
            var cv = _ds.GetViewForThreadPageForMyPosts(pageNo, _beforeLoad, _afterLoad, _noDataNotice);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNoForMyPosts();
                _startPageNo = pageNo;
                _leftListView.ItemsSource = cv;
            }
        }

        public void LoadPrevPageData()
        {
            if (_startPageNo > 1)
            {
                _ds.ClearThreadDataForMyPosts();
                LoadDataForMyPosts(_startPageNo);
            }
        }
    }
}
