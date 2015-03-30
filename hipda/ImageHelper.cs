using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace hipda
{
    public static class ImageHelper
    {
        public async static Task<byte[]> LoadAsync(StorageFile image)
        {
            Stream stream = null;
            var property = await image.GetBasicPropertiesAsync();
            if (property.Size > 307200)
            {
                var data = await image.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView);
                stream = data.AsStreamForRead();
            }
            else
            {
                var data = await image.OpenSequentialReadAsync();
                stream = data.AsStreamForRead();
            }

            if (null == stream)
                return null;

            var byteArray = new byte[stream.Length];
            var numBytesToRead = (int)stream.Length;
            var numBytesRead = 0;
            while (numBytesToRead > 0)
            {
                // Read may return anything from 0 to numBytesToRead.
                var n = stream.Read(byteArray, numBytesRead, numBytesToRead);

                // Break when the end of the file is reached.
                if (n == 0)
                    break;

                numBytesRead += n;
                numBytesToRead -= n;
            }

            return byteArray;
        }
    }
}
