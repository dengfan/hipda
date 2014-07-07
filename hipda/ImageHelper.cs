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
    public class ImageHelper
    {
        private const int ImageMaxiamSize = 307200;

        public async static Task<byte[]> LoadAsync(StorageFile image)
        {
            Stream stream;

            var property = await image.GetBasicPropertiesAsync();
            if (property.Size > ImageMaxiamSize)
            {
                var ratio = (double)ImageMaxiamSize / property.Size;

                var imageProperty = await image.Properties.GetImagePropertiesAsync();
                var scaledWidth = Convert.ToUInt32(imageProperty.Width * ratio);
                var scaledHeight = Convert.ToUInt32(imageProperty.Height * ratio);

                using (var sourceStream = await image.OpenAsync(FileAccessMode.Read))
                {
                    var decoder = await BitmapDecoder.CreateAsync(sourceStream);
                    var transform = new BitmapTransform { ScaledHeight = scaledHeight, ScaledWidth = scaledWidth, InterpolationMode = BitmapInterpolationMode.Cubic };
                    var pixelData = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, transform, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);

                    using (var destinationStream = new InMemoryRandomAccessStream())
                    {
                        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, destinationStream);
                        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, scaledWidth, scaledHeight, 96, 96, pixelData.DetachPixelData());
                        await encoder.FlushAsync();

                        stream = destinationStream.CloneStream().AsStreamForRead();
                    }
                }
            }
            else
            {
                stream = (await image.OpenSequentialReadAsync()).AsStreamForRead();
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
