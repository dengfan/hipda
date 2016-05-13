using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class MyFavoritesThreadListViewViewModel
    {
        int _startPageNo = 1;
        ListView _leftListView;
        CommandBar _leftCommandBar;
        Action _beforeLoad;
        Action _afterLoad;
        Action _noDataNotice;
        MyFavoritesService _ds;

        public int ThreadMaxPageNo { get; set; }

        public MyFavoritesThreadListViewViewModel(int pageNo, ListView leftListView, CommandBar leftCommandBar, Action beforeLoad, Action afterLoad, Action noDataNotice)
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
            _ds = new MyFavoritesService();

            LoadDataForMyFavorites(pageNo);

            var btnDeleteSelected = new AppBarButton { Icon = new SymbolIcon(Symbol.Delete), Label = "删除", IsEnabled = false };
            btnDeleteSelected.Tapped += async (s, e) =>
            {
                var deleteThreads = _leftListView.SelectedItems;
                if (deleteThreads != null)
                {
                    var ids = new List<int>();
                    foreach (ThreadItemForMyFavoritesModel thread in deleteThreads)
                    {
                        ids.Add(thread.ThreadId);
                    }
                    bool isOk = await _ds.DeleteThreadForMyFavorites(ids);
                    if (isOk)
                    {
                        _ds.ClearThreadDataForMyFavorites();
                        LoadDataForMyFavorites(1);

                        //var frame = Window.Current.Content as Frame;
                        //var mp = frame.Content as MainPage;
                        //if (mp != null)
                        //{
                        //    mp.ShowTipsBar("操作成功！");
                        //}
                    }
                }
            };

            var btnMultipleSelect = new AppBarButton { Icon = new FontIcon { Glyph = "\uE762", FontFamily = new FontFamily("Segoe MDL2 Assets") }, Label = "选择" };
            btnMultipleSelect.Tapped += (s, e) =>
            {
                if (_leftListView.SelectionMode == ListViewSelectionMode.Single)
                {
                    _leftListView.SelectionMode = ListViewSelectionMode.Multiple;
                    btnDeleteSelected.IsEnabled = true;
                }
                else
                {
                    _leftListView.SelectionMode = ListViewSelectionMode.Single;
                    btnDeleteSelected.IsEnabled = false;
                }
            };

            var btnRefreshForFavorites = new AppBarButton { Icon = new SymbolIcon(Symbol.Refresh), Label = "刷新" };
            btnRefreshForFavorites.Tapped += (s, e) => {
                _leftListView.SelectionMode = ListViewSelectionMode.Single;
                btnDeleteSelected.IsEnabled = false;

                _ds.ClearThreadDataForMyFavorites();
                LoadDataForMyFavorites(1);
            };

            _leftCommandBar.PrimaryCommands.Add(btnRefreshForFavorites);
            _leftCommandBar.PrimaryCommands.Add(btnMultipleSelect);
            _leftCommandBar.PrimaryCommands.Add(btnDeleteSelected);
        }

        void LoadDataForMyFavorites(int pageNo)
        {
            var cv = _ds.GetViewForThreadPageForMyFavorites(pageNo, _beforeLoad, _afterLoad, _noDataNotice);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNoForMyFavorites();
                _startPageNo = pageNo;
                _leftListView.ItemsSource = cv;
            }
        }

        public void LoadPrevPageData()
        {
            if (_startPageNo > 1)
            {
                _ds.ClearThreadDataForMyFavorites();
                LoadDataForMyFavorites(_startPageNo);
            }
        }
    }
}
