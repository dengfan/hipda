using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public struct BlockUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
    }

    public struct BlockThread
    {
        public int ThreadId { get; set; }
        public string ThreadTitle { get; set; }
    }

    public class SettingsModel
    {
        public int ThemeType { get; set; } = -1; // -1表示未设定
        public double FontSize1 { get; set; } = 15d;
        public double FontSize2 { get; set; } = 12d;
        public double LineHeight { get; set; } = 22d;
        public double PictureOpacity { get; set; } = 0.4d;
        public bool CanShowTopThread { get; set; } = true;
        public string MessageTail { get; set; } = string.Empty;
        public string BlockKeywords { get; set; } = string.Empty;
    }
}
