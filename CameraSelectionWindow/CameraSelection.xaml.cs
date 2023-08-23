using DevExpressWebcam.Control.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CameraSelectionWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CameraSelection : Window
    {
        public CameraSelection()
        {
            InitializeComponent();
        }

        private void webCamCapture_Click(object sender, RoutedEventArgs e)
        {

        }

        private void devExpressWebCam_Click(object sender, RoutedEventArgs e)
        {
            byte[] fileData = null;
            fileData = DevExCameraHelper.GetImageByteArrayFromCamera(null);

            if (fileData != null)
            {
                DevExpressCam expressCam = new DevExpressCam();
                var image = DevExCameraHelper.GetImageFromByteArray(fileData);

                expressCam.ImageSource = DevExCameraHelper.GetImageSourceFromImage(image);
                expressCam.Show();
            }
        }
    }
}
