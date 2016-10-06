using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Models
{
    public class ReplyPageModel
    {
        public int ThreadId { get; set; }

        public List<ReplyItemModel> Replies { get; set; }
    }
}
