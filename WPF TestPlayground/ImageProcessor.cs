using System;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace Media_Controller_Remote_Host;

public static class ImageProcessor
{
    public static byte[]? GetThumbnailAsByteArray(IRandomAccessStreamReference thumbnail, uint upscaleModifier)
    {
        if (thumbnail == null)
            return null;

        var imageStream = thumbnail.OpenReadAsync().GetAwaiter().GetResult();
        var decoder = BitmapDecoder.CreateAsync(imageStream).GetAwaiter().GetResult();

        //Calculate new width and height while keeping the original aspect ration
        var originalWidth = decoder.PixelWidth;
        var originalHeight = decoder.PixelHeight;
        uint newWidth, newHeight;

        newWidth = (uint)((double)originalWidth * upscaleModifier);
        newHeight = (uint)((double)originalHeight * upscaleModifier);

        // Create a new BitmapTransformer and set the desired size
        var transformer = new BitmapTransform
        {
            ScaledWidth = newWidth,
            ScaledHeight = newHeight,
            InterpolationMode = BitmapInterpolationMode.Fant
        };

        // Apply the transform
        var resizedPixelData = decoder.GetPixelDataAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            transformer,
            ExifOrientationMode.RespectExifOrientation,
            ColorManagementMode.DoNotColorManage
        ).GetAwaiter().GetResult();

        // Create a new InMemoryRandomAccessStream to store the resized image
        using (var resizedImageStream = new InMemoryRandomAccessStream())
        {
            // Create a BitmapEncoder and set the resized pixel data
            var encoder = BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, resizedImageStream).GetAwaiter()
                .GetResult();
            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                newWidth,
                newHeight,
                decoder.DpiX,
                decoder.DpiY,
                resizedPixelData.DetachPixelData()
            );

            // Flush the encoder and return the resized image as a byte array
            encoder.FlushAsync().GetAwaiter().GetResult();

            var resizedImageBytes = new byte[resizedImageStream.Size];

            using (var reader = new DataReader(resizedImageStream))
            {
                reader.LoadAsync((uint)resizedImageStream.Size).GetAwaiter().GetResult();
                reader.ReadBytes(resizedImageBytes);
            }

            return resizedImageBytes;
        }
    }
}