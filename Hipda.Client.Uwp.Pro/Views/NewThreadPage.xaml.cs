using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class NewThreadPage : Page
    {
        public NewThreadPage()
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
    }
}
