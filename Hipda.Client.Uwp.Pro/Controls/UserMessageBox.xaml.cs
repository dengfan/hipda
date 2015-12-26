using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
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

        private void UserMessageFaceGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as FaceItemModel;
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
