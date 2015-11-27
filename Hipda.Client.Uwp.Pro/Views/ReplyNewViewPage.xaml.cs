using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
    public sealed partial class ReplyNewViewPage : Page
    {
        ViewLifetimeControl _thisViewControl;
        int _mainViewId;
        CoreDispatcher _mainDispatcher;

        int _threadId;
        ReplyViewModel _replyViewModel;

        public ReplyNewViewPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var param = e.Parameter as OpenNewViewParameterModel;
            _thisViewControl = param.NewView;
            _mainViewId = ((App)App.Current).MainViewId;
            _mainDispatcher = ((App)App.Current).MainDispatcher;

            _threadId = param.ThreadId;
            RequestedTheme = param.ElementTheme;

            _replyViewModel = new ReplyViewModel(
                    1,
                    _threadId,
                    0,
                    ReplyListView,
                    () => {
                        rightProgress.IsActive = true;
                        rightProgress.Visibility = Visibility.Visible;
                        ReplyRefreshButton.IsEnabled = false;
                    },
                    (tid) => {
                        rightProgress.IsActive = false;
                        rightProgress.Visibility = Visibility.Collapsed;
                        ReplyRefreshButton.IsEnabled = true;
                    });

            DataContext = _replyViewModel;

            // When this view is finally release, clean up state
            _thisViewControl.Released += ViewLifetimeControl_Released;
        }

        private async void ViewLifetimeControl_Released(Object sender, EventArgs e)
        {
            ((ViewLifetimeControl)sender).Released -= ViewLifetimeControl_Released;
            // The ViewLifetimeControl object is bound to UI elements on the main thread
            // So, the object must be removed from that thread
            await _mainDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ((App)App.Current).SecondaryViews.Remove(_thisViewControl);
            });

            // The released event is fired on the thread of the window
            // it pertains to.
            //
            // It's important to make sure no work is scheduled on this thread
            // after it starts to close (no data binding changes, no changes to
            // XAML, creating new objects in destructors, etc.) since
            // that will throw exceptions
            Window.Current.Close();
        }
    }
}
