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
        public string ForumNameInfo
        {
            get
            {
                return $"@{ForumName}";
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
        public string UsernameInfo
        {
            get
            {
                return $"{Username} @{ForumName}";
            }
        }
    }

    public class RoamingSettingsModel
    {
        public ObservableCollection<BlockUser> BlockUsers { get; set; } = new ObservableCollection<BlockUser>();
        public ObservableCollection<BlockThread> BlockThreads { get; set; } = new ObservableCollection<BlockThread>();
    }
}
