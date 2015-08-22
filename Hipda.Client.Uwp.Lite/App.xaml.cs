using HipdaUwpLite.Common;
using HipdaUwpLite.Data;
using HipdaUwpLite.Settings;
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

namespace Hipda.Client.Uwp.Lite
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    public sealed partial class App : Application
    {
        private TransitionCollection transitions;
        public static int ThemeId = 0;
        private static int layoutModeId = 0;

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            this.Resuming += this.OnResuming;

            ThemeId = ThemeSettings.ThemeSetting;
            layoutModeId = LayoutModeSettings.LayoutModeSetting;
        }

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
                this.DebugSettings.EnableFrameRateCounter = false;
            }
#endif

            // 自动登录
            await AutoLogin();

            // 排序开关
            DataSource.ThreadListPageOrderBy = SortForThreadSettings.GetSortType;

            // 倒序看贴
            DataSource.RelayListPageOrderType = SortForReplySettings.GetSortType;

            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态。
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页。
                rootFrame = new Frame();

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

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();

                switch (layoutModeId)
                {
                    case 0: // 经典模式
                        switch (ThemeId)
                        {
                            case 0:
                                statusBar.BackgroundColor = ((SolidColorBrush)this.Resources["SystemControlBackgroundAccentBrush"]).Color;
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
                                statusBar.BackgroundColor = ((SolidColorBrush)this.Resources["SystemControlBackgroundAccentBrush"]).Color;
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
                                statusBar.BackgroundColor = ((SolidColorBrush)this.Resources["SystemControlBackgroundAccentBrush"]).Color;
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
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        private async void OnResuming(object sender, object e)
        {
            await AutoLogin();
        }

        private async Task AutoLogin()
        {
            #region 每次恢复时自动登录一下
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
            #endregion
        }
    }
}
