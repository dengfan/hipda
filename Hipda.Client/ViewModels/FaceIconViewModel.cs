using Hipda.Client.Models;
using Hipda.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.ViewModels
{
    public class FaceIconViewModel : NotificationObject
    {
        private List<FaceItemModel> _faceIcons;

        public List<FaceItemModel> FaceIcons
        {
            get { return _faceIcons; }
            set
            {
                _faceIcons = value;
                this.RaisePropertyChanged("FaceIcons");
            }
        }

        private List<EmojiItemModel> _emojiIcons = new List<EmojiItemModel>();

        public List<EmojiItemModel> EmojiIcons
        {
            get { return _emojiIcons; }
            set
            {
                _emojiIcons = value;
                this.RaisePropertyChanged("FaceIcons");
            }
        }

        public FaceIconViewModel()
        {
            GetFaceIconsData();
        }

        private void GetFaceIconsData()
        {
            FaceIcons = CommonService.FaceData;

            foreach (var pair in CommonService.EmojiDic)
            {
                EmojiIcons.Add(new EmojiItemModel { Label = pair.Key, Value = pair.Value });
            }
        }
    }
}
