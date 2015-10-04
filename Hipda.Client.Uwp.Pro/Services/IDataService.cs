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
        #region thread
        ICollectionView GetViewForThreadPage(int startPageNo, int forumId, Action beforeLoad, Action afterLoad);

        ICollectionView GetViewForThreadPageForMyThreads(int startPageNo, Action beforeLoad, Action afterLoad);

        int GetThreadMaxPageNo();

        int GetThreadMinPageNoInLoadedData();

        void ClearThreadData(int forumId);

        ThreadItemModel GetThreadItem(int threadId);

        void SetRead(int threadId);

        bool IsRead(int threadId);
        #endregion

        #region reply
        ICollectionView GetViewForReplyPage(int startPageNo, int threadId, int threadAuthorUserId, Action beforeLoad, Action afterLoad);

        int GetReplyMaxPageNo();

        int GetReplyMinPageNoInLoadedData(int threadId);

        void ClearReplyData(int threadId);
        #endregion
    }
}
