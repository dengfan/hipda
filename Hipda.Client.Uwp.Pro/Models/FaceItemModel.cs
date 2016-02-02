using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class FaceItemModel
    {
        public string Image { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class EmojiItemModel
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }
}
