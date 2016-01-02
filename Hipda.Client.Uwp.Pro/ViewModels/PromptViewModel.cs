using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class PromptViewModel : NotificationObject
    {
        // 饿汉模式，确保只有一个实例
        private static readonly PromptViewModel instance = new PromptViewModel();

        private PromptViewModel()
        {
        }

        public static PromptViewModel GetInstance()
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
                this.RaisePropertyChanged("PromptAll");
            }
        }

        private int _promptAnnouncePm;

        public int PromptAnnouncePm
        {
            get { return _promptAnnouncePm; }
            set
            {
                _promptAnnouncePm = value;
                this.RaisePropertyChanged("PromptAnnouncePm");
                this.RaisePropertyChanged("PromptAll");
            }
        }

        private int _promptSystemPm;

        public int PromptSystemPm
        {
            get { return _promptSystemPm; }
            set
            {
                _promptSystemPm = value;
                this.RaisePropertyChanged("PromptSystemPm");
                this.RaisePropertyChanged("PromptAll");
            }
        }

        private int _promptFriend;

        public int PromptFriend
        {
            get { return _promptFriend; }
            set
            {
                _promptFriend = value;
                this.RaisePropertyChanged("PromptFriend");
                this.RaisePropertyChanged("PromptAll");
            }
        }

        private int _promptThreads;

        public int PromptThreads
        {
            get { return _promptThreads; }
            set
            {
                _promptThreads = value;
                this.RaisePropertyChanged("PromptThreads");
                this.RaisePropertyChanged("PromptAll");
            }
        }

        public int PromptAll
        {
            get
            {
                // 这里不包含私信数量，私信数量会被单独显示
                return PromptAnnouncePm + PromptSystemPm + PromptFriend + PromptThreads;
            }
        }
    }
}
