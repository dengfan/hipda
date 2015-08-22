using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ThreadPageModel
    {
        public int PageNo { get; set; }

        public ObservableCollection<ThreadItemModel> ThreadItems { get; set; }
    }
}
