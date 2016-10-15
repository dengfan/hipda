using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace Hipda.Client.Controls
{
    public sealed class MyImage : Control
    {
        public MyImage()
        {
            this.DefaultStyleKey = typeof(MyImage);
        }

        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Url.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UrlProperty =
            DependencyProperty.Register("Url", typeof(string), typeof(MyImage), new PropertyMetadata(string.Empty));


        public string FolderName
        {
            get { return (string)GetValue(FolderNameProperty); }
            set { SetValue(FolderNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FolderName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FolderNameProperty =
            DependencyProperty.Register("FolderName", typeof(string), typeof(MyImage), new PropertyMetadata(string.Empty));


        bool _isCommon
        {
            get
            {
                return Url.Contains("hi-pda.com/forum/images/") || Url.Equals("http://www.hi-pda.com/forum/attachments/day_140621/1406211752793e731a4fec8f7b.png");
            }
        }

        bool _isGif
        {
            get
            {
                return Url.ToLower().EndsWith(".gif");
            }
        }

        StorageFile _file;
        StorageFolder _folder;

        protected async override void OnTapped(TappedRoutedEventArgs e)
        {
            base.OnTapped(e);

            if (!_isCommon)
            {
                await OpenPhoto();
            }
        }

        private async Task OpenPhoto()
        {
            var fileTypeFilter = new List<string>();
            fileTypeFilter.Add(".jpg");
            fileTypeFilter.Add(".jpeg");
            fileTypeFilter.Add(".png");
            fileTypeFilter.Add(".bmp");
            fileTypeFilter.Add(".gif");
            var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter);
            var query = _folder.CreateFileQueryWithOptions(queryOptions);
            var options = new LauncherOptions();
            options.NeighboringFilesQuery = query;
            await Launcher.LaunchFileAsync(_file, options);
        }

        protected async override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _folder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("hipda", CreationCollisionOption.OpenIfExists);
            if (_isCommon)
            {
                _folder = await _folder.CreateFolderAsync("common", CreationCollisionOption.OpenIfExists); // 为公共图片创建一个文件夹
            }
            else
            {
                _folder = await _folder.CreateFolderAsync(FolderName, CreationCollisionOption.OpenIfExists); // 为当前主题创建一个文件夹
            }

            ContentControl content1 = GetTemplateChild("content1") as ContentControl;
            var myDependencyObject = (LocalSettingsDependencyObject)App.Current.Resources["MyLocalSettings"];
            Binding pictureOpacityBinding = new Binding { Source = myDependencyObject, Path = new PropertyPath("PictureOpacity") };

            Image img = new Image();
            img.SetBinding(Image.OpacityProperty, pictureOpacityBinding);
            img.Stretch = Stretch.None;
            img.ImageFailed += (s, e) =>
            {
                return;
            };

            string[] urlAry = Url.Split('/');
            string fileFullName = urlAry.Last();
            try
            {
                IStorageItem existsFile = await _folder.TryGetItemAsync(fileFullName);
                if (existsFile != null)
                {
                    _file = existsFile as StorageFile;
                }
                else
                {
                    // 不存在则请求
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(new Uri(Url));
                        var buf = await response.Content.ReadAsBufferAsync();
                        _file = await _folder.CreateFileAsync(fileFullName, CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteBufferAsync(_file, buf);
                    }
                }
            }
            catch
            {

            }

            try
            {
                if (_folder != null && _file != null)
                {
                    if (_isCommon)
                    {
                        img.Stretch = Stretch.None;
                        var bm = new BitmapImage();
                        bm.UriSource = new Uri(Url, UriKind.Absolute);
                        img.Source = bm;
                    }
                    else
                    {
                        using (IRandomAccessStream fileStream = await _file.OpenAsync(FileAccessMode.Read))
                        {
                            if (fileStream != null)
                            {
                                BitmapImage bitmapImg = new BitmapImage();
                                await bitmapImg.SetSourceAsync(fileStream);
                                int imgWidth = bitmapImg.PixelWidth;
                                int imgHeight = bitmapImg.PixelHeight;

                                if (_isGif)
                                {
                                    if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Mobile"))
                                    {
                                        // 移动端用Button显示Gif
                                        img.Stretch = Stretch.Uniform;
                                        img.MaxWidth = imgWidth;
                                        img.Source = bitmapImg;

                                        SymbolIcon gifSymbolIcon = new SymbolIcon();
                                        gifSymbolIcon.Symbol = Symbol.Play;
                                        gifSymbolIcon.HorizontalAlignment = HorizontalAlignment.Center;
                                        gifSymbolIcon.VerticalAlignment = VerticalAlignment.Center;

                                        Grid gifGrid = new Grid();
                                        gifGrid.Children.Add(img);
                                        gifGrid.Children.Add(gifSymbolIcon);

                                        Button gifButton = new Button();
                                        gifButton.Padding = new Thickness(0);
                                        gifButton.Content = gifGrid;

                                        content1.Content = gifButton;
                                    }
                                    else if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily.Equals("Windows.Desktop"))
                                    {
                                        // PC端用WebView显示Gif图片
                                        WebView webView = new WebView();
                                        webView.SetBinding(WebView.OpacityProperty, pictureOpacityBinding);
                                        webView.DefaultBackgroundColor = Colors.Transparent;
                                        webView.Width = imgWidth;
                                        webView.Height = imgHeight;
                                        webView.ScriptNotify += async (s, e) =>
                                        {
                                            await OpenPhoto();
                                        };

                                        string imgHtml = @"<html><head><meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0""></head><body style=""margin:0;padding:0;"" onclick=""window.external.notify('go');""><img src=""{0}"" alt=""Gif Image"" /></body></html>";
                                        imgHtml = string.Format(imgHtml, Url);
                                        webView.NavigateToString(imgHtml);

                                        content1.Content = webView;
                                    }
                                }
                                else // 其它图片，使用Image控件显示
                                {
                                    img.MaxWidth = imgWidth;
                                    img.Stretch = Stretch.UniformToFill;
                                    img.Source = bitmapImg;
                                }
                            }
                        }
                    }
                }

                if (_isCommon || !_isGif) // 公共或非gif图片，使用Image控件显示
                {
                    content1.Content = img;
                }
            }
            catch
            {
                
            }
        }
    }
}
