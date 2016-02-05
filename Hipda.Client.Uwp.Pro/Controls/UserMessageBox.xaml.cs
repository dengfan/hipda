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
        public int UserId
        {
            get { return (int)GetValue(UserIdProperty); }
            set { SetValue(UserIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserIdProperty =
            DependencyProperty.Register("UserId", typeof(int), typeof(UserMessageBox), new PropertyMetadata(0));


        public delegate void SubmitEventHandler(object sender, EventArgs e);

        public event SubmitEventHandler Submit;


        public UserMessageBox()
        {
            this.InitializeComponent();
        }

        public List<FaceItemModel> FaceData
        {
            get
            {
                return FaceService.FaceData;
            }
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

        private void UserMessagePostButton_Click(object sender, RoutedEventArgs e)
        {
            if (Submit != null)
            {
                Submit(this, EventArgs.Empty);
            }
        }
    }
}
