using GalaSoft.MvvmLight;
using Hipda.Client.Models;
using Hipda.Client.Services;
using System.Collections.Generic;

namespace Hipda.Client.ViewModels
{
    public class FaceIconViewModel : ViewModelBase
    {
        private List<FaceItemModel> _faceIcons;

        public List<FaceItemModel> FaceIcons
        {
            get { return _faceIcons; }
            set
            {
                Set(ref _faceIcons, value);
            }
        }

        private List<EmojiItemModel> _emojiIcons = new List<EmojiItemModel>();

        public List<EmojiItemModel> EmojiIcons
        {
            get { return _emojiIcons; }
            set
            {
                Set(ref _emojiIcons, value);
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
