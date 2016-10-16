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

        bool _isCommon
        {
            get
            {
                return Url.Contains("hi-pda.com/forum/images/") || Url.Equals("http://www.hi-pda.com/forum/attachments/day_140621/1406211752793e731a4fec8f7b.png");
            }
        }

        protected async override void OnTapped(TappedRoutedEventArgs e)
        {
            base.OnTapped(e);

            try
            {
                if (!_isCommon)
                {
                    var folder = ApplicationData.Current.LocalCacheFolder;
                    var file = await SaveFile(folder);
                    if (file != null)
                    {
                        await OpenPhoto(file, folder);
                    }
                }
            }
            catch { }
        }

        async Task<IStorageFile> SaveFile(StorageFolder folder)
        {
            string[] urlAry = Url.Split('/');
            string fileFullName = urlAry.Last();
            var savedFile = await folder.TryGetItemAsync(fileFullName);
            if (savedFile != null)
            {
                return savedFile as StorageFile;
            }
            else
            {
                // 不存在则请求
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(new Uri(Url));
                    var buf = await response.Content.ReadAsBufferAsync();
                    var file = await folder.CreateFileAsync(fileFullName, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBufferAsync(file, buf);
                    return file;
                }
            }
        }

        private async Task OpenPhoto(IStorageFile file, StorageFolder folder)
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

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var cc1 = GetTemplateChild("cc1") as ContentControl;
            var myDependencyObject = (LocalSettingsDependencyObject)App.Current.Resources["MyLocalSettings"];
            Binding pictureOpacityBinding = new Binding { Source = myDependencyObject, Path = new PropertyPath("PictureOpacity") };

            var bi = new BitmapImage { UriSource = new Uri(Url) };
            var img = new Image();
            img.SetBinding(OpacityProperty, pictureOpacityBinding);
            img.Source = bi;
            img.ImageOpened += Img_ImageOpened;
            cc1.Content = img;
        }

        private void Img_ImageOpened(object sender, RoutedEventArgs e)
        {
            var img = sender as Image;
            var bi = img.Source as BitmapImage;
            bi.AutoPlay = false;
            img.MaxWidth = bi.PixelWidth;
            img.Stretch = Stretch.UniformToFill;
            img.ImageOpened -= Img_ImageOpened;
        }
    }
}
