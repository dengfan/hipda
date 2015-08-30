using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.Models
{
    public class ForumDataModel
    {
        public string ForumId { get; set; }

        public ObservableCollection<ThreadPageModel> ThreadData { get; set; }
    }
}
