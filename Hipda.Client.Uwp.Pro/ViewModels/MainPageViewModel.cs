using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class MainPageViewModel : NotificationObject
    {
        // 饿汉模式，确保只有一个实例
        private static readonly MainPageViewModel instance = new MainPageViewModel();

        public MainPageViewModel()
        {
            NavButtons = new ObservableCollection<NavButtonItemModel>();
            NavButtons.Add(new NavButtonItemModel { Icon = "\uE1A3", Text = "搜索贴子", TypeValue = "item=search" });
            NavButtons.Add(new NavButtonItemModel { Icon = "\uEA4A", Text = "我的贴子", TypeValue = "item=threads" });
            NavButtons.Add(new NavButtonItemModel { Icon = "\uE89B", Text = "我的回复", TypeValue = "item=posts" });
            NavButtons.Add(new NavButtonItemModel { Icon = "\uE1CE", Text = "我的收藏", TypeValue = "item=favorites" });
            NavButtons.Add(new NavButtonItemModel { Icon = "Di", Text = "Discovery 版", TypeValue = "fid=2" });
            NavButtons.Add(new NavButtonItemModel { Icon = "BS", Text = "Buy & Sell 版", TypeValue = "fid=6" });
            NavButtons.Add(new NavButtonItemModel { Icon = "EI", Text = "E-Ink 版", TypeValue = "fid=57" });
            NavButtons.Add(new NavButtonItemModel { Icon = "\uE712", Text = "更多版块", TypeValue = "more" });
        }

        public static MainPageViewModel GetInstance()
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

        private NavButtonItemModel _selectedNavButton;

        public NavButtonItemModel SelectedNavButton
        {
            get { return _selectedNavButton; }
            set
            {
                _selectedNavButton = value;
                this.RaisePropertyChanged("SelectedNavButton");
            }
        }

        #region prompt numbers
        private int _promptPm;

        public int PromptPm
        {
            get { return _promptPm; }
            set
            {
                _promptPm = value;
                this.RaisePropertyChanged("PromptPm");
                this.RaisePropertyChanged("PmNoVisibility");
                this.RaisePropertyChanged("PromptColor");
            }
        }

        private int _promptAnnouncePm;

        public int PromptAnnouncePm
        {
            get { return _promptAnnouncePm; }
            set
            {
                _promptAnnouncePm = value;
                this.RaisePropertyChanged("PromptAllWithoutPromptPm");
                this.RaisePropertyChanged("NoticeNoVisibility");
                this.RaisePropertyChanged("PromptColor");
            }
        }

        private int _promptSystemPm;

        public int PromptSystemPm
        {
            get { return _promptSystemPm; }
            set
            {
                _promptSystemPm = value;
                this.RaisePropertyChanged("PromptAllWithoutPromptPm");
                this.RaisePropertyChanged("NoticeNoVisibility");
                this.RaisePropertyChanged("PromptColor");
            }
        }

        private int _promptFriend;

        public int PromptFriend
        {
            get { return _promptFriend; }
            set
            {
                _promptFriend = value;
                this.RaisePropertyChanged("PromptAllWithoutPromptPm");
                this.RaisePropertyChanged("NoticeNoVisibility");
                this.RaisePropertyChanged("PromptColor");
            }
        }

        private int _promptThreads;

        public int PromptThreads
        {
            get { return _promptThreads; }
            set
            {
                _promptThreads = value;
                this.RaisePropertyChanged("PromptAllWithoutPromptPm");
                this.RaisePropertyChanged("NoticeNoVisibility");
                this.RaisePropertyChanged("PromptColor");
            }
        }

        public int PromptAllWithoutPromptPm
        {
            get
            {
                // 这里不包含私信数量，私信数量会被单独显示
                return PromptAnnouncePm + PromptSystemPm + PromptFriend + PromptThreads;
            }
        }

        public Visibility NoticeNoVisibility
        {
            get
            {
                return (PromptAnnouncePm + PromptSystemPm + PromptFriend + PromptThreads) > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility PmNoVisibility
        {
            get
            {
                return PromptPm > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public SolidColorBrush PromptColor
        {
            get
            {
                return (PromptPm + PromptAnnouncePm + PromptSystemPm + PromptFriend + PromptThreads) > 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Transparent);
            }
        }
        #endregion
    }
}
