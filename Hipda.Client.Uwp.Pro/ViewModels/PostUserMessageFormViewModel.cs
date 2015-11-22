using Hipda.Client.Uwp.Pro.Commands;
using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class PostUserMessageFormViewModel
    {
        public List<FaceItemModel> FaceData
        {
            get
            {
                return FaceService.FaceData;
            }
        }

        public DelegateCommand InsertFace { get; set; }
        public DelegateCommand Post { get; set; }

        public PostUserMessageFormViewModel()
        {
            
        }
    }
}
