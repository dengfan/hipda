using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public struct BlockUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public int ForumId { get; set; }
        public string ForumName { get; set; }
        public Uri AvatarUri
        {
            get
            {
                return Common.GetSmallAvatarUriByUserId(UserId);
            }
        }
    }

    public struct BlockThread
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public int ThreadId { get; set; }
        public string ThreadTitle { get; set; }
        public int ForumId { get; set; }
        public string ForumName { get; set; }
        public Uri AvatarUri
        {
            get
            {
                return Common.GetSmallAvatarUriByUserId(UserId);
            }
        }
    }

    //public class LocalSettingsModel
    //{
    //    public int ThemeType { get; set; } = -1; // -1表示未设定
    //    public double FontSize1 { get; set; } = 15D;
    //    public double FontSize2 { get; set; } = 12D;
    //    public double LineHeight { get; set; } = 22D;
    //    public double PictureOpacity { get; set; } = 0.4D;
    //    public bool CanShowTopThread { get; set; } = true;
    //}

    public class RoamingSettingsModel
    {
        public ObservableCollection<BlockUser> BlockUsers { get; set; } = new ObservableCollection<BlockUser>();
        public ObservableCollection<BlockThread> BlockThreads { get; set; } = new ObservableCollection<BlockThread>();
    }
}
