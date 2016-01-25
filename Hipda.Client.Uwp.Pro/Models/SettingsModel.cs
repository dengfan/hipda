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
        public int FontSizeType { get; set; } = 1;
        public double LineHeight { get; set; } = 22;
        public double PictureOpacity { get; set; } = 0.4;
        public bool CanShowTopTheme { get; set; } = true;
        public string MessageTail { get; set; } = string.Empty;
        public string BlockKeywords { get; set; } = string.Empty;
    }
}
