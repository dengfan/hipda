using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class OpenNewViewParameterModel
    {
        public int ThreadId { get; set; }

        public ElementTheme ElementTheme { get; set; }

        public ViewLifetimeControl NewView { get; set; }
    }
}
