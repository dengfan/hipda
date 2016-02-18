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
    public class SearchTitleThreadListViewViewModel
    {
        int _startPageNo = 1;
        ListView _leftListView;
        CommandBar _leftCommandBar;
        Action _beforeLoad;
        Action _afterLoad;
        Action _noDataNotice;
        SearchTitleService _ds;

        string _searchKeyword;
        string _searchAuthor;
        int _searchType;
        int _searchTimeSpan;
        int _searchForumSpan;

        public int ThreadMaxPageNo { get; set; }

        public DelegateCommand RefreshThreadCommand { get; set; }

        public SearchTitleThreadListViewViewModel(int pageNo, string searchKeyword, string searchAuthor, int searchType, int searchTimeSpan, int searchForumSpan, ListView leftListView, CommandBar leftCommandBar, Action beforeLoad, Action afterLoad, Action noDataNotice)
        {
            _searchKeyword = searchKeyword;
            _searchAuthor = searchAuthor;
            _searchType = searchType;
            _searchTimeSpan = searchTimeSpan;
            _searchForumSpan = searchForumSpan;

            _leftListView = leftListView;
            _leftListView.SelectionMode = ListViewSelectionMode.Single;
            _leftListView.ItemsSource = null;

            _leftCommandBar = leftCommandBar;
            _leftCommandBar.Visibility = Visibility.Visible;
            _leftCommandBar.PrimaryCommands.Clear();
            _leftCommandBar.SecondaryCommands.Clear();

            _beforeLoad = beforeLoad;
            _afterLoad = afterLoad;
            _noDataNotice = noDataNotice;
            _ds = new SearchTitleService();

            // 先清除已搜索的数据
            _ds.ClearThreadDataForSearchTitle();
            LoadDataForSearchTitle(pageNo);

            var RefreshThreadCommand = new DelegateCommand();
            RefreshThreadCommand.ExecuteAction = (p) =>
            {
                _ds.ClearThreadDataForSearchTitle();
                LoadDataForSearchTitle(1);
            };

            var btnRefresh = new AppBarButton { Icon = new FontIcon { Glyph = "\uE895" }, Label = "刷新" };
            btnRefresh.Command = RefreshThreadCommand;
            _leftCommandBar.PrimaryCommands.Add(btnRefresh);
        }

        void LoadDataForSearchTitle(int pageNo)
        {
            var cv = _ds.GetViewForThreadPageForSearchTitle(pageNo, _searchKeyword, _searchAuthor, _searchTimeSpan, _searchForumSpan, _beforeLoad, _afterLoad, _noDataNotice);
            if (cv != null)
            {
                ThreadMaxPageNo = _ds.GetThreadMaxPageNoForSearchTitle();
                _startPageNo = pageNo;
                _leftListView.ItemsSource = cv;
            }
        }
    }
}
