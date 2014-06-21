using hipda.Common;
using hipda.Data;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// “透视应用程序”模板在 http://go.microsoft.com/fwlink/?LinkID=391641 上有介绍

namespace hipda
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    public sealed partial class App : Application
    {
        private TransitionCollection transitions;

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
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
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            // 自动登录
            string err = string.Empty;
            try
            {
                await AccountHelper.AutoLogin();
            }
            catch (System.Net.WebException we)
            {
                err = string.Format("请检查网络设置 :( \n", we.Message);
            }

            if (!string.IsNullOrEmpty(err))
            {
                await new MessageDialog(err, "注意").ShowAsync();
            }

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
                //// 删除用于启动的旋转门导航。
                //if (rootFrame.ContentTransitions != null)
                //{
                //    this.transitions = new TransitionCollection();
                //    foreach (var c in rootFrame.ContentTransitions)
                //    {
                //        this.transitions.Add(c);
                //    }
                //}

                //rootFrame.ContentTransitions = null;
                //rootFrame.Navigated += this.RootFrame_FirstNavigated;

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
            statusBar.BackgroundColor = Colors.Purple;
            statusBar.BackgroundOpacity = 100;
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
        //private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        //{
        //    var rootFrame = sender as Frame;
        //    rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
        //    rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        //}

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }
    }
}
