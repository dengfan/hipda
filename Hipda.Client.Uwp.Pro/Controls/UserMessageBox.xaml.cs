using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Hipda.Client.Uwp.Pro.Controls
{
    public sealed partial class UserMessageBox : UserControl
    {
        public UserMessageBox()
        {
            this.InitializeComponent();
        }

        void EmojiGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = (EmojiItemModel)e.ClickedItem;
            if (data == null)
            {
                return;
            }

            string faceText = data.Label;

            int occurences = 0;
            string originalContent = UserMessageTextBox.Text;

            for (var i = 0; i < UserMessageTextBox.SelectionStart + occurences; i++)
            {
                if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                    occurences++;
            }

            int cursorPosition = UserMessageTextBox.SelectionStart + occurences;
            UserMessageTextBox.Text = UserMessageTextBox.Text.Insert(cursorPosition, faceText);
            UserMessageTextBox.SelectionStart = cursorPosition + faceText.Length;
            UserMessageTextBox.Focus(FocusState.Pointer);
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
            string originalContent = UserMessageTextBox.Text;

            for (var i = 0; i < UserMessageTextBox.SelectionStart + occurences; i++)
            {
                if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                    occurences++;
            }

            int cursorPosition = UserMessageTextBox.SelectionStart + occurences;
            UserMessageTextBox.Text = UserMessageTextBox.Text.Insert(cursorPosition, faceText);
            UserMessageTextBox.SelectionStart = cursorPosition + faceText.Length;
            UserMessageTextBox.Focus(FocusState.Pointer);
        }
    }
}
