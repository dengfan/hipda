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

        ICollectionView GetViewForThreadPageForMyPosts(int startPageNo, Action beforeLoad, Action afterLoad);


        int GetThreadMaxPageNo();

        int GetThreadMaxPageNoForMyThreads();

        int GetThreadMaxPageNoForMyPosts();


        int GetThreadMinPageNoInLoadedData();

        int GetThreadMinPageNoForMyThreadsInLoadedData();

        int GetThreadMinPageNoForMyPostsInLoadedData();


        void ClearThreadData(int forumId);

        void ClearThreadDataForMyThreads();

        void ClearThreadDataForMyPosts();


        ThreadItemModel GetThreadItem(int threadId);

        ThreadItemForMyThreadsModel GetThreadItemForMyThreads(int threadId);

        ThreadItemForMyPostsModel GetThreadItemForMyPosts(int threadId);


        void SetRead(int threadId);

        bool IsRead(int threadId);
        #endregion

        #region reply
        ICollectionView GetViewForReplyPage(int startPageNo, int threadId, int threadAuthorUserId, Action beforeLoad, Action afterLoad, Action<int> linkClickEvent);

        ICollectionView GetViewForReplyPage(int startPageNo, int threadId, int threadAuthorUserId, int postId, Action beforeLoad, Action afterLoad, Action<int> listViewScroll, Action<int> linkClickEvent);

        Task<int[]> LoadReplyDataForRedirectPageAsync(int threadId, int targetPostId, Action<int> linkClickEvent, CancellationTokenSource cts);

        int GetReplyMaxPageNo();

        int GetReplyMinPageNoInLoadedData(int threadId);

        void ClearReplyData(int threadId);

        void SetScrollState(bool isCompleted);

        bool GetScrollState();
        #endregion
    }
}
