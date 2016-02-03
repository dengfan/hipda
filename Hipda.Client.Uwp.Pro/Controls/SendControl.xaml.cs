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
            ContentTextBox.Focus(FocusState.Pointer);
        }

        void AfterUpload(int fileCount)
        {
            TipTextBlock.Text = $"文件上传已完成，共上传 {fileCount} 个文件。";

            // 提示信息，5秒后自动清除
            var now = DateTime.Now;
            while (now.AddSeconds(5) > DateTime.Now)
            {
                TipTextBlock.Text = string.Empty;
            }
        }

        void SentFailed()
        {
            TipTextBlock.Text = "对不起，发布请求失败，请稍后再试！";

            // 提示信息，5秒后自动清除
            var now = DateTime.Now;
            while (now.AddSeconds(5) > DateTime.Now)
            {
                TipTextBlock.Text = string.Empty;
            }
        }
        #endregion

        public SendControl(SendType sendType, int forumId, Action<string> sentSuccess)
        {
            this.InitializeComponent();
            this.DataContext = new NewThreadViewModel(forumId, BeforeUpload, InsertFileCodeIntoContextTextBox, AfterUpload, SentFailed, sentSuccess);
        }


    }
}
