using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class NavButtonViewModel : NotificationObject
    {
        // 饿汉模式，确保只有一个实例
        private static readonly NavButtonViewModel instance = new NavButtonViewModel();

        public NavButtonViewModel()
        {
            NavButtons = new ObservableCollection<NavButtonItemModel>();
            NavButtons.Add(new NavButtonItemModel { Icon = "\uE1A3", Text = "搜索贴子", Type = NavButtonType.BySearch, TypeValue = "item=search" });
            NavButtons.Add(new NavButtonItemModel { Icon = "\uEA4A", Text = "我的贴子", Type = NavButtonType.ByMyItem, TypeValue = "item=threads" });
            NavButtons.Add(new NavButtonItemModel { Icon = "\uE89B", Text = "我的回复", Type = NavButtonType.ByMyItem, TypeValue = "item=posts" });
            NavButtons.Add(new NavButtonItemModel { Icon = "\uE1CE", Text = "我的收藏", Type = NavButtonType.ByMyItem, TypeValue = "item=favorites" });
            NavButtons.Add(new NavButtonItemModel { Icon = "Di", Text = "Discovery 版", Type = NavButtonType.ByForumId, TypeValue = "fid=2" });
            NavButtons.Add(new NavButtonItemModel { Icon = "BS", Text = "Buy & Sell 版", Type = NavButtonType.ByForumId, TypeValue = "fid=6" });
            NavButtons.Add(new NavButtonItemModel { Icon = "EI", Text = "E-Ink 版", Type = NavButtonType.ByForumId, TypeValue = "fid=57" });
            NavButtons.Add(new NavButtonItemModel { Icon = "\uE712", Text = "更多版块", Type = NavButtonType.ByMyItem, TypeValue = "more" });
        }

        public static NavButtonViewModel GetInstance()
        {
            return instance;
        }

        private ObservableCollection<NavButtonItemModel> _navButtons;

        public ObservableCollection<NavButtonItemModel> NavButtons
        {
            get { return _navButtons; }
            set
            {
                _navButtons = value;
                this.RaisePropertyChanged("NavButton");
            }
        }

    }
}
