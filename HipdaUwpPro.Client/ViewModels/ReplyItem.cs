using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HipdaUwpPro.Client.ViewModels
{
    public class ReplyItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }

        
    }
}
