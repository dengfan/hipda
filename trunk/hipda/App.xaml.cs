using hipda.Common;
using hipda.Data;
using hipda.Settings;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace hipda
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    public sealed partial class App : Application
    {
        private TransitionCollection transitions;
        public static int ThemeId = 0;
        private static int layoutModeId = 0;

#if WINDOWS_PHONE_APP
        ContinuationManager continuationManager;
#endif

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            ThemeId = ThemeSettings.ThemeSetting;
            layoutModeId = LayoutModeSettings.LayoutModeSetting;
        }

        private Frame CreateRootFrame()
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            return rootFrame;
        }

        private async Task RestoreStatusAsync(ApplicationExecutionState previousExecutionState)
        {
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (previousExecutionState == ApplicationExecutionState.Terminated)
            {
                // Restore the saved session state only when appropriate
                try
                {
                    await SuspensionManager.RestoreAsync();
                }
                catch (SuspensionManagerException)
                {
                    //Something went wrong restoring state.
                    //Assume there is no state and continue
                }
            }
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Handle OnActivated event to deal with File Open/Save continuation activation kinds
        /// </summary>
        /// <param name="e">Application activated event arguments, it can be casted to proper sub-type based on ActivationKind</param>
        protected async override void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);

            continuationManager = new ContinuationManager();

            Frame rootFrame = CreateRootFrame();
            await RestoreStatusAsync(e.PreviousExecutionState);

            //Check if this is a continuation
            var continuationEventArgs = e as IContinuationActivatedEventArgs;
            if (continuationEventArgs != null)
                continuationManager.Continue(continuationEventArgs);

            Window.Current.Activate();
        }
#endif

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 当启动应用程序以打开特定的文件或显示搜索结果等操作时，
        /// 将使用其他入口点。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            // 自动登录
            string err = string.Empty;
            try
            {
                await AccountSettings.AutoLogin();
            }
            catch (System.Net.WebException we)
            {
                err = string.Format("请检查网络设置 :( \n", we.Message);
            }

            if (!string.IsNullOrEmpty(err))
            {
                await new MessageDialog(err, "注意").ShowAsync();
            }

            // 倒序看贴
            DataSource.OrderType = SortSettings.GetSortType;

            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态。
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页。
                rootFrame = new Frame();

                // 将框架与 SuspensionManager 键关联。
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                // TODO: 将此值更改为适合您的应用程序的缓存大小。
                //rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // 仅当合适时才还原保存的会话状态。
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        // 还原状态时出现问题。
                        // 假定没有状态并继续。
                    }
                }

                // 将框架放在当前窗口中。
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // 删除用于启动的旋转门导航。
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;

                // 当导航堆栈尚未还原时，导航到第一页，
                // 并通过将所需信息作为导航参数传入来配置
                // 新页面。
                if (!rootFrame.Navigate(typeof(HomePage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // 确保当前窗口处于活动状态。
            Window.Current.Activate();

            StatusBar statusBar = StatusBar.GetForCurrentView();

            switch (layoutModeId)
            {
                case 0: // 经典模式
                    switch (ThemeId)
                    {
                        case 0:
                            statusBar.BackgroundColor = Colors.Purple;
                            break;
                        case 1:
                            statusBar.BackgroundColor = Colors.Black;
                            break;
                        case 2:
                            statusBar.BackgroundColor = Colors.DarkRed;
                            break;
                        case 3:
                            statusBar.BackgroundColor = Color.FromArgb(255, 138, 107, 121);
                            break;
                    }
                    break;
                case 1: // 纯文本模式
                    switch (ThemeId)
                    {
                        case 0:
                            statusBar.BackgroundColor = Colors.Purple;
                            break;
                        case 1:
                            statusBar.BackgroundColor = Colors.Black;
                            break;
                        case 2:
                            statusBar.BackgroundColor = Colors.DarkRed;
                            break;
                        case 3:
                            statusBar.BackgroundColor = Color.FromArgb(255, 138, 107, 121);
                            break;
                    }
                    break;
                case 2: // 气泡模式
                    switch (ThemeId)
                    {
                        case 0:
                            statusBar.BackgroundColor = Colors.Purple;
                            break;
                        case 1:
                            statusBar.BackgroundColor = Colors.Black;
                            break;
                        case 2:
                            statusBar.BackgroundColor = Color.FromArgb(255, 108, 151, 193);
                            break;
                        case 3:
                            statusBar.BackgroundColor = Color.FromArgb(255, 7, 18, 40);
                            break;
                    }
                    break;
            }
            

            statusBar.BackgroundOpacity = 255;
            statusBar.ForegroundColor = Colors.White;
            statusBar.ProgressIndicator.ProgressValue = 0;
            await statusBar.ShowAsync();

            if (e.PreviousExecutionState == ApplicationExecutionState.NotRunning)
            {
                statusBar.ProgressIndicator.Text = string.Concat("Hi!PDA");
                await statusBar.ProgressIndicator.ShowAsync();
            }
        }

        /// <summary>
        /// 启动应用程序后还原内容转换。
        /// </summary>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }
    }
}
