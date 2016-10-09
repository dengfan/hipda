using GalaSoft.MvvmLight.Command;
using Hipda.Client.Services;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Hipda.Client.ViewModels
{
    public class DefaultThreadListViewViewModel
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

        public ICommand RefreshThreadCommand { get; set; }

        public DefaultThreadListViewViewModel(int pageNo, int forumId, ListView leftListView, CommandBar leftCommandBar, Action openCreateThreadPanel, Action beforeLoad, Action afterLoad, Action noDataNotice)
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

            RefreshThreadCommand = new RelayCommand(() =>
            {
                _ds.ClearThreadData(_forumId);
                LoadData(1, _forumId);
            });

            var btnAdd = new AppBarButton { Icon = new FontIcon { Glyph = "\uE104" }, Label = "发表新贴" };
            btnAdd.Click += (s, e) => openCreateThreadPanel();
            _leftCommandBar.PrimaryCommands.Add(btnAdd);

            var btnRefresh = new AppBarButton { Icon = new FontIcon { Glyph = "\uE895" }, Label = "刷新" };
            btnRefresh.Command = RefreshThreadCommand;
            _leftCommandBar.PrimaryCommands.Add(btnRefresh);

            var categories = SendService.GetCategory(_forumId);
            if (categories != null)
            {
                var btnSelectLabel = new AppBarButton { Icon = new FontIcon { Glyph = "\uE169" }, Label = "按标签浏览" };
                var menuFlyout = new MenuFlyout();

                foreach (var cat in categories)
                {
                    menuFlyout.Items.Add(CreateMenuFlyoutItem(cat[0], Convert.ToInt32(cat[1])));
                }

                btnSelectLabel.Flyout = menuFlyout;
                _leftCommandBar.PrimaryCommands.Add(btnSelectLabel);
            }
            

            var _btnSort = new AppBarToggleButton { Icon = new SymbolIcon(Symbol.Sort), Label = "\u2601 按发布时间倒序排列（限当前版块）" };
            _btnSort.IsChecked = RoamingSettingsService.GetOrderByDateline(_forumId);
            _btnSort.Checked += (s, e) =>
            {
                RoamingSettingsService.SetOrderByDateline(_forumId, true);
                _ds.ClearThreadData(_forumId);
                LoadData(1, _forumId);
            };
            _btnSort.Unchecked += (s, e) =>
            {
                RoamingSettingsService.SetOrderByDateline(_forumId, false);
                _ds.ClearThreadData(_forumId);
                LoadData(1, _forumId);
            };
            _leftCommandBar.SecondaryCommands.Add(_btnSort);
        }

        MenuFlyoutItem CreateMenuFlyoutItem(string typeName, int typeId)
        {
            var menuFlyoutItem = new MenuFlyoutItem();
            menuFlyoutItem.Text = typeName;

            var OpenThreadByTypeIdCommand = new RelayCommand(() =>
            {
                _ds.ClearThreadData(_forumId);
                LoadData(1, _forumId, typeId);
            });
            menuFlyoutItem.Command = OpenThreadByTypeIdCommand;

            return menuFlyoutItem;
        }

        void LoadData(int pageNo, int forumId)
        {
            LoadData(pageNo, forumId, -1);
        }

        void LoadData(int pageNo, int forumId, int typeId)
        {
            var cv = _ds.GetViewForThreadItems(pageNo, typeId, forumId, _beforeLoad, _afterLoad, _noDataNotice);
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
