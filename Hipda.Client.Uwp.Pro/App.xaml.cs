﻿using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using Hipda.Client.Uwp.Pro.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Hipda.Client.Uwp.Pro
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        Frame _rootFrame;
        bool _isLaunched = false;

        async void RegisterBackgroundTask()
        {
            // 判断一下是否允许访问后台任务
            var res = await BackgroundExecutionManager.RequestAccessAsync();
            if (res == BackgroundAccessStatus.Denied || res == BackgroundAccessStatus.Unspecified)
            {
                return;
            }

            Type taskType = typeof(BackgroundTask.UpdateToastTask);
            var task = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(t => t.Name == taskType.Name);
            if (task == null)
            {
                // 注册后台任务
                BackgroundTaskBuilder bd = new BackgroundTaskBuilder();
                bd.Name = taskType.Name;
                bd.TaskEntryPoint = taskType.FullName;

                // 声明触发器
                TimeTrigger trigger = new TimeTrigger(15, false);
                bd.SetTrigger(trigger);
                bd.Register();
            }

            Type taskType2 = typeof(BackgroundTask.HandleToastActionTask);
            var task2 = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(t => t.Name == taskType2.Name);
            if (task2 == null)
            {
                // 注册后台任务
                BackgroundTaskBuilder bd = new BackgroundTaskBuilder();
                bd.Name = taskType2.Name;
                bd.TaskEntryPoint = taskType2.FullName;

                // 声明触发器
                bd.SetTrigger(new ToastNotificationActionTrigger());
                bd.Register();
            }
        }

        //public ObservableCollection<ViewLifetimeControl> SecondaryViews = new ObservableCollection<ViewLifetimeControl>();

        private CoreDispatcher mainDispatcher;
        public CoreDispatcher MainDispatcher
        {
            get
            {
                return mainDispatcher;
            }
        }

        private int mainViewId;
        public int MainViewId
        {
            get
            {
                return mainViewId;
            }
        }

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }
       
        /// <summary>
        /// 读取设置数据并启用
        /// </summary>
        void InitSettings()
        {
            // 恢复本地设置
            new LocalSettingsService().ReadAndUpdate();

            // 恢复漫游设置
            RoamingSettingsService.ReadAndUpdate();
        }

        async Task<bool> CreateRootFrame()
        {
            // 自动登录
            var accountService = new AccountService();
            bool isLogin = await accountService.AutoLogin();

            _rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (_rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                _rootFrame = new Frame();

                _rootFrame.NavigationFailed += OnNavigationFailed;

                mainDispatcher = Window.Current.Dispatcher;
                mainViewId = ApplicationView.GetForCurrentView().Id;

                // 将框架放在当前窗口中
                Window.Current.Content = _rootFrame;
            }

            if (_rootFrame.Content == null)
            {
                // 当导航堆栈尚未还原时，导航到第一页，
                // 并通过将所需信息作为导航参数传入来配置
                // 参数
                if (isLogin)
                {
                    _rootFrame.Navigate(typeof(MainPage), "fid=2");
                }
                else
                {
                    _rootFrame.Navigate(typeof(LoginPage));
                }
            }
            // 确保当前窗口处于活动状态
            Window.Current.Activate();

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size { Width = 320, Height = 320 });

            return isLogin;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            await CreateRootFrame();

            _isLaunched = true;

            this.InitSettings();

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                //var applicationView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                //applicationView.SetDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.UseCoreWindow);

                await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
                //var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                //statusBar.BackgroundColor = ((SolidColorBrush)Resources["SystemControlBackgroundChromeMediumBrush"]).Color;
                //statusBar.BackgroundOpacity = 1;
            }

            //var c = ((SolidColorBrush)Resources["SystemControlBackgroundAccentBrush"]).Color;
            //var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            //titleBar.BackgroundColor = c;
            //titleBar.InactiveBackgroundColor = c;
            //titleBar.ButtonBackgroundColor = c;
            //titleBar.ButtonInactiveBackgroundColor = c;
            //titleBar.ForegroundColor = Colors.White;
            //titleBar.ButtonForegroundColor = Colors.White;

            // 注册后台任务
            RegisterBackgroundTask();
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            var flag = await CreateRootFrame();
            if (flag == false)
            {
                return;
            }

            this.InitSettings();

            if (args.Kind == ActivationKind.Protocol)
            {
                var eventArgs = args as ProtocolActivatedEventArgs;
                if (eventArgs.Uri.Scheme == "hipda")
                {
                    string uri = eventArgs.Uri.AbsoluteUri;
                    // 在新窗口中打开指定的回复列表
                    //if (uri.StartsWith("hipda:tid="))
                    //{
                    //    int tid = Convert.ToInt32(uri.Substring("hipda:tid=".Length));
                    //    await OpenThreadInNewView(tid);
                    //}
                }
            }
            else if (args.Kind == ActivationKind.ToastNotification)
            {
                var eventArgs = ((ToastNotificationActivatedEventArgs)args).Argument;
                if (eventArgs.StartsWith("view_post="))
                {
                    string[] tary = eventArgs.Substring("view_post=".Length).Split(',');
                    int postId = Convert.ToInt32(tary[0]);
                    int threadId = Convert.ToInt32(tary[1]);
                    if (postId > 0 && threadId > 0)
                    {
                        var mainPage = _rootFrame.Content as MainPage;
                        if (mainPage != null)
                        {
                            var threadAndReplyPage = mainPage.AppFrame.Content as ThreadAndReplyPage;
                            if (threadAndReplyPage != null)
                            {
                                threadAndReplyPage.OpenReplyPageByThreadId(postId, threadId);
                            }
                        }
                        
                    }
                }
                else if (eventArgs.StartsWith("view_pm="))
                {
                    string[] tary = eventArgs.Substring("view_pm=".Length).Split(',');
                    int userId = Convert.ToInt32(tary[0]);
                    string username = tary[1];
                    if (userId > 0)
                    {
                        MainPage.PopupUserId = userId;
                        MainPage.PopupUsername = username;
                        var mainPage = _rootFrame.Content as MainPage;
                        if (_isLaunched)
                        {
                            mainPage.OpenUserMessageDialog();
                        }
                        else
                        {
                            mainPage.Loaded += (s, e) => {
                                mainPage.OpenUserMessageDialog();
                            };
                        }
                    }
                }
            }
        }

        //private async Task OpenThreadInNewView(int threadId)
        //{
        //    ViewLifetimeControl viewControl = null;
        //    await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //    {
        //        // This object is used to keep track of the views and important
        //        // details about the contents of those views across threads
        //        // In your app, you would probably want to track information
        //        // like the open document or page inside that window
        //        viewControl = ViewLifetimeControl.CreateForCurrentView();
        //        viewControl.Title = "坐和放宽";
        //        // Increment the ref count because we just created the view and we have a reference to it                
        //        viewControl.StartViewInUse();

        //        var param = new OpenNewViewParameterModel
        //        {
        //            ElementTheme = ElementTheme.Dark,
        //            ThreadId = threadId,
        //            NewView = viewControl
        //        };
        //        var frame = new Frame();
        //        frame.Navigate(typeof(ReplyNewViewPage), param);
        //        Window.Current.Content = frame;
        //        Window.Current.Activate();
        //    });

        //    // Be careful! This collection is bound to the current thread,
        //    // so make sure to update it only from this thread
        //    ((App)App.Current).SecondaryViews.Add(viewControl);

        //    var selectedView = viewControl;
        //    var sizePreference = ViewSizePreference.Default;
        //    var anchorSizePreference = ViewSizePreference.Default;

        //    if (viewControl != null)
        //    {
        //        try
        //        {
        //            // Prevent the view from closing while
        //            // switching to it
        //            selectedView.StartViewInUse();

        //            // Show the previously created secondary view, using the size
        //            // preferences the user specified. In your app, you should
        //            // choose a size that's best for your scenario and code it,
        //            // instead of requiring the user to decide.
        //            var viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
        //                selectedView.Id,
        //                sizePreference,
        //                ApplicationView.GetForCurrentView().Id,
        //                anchorSizePreference);

        //            // Signal that switching has completed and let the view close
        //            selectedView.StopViewInUse();
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            // The view could be in the process of closing, and
        //            // this thread just hasn't updated. As part of being closed,
        //            // this thread will be informed to clean up its list of
        //            // views (see SecondaryViewPage.xaml.cs)
        //        }
        //    }
        //}
    }
}
