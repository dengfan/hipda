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

        public string Url { get; set; }


        public string FolderName { get; set; }


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

            var content1 = GetTemplateChild("content1") as ContentControl;
            var myDependencyObject = (LocalSettingsDependencyObject)App.Current.Resources["MyLocalSettings"];
            Binding pictureOpacityBinding = new Binding { Source = myDependencyObject, Path = new PropertyPath("PictureOpacity") };

            var bi = new BitmapImage { UriSource = new Uri(Url) };
            var img = new Image();
            img.SetBinding(OpacityProperty, pictureOpacityBinding);
            img.Source = bi;
            img.ImageOpened += Img_ImageOpened;
            content1.Content = img;

            //string[] urlAry = Url.Split('/');
            //string fileFullName = urlAry.Last();
            //try
            //{
            //    var f = await _folder.TryGetItemAsync(fileFullName);
            //    if (f != null)
            //    {
            //        _file = f as StorageFile;
            //    }
            //    else
            //    {
            //        // 不存在则请求
            //        using (var client = new HttpClient())
            //        {
            //            var response = await client.GetAsync(new Uri(Url));
            //            var buf = await response.Content.ReadAsBufferAsync();
            //            _file = await _folder.CreateFileAsync(fileFullName, CreationCollisionOption.ReplaceExisting);
            //            await FileIO.WriteBufferAsync(_file, buf);
            //        }
            //    }

            //    if (_folder != null && _file != null)
            //    {
            //        using (var fileStream = await _file.OpenAsync(FileAccessMode.Read))
            //        {
            //            if (fileStream != null)
            //            {
            //                var bitmapImg = new BitmapImage();
            //                await bitmapImg.SetSourceAsync(fileStream);
            //                int imgWidth = bitmapImg.PixelWidth;
            //                int imgHeight = bitmapImg.PixelHeight;

            //                if (bitmapImg.IsAnimatedBitmap)
            //                {

            //                }
            //                bitmapImg.AutoPlay = false;
            //                img.MaxWidth = imgWidth;
            //                img.Stretch = Stretch.UniformToFill;
            //                img.Source = bitmapImg;
            //                content1.Content = img;
            //            }
            //        }
            //    }
            //}
            //catch
            //{

            //}
        }

        private void Img_ImageOpened(object sender, RoutedEventArgs e)
        {
            var img = sender as Image;
            var bi = img.Source as BitmapImage;
            img.MaxWidth = bi.PixelWidth;
            img.Stretch = Stretch.UniformToFill;
            img.ImageOpened -= Img_ImageOpened;
        }
    }
}
