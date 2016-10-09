using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Hipda.Client.Models;
using Hipda.Http;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        // 饿汉模式，确保只有一个实例
        private static readonly MainPageViewModel instance = new MainPageViewModel();

        public MainPageViewModel()
        {
            NavButtons = new ObservableCollection<NavButtonItemModel>();
            NavButtons.Add(new NavButtonItemModel { Icon = "\uEA4A", Text = "我的贴子", TypeValue = "item=threads" });
            NavButtons.Add(new NavButtonItemModel { Icon = "\uE89B", Text = "我的回复", TypeValue = "item=posts" });
            NavButtons.Add(new NavButtonItemModel { Icon = "\uE1CE", Text = "我的收藏", TypeValue = "item=favorites" });
            NavButtons.Add(new NavButtonItemModel { Icon = "Di", Text = "Discovery", TypeValue = "fid=2" });
            NavButtons.Add(new NavButtonItemModel { Icon = "EI", Text = "E-Ink", TypeValue = "fid=59" });
            NavButtons.Add(new NavButtonItemModel { Icon = "BS", Text = "Buy & Sell", TypeValue = "fid=6" });
            NavButtons.Add(new NavButtonItemModel { Icon = "\uE712", Text = "更多版块", TypeValue = "more" });

            ClearCookiesCommand = new RelayCommand(() =>
            {
                var _httpClient = HttpHandle.GetInstance();
                _httpClient.ClearCookies();
            });
        }

        public static MainPageViewModel GetInstance()
        {
            return instance;
        }

        public ICommand ClearCookiesCommand { get; set; }


        private ObservableCollection<NavButtonItemModel> _navButtons;

        public ObservableCollection<NavButtonItemModel> NavButtons
        {
            get { return _navButtons; }
            set
            {
                Set(ref _navButtons, value);
            }
        }

        private NavButtonItemModel _selectedNavButton;

        public NavButtonItemModel SelectedNavButton
        {
            get { return _selectedNavButton; }
            set
            {
                Set(ref _selectedNavButton, value);
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
                RaisePropertyChanged("PromptPm");
                RaisePropertyChanged("PmNoVisibility");
                RaisePropertyChanged("PromptColor");
            }
        }

        private int _promptAnnouncePm;

        public int PromptAnnouncePm
        {
            get { return _promptAnnouncePm; }
            set
            {
                _promptAnnouncePm = value;
                RaisePropertyChanged("PromptAllWithoutPromptPm");
                RaisePropertyChanged("NoticeNoVisibility");
                RaisePropertyChanged("PromptColor");
            }
        }

        private int _promptSystemPm;

        public int PromptSystemPm
        {
            get { return _promptSystemPm; }
            set
            {
                _promptSystemPm = value;
                RaisePropertyChanged("PromptAllWithoutPromptPm");
                RaisePropertyChanged("NoticeNoVisibility");
                RaisePropertyChanged("PromptColor");
            }
        }

        private int _promptFriend;

        public int PromptFriend
        {
            get { return _promptFriend; }
            set
            {
                _promptFriend = value;
                RaisePropertyChanged("PromptAllWithoutPromptPm");
                RaisePropertyChanged("NoticeNoVisibility");
                RaisePropertyChanged("PromptColor");
            }
        }

        private int _promptThreads;

        public int PromptThreads
        {
            get { return _promptThreads; }
            set
            {
                _promptThreads = value;
                RaisePropertyChanged("PromptAllWithoutPromptPm");
                RaisePropertyChanged("NoticeNoVisibility");
                RaisePropertyChanged("PromptColor");
            }
        }

        private int _promptNoticeCountInToastTempData;

        public int PromptNoticeCountInToastTempData
        {
            get { return _promptNoticeCountInToastTempData; }
            set
            {
                _promptNoticeCountInToastTempData = value;
                RaisePropertyChanged("PromptAllWithoutPromptPm");
                RaisePropertyChanged("NoticeNoVisibility");
                RaisePropertyChanged("PromptColor");
            }
        }

        public int PromptAllWithoutPromptPm
        {
            get
            {
                // 这里不包含私信数量，私信数量会被单独显示
                return PromptAnnouncePm + PromptSystemPm + PromptFriend + PromptThreads + PromptNoticeCountInToastTempData;
            }
        }

        public Visibility NoticeNoVisibility
        {
            get
            {
                return PromptAllWithoutPromptPm > 0 ? Visibility.Visible : Visibility.Collapsed;
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
                return (PromptPm + PromptAllWithoutPromptPm) > 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Transparent);
            }
        }
        #endregion
    }
}
