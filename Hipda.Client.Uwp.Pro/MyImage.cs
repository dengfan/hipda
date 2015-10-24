using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Hipda.Client.Uwp.Pro
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
            DependencyProperty.Register("Url", typeof(string), typeof(MyImage), new PropertyMetadata(0));

        protected async override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            StorageFile file;
            using (var client = new HttpClient())
            {
                string[] tary = Url.Split('.');
                string fileExName = tary.Last();
                string fileFullName = string.Format("{0}.{1}", Guid.NewGuid().ToString(), fileExName);

                var response = await client.GetAsync(new Uri(Url));
                var buf = await response.Content.ReadAsBufferAsync();
                byte[] bytes = WindowsRuntimeBufferExtensions.ToArray(buf, 0, (int)buf.Length);

                file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileFullName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBytesAsync(file, bytes);
            }

            Image img = GetTemplateChild("img1") as Image;
            BitmapImage bitmapImg = new BitmapImage();
            IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
            await bitmapImg.SetSourceAsync(fileStream);
            int imgWidth = bitmapImg.PixelWidth;
            if (imgWidth > 1000)
            {
                img.Stretch = Stretch.Uniform;
                img.MaxWidth = 1000;
            }
            else if (imgWidth > 400)
            {
                img.Stretch = Stretch.Uniform;
                img.MaxWidth = 500;
            }
            else
            {
                img.Stretch = Stretch.None;
            }
            img.Source = bitmapImg;
            img.ImageFailed += (s, e) => {
                return;
            };

            img.Tapped += async (s, e) => {
                List<string> fileTypeFilter = new List<string>();
                fileTypeFilter.Add(".jpg");
                fileTypeFilter.Add(".png");
                fileTypeFilter.Add(".bmp");
                fileTypeFilter.Add(".gif");
                var queryOptions = new QueryOptions(CommonFileQuery.OrderByName, fileTypeFilter);
                var query = ApplicationData.Current.LocalFolder.CreateFileQueryWithOptions(queryOptions);
                var options = new LauncherOptions();
                options.NeighboringFilesQuery = query;
                await Launcher.LaunchFileAsync(file, options);
            };
        }
    }
}
