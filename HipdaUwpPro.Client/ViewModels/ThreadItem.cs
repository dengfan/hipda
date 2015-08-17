using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HipdaUwpPro.Client.ViewModels
{
    public class ThreadItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public List<ReplyItem> ReplyList { get; set; }

        public void GetReplyList()
        {
            if (ReplyList == null)
            {
                var data = new List<ReplyItem>();
                for (int i = 0; i < 50; i++)
                {
                    data.Add(new ReplyItem { Id = i, Name = "坐和放宽" + i, Content = "我人有的和，主产不为这。" + i });
                }

                ReplyList = data;
            }
        }
    }
}
