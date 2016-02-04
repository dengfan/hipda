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
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Hipda.Client.Uwp.Pro.Controls
{
    public sealed partial class QuickReplyControl : UserControl
    {
        #region UI事件
        void EmojiGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = (EmojiItemModel)e.ClickedItem;
            if (data == null)
            {
                return;
            }

            string faceText = data.Label;

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
        #endregion

        #region 委托事件
        void BeforeUpload(int fileIndex, int fileCount, string fileName)
        {
            TipTextBlock.Text = $"上载中 {fileIndex}/{fileCount} （{fileName}）";
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
            TipTextBlock.Text = $"文件上传已完成，共上传 {fileCount} 个文件。";
        }

        void SentFailed(string errorText)
        {
            TipTextBlock.Text = errorText;
        }
        #endregion


        public QuickReplyControl(int threadId, Action<string> sentSuccess)
        {
            this.InitializeComponent();

            this.SizeChanged += (s, e) => 
            {
                string userInteractionType = Windows.UI.ViewManagement.UIViewSettings.GetForCurrentView().UserInteractionMode.ToString();
                if (userInteractionType.Equals("Touch"))
                {
                    EmojiButton.Width = 80;
                    EmojiButton.Height = 40;
                    FaceButton.Width = 80;
                    FaceButton.Height = 40;
                    FileButton.Width = 80;
                    FileButton.Height = 40;
                    SendButton.Height = 40;
                }
                else if (userInteractionType.Equals("Mouse"))
                {
                    EmojiButton.Width = 32;
                    EmojiButton.Height = 32;
                    FaceButton.Width = 32;
                    FaceButton.Height = 32;
                    FileButton.Width = 32;
                    FileButton.Height = 32;
                    SendButton.Height = 32;
                }

                ContentTextBox.MaxHeight = this.ActualHeight / 2;
            };

            var cts = new CancellationTokenSource();
            this.DataContext = new SendThreadQuickReplyControlViewModel(cts, threadId, BeforeUpload, InsertFileCodeIntoContextTextBox, AfterUpload, SentFailed, sentSuccess);
        }
    }
}
