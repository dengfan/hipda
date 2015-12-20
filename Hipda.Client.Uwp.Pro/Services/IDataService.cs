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


        void ClearThreadData(int forumId);

        void ClearThreadDataForMyThreads();

        void ClearThreadDataForMyPosts();


        ThreadItemModel GetThreadItem(int threadId);

        ThreadItemForMyThreadsModel GetThreadItemForMyThreads(int threadId);

        ThreadItemForMyPostsModel GetThreadItemForMyPosts(int threadId);


        bool IsRead(int threadId);

        string GetThreadTitleFromReplyData(int threadId);
        #endregion

        #region reply
        ICollectionView GetViewForReplyPageByThreadId(int startPageNo, int threadId, int threadAuthorUserId, Action beforeLoad, Action<int, int> afterLoad);

        ICollectionView GetViewForRedirectReplyPageByThreadId(int startPageNo, int threadId, int threadAuthorUserId, int postId, Action beforeLoad, Action<int, int> afterLoad, Action<int> listViewScroll);

        Task<int[]> LoadReplyDataForRedirectReplyPageAsync(int threadId, int targetPostId, CancellationTokenSource cts);

        int GetReplyMaxPageNo();

        int GetReplyMinPageNoInLoadedData(int threadId);

        void ClearReplyData(int threadId);

        void SetScrollState(bool isCompleted);

        bool GetScrollState();

        Task<ReplyItemModel> GetPostDetail(int postId, int threadId);
        #endregion

        #region user
        Task<string> GetXamlForUserInfo(int userId);

        Task<List<UserMessageItemModel>> GetUserMessageData(int userId, int limitCount);

        Task<bool> PostUserMessage(string message, int userId);
        #endregion
    }
}
