using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class OpenNewViewParameterModel
    {
        public int ThreadId { get; set; }

        public ThemeMode ThemeMode { get; set; }

        public ViewLifetimeControl NewView { get; set; }
    }
}
