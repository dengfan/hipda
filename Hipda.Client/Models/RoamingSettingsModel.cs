using Hipda.Client.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Models
{
    public struct BlockUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public int ForumId { get; set; }
        public string ForumName { get; set; }
    }

    public struct BlockThread
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public int ThreadId { get; set; }
        public string ThreadTitle { get; set; }
        public int ForumId { get; set; }
        public string ForumName { get; set; }
    }

    public class RoamingSettingsModel
    {
        public bool CanShowTopThread { get; set; } = true;
        public ObservableCollection<BlockUser> BlockUsers { get; set; } = new ObservableCollection<BlockUser>();
        public ObservableCollection<BlockThread> BlockThreads { get; set; } = new ObservableCollection<BlockThread>();
    }
}
