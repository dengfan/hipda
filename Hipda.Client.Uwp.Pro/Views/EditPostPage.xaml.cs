using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Hipda.Client.Uwp.Pro.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class EditPostPage : Page
    {
        public EditPostPage()
        {
            this.InitializeComponent();
        }

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

        SendEditPostPageViewModel _vm;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var editData = (PostEditDataModel)e.Parameter;
            var cts = new CancellationTokenSource();
            _vm = new SendEditPostPageViewModel(cts, editData, BeforeUpload, InsertFileCodeIntoContextTextBox, AfterUpload, null, null);
            this.DataContext = _vm;
        }

        private void ImageAttachPanel_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "拖放到此处即可上传文件";
        }

        private async void ImageAttachPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var files = await e.DataView.GetStorageItemsAsync();
                var cts = new CancellationTokenSource();
                _vm.DragAndUploadFile(cts, files, BeforeUpload, AfterUpload);
            }
        }
    }
}
