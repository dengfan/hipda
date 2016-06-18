using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Hipda.Client.Uwp.Pro.Controls
{
    /// <summary>
    /// 发送信息控件
    /// 用于发贴、改贴、回贴
    /// </summary>
    public sealed partial class SendControl : UserControl
    {
        #region UI事件
        TextBox _currentTextBox;

        void EmojiGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_currentTextBox == null)
            {
                _currentTextBox = ContentTextBox;
            }

            var data = (EmojiItemModel)e.ClickedItem;
            if (data == null)
            {
                return;
            }

            string faceText = data.Label;

            if (_currentTextBox.Name.Equals("TitleTextBox") && _currentTextBox.Text.Length >= 50)
            {
                return;
            }

            int occurences = 0;
            string originalContent = _currentTextBox.Text;

            for (var i = 0; i < _currentTextBox.SelectionStart + occurences; i++)
            {
                if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                    occurences++;
            }

            int cursorPosition = _currentTextBox.SelectionStart + occurences;
            _currentTextBox.Text = _currentTextBox.Text.Insert(cursorPosition, faceText);
            _currentTextBox.SelectionStart = cursorPosition + faceText.Length;
            _currentTextBox.Focus(FocusState.Pointer);
        }

        void FaceGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = (FaceItemModel)e.ClickedItem;
            if (data == null)
            {
                return;
            }

            string faceText = data.Text;

            int occurences = 0;
            string originalContent = ContentTextBox.Text;

            for (var i = 0; i < ContentTextBox.SelectionStart + occurences; i++)
            {
                if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                    occurences++;
            }

            int cursorPosition = ContentTextBox.SelectionStart + occurences;
            ContentTextBox.Text = ContentTextBox.Text.Insert(cursorPosition, faceText);
            ContentTextBox.SelectionStart = cursorPosition + faceText.Length;
            ContentTextBox.Focus(FocusState.Pointer);
        }

        void TitleTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _currentTextBox = (TextBox)sender;
        }

        void ContentTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _currentTextBox = (TextBox)sender;
        }
        #endregion

        #region 委托事件
        void BeforeUpload(int fileIndex, int fileCount, string fileName)
        {
            TipsBarTextBlock.Text = $"上载中 {fileIndex}/{fileCount} （{fileName}）";
        }

        void InsertFileCodeIntoContextTextBox(string fileCode)
        {
            int occurences = 0;
            string originalContent = ContentTextBox.Text;

            for (var i = 0; i < ContentTextBox.SelectionStart + occurences; i++)
            {
                if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                    occurences++;
            }

            int cursorPosition = ContentTextBox.SelectionStart + occurences;
            ContentTextBox.Text = ContentTextBox.Text.Insert(cursorPosition, fileCode);
            ContentTextBox.SelectionStart = cursorPosition + fileCode.Length;
            ContentTextBox.Focus(FocusState.Programmatic);
        }

        void AfterUpload(int fileCount)
        {
            TipsBarTextBlock.Text = $"文件上传已完成，共上传 {fileCount} 个文件。";
        }

        void SentFailed(string errorText)
        {
            TipsBarTextBlock.Text = errorText;
        }
        #endregion

        public int Countdown
        {
            get { return (int)GetValue(CountdownProperty); }
            set { SetValue(CountdownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Countdown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountdownProperty =
            DependencyProperty.Register("Countdown", typeof(int), typeof(SendControl), new PropertyMetadata(0));


        /// <summary>
        /// 初始化为发贴控件
        /// </summary>
        /// <param name="sendType"></param>
        /// <param name="forumId"></param>
        /// <param name="sentSuccess"></param>
        public SendControl(SendType sendType, int forumId, Action<string> sentSuccess)
        {
            this.InitializeComponent();

            TipsBarTextBlock.Text = "请输入标题和内容。";
            var cts = new CancellationTokenSource();
            this.DataContext = new SendNewThreadContentDialogViewModel(cts, forumId, BeforeUpload, InsertFileCodeIntoContextTextBox, AfterUpload, SentFailed, sentSuccess);
        }

        /// <summary>
        /// 初始化为回贴控件
        /// </summary>
        /// <param name="sendType"></param>
        /// <param name="postAuthorUserId"></param>
        /// <param name="postAuthorUsername"></param>
        /// <param name="postSimpleContent"></param>
        /// <param name="floorNo"></param>
        /// <param name="postId"></param>
        /// <param name="threadId"></param>
        /// <param name="sentSuccess"></param>
        public SendControl(string replyType, int postAuthorUserId, string postAuthorUsername, string postSimpleContent, string postTime, int floorNo, int postId, int threadId, Action<string> sentSuccess)
        {
            this.InitializeComponent();

            TitleTextBox.Visibility = Visibility.Collapsed;
            TipsBarTextBlock.Text = "请输入回复内容。";
            var cts = new CancellationTokenSource();
            this.DataContext = new SendPostReplyContentDialogViewModel(cts, replyType, postAuthorUserId, postAuthorUsername, postSimpleContent, postTime, floorNo, postId, threadId, BeforeUpload, InsertFileCodeIntoContextTextBox, AfterUpload, SentFailed, sentSuccess);
        }

        /// <summary>
        /// 初始化为改贴控件
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="postId"></param>
        /// <param name="threadId"></param>
        /// <param name="sentSuccess"></param>
        //public SendControl(string title, string content, int postId, int threadId, Action<string> sentSuccess)
        //{
        //    this.InitializeComponent();

        //    TipsBarTextBlock.Text = "请输入更新内容。";
        //    var cts = new CancellationTokenSource();
        //    this.DataContext = new SendEditPostPageViewModel(cts, title, content, postId, threadId, BeforeUpload, InsertFileCodeIntoContextTextBox, AfterUpload, SentFailed, sentSuccess);
        //}
    }
}
