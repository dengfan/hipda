using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Models
{
    public class ForumModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ForumCategoryModel
    {
        public string ForumGroupName { get; set; }
        public List<ForumModel> Forums { get; set; } = new List<ForumModel>();
    }
}
