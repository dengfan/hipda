using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace Hipda.Client.Uwp.Pro.Controls
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
            DependencyProperty.Register("FolderName", typeof(string), typeof(MyImage), new PropertyMetadata(0));


        public bool isCommonImage
        {
            get
            {
                return Url.Contains("hi-pda.com/forum/images/") || Url.Equals("http://www.hi-pda.com/forum/attachments/day_140621/1406211752793e731a4fec8f7b.png");
            }
        }

        public bool isGif
        {
            get
            {
                return Url.ToLower().EndsWith(".gif");
            }
        }

        private StorageFile file;
        private StorageFolder folder;

        protected async override void OnTapped(TappedRoutedEventArgs e)
        {
            base.OnTapped(e);

            if (!isCommonImage)
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
            var query = folder.CreateFileQueryWithOptions(queryOptions);
            var options = new LauncherOptions();
            options.NeighboringFilesQuery = query;
            await Launcher.LaunchFileAsync(file, options);
        }

        protected async override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            try
            {
                folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("hipda", CreationCollisionOption.OpenIfExists);
                if (isCommonImage)
                {
                    folder = await folder.CreateFolderAsync("common", CreationCollisionOption.OpenIfExists); // 为公共图片创建一个文件夹
                }
                else
                {
                    folder = await folder.CreateFolderAsync(FolderName, CreationCollisionOption.OpenIfExists); // 为当前主题创建一个文件夹
                }

                ContentControl content1 = GetTemplateChild("content1") as ContentControl;

                Image img = new Image();
                img.Stretch = Stretch.None;
                img.ImageFailed += (s, e) =>
                {
                    return;
                };

                string[] urlAry = Url.Split('/');
                string fileFullName = urlAry.Last();
                IStorageItem existsFile = await folder.TryGetItemAsync(fileFullName);
                if (existsFile != null)
                {
                    file = existsFile as StorageFile;
                }
                else
                {
                    // 不存在则请求
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(new Uri(Url));
                        string statusCode = response.ReasonPhrase;
                        var buf = await response.Content.ReadAsBufferAsync();
                        byte[] bytes = WindowsRuntimeBufferExtensions.ToArray(buf, 0, (int)buf.Length);
                        file = await folder.CreateFileAsync(fileFullName, CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteBytesAsync(file, bytes);
                    }
                }

                if (folder != null && file != null)
                {
                    if (isCommonImage)
                    {
                        img.Stretch = Stretch.None;
                        var bm = new BitmapImage();
                        bm.UriSource = new Uri(Url, UriKind.Absolute);
                        img.Source = bm;
                    }
                    else
                    {
                        BitmapImage bitmapImg = new BitmapImage();
                        IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                        if (fileStream != null)
                        {
                            await bitmapImg.SetSourceAsync(fileStream);
                            int imgWidth = bitmapImg.PixelWidth;
                            int imgHeight = bitmapImg.PixelHeight;

                            if (isGif) // GIF图片且不是论坛表情图标，则使用WebView控件显示
                            {
                                WebView webView = new WebView();
                                webView.DefaultBackgroundColor = Colors.Transparent;
                                webView.Width = imgWidth;
                                webView.Height = imgHeight;
                                webView.ScriptNotify += async (s, e) =>
                                {
                                    await OpenPhoto();
                                };

                                string imgHtml = @"<html><body style=""margin:0;padding:0;"" onclick=""window.external.notify('go');""><img src=""{0}"" alt=""GIF Image"" /></body></html>";
                                imgHtml = string.Format(imgHtml, Url);
                                webView.NavigateToString(imgHtml);

                                content1.Content = webView;
                            }
                            else // 其它图片，使用Image控件显示
                            {
                                if (imgWidth > 900)
                                {
                                    img.Stretch = Stretch.Uniform;
                                    img.MaxWidth = 1000;
                                }
                                else if (imgWidth > 400)
                                {
                                    img.Stretch = Stretch.Uniform;
                                    img.MaxWidth = 600;
                                }
                                else
                                {
                                    img.Stretch = Stretch.None;
                                }
                                img.Source = bitmapImg;
                            }
                        }
                    }
                }

                if (isCommonImage || !isGif) // 公共或非gif图片，使用Image控件显示
                {
                    content1.Content = img;
                }
            }
            catch { }
        }
    }
}
