using Hipda.Client.Uwp.Pro.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class QuickReply : UserControl
    {


        public int ThreadId
        {
            get { return (int)GetValue(ThreadIdProperty); }
            set { SetValue(ThreadIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThreadId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThreadIdProperty =
            DependencyProperty.Register("ThreadId", typeof(int), typeof(QuickReply), new PropertyMetadata(0));



        public QuickReply()
        {
            this.InitializeComponent();

            this.SizeChanged += (s, e) => 
            {
                string userInteractionType = Windows.UI.ViewManagement.UIViewSettings.GetForCurrentView().UserInteractionMode.ToString();
                if (userInteractionType.Equals("Touch"))
                {
                    FaceButton.Width = 80;
                    FaceButton.Height = 40;
                    FileButton.Width = 80;
                    FileButton.Height = 40;
                    SendButton.Width = 80;
                    SendButton.Height = 40;
                }
                else if (userInteractionType.Equals("Mouse"))
                {
                    FaceButton.Width = 36;
                    FaceButton.Height = 32;
                    FileButton.Width = 36;
                    FileButton.Height = 32;
                    SendButton.Width = 80;
                    SendButton.Height = 32;
                }

                ReplyContentTextBox.MaxHeight = this.ActualHeight / 2;
            };
        }

        private void EmojiGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = (EmojiItemModel)e.ClickedItem;
            if (data == null)
            {
                return;
            }

            string faceText = data.Label;

            int occurences = 0;
            string originalContent = ReplyContentTextBox.Text;

            for (var i = 0; i < ReplyContentTextBox.SelectionStart + occurences; i++)
            {
                if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                    occurences++;
            }

            int cursorPosition = ReplyContentTextBox.SelectionStart + occurences;
            ReplyContentTextBox.Text = ReplyContentTextBox.Text.Insert(cursorPosition, faceText);
            ReplyContentTextBox.SelectionStart = cursorPosition + faceText.Length;
        }

        private void FaceGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = (FaceItemModel)e.ClickedItem;
            if (data == null)
            {
                return;
            }

            string faceText = data.Text;

            int occurences = 0;
            string originalContent = ReplyContentTextBox.Text;

            for (var i = 0; i < ReplyContentTextBox.SelectionStart + occurences; i++)
            {
                if (originalContent[i] == '\r' && originalContent[i + 1] == '\n')
                    occurences++;
            }

            int cursorPosition = ReplyContentTextBox.SelectionStart + occurences;
            ReplyContentTextBox.Text = ReplyContentTextBox.Text.Insert(cursorPosition, faceText);
            ReplyContentTextBox.SelectionStart = cursorPosition + faceText.Length;
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (ThreadId > 0)
            {

            }
        }
    }
}
