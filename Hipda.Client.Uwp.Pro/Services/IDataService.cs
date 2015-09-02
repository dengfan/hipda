using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Hipda.Client.Uwp.Pro.Services
{
    public interface IDataService
    {
        ICollectionView GetViewForThreadPage(int forumId, Action beforeLoad, Action afterLoad);

        ICollectionView GetViewForReplyPage(int threadId, Action beforeLoad, Action afterLoad);
    }
}
