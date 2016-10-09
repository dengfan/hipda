using GalaSoft.MvvmLight.Command;
using Hipda.Client.Models;
using Hipda.Client.Services;
using Hipda.Client.Views;
using System;
using System.Threading;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Hipda.Client.ViewModels
{
    public class NoticeItemViewModel : NoticeItemModel
    {
        public ICommand ReplyCommand { get; set; }
        public ICommand ViewCommand { get; set; }
        public ICommand AddBuddyCommand { get; set; }

        public NoticeItemViewModel(NoticeType noticeType, bool isNew, string username, string actionTime, string[] actionInfo)
        {
            NoticeType = noticeType;
            IsNew = isNew;
            Username = username;
            ActionTime = actionTime;
            ActionInfo = actionInfo;

            var frame = (Frame)Window.Current.Content;
            var mainPage = (MainPage)frame.Content;
            if (mainPage == null)
            {
                return;
            }

            switch (NoticeType)
            {
                case NoticeType.QuoteOrReply:
                    ReplyCommand = new RelayCommand(() =>
                    {
                        int userId = Convert.ToInt32(ActionInfo[0]);
                        string quoteSimpleContent = ActionInfo[3];
                        string quoteTime = ActionTime;
                        int postId = Convert.ToInt32(ActionInfo[6]);
                        int threadId = Convert.ToInt32(ActionInfo[1]);
                        //mainPage.OpenSendReplyPostPanel("q", userId, username, quoteSimpleContent, quoteTime, 0, postId, threadId);
                    });
                    ViewCommand = new RelayCommand(() =>
                    {
                        int postId = Convert.ToInt32(ActionInfo[6]);
                        var threadAndReplyPage = (ThreadAndReplyPage)mainPage.AppFrame.Content;
                        if (threadAndReplyPage != null)
                        {
                            threadAndReplyPage.PostId = postId;
                            threadAndReplyPage.OpenReplyPageByPostId();
                        }
                    });
                    break;
                case NoticeType.Thread:
                    ViewCommand = new RelayCommand(() =>
                    {
                        int postId = Convert.ToInt32(ActionInfo[2]);
                        var threadAndReplyPage = (ThreadAndReplyPage)mainPage.AppFrame.Content;
                        if (threadAndReplyPage != null)
                        {
                            threadAndReplyPage.PostId = postId;
                            threadAndReplyPage.OpenReplyPageByPostId();
                        }
                    });
                    break;
                case NoticeType.Buddy:
                    AddBuddyCommand = new RelayCommand(async () =>
                    {
                        int userId = Convert.ToInt32(ActionInfo[0]);
                        var cts = new CancellationTokenSource();
                        await SendService.SendAddBuddyActionAsync(cts, userId, username);
                    });
                    break;
            }
        }
    }
}
