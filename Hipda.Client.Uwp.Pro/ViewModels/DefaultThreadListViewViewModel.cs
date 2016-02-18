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

        public DelegateCommand RefreshThreadCommand { get; set; }

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

            RefreshThreadCommand = new DelegateCommand();
            RefreshThreadCommand.ExecuteAction = (p) => {
                _ds.ClearThreadData(_forumId);
                LoadData(1, _forumId);
            };

            var btnAdd = new AppBarButton { Icon = new FontIcon { Glyph = "\uE104" }, Label = "发表新贴" };
            btnAdd.Click += (s, e) => openCreateThreadPanel();
            _leftCommandBar.PrimaryCommands.Add(btnAdd);

            var btnRefresh = new AppBarButton { Icon = new FontIcon { Glyph = "\uE895" }, Label = "刷新" };
            btnRefresh.Command = RefreshThreadCommand;
            _leftCommandBar.PrimaryCommands.Add(btnRefresh);

            var btnSelectLabel = new AppBarButton { Icon = new FontIcon { Glyph = "\uE169" }, Label = "按标签浏览" };
            var menuFlyout = new MenuFlyout();
            if (_forumId == 2)
            {
                menuFlyout.Items.Add(CreateMenuFlyoutItem("聚会", 9));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("汽车", 33));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("大杂烩", 38));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("助学", 40));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("Discovery", 56));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("投资", 57));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("职场", 58));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("文艺", 65));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("版喃", 66));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("显摆", 67));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("晒物劝败", 79));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("装修", 81));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("YY", 39));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("站务", 19));
                btnSelectLabel.Flyout = menuFlyout;
                _leftCommandBar.PrimaryCommands.Add(btnSelectLabel);
            }
            else if (_forumId == 6)
            {
                menuFlyout.Items.Add(CreateMenuFlyoutItem("手机", 1));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("掌上电脑", 2));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("笔记本电脑", 3));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("无线产品", 4));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("数码相机、摄像机", 5));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("MP3随身听", 6));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("各类配件", 7));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("其他好玩的", 8));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("站务", 19));
                btnSelectLabel.Flyout = menuFlyout;
                _leftCommandBar.PrimaryCommands.Add(btnSelectLabel);
            }
            else if (_forumId == 59)
            {
                menuFlyout.Items.Add(CreateMenuFlyoutItem("Kindle", 68));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("SONY", 69));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("国产", 70));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("资源", 72));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("综合", 73));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("交流", 75));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("Nook", 77));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("Kobo", 80));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("求助", 18));
                menuFlyout.Items.Add(CreateMenuFlyoutItem("站务", 19));
                btnSelectLabel.Flyout = menuFlyout;
                _leftCommandBar.PrimaryCommands.Add(btnSelectLabel);
            }

            var btnSort = new AppBarToggleButton { Icon = new SymbolIcon(Symbol.Sort), Label = "按发布时间倒序排列" };
            _leftCommandBar.SecondaryCommands.Add(btnSort);
        }

        MenuFlyoutItem CreateMenuFlyoutItem(string typeName, int typeId)
        {
            var menuFlyoutItem = new MenuFlyoutItem();
            menuFlyoutItem.Text = typeName;

            var OpenThreadByTypeIdCommand = new DelegateCommand();
            OpenThreadByTypeIdCommand.ExecuteAction = (p) =>
            {
                _ds.ClearThreadData(_forumId);
                LoadData(1, _forumId, typeId);
            };
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
