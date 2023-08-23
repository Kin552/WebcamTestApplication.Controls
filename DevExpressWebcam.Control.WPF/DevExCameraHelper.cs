using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors.Internal;
using DevExpress.Xpf.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using DevExpress.Xpf.Core.Native;

namespace DevExpressWebcam.Control.WPF
{
    public interface ICapturePirctureService
    {
        byte[] GetImageByteArrayFromCamera(double height = 450, double width = 450, double minHeight = 450, double minWidth = 450);
    }

    public class CapturePictureService: ICapturePirctureService
    {
        public virtual byte[] GetImageByteArrayFromCamera(double height = 350, double width = 350, double minHeight = 350, double minWidth = 350)
        {
            return GetImageByteArrayFromCamera(null);
        }

        protected byte[] GetImageByteArrayFromCamera(Window owner, double height = 350, double width = 350, double minHeight = 350, double minWidth = 350)
        {
            return DevExCameraHelper.GetImageByteArrayFromCamera(owner, width, height, minHeight, minWidth);
        }
    }

    public static class DevExCameraHelper
    {
        private static int jpegQualityLevel = 90;

        /// <summary>
        /// Shows a dialog to capture an image from a web cam or image capturing device.
        /// </summary>
        /// <param name="owner">The window launching the camera control. Defaults to the main application window if null.</param>
        /// <param name="height">Height of the capture window. Defaults to 350.</param>
        /// <param name="width">Width of the capture window. Defaults to 350.</param>
        /// <param name="minHeight">Minimum height of the capture window. Defaults to 350.</param>
        /// <param name="minWidth">Minimum width of the capture window. Defaults to 350.</param>
        /// <returns>An array of type <see cref="System.Byte"/> representing the <see cref="System.Windows.Media.Imaging.BitmapImage"/> captured. Null if no capture or cancelled.</returns>
        public static byte[] GetImageByteArrayFromCamera(Window owner, double height = 450, double width = 450, double minHeight = 450, double minWidth = 450)
        {
            byte[] imageBytes = GetByteArrayFromImageSource(GetBitmapImageFromCamera(owner, width, height, minHeight, minWidth));

            return imageBytes;
        }

        /// <summary>
        /// Shows a dialog to capture an image from a web cam or image capturing device.
        /// </summary>
        /// <param name="owner">The window launching the camera control. Defaults to the main application window if null.</param>
        /// <param name="height">Height of the capture window. Defaults to 350.</param>
        /// <param name="width">Width of the capture window. Defaults to 350.</param>
        /// <param name="minHeight">Minimum height of the capture window. Defaults to 350.</param>
        /// <param name="minWidth">Minimum width of the capture window. Defaults to 350.</param>
        /// <returns>The <see cref="System.Windows.Media.Imaging.BitmapImage"/> captured. Null if no capture or cancelled.</returns>
        public static BitmapImage GetBitmapImageFromCamera(Window owner, double height = 450, double width = 450, double minHeight = 450, double minWidth = 450)
        {
            BitmapImage image = ShowCameraWindow(owner, width, height, minHeight, minWidth);

            return image;
        }

        private static BitmapImage ShowCameraWindow(Window owner, double height = 450, double width = 450, double minHeight = 450, double minWidth = 450)
        {
            BitmapImage image = null;

            DXWindow dlg = new DXWindow() { Width = width, Height = height, MinHeight = minHeight, MinWidth = minWidth, ShowIcon = false, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            var parentWindow = LayoutHelper.FindLayoutOrVisualParentObject<Window>(owner as DependencyObject, true);
            if (parentWindow == null)
                parentWindow = Application.Current.MainWindow;

            if (parentWindow != null)
                dlg.Owner = parentWindow;

            dlg.Title = EditorLocalizer.GetString(EditorStringId.CameraTakePictureCaption);
            TakePictureControl control = new TakePictureControl();
            var imageEdit = new ImageEdit();
            control.SetEditor(imageEdit, PopupBaseEdit.GetPopupOwnerEdit(imageEdit) as IImageEdit as PopupImageEdit);

            dlg.Content = control;
            dlg.Loaded += TakePictureControl_Loaded;
            dlg.ShowDialog();

            if (imageEdit.Source != null)
                image = imageEdit.Source as BitmapImage;

            var val = LayoutTreeHelper.GetVisualChildren(control).Where(c => c is CameraControl).FirstOrDefault() as CameraControl;

            return image;
        }

        private static void TakePictureControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var cameraName = "KLU - USB Camera";
                System.Drawing.Size cameraResolution = new System.Drawing.Size(width: 1280, height: 720);
                CameraControl cameraControl = LayoutTreeHelper.GetVisualChildren(sender as Window).Where(c => c is CameraControl).FirstOrDefault() as CameraControl;

                if (!string.IsNullOrEmpty(cameraName) && cameraControl != null)
                {
                    var device = cameraControl.GetAvailableDevices().Where(c => c.Name == cameraName).FirstOrDefault();
                    if (device != null)
                    {
                        cameraControl.DeviceInfo = device;
                        cameraControl.Device.Resolution = cameraResolution;
                        cameraControl.DeviceSettings.Brightness.Value = cameraControl.DeviceSettings.Brightness.Value;
                        cameraControl.DeviceSettings.Contrast.Value = cameraControl.DeviceSettings.Contrast.Value;

                        //DO NOT CHANGE OR SUFFER DEATH
                        cameraControl.DeviceSettings.Saturation.Value = cameraControl.DeviceSettings.Saturation.Default;
                    }
                }
            }
            catch (Exception error)
            {
                throw error;
            }
            finally
            {
                (sender as Window).Loaded -= TakePictureControl_Loaded;
            }
        }

        public static byte[] GetByteArrayFromImageSource(ImageSource imageSource)
        {
            if (imageSource is BitmapSource)
                return GetByteArrayFromBitmapSource((BitmapSource)imageSource);

            return null;
        }

        public static byte[] GetByteArrayFromBitmapSource(BitmapSource bitmapSource)
        {
            return GetByteArrayFromImage(GetBitmap(bitmapSource));
        }

        public static byte[] GetByteArrayFromImage(Image image)
        {
            Byte[] imageByte = null;

            if (image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    var format = GetImageFormat(image);
                    if (!format.Equals(ImageFormat.Jpeg))
                    {
                        var hasTransparency = HasTransparency(image as Bitmap);
                        if (hasTransparency)
                        {
                            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(GetImageSourceFromImage(image)));
                            encoder.Save(ms);
                            ms.Flush();

                            imageByte = ms.GetBuffer();
                        }
                    }

                    if (imageByte == null)
                    {
                        var encoder = new System.Windows.Media.Imaging.JpegBitmapEncoder();
                        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(GetImageSourceFromImage(image)));
                        encoder.QualityLevel = jpegQualityLevel;
                        encoder.Save(ms);
                        ms.Flush();

                        imageByte = ms.GetBuffer();
                    }
                }
            }

            return imageByte;
        }

        private static System.Drawing.Bitmap GetBitmap(BitmapSource source)
        {
            System.Drawing.Bitmap bmp = null;

            try
            {
                //convert image format
                var src = new System.Windows.Media.Imaging.FormatConvertedBitmap();
                src.BeginInit();
                src.Source = source;
                src.DestinationFormat = System.Windows.Media.PixelFormats.Bgra32;
                src.EndInit();

                bmp = new System.Drawing.Bitmap(
                    src.PixelWidth,
                    src.PixelHeight,
                    System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                BitmapData data = bmp.LockBits(
                    new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                    ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                src.CopyPixels(
                    System.Windows.Int32Rect.Empty,
                    data.Scan0,
                    data.Height * data.Stride,
                    data.Stride);
                bmp.UnlockBits(data);
            }
            catch
            {
            }

            return bmp;
        }

        public static BitmapImage GetImageSourceFromImage(Image image)
        {
            if (image != null)
            {
                // Don't close the stream
                MemoryStream ms = new MemoryStream();

                var imageFormat = GetImageFormat(image);
                bool hasTransparency = false;
                if (!imageFormat.Equals(ImageFormat.Jpeg))
                    hasTransparency = HasTransparency(new System.Drawing.Bitmap(image));

                if (hasTransparency)
                {
                    // When there is transparency we will convert to PNG.
                    // Other formats can have transparency
                    image.Save(ms, ImageFormat.Png);
                }
                else
                {
                    // Convert to JPEG with quality level of 90 (this is compression)
                    ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    EncoderParameters ep = new EncoderParameters();
                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
                    image.Save(ms, jpgEncoder, ep);
                }

                // Must reset to zero - works okay without doing this for PNG but JPEG fails
                // when converting to a BitmapImage
                ms.Position = 0;
                return GetImageSourceFromStream(ms);
            }

            return null;
        }

        public static BitmapImage GetImageSourceFromStream(Stream stream)
        {
            if (stream != null)
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                bitmapImage.Freeze();

                return bitmapImage;
            }

            return null;
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }

        public static Image GetImageFromByteArray(byte[] byteArray)
        {
            if (byteArray != null)
            {
                // Don't close the stream
                MemoryStream ms = new MemoryStream(byteArray);
                Image newImage = Image.FromStream(ms);
                return newImage;
            }

            return null;
        }

        public static BitmapImage GetBitmapImageSourceFromByteArray(byte[] byteArray)
        {
            if (byteArray != null)
            {
                // Don't close the stream
                MemoryStream ms = new MemoryStream(byteArray);
                BitmapImage bitmapImage = GetImageSourceFromStream(ms);
                return bitmapImage;
            }

            return null;
        }

        public static Boolean HasTransparency(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
                return false;

            try
            {
                // Not an alpha-capable color format. Note that GDI+ indexed images are alpha-capable on the palette.
                if (((ImageFlags)bitmap.Flags & ImageFlags.HasAlpha) == 0)
                    return false;

                // Indexed format, and no alpha colours in the image's palette: immediate pass.
                if ((bitmap.PixelFormat & System.Drawing.Imaging.PixelFormat.Indexed) != 0 && bitmap.Palette.Entries.All(c => c.A == 255))
                    return false;

                // Get the byte data 'as 32-bit ARGB'. This offers a converted version of the image data without modifying the original image.
                BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Int32 len = bitmap.Height * data.Stride;
                Byte[] bytes = new Byte[len];
                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, len);
                bitmap.UnlockBits(data);

                // Check the alpha bytes in the data. Since the data is little-endian, the actual byte order is [BB GG RR AA]
                for (Int32 i = 3; i < len; i += 4)
                {
                    if (bytes[i] != 255)
                        return true;
                }
            }
            catch
            {
            }

            return false;
        }

        public static ImageFormat GetImageFormat(System.Drawing.Image image)
        {
            ImageFormat result = ImageFormat.Jpeg;

            if (image.RawFormat.Equals(ImageFormat.Bmp))
            {
                result = ImageFormat.Bmp;
            }
            else if (image.RawFormat.Equals(ImageFormat.Emf))
            {
                result = ImageFormat.Emf;
            }
            else if (image.RawFormat.Equals(ImageFormat.Gif))
            {
                result = ImageFormat.Gif;
            }
            else if (image.RawFormat.Equals(ImageFormat.Icon))
            {
                result = ImageFormat.Icon;
            }
            else if (image.RawFormat.Equals(ImageFormat.Jpeg))
            {
                result = ImageFormat.Jpeg;
            }
            else if (image.RawFormat.Equals(ImageFormat.MemoryBmp))
            {
                result = ImageFormat.MemoryBmp;
            }
            else if (image.RawFormat.Equals(ImageFormat.Png))
            {
                result = ImageFormat.Png;
            }
            else if (image.RawFormat.Equals(ImageFormat.Tiff))
            {
                result = ImageFormat.Tiff;
            }
            else if (image.RawFormat.Equals(ImageFormat.Wmf))
            {
                result = ImageFormat.Wmf;
            }

            return result;
        }
    }
}
