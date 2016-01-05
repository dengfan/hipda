using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public enum NavButtonType
    {
        BySearch,

        ByMyItem,

        ByForumId,

        ByPage
    }

    public class NavButtonItemModel
    {
        public NavButtonType Type { get; set; }
        public string Icon { get; set; }
        public string Text { get; set; }
        public string TypeValue { get; set; }
    }
}
