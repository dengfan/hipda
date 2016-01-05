using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class PromptNumViewModel : NotificationObject
    {
        // 饿汉模式，确保只有一个实例
        private static readonly PromptNumViewModel instance = new PromptNumViewModel();

        private PromptNumViewModel()
        {
        }

        public static PromptNumViewModel GetInstance()
        {
            return instance;
        }

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

    }
}
