using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class ThreadListViewForDefaultViewModel
    {
        int _forumId;

        int _startPageNo = 1;
        ListView _leftListView;
        CommandBar _leftCommandBar;
        Action _beforeLoad;
        Action _afterLoad;
        Action _noDataNotice;
        ThreadListService _ds;

        public int ThreadMaxPageNo { get; set; }

        public DelegateCommand RefreshThreadCommand { get; set; }

        public ThreadListViewForDefaultViewModel(int pageNo, int forumId, ListView leftListView, CommandBar leftCommandBar, Action openCreateThreadPanel, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            _forumId = forumId;
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
            _ds = new ThreadListService();

            LoadData(pageNo, _forumId);

            RefreshThreadCommand = new DelegateCommand();
            RefreshThreadCommand.ExecuteAction = (p) => {
                _ds.ClearThreadData(_forumId);
                LoadData(1, _forumId);
            };

            var btnAdd = new AppBarButton { Icon = new FontIcon { Glyph = "\uE104" }, Label = "发表新贴" };
            btnAdd.Click += (s, e) => openCreateThreadPanel();
            var btnRefresh = new AppBarButton { Icon = new FontIcon { Glyph = "\uE895" }, Label = "刷新" };
            btnRefresh.Command = RefreshThreadCommand;
            var btnSort = new AppBarButton { Icon = new SymbolIcon(Symbol.Sort), Label = "按发布时间倒序排列" };
            _leftCommandBar.PrimaryCommands.Add(btnAdd);
            _leftCommandBar.PrimaryCommands.Add(btnRefresh);
            _leftCommandBar.PrimaryCommands.Add(btnSort);
        }

        void LoadData(int pageNo, int forumId)
        {
            var cv = _ds.GetViewForThreadItems(pageNo, forumId, _beforeLoad, _afterLoad, _noDataNotice);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNo();
                _startPageNo = pageNo;
                _leftListView.ItemsSource = cv;
            }
        }

        public void LoadPrevPageData()
        {
            if (_startPageNo > 1)
            {
                _ds.ClearThreadData(_forumId);
                LoadData(_startPageNo, _forumId);
            }
        }
    }
}
