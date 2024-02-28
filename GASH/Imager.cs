using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using GASH.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace GASH
{
    public static class Imager
    {
        public static Visual visual1 { get; set; }
        public async static Task<Bitmap?> ImagePicker(Visual visual)
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(visual);

            var dialog = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "КАРTINКА",
                AllowMultiple = false,
                FileTypeFilter = new FilePickerFileType[] { FilePickerFileTypes.ImageAll }
            });

            Bitmap? bitmap = null;

            if (dialog.Count >= 1)
            {
                await using Stream stream = await dialog[0].OpenReadAsync();

                try
                {
                    bitmap = new Bitmap(stream);

                    double width = bitmap.Size.Width;
                    double fwidth = width;

                    if (bitmap.Size.Width > 300)
                    {
                        bitmap = Bitmap.DecodeToWidth(stream, 300, BitmapInterpolationMode.MediumQuality);
                    }

                    if (bitmap.Size.Height > 200)
                    {
                        bitmap = Bitmap.DecodeToWidth(stream, 300, BitmapInterpolationMode.MediumQuality);
                    }

                    return bitmap;
                }
                catch
                {
                    OkMessageBoxWindow mb = new OkMessageBoxWindow("Не удалось открыть файл");
                    mb.Show();

                    return null;
                }
            }

            return bitmap;
        }

        public async static Task<(Bitmap?, string)> ImagePickerWithPath(Visual visual)
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(visual);

            var dialog = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "КАРTINКА",
                AllowMultiple = false,
                FileTypeFilter = new FilePickerFileType[] { FilePickerFileTypes.ImageAll }
            });

            Bitmap? bitmap = null;

            if (dialog.Count >= 1)
            {
                try
                {
                    await using (Stream stream = await dialog[0].OpenReadAsync())
                    {
                        bitmap = new Bitmap(stream);

                        if (bitmap.Size.Width > 300 || bitmap.Size.Height > 200)
                        {
                            OkMessageBoxWindow mb = new OkMessageBoxWindow("Слишком большой фото");
                            mb.Show();

                            return (null, null);
                        }

                        /*

                        if (bitmap.Size.Width > 300)
                        {
                            bitmap = Bitmap.DecodeToWidth(stream, 300, BitmapInterpolationMode.MediumQuality);
                        }

                        if (bitmap.Size.Height > 200)
                        {
                            bitmap = Bitmap.DecodeToWidth(stream, 300, BitmapInterpolationMode.MediumQuality);
                        }
                        */

                        return (bitmap, dialog[0].Path.LocalPath);
                    }
                }
                catch
                {
                    OkMessageBoxWindow mb = new OkMessageBoxWindow("Не удалось открыть файл");
                    mb.Show();

                    return (null, null);
                }

            }

            return (bitmap, null);
        }

        public static Bitmap ImageFromPath(string path)
        {
            try
            {
                Bitmap b = null;
                FileStream stream = File.OpenRead(path);
                b = new Bitmap(stream);
                
                return b;
            }
            catch
            {
               return new Bitmap(AssetLoader.Open(new Uri("avares://GASH/Assets/dstg.png")));
            }
        }
    }
}
